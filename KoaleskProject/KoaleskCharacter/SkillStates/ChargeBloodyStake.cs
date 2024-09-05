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

namespace KoaleskMod.KoaleskCharacter.SkillStates
{
    public class ChargeBloodyStake : BaseKoaleskSkillState
    {
        public float stackConsumptionDuration = 0.25f;
        public List<GameObject> vfxCreated = new List<GameObject>();

        private float stopwatch;


        public override void OnEnter()
        {
            base.OnEnter();

            PlayCrossfade("Gesture, Override", "Cast", 0.05f);
        }

        public override void FixedUpdate()
        {
            StartAimMode(GetAimRay(), 2f);

            float stacksAvailable = characterBody.GetBuffCount(KoaleskBuffs.koaleskLiquorBuff);

            base.FixedUpdate();

            stopwatch += Time.deltaTime;

            if (stopwatch >= stackConsumptionDuration && stacksAvailable > 0)
            {
                stopwatch -= stackConsumptionDuration;
                if (NetworkServer.active)
                {
                    characterBody.RemoveBuff(KoaleskBuffs.koaleskLiquorBuff);

                    var stakeProjectileVFX = UnityEngine.Object.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
                    vfxCreated.Add(stakeProjectileVFX);
                }
            }

            if (base.isAuthority && (!IsKeyDownAuthority() || stacksAvailable <= 0))
            {
                outer.SetNextState(new FireBloodyStake {
                    projectilesToGenerate = vfxCreated.Count + 1
                });
            }
        }

        public override void Update()
        {
            base.Update();

            if (vfxCreated != null && vfxCreated.Count > 0)
            {
                var vfxDistributionPoints = Modules.Helpers.DistributePointsEvenlyOnSphereCap(2.5f, vfxCreated.Count, characterBody.corePosition + inputBank.aimDirection, Quaternion.identity);

                foreach (var vfx in vfxCreated.Select((value, i) => new {i, value}))
                {
                    var value = vfx.value;
                    var index = vfx.i;
                    value.transform.position = vfxDistributionPoints[index];
                    value.transform.rotation = RoR2.Util.QuaternionSafeLookRotation(inputBank.aimDirection);
                }
            }
        }

        public override void OnExit()
        {
            foreach (var vfx in vfxCreated)
            {
                if (vfx != null)
                {
                    UnityEngine.Object.Destroy(vfx.gameObject);
                }
            }

            base.OnExit();

            PlayCrossfade("Gesture, Override", "BufferEmpty", 0f);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
