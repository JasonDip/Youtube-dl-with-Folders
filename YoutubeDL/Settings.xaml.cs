using System.Windows;


namespace DownloaderApp
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void SettingsScreen_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // Check if the name is blank.
            if (tbNewSettingName.Text == "")
            {
                MessageBox.Show("A name for the setting must be provided.");
                return;
            }

            // Check if the command is blank.
            if (tbNewSettingCommand.Text == "")
            {
                MessageBox.Show("A command for the setting must be provided.");
                return;
            }

            // Check if the name (key) already exists.
            if (MainWindow.appSettings.commands.ContainsKey(tbNewSettingName.Text))
            {
                MessageBox.Show("There is already a key with this name.");
                return;
            }

            // Add new setting to main commands dictionary.
            MainWindow.appSettings.commands.Add(tbNewSettingName.Text, tbNewSettingCommand.Text);

            // Close Window
            this.Close();
        }

    }
}
