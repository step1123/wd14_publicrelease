using Robust.Shared.Audio;

namespace Content.Server.White.Halt;

[RegisterComponent]
public sealed class HaltComponent : Component
{
    [DataField("color"), ViewVariables(VVAccess.ReadWrite)]
    public string ChatColor { get; set; } = Color.Red.ToHex();

    [DataField("locale"), ViewVariables(VVAccess.ReadWrite)]
    public string ChatLoc { get; set; } = "chat-manager-entity-say-hailer-wrap-message";

    public readonly Dictionary<string, SoundSpecifier> PhraseToSoundMap = new()
    {
        ["halt-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/halt.ogg"),
        ["bobby-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/bobby.ogg"),
        ["compliance-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/compliance.ogg"),
        ["justice-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/justice.ogg"),
        ["running-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/running.ogg"),
        ["dontmove-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/dontmove.ogg"),
        ["floor-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/floor.ogg"),
        ["robocop-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/robocop.ogg"),
        ["god-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/god.ogg"),
        ["freeze-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/freeze.ogg"),
        ["imperial-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/imperial.ogg"),
        ["bash-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/bash.ogg"),
        ["harry-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/harry.ogg"),
        ["asshole-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/asshole.ogg"),
        ["stfu-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/stfu.ogg"),
        ["shutup-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/shutup.ogg"),
        ["super-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/super.ogg"),
        ["dredd-phrase"] = new SoundPathSpecifier("/Audio/Voice/Complionator/dredd.ogg")
    };
}
