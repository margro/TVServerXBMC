using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;
using TvDatabase;
using TvLibrary.Interfaces;

namespace TVServerXBMC.Commands
{
    class Test : CommandHandler
    {
        public Test(ConnectionHandler connection)
            : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.User me)
        {

            //TvEngine.PowerScheduler.WaitableTimer tveps;
            TvBusinessLayer layer = new TvBusinessLayer();
            int schedtype = (int) TvDatabase.ScheduleRecordingType.Once;
            Schedule newSchedule;

            DateTime startTime = new DateTime(2009, 11, 23, 10, 00, 00);
            DateTime endTime = new DateTime(2009,11,23,11,15,00);

            try
            {
                newSchedule = layer.AddSchedule(32, "Marcel test", startTime, endTime, schedtype);
                newSchedule.Persist();
                Console.WriteLine("Schedule added: " + newSchedule.IdChannel.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override string getCommandToHandle()
        {
            return "Test";
        }
    }
}
