using Oxide.Core;
using Oxide.Plugins;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System;


namespace Oxide.Plugins
{
    [Info("TpPlugin", "Thodex", "1.0.0")]
    class TpPlugin : RustPlugin
    {
        private float CombatTime = 20f;
        List<MonumentInfo> monumentInfos;
        Dictionary<ulong, float> lastCombat = new Dictionary<ulong, float>();
        void OnServerInitialized()
        {
            monumentInfos = GameObject.FindObjectsOfType<MonumentInfo>().ToList();
        }
        
        [ChatCommand("tp")]
        private void TpCommandHandler(BasePlayer player, string command, string[] args)
        {
            if (args[0].Length == 0)
            {
                player.ChatMessage("⚠️ Please provide a player name --> /tp <name>");
                return;
            }
            var Target = args[0];
            var Team = RelationshipManager.ServerInstance.teams[player.currentTeam];
            BasePlayer targetPlayer = null;
            bool isTeamMate = false;
            getTargetPlayerFromTeam(Target, Team.members, player, ref isTeamMate, ref targetPlayer);
            if (!isTeamMate)
            {
                player.ChatMessage("⚠️ You cant tp to this player");
                return;
            }
            if (isPlayerInMonument(targetPlayer) && isPlayerInMonument(player) && !targetPlayer.InSafeZone())
            {
                player.ChatMessage("⚠️ The player inside Monument");
                return;
            }
            if (isPlayerHavePriv(targetPlayer) && isPlayerHavePriv(player))
            {
                player.ChatMessage("⚠️ Your teammate inside others land");
                return;
            }
            if (lastCombat[targetPlayer.userID] <= CombatTime && lastCombat[player.userID] <= CombatTime)
            {
                player.ChatMessage($"⚠️ Your teammate have combat try after {(int)lastCombat[targetPlayer.userID]} seconds");
                return;
            }

            targetPlayer.Teleport(player);
        }

        public bool isPlayerInMonument(BasePlayer player)
        {
            foreach (var m in monumentInfos)
            {
                if (m.IsInBounds(player.transform.position)) return true;
            }
            return false;
        }

        public bool isPlayerHavePriv(BasePlayer player)
        {
            BuildingPrivlidge priv = player.GetBuildingPrivilege();
            if (priv == null) return false;//the player not in privilege
            return !priv.authorizedPlayers.Any(p => p.userid == player.userID); // if these return true that mean the user inside his base or he have access to tc
        }

        public void getTargetPlayerFromTeam(string name, List<ulong> TeamMembers, BasePlayer player, ref bool isTeamMate, ref BasePlayer targetPlayer)
        {
            foreach (var id in TeamMembers)
            {
                if (id == player.userID) continue;

                var teamMate = BasePlayer.FindByID(id);

                if (teamMate.displayName == name)
                {
                    isTeamMate = true;
                    targetPlayer = teamMate;
                    break;
                }
            }
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            var NowDate = Time.realtimeSinceStartup;
            var victim = entity as BasePlayer;
            var attacker = info.Initiator as BasePlayer;
            if (victim != null)
            {
                lastCombat[victim.userID] = NowDate;
            }

            if (attacker != null && attacker != victim)
            {
                lastCombat[attacker.userID] = NowDate;
            }
        }
    }
}