using System;
using System.Collections.Generic;
using System.Text;
using TVServerKodi;

namespace TVServerKodi.Commands
{
    class IsTimeshifting : CommandHandler
    {
        public IsTimeshifting(ConnectionHandler connection)
            : base(connection)
        {

        }
        /*
         * No arguments needed
         */
        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            TimeShiftURLs timeShiftURLs = TVServerConnection.getTimeshiftURLs(ref me);
            bool result = true;

            if (timeShiftURLs == null || String.IsNullOrEmpty(timeShiftURLs.RTSPUrl) || String.IsNullOrEmpty(timeShiftURLs.TimeShiftFileName))
            {
                timeShiftURLs = new TimeShiftURLs { RTSPUrl = "", TimeShiftFileName = "" };
                result = false;
            }

            // results = isShifting;url;chanInfo as in ListChannels
            // chanInfo must be the same as the listchannel..
            ChannelInfo c = new ChannelInfo();
            if (result)
            {
                c = TVServerConnection.getTimeshiftInfo(ref me);
            }

            writer.write(writer.makeItemSmart(result.ToString(), timeShiftURLs.RTSPUrl, timeShiftURLs.TimeShiftFileName, c));

        }

        public override string getCommandToHandle()
        {
            return "IsTimeshifting";
        }
    }
}
