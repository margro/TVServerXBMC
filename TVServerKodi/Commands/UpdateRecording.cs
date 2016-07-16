using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
{
   class UpdateRecording : CommandHandler
   {
     public UpdateRecording(ConnectionHandler connection)
         : base(connection)
      {

      }

     public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
          // we want to list all channels in group arg[0]
          if ((arguments != null) && (arguments.Length == 2))
          {
              int recindex = int.Parse(arguments[0]);
              String name = arguments[1];

              bool result = TVServerConnection.UpdateRecording(recindex, name);
              Console.WriteLine("UpdateRecording result : " + result.ToString());
              writer.write(result.ToString());
          }
          else
          {
              getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle() + ":RecordingIndex|NewName");
          }
      }

      public override string getCommandToHandle()
      {
        return "UpdateRecording";
      }
   }
}
