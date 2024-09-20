using EntityStates;
using RoR2;
using KoaleskMod.KoaleskCharacter.Components;
using KoaleskMod.KoaleskCharacter.Content;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace KoaleskMod.Modules.BaseStates
{
    public abstract class BaseKoaleskSkillState : BaseSkillState
    {
        protected KoaleskController koaleskController;
        protected bool empowered;
        public virtual void AddRecoil2(float x1, float x2, float y1, float y2)
        {
            this.AddRecoil(x1, x2, y1, y2);
        }
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        protected void RefreshState()
        {
            if (!koaleskController)
            {
                koaleskController = base.GetComponent<KoaleskController>();
            }
            empowered = characterBody.HasBuff(KoaleskBuffs.koaleskGardenBuff);
        }
    }
}
