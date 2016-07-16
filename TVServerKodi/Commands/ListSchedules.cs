using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
{
   class ListSchedules : CommandHandler
   {
      public ListSchedules(ConnectionHandler connection)
         : base(connection)
      {

      }

      public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
          try
          {
              bool seriesSupport = false;
              if (arguments != null && arguments.Length >= 1)
              {
                  if (arguments[0].Length > 0)
                  {
                      seriesSupport = Boolean.Parse(arguments[0]);
                  }
              }
              
              // we want to list all scheduled recordings
              List<string> results = new List<string>();
              
              results = TVServerConnection.GetSchedules(seriesSupport);
              writer.writeList(results);
            } catch {
              getConnection().WriteLine("[ERROR]: Usage: " + getCommandToHandle() + ":[seriesSupport=False]" );
         }
      }

      public override string getCommandToHandle()
      {
          return "ListSchedules";
      }
   }
}
