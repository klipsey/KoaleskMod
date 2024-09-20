using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace KoaleskMod.KoaleskCharacter.Content
{
    public static class KoaleskBuffs
    {
        public static BuffDef koaleskLiquorBuff;
        public static BuffDef koaleskBlightBuff;
        public static BuffDef koaleskGardenBuff;
        public static BuffDef koaleskDeadOfNightBuff;
        public static void Init(AssetBundle assetBundle)
        {
            koaleskLiquorBuff = Modules.Content.CreateAndAddBuff("KoaleskLiquorBuff", assetBundle.LoadAsset<Sprite>("IconBloodliquor"),
                Color.white, true, false, false);
            koaleskBlightBuff = Modules.Content.CreateAndAddBuff("KoaleskBlightBuff", assetBundle.LoadAsset<Sprite>("IconDarkblight"),
                Color.white, true, false, false);
            koaleskGardenBuff = Modules.Content.CreateAndAddBuff("KoaleskGardenBuff", assetBundle.LoadAsset<Sprite>("IconBloodliquor"),
                Color.red, true, false, false);
            koaleskDeadOfNightBuff = Modules.Content.CreateAndAddBuff("KoaleskDeadOfNightBuff", assetBundle.LoadAsset<Sprite>("IconBloodliquor"),
                Color.black, false, true, false);
        }
    }
}
