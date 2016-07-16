using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
{
   class GetUserName : CommandHandler
   {
       public GetUserName(ConnectionHandler connection)
         : base(connection)
      {

      }

      public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
          bool timeshiftingUsers = false;
          List<string> results = new List<string>();

          if (arguments != null && arguments.Length == 1)
          {
              if (arguments[0] == "TimeshiftingUsers")
                  timeshiftingUsers = true;
          }

          if (timeshiftingUsers)
          {
              results = TVServerConnection.getTimeshiftingUserNames();
          }
          else
          {
              results.Add(me.Name);
          }

          writer.writeList(results);
      }

      public override string getCommandToHandle()
      {
        return "GetUserName";
      }
   }
}
