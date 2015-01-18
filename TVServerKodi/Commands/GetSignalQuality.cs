using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
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
            ReceptionDetails details = server.GetReceptionDetails(cardID);

            server.HeartBeat(me);
            writer.write(details.signalLevel.ToString() + "|" + details.signalQuality.ToString());
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
