using EntityStates;
using KoaleskMod.KoaleskCharacter.Content;
using KoaleskMod.Modules.BaseStates;
using R2API;
using Rewired.UI.ControlMapper;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking.Match;

namespace KoaleskMod.KoaleskCharacter.SkillStates
{
    public class ShadowDance : BaseKoaleskSkillState
    {
        private bool hasDelayed;
        private float previousAirControl;
        private Vector3 direction;
        private Vector3 hitPosition;
        private float upwardVelocity = 22f;
        private float forwardVelocity = 6f;
        private float minimumY = 0.05f;
        private float aimVelocity = 6f;
        private bool hasBlight;
        private bool foundTarget;
        protected CameraTargetParams.AimRequest request;
        public override void OnEnter()
        {
            base.OnEnter();

            RaycastHit hitInfo;
            Vector3 position = inputBank.GetAimRaycast(10f, out hitInfo) ? hitInfo.point : characterBody.corePosition;

            HurtBox[] hurtBoxes = new SphereSearch
            {
                origin = position,
                radius = 20f,
                mask = LayerIndex.entityPrecise.mask
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(this.teamComponent.teamIndex)).OrderCandidatesByDistance()
                .FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            if (hurtBoxes.Length > 0)
            {
                foreach (HurtBox hurtBox in hurtBoxes)
                {
                    if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.body)
                    {
                        hitPosition = hurtBox.healthComponent.body.corePosition;
                        foundTarget = true;
                        break;
                    }
                }
            }
            else
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(this.transform.position, Vector3.down, out raycastHit, 20f, LayerIndex.world.mask))
                {
                    hitPosition = raycastHit.point;
                    foundTarget = true;
                }
            }

            if(foundTarget)
            {
                previousAirControl = base.characterMotor.airControl;
                base.characterMotor.airControl = 0.3f;

                characterBody.SetAimTimer(4f);

                if (cameraTargetParams)
                {
                    request = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
                }

                direction = GetAimRay().direction;

                base.characterMotor.Motor.ForceUnground();

                hasBlight = characterBody.HasBuff(KoaleskBuffs.koaleskBlightBuff);

                if (hasBlight) koaleskController.ConsumeBlight(characterBody.GetBuffCount(KoaleskBuffs.koaleskBlightBuff) - 1);

                BlastAttack blastAttack = new BlastAttack
                {
                    attacker = base.gameObject,
                    procChainMask = default(ProcChainMask),
                    losType = BlastAttack.LoSType.NearestHit,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Stun1s,
                    procCoefficient = 1f,
                    bonusForce = Vector3.zero,
                    baseForce = 300f,
                    baseDamage = damageStat * KoaleskConfig.shadowDanceDamageCoefficient.Value,
                    falloffModel = BlastAttack.FalloffModel.None,
                    radius = 5f,
                    position = hitPosition,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    teamIndex = base.GetTeam(),
                    inflictor = base.gameObject,
                    crit = RollCrit()
                };
                blastAttack.AddModdedDamageType(DamageTypes.KoaleskBlightDamage);

                blastAttack.Fire();

                EffectManager.SpawnEffect(KoaleskAssets.specialSlashingEffect, new EffectData
                {
                    origin = hitPosition,
                }, transmit: true);
            }
            else if(base.isAuthority)
            {
                skillLocator.utility.stock++;
                outer.SetNextStateToMain();
                return;
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hasDelayed) base.characterMotor.velocity.y = 0f;

            if (base.fixedAge >= 0.5f / attackSpeedStat && !hasDelayed)
            {
                hasDelayed = true;

                if (base.isAuthority)
                {
                    base.characterBody.isSprinting = true;
                    direction.y = minimumY;
                    Vector3 vector = direction.normalized * aimVelocity * moveSpeedStat;
                    Vector3 vector2 = Vector3.up * upwardVelocity;
                    Vector3 vector3 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                    base.characterMotor.velocity = vector + vector2 + vector3;
                }

                base.characterDirection.moveVector = direction;

                if (EntityStates.BrotherMonster.BaseSlideState.slideEffectPrefab && base.characterBody)
                {
                    Vector3 position = base.characterBody.footPosition;
                    Quaternion rotation = Quaternion.identity;
                    Transform transform = base.FindModelChild("Base");

                    if (transform)
                    {
                        position = transform.position;
                    }

                    if (base.characterDirection)
                    {
                        rotation = Util.QuaternionSafeLookRotation(direction);
                    }

                    EffectManager.SimpleEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FeatherEffect"), position, rotation, true);
                }
            }

            if(base.isAuthority && hasDelayed)
            {
                base.characterMotor.moveDirection = base.inputBank.moveVector;
                base.characterDirection.moveVector = base.characterMotor.velocity;
            }

            if(base.isAuthority && base.fixedAge >= 2f)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            if(foundTarget)
            {
                if (!outer.destroying)
                {
                    if (cameraTargetParams)
                    {
                        request.Dispose();
                    }
                }
                base.characterMotor.airControl = previousAirControl;
                base.characterBody.isSprinting = false;
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
