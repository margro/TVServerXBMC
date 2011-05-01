using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
    class ListRadioChannels : CommandHandler
    {
        public ListRadioChannels(ConnectionHandler connection) : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            String group;

            if (arguments != null)
            {   //we want to list all channels in group arg[0]
                group = arguments[0];
            }
            else
            {
                group = "";
            }

            Console.WriteLine("Radio channels from group:" + group);
            
            List<string> radiochannels = TVServerConnection.GetRadioChannels(group);

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
