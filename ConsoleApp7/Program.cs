using System;
using VkNet;
using System.IO;
using VkNet.Model;
using System.Text;
using System.Net;
using VkNet.Model.RequestParams;
using System.Threading;

namespace ConversationBot
{
    class Program
    {
        private static BotManager botManager = new BotManager();

        static void Main(string[] args)
        {
            ActivateBot();
            while (true)
            {
                switch (Console.ReadLine().ToLower())
                {
                    case "exit": Environment.Exit(0); break;
                    default: break;
                }
            }
        }

        public static void ActivateBot()
        {
            botManager.StartBot();
        }
    }
}
