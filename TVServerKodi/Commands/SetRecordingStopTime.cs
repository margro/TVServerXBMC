using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
{
   class SetRecordingStopTime : CommandHandler
   {
     public SetRecordingStopTime(ConnectionHandler connection)
         : base(connection)
      {

      }

     public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
          if ((arguments != null) && (arguments.Length == 2))
          {
            int recindex = int.Parse(arguments[0]);
            int stopTime = int.Parse(arguments[1]);

            bool result = TVServerConnection.SetRecordingStopTime(recindex, stopTime);
            Console.WriteLine("SetRecordingStopTime result : " + result.ToString());
            writer.write(result.ToString());
          }
          else
          {
            getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle() + ":RecordingIndex|stopTime");
          }
      }

      public override string getCommandToHandle()
      {
        return "SetRecordingStopTime";
      }
   }
}
