using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Diagnostics;
using UnityEngine;
using KoaleskMod.KoaleskCharacter.Content;
using UnityEngine.Networking;
using KoaleskMod.Modules.BaseStates;
using System.Linq;
using RoR2;
using R2API;
using static R2API.DamageAPI;
using RoR2.Audio;
using static EntityStates.BaseState;

namespace KoaleskMod.KoaleskCharacter.SkillStates
{
    public class FlowerDance : BaseKoaleskSkillState
    {
        protected string hitboxGroupName = "SwordGroup";

        protected DamageType damageType = DamageType.Generic;
        protected List<DamageAPI.ModdedDamageType> moddedDamageTypeHolder = new List<DamageAPI.ModdedDamageType>();
        protected float damageCoefficient = 3.5f;
        protected float procCoefficient = 1f;
        protected float pushForce = 300f;
        protected Vector3 bonusForce = Vector3.zero;
        protected float baseDuration = 1f;

        protected float attackStartPercentTime = 0.2f;
        protected float attackEndPercentTime = 0.4f;

        protected float earlyExitPercentTime = 0.4f;

        protected float hitStopDuration = 0.012f;
        protected float attackRecoil = 0.75f;
        protected float hitHopVelocity = 4f;

        protected string swingSoundString = "";
        protected string hitSoundString = "";
        protected string muzzleString = "SwingCenter";
        protected string playbackRateParam = "Slash.playbackRate";
        protected GameObject swingEffectPrefab;
        protected GameObject hitEffectPrefab;
        protected NetworkSoundEventIndex impactSound = NetworkSoundEventIndex.Invalid;

        public float duration;
        protected bool hasFired;
        private float hitPauseTimer;
        protected OverlapAttack attack;
        protected bool inHitPause;
        private bool hasHopped;
        protected float stopwatch;
        protected Animator animator;
        private HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;

        private GameObject swingInstance;
        public override void OnEnter()
        {
            duration = 0.25f / attackSpeedStat;

            base.OnEnter();

            attack = new OverlapAttack();
            attack.damageType = DamageType.Generic;
            attack.damageColorIndex = DamageColorIndex.Default;
            attack.attacker = this.gameObject;
            attack.inflictor = this.gameObject;
            attack.teamIndex = TeamIndex.None;
            attack.damage = KoaleskConfig.swingLargeDamageCoefficient.Value * damageStat;
            attack.procCoefficient = 1;
            attack.hitEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactExecute");
            attack.forceVector = Vector3.zero;
            attack.pushAwayForce = 750f;
            attack.hitBoxGroup = FindHitBoxGroup("BigMeleeHitbox");
            attack.isCrit = RollCrit();
            attack.impactSound = NetworkSoundEventIndex.Invalid;

            attackRecoil = 2f / attackSpeedStat;

            characterMotor.Motor.ForceUnground();

            SmallHop(characterMotor, 6f);

            PlayCrossfade("Fullbody, Override", "CastCharge", 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            StartAimMode(GetAimRay(), 2f);

            float stacksAvailable = characterBody.GetBuffCount(KoaleskBuffs.koaleskLiquorBuff) + 1;

            hitPauseTimer -= Time.fixedDeltaTime;

            if (hitPauseTimer <= 0f && inHitPause)
            {
                RemoveHitstop();
            }

            if (!inHitPause)
            {
                stopwatch += Time.fixedDeltaTime;
            }
            else
            {
                if (characterMotor) characterMotor.velocity = Vector3.zero;
                if (animator) animator.SetFloat(playbackRateParam, 0f);
            }

            if (stopwatch >= duration && stacksAvailable > 0)
            {
                attack.ResetIgnoredHealthComponents();

                stopwatch -= duration;

                SmallHop(characterMotor, 6f);

                EnterAttack();
                FireAttack();

                if (NetworkServer.active)
                {
                    koaleskController.ConsumeLiquor(characterBody.GetBuffCount(KoaleskBuffs.koaleskLiquorBuff) - 1);
                }

                stacksAvailable--;
            }

            if (base.isAuthority && ((!IsKeyDownAuthority() && stacksAvailable <= 0) || stacksAvailable <= 0))
            {
                outer.SetNextStateToMain();
            }
        }

        protected void EnterAttack()
        {
            Util.PlayAttackSpeedSound(swingSoundString, gameObject, attackSpeedStat);

            PlaySwingEffect();

            if (base.isAuthority)
            {
                AddRecoil(-1f * attackRecoil, -2f * attackRecoil, -0.5f * attackRecoil, 0.5f * attackRecoil);
            }
        }
        protected virtual void OnHitEnemyAuthority()
        {
            Util.PlaySound(hitSoundString, gameObject);

            if (!hasHopped)
            {
                if (characterMotor && !characterMotor.isGrounded && hitHopVelocity > 0f)
                {
                    SmallHop(characterMotor, hitHopVelocity);
                }

                hasHopped = true;
            }

            ApplyHitstop();
        }

        protected void ApplyHitstop()
        {
            if (!inHitPause && hitStopDuration > 0f)
            {
                storedVelocity = characterMotor.velocity;
                hitStopCachedState = CreateHitStopCachedState(characterMotor, animator, playbackRateParam);
                hitPauseTimer = hitStopDuration / attackSpeedStat;
                inHitPause = true;
            }
        }

        protected virtual void FireAttack()
        {
            if (base.isAuthority)
            {
                if (attack.Fire())
                {
                    OnHitEnemyAuthority();
                }
            }
        }
        protected virtual void PlaySwingEffect()
        {
            if (this.swingEffectPrefab)
            {
                Transform muzzleTransform = this.FindModelChild(this.muzzleString);
                if (muzzleTransform)
                {
                    swingInstance = UnityEngine.Object.Instantiate(swingEffectPrefab, muzzleTransform);
                }
            }
        }
        private void RemoveHitstop()
        {
            ConsumeHitStopCachedState(hitStopCachedState, characterMotor, animator);
            inHitPause = false;
            characterMotor.velocity = storedVelocity;
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnExit()
        {
            PlayCrossfade("Fullbody, Override", "BufferEmpty", 0.05f);

            if (inHitPause)
            {
                RemoveHitstop();
            }
            if (swingInstance)
            {
                UnityEngine.Object.Destroy(swingInstance);
            }

            base.OnExit();
        }
    }
}
