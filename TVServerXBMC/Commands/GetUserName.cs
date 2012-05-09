using System;
using System.Collections.Generic;
using System.Text;
using TVServerXBMC;

namespace TVServerXBMC.Commands
{
   class GetUserName : CommandHandler
   {
       public GetUserName(ConnectionHandler connection)
         : base(connection)
      {

      }

      public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
          writer.write(me.Name);
      }

      public override string getCommandToHandle()
      {
        return "GetUserName";
      }
   }
}
