TVServerXBMC v1.1.0.x - Source package
MediaPortal TVServer plugin for XBMC
_______________________________________________________________________________
Original Author: EvilDude/Prashant 2008-2009
http://forum.team-mediaportal.com/mediaportal-1-talk-45/xbmc-python-script-36590/

Improvements & XBMC PVR-Addon extensions:
Marcel Groothuis, 2010
http://www.scintilla.utwente.nl/~marcelg/xbmc
_______________________________________________________________________________

Tested against MediaPortal 1.1.0-1.1.2 (TV Server 1.1.0-1.1.2)

This version is compatible with the python "TVServer" Video plugin for XBMC 9.11
and the "MediaPortal" PVR addon for the pvrtesting2 branch of XBMC.

This version is compatible with the python "TVServer" Video plugin for
XBMC 9.11 and the "MediaPortal" PVR addon for the pvrtesting2 branch of XBMC.
It requires the exact version of MediaPortal as specified above.
_______________________________________________________________________________

_[ General ]___________________________________________________________________
This plugin assumes the usage of MySQL as database backend for the MediaPortal
TV Server. When you are using Microsoft's SQL server instead, you will need to
change the Gentle.config file.

There are two versions of the TVServerXBMC. A standalone version and a TVServer
plugin version. The standalone version is a console application that shows
debug info. It is useful to figure out what is happening and where things go
wrong. It is adviced to start with the standalone version.

The TVServerXBMC must match your MediaPortal TV Server. If you are using a
different version than specified above (e.g. not the daily build from
 01-08-2010), you will need to recompile the plugin and standalone versions
using the source package.

-[ Compile ]___________________________________________________________________
1. Install "Visual C# .NET 2008 Express
2. If you are using a different version of MediaPortal than "1.1.2":
    Replace the files in the "References/" folder by the ones from your
    MediaPortal TV Server installation
3. Open the "TVServerXBMC.sln" and build the "Debug" version (.exe)
4. Test it against your TV Server (see below "Installation" and "Test")
If the debug version works fine, you can build the plugin version:
5. To build the DLL version, right-click on the TVServerXBMC project and select
   "Properties". Change the output-type under "Application" to "Class library",
   Save the project and build again. The resulting binary should now have a
   .dll extension.
6. Install the TVServerXBMC.dll plugin and test it. See below.
   

_[ Installation ]______________________________________________________________

Standalone version (for debugging/first tests):
  Debug/TVServerXBMC.exe:
  Copy it (together with Gentle.config) to your MediaPortal TVServer directory
  and run it from there.

TVServer plugin version
  Release/TVServerXBMC.dll:
  0. When replacing an old version:
     - Open a command line console (Start -> run -> cmd.exe)
        net stop TVService
     - Wait a few seconds
     - Delete the existing TVServerXBMC.dll in Plugins/ directory of your
       MediaPortal TV Server directory
  1. Copy to the Plugins/ directory of your MediaPortal TV Server directory, 
  2. Restart the TVService:
     Open a command line console (Start -> run -> cmd.exe)
       net stop TVService
       net start TVService
  3. Start the MediaPortal "TV-Server Configuration tool".
  4. Enable the "TVServerXBMC" plugin under "Plugins".
  
_[ Test the TVServerXBMC ]_____________________________________________________
Test the plugin/standalone version:

Windows:
You can test the TVServerXBMC tool/plugin using a "telnet" session to port 9596.
The TVServerXBMC plugin/exe can handle at the moment only one connection at a
time. Please make sure that your telnet connection is closed when you start
XBMC-pvr or open the XBMP-TV plugin.

PS. Windows Vista does not install "telnet" by default, you can enable it via
"Control Panel" -> "Programs and Features" -> "Turn Windows features on or off."
or use "PuTTY" in telnet mode instead.

The procedure below is based on "telnet":

Open a command line console (Start -> run -> cmd.exe)

telnet localhost 9596
type "telnet" + enter (without quotes) when you see an empty black screen with
a blinking cursor.

It should answer:
Protocol Accepted; TVServerXBMC version: 1.1.0.100
 ---- End ----
>

Now you can control the TV Server similar to the example below:
===============================================================

Protocol Accepted; TVServerXBMC version: 1.1.0.100
 ---- End ----
> help
ListChannels
ListGroups
ListRecordedTV
DeleteRecordedTV
CloseConnection
TimeshiftChannel
StopTimeshift
Help
IsTimeshifting
GetVersion
GetBackendName
GetEPG
GetTime
GetChannelCount
ListTVChannels
ListRadioChannels
ListRadioGroups
GetRecordingCount
ListRecordings
UpdateRecording
GetScheduleCount
ListSchedules
AddSchedule
DeleteSchedule
UpdateSchedule
GetScheduleInfo
 ---- End ----
> ListTVChannels
32|0|Nederland 1|0
33|0|Nederland 2|0
34|0|Nederland 3|0
> IsTimeshifting
False||||||||False
 ---- End ----
> timeshiftchannel
[ERROR]: Usage: TimeshiftChannel:ChannelId[|ResolveIPs]
 ---- End ----
> timeshiftchannel:32|False
rtsp://mypc/stream2.0
 ---- End ----
> timeshiftchannel:32|True|False
rtsp://192.168.2.5/stream2.0|rtsp://mypc/stream2.0|F:\Live TV\live2-0.ts.tsbuffer
 ---- End ----
> istimeshifting
True|rtsp://192.168.2.5/stream2.0|32|Nederland 1|||||False
 ---- End ----
> stoptimeshift
True
 ---- End ----
> getversion
1.1.0.0
 ---- End ----
> getbackendname
idefix
 ---- End ----
> quit
===============================================================

_[ Troubleshooting Hints]______________________________________________________
- Check the log files / console outputs
  o For the plugin version (.DLL), check the MediaPortal TVServer log files
  o For the debug version (.exe) check the console output
- Check the Gentle.config file for wrong database settings (MySQL or SQLSERVER)
