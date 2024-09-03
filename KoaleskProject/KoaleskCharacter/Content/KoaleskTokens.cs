using System;
using KoaleskMod.Modules;
using KoaleskMod.KoaleskCharacter;
using KoaleskMod.KoaleskCharacter.Achievements;
using UnityEngine.UIElements;

namespace KoaleskMod.KoaleskCharacter.Content
{
    public static class KoaleskTokens
    {
        public static void Init()
        {
            AddKoaleskTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Spy.txt");
            //todo guide
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddKoaleskTokens()
        {
            #region Koalesk
            string prefix = KoaleskSurvivor.KOALESK_PREFIX;

            string desc = "Koalesk relishes the pain of others. Don't have too much fun hurting your allies, or do...<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Punish the Guilty after they hit you to gain attack speed and move speed. No running from justice." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > If you need a quick and dirty Guilty buff, swing and hit yourself instead. The law applies to everyone!" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Falsify is a great way to spot the Guilty before they commit crimes. Unethical? What do you mean?" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Convict a Guilty target to make sure they are punished for their acts. Guilty until proven innocent after all." + Environment.NewLine + Environment.NewLine;

            string lore = "Insert goodguy lore here";
            string outro = "...and so she left, no longer split.";
            string outroFailure = "..and so she remained, enshrouded in petal and gloom, her dual forces stilled by fate's unyielding hand.";
            
            Language.Add(prefix + "NAME", "Koalesk");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "Unhinged Tormentor");
            Language.Add(prefix + "LORE", lore);
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Torment");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", $"<color=#FFBF66>Koalesk</color> can hit and be hit by both allies and enemies. " +
                $"Attackers that have hit <color=#FFBF66>Koalesk</color> are permanently marked as <color=#FFBF66>Guilty</color> granting <style=cIsDamage>attack speed</style> and <style=cIsDamage>damage</style> to Koalesk until they die (Once per target).");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_ROSETHORN_NAME", "Rose Thorn");
            Language.Add(prefix + "PRIMARY_ROSETHORN_DESCRIPTION", $".");

            Language.Add(prefix + "PRIMARY_DARKTHORN_NAME", "Dark Thorn");
            Language.Add(prefix + "PRIMARY_DARKTHORN_DESCRIPTION", $".");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_BLOODYSTAKE_NAME", "Bloody Stake");
            Language.Add(prefix + "SECONDARY_BLOODYSTAKE_DESCRIPTION", $".");

            Language.Add(prefix + "SECONDARY_GRAVESTAKE_NAME", "Grave Stake");
            Language.Add(prefix + "SECONDARY_GRAVESTAKE_DESCRIPTION", $".");
            #endregion

            #region Utility 
            Language.Add(prefix + "UTILITY_FLOWERDANCE_NAME", "Flower Dance");
            Language.Add(prefix + "UTILITY_FLOWERDANCE_DESCRIPTION", $".");

            Language.Add(prefix + "UTILITY_SHADOWDANCE_NAME", "Shadow Dance");
            Language.Add(prefix + "UTILITY_SHADOWDANCE_DESCRIPTION", $".");

            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_SCARLETGARDEN_NAME", "Scarlet Garden");
            Language.Add(prefix + "SPECIAL_SCARLETGARDEN_DESCRIPTION", $".");

            Language.Add(prefix + "SPECIAL_DEADNIGHT_NAME", "Dead of Night");
            Language.Add(prefix + "SPECIAL_DEADNIGHT_DESCRIPTION", $".");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(KoaleskMasteryAchievement.identifier), "Koalesk: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(KoaleskMasteryAchievement.identifier), "As Koalesk, beat the game or obliterate on Monsoon.");
            
            Language.Add(Tokens.GetAchievementNameToken(KoaleskUnlockAchievement.identifier), "");
            Language.Add(Tokens.GetAchievementDescriptionToken(KoaleskUnlockAchievement.identifier), ".");
            
            #endregion

            #endregion
        }
    }
}