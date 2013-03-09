using System;
using System.Collections.Generic;
using System.Text;
using TVServerXBMC;
using TvControl;
using System.Net;
using TvLibrary.Log;
using TvDatabase;

namespace TVServerXBMC
{
    class TVServerConnection
    {
        private static TVServerController serverIntf;

        // info about current timeshift
        private static string timeshiftUrl = "";
        private static Dictionary<String,int> timeshiftChannel;

        // create our objects and connect!
        public static bool Init(IController controller)
        {
            timeshiftUrl = null;
            timeshiftChannel = new Dictionary<String,int>();
            serverIntf = new TVServerController(controller);
            return serverIntf.Setup();
        }

        // Get a list of <string> tvgroups (channel tvgroups)
        public static List<string> GetTVGroups()
        {
            List<string> groups = serverIntf.GetGroupNames();
            if (groups == null)
            {
                disconnect();
                return null;
            }

            return groups;
        }

        // Get a list of <string> radiogroups (channel radiogroups)
        public static List<string> GetRadioGroups()
        {
            List<string> groups = serverIntf.GetRadioGroupNames();
            if (groups == null)
            {
                disconnect();
                return null;
            }

            return groups;
        }

        public static string getBackendName()
        {
            return serverIntf.GetBackendName();
        }

        public static int getChannelCount(String group)
        {
            return serverIntf.GetChannelCount(group);
        }

        public static int getRecordingCount()
        {
            return serverIntf.GetRecordingCount();
        }

        // get a list of all channels
        public static List<ChannelInfo> getChannels(String group)
        {
            List<ChannelInfo> refChannelInfos = serverIntf.GetChannelInfosForGroup(group);
            if (refChannelInfos == null)
                disconnect();

            return refChannelInfos;
        }

        public static List<string> GetTVChannels(String group)
        {
            List<string> result = serverIntf.GetTVChannels(group);
            return result;
        }

        public static List<string> GetRadioChannels(String group)
        {
            List<string> result = serverIntf.GetRadioChannels(group);
            return result;
        }

        public static List<ChannelInfo> getRadioChannels(String group)
        {
            List<ChannelInfo> refChannelInfos = serverIntf.getRadioChannels(group);
            if (refChannelInfos == null)
                disconnect();

            return refChannelInfos;
        }


        public static List<RecordingInfo> getRecordings()
        {
            List<RecordingInfo> recordings = serverIntf.getRecordings();
            if (recordings == null)
                disconnect();

            return recordings;
        }

        public static List<String> GetRecordings(bool resolveHostnames, ref string OriginalURL)
        {
            List<String> recordings = serverIntf.GetRecordings(resolveHostnames, ref OriginalURL);

            return recordings;
        }

        public static String getRecordingURL(int recId)
        {
          string OriginalURL = "";
          return serverIntf.GetRecordingURL(recId, null, true, ref OriginalURL);
        }

        // get a list of recorded shows
        public static List<RecordedInfo> getRecordedTV()
        {
            List<RecordedInfo> recorded = serverIntf.GetRecordedTV();
            if (recorded == null)
                disconnect();

            return recorded;
        }

        public static bool IsRecording()
        {
            return serverIntf.IsRecording();
        }

        public static bool StopRecording(int schedId)
        {
            return serverIntf.StopRecording(schedId);
        }

        public static string getRecordingInfo(int recordingId, bool withRTSPurl)
        {
            return serverIntf.GetRecordingInfo(recordingId, withRTSPurl);
        }

        public static bool SetRecordingTimesWatched(int recordingId, int timesWatched)
        {
          return serverIntf.SetRecordingTimesWatched(recordingId, timesWatched);
        }

        public static int GetRecordingStopTime(int recordingId)
        {
          return serverIntf.GetRecordingStopTime(recordingId);
        }

        public static bool SetRecordingStopTime(int recordingId, int stopTime)
        {
          return serverIntf.SetRecordingStopTime(recordingId, stopTime);
        }

        // get the amount of schedules recordings
        public static int getScheduleCount()
        {
            return serverIntf.GetScheduleCount();
        }

        // get a list of scheduled recordings
        public static List<ScheduleInfo> getSchedules()
        {
            List<ScheduleInfo> schedules = serverIntf.getSchedules();
            return schedules;
        }

        public static List<String> GetSchedules()
        {
            return serverIntf.GetSchedules();
        }

        public static String GetScheduleInfo(int index)
        {
            return serverIntf.GetScheduleInfo(index);
        }

        public static bool DeleteSchedule(int schedId)
        {
            return serverIntf.DeleteSchedule(schedId);  // no return value
        }

        /**
         * \brief  timeshift a channel and get the stream url
         * \param  OriginalURL is the URL given to us by the TVServer
         * \return value is the URL with resolved hostnames,
         */
        public static String playChannel(int chanId, bool resolveHostnames, ref string OriginalURL, ref TvControl.IUser me, ref string timeShiftFileName, ref Int64 timeShiftBufPos, ref long timeShiftBufNr)
        {
            string rtspURL = "";
            string remoteserver = "";

            //serverIntf.StopTimeShifting(ref me);
            timeshiftChannel.Remove(me.Name);

            TvResult result = serverIntf.StartTimeShifting(chanId, ref rtspURL, ref remoteserver, ref me, ref timeShiftFileName, ref timeShiftBufPos, ref timeShiftBufNr);

            if (result != TvResult.Succeeded)
            {
                rtspURL = "[ERROR]: TVServer answer: " + result.ToString() + "|" + (int) result;
            }
            else
            {
                // we resolve any host names, as xbox can't do NETBIOS resolution..
                try
                {
                    //Workaround for MP TVserver bug when using default port for rtsp => returns rtsp://ip:0
                    rtspURL = rtspURL.Replace(":554", "");
                    rtspURL = rtspURL.Replace(":0", "");

                    OriginalURL = rtspURL;

                    if (resolveHostnames)
                    {
                        Uri u = new Uri(rtspURL);
                        IPAddress ipaddr = null;
                        try
                        {
                            ipaddr = IPAddress.Parse(u.DnsSafeHost); //Fails on a hostname
                        }
                        catch { }

                        if ((ipaddr == null) || (ipaddr.IsIPv6LinkLocal || ipaddr.IsIPv6Multicast || ipaddr.IsIPv6SiteLocal))
                        {
                            try
                            {
                                IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(u.DnsSafeHost);

                                string newHost = "";
                                foreach (IPAddress ipaddr2 in hostEntry.AddressList)
                                {
                                    // we only want ipv4.. this is just the xbox after all
                                    if (ipaddr2.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                                    {
                                        newHost = ipaddr2.ToString();
                                        break;
                                    }
                                }

                                rtspURL = u.AbsoluteUri.Replace(u.Scheme + "://" + u.Host + "/", u.Scheme + "://" + newHost + "/");

                                if (newHost.Length == 0)
                                {
                                    Log.Debug("TVServerXBMC: No IPv4 adress found for '" + u.DnsSafeHost + "' failed. Returning original URL.");
                                    Console.WriteLine("No IPv4 adress found for '" + u.DnsSafeHost + "' failed. Returning original URL.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("IP resolve for '" + u.DnsSafeHost + "' failed.");
                                Console.WriteLine("Error: " + ex.ToString());
                                Log.Debug("TVServerXBMC: IP resolve for '" + u.DnsSafeHost + "' failed.");
                                Log.Debug("TVServerXBMC: Error: " + ex.ToString());
                            }
                        }
                    }
                }
                catch
                {
                    //Console.WriteLine("IP resolve failed");
                }

                // update globals
                timeshiftUrl = rtspURL;
                timeshiftChannel.Add(me.Name, chanId);

            }

            Log.Debug("TVServerXBMC: PlayChannel " + chanId.ToString() + " => URL=" + rtspURL);
            //Console.WriteLine("PlayChannel result : " + rtspURL);
            return rtspURL;
        }

        public static List<Program> getEpg(int chanId, DateTime startTime, DateTime endTime)
        {
            List<Program> epgs = serverIntf.GetEPGForChannel(chanId.ToString(), startTime, endTime);
            return epgs;
        }

        public static bool StopTimeshift(ref TvControl.IUser me)
        {
            bool result = serverIntf.StopTimeShifting(ref me);
            timeshiftUrl = null;
            return result;
        }

        public static string getTimeshiftUrl(ref TvControl.IUser me)
        {
            return serverIntf.GetTimeshiftUrl(ref me);
        }

        public static ChannelInfo getTimeshiftInfo(ref TvControl.IUser me)
        {
            int channelnr = 0;
            if (timeshiftChannel.TryGetValue(me.Name, out channelnr) )
            {
                return serverIntf.GetChannelInfo(channelnr);
            } else if (me.IdChannel > 0)
            {
                return serverIntf.GetChannelInfo(me.IdChannel);
            }

            return null;
        }
        

        public static bool DeleteRecordedTV(int recId)
        {
            bool result = true;
            serverIntf.DeleteRecording(recId);  // no return value
            // bool result = IController.DeleteRecording(recId);
            return result;
        }

        public static bool UpdateRecording(int recId, String recName)
        {
            return serverIntf.UpdateRecording(recId, recName);
        }

        public static bool AddSchedule(int channelId, String programName, DateTime startTime, DateTime endTime, int scheduleType, Int32 priority, Int32 keepmethod, DateTime keepdate, Int32 preRecordInterval, Int32 postRecordInterval)
        {
            return serverIntf.AddSchedule(channelId, programName, startTime, endTime, scheduleType, priority, keepmethod, keepdate, preRecordInterval, postRecordInterval);
        }

        public static bool UpdateSchedule(int scheduleindex, int channelId, int active, String programName, DateTime startTime, DateTime endTime, int scheduleType, Int32 priority, Int32 keepmethod, DateTime keepdate, Int32 preRecordInterval, Int32 postRecordInterval, Int32 program_id)
        {
            return serverIntf.UpdateSchedule(scheduleindex, channelId, active, programName, startTime, endTime, scheduleType, priority, keepmethod, keepdate, preRecordInterval, postRecordInterval, program_id);
        }

        public static void disconnect()
        {
            serverIntf.ResetConnection();
        }

        public static void CloseAll()
        {
            if (!String.IsNullOrEmpty(timeshiftUrl))
            {
                serverIntf.StopTimeShifting();
            }
        }

        public static string GetVersion()
        {
            return serverIntf.GetVersion();
        }

        public static TvControl.User RequestUser(string username)
        {
            return serverIntf.RequestUser(username);
        }

        public static string GetRecordingDriveSpace()
        {
            return serverIntf.GetRecordingDriveSpace();
        }

        public static TVServerController GetServerInterface()
        {
           return serverIntf;
        }
    }
}
