using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
    class GetVersion : CommandHandler
    {
        public GetVersion(ConnectionHandler connection) : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            string VersionNr = "1.0.0";

            // Fetch the MediaPortal TVServer version
            VersionNr = TVServerConnection.GetVersion();

            writer.write(VersionNr);
        }

        public override string getCommandToHandle()
        {
            return "GetVersion";
        }
    }
}
