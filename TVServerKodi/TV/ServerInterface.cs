using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Gentle.Common;
using Gentle.Framework;
using TvControl;
using TvDatabase;
using TvLibrary.Log;

using TVServerKodi.Common;

namespace TVServerKodi
{
    public class TVServerController
    {
        IController controller = null;

        IUser me = null;
        Dictionary<String, TimeShiftURLs> isTimeShifting = null;
        public static Dictionary<String, TvControl.User> userlist = null;
        public Exception lastException = null;

        #region Connection functions

        public TVServerController(IController controller)
        {
            this.controller = controller;
            this.isTimeShifting = new Dictionary<String, TimeShiftURLs>();
            TVServerController.userlist = new Dictionary<String, TvControl.User>();
        }

        public bool Setup()
        {
            try
            {
              // Just to check our database connection:
              IList<TvDatabase.ChannelGroup> tvgroups = ChannelGroup.ListAll();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("TVServerKodi: An exception occurred during Setup(): " + ex.Message);
                if (ex.Source == "Gentle.Common")
                {
                    Console.WriteLine("TVServerKodi has problems connecting to the database. Check your settings (Gentle.config)");
                    Log.Error("TVServerKodi has problems connecting to the database. Check your settings");
                }

                return false;
            }

            return true;
        }
        public void ResetConnection()
        {
            //_isTimeShifting = false;
        }
        #endregion

        #region Control functions
        public TvResult StartTimeShifting(int idChannel, ref string rtspURL, ref string remoteserver, ref IUser user, ref string timeshiftfilename, ref Int64 timeShiftBufPos, ref long timeShiftBufNr)
        {
            VirtualCard vcard;
            //int cardId = -1;
            TvResult result;
            remoteserver = "";
            timeShiftBufNr = 0;
            timeShiftBufPos = 0;
 
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

            //cardId = user.CardId;

            watch.Start();

            try
            {
                Console.WriteLine("Start timeshift for channel " + idChannel + " for user '" + user.Name);
                Log.Info("TVServerKodi: Start timeshift for channel " + idChannel + " for user '" + user.Name);

                //result = controller.StartTimeShifting(ref user, idChannel, out vcard, cardId != -1); //This one is faster but we need to know the card id on beforehand
                result = controller.StartTimeShifting(ref user, idChannel, out vcard);
            }
            catch (Exception ex)
            {
                lastException = ex;
                StopTimeShifting(ref user);
                return TvResult.UnknownError;
            }

            if (result == TvResult.Succeeded)
            {
                rtspURL = vcard.RTSPUrl;
                remoteserver = vcard.RemoteServer;
                timeshiftfilename = vcard.TimeShiftFileName;
                user.CardId = vcard.Id;   // keep track of the card the user is timeshifting
                user.IdChannel = vcard.IdChannel;
                TVServerController.userlist[user.Name].CardId = vcard.Id; //can't pass the list item by reference
                TVServerController.userlist[user.Name].IdChannel = vcard.IdChannel; //can't pass the list item by reference

                try
                {
                    isTimeShifting.Remove(user.Name); // Remove user with old timeshift data (on new channel tuning, without stopping the timeshift)
                    isTimeShifting.Add(user.Name, new TimeShiftURLs { RTSPUrl = rtspURL, TimeShiftFileName = timeshiftfilename });
                }
                catch { }

                Console.WriteLine("Timeshift started for channel: '" + vcard.ChannelName + "' on device '" + vcard.Name + "' card id=" + vcard.Id);
                Log.Debug("TVServerKodi: Timeshift started for channel: '" + vcard.ChannelName + "' on device '" + vcard.Name + "'");
                Console.WriteLine("TV Server returned '" + rtspURL + "' as timeshift URL and " + timeshiftfilename + " as timeshift file");
                Log.Debug("TVServerKodi: TV Server returned '" + rtspURL + "' as timeshift URL and " + timeshiftfilename + " as timeshift file");
                Console.WriteLine("Remote server='" + remoteserver + "'");
                Log.Debug("TVServerKodi: Remote server='" + remoteserver + "'");
                //Console.WriteLine("Streaming server IP-address: " + GetRTSPserverIP());

                try {
                  //Fetch video stream information (takes approx 2-5 ms):
                  TvLibrary.Interfaces.IVideoStream videostream = vcard.GetCurrentVideoStream(TVServerController.userlist[user.Name]);
                  TvLibrary.Interfaces.VideoStreamType vidtype = videostream.StreamType;
                  Console.WriteLine("Video stream type=" + vidtype.ToString() + " Pid=" + videostream.Pid.ToString("X") + " PcrPid=" + videostream.PcrPid.ToString("X"));

                  TvLibrary.Interfaces.IAudioStream audiostream = vcard.AudioStream;
                  TvLibrary.Interfaces.AudioStreamType audiotype = audiostream.StreamType;
                  Console.WriteLine("Audio stream type=" + audiotype.ToString() + " Pid=" + audiostream.Pid.ToString("X") + " Language=" + audiostream.Language);
                }
                catch { }

                controller.TimeShiftGetCurrentFilePosition(ref user, ref timeShiftBufPos, ref timeShiftBufNr);
                DateTime timeShiftStartTime = controller.TimeShiftStarted(user);
                Console.WriteLine("TimeShift file pos=" + timeShiftBufPos.ToString() + " buffer id=" + timeShiftBufNr.ToString() + " start time=" + timeShiftStartTime.ToString());
            }
            else if ((result == TvResult.NoTuningDetails) || (result== TvResult.UnknownError))
            {   //Hmmz, maybe a webstream?
                StopTimeShifting(ref user);
                Console.WriteLine("TV Server returned: 'NoTuningDetails' or 'UnknownError'.");
                Console.WriteLine("Checking the availability of a webstream for this channel (id=" + idChannel.ToString() + ")");
                Log.Debug("TVServerKodi: TV Server returned: 'NoTuningDetails' or 'UnknownError'.");
                Log.Debug("TVServerKodi: Checking the availability of a webstream for this channel (id=" + idChannel.ToString() + ")");
                
                rtspURL = GetWebStreamURL(idChannel);
                
                if (rtspURL.Length > 0)
                {
                    result = TvResult.Succeeded;
                    Console.WriteLine("Found a webstream: '" + rtspURL + "'");
                    Log.Debug("TVServerKodi: Found a webstream: '" + rtspURL + "'");
                }
                else
                {
                    Console.WriteLine("Unable to find tuning details or a webstream for channel (id=" + idChannel + ")");
                    Log.Debug("TVServerKodi: Unable to find tuning details or a webstream for channel (id=" + idChannel + ")");
                }
            }
            else
            {
                StopTimeShifting(ref user);
                Console.WriteLine("Failed. TvResult = " + result.ToString());
                Log.Debug("TVServerKodi: Failed. TvResult = " + result.ToString());
            }

            watch.Stop();
            Console.WriteLine("StartTimeShifting took " + watch.ElapsedMilliseconds.ToString() + " ms");
            Log.Debug("TVServerKodi: StartTimeShifting took " + watch.ElapsedMilliseconds.ToString() + " ms");

            return result;
        }

        public TimeShiftURLs GetTimeshiftURLs(ref TvControl.IUser me)
        {
            if (IsTimeShifting(ref me))
            {
                TimeShiftURLs timeShiftURLs = new TimeShiftURLs();
                if (isTimeShifting.TryGetValue(me.Name, out timeShiftURLs))
                    return timeShiftURLs;
            }

            return null;
        }

        public bool IsTimeShifting(ref IUser user)
        {
            bool result = controller.IsTimeShifting(ref user);

            if (result == false)
            {   // Fetch streaming status of all cards and search for this user
                List<StreamingStatus> lss = GetStreamingStatus();

                foreach (StreamingStatus ss in lss)
                {
                    if((ss.status == "Timeshifting") && (ss.userName == user.Name))
                    {   //Found one...
                        user.CardId = ss.cardId;
                        user.IdChannel = ss.channelId;
                        isTimeShifting.Remove(user.Name); // Make sure that the user is not in the dictionary
                        isTimeShifting.Add(user.Name, new TimeShiftURLs { RTSPUrl = ss.RTSPUrl, TimeShiftFileName = ss.TimeShiftFileName });
                        TVServerController.userlist[user.Name].CardId = ss.cardId;
                        TVServerController.userlist[user.Name].IdChannel = ss.channelId;
                        return true;
                    }
                }
            }
            return result;
        }

        // Stop the timeshift created by User user
        public bool StopTimeShifting(ref IUser user)
        {
            if ( !IsTimeShifting(ref user) )
                return false;

            bool result = false;
            try
            {
                result = controller.StopTimeShifting(ref user);
                isTimeShifting.Remove(user.Name);
            }
            catch (Exception ex)
            {
                lastException = ex;
                return false;
            }
            return result;
        }

        // Stop all active timeshifts created by the TVServerKodi plugin
        public bool StopTimeShifting()
        {
            Console.WriteLine("TODO: fixme StopTimeShifting() for all TVServerKodi users");
            Log.Debug("TVServerKodi: TODO: fixme StopTimeShifting()");
            return true;
        }

        public string GetRecordingURL(int idRecording, TvServer server, bool resolveHostnames, ref string OriginalURL)
        {
            string rtspURL;

            if (server == null)
                server = new TvServer();

            try
            {
                rtspURL = server.GetStreamUrlForFileName(idRecording);

                // we resolve any host names, as xbox can't do NETBIOS resolution..
                // XBMC/Kodi's ffmpeg rtsp code does not like port numbers in the url
                rtspURL = rtspURL.Replace(":554", "");
                // Workaround for MP TVserver bug when using default port for rtsp => returns rtsp://ip:0
                rtspURL = rtspURL.Replace(":0", "");

                OriginalURL = rtspURL;
                try
                {
                    if (resolveHostnames)
                    {
                        System.Net.IPAddress ipaddr = null;
                        Uri u = new Uri(rtspURL);
                        try
                        {
                            ipaddr = System.Net.IPAddress.Parse(u.DnsSafeHost); //Fails on a hostname
                        }
                        catch { }

                        if ((ipaddr == null) || (ipaddr.IsIPv6LinkLocal || ipaddr.IsIPv6Multicast || ipaddr.IsIPv6SiteLocal))
                        {
                            try
                            {
                                System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(u.DnsSafeHost);

                                string newHost = "";
                                foreach (System.Net.IPAddress ipaddr2 in hostEntry.AddressList)
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
                                    Console.WriteLine("No IPv4 adress found for '" + u.DnsSafeHost + "' failed. Returning original URL.");
                                    Log.Error("TVServerKodi: No IPv4 adress found for '" + u.DnsSafeHost + "' failed. Returning original URL.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("IP resolve for '" + u.DnsSafeHost + "' failed.");
                                Console.WriteLine("Error: " + ex.ToString());
                                Log.Error("TVServerKodi: IP resolve for '" + u.DnsSafeHost + "' failed.");
                                Log.Error("TVServerKodi: Error: " + ex.ToString());
                            }
                        }
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                Log.Error("TVServerKodi: GetStreamUrlForFileName(" + idRecording + ") failed.");
                Log.Error("TVServerKodi: Error: " + ex.ToString());
                return "";
            }


            return rtspURL;
        }
        public void DeleteRecording(int idRecording)
        {
            controller.DeleteRecording(idRecording);
        }
        public bool DeleteSchedule(int idSchedule)
        {
            try
            {
                Schedule sched = Schedule.Retrieve(idSchedule);
                sched.Remove();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsRecording()
        {
            return controller.IsAnyCardRecording();
        }

        public bool StopRecording(int schedId)
        {
            bool result = false;
            try
            {
                if (controller.IsAnyCardRecording())
                {
                    if (schedId >= 0)
                    {
                        controller.StopRecordingSchedule(schedId);
                        return true;
                        //return controller.IsRecordingSchedule(schedId,
                    }
                    else
                    {
                        result = controller.StopRecording(ref me);
                    }
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
            }
            return result;
        }

        #endregion

        #region Info functions
        public ReceptionDetails GetReceptionDetails(int cardID)
        {
          ReceptionDetails details = new ReceptionDetails();
          try
          {
            details.signalLevel = controller.SignalLevel(cardID);
            details.signalQuality = controller.SignalQuality(cardID);

          }
          catch
          {
            details.signalLevel = 0;
            details.signalQuality = 0;
          }
          return details;
        }

        public string GetBackendName()
        {
            return RemoteControl.HostName;
        }

        public List<StreamingStatus> GetStreamingStatus()
        {
            List<StreamingStatus> states = new List<StreamingStatus>();
            VirtualCard vcard;
            try
            {

                IList<TvDatabase.Card> cards = Card.ListAll();
                
                foreach (Card card in cards)
                {
                    IUser user = new User();
                    user.CardId = card.IdCard;
                    IUser[] usersForCard = controller.GetUsersForCard(card.IdCard);
                    if (usersForCard == null)
                    {
                        StreamingStatus state = new StreamingStatus();
                        vcard = new VirtualCard(user, RemoteControl.HostName);
                        string tmp = "idle";
                        if (vcard.IsScanning) tmp = "Scanning";
                        if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";
                        state.cardId = card.IdCard;
                        state.cardName = vcard.Name;
                        state.cardType = vcard.Type.ToString();
                        state.status = tmp;
                        state.channelName = "";
                        state.userName = "";
                        states.Add(state);
                        continue;
                    }
                    if (usersForCard.Length == 0)
                    {
                        StreamingStatus state = new StreamingStatus();
                        vcard = new VirtualCard(user, RemoteControl.HostName);
                        string tmp = "idle";
                        if (vcard.IsScanning) tmp = "Scanning";
                        if (vcard.IsGrabbingEpg) tmp = "Grabbing EPG";
                        state.cardId = card.IdCard;
                        state.cardName = vcard.Name;
                        state.cardType = vcard.Type.ToString();
                        state.status = tmp;
                        state.channelName = "";
                        state.userName = "";
                        states.Add(state);
                        continue;
                    }
                    for (int i = 0; i < usersForCard.Length; ++i)
                    {
                        StreamingStatus state = new StreamingStatus();
                        string tmp = "idle";
                        vcard = new VirtualCard(usersForCard[i], RemoteControl.HostName);
                        if (vcard.IsTimeShifting)
                            tmp = "Timeshifting";
                        else
                            if (vcard.IsRecording)
                                tmp = "Recording";
                            else
                                if (vcard.IsScanning)
                                    tmp = "Scanning";
                                else
                                    if (vcard.IsGrabbingEpg)
                                        tmp = "Grabbing EPG";
                        state.cardId = card.IdCard;
                        state.cardName = vcard.Name;
                        state.cardType = vcard.Type.ToString();
                        state.status = tmp;
                        state.channelName = vcard.ChannelName;
                        state.userName = vcard.User.Name;
                        state.channelId = vcard.IdChannel;
                        state.TimeShiftFileName = vcard.TimeShiftFileName;
                        state.RTSPUrl = vcard.RTSPUrl;
                        states.Add(state);
                    }
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return null;
            }
            return states;
        }
        public List<string> GetGroupNames()
        {
            if (!RemoteControl.IsConnected)
                return null;
            List<string> lGroups = new List<string>();
            try
            {
              IList<TvDatabase.ChannelGroup> tvgroups = ChannelGroup.ListAll();
              foreach (ChannelGroup group in tvgroups)
              {
                lGroups.Add(group.GroupName);
              }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return null;
            }
            return lGroups;
        }

        public List<string> GetRadioGroupNames()
        {
            if (!RemoteControl.IsConnected)
                return null;
            List<string> lGroups = new List<string>();
            try
            {
              IList<TvDatabase.RadioChannelGroup> radiogroups = RadioChannelGroup.ListAll();
              foreach (RadioChannelGroup group in radiogroups)
              {
                lGroups.Add(group.GroupName);
              }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return null;
            }
            return lGroups;
        }

        public int GetChannelCount(String group)
        {
          try
          {

            if (group == "" || group == null)
            {
              IList<TvDatabase.Channel> channels = Channel.ListAll();
              return channels.Count;
            }
            else
            {
              IList<ChannelGroup> tvgroups = ChannelGroup.ListAll();
              foreach (ChannelGroup chgroup in tvgroups)
              {
                if (chgroup.GroupName == group)
                {
                  IList<GroupMap> maps = chgroup.ReferringGroupMap();
                  return maps.Count;
                }
              }
            }
          }
          catch (Exception ex)
          {
            Log.Error("TVServerKodi: An exception occurred in GetChannelCount(): " + ex.Message);
          }
          return 0;
        }

        public ChannelInfo GetChannelInfo(int chanId)
        {
          try
          {
            IList<Channel> channels = Channel.ListAll();
            foreach (Channel chan in channels)
            {
              if (chan.IdChannel == chanId)
              {
                // this is the channel we want
                ChannelInfo channelInfo = new ChannelInfo();
                TvDatabase.Program epg = null;

                channelInfo.channelID = chan.IdChannel.ToString();
                channelInfo.name = chan.DisplayName;
                channelInfo.isWebStream = chan.IsWebstream();
                channelInfo.epgNow = new ProgrammInfo();
                channelInfo.epgNext = new ProgrammInfo();

                try
                {
                  epg = chan.CurrentProgram;
                  if (epg != null)
                  {
                    channelInfo.epgNow.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                    channelInfo.epgNow.description = epg.Title;
                  }
                  epg = chan.NextProgram;
                  if (epg != null)
                  {
                    channelInfo.epgNext.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                    channelInfo.epgNext.description = epg.Title;
                  }
                }
                catch { }

                return channelInfo;
              }
            }
          }
          catch (Exception ex)
          {
            Log.Error("TVServerKodi: An exception occurred in GetChannelInfo(): " + ex.Message);
          }
          return null;
        }

        public List<ChannelInfo> GetChannelInfosForGroup(string groupName)
        {
            List<ChannelInfo> refChannelInfos = new List<ChannelInfo>();
            try
            {
                IList<TvDatabase.ChannelGroup> tvgroups = ChannelGroup.ListAll();
                if (tvgroups != null)
                {
                    foreach (ChannelGroup group in tvgroups)
                    {
                        if (group.GroupName == groupName || groupName == "")
                        {
                            IList<GroupMap> maps = group.ReferringGroupMap();
                            TvDatabase.Program epg;
                            foreach (GroupMap map in maps)
                            {
                                ChannelInfo channelInfo = new ChannelInfo();
                                channelInfo.channelID = map.ReferencedChannel().IdChannel.ToString();
                                channelInfo.name = map.ReferencedChannel().DisplayName;
                                channelInfo.epgNow = new ProgrammInfo();

                                try
                                {
                                    epg = map.ReferencedChannel().CurrentProgram;
                                    if (epg != null)
                                    {
                                        channelInfo.epgNow.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                                        channelInfo.epgNow.description = epg.Title;
                                    }
                                    epg = map.ReferencedChannel().NextProgram;
                                    channelInfo.epgNext = new ProgrammInfo();
                                    if (epg != null)
                                    {
                                        channelInfo.epgNext.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                                        channelInfo.epgNext.description = epg.Title;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error while obtaining the EPG for channel " + channelInfo.channelID + ": " + e.ToString());
                                    Log.Error("TVServerKodi: Error while obtaining the EPG for channel " + channelInfo.channelID + ": " + e.ToString());
                                }
                                
                                //Get the tuning details
                                try
                                {
                                    IList<TuningDetail> listtuningdetails = map.ReferencedChannel().ReferringTuningDetail();
                                    foreach (TuningDetail tuningdetail in listtuningdetails)
                                    {
                                        channelInfo.isScrambled = !(tuningdetail.FreeToAir);
                                    }
                                }
                                catch
                                {
                                    channelInfo.isScrambled = false;
                                }

                                refChannelInfos.Add(channelInfo);
                            }
                            if (groupName != "")
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return null;
            }
            return refChannelInfos;
        }

        public List<string> GetTVChannels(List<string> groupNames)
        {
            try
            {
                List<string> tvchannels = new List<string>();
                HashSet<Channel> uniqueChannels = new HashSet<Channel>();

                if (groupNames.Count > 0)
                {
                    IList<TvDatabase.ChannelGroup> tvGroups = ChannelGroup.ListAll();

                    if (tvGroups != null)
                    {
                        foreach (ChannelGroup group in tvGroups)
                        {
                            if (group == null)
                                continue;
                            if (groupNames.Contains(group.GroupName))
                            {
                                IList<GroupMap> maps = group.ReferringGroupMap();

                                if (maps == null)
                                    continue;

                                foreach (GroupMap map in maps)
                                {
                                    Channel chan = map.ReferencedChannel();

                                    if (chan == null)
                                        continue;

                                    uniqueChannels.Add(chan);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("TVServerKodi: GetTVChannels: no tv groups?");
                        Log.Error("TVServerKodi: GetTVChannels: no tv groups?");
                        return null;
                    }
                }
                else
                {
                    IList<TvDatabase.Channel> channels = Channel.ListAll();

                    if (channels != null)
                    {
                        foreach (Channel chan in channels)
                        {
                            if (chan == null)
                                continue;
                            if (chan.IsTv)
                                uniqueChannels.Add(chan);
                        }
                    }
                }

                if (uniqueChannels.Count == 0)
                {
                    Console.WriteLine("TVServerKodi: Found no TV channels.");
                    Log.Error("TVServerKodi: Found no TV channels.");
                    foreach (string group in groupNames)
                    {
                        Console.WriteLine("TVServerKodi: TV channels requested for group " + group);
                        Log.Error("TVServerKodi: TV channels requested for group " + group);
                    }
                    return null;
                }


                foreach (Channel chan in uniqueChannels)
                {
                    string tvchannel;
                    int channelNumber = 10000;
                    bool freetoair = false;
                    int majorChannel = -1;
                    int minorChannel = -1;
                    string weburl = "";

                    try
                    {
                        channelNumber = chan.ChannelNumber;
                    }
                    catch
                    {
                    }

                    //Determine the channel number given by the provider using this channel's tuning details
                    IList<TuningDetail> tuningdetails = chan.ReferringTuningDetail();

                    try
                    {
                        foreach (TuningDetail tuningdetail in tuningdetails)
                        {
                            freetoair = freetoair || tuningdetail.FreeToAir;
                            if (tuningdetail.ChannelType == 1) // ATSC
                            {
                                if (tuningdetail.MajorChannel != -1)
                                    majorChannel = tuningdetail.MajorChannel;
                                if (tuningdetail.MinorChannel != -1)
                                    minorChannel = tuningdetail.MinorChannel;
                            }
                            else if (tuningdetail.ChannelType == 7) // DVB-IP
                            {
                                // If network id, transport id, service id and pmt pid are all 0
                                // accept this channel as a real webstream
                                // Note that MediaPortal itself will not be able to play this
                                if ((tuningdetail.NetworkId <= 0) && (tuningdetail.TransportId <= 0) && (tuningdetail.ServiceId <= 0) && (tuningdetail.PmtPid <= 0))
                                {
                                    weburl = tuningdetail.Url;
                                }
                            }
                            if ((channelNumber == 10000) && (tuningdetail.ChannelNumber > 0))
                            {
                                channelNumber = tuningdetail.ChannelNumber;
                                break;
                            }
                        }
                    }
                    catch
                    {
                    }

                    //Kodi side:
                    //uid, number, name, callsign, iconpath, isencrypted,
                    //isradio, ishidden, isrecording, bouquet, multifeed,
                    //stream_url;

                    //[0] = channel uid
                    //[1] = channel number
                    //[2] = channel name
                    tvchannel = chan.IdChannel + "|" + channelNumber + "|" + chan.DisplayName + "|";
                    //[3] = isencrypted
                    tvchannel += (freetoair ? "0" : "1");
                    //[4] = iswebstream
                    //[5] = webstream url
                    if (chan.IsWebstream())
                    {
                        tvchannel += "|1|";
                        Channel webChannel = chan;
                        tvchannel += GetWebStreamURL(ref webChannel) + "|";
                    }
                    else if (weburl.Length > 0)
                    {
                        tvchannel += "|1|";
                        tvchannel += weburl + "|";
                    }
                    else
                    {
                        tvchannel += "|0||";
                    }
                    //[6] = visibleinguide
                    tvchannel += (chan.VisibleInGuide ? "1" : "0") + "|";
                    //[7] = ATSC majorchannel
                    //[8] = ATSC minorchannel
                    tvchannel += majorChannel + "|" + minorChannel;

                    tvchannels.Add(tvchannel);
                }

                return tvchannels;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return null;
            }
        }
        
        public List<ChannelInfo> getRadioChannels(string groupName)
        {
            List<ChannelInfo> radioChannels = new List<ChannelInfo>();
            try
            {
                if (groupName == "")
                {
                    TvDatabase.Program epg;

                    IList<Channel> channels = Channel.ListAll();
                    foreach (Channel chan in channels)
                    {
                        if (!chan.IsRadio)
                            continue;
                        ChannelInfo channelInfo = new ChannelInfo();
                        channelInfo.channelID = chan.IdChannel.ToString();
                        channelInfo.name = chan.DisplayName;
                        channelInfo.isWebStream = chan.IsWebstream();
                        epg = chan.CurrentProgram;
                        channelInfo.epgNow = new ProgrammInfo();
                        if (epg != null)
                        {
                            channelInfo.epgNow.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                            channelInfo.epgNow.description = epg.Title;
                        }
                        epg = chan.NextProgram;
                        channelInfo.epgNext = new ProgrammInfo();
                        if (epg != null)
                        {
                            channelInfo.epgNext.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                            channelInfo.epgNext.description = epg.Title;
                        }
                        radioChannels.Add(channelInfo);
                    }
                }
                else
                {
                    IList<TvDatabase.RadioChannelGroup> radiogroups = RadioChannelGroup.ListAll();
                    foreach (RadioChannelGroup group in radiogroups)
                    {
                        if (group.GroupName == groupName)
                        {
                            IList<RadioGroupMap> maps = group.ReferringRadioGroupMap();
                            TvDatabase.Program epg;
                            foreach (RadioGroupMap map in maps)
                            {
                                ChannelInfo channelInfo = new ChannelInfo();

                                if (map.ReferencedChannel() == null) continue;

                                channelInfo.channelID = map.ReferencedChannel().IdChannel.ToString();
                                channelInfo.name = map.ReferencedChannel().DisplayName;
                                epg = map.ReferencedChannel().CurrentProgram;
                                channelInfo.epgNow = new ProgrammInfo();
                                if (epg != null)
                                {
                                    channelInfo.epgNow.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                                    channelInfo.epgNow.description = epg.Title;
                                }
                                epg = map.ReferencedChannel().NextProgram;
                                channelInfo.epgNext = new ProgrammInfo();
                                if (epg != null)
                                {
                                    channelInfo.epgNext.timeInfo = epg.StartTime.ToShortTimeString() + "-" + epg.EndTime.ToShortTimeString();
                                    channelInfo.epgNext.description = epg.Title;
                                }
                                radioChannels.Add(channelInfo);
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return null;
            }
            return radioChannels;
        }

        public List<string> GetRadioChannels(List<string> groupNames)
        {
            try
            {
                List<string> radiochannels = new List<string>();
                HashSet<Channel> uniqueChannels = new HashSet<Channel>();

                if (groupNames.Count > 0)
                {
                    IList<TvDatabase.RadioChannelGroup> radioGroups = RadioChannelGroup.ListAll();

                    if (radioGroups != null)
                    {
                        foreach (RadioChannelGroup group in radioGroups)
                        {
                            if (groupNames.Contains(group.GroupName))
                            {

                                IList<RadioGroupMap> maps = group.ReferringRadioGroupMap();
                                foreach (RadioGroupMap map in maps)
                                {
                                    Channel chan = map.ReferencedChannel();

                                    if (chan == null)
                                        continue;

                                    uniqueChannels.Add(chan);
                                }
                            }
                        }
                    }
                    else
                    {
                       Console.WriteLine("TVServerKodi: GetRadioChannels: no radio groups?");
                        Log.Error("TVServerKodi: GetRadioChannels: no radio groups?");
                        return null;
                    }
                }
                else
                {
                    IList<TvDatabase.Channel> channels = Channel.ListAll();

                    foreach (Channel chan in channels)
                    {
                        if (chan.IsRadio)
                            uniqueChannels.Add(chan);
                    }
                }

                foreach (Channel chan in uniqueChannels)
                {
                    string radiochannel;
                    int channelNumber = 10000;
                    bool freetoair = false;
                    int majorChannel = -1;
                    int minorChannel = -1;

                    try
                    {
                        channelNumber = chan.ChannelNumber;
                    }
                    catch
                    {
                    }

                    try
                    {
                        //Determine the channel number given by the provider using this channel's tuning details
                        IList<TuningDetail> tuningdetails = chan.ReferringTuningDetail();

                        foreach (TuningDetail tuningdetail in tuningdetails)
                        {
                            freetoair = freetoair || tuningdetail.FreeToAir;
                            if (tuningdetail.ChannelType == 1) // ATSC
                            {
                                if (tuningdetail.MajorChannel != -1)
                                    majorChannel = tuningdetail.MajorChannel;
                                if (tuningdetail.MinorChannel != -1)
                                    minorChannel = tuningdetail.MinorChannel;
                            }
                            if ((channelNumber == 10000) && (tuningdetail.ChannelNumber > 0))
                            {
                                channelNumber = tuningdetail.ChannelNumber;
                                break;
                            }
                        }
                    }
                    catch
                    {
                    }

                    //XBMC side:
                    //[0] = channel uid
                    //[1] = channel number
                    //[2] = channel name
                    radiochannel = chan.IdChannel + "|" + channelNumber + "|" + chan.DisplayName + "|";
                    //[3] = isencrypted
                    radiochannel += (freetoair ? "0" : "1");
                    //[4] = iswebstream
                    //[5] = webstream url
                    if (chan.IsWebstream())
                    {
                        radiochannel += "|1|";
                        Channel webChannel = chan;
                        radiochannel += GetWebStreamURL(ref webChannel) + "|";
                    }
                    else
                    {
                        radiochannel += "|0||";
                    }
                    //[6] = visibleinguide
                    radiochannel += (chan.VisibleInGuide ? "1" : "0") + "|";
                    //[7] = ATSC majorchannel
                    //[8] = ATSC minorchannel
                    radiochannel += majorChannel + "|" + minorChannel;

                    radiochannels.Add(radiochannel);
                }

                return radiochannels;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return null;
            }
        }

        public string GetWebStreamURL(int idChannel)
        {
            Channel chan = Channel.Retrieve(idChannel);
            return GetWebStreamURL(ref chan);
        }

        public string GetWebStreamURL(ref Channel chan)
        {
            string url = "";
            
            IList<TuningDetail> details = chan.ReferringTuningDetail();
            foreach (TuningDetail detail in details)
            {
                if (detail.ChannelType == 5)
                {
                    url = detail.Url;
                    break;
                }
            }
            return url;
        }

        public int GetRecordingCount()
        {
            IList<TvDatabase.Recording> recordings = Recording.ListAll();
            return recordings.Count;
        }

        public List<RecordingInfo> getRecordings()
        {
            List<RecordingInfo> recInfos = new List<RecordingInfo>();
            try
            {
                IList<TvDatabase.Recording> recordings = Recording.ListAll();
                foreach (Recording rec in recordings)
                {
                    RecordingInfo recInfo = new RecordingInfo();
                    recInfo.recordingID = rec.IdRecording.ToString();

                    try
                    {
                        recInfo.channel = rec.ReferencedChannel().DisplayName;
                    }
                    catch
                    {   // Occurs for example when a recording is pointing to a channel
                        // that is deleted in the meantime
                        recInfo.channel = rec.IdChannel.ToString();
                    }
                    recInfo.title = rec.Title;
                    recInfo.description = rec.Description;
                    recInfo.genre = rec.Genre;
                    recInfo.timeInfo = rec.StartTime.ToString("u") + "|" + rec.EndTime.ToString("u");
                    recInfos.Add(recInfo);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return null;
            }
            return recInfos;
        }

        public List<String> GetRecordings(bool resolveHostnames, ref string OriginalURL)
        {
            List<String> reclist = new List<String>();
            try
            {
                TvServer server = new TvServer();
                IList<TvDatabase.Recording> recordings = Recording.ListAll();

                foreach (Recording rec in recordings)
                {
                    try
                    {
                        string recording;
                        string channelname;
                        ChannelType channelType = TvDatabase.ChannelType.Tv;

                        string rtspURL = GetRecordingURL(rec.IdRecording, server, resolveHostnames, ref OriginalURL);//server.GetStreamUrlForFileName(rec.IdRecording);

                        //XBMC pvr side:
                        //index, channelname, lifetime, priority, start time, duration
                        //title, subtitle, description, stream_url, directory

                        //[0] index / mediaportal recording id
                        //[1] start time
                        //[2] end time
                        //[3] channel name
                        //[4] title
                        //[5] description
                        //[6] stream_url (resolved hostname if resolveHostnames = True)
                        //[7] filename (we can bypass rtsp streaming when XBMC and the TV server are on the same machine)
                        //[8] keepUntilDate (DateTime)
                        //[9] original unresolved stream_url if resolveHostnames = True, otherwise this field is missing
                        //[10] keepUntil (int)
                        //[11] episodeName (string)
                        //[12] episodeNumber (string)
                        //[13] episodePart (string)
                        //[14] seriesNumber (string)
                        //[15] scheduleID (int)
                        //[16] Genre (string)
                        //[17] channel id (int)
                        //[18] isrecording (bool)
                        //[19] timesWatched (int)
                        //[20] stopTime (int)
                        //[21] channelType (int)

                        try
                        {
                            Channel recChannel = rec.ReferencedChannel();
                            channelname = recChannel.DisplayName;
                            if (recChannel.IsRadio)
                            {
                                channelType = TvDatabase.ChannelType.Radio;
                            }
                            else if (recChannel.IsWebstream())
                            {
                                channelType = TvDatabase.ChannelType.Web;
                            }
                            else
                            {
                                // Assume Tv for all other cases
                                channelType = TvDatabase.ChannelType.Tv;
                            }

                        }
                        catch
                        {   // Occurs for example when a recording is pointing to a channel
                            // that is deleted in the meantime
                            channelname = rec.IdChannel.ToString();
                        }
                        recording = rec.IdRecording.ToString() + "|"  // 0
                            + rec.StartTime.ToString("u") + "|"       // 1
                            + rec.EndTime.ToString("u") + "|"         // 2
                            + channelname.Replace("|", "") + "|"      // 3
                            + rec.Title.Replace("|", "") + "|"        // 4
                            + rec.Description.Replace("|", "") + "|"  // 5
                            + rtspURL + "|"                           // 6
                            + rec.FileName + "|"                      // 7
                            + rec.KeepUntilDate.ToString("u") + "|";  // 8

                        if (resolveHostnames)
                        {
                            recording += OriginalURL + "|";           // 9
                        }
                        else
                        {
                            recording += rtspURL + "|";
                        }

                        recording += rec.KeepUntil.ToString() + "|"   // 10
                            + rec.EpisodeName.Replace("|", "") + "|"  // 11
                            + rec.EpisodeNum + "|"                    // 12
                            + rec.EpisodePart + "|"                   // 13
                            + rec.SeriesNum + "|"                     // 14
                            + rec.Idschedule.ToString() + "|"         // 15
                            + rec.Genre + "|"                         // 16
                            + rec.IdChannel.ToString() + "|"          // 17
                            + rec.IsRecording.ToString() + "|"        // 18
                            + rec.TimesWatched.ToString() + "|"       // 19
                            + rec.StopTime.ToString() +"|"            // 20
                            + (int)channelType;

                        reclist.Add(recording);
                    }
                    catch (Exception)
                    { }
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.Write("Exception in GetRecordings() " + ex.Message);
                Log.Debug("TVServerKodi: Exception in GetRecordings() " + ex.Message);
                return null;
            }
            return reclist;
        }
        
        public List<RecordedInfo> GetRecordedTV()
        {
            List<RecordedInfo> recInfos = new List<RecordedInfo>();
            try
            {
                IList<TvDatabase.Recording> recordings = Recording.ListAll();
                foreach (Recording rec in recordings)
                {
                    RecordedInfo recInfo = new RecordedInfo();
                    recInfo.ID = rec.IdRecording.ToString();
                    recInfo.title = rec.Title;
                    recInfo.description = rec.Description;
                    recInfo.genre = rec.Genre;
                    recInfo.filename = rec.FileName;
                    //recInfo.timeInfo = rec.StartTime.ToString() + "-" + rec.EndTime.ToString();
                    recInfo.startTime = rec.StartTime;
                    recInfo.endTime = rec.EndTime;
                    recInfo.played = rec.TimesWatched;
                    //recInfo.channelName = TVDatabase.GetChannelById(rec.IdChannel).Name;
                    recInfo.channelName = rec.IdChannel.ToString();
                    recInfos.Add(recInfo);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return null;
            }
            return recInfos;
        }

        public String GetRecordingInfo(int recId, bool withRTSPurl, bool resolveRTSPurlToIP)
        {
            string recording = "";

            try
            {
                TvServer server = new TvServer();
                TvDatabase.Recording rec = Recording.Retrieve(recId);

                string channelname;
                string rtspURL = "";
                string OriginalURL = "";
                ChannelType channelType = TvDatabase.ChannelType.Tv;

                if (withRTSPurl)
                {
                    rtspURL = GetRecordingURL(rec.IdRecording, server, resolveRTSPurlToIP, ref OriginalURL);
                }
                //XBMC pvr side:
                //index, channelname, lifetime, priority, start time, duration
                //title, subtitle, description, stream_url, directory

                //[0] index / mediaportal recording id
                //[1] start time
                //[2] end time
                //[3] channel name
                //[4] title
                //[5] description
                //[6] stream_url (resolved hostname if resolveHostnames = True)
                //[7] filename (we can bypass rtsp streaming when XBMC and the TV server are on the same machine)
                //[8] keepUntilDate (DateTime)
                //[9] original unresolved stream_url if resolveHostnames = True, otherwise this field is missing
                //[10] keepUntil (int)
                //[11] episodeName (string)
                //[12] episodeNumber (string)
                //[13] episodePart (string)
                //[14] seriesNumber (string)
                //[15] scheduleID (int)
                //[16] Genre (string)
                //[17] channel id (int)
                //[18] isrecording (bool)
                //[19] timesWatched (int)
                //[20] stopTime (int)
                //[21] channelType (int)

                try
                {
                    Channel recChannel = rec.ReferencedChannel();
                    channelname = recChannel.DisplayName;
                    if (recChannel.IsRadio)
                    {
                        channelType = TvDatabase.ChannelType.Radio;
                    }
                    else if (recChannel.IsWebstream())
                    {
                        channelType = TvDatabase.ChannelType.Web;
                    }
                    else
                    {
                        // Assume Tv for all other cases
                        channelType = TvDatabase.ChannelType.Tv;
                    }
                }
                catch
                {   // Occurs for example when a recording is pointing to a channel
                    // that is deleted in the meantime
                    channelname = rec.IdChannel.ToString();
                }

                recording = rec.IdRecording.ToString() + "|"  // 0
                    + rec.StartTime.ToString("u") + "|"       // 1
                    + rec.EndTime.ToString("u") + "|"         // 2
                    + channelname.Replace("|", "") + "|"      // 3
                    + rec.Title.Replace("|", "") + "|"        // 4
                    + rec.Description.Replace("|", "") + "|"  // 5
                    + rtspURL + "|"                           // 6
                    + rec.FileName + "|"                      // 7
                    + rec.KeepUntilDate.ToString("u") + "|";  // 8

                if (resolveRTSPurlToIP)
                {
                    recording += OriginalURL + "|";           // 9
                }
                else
                {
                    recording += rtspURL + "|";
                }

                recording += rec.KeepUntil.ToString() + "|"   // 10
                    + rec.EpisodeName.Replace("|", "") + "|"  // 11
                    + rec.EpisodeNum + "|"                    // 12
                    + rec.EpisodePart + "|"                   // 13
                    + rec.SeriesNum + "|"                     // 14
                    + rec.Idschedule.ToString() + "|"         // 15
                    + rec.Genre + "|"                         // 16
                    + rec.IdChannel.ToString() + "|"          // 17
                    + rec.IsRecording.ToString() + "|"        // 18
                    + rec.TimesWatched.ToString() + "|"       // 19
                    + rec.StopTime.ToString() + "|"            // 20
                    + (int)channelType;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return recording;
            }
            return recording;
        }


        public int GetScheduleCount()
        {
            IList<Schedule> schedules = Schedule.ListAll();
            return schedules.Count;
        }

        public List<ScheduleInfo> getSchedules()
        {
            List<ScheduleInfo> schedInfos = new List<ScheduleInfo>();
            try
            {
                IList<Schedule> schedules = Schedule.ListAll();
                foreach (Schedule schedule in schedules)
                {
                    ScheduleInfo sched = new ScheduleInfo();
                    sched.scheduleID = schedule.IdSchedule.ToString();
                    sched.startTime = schedule.StartTime;
                    sched.endTime = schedule.EndTime;
                    sched.channelID = schedule.IdChannel;
                    sched.channelName = schedule.ReferencedChannel().DisplayName;
                    sched.description = schedule.ProgramName;
                    ScheduleRecordingType stype = (ScheduleRecordingType)schedule.ScheduleType;
                    sched.type = stype.ToString();
                    sched.priority = schedule.Priority;
                    

                    schedInfos.Add(sched);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return null;
            }
            return schedInfos;
        }
        private string FormatSchedule(string strSchedId, string strStartTime, string strEndTime,
            string strIdChannel, string strchannelname, string strProgramName, Schedule sched,
            string strIsRecording, string stridProgram, DateTime Canceled, int iParentSchedule,
            string strGenre, string strProgramDescription)
        {
            string schedule;
            schedule = strSchedId + "|"
                + strStartTime + "|"
                + strEndTime + "|"
                + strIdChannel + "|"
                + strchannelname + "|"
                + strProgramName + "|"
                + sched.ScheduleType.ToString() + "|"
                + sched.Priority.ToString() + "|"
                + sched.IsDone().ToString() + "|"
                + sched.IsManual.ToString() + "|"
                + sched.Directory + "|"
                + sched.KeepMethod.ToString() + "|"
                + sched.KeepDate.ToString("u") + "|"
                + sched.PreRecordInterval.ToString() + "|"
                + sched.PostRecordInterval.ToString() + "|"
                + Canceled.ToString("u") + "|"
                + sched.Series.ToString() + "|"
                + strIsRecording + "|"
                + stridProgram + "|"
                + iParentSchedule.ToString() + "|"
                + strGenre + "|"
                + strProgramDescription;
            return schedule;
        }

        public List<String> GetSchedules(bool kodiHasSeriesSupport = false)
        {
            List<String> schedlist = new List<String>();
            try
            {
                IList<Schedule> schedules = Schedule.ListAll();
                foreach (Schedule sched in schedules)
                {
                    String schedule;
                    String channelname;
                    String strIsRecording = "False";
                    String strStartTime;
                    String strEndTime;
                    String strSchedId;
                    String strIdChannel;
                    String strProgramName;
                    int idProgram = 0;
 
                    if (sched.IdParentSchedule != -1) //If it has a parent it's a recording of a series we won't use this.
                        continue;

                    //XBMC pvr side:
                    //
                    //[0]  index/id
                    //[1]  start date + time
                    //[2]  end   date + time
                    //[3]  channel nr (mediaportal channel id)
                    //[4]  channel name
                    //[5]  program name
                    //[6]  repeat info/schedule type
                    //[7]  priorty
                    //[8]  isdone
                    //[9]  ismanual
                    //[10] directory
                    //[11] keep method
                    //[12] keep date
                    //[13] preRecordInterval
                    //[14] postRecordInterval
                    //[15] canceled
                    //[16] series
                    //[17] isrecording (TODO)
                    //[18] idProgram
                    //[19] parent schedule id
                    //[20] genre of the program
                    //[21] program description
                    try
                    {
                        channelname = sched.ReferencedChannel().DisplayName;
                    }
                    catch
                    {   // Occurs for example when a recording is pointing to a channel
                        // that is deleted in the meantime
                        channelname = sched.IdChannel.ToString();
                    }

                    try
                    {
                        strSchedId = sched.IdSchedule.ToString();
                        strStartTime = sched.StartTime.ToString("u");
                        strEndTime = sched.EndTime.ToString("u");
                        strIdChannel = sched.IdChannel.ToString();
                        strProgramName = sched.ProgramName;

                        IList<Program> progs = Schedule.GetProgramsForSchedule(sched);
                        IList<CanceledSchedule> canceled_progs = sched.ReferringCanceledSchedule();

                        if (kodiHasSeriesSupport == true && sched.ScheduleType != 0 /* Once */)
                        {
                            // return also the real schedule and not only the underlying programs
                            schedule = FormatSchedule(strSchedId, strStartTime, strEndTime, strIdChannel, channelname.Replace("|", ""),
                                        strProgramName.Replace("|", ""), sched, strIsRecording, idProgram.ToString(), sched.Canceled, sched.IdParentSchedule, "", "");
                            schedlist.Add(schedule);
                        }

                        foreach (Program pr in progs) 
                        {
                            DateTime dtCanceled = sched.Canceled;
                            int parentSchedule;

                            if (kodiHasSeriesSupport && sched.ScheduleType != 0 /* Once */)
                            {
                                // add the programs for this schedule as sub-timers in Kodi
                                parentSchedule = sched.IdSchedule;
                            }
                            else
                            {
                                parentSchedule = sched.IdParentSchedule;
                            }

                            strStartTime = pr.StartTime.ToString("u");
                            strEndTime = pr.EndTime.ToString("u");
                            strIdChannel = pr.IdChannel.ToString();
                            strProgramName = pr.Title;
                            if (pr.EpisodeName.Length > 0)
                                strProgramName += " - " + pr.EpisodeName;
                            if (pr.IsRecording)
                                strIsRecording = "True";
                            else strIsRecording = "False";
                            idProgram = pr.IdProgram;

                            // Check if this program is in the CanceledSchedule list
                            foreach (CanceledSchedule cs in canceled_progs)
                            {
                              if (cs.CancelDateTime == pr.StartTime)
                              {
                                dtCanceled = cs.CancelDateTime;
                                break;
                              }
                            }
                          
                            schedule = FormatSchedule(strSchedId, strStartTime, strEndTime, strIdChannel, channelname.Replace("|", ""),
                                          strProgramName.Replace("|", ""), sched, strIsRecording, idProgram.ToString(), dtCanceled, parentSchedule, pr.Genre, pr.Description);
                            schedlist.Add(schedule);
                        }
 
                        if (progs.Count == 0)
                        {
                            if ((ScheduleRecordingType)sched.ScheduleType != (ScheduleRecordingType.Once))
                            {
                                continue; //This timer does not resolve to a program. Do not return it until we have a state for it in XBMC.
                            }
                            else //If the schedule did not resolve to any program, typical when creating an Instant Recording from XBMC and the name does not match a program name.
                            {
                                VirtualCard card;
                                TvControl.TvServer tv = new TvServer();
                                if (tv.IsRecordingSchedule(sched.IdSchedule, out card))
                                    strIsRecording = "True";
                                idProgram = -1;
                                schedule = FormatSchedule(strSchedId, strStartTime, strEndTime, strIdChannel, channelname.Replace("|", ""),
                                            strProgramName.Replace("|", ""), sched, strIsRecording, idProgram.ToString(), sched.Canceled, sched.IdParentSchedule, "", ""); 
                                schedlist.Add(schedule);
                            }
                        }
                    }
                    catch
                    { }

                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
                return null;
            }
            return schedlist;
        }


        public String GetScheduleInfo(int index)
        {
            String schedInfo = "";
            String channelname;

            try
            {
                Schedule s = Schedule.Retrieve(index);

                if (s != null)
                {
                    try
                    {
                        channelname = s.ReferencedChannel().DisplayName;
                        //s.ReferencedChannel().
                    }
                    catch
                    {   // Occurs for example when a recording is pointing to a channel
                        // that is deleted in the meantime
                        channelname = s.IdChannel.ToString();
                    }

                    schedInfo = s.IdSchedule + "|"
                        + s.StartTime.ToString("u") + "|"
                        + s.EndTime.ToString("u") + "|"
                        + s.IdChannel.ToString() + "|"
                        + channelname.Replace("|", "") + "|"
                        + s.ProgramName.Replace("|", "") + "|"
                        + s.ScheduleType.ToString() + "|"
                        + s.Priority.ToString() + "|"
                        + s.IsDone().ToString() + "|"
                        + s.IsManual + "|"
                        + s.Directory + "|"
                        + s.KeepMethod.ToString() + "|"
                        + s.KeepDate.ToString("u") + "|"
                        + s.PreRecordInterval.ToString() + "|"
                        + s.PreRecordInterval.ToString() + "|"
                        + s.Canceled.ToString("u") + "|"
                        + s.Series.ToString() + "|"
                        + "False";
                }
                else
                {
                    Console.Write("Error retrieving Schedule info for schedule id=%i.", index);
                    Log.Error("TVServerKodi: Error retrieving Schedule info for schedule id=%i.", index);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerKodi: " + ex.ToString());
            }
            return schedInfo;
        }

        private string GetDateTimeString()
        {
            string provider = Gentle.Framework.ProviderFactory.GetDefaultProvider().Name.ToLower();
            switch (provider)
            {
                case "mysql":
                case "sqlite":
                    return "yyyy-MM-dd HH:mm:ss";
                default:
                    break;
            }
            return "yyyyMMdd HH:mm:ss";
        }
        public List<Program> GetEPGForChannel(string idChannel, DateTime startTime, DateTime endTime)
        {
            IFormatProvider mmddFormat = new CultureInfo(String.Empty, false);
            //List<Program> infos = new List<Program>();

            try
            {
                SqlBuilder sb = new SqlBuilder(Gentle.Framework.StatementType.Select, typeof(Program));
                sb.AddConstraint(Operator.Equals, "idChannel", Int32.Parse(idChannel));
                if (startTime >= DateTime.Now)
                {
                    DateTime thisMorning = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    sb.AddConstraint(String.Format("startTime>='{0}'", thisMorning.ToString(GetDateTimeString(), mmddFormat)));
                }
                else
                {
                    sb.AddConstraint(String.Format("startTime>='{0}'", startTime.ToString(GetDateTimeString(), mmddFormat)));
                }
                if (endTime>DateTime.Now)
                {
                    sb.AddConstraint(String.Format("endTime<='{0}'", endTime.ToString(GetDateTimeString(), mmddFormat)));
                }
                sb.AddOrderByField(true, "startTime");
                SqlStatement stmt = sb.GetStatement(true);
                IList programs = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
                return (List<Program>) programs;
            }
            catch(Exception e)
            {
                Console.WriteLine("Error while obtaining the EPG for channel " + idChannel + ": " + e.Message);
                Log.Debug("TVServerKodi: Error while obtaining the EPG for channel " + idChannel + ": " + e.Message);
            }
            return null;
        }

        public string GetVersion()
        {
            return controller.GetAssemblyVersion.ToString();
        }

        public bool AddSchedule(int channelId, String programName, DateTime startTime, DateTime endTime, int scheduleType, Int32 priority, Int32 keepmethod, DateTime keepdate, Int32 preRecordInterval, Int32 postRecordInterval)
        {
            try
            {
                TvBusinessLayer layer = new TvBusinessLayer();
                
                TvDatabase.Schedule s = new TvDatabase.Schedule(channelId, programName, startTime, endTime);
                if (scheduleType < 0)
                {
                    s.ScheduleType = (int)TvDatabase.ScheduleRecordingType.Once;
                }
                else
                {
                    s.ScheduleType = scheduleType;
                }

                if (priority != -1)
                {
                    s.Priority = priority;
                }

                if (keepmethod != -1)
                {
                    s.KeepMethod = keepmethod;
                    s.KeepDate = keepdate;
                }

                if ((preRecordInterval < 0) && (postRecordInterval < 0))
                {   //Use the settings from Mediaportal
                    s.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                    s.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                }
                else
                {   // Use the settings from XBMC
                    s.PreRecordInterval = preRecordInterval;
                    s.PostRecordInterval = postRecordInterval;
                }
                s.Persist();
                RemoteControl.Instance.OnNewSchedule();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while adding a schedule for channel " + channelId + " and program " + programName + ": " + e.Message);
                Log.Debug("TVServerKodi: Error while adding a schedule for channel " + channelId + " and program " + programName + ": " + e.Message);
                return false;
            }
        }

        public bool UpdateSchedule(int scheduleindex, int channelId, int active, String programName, DateTime startTime, DateTime endTime, int scheduleType, Int32 priority, Int32 keepmethod, DateTime keepdate, Int32 preRecordInterval, Int32 postRecordInterval, Int32 programId)
        {
            try
            {
                TvBusinessLayer layer = new TvBusinessLayer();
                Schedule updatedSchedule = Schedule.Retrieve(scheduleindex);
                DateTime defaultCanceled = new DateTime(2000, 01, 01, 0, 0, 0); //Active

                // Note that Kodi 16.x returns once for a program below a series schedule...
                if ((programId != -1) && (scheduleType != (int)TvDatabase.ScheduleRecordingType.Once || updatedSchedule.ScheduleType != (int)TvDatabase.ScheduleRecordingType.Once))
                {
                  // Series schedule, retrieve the canceled programs list
                  Program program = Program.Retrieve(programId);

                  //program.

                  IList<CanceledSchedule> canceledSched = updatedSchedule.ReferringCanceledSchedule();

                  if (active == 1)
                  {
                    // Check if this schedule is deactivated and remove it from the CanceledSchedule list
                    foreach (CanceledSchedule cs in canceledSched)
                    {
                      if (cs.CancelDateTime == program.StartTime)
                      {
                        cs.Remove();
                        break;
                      }
                    }
                  }
                  else
                  {
                    Boolean found = false;
                    // Add this schedule to the CanceledSchedule list if not already in the list
                    foreach (CanceledSchedule cs in canceledSched)
                    {
                      if (cs.CancelDateTime == program.StartTime)
                      {
                        found = true;
                        break;
                      }
                    }

                    if (!found)
                    {
                      CanceledSchedule newCs = new CanceledSchedule(scheduleindex, program.IdChannel, program.StartTime);
                      newCs.Persist();
                    }
                  }

                  //  if (!found)
                  //  {
                  //    Program.ProgramState
                  //  }
                  //}
                  return true;
                }

                

                updatedSchedule.ProgramName = programName;
                updatedSchedule.StartTime = startTime;
                updatedSchedule.EndTime = endTime;
                if ((active == 0) && (updatedSchedule.Canceled.Equals(defaultCanceled)))
                {   // Canceled from XBMC
                    updatedSchedule.Canceled = DateTime.Now;
                }
                else if ((active == 1) && (!updatedSchedule.Canceled.Equals(defaultCanceled)))
                {   // Re-activated
                    updatedSchedule.Canceled = defaultCanceled;
                }

                if (scheduleType < 0)
                {   //Unknown, record once
                    updatedSchedule.ScheduleType = (int)TvDatabase.ScheduleRecordingType.Once;
                }
                else
                {   //Use the given value
                    updatedSchedule.ScheduleType = scheduleType;
                }

                if (priority != -1)
                {
                    updatedSchedule.Priority = priority;
                }

                if (keepmethod != -1)
                {
                    updatedSchedule.KeepMethod = keepmethod;
                    updatedSchedule.KeepDate = keepdate;
                }

                if ((preRecordInterval < 0) && (postRecordInterval < 0))
                {   //Use the settings from Mediaportal
                    updatedSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                    updatedSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                }
                else
                {   // Use the settings from XBMC
                    updatedSchedule.PreRecordInterval = preRecordInterval;
                    updatedSchedule.PostRecordInterval = postRecordInterval;
                }
                updatedSchedule.Persist();
                RemoteControl.Instance.OnNewSchedule();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while updating schedule " + scheduleindex + " on channel " + channelId + " and program " + programName + ": " + e.Message);
                Log.Debug("TVServerKodi: Error while updating schedule " + scheduleindex + " on channel " + channelId + " and program " + programName + ": " + e.Message);
                return false;
            }
        }

        public bool UpdateRecording(int recordingindex, String recordingName)
        {
          try
          {
            Recording recording = Recording.Retrieve(recordingindex);

            recording.Title = recordingName;
            recording.Persist();

            return true;
          }
          catch
          {
            return false;
          }
        }

        public bool SetRecordingTimesWatched(int recordingindex, int timesWatched)
        {
          try
          {
            Recording recording = Recording.Retrieve(recordingindex);

            if (recording.TimesWatched < timesWatched)
              recording.TimesWatched = timesWatched;
            else
            {
              // some other client already updated the counter, so just do a +1 here
              recording.TimesWatched++;
            }
            recording.Persist();

            return true;
          }
          catch
          {
            return false;
          }
        }

        public int GetRecordingStopTime(int recordingindex)
        {
          try
          {
            Recording recording = Recording.Retrieve(recordingindex);

            return recording.StopTime;
          }
          catch
          {
            return -1;
          }
        }

        public bool SetRecordingStopTime(int recordingindex, int stopTime)
        {
          try
          {
            Recording recording = Recording.Retrieve(recordingindex);

            recording.StopTime = stopTime;
            recording.Persist();

            return true;
          }
          catch
          {
            return false;
          }
        }

        public TvControl.User RequestUser(string username)
        {
            // Re-use an existing TvControl.User with the same name,
            // otherwise create a new one
            if (!TVServerController.userlist.ContainsKey(username))
            {
                // Create a new user with admin rights
                TvControl.User user = new TvControl.User(username, true);
                TVServerController.userlist.Add(username, user);
            }
            return TVServerController.userlist[username];
        }

        public string GetRecordingDriveSpace()
        {
            string result = "0|0";
            long disktotal = 0;
            long diskused = 0;
            List<string> processedDrives = new List<string>();

            try
            {
                TvBusinessLayer layer = new TvBusinessLayer();
                //Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                IList<TvDatabase.Card> cards = Card.ListAll();

                System.IO.DriveInfo[] local_allDrives = System.IO.DriveInfo.GetDrives();

                foreach (Card card in cards)
                {

                    if (card.RecordingFolder.Length > 0)
                    {
                        string rec_drive = "";

                        if(card.RecordingFolder.StartsWith(@"\\"))
                        {
                            rec_drive = ShareExplorer.TryConvertUncToLocal(card.RecordingFolder);
                            rec_drive = rec_drive.Substring(0, 3);
                        }
                        else
                            rec_drive = card.RecordingFolder.Substring(0, 3);

                        if (!processedDrives.Exists(drive => drive == rec_drive))
                        {
                            foreach (System.IO.DriveInfo local_Drive in local_allDrives)
                            {
                                if (local_Drive.Name == rec_drive)
                                {
                                    processedDrives.Add(rec_drive);
                                    disktotal += (local_Drive.TotalSize / (1024)); // in kb
                                    diskused += ((local_Drive.TotalSize - local_Drive.TotalFreeSpace) / (1024)); // in kb
                                    break;
                                }
                            }
                        }
                    }
                }
                result = disktotal.ToString() + "|" + diskused.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while obtaining the GetRecordingDriveSpace: " + e.Message);
                Log.Debug("TVServerKodi: Error while obtaining the GetRecordingDriveSpace: " + e.Message);
            }

            return result;
        }

        public  List<String> GetCardSettings(int cardID)
        {
            string result = "";
            List<String> cardSettingsList = new List<String>();
            try
            {
              IList<TvDatabase.Card> cards = Card.ListAll();

              foreach (Card card in cards)
              {
                if ((cardID == -1) || (cardID == card.IdCard))
                {
                  try
                  {
                    result = card.IdCard.ToString() + "|"
                      + card.DevicePath.Replace('|', ' ') + "|"
                      + card.Name.Replace('|',' ') + "|"
                      + card.Priority.ToString() + "|"
                      + card.GrabEPG.ToString() + "|"
                      + card.LastEpgGrab.ToString("u") + "|"
                      + card.RecordingFolder + "|"
                      + card.IdServer.ToString() + "|"
                      + card.Enabled.ToString() + "|"
                      + card.CamType.ToString() + "|"
                      + card.TimeShiftFolder.ToString() + "|"
                      + card.RecordingFormat.ToString() + "|"
                      + card.DecryptLimit.ToString() + "|"
                      + card.PreloadCard.ToString() + "|"
                      + card.CAM.ToString() + "|"
                      + card.netProvider.ToString() + "|"
                      + card.StopGraph.ToString() + "|"
                      + ShareExplorer.GetUncPathForLocalPath(card.RecordingFolder) + "|"
                      + ShareExplorer.GetUncPathForLocalPath(card.TimeShiftFolder);
                    cardSettingsList.Add(result);
                  }
                  catch (Exception e)
                  {
                    Console.WriteLine("GetCardSettings: " + e.Message);
                    Log.Debug("TVServerKodi: Error while obtaining the GetCardSettings for card '" + card.Name + "': " + e.Message);
                  }
                }
              }
            }
            catch (Exception e)
            {
              Console.WriteLine("GetCardSettings: " + e.Message);
              Log.Debug("TVServerKodi: Error while obtaining the GetCardSettings: " + e.Message);
            }

            return cardSettingsList;
        }

        public void HeartBeat(IUser user)
        {
          try
          {
            RemoteControl.Instance.HeartBeat(user);
          }
          catch (Exception e)
          {
            Log.Error("TVServerKodi: failed sending HeartBeat signal to server. ({0})", e.Message);
          }
        }

        #endregion
    }
}
