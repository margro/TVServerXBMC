using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
    class GetDriveSpace : CommandHandler
    {
        public GetDriveSpace(ConnectionHandler connection) : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            string result = TVServerConnection.GetRecordingDriveSpace();

            writer.write(result);
        }

        public override string getCommandToHandle()
        {
            return "GetDriveSpace";
        }
    }
}
