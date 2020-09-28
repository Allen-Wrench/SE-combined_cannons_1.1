using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace BalancedDeformation
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Core : MySessionComponentBase
    {

        public const float General_Deformation_Ratio = 0.25f;
        public const float HA_Deformation_Ratio = 0.25f;
        public const float LA_Deformation_Ratio = 0.75f;
        public const float General_Damage_Multiplier = 1f;
        public const float small_grid_HA_Damage_Multiplier = 0.5f;
        public const float large_grid_HA_Damage_Multiplier = 1.0f;

        public override bool UpdatedBeforeInit()
        {
            foreach (MyDefinitionBase def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                MyCubeBlockDefinition blockDef = def as MyCubeBlockDefinition;

                if (blockDef == null) continue;

                blockDef.DeformationRatio = General_Deformation_Ratio;
		blockDef.GeneralDamageMultiplier = General_Damage_Multiplier;

                if (blockDef.Id.SubtypeName.Contains("Armor"))
                {
                    	blockDef.DeformationRatio = LA_Deformation_Ratio;
			if (blockDef.Id.SubtypeName.Contains("Heavy")) 
				{
				blockDef.GeneralDamageMultiplier = large_grid_HA_Damage_Multiplier; blockDef.DeformationRatio = HA_Deformation_Ratio;
				}
		    	if (blockDef.Id.SubtypeName.Contains("Heavy") && blockDef.Id.SubtypeName.Contains("Small")) 
				{
				blockDef.GeneralDamageMultiplier = small_grid_HA_Damage_Multiplier; blockDef.DeformationRatio = HA_Deformation_Ratio;
				}
                }
            }

            return true;
        }
    }
}