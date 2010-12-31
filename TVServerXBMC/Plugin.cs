using System;
using System.Collections.Generic;
using System.Text;
using TvEngine;
using System.Threading;
using TvLibrary.Log;

namespace TVServerXBMC
{
    public class XbmcServer : ITvServerPlugin
    {
        Thread listenThread;
        int serverPort = 9596;
        bool connected = false;

        public int Port
        {
            get { return serverPort; }
            set { serverPort = value; }
        }

        public string Author
        {
            get { return "margro, Prashant V"; }
        }

        public bool MasterOnly
        {
            get { return true; }
        }

        public string Name
        {
            get { return "TVServerXBMC"; }
        }

        public SetupTv.SectionSettings Setup
        {
            get { return null; }
        }

        public void Start(TvControl.IController controller)
        {
            // set up our remote control interface
            try {
                connected = TVServerConnection.Init(controller);
                if (connected)
                {
                    Log.Info("TVServerXBMC: Start listening on port " + serverPort);

                    // start a thread for the listener

                    Listener l = new Listener(serverPort);
                    if (l.StartListening())
                    {
                        // start a thread to listen for clients
                        //(new Thread(new ThreadStart(ListenForClients)).Start ();
                        try
                        {
                            listenThread = new Thread(new ThreadStart(l.ListenForClients));
                            listenThread.Start();
                        }
                        catch
                        {
                            l.Stop();
                        }
                    }
                    else
                    {
                        connected = false;
                    }
                }
                else
                {
                    Log.Error("TVServerXBMC: TVServerConnection init failed.");
                }
            }
            catch
            {
                Log.Error("TVServerXBMC: TVServerConnection.Init failed!");
                Console.WriteLine("TVServerConnection.Init failed!");
            }
        }

        public void Stop()
        {
            Log.Info("TVServerXBMC: Stop()");
            if (listenThread != null)
            {
                Log.Debug("TVServerXBMC: Listenthread is aborting");
                listenThread.Abort();
                listenThread = null;
            }
            if (connected)
              TVServerConnection.CloseAll();
        }

        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

        }

        public bool Connected
        {
            get
            {
                return connected;
            }
        }
    }

}
