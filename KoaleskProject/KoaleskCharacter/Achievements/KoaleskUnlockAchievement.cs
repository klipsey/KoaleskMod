using RoR2.Achievements;
using UnityEngine;
using RoR2;
using R2API;

namespace KoaleskMod.KoaleskCharacter.Achievements
{
    [RegisterAchievement("UnlockKoalesk", "Character.Koalesk", null, 3u, typeof(KoaleskUnlockServerAchievement))]
    public class KoaleskUnlockAchievement : BaseAchievement
    {
        public const string identifier = KoaleskSurvivor.KOALESK_PREFIX + "unlockAchievement";
        public const string unlockableIdentifier = KoaleskSurvivor.KOALESK_PREFIX + "unlockAchievement";

        private class KoaleskUnlockServerAchievement  : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                GlobalEventManager.onServerDamageDealt += UnlockKoalesk;
            }

            private void UnlockKoalesk(DamageReport damageReport)
            {
                Grant();
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
                GlobalEventManager.onServerDamageDealt -= UnlockKoalesk;
            }
        }

        public override void OnInstall()
        {
            base.OnInstall();
            SetServerTracked(shouldTrack: true);
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
        }
    }
}
