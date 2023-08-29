using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SteamInvisibleOnFirstLogin
{
    internal static class SteamVDF
    {
        public static int GetLineNumberOf(string[] VDF, string Pattern)
        {
            int LineNumber = -1;

            for (int i = 0; i < VDF.Length; i++)
            {
                if (VDF[i].Contains(Pattern))
                {
                    LineNumber = i;

                    break;
                }
            }

            return LineNumber;
        }

        public static string GetPersonaName(string SteamID)
        {
            string PersonaName;

            string steamPathLocalVDF = Program.SteamPathLocalVDF.Replace("%STEAMID%", SteamID);

            string[] VDF = File.ReadAllLines(steamPathLocalVDF);

            //Retrieve the line with the key value pair: "PersonaName" "%USERNAME%"

            int LineNumber = GetLineNumberOf(VDF, "\"PersonaName\"");

            string LineString;

            if (LineNumber > 0)
            {
                LineString = VDF[LineNumber];

                try
                {
                    //Extract the string between the first and the last quotation marks after erasing "PersonaName"

                    LineString = LineString.Replace("\"PersonaName\"", String.Empty);

                    int Start = LineString.IndexOf("\"");

                    int End = LineString.LastIndexOf("\"");

                    PersonaName = LineString.Substring(Start + 1, End - Start - 1);
                }
                catch
                {
                    PersonaName = "Error: getPersonaName() -> PersonaName could not be extracted.";
                }
            }
            else
            {
                PersonaName = "Error: getPersonaName() -> Key value could not be found.";
            }

            return PersonaName;
        }

        public static string GetPersonaState(string SteamID)
        {
            string PersonaState;

            string steamPathLocalVDF = Program.SteamPathLocalVDF.Replace("%STEAMID%", SteamID);

            string[] VDF = File.ReadAllLines(steamPathLocalVDF);

            //Retrieve the line with the key value: \"ePersonaState\":

            int LineNumber = GetLineNumberOf(VDF, "\\\"ePersonaState\\\":");

            string LineString;

            if (LineNumber > 0)
            {
                LineString = VDF[LineNumber];

                try
                {
                    //Extract the number %LASTSTATE% from the pattern: \"ePersonaState\":%LASTSTATE%

                    Match Match = Regex.Match(LineString, @"\\\""ePersonaState\\\"":(\d+)");

                    if (Match.Success)
                    {
                        PersonaState = Match.Groups[1].Value;
                    }
                    else
                    {
                        PersonaState = "Error: getPersonaState() -> PersonaState could not be extracted.";
                    }
                }
                catch
                {
                    PersonaState = "Error: getPersonaState() -> PersonaState could not be extracted.";
                }
            }
            else
            {
                PersonaState = "Error: getPersonaState() -> Key value could not be found.";
            }

            PersonaState = ConvertPersonaState(PersonaState);

            return PersonaState;
        }

        public static string ConvertPersonaState(string PersonaState)
        {
            switch (PersonaState)
            {
                case "0":
                    PersonaState = "Offline";
                    break;
                case "1":
                    PersonaState = "Online";
                    break;
                case "2":
                    PersonaState = "Busy";
                    break;
                case "3":
                    PersonaState = "Away";
                    break;
                case "4":
                    PersonaState = "Snooze";
                    break;
                case "5":
                    PersonaState = "Looking to trade";
                    break;
                case "6":
                    PersonaState = "Looking to play";
                    break;
                case "7":
                    PersonaState = "Invisible";
                    break;
                default:
                    PersonaState = "Unknown";
                    break;
            }

            return PersonaState;
        }

        public static string SetPersonaState(string SteamID)
        {
            string steamPathLocalVDF = Program.SteamPathLocalVDF.Replace("%STEAMID%", SteamID);

            string[] VDF = File.ReadAllLines(steamPathLocalVDF);

            //Retrieve the line with the key value: \"ePersonaState\":

            int LineNumber = GetLineNumberOf(VDF, "\\\"ePersonaState\\\":");

            if (LineNumber > 0)
            {
                string LineString = VDF[LineNumber];

                try
                {
                    //Extract the number %LASTSTATE% from the pattern: \"ePersonaState\":%LASTSTATE%

                    Match Match = Regex.Match(LineString, @"\\\""ePersonaState\\\"":(\d+)");

                    if (Match.Success)
                    {
                        LineString = LineString.Replace(Match.Groups[0].Value, "\\\"ePersonaState\\\":7");
                    }
                    else
                    {
                        LineString = LineString.Replace("\\\"ePersonaState\\\":", "\\\"ePersonaState\\\":7");
                    }

                    VDF[LineNumber] = LineString;

                    File.WriteAllLines(steamPathLocalVDF, VDF);
                }
                catch
                {
                    return "Error: SetPersonaState() -> Key value is found but could not be modified.";
                }
            }
            else
            {
                //The key value \"ePersonaState\" could not be found in the VDF file (e.g. on the first login on the PC)
                //Insert the corresponding string into the "WebStorage" section & Create the section if it does not exist

                string Template = String.Format("\t\t\"FriendStoreLocalPrefs_{0}\"\t\t\"{{\\\"ePersonaState\\\":7,\\\"strNonFriendsAllowedToMsg\\\":\\\"\\\"}}\"", SteamID);

                LineNumber = GetLineNumberOf(VDF, @"""WebStorage""");

                List<string> ListVDF = VDF.ToList();

                if (LineNumber > 0)
                {
                    try
                    {
                        ListVDF.Insert(LineNumber + 1 + 1, Template);
                    }
                    catch
                    {
                        return "Error: SetPersonaState() -> Section WebStoreage is found but pattern could not be inserted.";
                    }
                }
                else
                {
                    try
                    {
                        ListVDF.Insert(ListVDF.Count - 1, @"""WebStorage""");
                        ListVDF.Insert(ListVDF.Count - 1, @"{");
                        ListVDF.Insert(ListVDF.Count - 1, Template);
                        ListVDF.Insert(ListVDF.Count - 1, @"}");
                    }
                    catch
                    {
                        return "Error: SetPersonaState() -> Section WebStoreage is not found and section could not be created.";
                    }
                }

                VDF = ListVDF.ToArray();

                File.WriteAllLines(steamPathLocalVDF, VDF);
            }

            return "Success";
        }
    }
}
