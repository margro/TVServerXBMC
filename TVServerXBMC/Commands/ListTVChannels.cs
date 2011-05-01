using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
    class ListTVChannels : CommandHandler
    {
        public ListTVChannels(ConnectionHandler connection)
            : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            String group;

            if (arguments != null)
            {   //we want to list all channels in group arg[0]
                group = arguments[0];
            } else {
                group = "";
            }

            Console.WriteLine("TV channels from group:" + group);
            
            List<string> tvchannels = TVServerConnection.GetTVChannels(group);

            foreach (string channel in tvchannels)
            {
                Console.WriteLine(channel);
            }

            writer.writeList(tvchannels);
        }

        public override string getCommandToHandle()
        {
            return "ListTVChannels";
        }
    }
}
