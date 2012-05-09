using System;
using System.Collections.Generic;
using System.Text;
using TVServerXBMC;

namespace TVServerXBMC.Commands
{
  class GetCardSettings : CommandHandler
   {
       public GetCardSettings(ConnectionHandler connection)
         : base(connection)
      {

      }

      public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
        int cardID = -1;
        List<string> results = new List<string>();

        try
        {
            if (arguments != null)
            {
                cardID = Int32.Parse(arguments[0]);
            }

            TVServerController server = TVServerConnection.GetServerInterface();
            results = server.GetCardSettings(cardID);

            writer.writeList(results);
        }
        catch
        {
            Console.WriteLine(getCommandToHandle() + ": failed");
            writer.write("");
        }
        //getConnection().WriteLine("[ERROR]: Expected format: " + getCommandToHandle() + "[:cardID]");
      }

      public override string getCommandToHandle()
      {
        return "GetCardSettings";
      }
   }
}
