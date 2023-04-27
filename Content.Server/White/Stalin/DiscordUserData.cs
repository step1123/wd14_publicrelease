using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Content.Server.White.Stalin;

public sealed class DiscordUserData
{
    [JsonPropertyName("registered")]
    public bool Registered { get; set; }

    [JsonPropertyName("created_at")]
    public double UnixTimestamp { get; set; }

    public DateTime DiscordAge => UnixTimeStampToDateTime(UnixTimestamp);

    public static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }
}

public sealed class DiscordUsersDataRequest
{
    [JsonPropertyName("uuids")]
    public List<string> Uids { get; set; } = new();
}

public sealed class DiscordUsersData
{
    public Dictionary<string, DiscordUserData> Users { get; set; } = new();
}

