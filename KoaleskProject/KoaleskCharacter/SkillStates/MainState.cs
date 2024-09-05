using UnityEngine;
using RoR2;
using EntityStates;
using BepInEx.Configuration;
using KoaleskMod.Modules;
using KoaleskMod.KoaleskCharacter.Components;

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

            if (inputBank && characterBody && characterBody.skillLocator)
            {
                var darkSkills = characterBody.gameObject.GetComponent<KoaleskDarkSkills>();
                var skillLocator = characterBody.skillLocator;
                var movementInputDot = Vector3.Dot(inputBank.moveVector.normalized, aimDirection.normalized);

                // Define a small threshold value, like 0.1
                float threshold = 0.1f;

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
                    if (movementInputDot >= -threshold)
                    {
                        skillLocator.primary.UnsetSkillOverride(characterBody, darkSkills.darkPrimarySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                        skillLocator.secondary.UnsetSkillOverride(characterBody, darkSkills.darkSecondarySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                        skillLocator.utility.UnsetSkillOverride(characterBody, darkSkills.darkUtilitySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                        skillLocator.special.UnsetSkillOverride(characterBody, darkSkills.darkSpecialSkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                    }
                    else
                    {
                        if(b && !(b.state is ChargeBloodyStake) && !(b.state is FireBloodyStake))
                        {
                            skillLocator.primary.SetSkillOverride(characterBody, darkSkills.darkPrimarySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                            skillLocator.secondary.SetSkillOverride(characterBody, darkSkills.darkSecondarySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                            skillLocator.utility.SetSkillOverride(characterBody, darkSkills.darkUtilitySkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
                            skillLocator.special.SetSkillOverride(characterBody, darkSkills.darkSpecialSkillSlot.skillDef, GenericSkill.SkillOverridePriority.Replacement);
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
