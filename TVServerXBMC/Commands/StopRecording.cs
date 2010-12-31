using System;
using System.Collections.Generic;
using System.Text;

namespace TVServerXBMC.Commands
{
    class StopRecording : CommandHandler
    {
        public StopRecording(ConnectionHandler connection)
            : base(connection)
        {

        }
        /*
         * 1 optional argument: id
         */
        public override void handleCommand(string command, string[] arguments, ref TvControl.User me)
        {
            int schedId = -1;

            if ((arguments != null) && (arguments.Length == 1))
            {
                schedId = int.Parse(arguments[0]);
            }

            bool result = TVServerConnection.StopRecording(schedId);
            Console.WriteLine("StopRecording result : " + result.ToString());
            writer.write(result.ToString());
        }

        public override string getCommandToHandle()
        {
            return "StopRecording";
        }
    }
}
