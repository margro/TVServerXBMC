using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
{
    class GetChannelCount : CommandHandler
    {
        public GetChannelCount(ConnectionHandler connection) : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            String group;
            int count = 0;

            if (arguments != null)
            {   //we want to list all channels in group arg[0]
                group = arguments[0];
            }
            else
            {
                group = "";
            }

            count = TVServerConnection.getChannelCount(group);

            Console.WriteLine("GetChannelCount: " + count);

            writer.write(count.ToString());
        }

        public override string getCommandToHandle()
        {
            return "GetChannelCount";
        }
    }
}
