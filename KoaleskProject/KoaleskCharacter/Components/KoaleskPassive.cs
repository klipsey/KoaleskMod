using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KoaleskMod.KoaleskCharacter.Components
{
    public class KoaleskPassive : MonoBehaviour
    {
        public SkillDef koaleskPassive;
        public SkillDef koaleskGoodPassive;

        public GenericSkill passiveSkillSlot;

        public bool isMid
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

        public bool isNice
        {
            get
            {
                if (koaleskGoodPassive && passiveSkillSlot)
                {
                    return passiveSkillSlot.skillDef == koaleskGoodPassive;
                }

                return false;
            }
        }
    }
}