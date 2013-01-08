using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp
{
    class Program
    {
        const string Server = "irc.kabbage.com";
        static readonly IrcDotNet.IrcUserRegistrationInfo registration = new IrcDotNet.IrcUserRegistrationInfo
        {
            NickName = "music-bot",
            RealName = "music-bot",
            UserName = "music-bot",
        };

        static void Main(string[] args)
        {
            var kernel = new StandardKernel(
                new MusicHubNinjectModule());

            var bot = kernel.Get<MusicHubBot>();
            bot.ClientDisconnect += bot_ClientDisconnect;

            bot.Connect(
                Server,
                registration);

            Console.WriteLine("Done!");

            Console.ReadLine();
        }

        static void bot_ClientDisconnect(object sender, EventArgs e)
        {
            var bot = sender as MusicHubBot;
            if (bot == null)
                throw new ArgumentOutOfRangeException("sender", sender, "Unknown sender");

            bot.Connect(
                Server,
                registration);
            
        }
    }
}
