using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
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
        public override void handleCommand(string command, string[] arguments, ref TvControl.User me)
        {
            string rtspUrl = TVServerConnection.getTimeshiftUrl(ref me);
            bool result = true;
            if (String.IsNullOrEmpty(rtspUrl))
            {
                result = false;
                rtspUrl = "";
            }

            // results = isShifting;url;chanInfo as in ListChannels
            // chanInfo must be the same as the listchannel..
            ChannelInfo c = new ChannelInfo();
            if (result)
            {
                c = TVServerConnection.getTimeshiftInfo(ref me);
            }

            writer.write(writer.makeItemSmart(result.ToString(), rtspUrl, c));
            
        }

        public override string getCommandToHandle()
        {
            return "IsTimeshifting";
        }
    }
}
