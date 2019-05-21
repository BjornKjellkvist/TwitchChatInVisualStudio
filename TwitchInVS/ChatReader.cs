using System;
using System.Collections.Generic;
using TwitchChatSharp;
using TwitchInVS.Properties;

namespace TwitchInVS
{
    public class ChatReader
    {
        public TwitchConnection client;

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

        public ChatReader()
        {
            //UserName = Settings.Default.UserName;
            //AccessToken = Settings.Default.AccessToken;
            //Channel = Settings.Default.Channel;
            //IgnoreCommands = Settings.Default.IgnoreCommands;
        }

        public ChatReader(string userName, string accessToken, string channel)
        {
            //UserName = userName;
            //AccessToken = accessToken;
            //Channel = channel;
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
                nick: Settings.Default.UserName,
                oauth: Settings.Default.AccessToken, // no oauth: prefix
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
            if (!string.IsNullOrEmpty(Settings.Default.UserName) && !string.IsNullOrEmpty(Settings.Default.AccessToken) && !string.IsNullOrEmpty(Settings.Default.Channel))
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
