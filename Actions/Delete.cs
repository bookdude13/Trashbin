using UnityEngine;
using MelonLoader;
using Il2Cpp;
using Il2CppMono.Data.Sqlite;
using Il2CppNewtonsoft.Json;
using Il2CppSynth.SongSelection;
using Il2CppTMPro;
using System.Timers;
using SRModCore;

namespace Trashbin.Actions
{
    [RegisterTypeInIl2Cpp]
    public abstract class Delete : MonoBehaviour
    {        
        public static void VerifyDelete()
        {
            try
            {
                SongSelectionManager ssmInstance = SongSelectionManager.GetInstance;

                // Don't allow deleting non-customs!
                // TODO hook into the song select refresh like SRVoting and disable the button when the song is an OST
                if (ssmInstance.SelectedGameTrack == null || !ssmInstance.SelectedGameTrack.IsCustomSong)
                {
                    MelonLogger.Warning("Cannot delete OST songs!");
                    return;
                }


                // Open the Prompt Panel with unique prompt target 99 that will avoid pre-existing event from calling unwanted methods.
                // This also has a harmony hook to properly set the text of the prompt w/o localization interfering.
                ssmInstance.OpenPromptPanel(99);
            }
            catch (NullReferenceException ex)
            {
                MelonLogger.Msg("Null reference exception: " + ex.Message);
                MelonLogger.Msg("Stack Trace: " + ex.StackTrace);
            }
        }

        public static void DeleteSong()
        {
            SongSelectionManager ssmInstance = SongSelectionManager.GetInstance;
            int currentPrompt = ssmInstance.currentPrompt;

            //if (currentPrompt == 99) // since this method will also be called when the "continue button" is used in other contexts,
            //this unique currentPrompt value will prevent this method from doing anything in those cases
            
            if (currentPrompt != 99)
            {
                // Ignore if this request didn't come from our expected prompt
                return;
            }

            if (ssmInstance.SelectedGameTrack.IsCustomSong)
            {
                string filePath = ssmInstance.SelectedGameTrack.FilePath;
                DeleteCustomSong(ssmInstance, filePath);
            }
            else
            {
                GameObject deleteButton = GameObject.Find("DeleteSongButton");
                Transform tooltip = deleteButton.transform.Find("Tooltip");
                Transform tooltipText = tooltip.Find("Text");
                tooltipText.GetComponentInChildren<TMP_Text>().text = "Can't delete OST songs";
                MelonLogger.Msg("Can't delete OST songs");
            }
        }

        private static void DeleteCustomSong(SongSelectionManager ssmInstance, string filePath)
        {
            MelonLogger.Msg(filePath);
            if (filePath == null)
            {
                return;
            }
            FileInfo synthFile = new FileInfo(filePath);
            MelonLogger.Msg("Deleting custom song");
            // gametrack?

            // remove from DB
            SynthsFinder sf_instance = SynthsFinder.s_instance;
            string mainDirPath = FileUtil.GetSynthRidersUcDir();
            MelonLogger.Msg(mainDirPath);
            Type typeSF = typeof(SynthsFinder);

            string imageFilePath = "";
            SqliteConnection? connection = null;
            var isDuplicate = false;

            try
            {
                string connectionString = "URI=file:" + mainDirPath + "/SynthDB";
                connection = new SqliteConnection(connectionString);
                connection.Open();

                //setup db connection
                MelonLogger.Msg("Opened DB connection");

                //get song info from DB
                string queryGetFile = "SELECT image_file FROM TracksCache WHERE file_name = @FileName";
                MelonLogger.Msg(queryGetFile);
                SqliteCommand cmd = new(queryGetFile, connection);
                cmd.Parameters.Add(new SqliteParameter("@FileName", synthFile.Name));

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    imageFilePath = reader[0].ToString(); // column image_file
                }

                reader.Close();
                cmd.Dispose();


                //check for duplicate image files
                string queryGetImg = "SELECT image_file FROM TracksCache WHERE image_file= @ImageFile";
                MelonLogger.Msg(queryGetImg);
                cmd = new(queryGetImg, connection);
                cmd.Parameters.Add(new SqliteParameter("@ImageFile", imageFilePath));

                var count = 0;

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    count++;
                }
                reader.Close();

                if (count > 1)
                {
                    isDuplicate = true;
                }

                //delete song info from db
                MelonLogger.Msg("Creating query");
                string queryDelete = "DELETE FROM TracksCache WHERE file_name = @FileName";
                MelonLogger.Msg(queryDelete);
                cmd = new(queryDelete, connection);
                cmd.Parameters.Add(new SqliteParameter("@FileName", synthFile.Name));
                MelonLogger.Msg("Executing query");
                cmd.ExecuteNonQuery();
                MelonLogger.Msg("Deleted song from DB");

                // Cleanup
                cmd.Dispose();
                connection.Close();
            }
            catch (Exception ex)
            {
                MelonLogger.Msg("Failed to access DB");
                MelonLogger.Msg(ex);
                connection?.Close();
                return;
            }

            // delete synth-file
            if ((bool)ssmInstance.waitForSongsLoad)
            {
                Task.Delay(100).Wait();
            }

            Il2CppSystem.Collections.Generic.List<string> blacklist = new();

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    MelonLogger.Msg("Deleted synth file " + ssmInstance.SelectedGameTrack.m_name);
                }
                if (!isDuplicate && File.Exists(mainDirPath + "/NmBlacklist.json"))
                {
                    blacklist = JsonConvert.DeserializeObject<Il2CppSystem.Collections.Generic.List<string>>(File.ReadAllText(mainDirPath + "/NmBlacklist.json"));
                    blacklist.Add(synthFile.Name);
                    File.WriteAllText(mainDirPath + "/NmBlacklist.json", JsonConvert.SerializeObject(blacklist));
                }
                else if (!isDuplicate)
                {
                    StreamWriter blacklistStream = File.AppendText(mainDirPath + "/NmBlacklist.json");
                    blacklist.Add(synthFile.Name);
                    blacklistStream.Write(JsonConvert.SerializeObject(blacklist));
                    blacklistStream.Close();
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg("Loading ongoing");
                MelonLogger.Msg(ex);
                return;
            }

            //Delete coresponding image file and leftover audio file
            if (File.Exists(imageFilePath) & (!isDuplicate))
            {
                File.Delete(imageFilePath);
                MelonLogger.Msg("Deleted image file");
            }

            // reload song list 
            ssmInstance.RefreshSongList(false);
            MelonLogger.Msg("Updated song list"); // use RefreshCustomSongs() instead?
        }

        public void TimerEvent(object sender, ElapsedEventArgs e)
        {
            GameObject deleteButton = GameObject.Find("DeleteSongButton");
            Transform tooltip = deleteButton.transform.Find("Tooltip");
            Transform tooltipText = tooltip.Find("Text");
            tooltipText.GetComponentInChildren<TMP_Text>().text = "Delete current song";
        }

    }
}
