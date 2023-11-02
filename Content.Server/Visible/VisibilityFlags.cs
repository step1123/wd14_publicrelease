namespace Content.Server.Visible
{
    [Flags]
    public enum VisibilityFlags : uint
    {
        None   = 0,
        Normal = 1 << 0,
        Ghost  = 1 << 1,
        Invisible = 1 << 2,
        CultVisible = 1 << 3
    }
}
