namespace TwitchInVS
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Windows.Controls;
    using TwitchChatSharp;

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
            if (!string.IsNullOrEmpty(TwitchChatReader.UserName) && !string.IsNullOrEmpty(TwitchChatReader.AccessToken) && !string.IsNullOrEmpty(TwitchChatReader.Channel))
            {
                TwitchChatReader.CreateClient();
                ChannelName.Text = TwitchChatReader.Channel;
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
                TwitchChatReader.client.JoinChannel("#" + TwitchChatReader.Channel);
                AddLineToChat($"Connected to chat channel {TwitchChatReader.Channel}!");
            };
        }

        public void MessageReceived(object sender, IrcMessageEventArgs e)
        {
            if (e.Message.User.EndsWith("tmi.twitch.tv") || 
                string.IsNullOrEmpty(e.Message.Message.TrimEnd(' ')) || 
                TwitchChatReader.BanList.Contains(e.Message.User) || 
                (IgnoreCommands && e.Message.Message.StartsWith("!"))) return;
            AddLineToChat(e.Message.User + ": " + e.Message.Message);
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