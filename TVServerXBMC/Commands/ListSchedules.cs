using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
   class ListSchedules : CommandHandler
   {
      public ListSchedules(ConnectionHandler connection)
         : base(connection)
      {

      }

      public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
         // we want to list all recordings
         List<string> results = new List<string>();

         results = TVServerConnection.GetSchedules();
         writer.writeList(results);
      }

      public override string getCommandToHandle()
      {
         return "ListSchedules";
      }
   }
}
