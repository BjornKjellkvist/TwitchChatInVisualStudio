namespace TwitchInVS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Xml.Linq;

    /// <summary>
    /// Interaction logic for ConfigurationWindowControl.
    /// </summary>
    public partial class ConfigurationWindowControl : UserControl
    {

        public const string ConfigFileName = "TwitchConnection.config";

        public const string BanlistFileName = "IgnoreList.txt";


        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationWindowControl"/> class.
        /// </summary>
        public ConfigurationWindowControl()
        {
            this.InitializeComponent();

            UserName.TextChanged += UserName_TextChanged;

            AccessToken.PasswordChanged += AccessToken_PasswordChanged;

            Channel.TextChanged += Channel_TextChanged;

            if (File.Exists(ConfigFileName))
            {
                ChatWindowControl.TwitchChatReader.confs = LoadSettings();

                UserName.Text = ChatWindowControl.TwitchChatReader.confs.GetSafe("UserName");
                AccessToken.Password = ChatWindowControl.TwitchChatReader.confs.GetSafe("AccesToken");
                Channel.Text = ChatWindowControl.TwitchChatReader.confs.GetSafe("Channel");
                IgnoreCommands.IsChecked = bool.TryParse(ChatWindowControl.TwitchChatReader.confs.GetSafe("IgnoreCommands"), out var ignoreCommands) ? ignoreCommands : false;
            }
            if (File.Exists(BanlistFileName))
            {
                var banlist = LoadBanList();
                BanlistEditor.Text = string.Join(Environment.NewLine, banlist);
                ChatWindowControl.TwitchChatReader.BanList.AddRange(banlist);

            }
        }

        private void AccessToken_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ChatWindowControl.TwitchChatReader.confs.Update("AccesToken", AccessToken.Password);
            ChatWindowControl.TwitchChatReader.AccessToken = AccessToken.Password;
            SaveToFile();
        }

        private void UserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChatWindowControl.TwitchChatReader.confs.Update("UserName", UserName.Text);
            ChatWindowControl.TwitchChatReader.UserName = UserName.Text;
            SaveToFile();
        }

        private void Channel_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChatWindowControl.TwitchChatReader.confs.Update("Channel", Channel.Text);
            ChatWindowControl.TwitchChatReader.Channel = Channel.Text;
            SaveToFile();
        }

        private void SaveToFile()
        {
            new XElement("root", ChatWindowControl.TwitchChatReader.confs.Select(kv => new XElement(kv.Key, kv.Value))).Save(ConfigFileName, SaveOptions.OmitDuplicateNamespaces);
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

        public static Dictionary<string, string> LoadSettings()
        {
            if (File.Exists(ConfigFileName))
            {
                return XElement.Parse(File.ReadAllText(ConfigFileName)).Elements().ToDictionary(k => k.Name.ToString(), v => v.Value.ToString());
            }
            else return new Dictionary<string, string>();
        }

        public static List<string> LoadBanList()
        {
            if (File.Exists(BanlistFileName))
            {
                using (StreamReader stream = new StreamReader(BanlistFileName))
                {
                    var result = stream.ReadToEnd().TrimEnd(' ').Split(Environment.NewLine.ToCharArray());
                    return result.Where(x => !string.IsNullOrEmpty(x)).ToList();
                }
            }
            return new List<string>();
        }

        private void IgnoreCommands_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (IgnoreCommands.IsChecked != null)
            {
                ChatWindowControl.TwitchChatReader.confs.Update("IgnoreCommands", IgnoreCommands.IsChecked.ToString());
                ChatWindowControl.IgnoreCommands = (bool)IgnoreCommands.IsChecked;
                SaveToFile();
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
            if (!File.Exists(BanlistFileName))
            {
                File.Create(BanlistFileName);
            }
            var textValue = AddToBanListValue.Text;
            using (StreamWriter stream = new StreamWriter(BanlistFileName, true))
            {
                stream.WriteLine(textValue);
            }
            BanlistEditor.Text += (Environment.NewLine + textValue);
            ChatWindowControl.TwitchChatReader.BanList.Add(textValue);
        }
    }
}