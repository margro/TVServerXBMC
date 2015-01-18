using System;
using System.Collections.Generic;
using System.Text;
using TVServerXBMC;

namespace TVServerXBMC.Commands
{
   class ListRecordedTV : CommandHandler
   {
      public ListRecordedTV(ConnectionHandler connection)
         : base(connection)
      {
      }

      public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
      {
         // we want to list all recorded TV shows
         List<string> results = new List<string>();

         List<RecordedInfo> recorded = TVServerConnection.getRecordedTV();
         foreach (RecordedInfo r in recorded)
         {
             Int32 runningTime = Convert.ToInt32((r.endTime.Ticks - r.startTime.Ticks) / 10000000);
             string streamURL = TVServerConnection.getRecordingURL(Convert.ToInt32(r.ID));
             string recitem = writer.makeItem(
                 r.ID.ToString(),
                 r.title.Replace(";",""),
                 r.description.Replace(";", ""),
                 r.genre,
                 r.played.ToString(),
                 r.startTime.ToString(),
                 r.endTime.ToString(),
                 r.filename,
                 r.channelName.Replace(";", ""),
                 runningTime.ToString(),
                 streamURL);
             results.Add(recitem);
             Console.WriteLine("Recorded: " + recitem);
         }

         writer.writeList(results);
      }

      public override string getCommandToHandle()
      {
         return "ListRecordedTV";
      }
   }
}
