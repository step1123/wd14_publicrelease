using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Radio.Components;
using Content.Shared.White.Cyborg.Events;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgSystemRadioModule : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HeadsetComponent,ModuleInsertEvent>(OnRadiomoduleInsert);
        SubscribeLocalEvent<HeadsetComponent,ModuleRemoveEvent>(OnRadiomoduleRemoved);
    }

    private void OnRadiomoduleRemoved(EntityUid uid, HeadsetComponent component, ModuleRemoveEvent args)
    {
        var ev = new GotUnequippedEvent(args.CyborgUid, args.ModuleUid, new SlotDefinition());
        RaiseLocalEvent(uid,ev,true);
    }

    private void OnRadiomoduleInsert(EntityUid uid, HeadsetComponent component, ModuleInsertEvent args)
    {
        var ev = new GotEquippedEvent( args.CyborgUid, args.ModuleUid,new SlotDefinition() );
        RaiseLocalEvent(uid,ev,true);
        component.IsEquipped = true;
    }
}
