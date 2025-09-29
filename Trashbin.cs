using MelonLoader;
using Il2Cpp;
using Il2CppSynth.SongSelection;
using System.Reflection;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using Timer = System.Timers.Timer;
using Trashbin.Actions;
using UnityEngine.Events;
using Stream = System.IO.Stream;
using Directory = System.IO.Directory;
using SRModCore;
using static MelonLoader.MelonLogger;
using Il2CppTMPro;
using Il2Cppcom.Kluge.XR.UI;

namespace Trashbin
{
    public class Trashbin : MelonMod
    {
        public static Trashbin Instance { get; private set; }
        private SRLogger logger;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            logger = new MelonLoggerWrapper(LoggerInstance);
            Instance = this;
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            base.OnSceneWasInitialized(buildIndex, sceneName);

            MelonLogger.Msg(sceneName);
            SRScene scene = new SRScene(sceneName);

            if (scene.SceneType == SRScene.SRSceneType.MAIN_MENU)
            {
                ButtonInit(logger);
            }
        }

        private static void ButtonInit(SRLogger logger)
        {
            logger.Msg("Adding button...");
            var cs_instance = new Trashbin();

            // Initialise new button
            GameObject songSelection = GameObject.Find("Z-Wrap/SongSelection");
            Transform controls = UnityUtil.ValidatedFind(Instance.logger, songSelection?.transform, "SelectionSongPanel/CentralPanel/Song Selection/VisibleWrap/Canvas/DetailsPanel(Right)/Sectional BG - Details/Controls-Buttons");
            Transform blacklistButton = UnityUtil.ValidatedFind(Instance.logger, controls, "Blacklist");
            GameObject deleteButton = GameObject.Instantiate(blacklistButton.gameObject);
            deleteButton.transform.name = "DeleteSongButton";
            deleteButton.transform.SetParent(controls);

            // Change button icon
            Transform deleteIcon = UnityUtil.ValidatedFind(Instance.logger, deleteButton.transform, "Icon");
            var iconSprite = UnityUtil.CreateSpriteFromAssemblyResource(logger, Assembly.GetExecutingAssembly(), "Trashbin.Resources.bin.png");
            iconSprite.name = "bt-X";
            var iconImage = deleteIcon.GetComponent<Image>();
            iconImage.sprite = iconSprite;

            // Adjust position of button
            Game_InfoProvider gipInstance = Game_InfoProvider.s_instance;
            TwitchAuthSettings twitchAS = gipInstance.twitchAuth;
            deleteButton.transform.localScale = new Vector3(1f, 1f, 1f);
            deleteButton.transform.localRotation = new Quaternion(0, 0, 0, 1);

            // Use fav button as valid offset for position
            var favButton = SongSelectionManager.GetInstance.favoriteBtn.transform;

            // check if Twitch panel is enabled
            if (twitchAS.Channel != "")
            {
                deleteButton.transform.localPosition = new Vector3(favButton.localPosition.x + 1.5f, favButton.localPosition.y, 0);
            }
            else //if twitch credentials not setup take same position as blacklist button
            {
                deleteButton.transform.localPosition = new Vector3(favButton.localPosition.x + 1.0f, favButton.localPosition.y, 0);
            }

            // Crunch the spectrograph in the song select panel to make room for the button
            logger.Msg("Resizing spetrograph");
            var spectrum = UnityUtil.ValidatedFind(Instance.logger, controls.transform, "Visualizer Scale Wrap");
            var spectumRect = spectrum.GetComponent<RectTransform>();
            spectumRect.sizeDelta -= new Vector2(1f, 0f);

            // Add event to button
            logger.Msg("Adding button");
            var buttonUIToggle = deleteButton.gameObject.GetComponent<SynthUIButton>();
            buttonUIToggle.WhenClicked = new UnityEvent();

            deleteButton.SetActive(true);

            buttonUIToggle.WhenClicked.AddListener((UnityAction)Delete.VerifyDelete);

            // TODO set up fake localization for tooltip. 
            //buttonUIToggle.SetText("Delete current song");
            buttonUIToggle.TooltipLocalizationKey = string.Empty;

            logger.Msg("Button added");

            // add new events to the Two Buttons prompt's continue/cancel buttons
            cs_instance.AddEvents(logger);
        }

        public void AddEvents(SRLogger logger)
        {
            try
            {
                SongSelectionManager? ssmInstance = SongSelectionManager.GetInstance;
                if (ssmInstance == null)
                {
                    logger.Error("Null SSM instance, can't add events");
                    return;
                }

                GameObject? TwoButtonsPromptWrap = ssmInstance.TwoButtonsPromptWrap;
                if (TwoButtonsPromptWrap == null || TwoButtonsPromptWrap.transform == null)
                {
                    logger.Error("Null two buttons wrap");
                    return;
                }

                // Note to future self - returning an explicit transform here fails for some reason, but not elsewhere
                logger.Msg("Finding continue button");
                var continueBtnT = TwoButtonsPromptWrap.transform.Find("continue button");

                logger.Msg("Changing button to delete song");
                var SynthButton = UnityUtil.ValidatedGetComponent<SynthUIButton>(Instance.logger, continueBtnT?.gameObject)!;
                SynthButton.WhenClicked = new();
                SynthButton.WhenClicked.AddListener((UnityAction)Delete.DeleteSong);
            }
            catch (System.NullReferenceException ex)
            {
                logger.Error("Null reference exception: " + ex.Message);
                logger.Error("Stack Trace: " + ex.StackTrace);
            }
        }


        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();

            SynthsFinder sf_instance = SynthsFinder.s_instance;
            string audioFilePath = sf_instance.AudioFileCachePath;
            if (Directory.Exists(audioFilePath))
            {
                Directory.Delete(audioFilePath, true);
            }
            MelonLogger.Msg("Cleared audio cache");
        }
    }
}
