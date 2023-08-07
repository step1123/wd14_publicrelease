using Content.Shared.White.EntityCrimeRecords;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement.Components;
using Robust.Client.Player;
using Robust.Client.Graphics;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory;
using Robust.Shared.Enums;

namespace Content.Client.White.EntityCrimeRecords
{
    public sealed class ShowCrimeRecordsSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerManager _player = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IOverlayManager _overlayMan = default!;
        [Dependency] private readonly InventorySystem _inventorySystem = default!;

        private EntityCrimeRecordsOverlay _overlay = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShowCrimeRecordsComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<ShowCrimeRecordsComponent, ComponentRemove>(OnRemove);
            SubscribeLocalEvent<ShowCrimeRecordsComponent, PlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<ShowCrimeRecordsComponent, PlayerDetachedEvent>(OnPlayerDetached);
            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

            _overlay = new(EntityManager, _inventorySystem, this);
        }

        private void OnInit(EntityUid uid, ShowCrimeRecordsComponent component, ComponentInit args)
        {
            if (_player.LocalPlayer?.ControlledEntity == uid)
            {
                _overlayMan.AddOverlay(_overlay);
            }
        }
        private void OnRemove(EntityUid uid, ShowCrimeRecordsComponent component, ComponentRemove args)
        {
            if (_player.LocalPlayer?.ControlledEntity == uid)
            {
                _overlayMan.RemoveOverlay(_overlay);
            }
        }

        private void OnPlayerAttached(EntityUid uid, ShowCrimeRecordsComponent component, PlayerAttachedEvent args)
        {
            _overlayMan.AddOverlay(_overlay);
        }

        private void OnPlayerDetached(EntityUid uid, ShowCrimeRecordsComponent component, PlayerDetachedEvent args)
        {
            _overlayMan.RemoveOverlay(_overlay);
        }

        private void OnRoundRestart(RoundRestartCleanupEvent args)
        {
            _overlayMan.RemoveOverlay(_overlay);
        }

        public string CanIdentityName(EntityUid target)
        {
            var representation = GetIdentityRepresentation(target);
            var ev = new SeeIdentityAttemptEvent();

            RaiseLocalEvent(target, ev);
            return representation.ToStringKnown(!ev.Cancelled);
        }

        /// <summary>
        ///     Gets an 'identity representation' of an entity, with their true name being the entity name
        ///     and their 'presumed name' and 'presumed job' being the name/job on their ID card, if they have one.
        /// </summary>
        private IdentityRepresentation GetIdentityRepresentation(EntityUid target,
            InventoryComponent? inventory=null,
            HumanoidAppearanceComponent? appearance=null)
        {
            var age = 18;
            var gender = Gender.Epicene;

            // Always use their actual age and gender, since that can't really be changed by an ID.
            if (Resolve(target, ref appearance, false))
            {
                gender = appearance.Gender;
                age = appearance.Age;
            }

            var trueName = Name(target);
            if (!Resolve(target, ref inventory, false))
                return new(trueName, age, gender, string.Empty);

            string? presumedJob = null;
            string? presumedName = null;

            // If it didn't find a job, that's fine.
            return new IdentityRepresentation(trueName, age, gender, presumedName, presumedJob);
        }
    }
}
