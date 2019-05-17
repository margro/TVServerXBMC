using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
{
    class ListTVChannels : CommandHandler
    {
        public ListTVChannels(ConnectionHandler connection)
            : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            List<string> groups;

            if ((arguments != null) && (arguments[0].Length>0))
            {   //we want to list all channels in group arg[0]
                groups = new List<string>(arguments);
            }
            else
            {
                groups = new List<string>();
            }
            
            List<string> tvchannels = TVServerConnection.GetTVChannels(groups);

            if (tvchannels != null)
            {
                foreach (string channel in tvchannels)
                {
                    Console.WriteLine(channel);
                }
                writer.writeList(tvchannels);
            }
            else
            {
                getConnection().WriteLine("[ERROR]: No TV channels found");
            }
        }

        public override string getCommandToHandle()
        {
            return "ListTVChannels";
        }
    }
}
