using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Administration.Systems;
using Content.Server.Mind.Components;
using Content.Server.MoMMI;
using Content.Server.Players;
using Content.Server.Preferences.Managers;
using Content.Server.Station.Systems;
using Content.Server.UtkaIntegration;
using Content.Server.White.Sponsors;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.White;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Replays;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Chat.Managers
{
    /// <summary>
    ///     Dispatches chat messages to clients.
    /// </summary>
    internal sealed class ChatManager : IChatManager
    {
        private static readonly Dictionary<string, string> PatronOocColors = new()
        {
            // I had plans for multiple colors and those went nowhere so...
            { "nuclear_operative", "#aa00ff" },
            { "syndicate_agent", "#aa00ff" },
            { "revolutionary", "#aa00ff" }
        };

        [Dependency] private readonly IReplayRecordingManager _replay = default!;
        [Dependency] private readonly IServerNetManager _netManager = default!;
        [Dependency] private readonly IMoMMILink _mommiLink = default!;
        [Dependency] private readonly IAdminManager _adminManager = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly IServerPreferencesManager _preferencesManager = default!;
        [Dependency] private readonly IConfigurationManager _configurationManager = default!;
        [Dependency] private readonly INetConfigurationManager _netConfigManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        /// WD-EDIT
        [Dependency] private readonly SponsorsManager _sponsorsManager = default!;
        [Dependency] private readonly UtkaTCPWrapper _utkaSocketWrapper = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        /// WD-EDIT

        /// <summary>
        /// The maximum length a player-sent message can be sent
        /// </summary>
        public int MaxMessageLength => _configurationManager.GetCVar(CCVars.ChatMaxMessageLength);

        private bool _oocEnabled = true;
        private bool _adminOocEnabled = true;

        private readonly Dictionary<NetUserId, (string, TimeSpan)> _lastMessages = new();
        private bool _antispam;
        private int _antispamMinLength;
        private double _antispamIntervalSeconds;

        public void Initialize()
        {
            _netManager.RegisterNetMessage<MsgChatMessage>();

            _configurationManager.OnValueChanged(CCVars.OocEnabled, OnOocEnabledChanged, true);
            _configurationManager.OnValueChanged(CCVars.AdminOocEnabled, OnAdminOocEnabledChanged, true);
            // WD START
            _configurationManager.OnValueChanged(WhiteCVars.ChatAntispam, val => _antispam = val, true);
            _configurationManager.OnValueChanged(WhiteCVars.AntispamMinLength, val => _antispamMinLength = val, true);
            _configurationManager.OnValueChanged(WhiteCVars.AntispamIntervalSeconds,
                val => _antispamIntervalSeconds = val, true);
            // WD END
        }

        private void OnOocEnabledChanged(bool val)
        {
            if (_oocEnabled == val) return;

            _oocEnabled = val;
            DispatchServerAnnouncement(Loc.GetString(val ? "chat-manager-ooc-chat-enabled-message" : "chat-manager-ooc-chat-disabled-message"));
        }

        private void OnAdminOocEnabledChanged(bool val)
        {
            if (_adminOocEnabled == val) return;

            _adminOocEnabled = val;
            DispatchServerAnnouncement(Loc.GetString(val ? "chat-manager-admin-ooc-chat-enabled-message" : "chat-manager-admin-ooc-chat-disabled-message"));
        }

        #region Server Announcements

        public void DispatchServerAnnouncement(string message, Color? colorOverride = null)
        {
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", FormattedMessage.EscapeText(message)));
            ChatMessageToAll(ChatChannel.Server, message, wrappedMessage, EntityUid.Invalid, hideChat: false, recordReplay: true, colorOverride: colorOverride);
            Logger.InfoS("SERVER", message);

            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Server announcement: {message}");
        }

        public void DispatchServerMessage(IPlayerSession player, string message, bool suppressLog = false)
        {
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", FormattedMessage.EscapeText(message)));
            ChatMessageToOne(ChatChannel.Server, message, wrappedMessage, default, false, player.ConnectedClient);

            if (!suppressLog)
                _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Server message to {player:Player}: {message}");
        }

        public void SendAdminAnnouncement(string message)
        {
            var clients = _adminManager.ActiveAdmins.Select(p => p.ConnectedClient);

            var wrappedMessage = Loc.GetString("chat-manager-send-admin-announcement-wrap-message",
                ("adminChannelName", Loc.GetString("chat-manager-admin-channel-name")), ("message", FormattedMessage.EscapeText(message)));

            ChatMessageToMany(ChatChannel.Admin, message, wrappedMessage, default, false, true, clients);
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Admin announcement: {message}");
        }

        public void SendAdminAlert(string message)
        {
            var clients = _adminManager.ActiveAdmins.Select(p => p.ConnectedClient);

            var wrappedMessage = Loc.GetString("chat-manager-send-admin-announcement-wrap-message",
                ("adminChannelName", Loc.GetString("chat-manager-admin-channel-name")), ("message", FormattedMessage.EscapeText(message)));

            ChatMessageToMany(ChatChannel.AdminAlert, message, wrappedMessage, default, false, true, clients);
        }

        public void SendAdminAlert(EntityUid player, string message, MindContainerComponent? mindContainerComponent = null)
        {
            if ((mindContainerComponent == null && !_entityManager.TryGetComponent(player, out mindContainerComponent)) || !mindContainerComponent.HasMind)
            {
                SendAdminAlert(message);
                return;
            }

            var adminSystem = _entityManager.System<AdminSystem>();
            var antag = mindContainerComponent.Mind!.UserId != null
                        && (adminSystem.GetCachedPlayerInfo(mindContainerComponent.Mind!.UserId.Value)?.Antag ?? false);

            SendAdminAlert($"{mindContainerComponent.Mind!.Session?.Name}{(antag ? " (ANTAG)" : "")} {message}");
        }

        public void SendHookOOC(string sender, string message)
        {
            if (_configurationManager.GetCVar(CCVars.DisableHookedOOC))
            {
                return;
            }
            var wrappedMessage = Loc.GetString("chat-manager-send-hook-ooc-wrap-message", ("senderName", sender), ("message", FormattedMessage.EscapeText(message)));
            ChatMessageToAll(ChatChannel.OOC, message, wrappedMessage, source: EntityUid.Invalid, hideChat: false, recordReplay: true);
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Hook OOC from {sender}: {message}");
        }

        //WD-EDIT
        public void SendHookAdminChat(string sender, string message)
        {
            var admins = _adminManager.ActiveAdmins;

            var wrappedMessage = Loc.GetString("chat-manager-send-admin-chat-wrap-message",
                ("adminChannelName", Loc.GetString("chat-manager-admin-discord-channel-name")),
                ("playerName", sender), ("message", FormattedMessage.EscapeText(message)));

            ChatMessageToMany(ChatChannel.Admin, message, wrappedMessage, EntityUid.Invalid, false, false, admins.Select(p => p.ConnectedClient));

            var asayEventMessage = new UtkaChatEventMessage()
            {
                Command = "asay",
                Ckey = sender,
                Message = message
            };

            _utkaSocketWrapper.SendMessageToAll(asayEventMessage);
        }

        public bool TrySendNewMessage(IPlayerSession session, string newMessage, bool checkLength = false)
        {
            if (!_antispam || checkLength && newMessage.Length < _antispamMinLength)
            {
                _lastMessages.Remove(session.Data.UserId);
                return true;
            }

            var curTime = _timing.CurTime;
            if (_lastMessages.TryGetValue(session.Data.UserId, out var value))
            {
                var interval = (curTime - value.Item2).TotalSeconds;
                var difference = _antispamIntervalSeconds - interval;
                if (value.Item1 == newMessage && difference > 0d)
                {
                    DispatchServerMessage(session,
                        Loc.GetString("chat-manager-antispam-warn-message", ("remainingTime", (int) difference)));
                    return false;
                }
            }
            _lastMessages[session.Data.UserId] = (newMessage, curTime);

            return true;
        }
        //WD-EDIT

        #endregion

        #region Public OOC Chat API

        /// <summary>
        ///     Called for a player to attempt sending an OOC, out-of-game. message.
        /// </summary>
        /// <param name="player">The player sending the message.</param>
        /// <param name="message">The message.</param>
        /// <param name="type">The type of message.</param>
        public void TrySendOOCMessage(IPlayerSession player, string message, OOCChatType type)
        {
            // Check if message exceeds the character limit
            if (message.Length > MaxMessageLength)
            {
                DispatchServerMessage(player, Loc.GetString("chat-manager-max-message-length-exceeded-message", ("limit", MaxMessageLength)));
                return;
            }

            switch (type)
            {
                case OOCChatType.OOC:
                    SendOOC(player, message);
                    break;
                case OOCChatType.Admin:
                    SendAdminChat(player, message);
                    break;
            }
        }

        #endregion

        #region Private API

        private void SendOOC(IPlayerSession player, string message)
        {
            if (_adminManager.IsAdmin(player))
            {
                if (!_adminOocEnabled)
                {
                    return;
                }
            }
            else if (!_oocEnabled)
            {
                return;
            }

            if (!TrySendNewMessage(player, message)) // WD
                return;

            Color? colorOverride = null;
            var wrappedMessage = Loc.GetString("chat-manager-send-ooc-wrap-message", ("playerName",player.Name), ("message", FormattedMessage.EscapeText(message)));
            if (_adminManager.HasAdminFlag(player, AdminFlags.Admin))
            {
                var prefs = _preferencesManager.GetPreferences(player.UserId);
                colorOverride = prefs.AdminOOCColor;
            }
            if (player.ConnectedClient.UserData.PatronTier is { } patron &&
                     PatronOocColors.TryGetValue(patron, out var patronColor))
            {
                wrappedMessage = Loc.GetString("chat-manager-send-ooc-patron-wrap-message", ("patronColor", patronColor),("playerName", player.Name), ("message", FormattedMessage.EscapeText(message)));
            }

            //WD-EDIT
            if (_sponsorsManager.TryGetInfo(player.UserId, out var sponsorData) && sponsorData.OOCColor != null)
            {
                wrappedMessage = Loc.GetString("chat-manager-send-ooc-patron-wrap-message", ("patronColor", sponsorData.OOCColor),("playerName", player.Name), ("message", FormattedMessage.EscapeText(message)));
            }
            //WD-EDIT

            //TODO: player.Name color, this will need to change the structure of the MsgChatMessage
            ChatMessageToAll(ChatChannel.OOC, message, wrappedMessage, EntityUid.Invalid, hideChat: false, recordReplay: true, colorOverride);
            _mommiLink.SendOOCMessage(player.Name, message);
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"OOC from {player:Player}: {message}");

            //WD-EDIT
            var toUtkaMessage = new UtkaChatEventMessage()
            {
                Command = "ooc",
                Ckey = player.Name,
                Message = message,
            };

            _utkaSocketWrapper.SendMessageToAll(toUtkaMessage);
            //WD-EDIT
        }

        private void SendAdminChat(IPlayerSession player, string message)
        {
            if (!_adminManager.HasAdminFlag(player, AdminFlags.Admin))
            {
                _adminLogger.Add(LogType.Chat, LogImpact.Extreme, $"{player:Player} attempted to send admin message but was not admin");
                return;
            }

            var clients = _adminManager.ActiveAdmins
                .Where(p => _adminManager.HasAdminFlag(p, AdminFlags.Admin))
                .Select(p => p.ConnectedClient);
            var wrappedMessage = Loc.GetString("chat-manager-send-admin-chat-wrap-message",
                                            ("adminChannelName", Loc.GetString("chat-manager-admin-channel-name")),
                                            ("playerName", player.Name), ("message", FormattedMessage.EscapeText(message)));
            foreach (var client in clients)
            {
                var isSource = client != player.ConnectedClient;
                ChatMessageToOne(ChatChannel.AdminChat,
                    message,
                    wrappedMessage,
                    default,
                    false,
                    client,
                    audioPath: isSource ? _netConfigManager.GetClientCVar(client, CCVars.AdminChatSoundPath) : default,
                    audioVolume: isSource ? _netConfigManager.GetClientCVar(client, CCVars.AdminChatSoundVolume) : default);
            }

            _adminLogger.Add(LogType.Chat, $"Admin chat from {player:Player}: {message}");

            //WD-EDIT
            var asayEventMessage = new UtkaChatEventMessage()
            {
                Command = "asay",
                Ckey = player.Name,
                Message = message
            };

            _utkaSocketWrapper.SendMessageToAll(asayEventMessage);
            //WD-EDIT
        }

        #endregion

        #region Utility

        public void ChatMessageToOne(ChatChannel channel, string message, string wrappedMessage, EntityUid source, bool hideChat, INetChannel client, Color? colorOverride = null, bool recordReplay = false, string? audioPath = null, float audioVolume = 0)
        {
            var msg = new ChatMessage(channel, message, wrappedMessage, source, hideChat, colorOverride, audioPath, audioVolume);
            _netManager.ServerSendMessage(new MsgChatMessage() { Message = msg }, client);

            if (!recordReplay)
                return;

            if ((channel & ChatChannel.AdminRelated) == 0 ||
                _configurationManager.GetCVar(CCVars.ReplayRecordAdminChat))
            {
                _replay.RecordServerMessage(msg);
            }
        }

        public void ChatMessageToMany(ChatChannel channel, string message, string wrappedMessage, EntityUid source, bool hideChat, bool recordReplay, IEnumerable<INetChannel> clients, Color? colorOverride = null, string? audioPath = null, float audioVolume = 0)
            => ChatMessageToMany(channel, message, wrappedMessage, source, hideChat, recordReplay, clients.ToList(), colorOverride, audioPath, audioVolume);

        public void ChatMessageToMany(ChatChannel channel, string message, string wrappedMessage, EntityUid source, bool hideChat, bool recordReplay, List<INetChannel> clients, Color? colorOverride = null, string? audioPath = null, float audioVolume = 0)
        {
            var msg = new ChatMessage(channel, message, wrappedMessage, source, hideChat, colorOverride, audioPath, audioVolume);
            _netManager.ServerSendToMany(new MsgChatMessage() { Message = msg }, clients);

            if (!recordReplay)
                return;

            if ((channel & ChatChannel.AdminRelated) == 0 ||
                _configurationManager.GetCVar(CCVars.ReplayRecordAdminChat))
            {
                _replay.RecordServerMessage(msg);
            }
        }

        public void ChatMessageToManyFiltered(Filter filter, ChatChannel channel, string message, string wrappedMessage, EntityUid source,
            bool hideChat, bool recordReplay, Color? colorOverride = null, string? audioPath = null, float audioVolume = 0)
        {
            if (!recordReplay && !filter.Recipients.Any())
                return;

            var clients = new List<INetChannel>();
            foreach (var recipient in filter.Recipients)
            {
                clients.Add(recipient.ConnectedClient);
            }

            ChatMessageToMany(channel, message, wrappedMessage, source, hideChat, recordReplay, clients, colorOverride, audioPath, audioVolume);
        }

        public void ChatMessageToAll(ChatChannel channel, string message, string wrappedMessage, EntityUid source, bool hideChat, bool recordReplay, Color? colorOverride = null,  string? audioPath = null, float audioVolume = 0)
        {
            var msg = new ChatMessage(channel, message, wrappedMessage, source, hideChat, colorOverride, audioPath, audioVolume);
            _netManager.ServerSendToAll(new MsgChatMessage() { Message = msg });

            if (!recordReplay)
                return;

            if ((channel & ChatChannel.AdminRelated) == 0 ||
                _configurationManager.GetCVar(CCVars.ReplayRecordAdminChat))
            {
                _replay.RecordServerMessage(msg);
            }
        }

        public bool MessageCharacterLimit(IPlayerSession? player, string message)
        {
            var isOverLength = false;

            // Non-players don't need to be checked.
            if (player == null)
                return false;

            // Check if message exceeds the character limit if the sender is a player
            if (message.Length > MaxMessageLength)
            {
                var feedback = Loc.GetString("chat-manager-max-message-length-exceeded-message", ("limit", MaxMessageLength));

                DispatchServerMessage(player, feedback);

                isOverLength = true;
            }

            return isOverLength;
        }

        #endregion
    }

    public enum OOCChatType : byte
    {
        OOC,
        Admin
    }
}
