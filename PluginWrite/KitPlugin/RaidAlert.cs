using Oxide.Core;
using Oxide.Plugins;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using Oxide.Core.Libraries;
using ConVar;


namespace Oxide.Plugins
{
    [Info("RaidAlert", "Thodex", "1.0.0")]
    class RaidAlert : RustPlugin
    {

        void OnPlayerAttack(BasePlayer attacker, HitInfo info)
        {
            if (attacker == null || info == null) return;

            var HitEntity = info.HitEntity;

            var Walls = HitEntity as BuildingBlock;

            if (Walls != null)
            {
                var Base = Walls.GetBuildingPrivilege();
                var BaseOwners = Base.authorizedPlayers;
                string weaponName = info?.Weapon?.ShortPrefabName ?? "unknown";
                string projectileName = info?.ProjectilePrefab?.name ?? "none";
                if (projectileName.ToLower().Contains("explosive") && !BaseOwners.Any((p) => isTeamMate(attacker,p)))
                {
                    sendRiadAlert(BaseOwners, $"Your Base Under Attack with {info.ProjectilePrefab.name} by {attacker.displayName}!!!");
                }
            }
            return;
        }

        void OnExplosiveThrown(BasePlayer player, BaseEntity entity, ThrownWeapon item)
        {
            float radius = 30f;

            List<BuildingPrivlidge> privileges = new List<BuildingPrivlidge>();
            Vis.Entities(entity.transform.position, radius, privileges);

            foreach (var priv in privileges)
            {
                if (priv is BuildingPrivlidge building && !building.authorizedPlayers.Any((p) => isTeamMate(player,p)))
                {
                    sendRiadAlert(building.authorizedPlayers, $"Your Base Under Attack with {item.ShortPrefabName} by {player.displayName}!!!");
                }
            }
        }


        void OnRocketLaunched(BasePlayer player, BaseEntity entity)
        {
            float radius = 30f;

            List<BuildingPrivlidge> privileges = new List<BuildingPrivlidge>();
            Vis.Entities(entity.transform.position, radius, privileges);

            foreach (var priv in privileges)
            {
                if (priv is BuildingPrivlidge building && !building.authorizedPlayers.Any((p) => isTeamMate(player,p)))
                {
                    sendRiadAlert(building.authorizedPlayers, $"Your Base Under Attack with {entity.ShortPrefabName} by {player.displayName}!!!");
                }
            }
            return;
        }


        void OnFlameThrowerBurn(FlameThrower thrower, BaseEntity flame)
        {
            float radius = 30f;
            List<BuildingPrivlidge> privileges = new List<BuildingPrivlidge>();
            Vis.Entities(flame.transform.position, radius, privileges);

            foreach (var priv in privileges)
            {
                if (priv is BuildingPrivlidge building && !building.authorizedPlayers.Any((p) => isTeamMate(thrower.GetOwnerPlayer(),p)))
                {
                    sendRiadAlert(building.authorizedPlayers, $"Your Base Under Attack with {thrower.ShortPrefabName} by {thrower.GetOwnerPlayer().displayName}!!!");
                }
            }
            return;

        }

        void sendRiadAlert(HashSet<ProtoBuf.PlayerNameID> BaseOwners, string message)
        {
            foreach (var owner in BaseOwners)
            {
                var player = BasePlayer.FindByID(owner.userid);
                player.SendConsoleCommand("gametip.showgametip", message);
                timer.Once(7f, () =>
                {
                    player.SendConsoleCommand("gametip.hidegametip");
                });
            }
        }

        bool isTeamMate(BasePlayer attacker, ProtoBuf.PlayerNameID BaseOwner)
        {
            BasePlayer playerObject = BasePlayer.FindByID(BaseOwner.userid);
            if (attacker.currentTeam == playerObject.currentTeam && attacker.currentTeam == 0)
            {
                return true;
            }
            return false;
        }


    }
}