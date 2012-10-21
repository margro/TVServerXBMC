using System;
using System.Collections.Generic;
using System.Text;
using TVServerXBMC;

namespace TVServerXBMC.Commands
{
   class SetRecordingTimesWatched : CommandHandler
   {
     public SetRecordingTimesWatched(ConnectionHandler connection)
         : base(connection)
      {

      }

     public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
          // we want to list all channels in group arg[0]
          if ((arguments != null) && (arguments.Length == 2))
          {
              int recindex = int.Parse(arguments[0]);
              int count = int.Parse(arguments[1]);

              bool result = TVServerConnection.SetRecordingTimesWatched(recindex, count);
              Console.WriteLine("SetRecordingTimesWatched result : " + result.ToString());
              writer.write(result.ToString());
          }
          else
          {
              getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle() + ":RecordingIndex|timesWatched");
          }
      }

      public override string getCommandToHandle()
      {
        return "SetRecordingTimesWatched";
      }
   }
}
