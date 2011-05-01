using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
    class GetScheduleCount : CommandHandler
    {
        public GetScheduleCount(ConnectionHandler connection) : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            // we want to list all channels in group arg[0]
            // String group = arguments[0];
            int count = 0;

            count = TVServerConnection.getScheduleCount();

            Console.WriteLine("GetScheduleCount: " + count);

            writer.write(count.ToString());
        }

        public override string getCommandToHandle()
        {
            return "GetScheduleCount";
        }
    }
}
