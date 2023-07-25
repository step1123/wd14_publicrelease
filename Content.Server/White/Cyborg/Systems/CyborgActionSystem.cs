using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.White.Cyborg.CyborgMonitoring;
using Content.Shared.White.Cyborg.Components;
using Content.Shared.White.Cyborg.Events;

namespace Content.Server.White.Cyborg.Systems;

public sealed class CyborgActionSystem: EntitySystem
{
    [Dependency] private readonly CyborgMonitoringConsoleSystem _cyborgMonitoring = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<CyborgComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
    }

    private void OnPacketReceived(EntityUid uid, CyborgComponent component, DeviceNetworkPacketEvent args)
    {
        var payload = args.Data;
        // check command
        if (!payload.TryGetValue(DeviceNetworkConstants.Command, out string? command))
            return;
        if (command != DeviceNetworkConstants.CmdSetState)
            return;

        var status = _cyborgMonitoring.PacketToCyborgAction(payload);
        if(status == null) return;

        var ev = new CyborgActionSelectedEvent(status.Action, args.Sender,status.Actioner);
        RaiseLocalEvent(uid,ev);
    }
}
