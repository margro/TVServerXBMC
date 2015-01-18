using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
{
    class GetRecordingCount : CommandHandler
    {
        public GetRecordingCount(ConnectionHandler connection) : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            // we want to list all channels in group arg[0]
            // String group = arguments[0];
            int count = 0;

            count = TVServerConnection.getRecordingCount();

            Console.WriteLine("GetRecordingCount: " + count);

            writer.write(count.ToString());
        }

        public override string getCommandToHandle()
        {
            return "GetRecordingCount";
        }
    }
}
