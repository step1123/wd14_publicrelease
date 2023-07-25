namespace Content.Shared.White.CheapSurgery;

[RegisterComponent]
public sealed class ActiveSurgeryComponent : Component
{
    [ViewVariables] public EntityUid OrganUid = EntityUid.Invalid;
}
