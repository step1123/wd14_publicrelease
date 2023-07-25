using Content.Server.Mind;
using Content.Server.Mind.Components;
using Content.Server.White.Cult.Runes.Comps;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.White.Cult;
using Content.Shared.White.Cult.Items;

namespace Content.Server.White.Cult.Runes.Systems;

public partial class CultSystem
{
    [Dependency] private readonly SharedPointLightSystem _lightSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;

    public void InitializeSoulShard()
    {
        SubscribeLocalEvent<SoulShardComponent, AfterInteractEvent>(OnShardInteractUse);
        SubscribeLocalEvent<SoulShardComponent, MindAddedMessage>(OnShardMindAdded);
        SubscribeLocalEvent<SoulShardComponent, MindRemovedMessage>(OnShardMindRemoved);
    }

    private void OnShardInteractUse(EntityUid uid, SoulShardComponent component, AfterInteractEvent args)
    {
        var target = args.Target;

        if(!HasComp<CultistComponent>(args.User)) return;

        if(!TryComp<MobStateComponent>(target, out var state) || state.CurrentState != MobState.Dead) return;

        if(!TryComp<MindContainerComponent>(target, out var mindComponent) || mindComponent.Mind is null || !TryComp<HumanoidAppearanceComponent>(target, out _)) return;

        _mindSystem.TransferTo(mindComponent.Mind, uid);

        var shardMetaData = MetaData(uid);
        var targetName = MetaData(target.Value).EntityName;

        shardMetaData.EntityName = Loc.GetString("soul-shard-description",("soul", targetName));
        shardMetaData.EntityDescription = Loc.GetString("soul-shard-description", ("soul", targetName));
    }

    private void OnShardMindAdded(EntityUid uid, SoulShardComponent component, MindAddedMessage args)
    {
        _appearanceSystem.SetData(uid, SoulShardVisualState.State, true);
        _lightSystem.SetEnabled(uid, true);
    }

    private void OnShardMindRemoved(EntityUid uid, SoulShardComponent component, MindRemovedMessage args)
    {
        _appearanceSystem.SetData(uid, SoulShardVisualState.State, false);
        _lightSystem.SetEnabled(uid, false);
    }
}
