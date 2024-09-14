using BepInEx.Configuration;
using NoMoreMath.Config;
using UnityEngine;

namespace NoMoreMath.HalcyonBeacon
{
    public class HalcyonBeaconConfig
    {
        public readonly ConfigEntry<bool> EnableTotalCostDisplay;

        public const string TOTAL_COST_DISPLAY_COST_TAG = "{total_cost}";
        public const string TOTAL_COST_DISPLAY_COLOR_TAG = "{color}";
        public readonly TagReplacementStringConfig TotalCostDisplayFormatter;

        public readonly ConfigEntry<Color> TotalCostAffordableColor;

        public readonly ConfigEntry<Color> TotalCostNotAffordableColor;

        public HalcyonBeaconConfig(in ConfigContext context)
        {
            EnableTotalCostDisplay = context.Bind("Display Total Cost", true, new ConfigDescription("If the total cost of all Halcyon Beacons in Gilded Coast should be displayed in the Objective panel"));

            const string TOTAL_COST_DISPLAY_FORMAT_DEFAULT = $"<nobr>(<color={TOTAL_COST_DISPLAY_COLOR_TAG}>${TOTAL_COST_DISPLAY_COST_TAG}</color>)</nobr>";
            ConfigEntry<string> totalCostDisplayFormatConfig = context.Bind("Total Cost Display Format", TOTAL_COST_DISPLAY_FORMAT_DEFAULT, new ConfigDescription($"""
                The format that will be used to display the total cost.
                
                All instances of '{TOTAL_COST_DISPLAY_COST_TAG}' (braces included!) will be replaced with the total cost

                All instances of '{TOTAL_COST_DISPLAY_COLOR_TAG}' (braces included!) will be replaced with a color code depending on if the total cost is affordable or not
                """));
            TotalCostDisplayFormatter = new TagReplacementStringConfig(totalCostDisplayFormatConfig, [TOTAL_COST_DISPLAY_COST_TAG, TOTAL_COST_DISPLAY_COLOR_TAG]);

            TotalCostAffordableColor = context.Bind("Total Cost Affordable Color", new Color(0f, 1f, 0f), new ConfigDescription($"The color used for the '{TOTAL_COST_DISPLAY_COLOR_TAG}' tag in the display format if the total price can be afforded."));

            TotalCostNotAffordableColor = context.Bind("Total Cost Not Affordable Color", new Color(1f, 0f, 0f), new ConfigDescription($"The color used for the '{TOTAL_COST_DISPLAY_COLOR_TAG}' tag in the display format if the total price cannot be afforded."));
        }
    }
}
