using RoR2;
using UnityEngine;
using KoaleskMod.Modules;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using R2API;
using UnityEngine.Rendering.PostProcessing;
using ThreeEyedGames;
using KoaleskMod.KoaleskCharacter.Components;
using System.IO;
using System.Reflection;
using static R2API.DamageAPI;

namespace KoaleskMod.KoaleskCharacter.Content
{
    public static class KoaleskAssets
    {
        //AssetBundle
        internal static AssetBundle mainAssetBundle;

        //Materials
        internal static Material commandoMat;

        //Shader
        internal static Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/HGStandard");

        //Effects
        internal static GameObject swordSwingEffect;
        internal static GameObject swordBigSwingEffect;
        internal static GameObject swordHitEffect;

        internal static GameObject dashEffect;

        internal static GameObject KoaleskHealEffect;
        internal static GameObject specialSlashingEffect;
        //Models
        //Projectiles
        internal static GameObject KoaleskBlightProjectilePrefab;

        internal static GameObject KoaleskDarkThornProjectile;

        internal static GameObject KoaleskBloodyStakeProjectile;
        internal static GameObject KoaleskBloodyStakeGhost;

        internal static GameObject KoaleskGardenFlower;

        //Sounds
        internal static NetworkSoundEventDef swordImpactSoundEvent;

        //Colors
        internal static Color koaleskColor = new Color(212f / 255f, 59f / 255f, 72f / 255f);
        internal static Color koaleskSecondaryColor = new Color(70f / 255f, 63f / 255f, 94f / 255f);

        //Crosshair
        public static void Init(AssetBundle assetBundle)
        {
            mainAssetBundle = assetBundle;

            CreateMaterials();

            CreateModels();

            CreateEffects();

            CreateSounds();

            CreateProjectiles();

            CreateUI();
        }

        private static void CleanChildren(Transform startingTrans)
        {
            for (int num = startingTrans.childCount - 1; num >= 0; num--)
            {
                if (startingTrans.GetChild(num).childCount > 0)
                {
                    CleanChildren(startingTrans.GetChild(num));
                }
                Object.DestroyImmediate(startingTrans.GetChild(num).gameObject);
            }
        }

        private static void CreateMaterials()
        {

        }

        private static void CreateModels()
        {

        }
        #region effects
        private static void CreateEffects()
        {
            KoaleskHealEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteEarth/AffixEarthHealExplosion.prefab").WaitForCompletion().InstantiateClone("KoaleskLiquorHeal");

            Modules.Content.CreateAndAddEffectDef(KoaleskHealEffect);

            specialSlashingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/EvisOverlapProjectileGhost.prefab").WaitForCompletion().InstantiateClone("UnforgivenSpecialSlash");
            if (!specialSlashingEffect.GetComponent<NetworkIdentity>()) specialSlashingEffect.AddComponent<NetworkIdentity>();
            Component.Destroy(specialSlashingEffect.GetComponent<ProjectileGhostController>());

            Object.Destroy(specialSlashingEffect.transform.Find("Point Light").gameObject);

            EffectComponent ec2 = specialSlashingEffect.AddComponent<EffectComponent>();
            ec2.applyScale = true;

            Modules.Content.CreateAndAddEffectDef(specialSlashingEffect);

            dashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherDashEffect.prefab").WaitForCompletion().InstantiateClone("KoaleskDashEffect");
            dashEffect.AddComponent<NetworkIdentity>();
            Object.Destroy(dashEffect.transform.Find("Point light").gameObject);
            Object.Destroy(dashEffect.transform.Find("Flash, White").gameObject);
            Object.Destroy(dashEffect.transform.Find("NoiseTrails").gameObject);
            dashEffect.transform.Find("Donut").localScale *= 0.5f;
            dashEffect.transform.Find("Donut, Distortion").localScale *= 0.5f;
            dashEffect.transform.Find("Dash").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampDefault.png").WaitForCompletion());
            dashEffect.transform.Find("Dash").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", koaleskColor);
            Modules.Content.CreateAndAddEffectDef(dashEffect);

            swordHitEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/HitsparkBandit.prefab").WaitForCompletion().InstantiateClone("HitEffect");
            swordHitEffect.AddComponent<NetworkIdentity>();
            Modules.Content.CreateAndAddEffectDef(swordHitEffect);

            swordSwingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlash.prefab").WaitForCompletion().InstantiateClone("KoaleskSwordSwing", false);
            swordSwingEffect.transform.GetChild(0).localScale *= 1.5f;
            swordSwingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matGenericSwingTrail.mat").WaitForCompletion();
            swordSwingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", koaleskColor);
            var swing = swordSwingEffect.transform.GetChild(0).GetComponent<ParticleSystem>().main;
            swing.startLifetimeMultiplier = 2f;

            swordBigSwingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordFinisherSlash.prefab").WaitForCompletion().InstantiateClone("KoaleskBigSwordSwing", false);
            swordBigSwingEffect.transform.GetChild(0).localScale *= 1.5f;
            swordBigSwingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matGenericSwingTrail.mat").WaitForCompletion();
            swordBigSwingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", koaleskColor);
            swing = swordBigSwingEffect.transform.GetChild(0).GetComponent<ParticleSystem>().main;
            swing.startLifetimeMultiplier = 2f;
        }

        #endregion

        #region projectiles
        private static void CreateProjectiles()
        {
            KoaleskBloodyStakeProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/FMJRamping.prefab").WaitForCompletion().InstantiateClone("KoaleskBloodyStake");

            ProjectileSimple needleSimple = KoaleskBloodyStakeProjectile.GetComponent<ProjectileSimple>();
            needleSimple.desiredForwardSpeed = 125f;
            needleSimple.lifetime = 3f;
            needleSimple.updateAfterFiring = true;

            ProjectileDamage needleDamage = KoaleskBloodyStakeProjectile.GetComponent<ProjectileDamage>();
            needleDamage.damageType = DamageType.Generic;

            KoaleskBloodyStakeProjectile.AddComponent<ProjectileTargetComponent>();

            ProjectileOverlapAttack needleLap = KoaleskBloodyStakeProjectile.GetComponent<ProjectileOverlapAttack>();
            needleLap.impactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniImpactExecute");
            needleLap.resetInterval = 0.5f;
            needleLap.overlapProcCoefficient = 0.75f;


            KoaleskBloodyStakeProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            KoaleskBloodyStakeProjectile.GetComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(DamageTypes.KoaleskLiquorDamage);

            ProjectileController projectileController = KoaleskBloodyStakeProjectile.GetComponent<ProjectileController>();
            projectileController.procCoefficient = 1f;
            KoaleskBloodyStakeGhost = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/MageIceBombProjectile").GetComponent<ProjectileController>().ghostPrefab.InstantiateClone("NeedleGhost", false);
            KoaleskBloodyStakeGhost.transform.GetChild(0).gameObject.SetActive(false);
            KoaleskBloodyStakeGhost.transform.GetChild(1).gameObject.SetActive(false);
            KoaleskBloodyStakeGhost.transform.GetChild(2).localScale = new Vector3(0.2f, 0.2f, 1.66f);
            KoaleskBloodyStakeGhost.transform.GetChild(2).gameObject.GetComponent<MeshFilter>().mesh = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpVoidspikeProjectileGhost.prefab").WaitForCompletion().transform.GetChild(0).GetComponent<MeshFilter>().mesh;
            KoaleskBloodyStakeGhost.transform.GetChild(2).gameObject.GetComponent<MeshRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpClaw.mat").WaitForCompletion());
            KoaleskBloodyStakeGhost.transform.GetChild(3).gameObject.SetActive(false);
            KoaleskBloodyStakeGhost.transform.GetChild(4).localScale = new Vector3(.2f, .2f, .2f);
            KoaleskBloodyStakeGhost.transform.GetChild(4).GetChild(0).gameObject.GetComponent<TrailRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpPortalEffectEdge.mat").WaitForCompletion());
            KoaleskBloodyStakeGhost.transform.GetChild(4).GetChild(1).gameObject.GetComponent<TrailRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpPortalEffectEdge.mat").WaitForCompletion());
            KoaleskBloodyStakeGhost.transform.GetChild(4).GetChild(3).gameObject.SetActive(false);
            KoaleskBloodyStakeGhost = KoaleskBloodyStakeGhost.InstantiateClone("NeedleGhost");
            Object.Destroy(KoaleskBloodyStakeGhost.GetComponent<EffectComponent>());
            if (KoaleskBloodyStakeGhost)
                projectileController.ghostPrefab = KoaleskBloodyStakeGhost;
            if (!projectileController.ghostPrefab.GetComponent<NetworkIdentity>())
                projectileController.ghostPrefab.AddComponent<NetworkIdentity>();
            if (!projectileController.ghostPrefab.GetComponent<ProjectileGhostController>())
                projectileController.ghostPrefab.AddComponent<ProjectileGhostController>();
            projectileController.startSound = "";

            projectileController.ghostPrefab.GetComponent<VFXAttributes>().DoNotPool = true;

            Modules.Content.AddProjectilePrefab(KoaleskBloodyStakeProjectile);

            var BlightDot = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MiniMushroom/SporeGrenadeProjectileDotZone.prefab").WaitForCompletion().InstantiateClone("KoaleskDotZoneBlightProjectile");
            if (!BlightDot.GetComponent<NetworkIdentity>()) BlightDot.AddComponent<NetworkIdentity>();

            BlightDot.transform.localScale *= 0.5f;

            ProjectileDotZone dotZone = BlightDot.GetComponent<ProjectileDotZone>();
            dotZone.damageCoefficient = 0f;
            dotZone.lifetime = 5f;
            dotZone.overlapProcCoefficient = 0f;

            BlightDot.AddComponent<ModdedDamageTypeHolderComponent>().Add(DamageTypes.KoaleskBlightProjectileDamage);

            Modules.Content.AddProjectilePrefab(BlightDot);

            KoaleskBlightProjectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MiniMushroom/SporeGrenadeProjectile.prefab").WaitForCompletion().InstantiateClone("KoaleskBlightProjectile");
            if (!BlightDot.GetComponent<NetworkIdentity>()) BlightDot.AddComponent<NetworkIdentity>();

            ProjectileSimple ps = KoaleskBlightProjectilePrefab.GetComponent<ProjectileSimple>();

            ps.desiredForwardSpeed = 0f;

            ProjectileController pc = KoaleskBlightProjectilePrefab.GetComponent<ProjectileController>();

            pc.procCoefficient = 0f;
            pc.ghostPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MiniMushroom/SporeGrenadeGhost.prefab").WaitForCompletion();

            ProjectileImpactExplosion pie = KoaleskBlightProjectilePrefab.GetComponent<ProjectileImpactExplosion>();
            pie.childrenProjectilePrefab = BlightDot;
            pie.lifetime = 5f;

            Modules.Content.AddProjectilePrefab(KoaleskBlightProjectilePrefab);

            KoaleskGardenFlower = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/LightningStake"), "KoaleskFlowerProjectile");

            ModdedDamageTypeHolderComponent mdthc = KoaleskGardenFlower.AddComponent<ModdedDamageTypeHolderComponent>();
            mdthc.Add(DamageTypes.KoaleskGardenDamage);

            pie = KoaleskGardenFlower.GetComponent<ProjectileImpactExplosion>();
            pie.blastProcCoefficient = 0f;

            Modules.Content.AddProjectilePrefab(KoaleskGardenFlower);
        }
        #endregion

        #region sounds
        private static void CreateSounds()
        {
            LoadSoundbank();

            swordImpactSoundEvent = Modules.Content.CreateAndAddNetworkSoundEventDef("Play_merc_sword_impact");
        }

        internal static void LoadSoundbank()
        {
            using (Stream manifestResourceStream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("KoaleskMod.koalesk_bank.bnk"))
            {
                byte[] array = new byte[manifestResourceStream2.Length];
                manifestResourceStream2.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }
        }
        #endregion

        private static void CreateUI()
        {
        }

        #region helpers
        private static GameObject CreateImpactExplosionEffect(string effectName, Material bloodMat, Material decal, float scale = 1f)
        {
            GameObject newEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone(effectName, true);

            newEffect.transform.Find("Spikes, Small").gameObject.SetActive(false);

            newEffect.transform.Find("PP").gameObject.SetActive(false);
            newEffect.transform.Find("Point light").gameObject.SetActive(false);
            newEffect.transform.Find("Flash Lines").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustLargeDirectional.mat").WaitForCompletion();

            newEffect.transform.GetChild(3).GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.GetChild(6).GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.Find("Fire").GetComponent<ParticleSystemRenderer>().material = bloodMat;

            var boom = newEffect.transform.Find("Fire").GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.5f;
            boom = newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.3f;
            boom = newEffect.transform.GetChild(6).GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.4f;

            newEffect.transform.Find("Physics").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/MagmaWorm/matFracturedGround.mat").WaitForCompletion();

            newEffect.transform.Find("Decal").GetComponent<Decal>().Material = decal;
            newEffect.transform.Find("Decal").GetComponent<AnimateShaderAlpha>().timeMax = 10f;

            newEffect.transform.Find("FoamSplash").gameObject.SetActive(false);
            newEffect.transform.Find("FoamBilllboard").gameObject.SetActive(false);
            newEffect.transform.Find("Dust").gameObject.SetActive(false);
            newEffect.transform.Find("Dust, Directional").gameObject.SetActive(false);

            newEffect.transform.localScale = Vector3.one * scale;

            newEffect.AddComponent<NetworkIdentity>();

            ParticleSystemColorFromEffectData PSCFED = newEffect.AddComponent<ParticleSystemColorFromEffectData>();
            PSCFED.particleSystems = new ParticleSystem[]
            {
                newEffect.transform.Find("Fire").GetComponent<ParticleSystem>(),
                newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystem>(),
                newEffect.transform.GetChild(6).GetComponent<ParticleSystem>(),
                newEffect.transform.GetChild(3).GetComponent<ParticleSystem>()
            };
            PSCFED.effectComponent = newEffect.GetComponent<EffectComponent>();

            KoaleskMod.Modules.Content.CreateAndAddEffectDef(newEffect);

            return newEffect;
        }
        public static Material CreateMaterial(string materialName, float emission, Color emissionColor, float normalStrength)
        {
            if (!commandoMat) commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            Material mat = UnityEngine.Object.Instantiate<Material>(commandoMat);
            Material tempMat = mainAssetBundle.LoadAsset<Material>(materialName);

            if (!tempMat) return commandoMat;

            mat.name = materialName;
            mat.SetColor("_Color", tempMat.GetColor("_Color"));
            mat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            mat.SetColor("_EmColor", emissionColor);
            mat.SetFloat("_EmPower", emission);
            mat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            mat.SetFloat("_NormalStrength", normalStrength);

            return mat;
        }

        public static Material CreateMaterial(string materialName)
        {
            return CreateMaterial(materialName, 0f);
        }

        public static Material CreateMaterial(string materialName, float emission)
        {
            return CreateMaterial(materialName, emission, Color.black);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor)
        {
            return CreateMaterial(materialName, emission, emissionColor, 0f);
        }
        #endregion
    }
}