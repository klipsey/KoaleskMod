using KoaleskMod.KoaleskCharacter.Content;
using KoaleskMod.Modules.BaseStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace KoaleskMod.KoaleskCharacter.SkillStates
{
    public class ScarletGarden : BaseKoaleskSkillState
    {
        private float baseDuration = 5f;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.ClearTimedBuffs(KoaleskBuffs.koaleskGardenBuff);

            float buffCount = characterBody.GetBuffCount(KoaleskBuffs.koaleskLiquorBuff);

            koaleskController.ConsumeBloodLiquor();

            duration = baseDuration + buffCount;

            for(int i = 0; i < buffCount; i++)
            {
                characterBody.AddTimedBuff(KoaleskBuffs.koaleskGardenBuff, duration);
            }

            if (base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
