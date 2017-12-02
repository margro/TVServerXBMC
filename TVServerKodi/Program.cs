using System;
using TvControl;


namespace TVServerKodi.Commands
{
    // simple wrapper to let us run it as a program
    // allowing for much faster debugging

    class Program
    {
        //static string hostname = "htpc";    //hostname of the TV server
        //static int listenport = 9595;       //local port for the MPTV XBMC/Kodi server

        static string hostname = "localhost";
        static bool connected = false;
        static TvControl.IController controller;
        static string databaseConnectionString;
        static string databaseProvider;

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
            controller = RemoteControl.Instance;

            try
            {
                 Console.WriteLine("MediaPortal TVServer version: " + controller.GetAssemblyVersion.ToString());
            }
            catch
            {
            }

            // Display information about the EXE assembly.
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

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

            try
            {
              controller.GetDatabaseConnectionString(out databaseConnectionString, out databaseProvider);
              Console.WriteLine(databaseProvider + ":" + databaseConnectionString);
              Console.WriteLine("Set Gentle framework provider to " + databaseProvider);
              Console.WriteLine("Set Gentle framework provider string to " + databaseConnectionString);
              Gentle.Framework.ProviderFactory.SetDefaultProvider(databaseProvider);
              Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(databaseConnectionString);
            }
            catch (Exception ex)
            {
              Console.WriteLine("An exception occurred while connecting to the database. Did you select the right backend? TVServerKodi default=MySQL; change your Gentle.conf file if you are using MSSQL.");
              try
              {
                Console.WriteLine("The exception: " + ex.ToString());
              }
              catch { }
            }

            TVServerKodiPlugin plugin = new TVServerKodiPlugin();

            Console.WriteLine("Starting debug version of the TVServerKodi plugin");
            plugin.Start(controller);

            if (plugin.Connected)
            {
                Console.WriteLine("Running MediaPortal TV Server -> Kodi wrapper at port: " + plugin.Port);
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
