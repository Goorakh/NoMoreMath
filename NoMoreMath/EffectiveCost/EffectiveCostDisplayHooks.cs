using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using NoMoreMath.Config;
using RoR2;

namespace NoMoreMath.EffectiveCost
{
    static class EffectiveCostDisplayHooks
    {
        [SystemInitializer(typeof(CostTypeCatalog))]
        static void Init()
        {
            CostTypeDef moneyCostDef = CostTypeCatalog.GetCostTypeDef(CostTypeIndex.Money);

            new ILHook(moneyCostDef.buildCostString.Method, MoneyCostTypeDef_BuildCostString_Manipulator);
        }

        static void MoneyCostTypeDef_BuildCostString_Manipulator(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ParameterDefinition costTypeDefParam = null;
            ParameterDefinition contextParam = null;

            foreach (ParameterDefinition parameter in il.Method.Parameters)
            {
                if (parameter.ParameterType.Is(typeof(CostTypeDef)))
                {
                    costTypeDefParam ??= parameter;
                }
                else if (parameter.ParameterType.Is(typeof(CostTypeDef.BuildCostStringContext)))
                {
                    contextParam ??= parameter;
                }
            }

            if (costTypeDefParam == null)
            {
                Log.Error("Failed to find CostTypeDef parameter");
                return;
            }

            if (contextParam == null)
            {
                Log.Error("Failed to find context parameter");
                return;
            }

            while (c.TryGotoNext(MoveType.Before,
                                 x => x.MatchRet()))
            {
                c.Emit(OpCodes.Ldarg, costTypeDefParam);
                c.Emit(OpCodes.Ldarga, contextParam);
                c.EmitDelegate(postfix);
                static void postfix(CostTypeDef costTypeDef, in CostTypeDef.BuildCostStringContext context)
                {
                    if (costTypeDef == null)
                        return;

                    EffectiveCostConfig effectiveCostConfig = Configs.EffectiveCost;
                    if (!effectiveCostConfig.Enabled.Value)
                        return;

                    LocalUser localUser = LocalUserManager.GetFirstLocalUser();
                    if (localUser == null)
                        return;

                    CharacterMaster localUserMaster = localUser.cachedMaster;
                    if (!localUserMaster)
                        return;

                    int effectiveCost = EffectiveCostUtils.GetEffectiveCost(costTypeDef, context.cost, localUserMaster);
                    if (effectiveCost == context.cost)
                        return;

                    string formattedEffectiveCost = Language.GetStringFormatted(costTypeDef.costStringFormatToken, effectiveCost);

                    context.stringBuilder.EnsureCapacity(context.stringBuilder.Length + effectiveCostConfig.CostDisplayFormat.StrippedLength + formattedEffectiveCost.Length + 1);

                    context.stringBuilder.Append(' ');
                    effectiveCostConfig.CostDisplayFormat.AppendToStringBuilder(context.stringBuilder, new TagReplacementStringConfig.ReplacementInfo(EffectiveCostConfig.EFFECTIVE_COST_FORMAT_VALUE_TAG, formattedEffectiveCost));
                }

                c.SearchTarget = SearchTarget.Next;
            }
        }
    }
}
