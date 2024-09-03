using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KoaleskMod.KoaleskCharacter.Components
{
    public class KoaleskPassive : MonoBehaviour
    {
        public SkillDef koaleskPassive;

        public GenericSkill passiveSkillSlot;

        public bool isJump
        {
            get
            {
                if (koaleskPassive && passiveSkillSlot)
                {
                    return passiveSkillSlot.skillDef == koaleskPassive;
                }

                return false;
            }
        }
    }
}