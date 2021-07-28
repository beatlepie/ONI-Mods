using HarmonyLib;
using Klei.AI;
using STRINGS;
using System.Collections.Generic;
using TUNING;
using PeterHan.PLib.UI;
using UnityEngine;

namespace ChooseNeuralVacillator
{
    [HarmonyPatch(typeof(GeneShuffler), "ApplyRandomTrait")]
    public class ApplyRandomTraitPatch
    {
		// The instance for [SelectWindow] so it can be destroyed later
		private static GameObject SelectWindowInstance = new GameObject();
		private static TextStyleSetting myStyle = PUITuning.Fonts.UILightStyle.DeriveStyle(18);

        /// <summary>
        /// Removes the randomizing component of the Neural Vacillator
        /// </summary>
        /// <param name="worker"> The duplicant using the Neural Vacillator </param>
        /// <param name="___IsConsumed"> The [bool] indicating the status of whether the [GeneShuffler] can be used or not </param>
        /// <param name="___geneShufflerSMI"> The instance of [GeneShufflerSM] so it can change state when necessary </param>
        /// <returns> Will not run the original code! </returns>
        private static bool Prefix(Worker worker, bool ___IsConsumed, GeneShuffler.GeneShufflerSM.Instance ___geneShufflerSMI)
        {
            Traits component = worker.GetComponent<Traits>();
			List<string> list = new List<string>();

			// Makes a list of traits the dupicant can get (can't get duplicate traits)
			foreach (DUPLICANTSTATS.TraitVal traitVal in DUPLICANTSTATS.GENESHUFFLERTRAITS)
			{
				if (!component.HasTrait(traitVal.id))
				{
					list.Add(traitVal.id);
				}
			}

			// If the duplicant already has all the traits, indicate that the dupe cannot gain any more and end
			if(list.Count == 0)
            {
				InfoDialogScreen infoDialogScreen2 = (InfoDialogScreen)GameScreenManager.Instance.StartScreen(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, GameScreenManager.Instance.ssOverlayCanvas.gameObject, GameScreenManager.UIRenderTarget.ScreenSpaceOverlay);
				string text2 = string.Format(UI.GENESHUFFLERMESSAGE.BODY_FAILURE, worker.GetProperName());
				infoDialogScreen2.SetHeader(UI.GENESHUFFLERMESSAGE.HEADER).AddPlainText(text2).AddDefaultOK(false);
				return false;
			}

			// This initializes the [SelectWindow] and also REDEFINES [PUITuning.Fonts.UILightStyle] with [fontSize = 18]!
			PDialog SelectWindow = new PDialog("SelectWindow");
			SelectWindow.Body.Spacing = 20;
			PLabel label = new PLabel("Description");
			label.Text = $"The duplicant ({worker.GetProperName()}) will recieve: ";
			label.TextStyle = myStyle;
			SelectWindow.Body.AddChild(label);

            // Moved this here as a potential fix, probably could be moved down
            SetConsumed(true, ___geneShufflerSMI);

            // Adds all the remaining traits the duplicant can gain with a button each
            foreach (string trait in list)
            {
				SelectWindow.Body.AddChild(new PButton(trait) 
				{
					Text = Db.Get().traits.TryGet(trait).Name,
					TextStyle = myStyle,
					ToolTip = Db.Get().traits.TryGet(trait).description,
					FlexSize = new Vector2(0.8f, 0.5f),
					OnClick = delegate (GameObject button) { TraitSelected(worker, ___IsConsumed, ___geneShufflerSMI, button.name); }
				});
            }
			// Adds the random button last
			SelectWindow.Body.AddChild(new PButton("Random")
			{
				Text = "random",
				TextStyle = myStyle,
				FlexSize = new Vector2(0.8f, 1),
				OnClick = delegate (GameObject button) { RandomSelected(worker, ___IsConsumed, ___geneShufflerSMI, list); }
			});

			SelectWindowInstance = SelectWindow.Build();

			return false;
        }

		/// <summary>
		/// The action to be preformed when the button is pressed
		/// Same as original code, but with a specific [traitID]
		/// </summary>
		private static void TraitSelected(Worker worker, bool ___IsConsumed, GeneShuffler.GeneShufflerSM.Instance ___geneShufflerSMI, string traitID)
        {
			string id = traitID;
			Trait trait = Db.Get().traits.TryGet(id);
			worker.GetComponent<Traits>().Add(trait);
			InfoDialogScreen infoDialogScreen = (InfoDialogScreen)GameScreenManager.Instance.StartScreen(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, GameScreenManager.Instance.ssOverlayCanvas.gameObject, GameScreenManager.UIRenderTarget.ScreenSpaceOverlay);
			string text = string.Format(UI.GENESHUFFLERMESSAGE.BODY_SUCCESS, worker.GetProperName(), trait.Name, trait.GetTooltip());
			infoDialogScreen.SetHeader(UI.GENESHUFFLERMESSAGE.HEADER).AddPlainText(text).AddDefaultOK(false);

			SelectWindowInstance.DeleteObject();
		}

		/// <summary>
		/// Original [ApplyRandomTrait] code for applying a random trait
		/// </summary>
		private static void RandomSelected(Worker worker, bool ___IsConsumed, GeneShuffler.GeneShufflerSM.Instance ___geneShufflerSMI, List<string> list)
        {
			string id = list[UnityEngine.Random.Range(0, list.Count)];
			Trait trait = Db.Get().traits.TryGet(id);
			worker.GetComponent<Traits>().Add(trait);
			InfoDialogScreen infoDialogScreen = (InfoDialogScreen)GameScreenManager.Instance.StartScreen(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, GameScreenManager.Instance.ssOverlayCanvas.gameObject, GameScreenManager.UIRenderTarget.ScreenSpaceOverlay);
			string text = string.Format(UI.GENESHUFFLERMESSAGE.BODY_SUCCESS, worker.GetProperName(), trait.Name, trait.GetTooltip());
			infoDialogScreen.SetHeader(UI.GENESHUFFLERMESSAGE.HEADER).AddPlainText(text).AddDefaultOK(false);

			SelectWindowInstance.DeleteObject();
		}

		/// <summary>
		/// Original code that was present in the game code ([SetConsumed] and [RefreshConsumedState])
        /// Technically could just use [true] instead of [consumed], kept it in to keep it looking like the original
		/// </summary>
		private static void SetConsumed(bool consumed, GeneShuffler.GeneShufflerSM.Instance ___geneShufflerSMI)
        {
            ___geneShufflerSMI.master.IsConsumed = consumed;
            ___geneShufflerSMI.sm.isCharged.Set(!___geneShufflerSMI.master.IsConsumed, ___geneShufflerSMI);
        }
    }
}
