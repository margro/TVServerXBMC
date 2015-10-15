using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using TVServerKodi.Commands;
using TvLibrary.Log;
using TVServerKodi;

namespace TVServerKodi
{
    enum ClientType
    {
        unknown,
        telnet,
        python,
        pvrclient
    };


    class ConnectionHandler
    {
        private TcpClient client;
        private Dictionary<String, CommandHandler> allHandlers;
        private DataWriter writer;
        private String arg_sep = ",";
        private String cmd_sep = ":";
        private Mutex cmdMutex;
        private int clientnr = 0;
        private string username;
        private TvControl.IUser me;
        private ClientType clientType = ClientType.unknown;
        //private Dictionary<String, TvControl.User> userlist;

        public ConnectionHandler(TcpClient client, Mutex cmdMutex, int clientnr)
        {
            this.client = client;
            this.cmdMutex = cmdMutex;
            this.clientnr = clientnr;
            //this.userlist = userlist;
            addHandlers();
        }

        public Dictionary<String, CommandHandler> getAllHandlers()
        {
            return this.allHandlers;
        }

        private void addHandlers()
        {
            List<CommandHandler> handlers = new List<CommandHandler>();
            allHandlers = new Dictionary<String,CommandHandler>();

            // (Also) Used for the TVServer plugin from Prashantv
            handlers.Add(new ListChannels(this));
            handlers.Add(new ListGroups(this));
            handlers.Add(new ListRecordedTV(this));
            handlers.Add(new DeleteRecordedTV(this));
            handlers.Add(new CloseConnection(this));
            handlers.Add(new TimeshiftChannel(this));
            handlers.Add(new StopTimeshift(this));
            handlers.Add(new Help(this));
            handlers.Add(new IsTimeshifting(this));

            // Added for XBMC/Kodi PVR client addon:
            handlers.Add(new GetVersion(this));
            handlers.Add(new GetBackendName(this));
            handlers.Add(new GetDriveSpace(this));
            // EPG commands:
            handlers.Add(new GetEPG(this));
            handlers.Add(new GetTime(this));
            // Channel commands:
            handlers.Add(new GetChannelCount(this));
            handlers.Add(new ListTVChannels(this));
            handlers.Add(new ListRadioChannels(this));
            handlers.Add(new ListRadioGroups(this));
            handlers.Add(new GetChannelThumb(this));
            // Recording commands:
            handlers.Add(new GetRecordingCount(this));
            handlers.Add(new ListRecordings(this));
            handlers.Add(new UpdateRecording(this));
            handlers.Add(new IsRecording(this));
            handlers.Add(new StopRecording(this));
            handlers.Add(new GetRecordingInfo(this));
            handlers.Add(new SetRecordingTimesWatched(this));
            handlers.Add(new GetRecordingStopTime(this));
            handlers.Add(new SetRecordingStopTime(this));
            // Timer/Schedule commands:
            handlers.Add(new GetScheduleCount(this));
            handlers.Add(new ListSchedules(this));
            handlers.Add(new AddSchedule(this));
            handlers.Add(new DeleteSchedule(this));
            handlers.Add(new UpdateSchedule(this));
            handlers.Add(new GetScheduleInfo(this));
            // TvControl.User commands:
            handlers.Add(new SetUserName(this));
            handlers.Add(new GetUserName(this));
            // Settings:
            handlers.Add(new GetCardSettings(this));
            handlers.Add(new GetSignalQuality(this));

            //handlers.Add(new Test(this));

            foreach (CommandHandler h in handlers)
            {
                allHandlers.Add(h.getCommandToHandle().ToLower(), h);
            }

        }


        public DataWriter getWriter()
        {
            return this.writer;
        }

        public void HandleConnection()
        {
            try
            {
                NetworkStream cStream = this.client.GetStream();
                StreamReader reader = new StreamReader(cStream);
                writer = new DataWriter(cStream);

                // first we get what version of the protocol
                // we expect "TVServerXBMC:0-2"
                String versionInfo = reader.ReadLine();
                if (versionInfo == null) return;

                // version 2 is not backards compatible
                versionInfo = Uri.UnescapeDataString(versionInfo);
                if (versionInfo.ToLower().Contains("telnet"))
                {
                    // human connection
                    writer.setArgumentSeparator("|");
                    writer.setListSeparator(Environment.NewLine);
                    writer.setHumanEncoders();

                    //reader side:
                    cmd_sep = ":";
                    arg_sep = "|";

                    WriteLine("Protocol Accepted; TVServerKodi version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    Console.WriteLine("Correct protocol, telnet connection accepted!");
                    Log.Debug("TVServerKodi: telnet connection accepted!");

                    username = "Koditelnet";
                    clientType = ClientType.telnet;
                }
                else if (Regex.IsMatch(versionInfo, "^TVServerXBMC:0-[0-3]$", RegexOptions.IgnoreCase))
                {
                    WriteLine("Protocol-Accept;0-3");
                    Console.WriteLine("Correct protocol, connection accepted!");
                    Log.Debug("TVServerKodi: connection accepted!");
                    username = "XBMCpython";
                    clientType = ClientType.python;
                }
                else if (Regex.IsMatch(versionInfo, "^PVRclientXBMC:0-[1]$", RegexOptions.IgnoreCase))
                {
                    writer.setArgumentSeparator("|");
                    //reader side:
                    cmd_sep = ":";
                    arg_sep = "|";

                    WriteLine("Protocol-Accept:0|" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    Console.WriteLine("Correct protocol, connection accepted!");
                    Log.Debug("TVServerKodi: connection accepted from XBMC PVR addon");

                    username = "XBMCpvrclient" + clientnr;
                    clientType = ClientType.pvrclient;
                }
                else if (Regex.IsMatch(versionInfo, "^PVRclientKodi:0-[1]$", RegexOptions.IgnoreCase))
                {
                    writer.setArgumentSeparator("|");
                    //reader side:
                    cmd_sep = ":";
                    arg_sep = "|";

                    WriteLine("Protocol-Accept:0|" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    Console.WriteLine("Correct protocol, connection accepted!");
                    Log.Debug("TVServerKodi: connection accepted from Kodi PVR addon");

                    username = "Kodipvrclient" + clientnr;
                    clientType = ClientType.pvrclient;
                }
                else
                {
                    WriteLine("Unexpected Protocol");
                    client.Close();
                    Console.WriteLine("Unexpected protocol:" + versionInfo);
                    Log.Debug("TVServerKodi: Unexpected protocol:" + versionInfo);

                    clientType = ClientType.unknown;
                    return;
                }

                me = TVServerConnection.RequestUser(username);

                ProcessConnection(reader);
                reader.Dispose();
                cStream.Dispose();
            }
            catch (Exception e)
            {
                WriteLine("[ERROR]: HandleConnection failed");
                Log.Debug("TVServerKodi: HandleConnection failed");
                Log.Debug("TVServerKodi: Exception => " + e.Message);
                Log.Debug("TVServerKodi: Stack trace: " + e.StackTrace);
            }
        }

        public void WriteLine(String line)
        {
            writer.write(line);
        }

        public void Disconnect()
        {
            client.Close();
        }


        private void handleCommand(String command, String[] arguments)
        {
          try
          {
            String handleCommand = command.ToLower();
            if (allHandlers.ContainsKey(handleCommand))
            {
              try
              {
                cmdMutex.WaitOne();
                allHandlers[handleCommand].handleCommand(command, arguments, ref me);
              }
              catch (Exception e)
              {
                WriteLine("[ERROR]: Command failed: " + command);
                Log.Debug("TVServerKodi: Command failed: " + command);
                Log.Debug("TVServerKodi: Exception: " + command + " => " + e.Message);
                Log.Debug("TVServerKodi: Stack trace: " + e.StackTrace);
              }
              finally
              {
                cmdMutex.ReleaseMutex();
              }
            }
            else if (handleCommand == "quit" || handleCommand == "exit")
            {
              Disconnect();
            }
            else if (handleCommand == "?")
            {
              foreach (KeyValuePair<String, CommandHandler> kvp in allHandlers)
              {
                WriteLine(kvp.Key);
                Console.WriteLine("Command: " + kvp.Key);
              }
            }
            else
            {
              WriteLine("[ERROR]: UnknownCommand:" + command);
              Console.WriteLine("[ERROR]: Unknown command : " + command);
              Log.Debug("TVServerKodi: Unknown command: " + command);
            }
          }
          catch(Exception e)
          {
            WriteLine("[ERROR]: Exception:" + command + " => " + e.Message);
            Log.Debug("TVServerKodi: Exception: " + command + " => " + e.Message);
            Log.Debug("TVServerKodi: Stack trace: " + e.StackTrace);
          }
        }

        private void ProcessConnection(StreamReader reader)
        {
            try
            {
                while (client.Connected)
                {
                    String line = reader.ReadLine();

                    if (line != null)
                    {
                        Console.WriteLine("Socket read: " + line);
                        // every command is Command:Argument,Argument,Argument
                        // where the arguments are uri encoded. Commands are not encoded
                        String[] parts = line.Split(cmd_sep.ToCharArray(), 2);
                        String command = parts[0];
                        String[] arguments = null;

                        if (parts.Length > 1)
                        {
                            arguments = parts[1].Split(arg_sep.ToCharArray());
                            for (int i = 0; i < arguments.Length; i++)
                            {
                                arguments[i] = System.Uri.UnescapeDataString(arguments[i]);
                            }
                        }

                        Console.WriteLine("Handling command; " + command);
                        Log.Debug("TVServerKodi: Handling command: " + command);
                        handleCommand(command, arguments);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while processing connection : " + e.ToString());
                Log.Debug("TVServerKodi: Exception while processing connection: " + e.ToString());
            }
            Console.WriteLine("Connection closed");
            Log.Debug("TVServerKodi: Connection closed");

            if (clientType != ClientType.python)
            {
              try
              {
                cmdMutex.WaitOne();
                allHandlers["stoptimeshift"].handleCommand("StopTimeshift", null, ref me);
              }
              catch
              { }
              finally
              {
                cmdMutex.ReleaseMutex();
              }
            }
        }
    }
}
