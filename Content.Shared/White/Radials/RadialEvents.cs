using Content.Shared.ActionBlocker;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.White.Radials;

    [Serializable, NetSerializable]
    public sealed class RequestServerRadialsEvent : EntityEventArgs
    {
        public readonly EntityUid EntityUid;

        public readonly List<string> RadialTypes = new();

        public readonly EntityUid? SlotOwner;

        public readonly bool AdminRequest;

        public RequestServerRadialsEvent(EntityUid entityUid, IEnumerable<Type> radialTypes, EntityUid? slotOwner = null, bool adminRequest = false)
        {
            EntityUid = entityUid;
            SlotOwner = slotOwner;
            AdminRequest = adminRequest;

            foreach (var type in radialTypes)
            {
                //DebugTools.Assert(typeof(Radial).IsAssignableFrom(type));
                RadialTypes.Add(type.Name);
            }
        }
    }

    [Serializable, NetSerializable]
    public sealed class RadialsResponseEvent : EntityEventArgs
    {
        public readonly List<Radial>? Radials;
        public readonly EntityUid Entity;

        public RadialsResponseEvent(EntityUid entity, SortedSet<Radial>? radials)
        {
            Entity = entity;

            if (radials == null)
                return;

            Radials = new(radials);
        }
    }

    [Serializable, NetSerializable]
    public sealed class ExecuteRadialEvent : EntityEventArgs
    {
        public readonly EntityUid Target;
        public readonly Radial RequestedRadial;

        public ExecuteRadialEvent(EntityUid target, Radial requestedRadial)
        {
            Target = target;
            RequestedRadial = requestedRadial;
        }
    }

    public sealed class GetRadialsEvent<TValue> : EntityEventArgs where TValue : Radial
    {

        public readonly SortedSet<TValue> Radials = new();

        public readonly bool CanAccess = false;

        public readonly EntityUid Target;

        public readonly EntityUid User;

        public readonly bool CanInteract;

        public readonly HandsComponent? Hands;

        public readonly EntityUid? Using;

        public GetRadialsEvent(EntityUid user, EntityUid target, EntityUid? @using, HandsComponent? hands, bool canInteract, bool canAccess)
        {
            User = user;
            Target = target;
            Using = @using;
            Hands = hands;
            CanAccess = canAccess;
            CanInteract = canInteract;
        }
    }
