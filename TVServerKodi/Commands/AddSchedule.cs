using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
{
   class AddSchedule : CommandHandler
   {
      public AddSchedule(ConnectionHandler connection)
         : base(connection)
      {

      }

      public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
          // we want to list all channels in group arg[0]
          if ((arguments != null) && (arguments.Length >= 14))
          {
              Int32 preRecordInterval = -1;
              Int32 postRecordInterval = -1;
              int scheduleType = (int)TvDatabase.ScheduleRecordingType.Once;
              int priority = -1;   // Use MediaPortal default
              int keepmethod = -1; // Use MediaPortal default
              DateTime keepdate;

              int channelid = int.Parse(arguments[0]);
              String title = arguments[1];

              int year = int.Parse(arguments[2]);
              int month = int.Parse(arguments[3]);
              int day = int.Parse(arguments[4]);
              int hour = int.Parse(arguments[5]);
              int min = int.Parse(arguments[6]);
              int sec = int.Parse(arguments[7]);
              DateTime starttime = new DateTime(year, month, day, hour, min, sec);

              year = int.Parse(arguments[8]);
              month = int.Parse(arguments[9]);
              day = int.Parse(arguments[10]);
              hour = int.Parse(arguments[11]);
              min = int.Parse(arguments[12]);
              sec = int.Parse(arguments[13]);
              DateTime endtime = new DateTime(year, month, day, hour, min, sec);

              if (arguments.Length >= 25)
              {
                  scheduleType = int.Parse(arguments[14]);
                  priority = int.Parse(arguments[15]);
                  keepmethod = int.Parse(arguments[16]);

                  year = int.Parse(arguments[17]);
                  month = int.Parse(arguments[18]);
                  day = int.Parse(arguments[19]);
                  hour = int.Parse(arguments[20]);
                  min = int.Parse(arguments[21]);
                  sec = int.Parse(arguments[22]);
                  keepdate = new DateTime(year, month, day, hour, min, sec);

                  preRecordInterval = Int32.Parse(arguments[23]);
                  postRecordInterval = Int32.Parse(arguments[24]);
              }
              else
              {
                  keepdate = new DateTime(2000, 01, 01, 0, 0, 0); //MediaPortal default value 2000-01-01 00:00:00
              }

              bool result = TVServerConnection.AddSchedule(channelid, title, starttime, endtime, scheduleType, priority, keepmethod, keepdate, preRecordInterval, postRecordInterval);
              Console.WriteLine("AddSchedule result : " + result.ToString());
              writer.write(result.ToString());
          }
          else
          {
              getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle() +
                  ":ChannelId|Title|Y|M|D|H|m|s|Y|M|D|H|m|s|preMin|postMin|schedType");
          }
      }

      public override string getCommandToHandle()
      {
         return "AddSchedule";
      }
   }
}
