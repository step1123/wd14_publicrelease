﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Network;

namespace Content.Server.Commands
{
    /// <summary>
    /// Utilities for writing commands
    /// </summary>
    public static class CommandUtils
    {
        /// <summary>
        /// Gets the player session for the player with the indicated id,
        /// sending a failure to the performer if unable to.
        /// </summary>
        public static bool TryGetSessionByUsernameOrId(IConsoleShell shell,
            string usernameOrId, IPlayerSession performer, [NotNullWhen(true)] out IPlayerSession? session)
        {
            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (plyMgr.TryGetSessionByUsername(usernameOrId, out session)) return true;
            if (Guid.TryParse(usernameOrId, out var targetGuid))
            {
                if (plyMgr.TryGetSessionById(new NetUserId(targetGuid), out session)) return true;
                shell.WriteLine("Unable to find user with that name/id.");
                return false;
            }

            shell.WriteLine("Unable to find user with that name/id.");
            return false;
        }

        /// <summary>
        /// Gets the attached entity for the player session with the indicated id,
        /// sending a failure to the performer if unable to.
        /// </summary>
        public static bool TryGetAttachedEntityByUsernameOrId(IConsoleShell shell,
            string usernameOrId, IPlayerSession performer, [NotNullWhen(true)] out EntityUid? attachedEntity)
        {
            attachedEntity = null;
            if (!TryGetSessionByUsernameOrId(shell, usernameOrId, performer, out var session)) return false;
            if (session.AttachedEntity == null)
            {
                shell.WriteLine("User has no attached entity.");
                return false;
            }

            attachedEntity = session.AttachedEntity;
            return true;
        }

        public static string SubstituteEntityDetails(IConsoleShell shell, EntityUid ent, string ruleString)
        {
            // gross, is there a better way to do this?
            ruleString = ruleString.Replace("$ID", ent.ToString());
            ruleString = ruleString.Replace("$WX",
                IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(ent).WorldPosition.X.ToString(CultureInfo.InvariantCulture));
            ruleString = ruleString.Replace("$WY",
                IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(ent).WorldPosition.Y.ToString(CultureInfo.InvariantCulture));
            ruleString = ruleString.Replace("$LX",
                IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(ent).LocalPosition.X.ToString(CultureInfo.InvariantCulture));
            ruleString = ruleString.Replace("$LY",
                IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(ent).LocalPosition.Y.ToString(CultureInfo.InvariantCulture));
            ruleString = ruleString.Replace("$NAME", IoCManager.Resolve<IEntityManager>().GetComponent<MetaDataComponent>(ent).EntityName);

            if (shell.Player is IPlayerSession player)
            {
                if (player.AttachedEntity is {Valid: true} p)
                {
                    ruleString = ruleString.Replace("$PID", ent.ToString());
                    ruleString = ruleString.Replace("$PWX",
                        IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(p).WorldPosition.X.ToString(CultureInfo.InvariantCulture));
                    ruleString = ruleString.Replace("$PWY",
                        IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(p).WorldPosition.Y.ToString(CultureInfo.InvariantCulture));
                    ruleString = ruleString.Replace("$PLX",
                        IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(p).LocalPosition.X.ToString(CultureInfo.InvariantCulture));
                    ruleString = ruleString.Replace("$PLY",
                        IoCManager.Resolve<IEntityManager>().GetComponent<TransformComponent>(p).LocalPosition.Y.ToString(CultureInfo.InvariantCulture));
                }
            }
            return ruleString;
        }
    }
}
