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
        static void Main(string[] args)
        {
            var kernel = new StandardKernel(
                new MusicHubNinjectModule());

            var bot = kernel.Get<MusicHubBot>();

            bot.Connect(
                "irc.kabbage.com",
                new IrcDotNet.IrcUserRegistrationInfo
                {
                    NickName = "music-bot",
                    RealName = "music-bot",
                    UserName = "music-bot",
                });

            Console.WriteLine("Done!");

            Console.ReadLine();
        }
    }
}
