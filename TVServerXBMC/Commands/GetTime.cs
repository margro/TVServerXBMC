using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
    class GetTime : CommandHandler
    {
        public GetTime(ConnectionHandler connection) : base(connection)
        {
        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.User me)
        {
          DateTime localtime = DateTime.Now;
          DateTime utctime = DateTime.UtcNow;
          TimeSpan utcdiff = (localtime - utctime);

          Console.WriteLine("Local time: " + localtime + " UTC time: " + utctime + " Offset: " + utcdiff.Hours);
          //Format: dd-mm-yyyy hh:mm:ss;offset hours;offset seconds
          writer.write(writer.makeItem(localtime.ToString("u"),
              utcdiff.Hours.ToString(),
              utcdiff.Minutes.ToString()));
          
        }

        public override string getCommandToHandle()
        {
            return "GetTime";
        }
    }
}
