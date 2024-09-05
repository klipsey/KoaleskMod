using UnityEngine;
using EntityStates;
using KoaleskMod.Modules.BaseStates;
using RoR2;
using UnityEngine.AddressableAssets;
using KoaleskMod.KoaleskCharacter.Content;
using static R2API.DamageAPI;
using R2API;

namespace KoaleskMod.KoaleskCharacter.SkillStates
{
    public class DarkThorn : BaseMeleeAttack
    {
        public static GameObject clawTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunCryo.prefab").WaitForCompletion();
        private GameObject swingInstance;
        private int blightStacks;
        public override void OnEnter()
        {
            blightStacks = characterBody.GetBuffCount(KoaleskBuffs.koaleskBlightBuff);

            RefreshState();
            hitboxGroupName = "DarkThornHitbox";

            damageType = DamageType.Generic;
            damageCoefficient = KoaleskConfig.swingDamageCoefficient.Value;
            procCoefficient = 1f;
            pushForce = 0;
            bonusForce = Vector3.zero;
            baseDuration = 1.6f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.25f;
            attackEndPercentTime = 0.6f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.8f;

            hitStopDuration = 0.25f;
            attackRecoil = 2f / attackSpeedStat;
            hitHopVelocity = 4f;

            swingSoundString = "Play_merc_sword_swing";
            hitSoundString = "";
            muzzleString = "SwingMuzzle3";
            playbackRateParam = "Swing.playbackRate";
            swingEffectPrefab = KoaleskAssets.swordSwingEffect;

            hitEffectPrefab = KoaleskAssets.swordHitEffect;

            impactSound = KoaleskAssets.swordImpactSoundEvent.index;

            base.OnEnter();

            koaleskController.ConsumeBlight();
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
        }

        protected override void FireAttack()
        {
            Ray aimRay = GetAimRay();

            if (base.isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack
                {
                    damage = damageStat * damageCoefficient,
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    owner = base.gameObject,
                    bulletCount = (uint)1,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.None,
                    force = 0,
                    HitEffectNormal = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    maxDistance = 20f + blightStacks,
                    radius = 6f,
                    isCrit = RollCrit(),
                    muzzleName = "HandL",
                    minSpread = 0f,
                    maxSpread = 0f,
                    hitEffectPrefab = hitEffectPrefab,
                    smartCollision = true,
                    sniper = false,
                    tracerEffectPrefab = clawTracer,
                    hitMask = (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask,
                    stopperMask = LayerIndex.world.mask
            };
                bulletAttack.AddModdedDamageType(DamageTypes.KoaleskDarkThornDamage);
                bulletAttack.AddModdedDamageType(DamageTypes.KoaleskBlightedDamage);

                bulletAttack.Fire();

                hasFired = true;
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
            PlayCrossfade("Gesture, Override", "SwingClaw", playbackRateParam, duration, duration * 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (swingInstance) GameObject.Destroy(swingInstance);
        }
    }
}