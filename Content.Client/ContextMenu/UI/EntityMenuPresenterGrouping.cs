using System;
using System.Collections.Generic;
using System.Linq;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;

namespace Content.Client.ContextMenu.UI
{
    public sealed partial class EntityMenuPresenter : ContextMenuPresenter
    {
        public const int GroupingTypesCount = 2;
        private int GroupingContextMenuType { get; set; }
        public void OnGroupingChanged(int obj)
        {
            Close();
            GroupingContextMenuType = obj;
        }

        private List<List<IEntity>> GroupEntities(IEnumerable<IEntity> entities, int depth = 0)
        {
            if (GroupingContextMenuType == 0)
            {
                var newEntities = entities.GroupBy(e => e.Name + (IoCManager.Resolve<IEntityManager>().GetComponent<MetaDataComponent>(e.Uid).EntityPrototype?.ID ?? string.Empty)).ToList();
                return newEntities.Select(grp => grp.ToList()).ToList();
            }
            else
            {
                var newEntities = entities.GroupBy(e => e, new PrototypeAndStatesContextMenuComparer(depth)).ToList();
                return newEntities.Select(grp => grp.ToList()).ToList();
            }
        }

        private sealed class PrototypeAndStatesContextMenuComparer : IEqualityComparer<IEntity>
        {
            private static readonly List<Func<IEntity, IEntity, bool>> EqualsList = new()
            {
                (a, b) => IoCManager.Resolve<IEntityManager>().GetComponent<MetaDataComponent>(a.Uid).EntityPrototype!.ID == IoCManager.Resolve<IEntityManager>().GetComponent<MetaDataComponent>(b.Uid).EntityPrototype!.ID,
                (a, b) =>
                {
                    a.TryGetComponent<ISpriteComponent>(out var spriteA);
                    b.TryGetComponent<ISpriteComponent>(out var spriteB);

                    if (spriteA == null || spriteB == null)
                        return spriteA == spriteB;

                    var xStates = spriteA.AllLayers.Where(e => e.Visible).Select(s => s.RsiState.Name);
                    var yStates = spriteB.AllLayers.Where(e => e.Visible).Select(s => s.RsiState.Name);

                    return xStates.OrderBy(t => t).SequenceEqual(yStates.OrderBy(t => t));
                },
            };
            private static readonly List<Func<IEntity, int>> GetHashCodeList = new()
            {
                e => EqualityComparer<string>.Default.GetHashCode(IoCManager.Resolve<IEntityManager>().GetComponent<MetaDataComponent>(e.Uid).EntityPrototype!.ID),
                e =>
                {
                    var hash = 0;
                    foreach (var element in e.GetComponent<ISpriteComponent>().AllLayers.Where(obj => obj.Visible).Select(s => s.RsiState.Name))
                    {
                        hash ^= EqualityComparer<string>.Default.GetHashCode(element!);
                    }
                    return hash;
                },
            };

            private static int Count => EqualsList.Count - 1;

            private readonly int _depth;
            public PrototypeAndStatesContextMenuComparer(int step = 0)
            {
                _depth = step > Count ? Count : step;
            }

            public bool Equals(IEntity? x, IEntity? y)
            {
                if (x == null)
                {
                    return y == null;
                }

                return y != null && EqualsList[_depth](x, y);
            }

            public int GetHashCode(IEntity e)
            {
                return GetHashCodeList[_depth](e);
            }
        }
    }
}
