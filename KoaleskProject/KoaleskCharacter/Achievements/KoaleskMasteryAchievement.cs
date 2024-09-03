using RoR2;
using KoaleskMod.Modules.Achievements;
using KoaleskMod.KoaleskCharacter;

namespace KoaleskMod.KoaleskCharacter.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, 0)]
    public class KoaleskMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = KoaleskSurvivor.KOALESK_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = KoaleskSurvivor.KOALESK_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => KoaleskSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}