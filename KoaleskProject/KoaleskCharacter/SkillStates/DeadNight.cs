using KoaleskMod.KoaleskCharacter.Content;
using KoaleskMod.Modules.BaseStates;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;

namespace KoaleskMod.KoaleskCharacter.SkillStates
{
    public class DeadNight : BaseKoaleskSkillState
    {
        private float baseDuration = 5f;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();

            float buffCount = characterBody.GetBuffCount(KoaleskBuffs.koaleskBlightBuff);

            koaleskController.ConsumeBlight();

            duration = baseDuration + buffCount;

            if (base.isAuthority)
            {
                if (base.isAuthority)
                {
                    BulletAttack bulletAttack = new BulletAttack
                    {
                        damage = damageStat * 5f,
                        aimVector = Vector3.up,
                        origin = this.FindModelChild("SwingMuzzle3").position + Vector3.down,
                        owner = base.gameObject,
                        bulletCount = (uint)1,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
                        falloffModel = BulletAttack.FalloffModel.None,
                        force = 200f,
                        HitEffectNormal = false,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = 1f,
                        maxDistance = 10f,
                        radius = 20f,
                        isCrit = RollCrit(),
                        muzzleName = "SwingMuzzle3",
                        minSpread = 0f,
                        maxSpread = 0f,
                        hitEffectPrefab = KoaleskAssets.swordHitEffect,
                        smartCollision = true,
                        sniper = false,
                        tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunCryo.prefab").WaitForCompletion(),
                        hitMask = (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask,
                        stopperMask = LayerIndex.world.mask
                    };
                    bulletAttack.AddModdedDamageType(DamageTypes.KoaleskDeadNightDamage);

                    bulletAttack.Fire();
                }

                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
