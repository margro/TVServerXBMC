using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
{
   class GetRecordingInfo : CommandHandler
   {
       public GetRecordingInfo(ConnectionHandler connection)
         : base(connection)
      {

      }

      public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
        // Fetch the info for a particular recording
        String result;

        if (arguments != null)
        {
            try
            {
                int index = Int32.Parse(arguments[0]);
                bool withRTSPurl = false;

                if (arguments.Length >= 2)
                {
                    withRTSPurl = bool.Parse(arguments[1]);
                }

                result = TVServerConnection.getRecordingInfo(index, withRTSPurl);
                Console.WriteLine(getCommandToHandle() + ":" + index + " " + result);

                writer.write(result);
            }
            catch
            {
                Console.WriteLine(getCommandToHandle() + ": failed");
                writer.write("");
            }
        }
        else
        {
            getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle() + ":RecordingID[|withRTSPurl=False]");
        }
      }

      public override string getCommandToHandle()
      {
          return "GetRecordingInfo";
      }
   }
}
