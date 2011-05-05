using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace MPTvClient
{
  public class ReceptionDetails
  {
    public int signalLevel;
    public int signalQuality;
  }
  public class StreamingStatus
  {
    public int cardId;
    public string cardName;
    public string cardType;
    public string status;
    public string channelName;
    public int channelId;
    public string RTSPUrl;
    public string TimeShiftFileName;
    public string userName;
  }
  public class ProgrammInfo
  {
    public string timeInfo;
    public string description;
  }
  //public class EPGInfo
  //{
  //  public int      idProgram;
  //  public int      idChannel;
  //  public DateTime startTime;
  //  public DateTime endTime;
  //  public string   title;
  //  public string   description;
  //  public string   seriesNum;
  //  public string   episodeNum;
  //  public string   genre;
  //  public DateTime originalAirDate;
  //  public string   classification;
  //  public int      starRating;
  //  public int      parentalRating;
  //  public string   episodeName;
  //  public string   episodePart;
  //  public int      state;
  //}
  public class ChannelInfo
  {
    public string channelID;
    public string name;
    public bool isWebStream;
    public ProgrammInfo epgNow;
    public ProgrammInfo epgNext;
    public bool isScrambled;
  }
  public class RecordingInfo
  {
    public string recordingID;
    public string title;
    public string genre;
    public string description;
    public string timeInfo;
    public string channel;
  }
  public class RecordedInfo
  {
      public string ID;
      public string title;
      public string genre;
      public string description;
      public string filename;
      public DateTime startTime;
      public DateTime endTime;
      public string channelName;
      public int played;
  }
  public class ScheduleInfo
  {
    public string scheduleID;
    public DateTime startTime;
    public DateTime endTime;
    public int channelID;
    public string channelName;
    public string description;
    public string type;
    public int priority;
  }
}
