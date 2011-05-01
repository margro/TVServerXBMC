using System;
using System.Collections.Generic;
using System.Text;

namespace TVServerXBMC.Commands
{
    class DeleteSchedule : CommandHandler
    {
        public DeleteSchedule(ConnectionHandler connection)
            : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            if (arguments == null)
            {
                getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle() + ":ScheduleId");
                return;
            }

            int schedId = int.Parse(arguments[0]);

            bool result = TVServerConnection.DeleteSchedule(schedId);
            Console.WriteLine("DeleteSchedule result : " + result.ToString());
            writer.write(result.ToString());
        }

        public override string getCommandToHandle()
        {
            return "DeleteSchedule";
        }
    }
}
