using Robust.Shared.Configuration;

namespace Content.Shared.White;

/*
 * PUT YOUR CUSTOM VARS HERE
 * DO IT OR I WILL KILL YOU
 * with love, by hailrakes
 */


[CVarDefs]
public sealed class WhiteCVars
{
    /*
 * Slang
    */

    public static readonly CVarDef<bool> ChatSlangFilter =
        CVarDef.Create("ic.slang_filter", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /*
 * Sponsors
    */

    public static readonly CVarDef<string> SponsorsApiUrl =
        CVarDef.Create("sponsor.api_url", "", CVar.SERVERONLY);

    /*
 * Queue
    */

    public static readonly CVarDef<bool>
        QueueEnabled = CVarDef.Create("queue.enabled", false, CVar.SERVERONLY);


    /*
 * RoundNotifications
    */

    /// <summary>
    ///     URL of the Discord webhook which will send round status notifications.
    /// </summary>
    public static readonly CVarDef<string> DiscordRoundWebhook =
        CVarDef.Create("discord.round_webhook", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     Discord ID of role which will be pinged on new round start message.
    /// </summary>
    public static readonly CVarDef<string> DiscordRoundRoleId =
        CVarDef.Create("discord.round_roleid", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     Send notifications only about a new round begins.
    /// </summary>
    public static readonly CVarDef<bool> DiscordRoundStartOnly =
        CVarDef.Create("discord.round_start_only", false, CVar.SERVERONLY);

    /*
  * Sockets
     */

    public static readonly CVarDef<string> UtkaSocketKey = CVarDef.Create("utka.socket_key", "ass", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /**
     * TTS (Text-To-Speech)
        */

    /// <summary>
    /// URL of the TTS server API.
    /// </summary>
    public static readonly CVarDef<bool> TTSEnabled =
        CVarDef.Create("tts.enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// URL of the TTS server API.
    /// </summary>
    public static readonly CVarDef<string> TTSApiUrl =
        CVarDef.Create("tts.api_url", "", CVar.SERVERONLY);

    /// <summary>
    /// TTS Volume
    /// </summary>
    public static readonly CVarDef<float> TtsVolume =
        CVarDef.Create("tts.volume", 0f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// TTS Cache
    /// </summary>
    public static readonly CVarDef<int> TTSMaxCacheSize =
        CVarDef.Create("tts.max_cash_size", 200, CVar.SERVERONLY | CVar.ARCHIVE);



    /*
 * Stalin
     */

    public static readonly CVarDef<string> StalinSalt =
        CVarDef.Create("stalin.salt", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL | CVar.ARCHIVE);
    public static readonly CVarDef<string> StalinApiUrl =
        CVarDef.Create("stalin.api_url", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL | CVar.ARCHIVE);
    public static readonly CVarDef<string> StalinAuthUrl =
        CVarDef.Create("stalin.auth_url", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL | CVar.ARCHIVE);
    public static readonly CVarDef<bool> StalinEnabled =
        CVarDef.Create("stalin.enabled", false, CVar.SERVERONLY | CVar.ARCHIVE);
    public static readonly CVarDef<float> StalinDiscordMinimumAge =
        CVarDef.Create("stalin.minimal_discord_age_minutes", 30.0f, CVar.SERVERONLY | CVar.ARCHIVE);


    /*
   * NonPeaceful Round End
     */

    public static readonly CVarDef<bool> NonPeacefulRoundEndEnabled =
        CVarDef.Create("white.non_peaceful_round_end_enabled", true, CVar.SERVERONLY | CVar.ARCHIVE);


    /*
  * Disabling calling shuttle by admin button
     */

    public static readonly CVarDef<bool> EmergencyShuttleCallEnabled =
        CVarDef.Create("shuttle.emergency_shuttle_call", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);


    /*
   * Xenophobia
     */

    public static readonly CVarDef<bool> FanaticXenophobiaEnabled =
        CVarDef.Create("white.fanatic_xenophobia", true, CVar.SERVERONLY | CVar.ARCHIVE);

    /*
   * MeatyOre
     */

    public static readonly CVarDef<bool> MeatyOrePanelEnabled =
        CVarDef.Create("white.meatyore_panel_enabled", true, CVar.REPLICATED | CVar.SERVER | CVar.ARCHIVE);

    public static readonly CVarDef<int> MeatyOreDefaultBalance =
        CVarDef.Create("white.meatyore_default_balance", 15, CVar.SERVER | CVar.ARCHIVE);

    /*
   * Ghost Respawn
     */

    public static readonly CVarDef<float> GhostRespawnTime =
        CVarDef.Create("ghost.respawn_time", 15f, CVar.SERVERONLY);

    public static readonly CVarDef<int> GhostRespawnMaxPlayers =
        CVarDef.Create("ghost.respawn_max_players", 40, CVar.SERVERONLY);

    /*
    * Bwoink
     */

    public static readonly CVarDef<float> BwoinkVolume =
        CVarDef.Create("white.admin.bwoinkVolume", 0f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /*
    * Jukebox
     */

    public static readonly CVarDef<float> MaxJukeboxSongSizeInMB = CVarDef.Create("white.max_jukebox_song_size",
        3.5f, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    public static readonly CVarDef<float> MaxJukeboxSoundRange = CVarDef.Create("white.max_jukebox_sound_range", 20f,
        CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    public static readonly CVarDef<float> JukeboxVolume =
        CVarDef.Create("white.jukebox_volume", 0f, CVar.CLIENTONLY | CVar.ARCHIVE);


     /*
    * Chat
      */

    public static readonly CVarDef<string> SeparatedChatSize =
        CVarDef.Create("white.chat_size_separated", "0.6;0", CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<string> DefaultChatSize =
        CVarDef.Create("white.chat_size_default", "300;500", CVar.CLIENTONLY | CVar.ARCHIVE);
}
