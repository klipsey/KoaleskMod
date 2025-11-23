using BepInEx.Configuration;
using KoaleskMod.Modules;
using KoaleskMod.Modules.Characters;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using RoR2.UI;
using R2API;
using R2API.Networking;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using KoaleskMod.KoaleskCharacter.Components;
using KoaleskMod.KoaleskCharacter.Content;
using KoaleskMod.KoaleskCharacter.SkillStates;
using HG;
using EntityStates;
using R2API.Networking.Interfaces;
using EmotesAPI;
using System.Runtime.CompilerServices;

namespace KoaleskMod.KoaleskCharacter
{
    public class KoaleskSurvivor : SurvivorBase<KoaleskSurvivor>
    {
        public override string assetBundleName => "koalesk";
        public override string bodyName => "KoaleskBody";
        public override string masterName => "KoaleskMonsterMaster";
        public override string modelPrefabName => "mdlKoalesk";
        public override string displayPrefabName => "KoaleskDisplay";

        public const string KOALESK_PREFIX = KoaleskPlugin.DEVELOPER_PREFIX + "_KOALESK_";
        public override string survivorTokenPrefix => KOALESK_PREFIX;

        internal static GameObject characterPrefab;

        public static SkillDef convictScepterSkillDef;

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = KOALESK_PREFIX + "NAME",
            subtitleNameToken = KOALESK_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("KoaleskIcon"),
            bodyColor = KoaleskAssets.koaleskColor,
            sortPosition = 6f,

            crosshair = Modules.CharacterAssets.LoadCrosshair("Standard"),
            podPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 120f,
            healthRegen = 1f,
            armor = 10f,

            jumpCount = 1,
            autoCalculateLevelStats = true,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "Model",
                },
                new CustomRendererInfo
                {
                    childName = "BoneModel",
                },
                new CustomRendererInfo
                {
                    childName = "HandModel",
                },
                new CustomRendererInfo
                {
                    childName = "RoseModel",
                },
                new CustomRendererInfo
                {
                    childName = "SkirtModel",
                },
                new CustomRendererInfo
                {
                    childName = "TrimModel"
                }
        };

        public override UnlockableDef characterUnlockableDef => KoaleskUnlockables.characterUnlockableDef;

        public override ItemDisplaysBase itemDisplays => new KoaleskItemDisplays();
        public override AssetBundle assetBundle { get; protected set; }
        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }
        public override void Initialize()
        {

            //uncomment if you have multiple characters
            //ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Henry");

            //if (!characterEnabled.Value)
            //    return;

            //need the character unlockable before you initialize the survivordef

            base.Initialize();
        }

        public override void InitializeCharacter()
        {
            KoaleskConfig.Init();

            KoaleskUnlockables.Init();

            base.InitializeCharacter();

            CameraParams.InitializeParams();

            DamageTypes.Init();

            KoaleskStates.Init();
            KoaleskTokens.Init();

            KoaleskBuffs.Init(assetBundle);
            KoaleskAssets.Init(assetBundle);

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();

            characterPrefab = bodyPrefab;

            AddHooks();
        }

        private void AdditionalBodySetup()
        {
            bool buffRequired(CharacterBody body) => body.HasBuff(KoaleskBuffs.koaleskDeadOfNightBuff);
            float radius(CharacterBody body) => body.radius;
            TempVisualEffectAPI.AddTemporaryVisualEffect(KoaleskAssets.heldEffect, radius, buffRequired);

            AddHitboxes();
            bodyPrefab.AddComponent<KoaleskController>();
        }
        public void AddHitboxes()
        {
            Prefabs.SetupHitBoxGroup(characterModelObject, "MeleeHitbox", "MeleeHitbox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "BigMeleeHitbox", "BigMeleeHitbox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "DarkThornHitbox", "DarkThornHitbox");
        }

        public override void InitializeEntityStateMachines()
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(SkillStates.MainState), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your HenryStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Dash");
        }

        #region skills
        public override void InitializeSkills()
        {
            bodyPrefab.AddComponent<KoaleskPassive>();
            bodyPrefab.AddComponent<KoaleskDarkSkills>();
            Skills.CreateSkillFamilies(bodyPrefab);
            AddPassiveSkills();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtilitySkills();
            AddSpecialSkills();
        }

        private void AddPassiveSkills()
        {
            KoaleskPassive passive = bodyPrefab.GetComponent<KoaleskPassive>();

            SkillLocator skillLocator = bodyPrefab.GetComponent<SkillLocator>();

            skillLocator.passiveSkill.enabled = false;

            passive.koaleskPassive = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = KOALESK_PREFIX + "PASSIVE_NAME",
                skillNameToken = KOALESK_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = KOALESK_PREFIX + "PASSIVE_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texKoaleskPassiveIcon"),
                keywordTokens = new string[] {},
                activationState = new EntityStates.SerializableEntityStateType(typeof(Idle)),
                activationStateMachineName = "",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Any,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 2,
                stockToConsume = 1
            });

            passive.koaleskGoodPassive = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "KoaleskGoodPassive",
                skillNameToken = KOALESK_PREFIX + "PASSIVE2_NAME",
                skillDescriptionToken = KOALESK_PREFIX + "PASSIVE2_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texKoaleskPassiveIcon"),
                keywordTokens = new string[] { },
                activationState = new EntityStates.SerializableEntityStateType(typeof(Idle)),
                activationStateMachineName = "",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Any,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 2,
                stockToConsume = 1
            });

            Skills.AddAdditionalSkills(passive.passiveSkillSlot.skillFamily, passive.koaleskPassive, passive.koaleskGoodPassive);
        }

        private void AddPrimarySkills()
        {
            KoaleskDarkSkills darkSkills = bodyPrefab.GetComponent<KoaleskDarkSkills>();

            SteppedSkillDef swordSwing = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "KoaleskRoseThorn",
                    KOALESK_PREFIX + "PRIMARY_ROSETHORN_NAME",
                    KOALESK_PREFIX + "PRIMARY_ROSETHORN_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texRoseThorn"),
                    new SerializableEntityStateType(typeof(RoseThorn)),
                    "Weapon"
                ));
            swordSwing.stepCount = 3;
            swordSwing.stepGraceDuration = 0.5f;
            swordSwing.keywordTokens = new string[]{ };

            Skills.AddPrimarySkills(bodyPrefab, swordSwing);

            SteppedSkillDef clawSwing = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
            (
                "KoaleskDarkThorn",
                KOALESK_PREFIX + "PRIMARY_DARKTHORN_NAME",
                KOALESK_PREFIX + "PRIMARY_DARKTHORN_DESCRIPTION",
                assetBundle.LoadAsset<Sprite>("texDarkThorn"),
                new SerializableEntityStateType(typeof(DarkThorn)),
                "Weapon"
            ));
            clawSwing.stepCount = 1;
            clawSwing.stepGraceDuration = 0.01f;
            clawSwing.keywordTokens = new string[] { };

            Skills.AddAdditionalSkills(darkSkills.darkPrimarySkillSlot.skillFamily, clawSwing);
        }

        private void AddSecondarySkills()
        {
            KoaleskDarkSkills darkSkills = bodyPrefab.GetComponent<KoaleskDarkSkills>();

            SkillDef bloodyStake = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "KoaleskBloodyStake",
                skillNameToken = KOALESK_PREFIX + "SECONDARY_BLOODYSTAKE_NAME",
                skillDescriptionToken = KOALESK_PREFIX + "SECONDARY_BLOODYSTAKE_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texBloodyStake"),

                activationState = new SerializableEntityStateType(typeof(ChargeBloodyStake)),

                activationStateMachineName = "Weapon",
                interruptPriority = InterruptPriority.Skill,

                baseMaxStock = 1,
                baseRechargeInterval = 5f,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                beginSkillCooldownOnSkillEnd = true,
                mustKeyPress = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,
            });

            Skills.AddAdditionalSkills(darkSkills.darkSecondarySkillSlot.skillFamily, bloodyStake);

            SkillDef graveStake = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "KoaleskGraveStake",
                skillNameToken = KOALESK_PREFIX + "SECONDARY_GRAVESTAKE_NAME",
                skillDescriptionToken = KOALESK_PREFIX + "SECONDARY_GRAVESTAKE_DESCRIPTION",
                keywordTokens = new string[] { Tokens.koaleskToughKeyword },
                skillIcon = assetBundle.LoadAsset<Sprite>("texGraveStake"),

                activationState = new SerializableEntityStateType(typeof(GraveStake)),

                activationStateMachineName = "Weapon",
                interruptPriority = InterruptPriority.Skill,

                baseMaxStock = 1,
                baseRechargeInterval = 5f,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                beginSkillCooldownOnSkillEnd = false,
                mustKeyPress = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
            });

            Skills.AddSecondarySkills(bodyPrefab, graveStake);

        }

        private void AddUtilitySkills()
        {
            KoaleskDarkSkills darkSkills = bodyPrefab.GetComponent<KoaleskDarkSkills>();

            SkillDef flowerDance = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "KoaleskFlowerDance",
                skillNameToken = KOALESK_PREFIX + "UTILITY_FLOWERDANCE_NAME",
                skillDescriptionToken = KOALESK_PREFIX + "UTILITY_FLOWERDANCE_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texFlowerDance"),

                activationState = new SerializableEntityStateType(typeof(FlowerDance)),
                activationStateMachineName = "Dash",
                interruptPriority = InterruptPriority.Skill,

                baseRechargeInterval = 6f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = true,
                cancelSprintingOnActivation = true,
                forceSprintDuringState = false,

            });

            Skills.AddUtilitySkills(bodyPrefab, flowerDance);

            SkillDef shadowDance = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "KoaleskShadowDance",
                skillNameToken = KOALESK_PREFIX + "UTILITY_SHADOWDANCE_NAME",
                skillDescriptionToken = KOALESK_PREFIX + "UTILITY_SHADOWDANCE_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texShadowDance"),

                activationState = new SerializableEntityStateType(typeof(ShadowDance)),
                activationStateMachineName = "Dash",
                interruptPriority = InterruptPriority.Skill,

                baseRechargeInterval = 6f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,

            });

            Skills.AddAdditionalSkills(darkSkills.darkUtilitySkillSlot.skillFamily, shadowDance);
        }

        private void AddSpecialSkills()
        {
            KoaleskDarkSkills darkSkills = bodyPrefab.GetComponent<KoaleskDarkSkills>();

            SkillDef scarletGarden = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "KoaleskScarletGarden",
                skillNameToken = KOALESK_PREFIX + "SPECIAL_SCARLETGARDEN_NAME",
                skillDescriptionToken = KOALESK_PREFIX + "SPECIAL_SCARLETGARDEN_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texScarletGarden"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(ScarletGarden)),
                activationStateMachineName = "Weapon",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 8f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
            });

            Skills.AddSpecialSkills(bodyPrefab, scarletGarden);

            SkillDef deadNight = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "KoaleskDeadNight",
                skillNameToken = KOALESK_PREFIX + "SPECIAL_DEADNIGHT_NAME",
                skillDescriptionToken = KOALESK_PREFIX + "SPECIAL_DEADNIGHT_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texDeadOfNight"),

                activationState = new SerializableEntityStateType(typeof(DeadNight)),
                activationStateMachineName = "Weapon",
                interruptPriority = InterruptPriority.Skill,

                baseRechargeInterval = 12f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,

            });

            Skills.AddAdditionalSkills(darkSkills.darkSpecialSkillSlot.skillFamily, deadNight);
        }
        #endregion skills

        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texDefaultSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
            //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
            //uncomment this when you have another skin
            defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
                "KoaleskBody",
                "KoaleskBone",
                "KoaleskDark",
                "KoaleskRose",
                "KoaleskCloth",
                "KoaleskMetal");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            /*
            defaultSkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChildGameObject("Tie"),
                    shouldActivate = true,
                }
            };
            */
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            /*
            #region MasterySkin

            ////creating a new skindef as we did before
            SkinDef masterySkin = Modules.Skins.CreateSkinDef(INTERROGATOR_PREFIX + "MASTERY_SKIN_NAME",
                assetBundle.LoadAsset<Sprite>("texMonsoonSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject,
                KoaleskUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
                "meshSpyAlt",
                "meshRevolverAlt",//no gun mesh replacement. use same gun mesh
                "meshKnifeAlt",
                "meshWatchAlt",
                null,
                "meshVisorAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            masterySkin.rendererInfos[0].defaultMaterial = KoaleskAssets.spyMonsoonMat;
            masterySkin.rendererInfos[1].defaultMaterial = KoaleskAssets.spyMonsoonMat;
            masterySkin.rendererInfos[2].defaultMaterial = KoaleskAssets.spyMonsoonMat;
            masterySkin.rendererInfos[3].defaultMaterial = KoaleskAssets.spyMonsoonMat;
            masterySkin.rendererInfos[5].defaultMaterial = KoaleskAssets.spyVisorMonsoonMat;

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChildGameObject("Tie"),
                    shouldActivate = false,
                }
            };
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            skins.Add(masterySkin);

            #endregion
            */
            skinController.skins = skins.ToArray();
        }
        #endregion skins


        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");
                
            //how to set up AI in code
            KoaleskAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }

        private void AddHooks()
        {
            On.RoR2.UI.LoadoutPanelController.Rebuild += LoadoutPanelController_Rebuild;
            On.RoR2.HealthComponent.TakeDamageProcess += new On.RoR2.HealthComponent.hook_TakeDamageProcess(HealthComponent_TakeDamageProcess);
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            if(KoaleskPlugin.emotesInstalled) Emotes();
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.bodyIndex == BodyCatalog.FindBodyIndex("KoaleskBody"))
            {
                KoaleskPassive koaleskPassive = sender.GetComponent<KoaleskPassive>();
                if (koaleskPassive != null && koaleskPassive.isNice)
                {
                    if(sender.HasBuff(KoaleskBuffs.koaleskBlightBuff))
                    {
                        args.armorAdd -= 1f * sender.GetBuffCount(KoaleskBuffs.koaleskBlightBuff);
                    }
                    if(sender.HasBuff(KoaleskBuffs.koaleskLiquorBuff))
                    {
                        args.regenMultAdd += 0.1f * sender.GetBuffCount(KoaleskBuffs.koaleskLiquorBuff);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = KoaleskAssets.mainAssetBundle.LoadAsset<GameObject>("koalesk_emoteskeleton");
                CustomEmotesAPI.ImportArmature(KoaleskSurvivor.characterPrefab, skele);
            };
        }


        private static void LoadoutPanelController_Rebuild(On.RoR2.UI.LoadoutPanelController.orig_Rebuild orig, LoadoutPanelController self)
        {
            int index = 0;

            orig(self);

            if (self.currentDisplayData.bodyIndex == BodyCatalog.FindBodyIndex("KoaleskBody"))
            {
                foreach (LanguageTextMeshController i in self.gameObject.GetComponentsInChildren<LanguageTextMeshController>())
                {
                    if (i && i.token == "LOADOUT_SKILL_MISC")
                    {
                        switch(index)
                        {
                            case 0:
                                index++;
                                i.token = "Passive";
                                break;
                            case 1:
                                index++;
                                i.token = "Blight Primary";
                                break;
                            case 2:
                                index++;
                                i.token = "Blight Secondary";
                                break;
                            case 3:
                                index++;
                                i.token = "Blight Utility";
                                break;
                            case 4:
                                index++;
                                i.token = "Blight Special";
                                break;
                        }
                    }
                }
            }
        }
        private void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (NetworkServer.active && self.alive || !self.godMode || self.ospTimer <= 0f)
            {
                CharacterBody victimBody = self.body;
                CharacterBody attackerBody = null;

                if (damageInfo.attacker)
                {
                    attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                }

                if (!damageInfo.rejected && victimBody)
                {
                    if (damageInfo.HasModdedDamageType(DamageTypes.KoaleskBlightProjectileDamage))
                    {
                        damageInfo.damage = self.fullCombinedHealth * 0.02f;
                    }
                }
            }

            orig.Invoke(self, damageInfo);
        }
    }
}