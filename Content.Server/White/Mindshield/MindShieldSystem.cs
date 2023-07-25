using System.Linq;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Implants;
using Content.Shared.Humanoid;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Tag;
using Content.Shared.White.Mindshield;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;


namespace Content.Server.White.Mindshield;

public class MindShieldImplanted
{
    public EntityUid Target;
    public EntityUid MindShield;
    public MindShieldImplanted(EntityUid target, EntityUid mindShield)
    {
        Target = target;
        MindShield = mindShield;
    }
}
public sealed class MindShieldSystem : EntitySystem
{
    [Dependency] private readonly SubdermalImplantSystem _subdermalImplantSystem = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    private Dictionary<EntityUid, SubdermalImplantComponent> _mindShields = new();

    private string _mindshieldPrototype = "MindShieldImplant";


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SubdermalImplantInserted>(OnImplantAdded);
        SubscribeLocalEvent<SubdermalImplantRemoved>(OnImplantRemoved);
    }

    public void RemoveMindShieldImplant(EntityUid target, EntityUid mindShieldEntity, bool removeComponent = true)
    {

        _subdermalImplantSystem.ForceRemove(target, mindShieldEntity);

        if (removeComponent)
        {
            EntityManager.RemoveComponent<MindShieldComponent>(target);
        }

        if (_mindShields.TryGetValue(target, out _))
        {
            _mindShields.Remove(target);
        }
    }

    private void OnImplantAdded(SubdermalImplantInserted ev)
    {
        if (!_tagSystem.HasTag(ev.Component.Owner, "MindshieldImplant")) return;


        var entity = ev.Component.ImplantedEntity;
        if (entity == null) return;

        EntityManager.EnsureComponent<MindShieldComponent>(entity.Value);
        _mindShields[entity.Value] = ev.Component;

        RaiseLocalEvent(new MindShieldImplanted(entity.Value, ev.Component.Owner));
    }

    private void OnImplantRemoved(SubdermalImplantRemoved ev)
    {
        if (!_tagSystem.HasTag(ev.Component.Owner, "MindshieldImplant")) return;

        var entity = ev.Component.ImplantedEntity;
        if (entity == null) return;

        RemComp<MindShieldComponent>(ev.Component.ImplantedEntity!.Value);
    }
}
