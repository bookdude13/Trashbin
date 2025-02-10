﻿using MelonLoader;
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
        //public static Trashbin? cs_instance;
        public static Trashbin Instance { get; private set; }
        private SRLogger logger;
        //Timer warnTimer = new(2000);

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
            Transform controls = songSelection.transform.Find("SelectionSongPanel/CentralPanel/Song Selection/VisibleWrap/Canvas/DetailsPanel(Right)/Sectional BG - Details/Controls-Buttons");
            Transform blacklistButton = controls.Find("Blacklist");
            GameObject deleteButton = GameObject.Instantiate(blacklistButton.gameObject);
            deleteButton.transform.name = "DeleteSongButton";
            deleteButton.transform.SetParent(controls);

            // Change button icon
            Transform deleteIcon = deleteButton.transform.Find("Outer Background/Inner Background/Icon");
            Texture2D iconTexture = new Texture2D(2, 2);
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream? binStream = assembly.GetManifestResourceStream("Trashbin.Resources.bin.png"))
            {
                if (binStream != null)
                {
                    byte[] data = new byte[binStream.Length];
                    binStream.Read(data, 0, (int)binStream.Length);
                    iconTexture.LoadImage(data);
                }
                else
                {
                    logger.Msg("Could not load trashbin image file");
                }
            }

            iconTexture.name = "bt-Close-X";
            Sprite iconSprite = Sprite.Create(iconTexture, new Rect(0, 0.0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
            iconSprite.name = "bt-X";
            Component[] components = deleteButton.GetComponents<Component>();
            Component[] allComponentsInChildren = deleteButton.GetComponentsInChildren<Component>(true);
            deleteIcon.GetComponent<Image>().sprite = iconSprite;
            //deleteIcon.localScale = new Vector3(0.15f, 0.15f, 1);

            // Adjust position of button
            Game_InfoProvider gipInstance = Game_InfoProvider.s_instance;
            TwitchAuthSettings twitchAS = gipInstance.twitchAuth;
            deleteButton.transform.localScale = new Vector3(0.7f, 0.7f, 1);
            deleteButton.transform.localRotation = new Quaternion(0, 0, 0, 1);

            // check if Twitch panel is enabled
            if (twitchAS.Channel != "")
            {
                deleteButton.transform.localPosition = new Vector3(-0.3f, 4.2102f, 0);
            }
            else //if twitch credentials not setup take same position as blacklist button
            {
                deleteButton.transform.localPosition = new Vector3(-0.8f, 4.2102f, 0);
            }

            //UnityUtil.LogComponentsRecursive(logger, deleteButton.transform.parent);

            // Crunch the spectrograph in the song select panel to make room for the button
            var spectrum = controls.Find("Visualizer Scale Wrap").GetComponent<RectTransform>();
            //UnityUtil.LogComponentsRecursive(logger, spectrum.parent);
            //spectrum.position += new Vector3(paddingLeft, 0f, 0f);
            spectrum.sizeDelta -= new Vector2(1f, 0f);

            // Add event to button
            var buttonUIToggle = deleteButton.gameObject.GetComponent<HexagonIconButton>();
            buttonUIToggle.WhenClicked = new UnityEvent();
            deleteButton.SetActive(true);

            buttonUIToggle.WhenClicked.AddListener((UnityAction)Delete.VerifyDelete);

            //UnityUtil.LogComponentsRecursive(logger, deleteButton.transform);
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
                SongSelectionManager ssmInstance = SongSelectionManager.GetInstance;
                GameObject TwoButtonsPromptWrap = ssmInstance.TwoButtonsPromptWrap;

                Transform continueBtnT = TwoButtonsPromptWrap.transform.Find("continue button");
                Component[] components = continueBtnT.GetComponents<Component>();

                var SynthButton = continueBtnT.gameObject.GetComponent<SynthUIButton>();
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
