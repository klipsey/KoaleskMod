using HG;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using RoR2.Projectile;
using KoaleskMod.Modules;
using KoaleskMod.KoaleskCharacter.Components;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using KoaleskMod.KoaleskCharacter.SkillStates;

namespace KoaleskMod.KoaleskCharacter.Content
{
    public static class DamageTypes
    {
        public static DamageAPI.ModdedDamageType Default;
        public static DamageAPI.ModdedDamageType KoaleskDarkThornDamage;
        public static DamageAPI.ModdedDamageType KoaleskLiquorDamage;
        public static DamageAPI.ModdedDamageType KoaleskBlightDamage;
        public static DamageAPI.ModdedDamageType KoaleskBlightProjectileDamage;
        public static DamageAPI.ModdedDamageType KoaleskGardenDamage;
        public static DamageAPI.ModdedDamageType KoaleskDeadNightDamage;

        internal static void Init()
        {
            Default = DamageAPI.ReserveDamageType();
            KoaleskDarkThornDamage = DamageAPI.ReserveDamageType();
            KoaleskBlightDamage = DamageAPI.ReserveDamageType();
            KoaleskLiquorDamage = DamageAPI.ReserveDamageType();
            KoaleskGardenDamage = DamageAPI.ReserveDamageType();
            KoaleskDeadNightDamage = DamageAPI.ReserveDamageType();
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

                        if (damageInfo.HasModdedDamageType(KoaleskBlightDamage))
                        {
                            koaleskController.AddBuffResetDecay(KoaleskBuffs.koaleskLiquorBuff);
                        }
                    }

                    if(victimBody && attackerBody)
                    {
                        if (attackerBody.HasBuff(KoaleskBuffs.koaleskGardenBuff) && damageInfo.damageColorIndex != DamageColorIndex.Item && !damageInfo.HasModdedDamageType(KoaleskGardenDamage))
                        {
                            float damageCoefficient2 = 0.2f;
                            float damage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient2);
                            float force = 0f;
                            Vector3 position = damageInfo.position;
                            ProjectileManager.instance.FireProjectile(KoaleskAssets.KoaleskGardenFlower, position, Quaternion.identity, damageInfo.attacker, damage, force, damageInfo.crit, DamageColorIndex.Item);

                            for(int i = 0; i < attackerBody.GetBuffCount(KoaleskBuffs.koaleskGardenBuff) - 1; i++)
                            {
                                Vector3 newPosition = position + new Vector3(
                                    (UnityEngine.Random.value - 0.5f) * 10f,
                                    (UnityEngine.Random.value - 0.5f) * 10f,
                                    (UnityEngine.Random.value - 0.5f) * 10f);

                                ProjectileManager.instance.FireProjectile(KoaleskAssets.KoaleskGardenFlower, newPosition, Quaternion.identity, damageInfo.attacker, damage, force, damageInfo.crit, DamageColorIndex.Item);
                            }
                        }
                        else if(damageInfo.HasModdedDamageType(KoaleskGardenDamage))
                        {
                            attackerBody.healthComponent.Heal(attackerBody.healthComponent.fullHealth * 0.025f, default(ProcChainMask));
                        }

                        if (damageInfo.HasModdedDamageType(KoaleskDarkThornDamage))
                        {
                            PullEnemiesTowardsBody(attackerBody, victimBody, 25f);
                        }

                        if (damageInfo.HasModdedDamageType(KoaleskDeadNightDamage))
                        {
                            if (!victimBody.isBoss)
                            {
                                CharacterMotor victimMotor = victim.GetComponent<CharacterMotor>();
                                victim.GetComponent<RigidbodyMotor>();
                                if (victimMotor && victimMotor.isGrounded && !victimBody.isChampion && (victimBody.bodyFlags & CharacterBody.BodyFlags.IgnoreFallDamage) == 0 && !victimBody.HasBuff(KoaleskBuffs.koaleskDeadOfNightBuff))
                                {
                                    victimBody.AddTimedBuff(KoaleskBuffs.koaleskDeadOfNightBuff, 5f);
                                    if (!victimBody.mainHurtBox)
                                    {
                                        _ = victimBody.transform;
                                    }
                                    else
                                    {
                                        _ = victimBody.mainHurtBox.transform;
                                    }
                                    Vector3 upVector = new Vector3(0f, 1f, 0f);
                                    float massCalc = victimMotor.mass * 20f;
                                    float finalCalc = massCalc + massCalc / 10f * 2f;
                                    victimMotor.ApplyForce(finalCalc * upVector);

                                    if (victim.TryGetComponent(out SetStateOnHurt setStateOnHurt))
                                    {
                                        HeldState bubbledState = new HeldState();
                                        setStateOnHurt.targetStateMachine.SetInterruptState(bubbledState, InterruptPriority.Frozen);

                                        EntityStateMachine[] array = setStateOnHurt.idleStateMachine;

                                        for (int i = 0; i < array.Length; i++)
                                        {
                                            array[i].SetNextState(new Idle());
                                        }
                                    }
                                    else if (victim.TryGetComponent(out EntityStateMachine entityMachine))
                                    {
                                        HeldState bubbledState = new HeldState();
                                        entityMachine.SetInterruptState(bubbledState, InterruptPriority.Frozen);
                                    }
                                }
                                else
                                {
                                    victimBody.AddTimedBuff(KoaleskBuffs.koaleskDeadOfNightBuff, 5f);
                                }
                            }
                        }
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
