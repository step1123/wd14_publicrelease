using System.Diagnostics.CodeAnalysis;

namespace Content.Server.Chat.Managers;

public interface IChatSanitizationManager
{
    public void Initialize();

    public bool TrySanitizeOutSmilies(string input, EntityUid speaker, out string sanitized, [NotNullWhen(true)] out string? emote);

    //WD-EDIT
    public string SanitizeOutSlang(string input);

    public string SanitizeTags(string input);
    //WD-EDIT
}
