namespace TwitchInVS
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using TwitchInVS.Properties;

    /// <summary>
    /// Interaction logic for ConfigurationWindowControl.
    /// </summary>
    public partial class ConfigurationWindowControl : UserControl
    {

        //public const string ConfigFileName = "TwitchConnection.config";

        //public const string BanlistFileName = "IgnoreList.txt";
        private static object _syncLock = new object();

        ObservableCollection<string> IgnoreListDisplay = new ObservableCollection<string>();


        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationWindowControl"/> class.
        /// </summary>
        public ConfigurationWindowControl()
        {
            this.InitializeComponent();

            UserName.TextChanged += UserName_TextChanged;

            AccessToken.PasswordChanged += AccessToken_PasswordChanged;

            Channel.TextChanged += Channel_TextChanged;

            UserName.Text = Settings.Default.UserName;
            AccessToken.Password = Settings.Default.AccessToken;
            Channel.Text = Settings.Default.Channel;
            IgnoreCommands.IsChecked = Settings.Default.IgnoreCommands;
            if (Settings.Default.IgnoreList == null) Settings.Default.IgnoreList = new System.Collections.Specialized.StringCollection();
            BindingOperations.EnableCollectionSynchronization(IgnoreListDisplay, _syncLock);
            BanlistEditor.ItemsSource = IgnoreListDisplay;
            for (int i = 0; i < Settings.Default.IgnoreList.Count; i++)
            {
                IgnoreListDisplay.Add(Settings.Default.IgnoreList[i]);
            }

        }

        private void AccessToken_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Settings.Default.AccessToken = AccessToken.Password;
            Settings.Default.Save();
        }

        private void UserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.UserName = UserName.Text;
            Settings.Default.Save();

        }

        private void Channel_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Channel = Channel.Text;
            Settings.Default.Save();
        }

        private void IgnoreCommands_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (IgnoreCommands.IsChecked != null)
            {
                Settings.Default.IgnoreCommands = (bool)IgnoreCommands.IsChecked;
                Settings.Default.Save();
                ChatWindowControl.IgnoreCommands = (bool)IgnoreCommands.IsChecked;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ChatWindowControl.TwitchChatReader.CreateClient();
                ChatWindowControl.TwitchChatReader.ConnectAsync();
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Error - Please check your configuration");
            }
        }

        private void Button_ShowBanlistEdit(object sender, RoutedEventArgs e)
        {
            if (BanlistEditorView.Visibility == Visibility.Collapsed)
            {
                BanlistEditorView.Visibility = Visibility.Visible;
                AddToBanListGrid.Visibility = Visibility.Visible;
            }
            else
            {
                BanlistEditorView.Visibility = Visibility.Collapsed;
                AddToBanListGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void AddToBanListButton_Click(object sender, RoutedEventArgs e)
        {
            var textValue = AddToBanListValue.Text;
            AddToBanListValue.Text = string.Empty;
            if (string.IsNullOrEmpty(textValue) || Settings.Default.IgnoreList.Contains(textValue)) return;
            Settings.Default.IgnoreList.Add(textValue);
            Settings.Default.Save();
            IgnoreListDisplay.Add(textValue);
            BanlistEditor.Items.Refresh();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var value = button.Tag as string;
            int IgnoreListIndex = Settings.Default.IgnoreList.IndexOf(value);
            Settings.Default.IgnoreList.RemoveAt(IgnoreListIndex);
            Settings.Default.Save();
            int IgnoreListDisplayIndex = IgnoreListDisplay.IndexOf(value);
            IgnoreListDisplay.RemoveAt(IgnoreListDisplayIndex);
            BanlistEditor.Items.Refresh();
        }
    }
}