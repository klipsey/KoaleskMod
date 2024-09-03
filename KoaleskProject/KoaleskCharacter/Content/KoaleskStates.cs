using KoaleskMod.Modules.BaseStates;
using KoaleskMod.KoaleskCharacter.SkillStates;

namespace KoaleskMod.KoaleskCharacter.Content
{
    public static class KoaleskStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(BaseKoaleskSkillState));
            Modules.Content.AddEntityState(typeof(MainState));
            Modules.Content.AddEntityState(typeof(BaseKoaleskState));
            Modules.Content.AddEntityState(typeof(RoseThorn));
            Modules.Content.AddEntityState(typeof(DarkThorn));
        }
    }
}
