using HarmonyLib;
using Il2CppSynth.SongSelection;

namespace Trashbin.Harmony
{
    [HarmonyPatch(typeof(SongSelectionManager), nameof(SongSelectionManager.OpenPromptPanel))]
    public class Patch_SongSelectionManager_OpenPromptPanel
    {
        public static bool Prefix(SongSelectionManager __instance, int promptTarget)
        {
            var localization = __instance.promptLabel;

            if (promptTarget == 99)
            {
                // Handle as our own custom prompt; override existing logic
                Trashbin.Instance.LoggerInstance.Msg("Prompt target 99 opened");
                
                // Don't allow localization to change the text we set
                localization.enabled = false;

                // Update to the prompt text we want
                var prompt = "Delete and blacklist this song?";
                localization.m_TextMeshField?.SetText(prompt);
                localization.m_TextMeshUI?.SetText(prompt);
                if (localization.m_UnityUIText)
                {
                    localization.m_UnityUIText.text = prompt;
                }
                //return false;
            }
            else
            {
                // Re-enable for other prompts
                localization.enabled = true;
                // Make sure string is updated away from what we set
                localization.RefreshTextString();
            }

            return true;
        }
    }
}
