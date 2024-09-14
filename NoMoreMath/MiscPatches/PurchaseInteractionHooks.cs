using RoR2;
using System;
using System.Text;

namespace NoMoreMath.MiscPatches
{
    static class PurchaseInteractionHooks
    {
        static readonly StringBuilder _sharedStringBuilder = new StringBuilder();

        public delegate void ModifyNameDelegate(PurchaseInteraction purchaseInteraction, CharacterMaster viewer, StringBuilder nameBuilder);
        public static event ModifyNameDelegate ModifyName;

        [SystemInitializer]
        static void Init()
        {
            On.RoR2.PurchaseInteraction.GetDisplayName += PurchaseInteraction_GetDisplayName;
            On.RoR2.PurchaseInteraction.GetContextString += PurchaseInteraction_GetContextString;
        }

        static string tryModifyName(string name, PurchaseInteraction instance, CharacterMaster viewer)
        {
            if (ModifyName == null)
                return name;

            try
            {
                _sharedStringBuilder.Clear();
                _sharedStringBuilder.Append(name);

                ModifyName(instance, viewer, _sharedStringBuilder);

                return _sharedStringBuilder.ToString();
            }
            catch (Exception e)
            {
                Log.Error_NoCallerPrefix(e);
                return name;
            }
        }

        static string PurchaseInteraction_GetDisplayName(On.RoR2.PurchaseInteraction.orig_GetDisplayName orig, PurchaseInteraction self)
        {
            CharacterMaster viewerMaster = null;
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser != null)
            {
                viewerMaster = localUser.cachedMaster;
            }

            return tryModifyName(orig(self), self, viewerMaster);
        }

        static string PurchaseInteraction_GetContextString(On.RoR2.PurchaseInteraction.orig_GetContextString orig, PurchaseInteraction self, Interactor activator)
        {
            CharacterMaster viewerMaster = null;
            if (activator && activator.TryGetComponent(out CharacterBody viewerBody))
            {
                viewerMaster = viewerBody.master;
            }

            return tryModifyName(orig(self, activator), self, viewerMaster);
        }
    }
}
