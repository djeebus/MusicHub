using IrcDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    public interface ICommand
    {
        string Command { get; }

        void ExecuteCommand(
            IrcClient client, 
            IIrcMessageSource source, 
            IList<IIrcMessageTarget> targets, 
            string command, 
            IList<string> parameters);
    }
}
