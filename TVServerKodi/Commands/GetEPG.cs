using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
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
                DateTime starttime;
                DateTime endtime;
                string channel = arguments[0];
                
                if (arguments.Length>=3)
                {
                    starttime = DateTime.Parse(arguments[1]);
                    endtime = DateTime.Parse(arguments[2]);
                } else {
                    starttime = DateTime.Now;
                    endtime = starttime;
                }

                if (channel != "")
                {
                    List<TvDatabase.Program> epgs = TVServerConnection.getEpg(int.Parse(arguments[0]), starttime, endtime);

                    foreach (TvDatabase.Program e in epgs)
                    {
                        string epg = e.StartTime.ToString("u") + "|"
                            + e.EndTime.ToString("u") + "|"
                            + e.Title.Replace("|", "") + "|"
                            + e.Description.Replace("|", "") + "|"
                            + e.Genre.Replace("|", "");
                        if (arguments.Length >= 3)
                        {
                            epg += "|"
                                + e.IdProgram.ToString() + "|"
                                + e.IdChannel.ToString() + "|"
                                + e.SeriesNum + "|"
                                + e.EpisodeNumber + "|"
                                + e.EpisodeName + "|"
                                + e.EpisodePart + "|"
                                + e.OriginalAirDate.ToString("u") + "|"
                                + e.Classification + "|"
                                + e.StarRating.ToString() + "|"
                                + e.ParentalRating.ToString();
                        }
                        results.Add(epg);
                    }
                    writer.writeList(results);
                }
                else
                {
                    writer.write("[ERROR]: Usage: " + getCommandToHandle() + ":chanNr[|startDateTime|endDateTime]");
                }
            }
        }

        public override string getCommandToHandle()
        {
            return "GetEPG";
        }
    }
}
