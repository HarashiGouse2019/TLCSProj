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
        static int _CurrentPage = 1;
        static int _TotalPages = 1;

        static SessionCallbackMethods _WindowResizeEvent = () =>
        {
            _PreviousWindowSize = _WindowSize;
            Clear(0, 0, Console.BufferWidth, Console.BufferHeight);
        };

        static Thread consoleUIThread = new Thread(new ThreadStart(() => UiUpdate()));
        static Thread sessionThread;
        static int _WindowSize
        {
            get
            {
                return Console.WindowWidth * Console.WindowHeight;
            }
        }

        static int _PreviousWindowSize;
        static void Main(string[] args)
        {
            Session targetSession = new Session();
            _PreviousWindowSize = _WindowSize;
            UIMain();
            consoleUIThread.IsBackground = true;
            consoleUIThread.Start();
            sessionThread = new Thread(new ThreadStart(() => ReadInput(targetSession)));
            sessionThread.Start();
            Console.SetWindowSize(108, 38);
            Console.SetBufferSize(108, 48);
            
        }

        static void ReadInput(Session targetSession)
        {
            
            while (true)
            {
                ConsoleDrawer.Draw(new Label(">> "), 0, 11);
                ConsoleDrawer.Draw(new Label(targetSession._commandString == "" || targetSession._commandString == null ?
                    "ENTER COMMAND OR PRESS A HOTKEY▓ " :
                    targetSession._commandString + "▓" + (" ").PadLeft(Console.WindowWidth, ' ')), 3, 11);
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                targetSession._commandString += keyInfo.KeyChar;
                int width = targetSession._commandString.Length -1;
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    string[] currentArgs = targetSession._commandString.Split(' ');
                    Session.Validate(currentArgs);
                    targetSession._commandString = string.Empty;
                    Console.Clear();
                    ConsoleDrawer.Draw(new Label(targetSession._commandString), 3, 11);
                }

                else if (keyInfo.Key == ConsoleKey.Backspace)
                {

                    width -= (width > 0) ? 1 : 0;
                    targetSession._commandString = targetSession._commandString.Substring(0, width) + "";
                    ConsoleDrawer.Draw(new Label(targetSession._commandString), 3, 11);
                }

                else if (keyInfo.Key == ConsoleKey.F5)
                {
                    if (_CurrentPage > 1)
                    {
                        _CurrentPage--;
                    }
                    targetSession._commandString = string.Empty;
                    Console.Clear();
                    ConsoleDrawer.Draw(new Label(targetSession._commandString), 3, 11);
                }
                else if (keyInfo.Key == ConsoleKey.F6)
                {
                    if (_CurrentPage < _TotalPages)
                    {
                        _CurrentPage++;
                    }
                    targetSession._commandString = string.Empty;
                    Console.Clear();
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
                Label l_LoggedIn = new Label($"Logged in As : {Session.UserID} ({Session.UserStatus})");
                Label l_Computer = new Label($"Computer     : {Session.CurrentTerminalName}");
                Label l_Processes = new Label($"Processes    : {Session.TotalProcesses} Process(s) running...");
                Label l_Services = new Label($"Services     : {Session.TotalServices} Service(s) running...");
                Label l_IPAddress = new Label($"IP Address   : {Session.CurrentIPAddress}");
                Label l_LastPunchIn = new Label($"Last Punch In : {Session.LastPunchIn}");
                Label l_LastPunchOut = new Label($"Last Punch Out : {Session.LastPunchOut}");
                Label l_TotalRuntime = new Label($"Total Runtime : " + Session.CumulativeSessionRuntime.Elapsed.ToString(@"hh\:mm\.ss"));
                Label l_Line = new Label("-".PadRight(Console.WindowWidth, '-'));
                Label l_TimeLog = new Label("Time Log");
                Label pageOfPage = null;

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

                int bottom = 15;
                int startPoint = bottom;

                int logLength = Session.SessionTimeLog.Get().Count -1 ;
                int viewLength = (Console.WindowHeight - 15) - 2;

                _TotalPages = (logLength / viewLength) + 1;

                if (_CurrentPage > _TotalPages)
                    _CurrentPage = _TotalPages;

                pageOfPage = new Label($"Page(s) {_CurrentPage} of {_TotalPages}");

                //Display New Time Log Information
                for (int index = 0; index < logLength + 1; index++)
                {
                    if (index >= (viewLength * (_CurrentPage - 1)) && index < (viewLength * _CurrentPage))
                    {
                        bottom = (startPoint + (index - (viewLength * (_CurrentPage - 1))));
                        TimeEntry entry = Session.SessionTimeLog.Get()[index];
                        Label l_Entry = new Label(entry.EntryMessage);
                        ConsoleDrawer.Draw(l_Entry, 0, bottom, entry.TextColor);
                    }
                }

                

                ConsoleDrawer.Draw(l_Line, 0, Console.WindowHeight - 2);
                ConsoleDrawer.Draw(new Label("F5: PREVIOUS PAGE | F6: NEXT PAGE | F10: Enable Page Follow"), 0, Console.WindowHeight - 1);
                ConsoleDrawer.Draw(pageOfPage, Console.WindowWidth - pageOfPage.Size, Console.WindowHeight - 1);
            };
        }
        static void UiUpdate()
        {
            while (true)
            {
                if (_PreviousWindowSize != _WindowSize)
                    _WindowResizeEvent.Invoke();

                drawCallback.Invoke();
            }
        }

        static void Clear(int x, int y, int width, int height)
        {
            int curTop = Console.CursorTop;
            int curLeft = Console.CursorLeft;
            for (; height > 0;)
            {
                Console.SetCursorPosition(x, y + --height);
                Console.Write(new string(' ', width));
            }
            Console.SetCursorPosition(curLeft, curTop);
        }
    }
}
