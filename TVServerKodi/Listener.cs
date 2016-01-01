using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using TvLibrary.Log;

namespace TVServerKodi
{
    class Listener
    {
        private TcpListener tcpListener;
        private int port;
        private List<TcpClient> m_clients;
        private List<Thread> m_communicationThreads;
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
                    Console.WriteLine("Socket error: address already in use. Check if you have other instances of the TVServerKodi running (plugin/exe)");
                    Log.Error("TVServerKodi: Socket error: address already in use. Check if you have other instances of the TVServerKodi running (plugin/exe)");
                }
                else
                {
                    Log.Error("TVServerKodi: Socket error : " + e.ToString());
                    Console.WriteLine(e.ToString());
                }
            }
            catch (Exception e)
            {
                Log.Error("TVServerKodi: Unknown error : " + e.ToString());
                Console.WriteLine(e.ToString());
            }

            return false;
        }

        public void Stop()
        {
            try
            {
                Log.Debug("TVServerKodi: tcpListener.Stop()");
                Console.WriteLine("TVServerKodi: tcpListener.Stop()");
                stopme = true;

                if (m_listenThread != null)
                {
                    Log.Debug("TVServerKodi: Listenthread is aborting");
                    m_listenThread.Abort();
                    m_listenThread = null;
                }

                if (m_communicationThreads.Count > 0)
                {
                    Log.Debug("TVServerKodi: Stop all communication threads");
                    foreach (Thread thread in m_communicationThreads)
                    {
                        thread.Abort();
                    }
                    m_communicationThreads.Clear();
                    m_communicationThreads = null;
                }

                if (m_clients.Count > 0)
                {
                    Log.Debug("TVServerKodi: Closing all client connections");
                    foreach (TcpClient client in m_clients)
                    {
                        client.Close();
                    }

                    m_clients = null;
                }
            }
            catch (Exception)
            { }
        }

        public void ListenForClients()
        {
            m_clients = new List<TcpClient>();
            m_communicationThreads =  new List<Thread>();
            cmdMutex = new Mutex();

            while (!stopme)
            {
                Console.WriteLine("Waiting for clients...");
                Log.Debug("TVServerKodi: Waiting for clients...");
                while (!this.tcpListener.Pending())
                {
                    Thread.Sleep(30);
                }

                // blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                // Multithreaded version:
                Console.WriteLine("New Connection! Starting handler thread for client." + client.Client.RemoteEndPoint.ToString());
                Log.Debug("TVServerKodi: New Connection! Starting handler thread for client." + client.Client.RemoteEndPoint.ToString());
                //___________________________
                lock(this) {
                    m_clients.Add(client);
                }

                ConnectionHandler handler = new ConnectionHandler(client, cmdMutex, m_clients.Count);
                ThreadStart thdstHandler = new ThreadStart(handler.HandleConnection); 
                Thread communicationThread = new Thread(thdstHandler);
                lock(this) {
                    m_communicationThreads.Add(communicationThread);
                }
                communicationThread.Start();
            }

            tcpListener.Stop();
        }
    }
}
