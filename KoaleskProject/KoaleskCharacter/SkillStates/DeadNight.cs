using KoaleskMod.KoaleskCharacter.Content;
using KoaleskMod.Modules.BaseStates;
using System;
using System.Collections.Generic;
using System.Text;

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
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
