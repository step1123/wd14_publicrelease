namespace Content.Shared.White.ServerEvent.Data;

[ImplicitDataDefinitionForInheritors]
public interface IEventAction
{
    void Execute(ServerEventPrototype prototype);
}
