using System;
using System.Collections.Generic;
using System.Text;
using TVServerXBMC;

namespace TVServerXBMC.Commands
{
   class GetRecordingStopTime : CommandHandler
   {
     public GetRecordingStopTime(ConnectionHandler connection)
         : base(connection)
      {

      }

     public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
          if ((arguments != null) && (arguments.Length == 1))
          {
            int recindex = int.Parse(arguments[0]);

            int result = TVServerConnection.GetRecordingStopTime(recindex);
            Console.WriteLine("GetRecordingStopTime result : " + result.ToString());
            writer.write(result.ToString());
          }
          else
          {
            getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle() + ":RecordingIndex");
          }
      }

      public override string getCommandToHandle()
      {
        return "GetRecordingStopTime";
      }
   }
}
