using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
   class ListRecordings : CommandHandler
   {
      public ListRecordings(ConnectionHandler connection)
         : base(connection)
      {

      }

      public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
        // we want to list all recordings
        List<string> results = new List<string>();
        bool resolveToIP = true;
        string originalURL = "";

        if (arguments != null && arguments.Length == 1)
        {
            resolveToIP = Boolean.Parse(arguments[0]);
        }

        results = TVServerConnection.GetRecordings(resolveToIP, ref originalURL);
        writer.writeList(results);
      }

      public override string getCommandToHandle()
      {
        return "ListRecordings";
      }
   }
}
