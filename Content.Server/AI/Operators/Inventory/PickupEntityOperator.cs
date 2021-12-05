using Content.Server.Hands.Components;
using Content.Server.Interaction;
using Content.Server.Items;
using Content.Shared.Interaction.Helpers;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Server.AI.Operators.Inventory
{
    public class PickupEntityOperator : AiOperator
    {
        // Input variables
        private readonly EntityUid _owner;
        private readonly EntityUid _target;

        public PickupEntityOperator(EntityUid owner, EntityUid target)
        {
            _owner = owner;
            _target = target;
        }

        public override Outcome Execute(float frameTime)
        {
            if ((!IoCManager.Resolve<IEntityManager>().EntityExists(_target) ? EntityLifeStage.Deleted : IoCManager.Resolve<IEntityManager>().GetComponent<MetaDataComponent>(_target).EntityLifeStage) >= EntityLifeStage.Deleted ||
                !IoCManager.Resolve<IEntityManager>().HasComponent<ItemComponent>(_target) ||
                _target.IsInContainer() ||
                !_owner.InRangeUnobstructed(_target, popup: true))
            {
                return Outcome.Failed;
            }

            if (!IoCManager.Resolve<IEntityManager>().TryGetComponent(_owner, out HandsComponent? handsComponent))
            {
                return Outcome.Failed;
            }

            var emptyHands = false;

            foreach (var hand in handsComponent.ActivePriorityEnumerable())
            {
                if (handsComponent.GetItem(hand) == null)
                {
                    if (handsComponent.ActiveHand != hand)
                    {
                        handsComponent.ActiveHand = hand;
                    }

                    emptyHands = true;
                    break;
                }
            }

            if (!emptyHands)
            {
                return Outcome.Failed;
            }

            var interactionSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<InteractionSystem>();
            interactionSystem.InteractHand(_owner, _target);
            return Outcome.Success;
        }
    }
}
