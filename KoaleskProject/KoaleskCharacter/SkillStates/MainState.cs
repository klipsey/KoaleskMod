using UnityEngine;
using RoR2;
using EntityStates;
using BepInEx.Configuration;
using KoaleskMod.Modules;
using KoaleskMod.KoaleskCharacter.Components;
using KoaleskMod.KoaleskCharacter.Content;

namespace KoaleskMod.KoaleskCharacter.SkillStates
{
    public class MainState : GenericCharacterMain
    {
        private Animator animator;
        public LocalUser localUser;
        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = this.modelAnimator;
            this.FindLocalUser();
        }
        private void CheckEmote<T>(ConfigEntry<KeyboardShortcut> keybind) where T : EntityState, new()
        {
            if (Modules.Config.GetKeyPressed(keybind))
            {
                FindLocalUser();

                if (localUser != null && !localUser.isUIFocused)
                {
                    outer.SetInterruptState(new T(), InterruptPriority.Any);
                }
            }
        }
        private void FindLocalUser()
        {
            if (this.localUser == null)
            {
                if (base.characterBody)
                {
                    foreach (LocalUser lu in LocalUserManager.readOnlyLocalUsersList)
                    {
                        if (lu.cachedBody == base.characterBody)
                        {
                            this.localUser = lu;
                            break;
                        }
                    }
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this.animator)
            {
                bool cock = false;
                if (!this.characterBody.outOfDanger || !this.characterBody.outOfCombat) cock = true;

                this.animator.SetBool("inCombat", cock);

                if (this.isGrounded) this.animator.SetFloat("airBlend", 0f);
                else this.animator.SetFloat("airBlend", 1f);
            }

            if (base.isAuthority && base.characterMotor.isGrounded)
            {
                this.CheckEmote<PoleDance>(KoaleskConfig.restKey);
            }

            if (inputBank && characterBody && characterBody.skillLocator)
            {
                var darkSkills = characterBody.gameObject.GetComponent<KoaleskDarkSkills>();
                var skillLocator = characterBody.skillLocator;

                if (skillLocator.primary && skillLocator.secondary && skillLocator.utility && skillLocator.special)
                {
                    EntityStateMachine b = null;
                    EntityStateMachine[] components = base.gameObject.GetComponents<EntityStateMachine>();
                    for (int i = 0; i < components.Length; i++)
                    {
                        if (components[i].customName == "Weapon")
                        {
                            b = components[i];

                            break;
                        }
                    }
                    if (inputBank.rawMoveDown.down)
                    {
                        float primaryCd = skillLocator.primary.rechargeStopwatch;
                        int primaryStock = skillLocator.primary.stock;

                        float secondaryCd = skillLocator.secondary.rechargeStopwatch;
                        int secondaryStock = skillLocator.secondary.stock;  

                        float utilityCd = skillLocator.utility.rechargeStopwatch;
                        int utilityStock = skillLocator.utility.stock;

                        float specialCd = skillLocator.special.rechargeStopwatch;
                        int specialStock = skillLocator.special.stock;  

                        skillLocator.primary.SetSkillOverride(characterBody, darkSkills.darkPrimarySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                        skillLocator.secondary.SetSkillOverride(characterBody, darkSkills.darkSecondarySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                        skillLocator.utility.SetSkillOverride(characterBody, darkSkills.darkUtilitySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                        skillLocator.special.SetSkillOverride(characterBody, darkSkills.darkSpecialSkillSlot.skillDef, GenericSkill.SkillOverridePriority.Contextual);

                        skillLocator.primary.rechargeStopwatch = primaryCd;
                        skillLocator.primary.stock = primaryStock;

                        skillLocator.secondary.rechargeStopwatch= secondaryCd;
                        skillLocator.secondary.stock = secondaryStock;

                        skillLocator.utility.rechargeStopwatch = utilityCd;
                        skillLocator.utility.stock = utilityStock;

                        skillLocator.special.rechargeStopwatch = specialCd;
                        skillLocator.special.stock = specialStock;
                    }
                    else
                    {
                        if(b && !(b.state is ChargeBloodyStake) && !(b.state is FireBloodyStake))
                        {
                            float primaryCd = skillLocator.primary.rechargeStopwatch;
                            int primaryStock = skillLocator.primary.stock;

                            float secondaryCd = skillLocator.secondary.rechargeStopwatch;
                            int secondaryStock = skillLocator.secondary.stock;

                            float utilityCd = skillLocator.utility.rechargeStopwatch;
                            int utilityStock = skillLocator.utility.stock;

                            float specialCd = skillLocator.special.rechargeStopwatch;
                            int specialStock = skillLocator.special.stock;

                            skillLocator.primary.UnsetSkillOverride(characterBody, darkSkills.darkPrimarySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                            skillLocator.secondary.UnsetSkillOverride(characterBody, darkSkills.darkSecondarySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                            skillLocator.utility.UnsetSkillOverride(characterBody, darkSkills.darkUtilitySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Contextual);
                            skillLocator.special.UnsetSkillOverride(characterBody, darkSkills.darkSpecialSkillSlot.skillDef, GenericSkill.SkillOverridePriority.Contextual);

                            skillLocator.primary.rechargeStopwatch = primaryCd;
                            skillLocator.primary.stock = primaryStock;

                            skillLocator.secondary.rechargeStopwatch = secondaryCd;
                            skillLocator.secondary.stock = secondaryStock;

                            skillLocator.utility.rechargeStopwatch = utilityCd;
                            skillLocator.utility.stock = utilityStock;

                            skillLocator.special.rechargeStopwatch = specialCd;
                            skillLocator.special.stock = specialStock;
                        }
                    }
                }
            }
        }

        public override void ProcessJump()
        {

            if (this.hasCharacterMotor)
            {
                bool hopooFeather = false;
                bool waxQuail = false;

                if (this.jumpInputReceived && base.characterBody && base.characterMotor.jumpCount < base.characterBody.maxJumpCount)
                {
                    int waxQuailCount = base.characterBody.inventory.GetItemCount(RoR2Content.Items.JumpBoost);
                    float horizontalBonus = 1f;
                    float verticalBonus = 1f;

                    if (base.characterMotor.jumpCount >= base.characterBody.baseJumpCount)
                    {
                        hopooFeather = true;
                        horizontalBonus = 1.5f;
                        verticalBonus = 1.5f;
                    }
                    else if (waxQuailCount > 0 && base.characterBody.isSprinting)
                    {
                        float v = base.characterBody.acceleration * base.characterMotor.airControl;

                        if (base.characterBody.moveSpeed > 0f && v > 0f)
                        {
                            waxQuail = true;
                            float num2 = Mathf.Sqrt(10f * (float)waxQuailCount / v);
                            float num3 = base.characterBody.moveSpeed / v;
                            horizontalBonus = (num2 + num3) / num3;
                        }
                    }

                    GenericCharacterMain.ApplyJumpVelocity(base.characterMotor, base.characterBody, horizontalBonus, verticalBonus, false);

                    if (this.hasModelAnimator)
                    {
                        int layerIndex = base.modelAnimator.GetLayerIndex("Body");
                        if (layerIndex >= 0)
                        {
                            if (this.characterBody.isSprinting)
                            {
                                this.modelAnimator.CrossFadeInFixedTime("SprintJump", this.smoothingParameters.intoJumpTransitionTime, layerIndex);
                            }
                            else
                            {
                                if (hopooFeather)
                                {
                                    this.modelAnimator.CrossFadeInFixedTime("BonusJump", this.smoothingParameters.intoJumpTransitionTime, layerIndex);
                                }
                                else
                                {
                                    this.modelAnimator.CrossFadeInFixedTime("Jump", this.smoothingParameters.intoJumpTransitionTime, layerIndex);
                                }
                            }
                        }
                    }

                    if (hopooFeather)
                    {
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FeatherEffect"), new EffectData
                        {
                            origin = base.characterBody.footPosition
                        }, true);
                    }
                    else if (base.characterMotor.jumpCount > 0)
                    {
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/CharacterLandImpact"), new EffectData
                        {
                            origin = base.characterBody.footPosition,
                            scale = base.characterBody.radius
                        }, true);
                    }

                    if (waxQuail)
                    {
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
                        {
                            origin = base.characterBody.footPosition,
                            rotation = Util.QuaternionSafeLookRotation(base.characterMotor.velocity)
                        }, true);
                    }

                    base.characterMotor.jumpCount++;

                }
            }
        }
    }
}
