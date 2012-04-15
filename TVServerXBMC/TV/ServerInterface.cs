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

namespace MPTvClient
{
    public class TVServerController
    {
        IList<TvDatabase.ChannelGroup> tvgroups = null;
        IList<TvDatabase.Channel> channels = null;
        IList<TvDatabase.GroupMap> tvmappings = null;
        IList<TvDatabase.RadioChannelGroup> radiogroups = null;
        IList<TvDatabase.RadioGroupMap> radiomappings = null;
        IList<TvDatabase.Card> cards = null;
        IController controller = null;

        IUser me = null;
        Dictionary<String, String> isTimeShifting = null;
        public static Dictionary<String, TvControl.User> userlist = null;
        public Exception lastException = null;

        #region Connection functions

        public TVServerController(IController controller)
        {
            this.controller = controller;
            this.isTimeShifting = new Dictionary<String,String>();
            TVServerController.userlist = new Dictionary<String, TvControl.User>();
        }

        public bool Setup()
        {
            string connStr = "";
            string provider = "";

            try
            {
                controller.GetDatabaseConnectionString(out connStr, out provider);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerXBMC: An exception occurred while connecting to the database. Did you select the right backend? TVServerXBMC default=MySQL; change your Gentle.conf file if you are using MSSQL.");
                Log.Error("TVServerXBMC: The exception: " + ex.ToString());
                return false;
            }

            Console.WriteLine(provider + ":" + connStr);

            try
            {
                Console.WriteLine("Set Gentle framework provider to " + provider);
                Console.WriteLine("Set Gentle framework provider string to " + connStr);
                Gentle.Framework.ProviderFactory.SetDefaultProvider(provider);
                Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connStr);
                Console.WriteLine("New TvControl Admin user");
                me = new User("TVServerXBMC", true);
            }
            catch (Exception ex)
            {
                try
                {
                    Console.WriteLine(ex.ToString());
                }
                catch {}
                lastException = ex;
                return false;
            }

            try
            {
                Console.WriteLine("Fetch card list");
                cards = Card.ListAll();
                Console.Write("Fetch TV and radio channel list");
                channels = Channel.ListAll();
                Console.WriteLine(": " + channels.Count + " channels found");

                // TV channel groups information:
                Console.Write("Fetch TV groups list");
                tvgroups = ChannelGroup.ListAll();
                Console.WriteLine(": " + tvgroups.Count + " tvgroups found");
                Console.Write("Fetch TV group mappings");
                tvmappings = GroupMap.ListAll();
                Console.WriteLine(": " + tvmappings.Count + " tvmappings found");

                // Radio channel groups information:
                Console.Write("Fetch radio groups");
                radiogroups = RadioChannelGroup.ListAll();
                Console.WriteLine(": " + radiogroups.Count + " radiogroups found");
                Console.Write("Fetch radio group mappings");
                radiomappings = RadioGroupMap.ListAll();
                Console.WriteLine(": " + channels.Count + " radiomappings found");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Error("TVServerXBMC: An exception occurred during Setup(): " + ex.Message);
                if (ex.Source == "Gentle.Common")
                {
                    Console.WriteLine("TVServerXBMC has problems connecting to the database. Check your settings (Gentle.config)");
                    Log.Error("TVServerXBMC has problems connecting to the database. Check your settings");
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
            timeShiftBufNr = -1;
            timeShiftBufPos = -1;
 
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

            //cardId = user.CardId;

            watch.Start();

            try
            {
                Console.WriteLine("Start timeshift for channel " + idChannel + " for user '" + user.Name);
                Log.Info("TVServerXBMC: Start timeshift for channel " + idChannel + " for user '" + user.Name);

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
                    isTimeShifting.Add(user.Name, rtspURL);
                }
                catch { }

                Console.WriteLine("Timeshift started for channel: '" + vcard.ChannelName + "' on device '" + vcard.Name + "' card id=" + vcard.Id);
                Log.Debug("TVServerXBMC: Timeshift started for channel: '" + vcard.ChannelName + "' on device '" + vcard.Name + "'");
                Console.WriteLine("TV Server returned '" + rtspURL + "' as timeshift URL and " + timeshiftfilename + " as timeshift file");
                Log.Debug("TVServerXBMC: TV Server returned '" + rtspURL + "' as timeshift URL and " + timeshiftfilename + " as timeshift file");
                Console.WriteLine("Remote server='" + remoteserver + "'");
                Log.Debug("TVServerXBMC: Remote server='" + remoteserver + "'");
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
                Console.WriteLine("TimeShift file pos=" + timeShiftBufPos.ToString() + " buffer id=" + timeShiftBufNr.ToString());
            }
            else if ((result == TvResult.NoTuningDetails) || (result== TvResult.UnknownError))
            {   //Hmmz, maybe a webstream?
                StopTimeShifting(ref user);
                Console.WriteLine("TV Server returned: 'NoTuningDetails' or 'UnknownError'.");
                Console.WriteLine("Checking the availability of a webstream for this channel (id=" + idChannel.ToString() + ")");
                Log.Debug("TVServerXBMC: TV Server returned: 'NoTuningDetails' or 'UnknownError'.");
                Log.Debug("TVServerXBMC: Checking the availability of a webstream for this channel (id=" + idChannel.ToString() + ")");
                
                rtspURL = GetWebStreamURL(idChannel);
                
                if (rtspURL.Length > 0)
                {
                    result = TvResult.Succeeded;
                    Console.WriteLine("Found a webstream: '" + rtspURL + "'");
                    Log.Debug("TVServerXBMC: Found a webstream: '" + rtspURL + "'");
                }
                else
                {
                    Console.WriteLine("Unable to find tuning details or a webstream for channel (id=" + idChannel + ")");
                    Log.Debug("TVServerXBMC: Unable to find tuning details or a webstream for channel (id=" + idChannel + ")");
                }
            }
            else
            {
                StopTimeShifting(ref user);
                Console.WriteLine("Failed. TvResult = " + result.ToString());
                Log.Debug("TVServerXBMC: Failed. TvResult = " + result.ToString());
            }

            watch.Stop();
            Console.WriteLine("StartTimeShifting took " + watch.ElapsedMilliseconds.ToString() + " ms");
            Log.Debug("TVServerXBMC: StartTimeShifting took " + watch.ElapsedMilliseconds.ToString() + " ms");

            return result;
        }

        public string GetTimeshiftUrl(ref TvControl.IUser me)
        {
            if (IsTimeShifting(ref me))
            {
                string url;
                if ( isTimeShifting.TryGetValue(me.Name, out url) )
                    return url;
            }

            return "";
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
                        isTimeShifting.Add(user.Name, ss.RTSPUrl);
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

        // Stop all active timeshifts created by the TVServerXBMC plugin
        public bool StopTimeShifting()
        {
            Console.WriteLine("TODO: fixme StopTimeShifting() for all TVServerXBMC users");
            Log.Debug("TVServerXBMC: TODO: fixme StopTimeShifting()");
            return true;
        }

        public string GetRecordingURL(int idRecording, TvServer server, bool resolveHostnames, ref string OriginalURL)
        {
            //TvServer server = new TvServer();
            if (server == null)
                server = new TvServer();

            string rtspURL = server.GetStreamUrlForFileName(idRecording);

            // we resolve any host names, as xbox can't do NETBIOS resolution..
            try
            {
                // XBMC's ffmpeg rtsp code does not like port numbers in the url
                rtspURL = rtspURL.Replace(":554", "");
                // Workaround for MP TVserver bug when using default port for rtsp => returns rtsp://ip:0
                rtspURL = rtspURL.Replace(":0", "");

                OriginalURL = rtspURL;
                if (resolveHostnames)
                {
                    Uri u = new Uri(rtspURL);
                    System.Net.IPAddress ipaddr = null;
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
                                Log.Debug("TVServerXBMC: No IPv4 adress found for '" + u.DnsSafeHost + "' failed. Returning original URL.");
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
                Log.Error("TVServerXBMC: " + ex.ToString());
            }
            return result;
        }

        #endregion

        #region Info functions
        public ReceptionDetails GetReceptionDetails()
        {
            VirtualCard vcard;
            try
            {
                vcard = new VirtualCard(me, RemoteControl.HostName);
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerXBMC: " + ex.ToString());
                return null;
            }
            ReceptionDetails details = new ReceptionDetails();
            details.signalLevel = vcard.SignalLevel;
            details.signalQuality = vcard.SignalQuality;
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
                Log.Error("TVServerXBMC: " + ex.ToString());
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
                foreach (ChannelGroup group in tvgroups)
                    lGroups.Add(group.GroupName);
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerXBMC: " + ex.ToString());
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
                foreach (RadioChannelGroup group in radiogroups)
                    lGroups.Add(group.GroupName);
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerXBMC: " + ex.ToString());
                return null;
            }
            return lGroups;
        }

        public int GetChannelCount(String group)
        {
            if (group == "" || group == null)
            {
                return channels.Count;
            }
            else
            {
                foreach (ChannelGroup chgroup in tvgroups)
                {
                    if (chgroup.GroupName == group)
                    {
                        IList<GroupMap> maps = chgroup.ReferringGroupMap();
                        return maps.Count;
                    }
                }
            }

            return 0;
        }

        public ChannelInfo GetChannelInfo(int chanId)
        {
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
            return null;
        }

        public List<ChannelInfo> GetChannelInfosForGroup(string groupName)
        {
            List<ChannelInfo> refChannelInfos = new List<ChannelInfo>();
            try
            {
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
                                    Log.Error("TVServerXBMC: Error while obtaining the EPG for channel " + channelInfo.channelID + ": " + e.ToString());
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
                Log.Error("TVServerXBMC: " + ex.ToString());
                return null;
            }
            return refChannelInfos;
        }

        public List<string> GetTVChannels(string groupName)
        {
            try
            {
                if (tvgroups != null)
                {
                    List<string> tvchannels = new List<string>();

                    if (groupName.Length == 0)
                        groupName = "All Channels";

                    foreach (ChannelGroup group in tvgroups)
                    {
                        if (group.GroupName == groupName)
                        {
                            IList<GroupMap> maps = group.ReferringGroupMap();
                            foreach (GroupMap map in maps)
                            {
                                Channel chan = map.ReferencedChannel();

                                if (chan == null) continue;

                                string tvchannel;
                                int channelNumber = 0;
                                bool freetoair = true;

                                //Determine the channel number given by the provider using this channel's tuning details
                                IList<TuningDetail> tuningdetails = chan.ReferringTuningDetail();

                                foreach (TuningDetail tuningdetail in tuningdetails)
                                {
                                    //For now, just take the first one:
                                    channelNumber = tuningdetail.ChannelNumber;
                                    freetoair = tuningdetail.FreeToAir;
                                    break;
                                }

                                //XBMC side:
                                //uid, number, name, callsign, iconpath, isencrypted,
                                //isradio, ishidden, isrecording, bouquet, multifeed,
                                //stream_url;

                                //[0] = channel uid
                                //[1] = channel number
                                //[2] = channel name
                                tvchannel = chan.IdChannel + "|" + channelNumber + "|" + chan.DisplayName + "|";
                                //[3] = isencrypted
                                tvchannel += (freetoair ? "0" : "1");

                                tvchannels.Add(tvchannel);
                            }
                        }
                    }

                    return tvchannels;
                }
                return null;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerXBMC: " + ex.ToString());
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
                Log.Error("TVServerXBMC: " + ex.ToString());
                return null;
            }
            return radioChannels;
        }

        public List<string> GetRadioChannels(string groupName)
        {
            try
            {
                if (radiogroups != null)
                {
                    List<string> radiochannels = new List<string>();

                    if (groupName.Length == 0)
                        groupName = "All Channels";

                    foreach (RadioChannelGroup group in radiogroups)
                    {
                        if (group.GroupName == groupName)
                        {
                            IList<RadioGroupMap> maps = group.ReferringRadioGroupMap();
                            foreach (RadioGroupMap map in maps)
                            {
                                Channel chan = map.ReferencedChannel();

                                if (chan == null) continue;

                                string radiochannel;
                                int channelNumber = 0;
                                bool freetoair = true;

                                //Determine the channel number given by the provider using this channel's tuning details
                                IList<TuningDetail> tuningdetails = chan.ReferringTuningDetail();

                                foreach (TuningDetail tuningdetail in tuningdetails)
                                {
                                    //For now, just take the first one:
                                    channelNumber = tuningdetail.ChannelNumber;
                                    freetoair = tuningdetail.FreeToAir;
                                    break;
                                }

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
                                    radiochannel += GetWebStreamURL(ref chan);
                                }
                                else
                                {
                                    radiochannel += "|0|";
                                }

                                radiochannels.Add(radiochannel);

                            }
                        }
                    }

                    return radiochannels;
                }
                return null;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerXBMC: " + ex.ToString());
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
                    recInfo.title = rec.EpisodeName.Length>0 ? rec.EpisodeName:rec.Title;
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
                Log.Error("TVServerXBMC: " + ex.ToString());
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
                    string recording;
                    string channelname;
                    string title;
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

                    try
                    {
                        channelname = rec.ReferencedChannel().DisplayName;
                    }
                    catch
                    {   // Occurs for example when a recording is pointing to a channel
                        // that is deleted in the meantime
                        channelname = rec.IdChannel.ToString();
                    }
                    title = rec.EpisodeName.Length > 0 ? rec.EpisodeName : rec.Title;
                    recording = rec.IdRecording.ToString() + "|"  // 0
                        + rec.StartTime.ToString("u") + "|"       // 1
                        + rec.EndTime.ToString("u") + "|"         // 2
                        + channelname.Replace("|", "") + "|"      // 3
                        + title.Replace("|","") + "|"         // 4
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
                        + rec.IsRecording.ToString();             // 18
                    

                    reclist.Add(recording);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.Write("Exception in GetRecordings() " + ex.Message);
                Log.Debug("TVServerXBMC: Exception in GetRecordings() " + ex.Message);
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
                Log.Error("TVServerXBMC: " + ex.ToString());
                return null;
            }
            return recInfos;
        }

        public String GetRecordingInfo(int recId, bool withRTSPurl)
        {
            String result = "";

            try
            {
                TvServer server = new TvServer();
                TvDatabase.Recording rec = Recording.Retrieve(recId);

                string channelname;
                string rtspURL = "";
                string dummy = "";

                if (withRTSPurl)
                {
                    rtspURL = GetRecordingURL(rec.IdRecording, server, false, ref dummy);//server.GetStreamUrlForFileName(rec.IdRecording);
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
                //[8] lifetime (mediaportal keep until?)
                //[9] original unresolved stream_url if resolveHostnames = True, otherwise this field is missing

                try
                {
                    channelname = rec.ReferencedChannel().DisplayName;
                }
                catch
                {   // Occurs for example when a recording is pointing to a channel
                    // that is deleted in the meantime
                    channelname = rec.IdChannel.ToString();
                }

                result = rec.IdRecording.ToString() + "|"
                    + rec.StartTime.ToString("u") + "|"
                    + rec.EndTime.ToString("u") + "|"
                    + channelname.Replace("|", "") + "|"
                    + rec.Title.Replace("|", "") + "|"
                    + rec.Description.Replace("|", "") + "|"
                    + rtspURL + "|"
                    + rec.FileName + "|"
                    + rec.KeepUntilDate.ToString("u");
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerXBMC: " + ex.ToString());
                return result;
            }
            return result;
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
                Log.Error("TVServerXBMC: " + ex.ToString());
                return null;
            }
            return schedInfos;
        }

        public List<String> GetSchedules()
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
                    String strIdChannel;
                    String strProgramName;
         
                    if (sched.IdParentSchedule != -1) //If it has a parent it's a recording of a serie we won't use this.
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
                    try
                    {
                        channelname = sched.ReferencedChannel().DisplayName;
                    }
                    catch
                    {   // Occurs for example when a recording is pointing to a channel
                        // that is deleted in the meantime
                        channelname = sched.IdChannel.ToString();
                    }

                    strStartTime = sched.StartTime.ToString("u");
                    strEndTime = sched.EndTime.ToString("u");
                    strIdChannel = sched.IdChannel.ToString();
                    strProgramName = sched.ProgramName;
                    try
                    {
                        IList<Program> progs = Schedule.GetProgramsForSchedule(sched);
                        if (progs.Count > 0) //Currently xbmc use the last occurence of each program has the next recording, when fixe we should return all resolved program.
                        {
                            Program pr = progs[0];
                            strStartTime = pr.StartTime.ToString("u");
                            strEndTime = pr.EndTime.ToString("u");
                            strIdChannel = pr.IdChannel.ToString();
                            strProgramName = pr.Title;
                            if (pr.EpisodeName.Length > 0)
                                strProgramName += " - " + pr.EpisodeName;
                            if (pr.IsRecording)
                                strIsRecording = "True";
                        }
                        else if((ScheduleRecordingType)sched.ScheduleType!=(ScheduleRecordingType.Once))
                        {
                            continue; //This timer does not resolve to a program. Do not return it until we have a state for it in XBMC.
                        }
                        else //IF the schedule did not resolve to any program, typical when creating an Instant Recording frm XBMC and the name does not match a program name.
                        {
                            VirtualCard card;
                            TvControl.TvServer tv = new TvServer();
                            if(tv.IsRecordingSchedule(sched.IdSchedule, out card))
                                strIsRecording = "True";
                        }
                    }
                    catch
                    { }
                    
                    schedule = sched.IdSchedule.ToString() + "|"
                        + strStartTime + "|"
                        + strEndTime + "|"
                        + strIdChannel + "|"
                        + channelname.Replace("|", "") + "|"
                        + strProgramName.Replace("|", "") + "|"
                        + sched.ScheduleType.ToString() + "|"
                        + sched.Priority.ToString() + "|"
                        + sched.IsDone().ToString() + "|"
                        + sched.IsManual.ToString() + "|"
                        + sched.Directory + "|"
                        + sched.KeepMethod.ToString() + "|"
                        + sched.KeepDate.ToString("u") + "|"
                        + sched.PreRecordInterval.ToString() + "|"
                        + sched.PreRecordInterval.ToString() + "|"
                        + sched.Canceled.ToString("u") + "|"
                        + sched.Series.ToString() + "|"
                        + strIsRecording;

                    schedlist.Add(schedule);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerXBMC: " + ex.ToString());
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
                    Log.Error("TVServerXBMC: Error retrieving Schedule info for schedule id=%i.", index);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine(ex.ToString());
                Log.Error("TVServerXBMC: " + ex.ToString());
            }
            return schedInfo;
        }

        private string GetDateTimeString()
        {
            string provider = Gentle.Framework.ProviderFactory.GetDefaultProvider().Name.ToLower();
            if (provider == "mysql") return "yyyy-MM-dd HH:mm:ss";
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
                Log.Debug("TVServerXBMC: Error while obtaining the EPG for channel " + idChannel + ": " + e.Message);
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
            catch
            {
                return false;
            }
        }

        public bool UpdateSchedule(int scheduleindex, int channelId, int active, String programName, DateTime startTime, DateTime endTime, int scheduleType, Int32 priority, Int32 keepmethod, DateTime keepdate, Int32 preRecordInterval, Int32 postRecordInterval)
        {
            try
            {
                TvBusinessLayer layer = new TvBusinessLayer();
                Schedule updatedSchedule = Schedule.Retrieve(scheduleindex);
                DateTime defaultCanceled = new DateTime(2000, 01, 01, 0, 0, 0); //Active

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
            catch
            {
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

            try
            {
                TvBusinessLayer layer = new TvBusinessLayer();
                //Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);

                foreach (Card card in cards)
                {
                    Int32 disktotal = 0;
                    Int32 diskused = 0;

                    string rec_drive = card.RecordingFolder.Substring(0, 3);

                    System.IO.DriveInfo[] local_allDrives = System.IO.DriveInfo.GetDrives();
                    foreach (System.IO.DriveInfo local_Drive in local_allDrives)
                    {
                        if (local_Drive.Name == rec_drive)
                        {
                            disktotal = (Int32) (local_Drive.TotalSize / (1024)); // in kb
                            diskused = (Int32)((local_Drive.TotalSize-local_Drive.TotalFreeSpace) / (1024)); // in kb
                        }
                    }

                    result = disktotal.ToString() + "|" + diskused.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while obtaining the GetRecordingDriveSpace: " + e.Message);
                Log.Debug("TVServerXBMC: Error while obtaining the GetRecordingDriveSpace: " + e.Message);
            }

            return result;
        }

        public  List<String> GetCardSettings(int cardID)
        {
            string result = "";
            List<String> cardSettingsList = new List<String>();
            try
            {
              foreach (Card card in cards)
              {
                if ((cardID == -1) || (cardID == card.IdCard))
                {
                  result = card.IdCard.ToString() + "|"
                    + card.DevicePath + "|"
                    + card.Name + "|"
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
                    + card.StopGraph.ToString();
                  cardSettingsList.Add(result);
                }
              }
            }
            catch (Exception e)
            {
              Console.WriteLine("GetCardSettings: " + e.Message);
              Log.Debug("TVServerXBMC: Error while obtaining the GetCardSettings: " + e.Message);
            }

            return cardSettingsList;
        }

        public int GetSignalQuality(int cardID)
        {
          return controller.SignalLevel(cardID);
        }

        public int GetSignalLevel(int cardID)
        {
          return controller.SignalLevel(cardID);
        }
        #endregion
    }
}
