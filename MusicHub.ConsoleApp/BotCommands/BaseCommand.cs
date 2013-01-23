using IrcDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    public abstract class BaseCommand : ICommand
    {
        public abstract string Command
        {
            get;
        }

        public abstract void ExecuteCommand(
            IrcClient client, 
            IIrcMessageSource source, 
            IList<IIrcMessageTarget> targets, 
            string command, 
            IList<string> parameters);

        const char Delimiter = '"';
        protected IList<string> NormalizeParameters(IList<string> parameters)
        {
            var list = new List<string>();
            string partial = null;
            foreach (var p in parameters)
            {
                if (string.IsNullOrWhiteSpace(p))
                    continue;


                if (partial != null)
                {
                    partial += " " + p;

                    if (partial.Last() == Delimiter)
                    {
                        list.Add(partial.Substring(0, partial.Length - 1));
                        partial = null;
                    }
                    continue;
                }

                if (p[0] == Delimiter)
                {
                    if (p.Last() != Delimiter)
                    {
                        partial = p.Substring(1);
                        continue;
                    }

                    list.Add(p.Substring(1, p.Length - 2));
                    continue;
                }

                list.Add(p);
            }

            if (partial != null)
                throw new ArgumentOutOfRangeException("parameters", partial, "Missing final delimiter for argument");

            return list;
        }

        protected void InvalidSyncLibraryCommand(
            IrcClient client, 
            IList<IIrcMessageTarget> replyTarget, 
            string commandError)
        {
            client.LocalUser.SendMessage(replyTarget, string.Format("Invalid .sync-command: {0}", commandError));
            client.LocalUser.SendMessage(replyTarget, "Usage: .sync-library library-guid");
        }
    }
}
