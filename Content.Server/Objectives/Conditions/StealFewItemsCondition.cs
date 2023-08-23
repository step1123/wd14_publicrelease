using Content.Server.Objectives.Interfaces;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using System.Linq;
using Content.Shared.Tag;

namespace Content.Server.Objectives.Conditions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed class StealFewItemsCondition : IObjectiveCondition, ISerializationHooks
    {
        private Mind.Mind? _mind;

        [DataField("prototype")]
        private string _prototypeId = string.Empty;

        [DataField("prototypeParent")]
        private string _prototypeParentId = string.Empty;

        [DataField("tag")]
        private string _tag = string.Empty;

        [DataField("quantityRange")]
        private List<int> _quantityRange = new();

        private int _quantity = 0;
        /// <summary>
        /// Help newer players by saying e.g. "steal the chief engineer's advanced magboots"
        /// instead of "steal advanced magboots. Should be a loc string.
        /// </summary>
        [DataField("owner")]
        private string? _owner = null;

        private string PrototypeName =>
            IoCManager.Resolve<IPrototypeManager>().TryIndex<EntityPrototype>(
                _prototypeParentId == string.Empty ? _prototypeId : _prototypeParentId, out var prototype
                ) ? prototype.Name : "[CANNOT FIND NAME]";

        public string Title =>
            _owner == null
                ? Loc.GetString("objective-condition-steal-few-items-title-no-owner",
                    ("itemName", Loc.GetString(PrototypeName)),
                    ("quantity", _quantity))
                : Loc.GetString("objective-condition-steal-few-items-title",
                    ("owner", Loc.GetString(_owner)),
                    ("itemName", Loc.GetString(PrototypeName)),
                    ("quantity", _quantity));

        public string Description => Loc.GetString("objective-condition-steal-few-items-description",
            ("itemName", Loc.GetString(PrototypeName)),
            ("quantity", _quantity));

        public SpriteSpecifier Icon => new SpriteSpecifier.EntityPrototype(_prototypeId);

        public float Difficulty => 2.25f;

        public IObjectiveCondition GetAssigned(Mind.Mind mind)
        {
            return new StealFewItemsCondition
            {
                _mind = mind,
                _prototypeId = _prototypeId,
                _prototypeParentId = _prototypeParentId,
                _tag = _tag,
                _owner = _owner,
                _quantity = IoCManager.Resolve<IRobustRandom>().Next(_quantityRange[0], _quantityRange[1])
            };
        }

        public float Progress
        {
            get
            {
                var uid = _mind?.OwnedEntity;
                var entMan = IoCManager.Resolve<IEntityManager>();
                var currentQuantity = 0;

                var metaQuery = entMan.GetEntityQuery<MetaDataComponent>();
                var managerQuery = entMan.GetEntityQuery<ContainerManagerComponent>();
                var tagQuery = entMan.GetEntityQuery<TagComponent>();
                var stack = new Stack<ContainerManagerComponent>();

                if (!metaQuery.TryGetComponent(_mind?.OwnedEntity, out var meta))
                    return 0f;

                if (meta.EntityPrototype?.ID == _prototypeId)
                    return 1f;

                if (!managerQuery.TryGetComponent(uid, out var currentManager))
                    return 0f;

                do
                {
                    foreach (var container in currentManager.Containers.Values)
                    {
                        if (container.ID == "BodyContainer")
                            continue;

                        foreach (var entity in container.ContainedEntities)
                        {
                            var isParentProto = metaQuery.GetComponent(entity).EntityPrototype?.Parents
                                ?.Contains(_prototypeParentId) ?? false;
                            var hasTag = _tag != string.Empty && tagQuery.TryGetComponent(entity, out var tag) &&
                                         tag.Tags.Contains(_tag);
                            if (metaQuery.GetComponent(entity).EntityPrototype?.ID == _prototypeId || isParentProto ||
                                hasTag)
                                currentQuantity += 1;

                            if (!managerQuery.TryGetComponent(entity, out var containerManager))
                                continue;

                            stack.Push(containerManager);
                        }
                    }
                } while (stack.TryPop(out currentManager));

                return Math.Min((1f / _quantity) * currentQuantity, 1f);
            }
        }

        public bool Equals(IObjectiveCondition? other)
        {
            return other is StealFewItemsCondition stealCondition &&
                   Equals(_mind, stealCondition._mind) &&
                   _prototypeId == stealCondition._prototypeId;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StealFewItemsCondition) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_mind, _prototypeId);
        }
    }
}
