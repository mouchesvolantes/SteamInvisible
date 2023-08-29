using System.IO;
using System.Diagnostics;

namespace SteamInvisibleOnFirstLogin
{
    internal static class SteamEXE
    {
        public static bool IsSteamRunning()
        {
            Process[] RunningProcesses = Process.GetProcesses();

            for (int i = 0; i < RunningProcesses.Length; i++)
            {
                if (RunningProcesses[i].ProcessName == "steam")
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetSteamPath()
        {
            Process[] RunningProcesses = Process.GetProcesses();

            for (int i = 0; i < RunningProcesses.Length; i++)
            {
                if (RunningProcesses[i].ProcessName == "steam")
                {
                    string FileName =  RunningProcesses[i].MainModule.FileName;

                    return Path.GetDirectoryName(FileName);
                }
            }

            return null;
        }

        public static void StartSteam()
        {
            string steamPath = Path.Combine(Program.SteamPath, "steam.exe");

            Process.Start(steamPath);
        }

        public static void ExitSteam()
        {
            if (IsSteamRunning())
            {
                string steamPath = Path.Combine(Program.SteamPath, "steam.exe");

                Process.Start(steamPath, "-shutdown");
            }
        }

        public static void KillSteam()
        {
            if (IsSteamRunning())
            {
                foreach (var Process in Process.GetProcessesByName("steam"))
                {
                    Process.Kill();
                }
            }
        }
    }
}
