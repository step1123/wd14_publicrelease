using Content.Shared.White.ServerEvent;
using Content.Shared.White.ServerEvent.Data;

namespace Content.Server.White.ERTRecruitment.Events;

public sealed class ERTRecruitmentStartEvent : IEventAction
{
    public void Execute(ServerEventPrototype p)
    {
        Logger.Debug("Event " + p.ID + " is started!");
        IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ERTRecruitmentSystem>().EventStart();
    }
}

public sealed class ERTRecruitmentEndEvent : IEventAction
{
    public void Execute(ServerEventPrototype p)
    {
        Logger.Debug("Event " + p.ID + " is end!");
        IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ERTRecruitmentSystem>().EventEnd();
    }
}
