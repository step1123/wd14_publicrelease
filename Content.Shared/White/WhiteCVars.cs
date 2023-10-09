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
 * Wiki rules
    */

    public static readonly CVarDef<string> RulesWiki =
        CVarDef.Create("white.wiki_rules", "https://wiki.ss14.su/%D0%9F%D1%80%D0%B0%D0%B2%D0%B8%D0%BB%D0%B0",
            CVar.SERVER | CVar.REPLICATED);

    /*
 * Slang
    */

    public static readonly CVarDef<bool> ChatSlangFilter =
        CVarDef.Create("ic.slang_filter", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /*
 * Antispam
    */

    public static readonly CVarDef<bool> ChatAntispam =
        CVarDef.Create("ic.antispam", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    public static readonly CVarDef<int> AntispamMinLength =
        CVarDef.Create("ic.antispam_min_length", 7, CVar.SERVERONLY);

    public static readonly CVarDef<double> AntispamIntervalSeconds =
        CVarDef.Create("ic.antispam_interval_seconds", 60d, CVar.SERVERONLY);

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
    public static readonly CVarDef<bool> TtsEnabled =
        CVarDef.Create("tts.enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// URL of the TTS server API.
    /// </summary>
    public static readonly CVarDef<string> TtsApiUrl =
        CVarDef.Create("tts.api_url", "", CVar.SERVERONLY);

    /// <summary>
    /// TTS Volume
    /// </summary>
    public static readonly CVarDef<float> TtsVolume =
        CVarDef.Create("tts.volume", 0f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// TTS Cache
    /// </summary>
    public static readonly CVarDef<int> TtsMaxCacheSize =
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

    public static readonly CVarDef<float> MaxJukeboxSongSizeInMb = CVarDef.Create("white.max_jukebox_song_size",
        3.5f, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    public static readonly CVarDef<float> JukeboxVolume =
        CVarDef.Create("white.jukebox_volume", 0f, CVar.CLIENTONLY | CVar.ARCHIVE);


     /*
    * Chat
      */

    public static readonly CVarDef<string> SeparatedChatSize =
        CVarDef.Create("white.chat_size_separated", "0.6;0", CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<string> DefaultChatSize =
        CVarDef.Create("white.chat_size_default", "300;500", CVar.CLIENTONLY | CVar.ARCHIVE);

    /*
    * Chat GPT
     */

    public static readonly CVarDef<string> ChatGptApi =
        CVarDef.Create("white.gpt_api_link", "", CVar.SERVERONLY | CVar.ARCHIVE | CVar.CONFIDENTIAL);


    /*
    * OnlyInOhio
     */

    public static readonly CVarDef<string> OnlyInOhio =
        CVarDef.Create("white.ohio_api_link", "", CVar.SERVERONLY | CVar.ARCHIVE | CVar.CONFIDENTIAL);

    /*
     * Mark dead chat messages as admin
     */
    public static readonly CVarDef<bool> DeadChatAdmin =
        CVarDef.Create("white.admin.deadChatAdmin", false, CVar.CLIENT | CVar.REPLICATED | CVar.ARCHIVE);

    //Поставишь ниже 10 умрешь, факт.
    public static readonly CVarDef<int> CultMinPlayers =
        CVarDef.Create("white.cult_min_players", 20, CVar.SERVERONLY | CVar.ARCHIVE);

    public static readonly CVarDef<int> CultMaxStartingPlayers =
        CVarDef.Create("white.cult_max_starting_players", 4, CVar.SERVERONLY | CVar.ARCHIVE);

    public static readonly CVarDef<int> CultMinStartingPlayers =
        CVarDef.Create("white.cult_min_starting_players", 2, CVar.SERVERONLY | CVar.ARCHIVE);


        /*
     * End of round stats
        */

    /// <summary>
    ///     The amount of blood lost required to trigger the BloodLost end of round stat.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the BloodLost end of round stat.
    /// </remarks>
    public static readonly CVarDef<float> BloodLostThreshold =
        CVarDef.Create("eorstats.bloodlost_threshold", 100f, CVar.SERVERONLY);

    /// <summary>
    ///     The amount of time required to trigger the CuffedTime end of round stat, in minutes.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the CuffedTime end of round stat.
    /// </remarks>
    public static readonly CVarDef<int> CuffedTimeThreshold =
        CVarDef.Create("eorstats.cuffedtime_threshold", 5, CVar.SERVERONLY);

    /// <summary>
    ///     The amount of sounds required to trigger the EmitSound end of round stat.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the EmitSound end of round stat.
    /// </remarks>
    public static readonly CVarDef<int> EmitSoundThreshold =
        CVarDef.Create("eorstats.emitsound_threshold", 10, CVar.SERVERONLY);

    /// <summary>
    ///     The amount of instruments required to trigger the InstrumentPlayed end of round stat, in minutes.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the InstrumentPlayed end of round stat.
    /// </remarks>
    public static readonly CVarDef<int> InstrumentPlayedThreshold =
        CVarDef.Create("eorstats.instrumentplayed_threshold", 4, CVar.SERVERONLY);

    /// <summary>
    ///     The amount of shots fired required to trigger the ShotsFired end of round stat.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the ShotsFired end of round stat.
    /// </remarks>
    public static readonly CVarDef<int> ShotsFiredThreshold =
        CVarDef.Create("eorstats.shotsfired_threshold", 40, CVar.SERVERONLY);

    /// <summary>
    ///     Should a stat be displayed specifically when no shots were fired?
    /// </summary>
    public static readonly CVarDef<bool> ShotsFiredDisplayNone =
        CVarDef.Create("eorstats.shotsfired_displaynone", true, CVar.SERVERONLY);

    /// <summary>
    ///     The amount of times slipped required to trigger the SlippedCount end of round stat.
    /// </summary>
    /// <remarks>
    ///     Setting this to 0 will disable the SlippedCount end of round stat.
    /// </remarks>
    public static readonly CVarDef<int> SlippedCountThreshold =
        CVarDef.Create("eorstats.slippedcount_threshold", 30, CVar.SERVERONLY);

    /// <summary>
    ///     Should a stat be displayed specifically when nobody was done?
    /// </summary>
    public static readonly CVarDef<bool> SlippedCountDisplayNone =
        CVarDef.Create("eorstats.slippedcount_displaynone", true, CVar.SERVERONLY);

    /// <summary>
    ///     Should the top slipper be displayed in the end of round stats?
    /// </summary>
    public static readonly CVarDef<bool> SlippedCountTopSlipper =
        CVarDef.Create("eorstats.slippedcount_topslipper", true, CVar.SERVERONLY);

    public static readonly CVarDef<string>
        ServerCulture = CVarDef.Create("white.culture", "ru-RU", CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Should load a ERT map?
    /// </summary>
    public static readonly CVarDef<bool> LoadErtMap = CVarDef.Create("white.ert_load", true, CVar.SERVERONLY);

    public static readonly CVarDef<bool> LogChatActions =
        CVarDef.Create("white.log_to_chat", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);

    /*
     * Aspects
     */

    public static readonly CVarDef<bool> IsAspectsEnabled =
        CVarDef.Create("aspects.enabled", true, CVar.SERVERONLY);

    public static readonly CVarDef<double> AspectChance =
        CVarDef.Create("aspects.chance", 0.33, CVar.SERVERONLY);

    /*
     * Damage
     */

    // Applies Projectile and Melee damage.
    public static readonly CVarDef<float> DamageModifier  =
        CVarDef.Create("damage.modifier", 1.0f, CVar.REPLICATED);

    // Applies ALL damage, EVEN walls and etc.
    public static readonly CVarDef<float> DamageGetModifier  =
        CVarDef.Create("damage.get_modifier", 1.0f, CVar.REPLICATED);

    public static readonly CVarDef<bool> AutoKickVpnUsers =
        CVarDef.Create("white.auto_kick_vpn_users", false, CVar.SERVERONLY);

    public static readonly CVarDef<string> SalusApiLink = CVarDef.Create("white.salus_api_link", "http://localhost:7100/vpnchecker?address=", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /*
     * Reputation
     */
    public static readonly CVarDef<bool> ReputationEnabled =
        CVarDef.Create("white.reputation_enabled", true, CVar.SERVERONLY);
}
