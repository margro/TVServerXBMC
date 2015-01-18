using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using TvLibrary.Log;

namespace TVServerXBMC
{
    class Listener
    {
        private TcpListener tcpListener;
        private int port;
        private List<TcpClient> clients;
        private Mutex cmdMutex;
        private bool stopme = false;
        private Thread m_listenThread;

        public Listener()
        {
            this.port = 9596;
            this.tcpListener = new TcpListener(IPAddress.Any, port);
        }

        public Listener(int listenport)
        {
            this.port = listenport;
            try
            {
                this.tcpListener = new TcpListener(IPAddress.Any, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        public bool StartListening()
        {
            try
            {
                tcpListener.Start();

                // start a thread to listen for clients
                m_listenThread = new Thread(new ThreadStart(ListenForClients));
                m_listenThread.Start();

                return true;
            }
            catch (ThreadAbortException)
            {
                // a thread abort, we want to end nicely
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (e.SocketErrorCode == System.Net.Sockets.SocketError.AddressAlreadyInUse)
                {
                    Console.WriteLine("Socket error: address already in use. Check if you have other instances of the TVServerXBMC running (plugin/exe)");
                    Log.Error("TVServerXBMC: Socket error: address already in use. Check if you have other instances of the TVServerXBMC running (plugin/exe)");
                }
                else
                {
                    Log.Error("TVServerXBMC: Socket error : " + e.ToString());
                    Console.WriteLine(e.ToString());
                }
            }
            catch (Exception e)
            {
                Log.Error("TVServerXBMC: Unknown error : " + e.ToString());
                Console.WriteLine(e.ToString());
            }

            return false;
        }

        public void Stop()
        {
            Log.Debug("TVServerXBMC: tcpListener.Stop()");
            Console.WriteLine("TVServerXBMC: tcpListener.Stop()");
            stopme = true;

            if (m_listenThread != null)
            {
              Log.Debug("TVServerXBMC: Listenthread is aborting");
              m_listenThread.Abort();
              m_listenThread = null;
            }

            if (clients.Count > 0)
            {
              foreach (TcpClient client in clients)
              {
                client.Close();
              }

              clients = null;
            }
        }

        public void ListenForClients()
        {
            clients = new List<TcpClient>();
            cmdMutex = new Mutex();

            while (!stopme)
            {
                Console.WriteLine("Waiting for clients...");
                Log.Debug("TVServerXBMC: Waiting for clients...");
                while (!this.tcpListener.Pending())
                {
                    Thread.Sleep(30);
                }

                // blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                // Multithreaded version:
                Console.WriteLine("New Connection! Starting handler thread for client." + client.Client.RemoteEndPoint.ToString());
                Log.Debug("TVServerXBMC: New Connection! Starting handler thread for client." + client.Client.RemoteEndPoint.ToString());
                //___________________________
                lock(this) {
                    clients.Add(client);
                }

                ConnectionHandler handler = new ConnectionHandler(client, cmdMutex, clients.Count);
                ThreadStart thdstHandler = new ThreadStart(handler.HandleConnection); 
                Thread t = new Thread(thdstHandler);
                t.Start();

                // Single threaded version:
                //___________________________
                //
                //Console.WriteLine("New Connection! Not listening for any new connections");
                //tcpListener.Stop();
                //
                //ConnectionHandler handler = new ConnectionHandler(client);
                //handler.HandleConnection();
                //
                //tcpListener.Start();
                //Console.WriteLine("Connection complete, listening for new connections...");
            }

            tcpListener.Stop();
        }
    }
}
