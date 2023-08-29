using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace SteamInvisibleOnFirstLogin
{
    public class Program
    {
        public static string SteamPath, SteamPathUserdata, SteamPathLocalVDF;

        private static string SteamID;

        private static int PointCounter;

        static void Main(string[] Arguments)
        {
            Console.WriteLine("Steam Invisible On First Login - Build: {0}", DateTime.Today);

            Console.WriteLine();

            Console.WriteLine("http://github.com/MouchesVolantes/");

            Console.WriteLine();

            //Parse the command line arguments

            if (Arguments.Length > 0) 
            {
                string steamPath;

                if (Arguments.Length > 1)
                {
                    steamPath = String.Join(" ", Arguments);
                }
                else
                {                    
                    steamPath = Arguments[0];
                }

                if (Path.IsPathRooted(steamPath))
                {
                    try
                    {
                        SteamPath = Path.GetFullPath(steamPath);

                        Console.WriteLine("Command line argument parsed: {0}\n", SteamPath);
                    }
                    catch
                    {
                        Console.WriteLine("Error parsing command line arguments: {0} is not a valid path.\n", steamPath);
                    }
                }
                else
                {
                    Console.WriteLine("Error parsing command line arguments: {0} is not a valid path.\n", steamPath);
                }
            }
            
            PrintSeperator();

            Console.WriteLine();

            //Get the Steam path from running processes if it still null

            if (String.IsNullOrEmpty(SteamPath))
            {
                if (SteamEXE.IsSteamRunning())
                {
                    try
                    {
                        SteamPath = SteamEXE.GetSteamPath();
                    }
                    catch
                    {
                        Console.WriteLine("Could not retrieve the Steam path from running processes.");

                        Console.WriteLine();

                        Console.WriteLine("Using the default values for Steam path.");

                        Console.WriteLine();
                    }
                }
            }

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

            CheckSteamPath:

            String SteamPathExecutable = Path.Combine(SteamPath, "steam.exe");

            if (Directory.Exists(SteamPath) && File.Exists(SteamPathExecutable))
            {
                Console.WriteLine("Steam installation found at {0}", SteamPath);

                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Steam installation could not be found at {0}", SteamPath);

                Console.WriteLine();

                Console.WriteLine("Please enter the path where Steam is installed.");

                Console.WriteLine();

                Console.WriteLine("Example: {0,-35}{1}", @"C:\Program Files (x86)\Steam", "(Default Steam path for 64-bit)");

                Console.WriteLine("Example: {0,-35}{1}", @"C:\Program Files\Steam", "(Default Steam path for 32-bit)");

                Console.WriteLine();

                SteamPath = Console.ReadLine();

                Console.WriteLine();

                PrintSeperator();

                Console.WriteLine();

                goto CheckSteamPath;
            }

            PrintSeperator();

            Console.WriteLine();

            //SteamPath should have been set at this point

            SteamPathUserdata = Path.Combine(SteamPath, @"userdata");

            SteamPathLocalVDF = Path.Combine(SteamPath, @"userdata\%STEAMID%\config\localconfig.vdf");

            //User instructions

            Console.WriteLine("Log in to your Steam account.");

            Console.WriteLine();

            //Loop 1: Wait for folder creation

            PointCounter = 0;

            Console.Write("Waiting for login");

            while (!Directory.Exists(SteamPathUserdata))
            {
                JustLoop("Waiting for login");
            }

            List<String> UserdataFoldersAtStart = Directory.GetDirectories(SteamPathUserdata).ToList();

            List<String> UserdataFoldersCurrent = Directory.GetDirectories(SteamPathUserdata).ToList();

            List<String> Difference = UserdataFoldersCurrent.Except(UserdataFoldersAtStart).ToList();

            while (Difference.Count == 0)
            {
                UserdataFoldersCurrent = Directory.GetDirectories(SteamPathUserdata).ToList();

                Difference = UserdataFoldersCurrent.Except(UserdataFoldersAtStart).ToList();

                JustLoop("Waiting for login");
            }

            SteamID = new DirectoryInfo(Difference[0]).Name;

            ClearConsoleLine();

            Console.WriteLine("Waiting for login: OK");

            Console.WriteLine();

            ///Loop 2: Wait for localconfig.vdf creation

            PointCounter = 0;

            Console.Write(@"Waiting for userdata\{0}\config\localconfig.vdf", SteamID);

            SteamPathLocalVDF = SteamPathLocalVDF.Replace("%STEAMID%", SteamID);

            while (!File.Exists(SteamPathLocalVDF))
            {
                string Message = String.Format(@"Waiting for userdata\{0}\config\localconfig.vdf", SteamID);

                JustLoop(Message);
            }

            ClearConsoleLine();

            Console.WriteLine(@"Waiting for userdata\{0}\config\localconfig.vdf: OK", SteamID);

            Console.WriteLine();

            //Loop 3: Exit Steam

            PointCounter = 0;

            Console.Write("Shutting down Steam");

            SteamEXE.KillSteam();

            while (SteamEXE.IsSteamRunning())
            {
                JustLoop("Shutting down Steam");
            }

            ClearConsoleLine();

            Console.WriteLine("Shutting down Steam: Success");

            Console.WriteLine();

            Thread.Sleep(1000);

            //Set online status to invisible

            string SetPersonaState = SteamVDF.SetPersonaState(SteamID);

            ClearConsoleLine();

            Console.WriteLine("Set online status to invisible: {0}", SetPersonaState);

            Console.WriteLine();

            //Check online status

            string PersonaName = SteamVDF.GetPersonaName(SteamID);

            string PersonaState = SteamVDF.GetPersonaState(SteamID);

            Console.WriteLine("{0,-15}{1}", "Name", PersonaName);

            Console.WriteLine("{0,-15}{1}", "Steam ID", SteamID);

            Console.WriteLine("{0,-15}{1}", "Next Login", PersonaState);

            Console.WriteLine();

            //Close the terminal

            Console.Write("Press any key to exit...");

            Console.ReadKey();
        }

        public static void ClearConsoleLine()
        {
            int CurrentLine = Console.CursorTop;

            Console.SetCursorPosition(0, Console.CursorTop);

            Console.Write(new string(' ', Console.WindowWidth));

            Console.SetCursorPosition(0, CurrentLine);
        }

        public static void PrintSeperator(int n = 75)
        {
            for (int i = 0; i < n; i++)
            {
                Console.Write("-");
            }

            Console.WriteLine();
        }

        public static void JustLoop(string Message, int SleepLength = 25)
        {
            if (PointCounter < 40)
            {
                PointCounter++;

                if (PointCounter % 10 == 0 && PointCounter < 40)
                {
                    Console.Write(".");
                }
            }
            else
            {  
                ClearConsoleLine();

                PointCounter = 0;

                Console.Write(Message);
            }

            Thread.Sleep(SleepLength);
        }
    }
}
