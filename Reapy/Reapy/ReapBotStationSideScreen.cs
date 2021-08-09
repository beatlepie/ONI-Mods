using PeterHan.PLib.UI;
using UnityEngine;

namespace Reapy
{
    // Special thanks to [Zonkeeh](https://github.com/Zonkeeh/ONI-Mods) and his [ConfigurableSweepy]!
    // Special thanks to [Peterhaneve]() and his [PLib]!
    class ReapBotStationSideScreen : SideScreenContent
    {
        private ReapBotStation target;

        public override void ClearTarget() => target = null;

        public override string GetTitle() => target.ReapBot == null ? "Reapy Configurator" : target.storedName + "'s Configurator";

        public override bool IsValidForTarget(GameObject target) => target.GetComponent<ReapBotStation>() != null;

        protected override void OnPrefabInit()
        {
            // Values used by [Zonkeeh], they should be similar to the game values!
            Color backColour = new Color(0.998f, 0.998f, 0.998f);
            RectOffset rectOffset = new RectOffset(8, 8, 8, 8);

            PPanel moveTitle_panel = new PPanel("MovespeedTitleRow")
            {
                BackColor = backColour,
                // This required reference to [UnityEngine.TextRenderingModule]!
                Alignment = TextAnchor.MiddleCenter,
                Direction = PanelDirection.Horizontal,
                Spacing = 10,
                Margin = rectOffset,
                FlexSize = Vector2.right
            };
            PLabel moveTitle_label = new PLabel("MovespeedTitleLabel")
            {
                TextAlignment = TextAnchor.MiddleRight,
                Text = $"{target.storedName}'s Speed",
                ToolTip = $"How fast {target.storedName} will move in tiles per second",
                TextStyle = PUITuning.Fonts.TextDarkStyle
            };
            //PTextField moveTitle_textField = new PTextField("MovespeedSliderTextField")
            //{
            //    Text = SweepyConfigChecker.BaseMovementSpeed.ToString("0.00"),
            //    MaxLength = 10,
            //};
            //moveTitle_textField.OnTextChanged = ChangeTextFieldMovespeed;
            //moveTitle_textField.OnRealize += obj => MoveSpeedText = obj;
        }
    }
}
