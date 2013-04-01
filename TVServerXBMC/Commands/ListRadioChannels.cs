using System;
using System.Collections.Generic;
using System.Text;
using TVServerXBMC;

namespace TVServerXBMC.Commands
{
    class ListRadioChannels : CommandHandler
    {
        public ListRadioChannels(ConnectionHandler connection) : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            List<string> groups;

            if ((arguments != null) && (arguments[0].Length > 0))
            {   //we want to list all channels in group arg[0]
                groups = new List<string>(arguments);
            }
            else
            {
                groups = new List<string>();
            }
           
            List<string> radiochannels = TVServerConnection.GetRadioChannels(groups);

            foreach (string channel in radiochannels)
            {
                Console.WriteLine(channel);
            }

            writer.writeList(radiochannels);
        }

        public override string getCommandToHandle()
        {
            return "ListRadioChannels";
        }
    }
}
