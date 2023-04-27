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
