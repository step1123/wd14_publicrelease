using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.White.CheapSurgery;

[Serializable,NetSerializable]
public sealed class OnSurgeryStarted : EntityEventArgs
{
    public EntityUid Target;
    public List<OrganItem> OrganItems;

    public OnSurgeryStarted(EntityUid target, List<OrganItem> organItems)
    {
        Target = target;
        OrganItems = organItems;
    }
}

[Serializable,NetSerializable]
public sealed class OrganItem
{
    public string Name;
    public EntityUid Uid;
    public SpriteSpecifier Icon;
    public List<OrganItem> Children = new();

    public OrganItem(string name, EntityUid uid, SpriteSpecifier icon)
    {
        Name = name;
        Uid = uid;
        Icon = icon;
    }
}

[Serializable,NetSerializable]
public sealed class OnOrganSelected : EntityEventArgs
{
    public EntityUid Uid;

    public OnOrganSelected(EntityUid uid)
    {
        Uid = uid;
    }
}
