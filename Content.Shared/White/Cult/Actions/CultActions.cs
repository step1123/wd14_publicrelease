using Content.Shared.Actions;

namespace Content.Shared.White.Cult.Actions;

public sealed class CultTwistedConstructionActionEvent : EntityTargetActionEvent
{
}

public sealed class CultSummonDaggerActionEvent : InstantActionEvent
{
}

public sealed class CultStunTargetActionEvent : EntityTargetActionEvent { }

public sealed class CultTeleportTargetActionEvent : EntityTargetActionEvent {}

public sealed class CultElectromagneticPulseTargetActionEvent : EntityTargetActionEvent {}

public sealed class CultShadowShacklesTargetActionEvent : EntityTargetActionEvent {}

public sealed class CultSummonCombatEquipmentTargetActionEvent : EntityTargetActionEvent {}

public sealed class CultConcealPresenceWorldActionEvent : WorldTargetActionEvent {}

public sealed class CultBloodRitesInstantActionEvent : InstantActionEvent {}



