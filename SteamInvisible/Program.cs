using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SteamInvisible
{
    public class Program
    {
        public static string SteamPath, SteamPathUserdata, SteamPathLocalVDF;

        private static List<String> IgnoreID = new List<String>();

        private static bool Error = false;

        private static bool NoGUI = false;

        private static bool Start = false;

        static void Main(string[] Arguments)
        {
            Console.WriteLine("Stea m Invisible - Build: {0}", DateTime.Today);

            Console.WriteLine();

            Console.WriteLine("http://github.com/MouchesVolantes/");

            Console.WriteLine();

            if (SteamEXE.IsSteamRunning())
            {
                Console.WriteLine("Steam is running. Please exit Steam and restart the application.");

                Console.WriteLine();

                Console.Write("Press any key to exit...");

                Console.ReadKey();

                return;
            }

            ParseCommandLineArguments(Arguments);

            Console.WriteLine();

            //Use the default Steam path if it is still null

            if (String.IsNullOrEmpty(SteamPath))
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    SteamPath = @"C:\Program Files (x86)\Steam";
                }
                else
                {
                    SteamPath = @"C:\Program Files\Steam";
                }
            }

            //Check if the Steam path is correct

            String SteamPathExecutable = Path.Combine(SteamPath, "steam.exe");

            if (Directory.Exists(SteamPath) && File.Exists(SteamPathExecutable))
            {
                SteamPathUserdata = Path.Combine(SteamPath, @"userdata");

                SteamPathLocalVDF = Path.Combine(SteamPath, @"userdata\%STEAMID%\config\localconfig.vdf");

                Console.WriteLine("Steam installation found at {0}", SteamPath);

                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Steam installation could not be found at {0}", SteamPath);

                Console.WriteLine();

                Console.Write("Press any key to exit...");

                Console.ReadKey();

                return;
            }

            PrintSeperator();

            Console.WriteLine();

            if (Directory.Exists(SteamPathUserdata))
            {
                SetVisibility();
            }
            else
            {
                Console.WriteLine("Steam userdata folder could not be found at {0}", SteamPathUserdata);

                Console.WriteLine();

                Console.Write("Press any key to exit...");

                Console.ReadKey();

                return;
            }

            //Close the application if NoGUI flag is set and Error flag is not set
            
            if (NoGUI && !Error)
            {
                if (Start)
                {
                    SteamEXE.StartSteam();
                }

                return;
            }
            else
            {
                Console.WriteLine();

                Console.Write("Press any key to exit...");

                Console.ReadKey();

                if (Start)
                {
                    SteamEXE.StartSteam();
                }
            }
        }

        private static void SetVisibility()
        {
            String Header = String.Format("{0,-40}{1,-15}{2,-15}{3,-15}{4,-15}", "Name", "Steam ID", "Last Seen", "Next Login", "Ignore Flag");

            Console.WriteLine(Header);

            Console.WriteLine();

            string[] UserDataFolders = Directory.GetDirectories(SteamPathUserdata);

            foreach (string Folder in UserDataFolders)
            {
                string SteamID = new DirectoryInfo(Folder).Name;

                string PersonaName = SteamVDF.GetPersonaName(SteamID);

                string LastSeen = SteamVDF.GetPersonaState(SteamID);

                string NextLogin;

                string Ignored;

                if (IgnoreID.Contains(SteamID))
                {
                    NextLogin = LastSeen;

                    Ignored = "True";
                }
                else
                {
                    SteamVDF.SetPersonaState(SteamID);

                    NextLogin = SteamVDF.GetPersonaState(SteamID);

                    Ignored = String.Empty;
                }

                String Line = String.Format("{0,-40}{1,-15}{2,-15}{3,-15}{4,-10}", PersonaName, SteamID, LastSeen, NextLogin, Ignored);

                Console.WriteLine(Line);
            }
        }

        private static void ParseCommandLineArguments(String[] arguments)
        {
            List<String> Arguments = arguments.ToList<String>();

            if (Arguments.Contains("-nogui"))
            {
                NoGUI = true;
            }

            if (Arguments.Contains("-startsteam"))
            {
                Start = true;
            }

            if (Arguments.Contains("-path"))
            {
                try
                {
                    int i = Arguments.FindIndex(List => List.Contains("-path"));

                    String steamPath = Arguments[i + 1];

                    if (Path.IsPathRooted(steamPath))
                    {
                        try
                        {
                            SteamPath = Path.GetFullPath(steamPath);
                        }
                        catch
                        {
                            Console.WriteLine("Error parsing arguments (-path): {0} is not a correctly formatted path.\n", steamPath);

                            Error = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing arguments (-path): {0} is not an absolute path.\n", steamPath);

                        Error = true;
                    }
                }
                catch
                {
                    Console.WriteLine("Error parsing arguments (-path): Argument could not be found.\n");

                    Error = true;
                }
            }

            if (Arguments.Contains("-ignore"))
            {
                try
                {
                    int i = Arguments.FindIndex(e => e.Contains("-ignore"));

                    String ignoreID = Arguments[i + 1];

                    if (ignoreID == "-nogui" || ignoreID == "-path" || ignoreID == "-startsteam")
                    {
                        Console.WriteLine("Error parsing arguments (-ignore): Argument could not be found.\n");

                        Error = true;
                    }
                    else
                    {
                        IgnoreID = ignoreID.Split(new[] { ';', ',' , ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }
                catch
                {
                    Console.WriteLine("Error parsing arguments (-ignore): Argument could not be found.\n");

                    Error = true;
                }
            }

            Console.WriteLine("{0,-15}{1}", "-path", SteamPath);
            Console.WriteLine("{0,-15}{1}", "-nogui", NoGUI);
            Console.WriteLine("{0,-15}{1}", "-ignore", String.Join(" ", IgnoreID.ToArray()));
            Console.WriteLine("{0,-15}{1}", "-startsteam", Start);
        }

        private static void PrintSeperator(int n = 100)
        {
            for (int i = 0; i < n; i++)
            {
                Console.Write("-");
            }

            Console.WriteLine();
        }
    }
}
