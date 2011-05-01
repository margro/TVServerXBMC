using System;
using System.Collections.Generic;
using System.Text;
using TvControl;


namespace TVServerXBMC.Commands
{
    // simple wrapper to let us run it as a program
    // allowing for much faster debugging

    class Program
    {
        //static string hostname = "htpc";    //hostname of the TV server
        //static int listenport = 9595;       //local port for the MPTV XBMC server

        static string hostname = "localhost";
        static int listenport = 9596;
        static bool connected = false;

        public static void Main(string[] args)
        {
            Console.WriteLine("TvServer Debug EXE: " + System.Windows.Forms.Application.ProductVersion.ToString());
            RemoteControl.HostName = hostname;
            connected = RemoteControl.IsConnected;

            if (!connected)
            {
                Console.WriteLine("Could not connect to the MediaPortal TVServer running on: '" + hostname + "'");
                Console.WriteLine("Check the hostname and if the TVservice is running.");
                Console.ReadLine();
                return;
            }
            
            Console.WriteLine("Connected to the MediaPortal TVServer running on: '" + hostname + "'");
            try
            {
                 Console.WriteLine("MediaPortal TVServer version: " + RemoteControl.Instance.GetAssemblyVersion.ToString());
            }
            catch
            {
            }

            // Display information about the EXE assembly.
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            //Console.WriteLine("Assembly identity={0}" + a.FullName);
            //Console.WriteLine("Codebase={0}" + a.CodeBase);

            // Display the set of assemblies our assemblies reference.
            Console.WriteLine("Referenced assemblies:");
            foreach (System.Reflection.AssemblyName an in a.GetReferencedAssemblies())
            {
                if (an.Name == "TvControl")
                {
                    Console.WriteLine("TvControl version=" + an.Version);
                }
                else if (an.Name == "TVDatabase")
                {
                    Console.WriteLine("TvDatabase version=" + an.Version);
                }
                else if (an.Name == "TvLibrary.Interfaces")
                {
                    Console.WriteLine("TvLibrary version=" + an.Version);
                }
            }

            TVServerXBMC plugin = new TVServerXBMC();
            plugin.Port = listenport;

            plugin.Start(RemoteControl.Instance);

            if(plugin.Connected)
            {
                Console.WriteLine("Running MediaPortal TV Server -> XBMC wrapper at port: " + listenport);
                try
                {
                    while (!Console.ReadLine().Contains("quit"))
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                catch
                {

                }
                plugin.Stop();
            }
            else
            {
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
                return;
            }
        }

    }
}
