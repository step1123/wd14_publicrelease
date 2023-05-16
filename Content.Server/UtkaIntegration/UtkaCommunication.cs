using System.Text.Json.Serialization;

namespace Content.Server.UtkaIntegration;

public class UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public virtual string? Command { get; set; }
}

public class UtkaHandshakeMessage : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string Command => "handshake";

    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class UtkaOOCRequest : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "ooc";

    [JsonPropertyName("ckey")]
    public string? CKey { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class UtkaAsayRequest : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "asay";

    [JsonPropertyName("a_ckey")]
    public string? ACkey { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class UtkaPmRequest : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "discord_pm";

    [JsonPropertyName("sender")]
    public string? Sender { get; set; }

    [JsonPropertyName("receiver")]
    public string? Reciever { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class UtkaPmResponse : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "discord_pm";

    [JsonPropertyName("message")]
    public bool? Message { get; set; }
}

public class UtkaWhoRequest : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "who";
}

public class UtkaWhoResponse : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "who";

    [JsonPropertyName("players")]
    public List<string>? Players { get; set; }
}

public class UtkaAdminWhoRequest : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "adminwho";
}

public class UtkaAdminWhoResponse : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "adminwho";

    [JsonPropertyName("admins")]
    public List<string>? Admins { get; set; }
}

public class UtkaStatusRequsets : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "status";
}

public class UtkaStatusResponse : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "status";

    [JsonPropertyName("players")]
    public int? Players { get; set; }

    [JsonPropertyName("admins")]
    public int? Admins { get; set; }

    [JsonPropertyName("map")]
    public string? Map { get; set; }

    [JsonPropertyName("round_duration")]
    public double RoundDuration { get; set; }

    [JsonPropertyName("shuttle_status")]
    public string? ShuttleStatus { get; set; }

    [JsonPropertyName("station_code")]
    public string? StationCode { get; set; }
}

public class UtkaRoundstatusUpdate : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "roundstatus";

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}


public sealed class UtkaChatEventMessage : UtkaBaseMessage
{
    [JsonPropertyName("ckey")]
    public string? Ckey { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public sealed class UtkaRoundStatusEvent : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "roundstatus";

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public sealed class UtkaChatMeEvent : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "me";

    [JsonPropertyName("ckey")]
    public string? Ckey { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("character_name")]
    public string? CharacterName { get; set; }
}

public sealed class UtkaAhelpPmEvent : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "pm";

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("ckey")]
    public string? Ckey { get; set; }

    [JsonPropertyName("sender")]
    public string? Sender { get; set; }

    [JsonPropertyName("rid")]
    public int? Rid { get; set; }

    [JsonPropertyName("no_admins")]
    public bool? NoAdmins { get; set; }

    [JsonPropertyName("entity")]
    public string? Entity { get; set; }
}

public sealed class UtkaBannedEvent : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "banned";

    [JsonPropertyName("ckey")]
    public string? Ckey { get; set; }

    [JsonPropertyName("a_ckey")]
    public string? ACkey { get; set; }

    [JsonPropertyName("bantype")]
    public string? Bantype { get; set; }

    [JsonPropertyName("duration")]
    public uint? Duration { get; set; }

    [JsonPropertyName("global")]
    public bool? Global { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("rid")]
    public int? Rid { get; set; }

    [JsonPropertyName("ban_id")]
    public int? BanId { get; set; }
}

public sealed class UtkaBanRequest : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "ban";

    [JsonPropertyName("ckey")]
    public string? Ckey { get; set; }

    [JsonPropertyName("a_ckey")]
    public string? ACkey { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("duration")]
    public uint? Duration { get; set; }

    [JsonPropertyName("global")]
    public bool? Global { get; set; }
}

public sealed class UtkaBanResponse : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "ban";

    [JsonPropertyName("banned")]
    public bool? Banned { get; set; }
}

public sealed class UtkaJobBanRequest : UtkaBaseMessage
{
    public override string? Command => "jobban";

    [JsonPropertyName("ckey")]
    public string? Ckey { get; set; }

    [JsonPropertyName("a_ckey")]
    public string? ACkey { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("duration")]
    public uint? Duration { get; set; }

    [JsonPropertyName("global")]
    public bool? Global { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public sealed class UtkaJobBanResponse : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "jobban";

    [JsonPropertyName("banned")]
    public bool? Banned { get; set; }
}

public sealed class UtkaRestartRoundRequest : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "restart_round";
}

public sealed class UtkaRestartRoundResponse : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "restart_round";

    [JsonPropertyName("restarted")]
    public bool? Restarted { get; set; }
}

public sealed class UtkaUnbanRequest : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "unban";

    [JsonPropertyName("a_ckey")]
    public string? ACkey { get; set; }

    [JsonPropertyName("bid")]
    public int? Bid { get; set; }
}

public sealed class UtkaUnbanResponse : UtkaBaseMessage
{
    [JsonPropertyName("command")]
    public override string? Command => "unban";

    [JsonPropertyName("unbanned")]
    public bool? Unbanned { get; set; }
}
