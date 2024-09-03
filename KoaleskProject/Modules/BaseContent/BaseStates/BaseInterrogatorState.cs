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
    public abstract class BaseKoaleskState : BaseState
    {
        protected KoaleskController koaleskController;

        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RefreshState();
        }
        protected void RefreshState()
        {
            if (!koaleskController)
            {
                koaleskController = base.GetComponent<KoaleskController>();
            }
        }
    }
}
