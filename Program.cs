using System;
using System.Threading;

using TLCSProj.Core;
using TLCSProj.Core.Time;
using TLCSProj.UI;
using TLCSProj.Kronos;

using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

namespace TLCSProj
{
    class Program
    {
        static Thread consoleUIThread = new Thread(new ThreadStart(() => UiUpdate()));
        static Thread sessionThread;
        static void Main(string[] args)
        {
            Session targetSession = new Session();
            UIMain();
            consoleUIThread.Start();
            sessionThread = new Thread(new ThreadStart(() => ReadInput(targetSession)));
            sessionThread.IsBackground = true;
            sessionThread.Start();
        }

        static void ReadInput(Session targetSession)
        {
            int width = 0;
            while (true)
            {

                
                ConsoleDrawer.Draw(new Label(">> "), 0, 11);
                ConsoleDrawer.Draw(new Label(targetSession._commandString == string.Empty ? 
                    "ENTER COMMAND OR PRESS A HOTKEY▓ " : 
                    targetSession._commandString+"▓"+(" ").PadLeft(Console.WindowWidth, ' ')), 3, 11);
                targetSession._commandString += Console.ReadKey().KeyChar;
                width = targetSession._commandString.Length;
                if (targetSession._commandString.ToCharArray()[targetSession._commandString.Length-1] == (char)ConsoleKey.Enter){
                    string[] currentArgs = targetSession._commandString.Split(' ');
                    Session.Validate(currentArgs);
                    targetSession._commandString = string.Empty;
                    Console.Clear();
                    ConsoleDrawer.Draw(new Label(targetSession._commandString), 3, 11);

                } 
                else if(targetSession._commandString.ToCharArray()[targetSession._commandString.Length - 1] == (char)ConsoleKey.Backspace)
                {
                    width -= (width > 1) ? 1 : 0;
                    targetSession._commandString = targetSession._commandString.Substring(0, width-1) + "";
                    ConsoleDrawer.Draw(new Label(targetSession._commandString), 3, 11);
                }
            }
        }
        delegate void UiDraw();
        static UiDraw drawCallback;
        static void UIMain()
        {
             drawCallback = () =>
            {
                DateTime dt = DateTime.Now;

                Label l_Title = new Label("Time Log Console System");
                Label l_Date = new Label($"{dt.DayOfWeek} {(MonthOfYear)dt.Month} {dt.Day}, {dt.Year}");
                Label l_Time = new Label(DateTime.Now.ToLongTimeString() + $" {TimeZoneInfo.Local.Id}");
                Label l_LoggedIn = new Label($"Logged in As : {Session.UserID}");
                Label l_Computer = new Label($"Computer     : {Session.CurrentTerminalName}");
                Label l_Processes = new Label($"Processes    : {Session.TotalProcesses} Process(s) running...");
                Label l_Services = new Label($"Services     : {Session.TotalServices} Service(s) running...");
                Label l_IPAddress = new Label($"IP Address   : {Session.CurrentIPAddress}");
                Label l_LastPunchIn = new Label($"Last Punch In : {Session.LastPunchIn}");
                Label l_LastPunchOut = new Label($"Last Punch Out : {Session.LastPunchOut}");
                Label l_TotalRuntime = new Label($"Total Runtime : " + Session.CumulativeSessionRuntime.Elapsed.ToString(@"hh\:mm\.ss"));
                Label l_Line = new Label("-".PadRight(Console.WindowWidth, '-'));
                Label l_TimeLog = new Label("Time Log");

                Console.CursorVisible = false;


                ConsoleDrawer.Draw(l_Date, 0, 0);
                ConsoleDrawer.Draw(l_Title, (Console.WindowWidth / 2) - (l_Title.Size / 2), 0);
                ConsoleDrawer.Draw(l_Time, Console.WindowWidth - l_Time.Size, 0);
                ConsoleDrawer.Draw(l_Line, 0, 1);
                ConsoleDrawer.Draw(l_LoggedIn, 0, 2);
                ConsoleDrawer.Draw(l_Computer, 0, 3);
                ConsoleDrawer.Draw(l_Processes, 0, 4);
                ConsoleDrawer.Draw(l_Services, 0, 5);
                ConsoleDrawer.Draw(l_IPAddress, 0, 6);
                ConsoleDrawer.Draw(l_Line, 0, 7);
                ConsoleDrawer.Draw(l_LastPunchIn, 0, 8);
                ConsoleDrawer.Draw(l_LastPunchOut, (Console.WindowWidth / 2) - (l_LastPunchOut.Size / 2), 8);
                ConsoleDrawer.Draw(l_TotalRuntime, Console.WindowWidth - l_TotalRuntime.Size, 8);
                ConsoleDrawer.Draw(l_Line, 0, 9);
                ConsoleDrawer.Draw(Label.NewLine, 0, 10);
                ConsoleDrawer.Draw(Label.NewLine, 0, 12);
                ConsoleDrawer.Draw(l_Line, 0, 13);
                ConsoleDrawer.Draw(l_TimeLog, (Console.WindowWidth / 2) - (l_TimeLog.Size / 2), 14);

                int bottom = 0;

                //Display New Time Log Information
                for (int index = 0; index < Session.SessionTimeLog.Get().Count; index++)
                {
                    bottom = 15 + index;
                    TimeEntry entry = Session.SessionTimeLog.Get()[index];
                    Label l_Entry = new Label(entry.EntryMessage);
                    ConsoleDrawer.Draw(l_Entry, 0, bottom);
                }
            };
        }
        static void UiUpdate()
        {
            while (true)
            {
                drawCallback.Invoke();
            }
        }
    }
}
