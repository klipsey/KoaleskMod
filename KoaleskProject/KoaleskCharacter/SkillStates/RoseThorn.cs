using UnityEngine;
using EntityStates;
using KoaleskMod.Modules.BaseStates;
using RoR2;
using UnityEngine.AddressableAssets;
using KoaleskMod.KoaleskCharacter.Content;

namespace KoaleskMod.KoaleskCharacter.SkillStates
{
    public class RoseThorn : BaseMeleeAttack
    {
        private GameObject swingInstance;
        private static float swingInterval = 0.1f;
        private float intervalStopwatch;
        private bool secondHit;
        private bool hasBloodLiquor;
        private bool hasFiredBloodLiquor;
        public override void OnEnter()
        {
            RefreshState();

            hitboxGroupName = swingIndex == 2 ? "BigMeleeHitbox" : "MeleeHitbox";

            damageType = DamageType.Generic;
            moddedDamageTypeHolder.Add(DamageTypes.KoaleskLiquorDamage);
            damageCoefficient = swingIndex == 2 ? KoaleskConfig.swingLargeDamageCoefficient.Value : KoaleskConfig.swingDamageCoefficient.Value;
            procCoefficient = 1f;
            pushForce = swingIndex == 2 ? 750f : 300f;
            bonusForce = Vector3.zero;
            baseDuration = swingIndex == 2 ? 1.75f + swingInterval : 1.1f + swingInterval;

            if(swingInterval == 1)
            {
                baseDuration += 0.1f;
            }

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = swingIndex == 3 ? 0.5f : 0.4f;
            attackEndPercentTime = swingIndex == 3 ? 0.75f : 0.6f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.85f;

            hitStopDuration = swingIndex == 2 ? 0.1f : 0.05f;
            attackRecoil = 2f / attackSpeedStat;
            hitHopVelocity = 4f;

            swingSoundString = "Play_merc_sword_swing";
            hitSoundString = "";
            muzzleString = "SwingMuzzle" + (swingIndex + 1);
            playbackRateParam = "Swing.playbackRate";
            swingEffectPrefab = KoaleskAssets.swordSwingEffect;

            hitEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactExecute");

            base.OnEnter();

            hasBloodLiquor = characterBody.HasBuff(KoaleskBuffs.koaleskLiquorBuff);

            if(hasBloodLiquor) koaleskController.ConsumeBloodLiquor(characterBody.GetBuffCount(KoaleskBuffs.koaleskLiquorBuff) - 1);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(hasFired)
            {
                intervalStopwatch += Time.fixedDeltaTime;

                if (intervalStopwatch >= swingInterval && hasBloodLiquor && !hasFiredBloodLiquor)
                {
                    hasFiredBloodLiquor = true;

                    attack.ignoredHealthComponentList.Clear();

                    intervalStopwatch = 0;

                    Util.PlayAttackSpeedSound(swingSoundString, gameObject, attackSpeedStat);

                    PlaySwingEffect();

                    FireAttack();
                    
                    return;
                }

                if (intervalStopwatch >= swingInterval && swingIndex == 1 && !secondHit) 
                {
                    secondHit = true;

                    if (hasBloodLiquor) hasFiredBloodLiquor = false;

                    intervalStopwatch = 0;

                    attack.ignoredHealthComponentList.Clear();

                    Util.PlayAttackSpeedSound(swingSoundString, gameObject, attackSpeedStat);

                    PlaySwingEffect();

                    FireAttack();

                    return;
                }
            }
        }
        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
        }

        protected override void FireAttack()
        {
            if (base.isAuthority)
            {
                Vector3 direction = this.GetAimRay().direction;
                direction.y = Mathf.Max(direction.y, direction.y * 0.5f);
                this.FindModelChild("MeleePivot").rotation = Util.QuaternionSafeLookRotation(direction);
                base.FireAttack();
            }
        }

        protected override void PlaySwingEffect()
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

        protected override void PlayAttackAnimation()
        {
            if(inputBank.moveVector.magnitude == 0) PlayCrossfade("Gesture, Override", "Swing" + (1 + swingIndex), playbackRateParam, duration, duration * 0.05f);
            else PlayCrossfade("Gesture, Override", "Swing" + (1 + swingIndex), playbackRateParam, duration, duration * 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (swingInstance) GameObject.Destroy(swingInstance);
        }
    }
}