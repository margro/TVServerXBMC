using System;
using System.Collections.Generic;
using System.Text;

namespace TVServerXBMC.Commands
{
    class ListGroups : CommandHandler 
    {
        public ListGroups(ConnectionHandler connection)
            : base(connection)
        {
        }
        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            // this needs no arguments, and will just list all the tvgroups

            List<String> groups = TVServerConnection.GetTVGroups();
            foreach (String group in groups)
            {
                Console.WriteLine("GROUP : " + group);
            }

            writer.writeList(groups);

        }

        public override string getCommandToHandle()
        {
            return "ListGroups";
        }
    }
}
