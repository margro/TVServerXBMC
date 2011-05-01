using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
    class GetBackendName : CommandHandler
    {
        public GetBackendName(ConnectionHandler connection)
            : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            writer.write(TVServerConnection.getBackendName());
        }

        public override string getCommandToHandle()
        {
            return "GetBackendName";
        }
    }
}
