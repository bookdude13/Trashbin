using HarmonyLib;
using Il2Cpp;
using Il2Cppcom.Kluge.XR.UI;
using Il2CppUtil.Controller;
using UnityEngine;

namespace Trashbin.Harmony
{
    [HarmonyPatch(typeof(PlaylistManagementController), nameof(PlaylistManagementController.TryDisplayCorrectRemoveFavoriteButton))]
    public class Patch_PlaylistManagementController_TryDisplayCorrectRemoveFavoriteButton
    {
        public static void Postfix(PlaylistManagementController __instance)
        {
            // SRPlaylistManager changes this button too. Reposition _after_ it deals with it in its Prefix
            RepositionButton(__instance.pf_RemoveFromPlaylistButton);
        }

        private static void RepositionButton(GameObject buttonGO)
        {
            if (buttonGO == null)
                return;

            // The Remove From Playlist button is big enough to overlap with the Delete button we added,
            // so here we shrink the Remove From Playlist button a bit to compensate.
            var rect = buttonGO.GetComponent<RectTransform>();
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 3f, 4.5f);
            
            var button = buttonGO.GetComponentInChildren<SynthUIButton>();
            if (button != null)
            {
                button.hideTooltipOnClick = true;
                button.stayHoveredwhenClicked = false;
            }

            // Try to "leave" the button to reset scale properly
            var hexButton = buttonGO.GetComponent<HexagonIconButton>();
            if (hexButton != null)
            {
                hexButton.OnHexButtonExit();
            }
        }
    }
}
