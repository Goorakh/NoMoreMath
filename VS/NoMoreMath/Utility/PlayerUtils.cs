using RoR2;

namespace NoMoreMath.Utility
{
    public static class PlayerUtils
    {
        public static CharacterMaster GetLocalUserMaster()
        {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser != null)
            {
                CharacterMaster localPlayerMaster = localUser.cachedMaster;
                if (localPlayerMaster)
                {
                    return localPlayerMaster;
                }
            }

            return null;
        }
    }
}
