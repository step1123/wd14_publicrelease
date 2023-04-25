using Content.Server.Chat.Systems;
using Content.Shared.Humanoid;

namespace Content.Server.Chat;

public sealed class RainbowChatSystem : EntitySystem
{
    public Dictionary<string, string> NameToColor = new();

    private const float MinSaturation = 0.22f;
    private const float MaxSaturation = 0.30f;
    private const float MinValue = 0.70f;
    private const float MaxValue = 0.80f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HumanoidAppearanceComponent, SetSpeakerColorEvent>(OnHumanoidSpeak);
    }

    private void OnHumanoidSpeak(EntityUid uid, HumanoidAppearanceComponent component, SetSpeakerColorEvent args)
    {
        var color = GetColor(args.Name);
        args.Name = $"[color={color}]{args.Name}[/color]";
    }

    private string GetColor(string characterName)
    {
        if (NameToColor.TryGetValue(characterName, out var hexColor))
            return hexColor;

        return CreateCharacterHexColor(characterName);
    }

    private string CreateCharacterHexColor(string characterName)
    {
        var random = new Random(characterName.GetHashCode());
        var h = GetRandomFloat(ref random,0f,1f);
        var s = GetRandomFloat(ref random, MinSaturation, MaxSaturation);
        var v = GetRandomFloat(ref random,MinValue, MaxValue);

        var color = Color.FromHsv(new Vector4(h, s, v, 1)).ToHex();
        NameToColor[characterName] = color;
        return color;
    }

    private float GetRandomFloat(ref Random random, float minimum, float maximum)
    {
        return (float)(random.NextDouble() * (maximum - minimum) + minimum);
    }
}
