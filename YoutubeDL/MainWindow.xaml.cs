using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

namespace DownloaderApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /**********************************************************************
         * Home Screen Load Events
         **********************************************************************/
        private void HomeScreen_Initialized(object sender, EventArgs e)
        {

        }

        private void HomeScreen_Loaded(object sender, RoutedEventArgs e)
        {
            disableButtons();
            lblStatus.Content = "Loading...";
            lblStatus.Foreground = Brushes.Red;

            appSettings.checkForSettings();
            appSettings.getSavedSettingsFromFile();
            populate_lbFolderList();
            setPresetSettingInMainWindow();

            lblStatus.Content = "Ready";
            lblStatus.Foreground = Brushes.Green;
            enableButtons();

            tbURL.Focus();
        }

        /**********************************************************************
         * UI Events
         **********************************************************************/
        private void btnBrowseFolders_Click(object sender, RoutedEventArgs e)
        {
            openFolderBrowser();
        }


        private void lbFolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            disableButtons();
            appSettings.saveSettings();
            lblStatus.Content = "Ready [Settings were saved.]";
            lblStatus.Foreground = Brushes.Green;
            enableButtons();
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (tbURL.Text == "")
            {
                lblStatus.Content = "A URL must be provided.";
                lblStatus.Foreground = Brushes.Red;
                return;
            }
            else if (lbSettings.SelectedIndex == -1)
            {
                lblStatus.Content = "A save setting must be selected.";
                lblStatus.Foreground = Brushes.Red;
                return;
            }
            else if ((lbFolderList.SelectedIndex == -1) && lbSettings.SelectedItem.ToString().Replace(" ", string.Empty).ToLower().Contains("+folder"))
            {
                lblStatus.Content = "A destination folder must be selected.";
                lblStatus.Foreground = Brushes.Red;
                return;
            }
            else // Valid settings. Begin download.
            {
                // Disable UI
                disableButtons();
                lblStatus.Content = "Working...";
                lblStatus.Foreground = Brushes.Blue;
                tbConsole.Clear();
                // Start Timer
                string startTime = DateTime.Now.ToString("hh:mm:ss tt");
                Stopwatch downloadStopWatch = new Stopwatch();
                downloadStopWatch.Start();
                // Start Download
                downloadLink();
                // Stop Timer
                downloadStopWatch.Stop();
                string endTime = DateTime.Now.ToString("hh:mm:ss tt");
                TimeSpan timeSpan = downloadStopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                tbConsole.AppendText("Start Time: "+startTime+" --- End Time: "+endTime+"\nTime Elapsed: " + elapsedTime);
                // Enable UI
                lblStatus.Content = "Ready";
                lblStatus.Foreground = Brushes.Green;
                enableButtons();
            }
        }

        private void btnRemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to remove \"" + lbFolderList.SelectedItem.ToString() + "\" from the folder list?", "Confirm Deletion", MessageBoxButtons.YesNo);
            if (dialogResult == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            if (lbFolderList.SelectedIndex != -1)
            {
                lbFolderList.Items.RemoveAt(lbFolderList.SelectedIndex);
            }
        }

        private void tbURL_GotFocus(object sender, RoutedEventArgs e)
        {
            tbURL.Text = "";
        }

        private void tbURL_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                typeof(System.Windows.Controls.Primitives.ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(btnDownload, new object[0]);
            }
        }

        private void btnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsPopup = new DownloaderApp.Settings();
            settingsPopup.ShowDialog();

            // repopulate the Settings list-box
            lbSettings.Items.Clear();
            foreach (KeyValuePair<String, String> kvp in appSettings.commands)
            {
                lbSettings.Items.Add(kvp.Key.ToString());
            }
        }

        private void BtnRemoveSetting_Click(object sender, RoutedEventArgs e)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to delete \"" + lbSettings.SelectedItem.ToString() +"\"?", "Confirm Deletion", MessageBoxButtons.YesNo);
            if (dialogResult == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            if (lbSettings.SelectedIndex != -1)
            {
                appSettings.commands.Remove(lbSettings.SelectedItem.ToString());
                lbSettings.Items.RemoveAt(lbSettings.SelectedIndex);
            }
        }

        private void LbSettings_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string defaultFolderCommand = "";

            if (lbSettings.SelectedIndex == -1)
            {
                return;
            }

            if (lbSettings.SelectedItem.ToString().Replace(" ", string.Empty).ToLower().Contains("+folder"))
            {
                // Check if login is being used. If so, append the username and password.
                if ((bool)cbUseLogin.IsChecked)
                {
                    defaultFolderCommand = defaultFolderCommand + " -u " + tbUsername.Text + " -p " + tbPassword.Password + " ";
                }

                if (lbFolderList.SelectedIndex == -1)
                {
                    System.Windows.MessageBox.Show("This setting is using the \"+Folder\" feature. Add and then select a folder.");
                    return;
                }
                else
                {
                    defaultFolderCommand = defaultFolderCommand + " -o \"" + lbFolderList.SelectedItem + "\\%(title)s.%(ext)s\"" + " ";
                    defaultFolderCommand = defaultFolderCommand + tbURL.Text;
                }
            }
            System.Windows.MessageBox.Show(appSettings.commands[lbSettings.SelectedItem.ToString()] + " " + defaultFolderCommand);
        }

        private void LbSettings_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lbSettings.SelectedIndex != -1)
            {
                appSettings.PresetSetting = lbSettings.SelectedItem.ToString();
            }
        }

        private void CbUseLogin_Click(object sender, RoutedEventArgs e)
        {
            // Note: IsChecked returns the state BEFORE the click event finishes
            if ((bool)cbUseLogin.IsChecked)
            {
                // About to apply a check
                tbUsername.IsEnabled = true;
                tbPassword.IsEnabled = true;
            }
            else
            {
                // About to remove a check
                tbUsername.IsEnabled = false;
                tbPassword.IsEnabled = false;
            }
        }

        private void TbUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            tbUsername.Text = "";
        }

        private void TbPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            tbPassword.Password = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var aboutPopup = new DownloaderApp.About();
            aboutPopup.ShowDialog();
        }

        /**********************************************************************
         * UI Helper Functions
         **********************************************************************/
        /// <summary>
        /// Disable buttons so user cannot mess with stuff while download is in progress.
        /// </summary>
        private void disableButtons()
        {
            btnDownload.IsEnabled = false;
            btnSaveSettings.IsEnabled = false;
        }

        /// <summary>
        /// Enable buttons after work is finished.
        /// </summary>
        private void enableButtons()
        {
            btnDownload.IsEnabled = true;
            btnSaveSettings.IsEnabled = true;
        }

        /// <summary>
        /// Open the Win7 Folder Browser. After a folder is selected, add it to lbFolderList.
        /// </summary>
        private void openFolderBrowser()
        {
            var dialog = new Win7FolderBrowser.FolderSelectDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Title = "Select a Folder"
            };
            if (appSettings.PreviousPath == "")
            {
                dialog.InitialDirectory = Directory.GetCurrentDirectory();
            }
            else
            {
                dialog.InitialDirectory = appSettings.PreviousPath;
            }
            if (dialog.Show(Process.GetCurrentProcess().MainWindowHandle))
            {
                lbFolderList.Items.Add(dialog.FileName);
                appSettings.PreviousPath = dialog.FileName;
                saveFolderPathsList();
            }
        }

        /// <summary>
        /// Populated lbFolderList with folder paths from the saved configuration settings file.
        /// </summary>
        private void populate_lbFolderList()
        {
            for (int x = 0; x < appSettings.FolderPaths.Count; x++)
            {
                lbFolderList.Items.Add(appSettings.FolderPaths[x]);
            }
        }

        /// <summary>
        /// Saves a List<string> object with items from lbFolderList to Settings Class
        /// </summary>
        private void saveFolderPathsList()
        {
            List<String> tempList = new List<String>();

            for (int x = 0; x < lbFolderList.Items.Count; x++)
            {
                tempList.Add(lbFolderList.Items[x].ToString());
            }

            appSettings.FolderPaths = tempList;
        }

        /// <summary>
        /// Sets the radio button for the previous preset setting.
        /// </summary>
        private void setPresetSettingInMainWindow()
        {
            foreach(KeyValuePair<String, String> kvp in appSettings.commands)
            {
                lbSettings.Items.Add(kvp.Key.ToString());
            }

            lbSettings.SelectedItem = appSettings.PresetSetting;
            lbFolderList.SelectedItem = appSettings.PreviousPath;
        }

        
    }
}



