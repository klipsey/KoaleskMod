using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace KoaleskMod.KoaleskCharacter.Components
{
    public class KoaleskDarkSkills : MonoBehaviour
    {
        public SkillDef DarkThorn;
        public GenericSkill darkPrimarySkillSlot;

        public SkillDef GraveStake;
        public GenericSkill darkSecondarySkillSlot;

        public SkillDef ShadowDance;
        public GenericSkill darkUtilitySkillSlot;

        public SkillDef DeadNight;
        public GenericSkill darkSpecialSkillSlot;

        public bool isDarkThorn
        {
            get
            {
                if (DarkThorn && darkPrimarySkillSlot)
                {
                    return darkPrimarySkillSlot.skillDef == DarkThorn;
                }

                return false;
            }
        }
        public bool isGraveStake
        {
            get
            {
                if (GraveStake && darkSecondarySkillSlot)
                {
                    return darkSecondarySkillSlot.skillDef == GraveStake;
                }

                return false;
            }
        }
        public bool isShadowDance
        {
            get
            {
                if (ShadowDance && darkUtilitySkillSlot)
                {
                    return darkUtilitySkillSlot.skillDef == ShadowDance;
                }

                return false;
            }
        }
        public bool isNight
        {
            get
            {
                if (DeadNight && darkSpecialSkillSlot)
                {
                    return darkSpecialSkillSlot.skillDef == DeadNight;
                }

                return false;
            }
        }
    }
}