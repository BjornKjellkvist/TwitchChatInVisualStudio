namespace TwitchInVS
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Windows.Controls;
    using TwitchChatSharp;
    using TwitchInVS.Properties;

    /// <summary>
    /// Interaction logic for ChatWindowControl.
    /// </summary>
    public partial class ChatWindowControl : UserControl
    {

        public static ChatReader TwitchChatReader;

        public static bool IgnoreCommands;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatWindowControl"/> class.
        /// </summary>
        public ChatWindowControl()
        {
            this.InitializeComponent();
            TwitchChatReader = new ChatReader();
            TwitchChatReader.MessageReceived += MessageReceived;
            TwitchChatReader.Connceted += OnConnect;
            if (!string.IsNullOrEmpty(Settings.Default.UserName) && !string.IsNullOrEmpty(Settings.Default.AccessToken) && !string.IsNullOrEmpty(Settings.Default.Channel))
            {
                TwitchChatReader.CreateClient();
                ChannelName.Text = Settings.Default.Channel;
                try
                {
                    AddLineToChat("Trying to connect");
                    TwitchChatReader.ConnectAsync();
                }
                catch (ArgumentException)
                {
                    AddLineToChat("Cannot connect, please check your configuration! (Click the icon on the top right)");
                }
            }
            else
            {
                AddLineToChat("Cannot connect, please check your configuration! (Click the icon on the top right)");
            }
        }

        private void AddLineToChat(string line)
        {
            Dispatcher.Invoke(() =>
            {
                ChatWindow.Text = line + Environment.NewLine + ChatWindow.Text;
            });
        }

        public void OnConnect(object sender, IrcConnectedEventArgs e)
        {
            {
                TwitchChatReader.client.JoinChannel("#" + Settings.Default.Channel);
                ChannelName.Text = Settings.Default.Channel;
                AddLineToChat($"Connected to chat channel {Settings.Default.Channel}!");
            };
        }

        public void MessageReceived(object sender, IrcMessageEventArgs e)
        {
            if (MessageVerified(e.Message))
            {
                AddLineToChat(e.Message.User + ": " + e.Message.Message);
            }
        }

        private bool MessageVerified(IrcMessage message)
        {
            return !message.User.EndsWith("tmi.twitch.tv") &&
                !string.IsNullOrEmpty(message.Message) &&
                !string.IsNullOrEmpty(message.User) &&
                !Settings.Default.IgnoreList.Contains(message.User) &&
                !(IgnoreCommands && message.Message.StartsWith("!"));
        }

        private void ConfigButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFromPackage();
        }

        private void OpenFromPackage()
        {
            var inst = ConfigurationWindowCommand.Instance;
            Dispatcher.VerifyAccess();
            ToolWindowPane window = inst.package.FindToolWindow(typeof(ConfigurationWindow), 0, true); // True means: crate if not found. 0 means there is only 1 instance of this tool window
            if (null == window || null == window.Frame)
                throw new NotSupportedException("MyToolWindow not found");

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}