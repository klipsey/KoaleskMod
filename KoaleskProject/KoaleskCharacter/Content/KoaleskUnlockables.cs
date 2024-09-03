using RoR2;
using UnityEngine;
using KoaleskMod.KoaleskCharacter;
using KoaleskMod.KoaleskCharacter.Achievements;

namespace KoaleskMod.KoaleskCharacter.Content
{
    public static class KoaleskUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockableDef(KoaleskMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(KoaleskMasteryAchievement.unlockableIdentifier),
                KoaleskSurvivor.instance.assetBundle.LoadAsset<Sprite>("texKoaleskMonsoonSkin"));

            /*
            characterUnlockableDef = Modules.Content.CreateAndAddUnlockableDef(KoaleskUnlockAchievement.unlockableIdentifier,
            Modules.Tokens.GetAchievementNameToken(KoaleskUnlockAchievement.unlockableIdentifier),
            KoaleskSurvivor.instance.assetBundle.LoadAsset<Sprite>("texKoaleskIcon"));
            */
        }
    }
}
