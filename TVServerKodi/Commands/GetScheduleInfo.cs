using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
{
   class GetScheduleInfo : CommandHandler
   {
       public GetScheduleInfo(ConnectionHandler connection)
         : base(connection)
      {

      }

       public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
        String result;

        if (arguments != null)
        {
            try
            {
                int index = Int32.Parse(arguments[0]);
                bool useUriEncoding = false;

                if (arguments.Length >= 2)
                {
                    useUriEncoding = bool.Parse(arguments[1]);
                }

                result = TVServerConnection.GetScheduleInfo(index);
                Console.WriteLine("GetScheduleInfo:" + index + " " + result);

                writer.write(result, useUriEncoding);
            }
            catch
            {
                Console.WriteLine("GetScheduleInfo: failed");
                writer.write("");
            }
        }
        else
        {
            getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle() + ":ScheduleID");
        }
      }

      public override string getCommandToHandle()
      {
          return "GetScheduleInfo";
      }
   }
}
