namespace Content.Server.White.Cult.Items.Components;

[RegisterComponent]
public sealed class ShuttleCurseComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("delayTime")]
    public TimeSpan DelayTime = TimeSpan.FromSeconds(120);

    [ViewVariables(VVAccess.ReadWrite), DataField("cooldown")]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(180);
}
