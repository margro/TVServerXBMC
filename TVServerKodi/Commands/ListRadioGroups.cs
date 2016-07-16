using System;
using System.Collections.Generic;
using System.Text;

namespace TVServerKodi.Commands
{
    class ListRadioGroups : CommandHandler 
    {
        public ListRadioGroups(ConnectionHandler connection)
            : base(connection)
        {
        }
        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            // this needs no arguments, and will just list all the tvgroups

            List<String> groups = TVServerConnection.GetRadioGroups();
            foreach (String group in groups)
            {
                Console.WriteLine("RADIOGROUP : " + group);
            }

            writer.writeList(groups);

        }

        public override string getCommandToHandle()
        {
            return "ListRadioGroups";
        }
    }
}
