using System.Globalization;
using System.Numerics;

namespace Content.Shared.White.Utils;

public static class Vector2Utils
{
    public static Vector2 ParseVector2FromString(string data, char separator)
    {
        var dataSplit = data.Split(separator);

        if (dataSplit.Length != 2)
        {
            Logger.Warning("Error at parsing string for vector, returning 0;0");
            return new Vector2(0, 0);
        }

        var hasX = float.TryParse(dataSplit[0], CultureInfo.InvariantCulture, out var x);
        var hasY = float.TryParse(dataSplit[1], CultureInfo.InvariantCulture, out var y);

        if (!hasX || !hasY)
        {
            Logger.Warning("Error at parsing string for vector, returning 0;0");
            return new Vector2(0, 0);
        }

        return new Vector2(x, y);
    }

    public static string ConvertToString(this Vector2 vector, char separator)
    {
        return $"{vector.X}{separator}{vector.Y}";
    }
}
