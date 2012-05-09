using System;
using System.Collections.Generic;
using System.Text;
using TVServerXBMC;

namespace TVServerXBMC.Commands
{
   class UpdateSchedule : CommandHandler
   {
      public UpdateSchedule(ConnectionHandler connection)
         : base(connection)
      {

      }

      public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
          // we want to list all channels in group arg[0]
          if ((arguments != null) && (arguments.Length >= 16))
          {
              Int32 preRecordInterval = -1;
              Int32 postRecordInterval = -1;
              int scheduleType = (int)TvDatabase.ScheduleRecordingType.Once;
              int priority = -1;   // Use MediaPortal default
              int keepmethod = -1; // Use MediaPortal default
              DateTime keepdate;

              int schedindex = int.Parse(arguments[0]);
              int active = int.Parse(arguments[1]);
              int channelid = int.Parse(arguments[2]);
              String title = arguments[3];

              int year = int.Parse(arguments[4]);
              int month = int.Parse(arguments[5]);
              int day = int.Parse(arguments[6]);
              int hour = int.Parse(arguments[7]);
              int min = int.Parse(arguments[8]);
              int sec = int.Parse(arguments[9]);
              DateTime starttime = new DateTime(year, month, day, hour, min, sec);

              year = int.Parse(arguments[10]);
              month = int.Parse(arguments[11]);
              day = int.Parse(arguments[12]);
              hour = int.Parse(arguments[13]);
              min = int.Parse(arguments[14]);
              sec = int.Parse(arguments[15]);
              DateTime endtime = new DateTime(year, month, day, hour, min, sec);

              if (arguments.Length >= 25)
              {
                  scheduleType = int.Parse(arguments[16]);
                  priority = int.Parse(arguments[17]);
                  keepmethod = int.Parse(arguments[18]);

                  year = int.Parse(arguments[19]);
                  month = int.Parse(arguments[20]);
                  day = int.Parse(arguments[21]);
                  hour = int.Parse(arguments[22]);
                  min = int.Parse(arguments[23]);
                  sec = int.Parse(arguments[24]);
                  keepdate = new DateTime(year, month, day, hour, min, sec);

                  preRecordInterval = Int32.Parse(arguments[25]);
                  postRecordInterval = Int32.Parse(arguments[26]);
              }
              else
              {
                  keepdate = new DateTime(2000, 01, 01, 0, 0, 0); //MediaPortal default value 2000-01-01 00:00:00
              }

              bool result = TVServerConnection.UpdateSchedule(schedindex, channelid, active, title, starttime, endtime, scheduleType, priority, keepmethod, keepdate, preRecordInterval, postRecordInterval);
              Console.WriteLine("UpdateSchedule result : " + result.ToString());
              writer.write(result.ToString());
          }
          else
          {
              getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle()
                  + ":ScheduleIndex|ChannelId|Active|Title|Y|M|D|H|m|s|Y|M|D|H|m|s[|SchedType|Priority|KeepMethod|Y|M|D|h|m|s|PreRecInt|PostRecInt] (16 or 25 arguments)");
          }
      }

      public override string getCommandToHandle()
      {
          return "UpdateSchedule";
      }
   }
}
