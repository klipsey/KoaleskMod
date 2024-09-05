using RoR2.Projectile;
using RoR2;
using RoR2.HudOverlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using KoaleskMod.KoaleskCharacter.Content;
using System;
using TMPro;

namespace KoaleskMod.KoaleskCharacter.Components
{
    public class KoaleskController : NetworkBehaviour
    {
        private GameObject blightProjectilePrefab = KoaleskAssets.KoaleskBlightProjectilePrefab;
        private CharacterBody characterBody;
        private ModelSkinController skinController;
        private ChildLocator childLocator;
        private CharacterModel characterModel;
        private Animator animator;
        private SkillLocator skillLocator;

        private float liquorDecayTimer;
        private float blightDecayTimer;

        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelBaseTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelBaseTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            this.skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();
        }
        private void Start()
        {
        }
        private void FixedUpdate()
        {
            if(characterBody.HasBuff(KoaleskBuffs.koaleskLiquorBuff))
            {
                liquorDecayTimer -= Time.fixedDeltaTime;

                if(liquorDecayTimer <= 0) 
                {
                    if (NetworkServer.active)
                    {
                        characterBody.RemoveBuff(KoaleskBuffs.koaleskLiquorBuff);
                        HealingPulse healingPulse = new HealingPulse();
                        healingPulse.healAmount = characterBody.maxHealth * 0.02f;
                        healingPulse.origin = characterBody.corePosition;
                        healingPulse.radius = 6f;
                        healingPulse.effectPrefab = KoaleskAssets.KoaleskHealEffect;
                        healingPulse.teamIndex = characterBody.teamComponent.teamIndex;
                        healingPulse.overShield = 0f;
                        healingPulse.Fire();
                    }
                    liquorDecayTimer = KoaleskConfig.buffIntervalDecayDelay.Value;
                }
            }

            if(characterBody.HasBuff(KoaleskBuffs.koaleskBlightBuff))
            {
                blightDecayTimer -= Time.fixedDeltaTime;

                if(blightDecayTimer <= 0)
                {
                    if (NetworkServer.active) characterBody.RemoveBuff(KoaleskBuffs.koaleskBlightBuff);

                    if(hasAuthority)
                    {
                        ProjectileManager.instance.FireProjectile(blightProjectilePrefab, characterBody.footPosition, Quaternion.identity, null, 0f, 0f, false, DamageColorIndex.Default, null);
                    }

                    blightDecayTimer = KoaleskConfig.buffIntervalDecayDelay.Value;
                }
            }
        }
        public void AddBuffResetDecay(BuffDef buff)
        {
            if(NetworkServer.active)
            {
                characterBody.AddBuff(buff);

                if (buff.buffIndex == KoaleskBuffs.koaleskLiquorBuff.buffIndex)
                {
                    liquorDecayTimer = KoaleskConfig.buffDecayDelay.Value;
                }

                if (buff.buffIndex == KoaleskBuffs.koaleskBlightBuff.buffIndex)
                {
                    blightDecayTimer = KoaleskConfig.buffDecayDelay.Value;
                }
            }
        }
        public void ConsumeBloodLiquor()
        {
            if(NetworkServer.active)
            {
                characterBody.SetBuffCount(KoaleskBuffs.koaleskLiquorBuff.buffIndex, 0);
                liquorDecayTimer = KoaleskConfig.buffDecayDelay.Value;
            }
        }
        public void ConsumeBlight()
        {
            if(NetworkServer.active)
            {
                characterBody.SetBuffCount(KoaleskBuffs.koaleskBlightBuff.buffIndex, 0);
                blightDecayTimer = KoaleskConfig.buffDecayDelay.Value;
            }
        }
        private void OnDestroy()
        {
        }
    }
}
