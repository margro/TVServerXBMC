using System;
using System.Collections.Generic;
using System.Text;
using TvEngine;
using System.Threading;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvEngine.Events;
using TVServerKodi.Forms;

namespace TVServerKodi
{
    public class TVServerKodiPlugin : ITvServerPlugin
    {
      #region variables
      int m_defaultServerPort = 9596;
      int m_serverPort = 9596;
      bool connected = false;
      Listener m_listener = null;

      #endregion

      #region properties

      /// <summary>
      /// returns the name of the plugin
      /// </summary>
      public string Name
      {
        get { return "TVServerKodi"; }
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

      // properties below are TVServerKodi specific
      public bool Connected
      {
        get
        {
          return connected;
        }
      }

      public int Port
      {
        get { return m_serverPort; }
        set { m_serverPort = value; }
      }

      #endregion

      #region IPlugin Methods

      /// <summary>
      /// Starts the plugin
      /// </summary>
      public void Start(TvControl.IController controller)
      {
          // set up our remote control interface
          Log.WriteFile("TVServerKodi: plugin started");

          try
          {
              LoadSettings();

              connected = TVServerConnection.Init(controller);
              if (connected)
              {
                if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
                {
                  GlobalServiceProvider.Instance.TryGet<ITvServerEvent>().OnTvServerEvent += events_OnTvServerEvent;
                  Log.Debug("TVServerKodi: Registered OnTvServerEvent with TV Server");
                }

                connected = StartListenThread();
              }
              else
              {
                  Log.Error("TVServerKodi: TVServerConnection init failed.");
              }
          }
          catch
          {
              Log.Error("TVServerKodi: TVServerConnection.Init failed!");
              Console.WriteLine("TVServerConnection.Init failed!");
          }
      }

      /// <summary>
      /// Stops the plugin
      /// </summary>
      public void Stop()
      {
        if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
        {
          Log.Debug("TVServerKodi: Unregistered OnTvServerEvent with TV Server");
          GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent -= events_OnTvServerEvent;
        }

        StopListenThread();

        Log.WriteFile("TVServerKodi: plugin stopped");
      }

      private bool StartListenThread()
      {
        Log.Info("TVServerKodi: Start listening on port " + m_serverPort);
        Console.WriteLine("TVServerKodi: Start listening on port " + m_serverPort);

        // start a thread for the listener

        m_listener = new Listener(m_serverPort);
        m_listener.StartListening();

        return false;
      }

      private void StopListenThread()
      {
        if (m_listener != null)
        {
          if (connected)
            TVServerConnection.CloseAll();

          Log.Info("TVServerKodi: Stop listening");

          m_listener.Stop();
          m_listener = null;
        }
      }

      /// <summary>
      /// returns the setup sections for display in SetupTv
      /// </summary>
      public SetupTv.SectionSettings Setup
      {
        get
        {
          SetupForm setupForm = new SetupForm();
          setupForm.Plugin = this;
          return setupForm;
        }
      }

      #endregion IPlugin Methods

      #region TvServer Events
      private void events_OnTvServerEvent(object sender, EventArgs eventArgs)
      {
        TvEngine.Events.TvServerEventArgs args = eventArgs as TvEngine.Events.TvServerEventArgs;

        if (args == null)
          return;

        Log.Debug("TVServerKodi: OnTvServerEvent: " + args.EventType.ToString());

        if (args.EventType == TvEngine.Events.TvServerEventType.ImportEpgPrograms
            && args.EpgChannel != null
            && args.EpgChannel.Programs.Count > 0)
        {
          try
          {
            if (args.channel != null)
              Log.Info("TVServerKodi: EPG import for channel: " + args.channel.Name);

            TvLibrary.Channels.DVBBaseChannel dvbChannel = args.EpgChannel.Channel as TvLibrary.Channels.DVBBaseChannel;
            if (dvbChannel != null)
            {
              TvDatabase.TvBusinessLayer layer = new TvDatabase.TvBusinessLayer();
              TvDatabase.Channel mpChannel = layer.GetChannelByTuningDetail(dvbChannel.NetworkId, dvbChannel.TransportId, dvbChannel.ServiceId);
              if (mpChannel != null)
              {
                Log.Debug("TVServerKodi: received {0} programs on {1}", args.EpgChannel.Programs.Count, mpChannel.DisplayName);
                //foreach (TvLibrary.Epg.EpgProgram p in args.EpgChannel.Programs)
                //{
                //  Log.Info("TVServerKodi: program: " + p.StartTime.ToString() + "-" + p.EndTime.ToString());
                //}
              }
            }
            //ImportEpgPrograms(args.EpgChannel);
          }
          catch (Exception ex)
          {
            Log.Error("TVServerKodi: ImportEpgPrograms(): {0}", ex.Message);
          }
        }
      }
      #endregion

      #region Local methods

      internal void LoadSettings()
      {
        try
        {
          TvDatabase.TvBusinessLayer layer = new TvDatabase.TvBusinessLayer();
          m_serverPort = Convert.ToInt32(layer.GetSetting("TVServerKodi.port", m_defaultServerPort.ToString()).Value);
        }
        catch (Exception ex)
        {
          m_serverPort = m_defaultServerPort;
          Log.Error("TVServerKodi: LoadSettings(): {0}", ex.Message);
        }
      }

      internal void SaveSettings()
      {
        bool forceReload = false;

        try
        {
          TvDatabase.TvBusinessLayer layer = new TvDatabase.TvBusinessLayer();
          TvDatabase.Setting setting;

          setting = layer.GetSetting("TVServerKodi.port");
          if (Convert.ToInt32(setting.Value) != m_serverPort)
          {
            Log.Info("TVServerKodi: port setting changed from " + setting.Value + " to " + m_serverPort);
            setting.Value = m_serverPort.ToString();
            setting.Persist();
            forceReload = true;
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVServerKodi: SaveSettings(): {0}", ex.Message);
        }

        if (forceReload)
        {
          StopListenThread();
          StartListenThread();
        }
      }

      #endregion
    }
}
