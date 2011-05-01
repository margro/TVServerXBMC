using System;
using System.Collections.Generic;
using System.Text;
using TvEngine;
using System.Threading;
using TvLibrary.Log;

namespace TVServerXBMC
{
    public class TVServerXBMC : ITvServerPlugin
    {
        #region variables
        Thread listenThread;
        int serverPort = 9596;
        bool connected = false;

        #endregion

        #region properties

        /// <summary>
        /// returns the name of the plugin
        /// </summary>
        public string Name
        {
            get { return "TVServerXBMC"; }
        }

        /// <summary>
        /// returns the version of the plugin
        /// </summary>
        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

        }

        /// <summary>
        /// returns the author of the plugin
        /// </summary>
        public string Author
        {
            get { return "margro, Prashant V"; }
        }

        /// <summary>
        /// returns if the plugin should only run on the master server
        /// or also on slave servers
        /// </summary>
        public bool MasterOnly
        {
            get { return true; }
        }

        /// <summary>
        /// returns the setup sections for display in SetupTv
        /// </summary>
        public SetupTv.SectionSettings Setup
        {
            get { return null; }
        }

        // properties below are TVServerXBMC specific
        public bool Connected
        {
            get
            {
                return connected;
            }
        }

        public int Port
        {
            get { return serverPort; }
            set { serverPort = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Starts the plugin
        /// </summary>
        public void Start(TvControl.IController controller)
        {
            // set up our remote control interface
            Log.WriteFile("plugin: TVServerXBMC started");

            try
            {
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

        /// <summary>
        /// Stops the plugin
        /// </summary>
        public void Stop()
        {
            if (listenThread != null)
            {
                Log.Debug("TVServerXBMC: Listenthread is aborting");
                listenThread.Abort();
                listenThread = null;
            }
            if (connected)
              TVServerConnection.CloseAll();

            Log.WriteFile("plugin: TVServerXBMC stopped");
        }

        #endregion methods
    }
}
