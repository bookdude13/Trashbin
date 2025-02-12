using HarmonyLib;
using System;
using Il2CppTMPro;
using Il2CppUtil.Controller;
using Il2Cpp;
using SRModCore;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace Trashbin.Harmony
{
    [HarmonyPatch(typeof(PlaylistManagementController), nameof(PlaylistManagementController.TryDisplayCorrectRemoveFavoriteButton))]
    public class Patch_PlaylistManagementController_TryDisplayCorrectRemoveFavoriteButton
    {
        public static void Prefix(PlaylistManagementController __instance)
        {
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
        }
    }
}
