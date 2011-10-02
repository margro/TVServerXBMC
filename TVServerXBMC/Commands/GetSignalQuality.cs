using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
    class GetSignalQuality : CommandHandler
    {
      public GetSignalQuality(ConnectionHandler connection)
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
            else
            {
              cardID = me.CardId;
            }

            TVServerController server = TVServerConnection.GetServerInterface();
            int level = server.GetSignalLevel(cardID);
            int quality = server.GetSignalQuality(cardID);

            writer.write(level.ToString() + "|" + quality.ToString());
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
          return "GetSignalQuality";
        }
    }
}
