namespace Content.Server.White.WhiteGPT;

[RegisterComponent]
public sealed class ChatGPTComponent : Component
{
    public string? GPTOwner { get; set; }
    public bool Thinking { get; set; }
}
