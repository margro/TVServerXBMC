using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
   class SetUserName : CommandHandler
   {
       public SetUserName(ConnectionHandler connection)
         : base(connection)
      {

      }

      public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
          // we want to list all channels in group arg[0]
          if ((arguments != null) && (arguments.Length == 1))
          {
              try
              {
                  String name = arguments[0];
                  me = TVServerConnection.RequestUser(name);
                  writer.write("True");
              }
              catch
              {
                  writer.write("[ERROR]");
              }
          }
          else
          {
              getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle() + ":Name");
          }
      }

      public override string getCommandToHandle()
      {
        return "SetUserName";
      }
   }
}
