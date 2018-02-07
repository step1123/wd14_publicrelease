﻿using Content.Client.GameObjects;
using Content.Client.Interfaces.GameObjects;
using SS14.Shared.ContentPack;
using SS14.Shared.Interfaces.GameObjects;
using SS14.Shared.IoC;

namespace Content.Client
{
    public class EntryPoint : GameClient
    {
        public override void Init()
        {
            var factory = IoCManager.Resolve<IComponentFactory>();

            factory.RegisterIgnore("Inventory");
            factory.RegisterIgnore("Item");
            factory.RegisterIgnore("Interactable");
            factory.RegisterIgnore("Damageable");
            factory.RegisterIgnore("Destructible");
            factory.RegisterIgnore("Temperature");
            factory.RegisterIgnore("PowerTransfer");
            factory.RegisterIgnore("PowerNode");
            factory.RegisterIgnore("PowerProvider");
            factory.RegisterIgnore("PowerDevice");
            factory.RegisterIgnore("PowerStorage");
            factory.RegisterIgnore("PowerGenerator");

            factory.RegisterIgnore("Wirecutter");
            factory.RegisterIgnore("Screwdriver");
            factory.RegisterIgnore("Multitool");
            factory.RegisterIgnore("Welder");
            factory.RegisterIgnore("Wrench");
            factory.RegisterIgnore("Crowbar");

            factory.Register<HandsComponent>();
            factory.RegisterReference<HandsComponent, IHandsComponent>();

            factory.Register<ClientDoorComponent>();
        }
    }
}
