using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
    class GetEPG : CommandHandler
    {
        public GetEPG(ConnectionHandler connection)
            : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            // we want to get the EPG info for that channel

            List<string> results = new List<string>();

            if (arguments == null || arguments.Length < 1)
            {
                writer.write("[ERROR]: Usage: " + getCommandToHandle() + ":chanNr");
            }
            else
            {
                string channel = arguments[0];

                if (channel != "")
                {
                    List<EPGInfo> epgs = TVServerConnection.getEpg(int.Parse(arguments[0]));

                    foreach (EPGInfo e in epgs)
                    {
                        string epg = e.startTime.ToString("u") + "|"
                          + e.endTime.ToString("u") + "|"
                          + e.title.Replace("|", "") + "|"
                          + e.description.Replace("|", "") + "|"
                          + e.genre;
                        results.Add(epg);
                    }
                    writer.writeList(results);
                }
                else
                {
                    writer.write("[ERROR]: Usage: " + getCommandToHandle() + ":chanNr");
                }
            }
        }

        public override string getCommandToHandle()
        {
            return "GetEPG";
        }
    }
}
