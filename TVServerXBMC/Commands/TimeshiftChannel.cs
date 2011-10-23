using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace TVServerXBMC.Commands
{
    class TimeshiftChannel : CommandHandler
    {

        public TimeshiftChannel(ConnectionHandler connection)
            : base(connection)
        {
            
        }

        /*
         * Expect arguments: ChannelID
         */
        public override void handleCommand(string command, string[] arguments, ref TvControl.IUser me)
        {
            if (arguments == null || arguments.Length < 1)
            {
                getConnection().WriteLine("[ERROR]: Usage: " + getCommandToHandle() + ":ChannelId[|ResolveIPs=False][|StopTimeshift=True]");
                return;
            }

            try {
                int chanId = int.Parse(arguments[0]);
                bool resolveToIP = true;
                bool stoptimeshift = true;
                string originalURL = "";
                string result;
                string timeShiftFileName = "";

                if (arguments.Length >= 2)
                {
                    resolveToIP = Boolean.Parse(arguments[1]);
                    if (arguments.Length >= 3)
                    {
                        stoptimeshift = Boolean.Parse(arguments[2]);
                    }

                    if (stoptimeshift)
                    {
                        TVServerConnection.StopTimeshift(ref me);
                    }

                    result = TVServerConnection.playChannel(chanId, resolveToIP, ref originalURL, ref me, ref timeShiftFileName);
                    if ( !result.StartsWith("[ERROR]") )
                    {
                      if (resolveToIP == true)
                      {
                          result += "|" + originalURL;
                      }
                      else
                      {
                          result += "|";
                      }
                      result += "|" + timeShiftFileName +
                        "|" + me.CardId.ToString();
                    }

                    writer.write(result);
                }
                else
                {   //backward compatibility
                    TVServerConnection.StopTimeshift(ref me);
                    result = TVServerConnection.playChannel(chanId, resolveToIP, ref originalURL, ref me, ref timeShiftFileName);
                    writer.write(result);
                }
                
            } catch {
                getConnection().WriteLine("[ERROR]: Usage: " + getCommandToHandle() + ":ChannelId[|ResolveIPs][|StopTimeshift]");
            }

        }

        public override string getCommandToHandle()
        {
            return "TimeshiftChannel";
        }
    }
}
