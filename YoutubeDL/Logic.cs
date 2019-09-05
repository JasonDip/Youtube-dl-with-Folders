using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.Windows.Threading;

namespace DownloaderApp
{
    /**********************************************************************
    * MainWindow Class
    **********************************************************************/
    public partial class MainWindow
    {
        public static SavedSettings appSettings = new SavedSettings();

        /// <summary>
        /// Download the link provided by the user.
        /// </summary>
        public void downloadLink()
        {
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", "/C cd " + userPath + " && " + getCommand());

            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.CreateNoWindow = true;
            Process process = new Process();
            process.StartInfo = processStartInfo;

            bool processStarted = process.Start();

            StreamWriter inputWriter = process.StandardInput;
            StreamReader outputReader = process.StandardOutput;
            StreamReader errorReader = process.StandardError;

            int x = 0;
            while (process.HasExited == false)
            {
                tbConsole.AppendText(outputReader.ReadLine());
                tbConsole.AppendText("\n");
                tbConsole.Refresh();
                tbConsole.ScrollToEnd();

                // Clears the console text box to prevent it from becomming to large and laggy.
                if (x > 500)
                {
                    x = 0;
                    tbConsole.Text = "";
                }
                x++;

                System.Windows.Forms.Application.DoEvents();
            }

            tbConsole.AppendText(outputReader.ReadToEnd());
            tbConsole.AppendText("\r\n");
            tbConsole.ScrollToEnd();
        }

        private string getCommand()
        {
            string defaultFolderCommand = "";

            // Check if the command contains "+Folder". 
            if (lbSettings.SelectedItem.ToString().Replace(" ", string.Empty).ToLower().Contains("+folder"))
            {
                // Check if login is being used. If so, append the username and password.
                if ((bool)cbUseLogin.IsChecked)
                {
                    defaultFolderCommand = defaultFolderCommand + " -u " + tbUsername.Text + " -p " + tbPassword.Password + " ";
                }

                // Check if a folder is selected. If so, append folder location and default name.
                if (lbFolderList.SelectedIndex == -1)
                {
                    throw new Exception("No folder was selected while using +Folder."); // Should never get here.
                }
                else
                {
                    defaultFolderCommand = defaultFolderCommand + " -o \"" + lbFolderList.SelectedItem + "\\%(title)s.%(ext)s\"" + " ";
                    defaultFolderCommand = defaultFolderCommand + tbURL.Text;
                }
            }

            string commandValue = "";
            appSettings.commands.TryGetValue(appSettings.PresetSetting, out commandValue);

            return commandValue + defaultFolderCommand;
        }

    }

    /**********************************************************************
    * Settings Class
    **********************************************************************/
    public partial class SavedSettings
    {
        public SavedSettings() { }

        public string configurationSettingsPath = Directory.GetCurrentDirectory() + "\\.ConfigurationSettings";
        public List<string> FolderPaths = new List<String>();
        public string PreviousPath;
        public string PresetSetting="Video";
        public Dictionary<String, String> commands = new Dictionary<string, string> (); 
        
        /// <summary>
        /// Creates the current (full) settings string used in the Settings File.
        /// </summary>
        public string getSettingsString()
        {
            string settings = "";

            // Most recently browsed file path.
            settings = settings + "PREVIOUSPATH\r\n";
            settings = settings + this.PreviousPath + "\r\n";

            // Save Folder Paths.
            settings = settings + "SAVEFOLDERS\r\n";
            settings = settings + FolderPaths.Count + "\r\n";
            for (int x = 0; x < FolderPaths.Count; x++)
            {
                settings = settings + FolderPaths[x].ToString() + "\r\n";
            }

            // Previous Preset Setting.
            settings = settings + "PRESETSETTING\r\n";
            settings = settings + this.PresetSetting + "\r\n";

            // Previous Preset Setting.
            settings = settings + "ALLCOMMANDS\r\n";
            settings = settings + commands.Count.ToString() + "\r\n";
            foreach (KeyValuePair<String, String> kvp in commands)
            {
                settings = settings + kvp.Key + "\r\n";
                settings = settings + kvp.Value + "\r\n";
            }

            return settings;
        }

        /// <summary>
        /// Gets the default setting string (useful for when/if the setting file gets messed up.
        /// </summary>
        public string getDefaultSettingsString()
        {
            string settings = "";
            settings = settings + "PREVIOUSPATH\r\n";
            settings = settings + "%userprofile%\\documents\\youtube-dl\r\n";
            settings = settings + "SAVEFOLDERS\r\n";
            settings = settings + "1\r\n";
            settings = settings + "%userprofile%\\documents\\youtube-dl\r\n";
            settings = settings + "PRESETSETTING\r\n";
            settings = settings + "Audio(Default) +Folder\r\n";
            settings = settings + "ALLCOMMANDS\r\n";
            settings = settings + "2\r\n";
            settings = settings + "Audio(Default) +Folder\r\n";
            settings = settings + "youtube-dl --download-archive Downloaded.txt --no-post-overwrites -ciwx --extract-audio --audio-format mp3 -i --prefer-ffmpeg \r\n";
            settings = settings + "Video(Default) +Folder\r\n";
            settings = settings + "youtube-dl --write-sub --sub-lang enUS \r\n";

            return settings;
        }

        /// <summary>
        /// Get the previous saved settings from the configuration settings text file.
        /// </summary>
        public void getSavedSettingsFromFile()
        {
            string readLine;
            StreamReader reader;

            try { reader = new StreamReader(configurationSettingsPath); }
            catch { return; }

            while (reader.Peek() >= 0)
            {
                readLine = reader.ReadLine();
                switch (readLine)
                {
                    case "SAVEFOLDERS":
                        {
                            int folderCount = Convert.ToInt32(reader.ReadLine());
                            for (int x = 0; x < folderCount; x++)
                            {
                                this.FolderPaths.Add(reader.ReadLine());
                            } 
                        }
                        break;

                    case "PREVIOUSPATH":
                        {
                            this.PreviousPath = reader.ReadLine();
                        }
                        break;

                    case "PRESETSETTING":
                        {
                            this.PresetSetting = reader.ReadLine();
                        }
                        break;

                    case "ALLCOMMANDS":
                        {
                            commands.Clear();
                            int commandCount = Convert.ToInt32(reader.ReadLine());
                            for (int x = 0; x < commandCount; x++)
                            {
                                this.commands.Add(reader.ReadLine(), reader.ReadLine());
                            }
                        }
                        break;
                }
            }
            reader.Close();
        }

        /// <summary>
        /// Save the current settings to a configuration settings file.
        /// </summary>
        public void saveSettings()
        {
            
            // Close file in case it is open.
            File.Create(configurationSettingsPath).Close();
            // Create new file and write settings string.
            FileStream fileStream = new FileStream(configurationSettingsPath, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fileStream);
            writer.Write(getSettingsString());
            writer.Close();
        }

        /// <summary>
        /// Checks if the ConfigurationSettings files exists. If not, creates one.
        /// </summary>
        public void checkForSettings()
        {
            if (!File.Exists(configurationSettingsPath))
            {
                // Close file in case it is open.
                File.Create(configurationSettingsPath).Close();
                // Create new file and write settings string.
                FileStream fileStream = new FileStream(configurationSettingsPath, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.Write(getDefaultSettingsString());
                writer.Close();
            }
        }

    }


    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };


        public static void Refresh(this UIElement uiElement)

        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }

}
