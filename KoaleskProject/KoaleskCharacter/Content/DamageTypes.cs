using HG;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using RoR2.Projectile;
using KoaleskMod.Modules;
using KoaleskMod.KoaleskCharacter.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using static RoR2.DotController;
using UnityEngine.Diagnostics;

namespace KoaleskMod.KoaleskCharacter.Content
{
    public static class DamageTypes
    {
        public static DamageAPI.ModdedDamageType Default;
        public static DamageAPI.ModdedDamageType KoaleskDarkThornDamage;
        public static DamageAPI.ModdedDamageType KoaleskLiquorDamage;
        public static DamageAPI.ModdedDamageType KoaleskBlightedDamage;
        public static DamageAPI.ModdedDamageType KoaleskBlightProjectileDamage;
        internal static void Init()
        {
            Default = DamageAPI.ReserveDamageType();
            KoaleskDarkThornDamage = DamageAPI.ReserveDamageType();
            KoaleskBlightedDamage = DamageAPI.ReserveDamageType();
            KoaleskLiquorDamage = DamageAPI.ReserveDamageType();
            Hook();
        }
        private static void Hook()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }
        private static void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            DamageInfo damageInfo = damageReport.damageInfo;
            if (!damageReport.attackerBody || !damageReport.victimBody)
            {
                return;
            }
            HealthComponent victim = damageReport.victim;
            GameObject inflictorObject = damageInfo.inflictor;
            CharacterBody victimBody = damageReport.victimBody;
            EntityStateMachine victimMachine = victimBody.GetComponent<EntityStateMachine>();
            CharacterBody attackerBody = damageReport.attackerBody;
            if(damageReport.attacker)
            {
                KoaleskController koaleskController = damageReport.attacker.GetComponent<KoaleskController>();

                if (NetworkServer.active)
                {
                    if (koaleskController)
                    {
                        if (damageInfo.HasModdedDamageType(KoaleskLiquorDamage))
                        {
                            koaleskController.AddBuffResetDecay(KoaleskBuffs.koaleskBlightBuff);
                        }

                        if (damageInfo.HasModdedDamageType(KoaleskBlightedDamage))
                        {
                            koaleskController.AddBuffResetDecay(KoaleskBuffs.koaleskLiquorBuff);
                        }
                    }

                    if (damageInfo.HasModdedDamageType(KoaleskDarkThornDamage) && victimBody && attackerBody)
                    {
                        PullEnemiesTowardsBody(attackerBody, victimBody, 25f);
                    }
                }
            }
        }

        public static void PullEnemiesTowardsBody(CharacterBody target, CharacterBody victim, float maxVelocity)
        {
            if (!target || !victim || !victim.rigidbody)
            {
                return;
            }

            Vector3 directionToPlayer = (target.corePosition - victim.corePosition).normalized;
            Vector3 projectedVelocity = Vector3.Project(victim.rigidbody.velocity, directionToPlayer);

            float velocityDifference = Mathf.Max(0, maxVelocity - projectedVelocity.magnitude);
            var desiredForce = directionToPlayer * velocityDifference;

            var physInfo = new PhysForceInfo()
            {
                massIsOne = true,
                disableAirControlUntilCollision = true,
                ignoreGroundStick = true,
                force = desiredForce,
            };

            var characterMotor = victim.characterMotor;
            if (characterMotor)
            {
                characterMotor.ApplyForceImpulse(physInfo);
                return;
            }

            var rigidBodyMotor = victim.GetComponent<RigidbodyMotor>();
            if (rigidBodyMotor)
            {
                rigidBodyMotor.ApplyForceImpulse(physInfo);
                return;
            }

            //Can play vfx here maybe
        }
    }
}
