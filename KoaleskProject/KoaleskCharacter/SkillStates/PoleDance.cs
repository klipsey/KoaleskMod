using System;
using System.Collections.Generic;
using System.Text;

namespace KoaleskMod.KoaleskCharacter.SkillStates
{
    public class PoleDance : BaseEmote
    {
        public override void OnEnter()
        {
            base.OnEnter();

            this.PlayEmote("PoleDance", "", 1.5f);
        }
    }
}
