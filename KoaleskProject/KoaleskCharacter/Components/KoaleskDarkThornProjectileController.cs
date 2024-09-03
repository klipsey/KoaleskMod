using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using RoR2.HudOverlay;
using UnityEngine;

namespace KoaleskMod.KoaleskCharacter.Components
{
    public class KoaleskDarkThornProjectileController : MonoBehaviour
    {
        public RadialForce RadialForce;
        public BoomerangProjectile Boomerang;
        public SphereCollider Collider;

        public void FixedUpdate()
        {
            if (RadialForce && Boomerang && Collider)
            {
                if (Boomerang.NetworkboomerangState != BoomerangProjectile.BoomerangState.FlyBack)
                {
                    RadialForce.radius = 0;
                    Collider.radius = 0;
                }
                else
                {
                    RadialForce.radius = gameObject.transform.localScale.x;
                    Collider.radius = gameObject.transform.localScale.x;
                }
            }
        }
    }
}
