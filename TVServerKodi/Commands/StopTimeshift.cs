using System;
using System.Collections.Generic;
using System.Text;

namespace TVServerKodi.Commands
{
    class StopTimeshift : CommandHandler
    {
        public StopTimeshift(ConnectionHandler connection)
            : base(connection)
        {

        }
        /*
         * No arguments needed
         */
        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            bool result = TVServerConnection.StopTimeshift(ref me);
            Console.WriteLine("StopTimeshift result: " + result.ToString());
            writer.write(result.ToString());
        }

        public override string getCommandToHandle()
        {
            return "StopTimeshift";
        }
    }
}
