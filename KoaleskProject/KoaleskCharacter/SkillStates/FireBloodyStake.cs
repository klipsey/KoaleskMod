using RoR2.Projectile;
using RoR2;
using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Diagnostics;
using UnityEngine;
using KoaleskMod.KoaleskCharacter.Content;
using UnityEngine.Networking;
using KoaleskMod.Modules.BaseStates;
using KoaleskMod.Modules;

namespace KoaleskMod.KoaleskCharacter.SkillStates
{
    public class FireBloodyStake : BaseKoaleskSkillState
    {
        public int projectilesToGenerate;

        private static float baseDelay = 0.1f;

        private float delay;
        private float stopwatch;
        private int index = 0;

        private List<Vector3> vectorsToSpawnProjectiles;

        public override void OnEnter()
        {
            vectorsToSpawnProjectiles = Helpers.DistributePointsEvenlyOnSphereCap(2.5f, projectilesToGenerate, characterBody.corePosition + inputBank.aimDirection, RoR2.Util.QuaternionSafeLookRotation(inputBank.aimDirection));

            delay = baseDelay / attackSpeedStat;

            stopwatch = delay;

            base.OnEnter();

            StartAimMode(GetAimRay(), 2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.deltaTime;

            if (base.isAuthority)
            {
                if(projectilesToGenerate > 0 && stopwatch >= delay)
                {
                    stopwatch = 0f;

                    var hasValidAim = inputBank.GetAimRaycast(500, out var rayCastHit);
                    FireProjectileInfo Stake = new FireProjectileInfo()
                    {
                        owner = characterBody.gameObject,
                        projectilePrefab = KoaleskAssets.KoaleskBloodyStakeProjectile,
                        speedOverride = 150.0f,
                        damage = damageStat * KoaleskConfig.bloodyStakeDamageCoefficient.Value,
                        damageTypeOverride = null,
                        damageColorIndex = DamageColorIndex.Default,
                        position = vectorsToSpawnProjectiles[index],
                        rotation = RoR2.Util.QuaternionSafeLookRotation(inputBank.aimDirection),
                        procChainMask = default(ProcChainMask)
                    };
                    ProjectileManager.instance.FireProjectile(Stake);

                    projectilesToGenerate--;
                    index++;
                }
                else if (projectilesToGenerate <= 0)
                {
                    outer.SetNextStateToMain();
                }
            }
        }
    }
}
