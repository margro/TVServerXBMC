using System;
using System.Collections.Generic;
using System.Text;

namespace TVServerXBMC.Commands
{
    class Help : CommandHandler
    {
        public Help(ConnectionHandler connection)
            : base(connection)
        {

        }
        /*
         * No arguments needed, will list all commands available
         */
        public override void handleCommand(string myCommand, string[] arguments, ref TvControl.User me)
        {
            // get a list of all commands
            Dictionary<String, CommandHandler> allCommands = this.getConnection().getAllHandlers();

            List<string> results = new List<string>();
            foreach (CommandHandler command in allCommands.Values )
            {
                results.Add(command.getCommandToHandle());
            }
            writer.writeList(results);

            Console.WriteLine("Returned " + results.Count + " commands");
        }

        public override string getCommandToHandle()
        {
            return "Help";
        }
    }
}
