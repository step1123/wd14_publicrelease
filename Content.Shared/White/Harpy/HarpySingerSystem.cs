using Content.Shared.Actions;

namespace Content.Shared.White.Harpy
{
    public class HarpySingerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<HarpySingerComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<HarpySingerComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnStartup(EntityUid uid, HarpySingerComponent component, ComponentStartup args)
        {
            if (component.MidiAction != null) //WD EDIT
                _actionsSystem.AddAction(uid, component.MidiAction, null); //WD EDIT
        }

        private void OnShutdown(EntityUid uid, HarpySingerComponent component, ComponentShutdown args)
        {
            if (component.MidiAction != null) //WD EDIT
                _actionsSystem.RemoveAction(uid, component.MidiAction); //WD EDIT
        }
    }
}
