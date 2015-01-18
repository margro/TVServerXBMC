using System;
using System.Collections.Generic;
using System.Text;

namespace TVServerKodi.Commands
{
    class CloseConnection : CommandHandler
    {
        
        public CloseConnection(ConnectionHandler connection) : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            // does not matter what arguments it has.

            getConnection().Disconnect();
        }

        public override string getCommandToHandle()
        {
            return "CloseConnection";
        }

    }
}
