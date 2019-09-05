using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace DownloaderApp
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void btnUpdateYoutubedl_Click(object sender, RoutedEventArgs e)
        {
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", "/K cd " + userPath + " && " + "youtube-dl -U");

            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = false;
            Process process = new Process();
            process.StartInfo = processStartInfo;

            bool processStarted = process.Start();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
