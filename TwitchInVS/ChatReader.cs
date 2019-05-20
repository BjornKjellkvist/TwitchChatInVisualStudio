using System;
using System.Collections.Generic;
using TwitchChatSharp;

namespace TwitchInVS
{
    public class ChatReader
    {
        public TwitchConnection client;

        public string UserName { get; set; }

        public string AccessToken { get; set; }

        public string Channel { get; set; }

        public bool IgnoreCommands { get; set; }

        public Dictionary<string, string> confs = new Dictionary<string, string>();

        public EventHandler<IrcConnectedEventArgs> Connceted;

        public void OnConnected(object sender, IrcConnectedEventArgs e)
        {
            Connceted?.Invoke(sender, e);
        }

        public EventHandler<IrcMessageEventArgs> MessageReceived;

        public void OnMessageReceived(object sender, IrcMessageEventArgs e)
        {
            MessageReceived?.Invoke(sender, e);
        }

        //TODO
        public List<string> BanList => new List<string>();

        public ChatReader()
        {
            var dict = ConfigurationWindowControl.LoadSettings();
            UserName = dict.GetSafe("UserName");
            AccessToken = dict.GetSafe("AccesToken");
            Channel = dict.GetSafe("Channel");
            IgnoreCommands = bool.TryParse(dict.GetSafe("IgnoreCommands"), out var ignoreCommands) ? ignoreCommands : false;
        }

        public ChatReader(string userName, string accessToken, string channel)
        {
            UserName = userName;
            AccessToken = accessToken;
            Channel = channel;
        }

        public void CreateClient()
        {
            if (client != null)
            {
                client.Disconnect();
                client.MessageReceived -= OnMessageReceived;
                client.Connected -= OnConnected;
            }
            client = new TwitchConnection(
                cluster: ChatEdgeCluster.Aws,
                nick: UserName,
                oauth: AccessToken, // no oauth: prefix
                port: 6697,
                capRequests: new string[] { "twitch.tv/tags", "twitch.tv/commands" },
                ratelimit: 1500,
                secure: true
                );
            client.MessageReceived += OnMessageReceived;
            client.Connected += OnConnected;
        }

        public void ConnectAsync()
        {
            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(Channel))
            {
                client.ConnectAsync();
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
