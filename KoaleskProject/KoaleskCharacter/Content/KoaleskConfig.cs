using BepInEx.Configuration;
using KoaleskMod.Modules;

namespace KoaleskMod.KoaleskCharacter.Content
{
    public static class KoaleskConfig
    {
        public static ConfigEntry<bool> forceUnlock;
        public static ConfigEntry<float> buffDecayDelay;
        public static ConfigEntry<float> buffIntervalDecayDelay;
        public static ConfigEntry<float> swingDamageCoefficient;
        public static ConfigEntry<float> swingLargeDamageCoefficient;
        public static ConfigEntry<float> darkThornDamageCoefficient;
        public static ConfigEntry<float> shoulderBashDamageCoefficient;
        public static ConfigEntry<float> chargeDamageCoefficient;
        public static ConfigEntry<float> swordSlamDamageCoefficient;
        public static void Init()
        {
            string section = "01 - General";
            string section2 = "02 - Stats";

            //add more here or else you're cringe
            forceUnlock = Config.BindAndOptions(
                section,
                "Unlock Koalesk",
                false,

                "Unlock Koalesk.", true);

            buffDecayDelay = Config.BindAndOptions(section2, "Decay Timer", 2.5f, "Change the time it takes to decay stacks of blood liquor or dark blight.", false);

            buffIntervalDecayDelay = Config.BindAndOptions(section2, "Interval Decay Timer", 2.5f, "Change the time it takes between decaying stacks of blood liquor or dark blight.", false);

            swingDamageCoefficient = Config.BindAndOptions(section2, "Rose Thorn Swing Damage", 2.8f, "Change the damage coefficient of Rose Thorns first two swings.", false);

            swingLargeDamageCoefficient = Config.BindAndOptions(section2, "Rose Thorn Swing3 Damage", 3.6f, "Change the damage coefficient of Rose Thorns final swing.", false);

            darkThornDamageCoefficient = Config.BindAndOptions(section2, "Dark Thorn Damage", 1.6f, "Change the damage coefficient of Dark Thorn.", false);

            shoulderBashDamageCoefficient = Config.BindAndOptions(section2, "Shoulder Bash Damage", 2.4f, "Change the damage coefficient of Shoulder Bash.", false);

            chargeDamageCoefficient = Config.BindAndOptions(section2, "Charge Damage", 7f, "Change the damage coefficient of Shoulder Bash.", false);
        }
    }
}
