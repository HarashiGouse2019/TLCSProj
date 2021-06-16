using System;
using TLCSProj.Net;
using TLCSProj.EntryInfo;
using TLCSProj.Core.Time;
using System.Diagnostics;
using System.ServiceProcess;
using System.Collections.Generic;

namespace TLCSProj.Core
{
    internal class Session
    {
        internal static Session Instance;

        internal static string UserID { get; private set; } = null;
        internal static string CurrentTerminalName { get; private set; } = null;
        internal static string CurrentIPAddress { get; private set; } = null;
        internal static string TotalProcesses { get; private set; } = null;
        internal static string TotalServices { get; private set; } = null;


        internal static string LastPunchIn = "00:00.00"; 
        internal static string LastPunchOut = "00:00.00";
        internal static string TotalRuntime = "00:00.00";

        internal const bool OK = true;
        internal const bool FAIL = false;
        internal const string SPAN_FMT = @"hh\:mm\.ss";

        internal string _commandString;

        static SessionCallback[] _SessionCallbacks;
        internal static Stopwatch SessionRuntime { get; private set; }
        internal static Stopwatch CumulativeSessionRuntime { get; private set; }
        static bool _IncludeSystemEvents = false;
        static bool _inSession = false;
        static Process[] _TrackedProcesses;
        
        static readonly SessionCallbackMethods[] CommandMethodList = new SessionCallbackMethods[13]{
                //Post command (args: string)
                () =>
                {
                    _storedArgs[1] = Instance._commandString.Split(' ', 2, StringSplitOptions.None)[1];
                    SessionTimeLog.AddNewEntry(EntryType.POST, _storedArgs[1]);
                },

                //Login Command (void)
                () =>
                {
                    _inSession = true;
                    SessionTimeLog.AddNewEntry(EntryType.PUNCHIN);
                    SessionRuntime = Stopwatch.StartNew();
                    CumulativeSessionRuntime = Stopwatch.StartNew();
                    MarkPunchIn();
                },

                //Rest Command (void)
                () =>
                {
                    SessionTimeLog.AddNewEntry(EntryType.PUNCHOUT, $"{GetSessionRuntime()} | " +
                        $"{GetCumulativeSessionRuntime()}");
                    SessionRuntime.Stop();
                    CumulativeSessionRuntime.Stop();
                    MarkPunchOut();
                },

                //Resume Command (void)
                () =>
                {
                    SessionTimeLog.AddNewEntry(EntryType.PUNCHIN, $"{GetSessionRuntime()} | " +
                        $"{GetCumulativeSessionRuntime()}");
                    SessionRuntime.Restart();
                    CumulativeSessionRuntime.Start();
                    MarkPunchIn();
                },

                //Out Command (void)
                () =>
                {
                    _inSession = false;
                    SessionTimeLog.AddNewEntry(EntryType.PUNCHOUT, $"{GetSessionRuntime()} | " +
                        $"{GetCumulativeSessionRuntime()}");
                    SessionRuntime.Stop();
                    CumulativeSessionRuntime.Stop();
                    MarkPunchOut();
                },

                //Open Command (args: string)
                () =>
                {
                    if (!_IncludeSystemEvents) return;
                    _storedArgs[1] = Instance._commandString.Split(' ', 2, StringSplitOptions.None)[1];
                    //TODO: Find Service Name, and execute
                    SessionTimeLog.AddNewEntry(EntryType.SYSTEM, _IncludeSystemEvents ?
                        $"Opening {_storedArgs[1]}..." : null);
                    OpenApplication(_storedArgs[1]);
                },

                //Close Command (args: string)
                () =>
                {
                    if (!_IncludeSystemEvents) return;
                    _storedArgs[1] = Instance._commandString.Split(' ', 2, StringSplitOptions.None)[1];
                    //TODO: Find Service Name, and close
                    SessionTimeLog.AddNewEntry(EntryType.SYSTEM, _IncludeSystemEvents ?
                        $"Closing {_storedArgs[1]}..." : null);
                    CloseApplication(_storedArgs[1]);
                },

                //Hotkey Command (args: char, int)
                () =>
                {
                    if (!_IncludeSystemEvents) return;

                    SessionTimeLog.AddNewEntry(EntryType.SYSTEM, _IncludeSystemEvents ?
                        $"Key {_storedArgs[1]} set to {(CommandList)Convert.ToInt32(_storedArgs[2])} command.\n" +
                        $"Hold ALT then the hotkey you've registered." : null);
                },

                //System Listen Command (args: bool)
                () =>
                {
                    //TODO: Enable Log to listen to System Events or Entry Types
                    _IncludeSystemEvents = bool.Parse(_storedArgs[1]);

                    SessionTimeLog.AddNewEntry(EntryType.SYSTEM, _IncludeSystemEvents ?
                        $"System Now Listening. System Events will now be logged." :
                        "System Has Stopped Listening. System Events will not be logged.");
                },

                //Log Target Command (args: string)
                () =>
                {
                    //TODO: Change directory where log info will be printed
                },

                //Session Time Command (void)
                () =>
                {
                    SessionTimeLog.AddNewEntry(EntryType.NULL,
                        $"{GetSessionRuntime()} | " +
                        $"{GetCumulativeSessionRuntime()} | " +
                        $"Total Hours is {CumulativeSessionRuntime.Elapsed.TotalHours}");
                },

                //Print Command (void)
                () =>
                {
                    //TODO: Open Print Service, and set print job for Target File 
                },
                
                //End Command (void)
                () =>
                {
                    //TODO: End Session
                    _inSession = false;
                    SessionTimeLog .AddNewEntry(EntryType.PUNCHOUT, "End of Time Logging Session!");

                    //TODO: Print cumulative stats, and generate log file
                }
            };

        private static void OpenApplication(string applicationPath)
        {
            //Start application
            try
            {
                Process.Start(applicationPath);
                _TrackedProcesses = Process.GetProcesses();
            } catch
            {
                if (!_IncludeSystemEvents) return;
                SessionTimeLog.AddNewEntry(EntryType.SYSTEM, $"Failed to execute process {applicationPath}...");
            }
        }

        private static void CloseApplication(string applicationPath)
        {
            //Close Application
            foreach(Process process in _TrackedProcesses)
            {
                if (process.ProcessName == applicationPath)
                    process.Close();
            }
        }

        enum CommandList
        {
            NULL,
            POST,
            IN,
            REST,
            RESUME,
            OUT,
            OPEN,
            CLOSE,
            HOTKEY,
            SYSLIS,
            LOGTAR,
            SESTIM,
            PRINT,
            END
        }

        static string[] _storedArgs = null;

        internal static TimeLog SessionTimeLog;
        
        internal static void Validate(string[] args)
        {
            _storedArgs = args;

            //First argument will be command call
            
            CallCommand(args[0]);
        }

        static void CallCommand(string command)
        {
            for (int index = 1; index < CommandMethodList.Length; index++)
            {
                if (command.ToUpper().Contains(_SessionCallbacks[index].Code))
                {
                    _SessionCallbacks[index].Trigger();
                }
            }
        }
        internal Session()
        {
            SessionTimeLog = new TimeLog();

            UserID = Environment.UserName;
            CurrentTerminalName = Environment.MachineName;
            CurrentIPAddress = Network.IsConnected ? Network.GetLocalIPAddress() : "Unknown";
            TotalProcesses = Process.GetProcesses().Length.ToString();
            TotalServices = ServiceController.GetServices().Length.ToString();
           
            InitCommands();

            SessionRuntime = Stopwatch.StartNew();
            CumulativeSessionRuntime = Stopwatch.StartNew();

            Instance = this;
        }

        static void InitCommands()
        {
            var commandListCount = Enum.GetValues(typeof(CommandList)).Length - 1;

            _SessionCallbacks = new SessionCallback[commandListCount];

            for (int index = 0; index < commandListCount; index++)
            {
                SessionCallback newCommandEntry = null;
                if(index > 0)
                    newCommandEntry = SessionCallback.Create(((CommandList)index).ToString(), CommandMethodList[index-1]);
                _SessionCallbacks[index] = newCommandEntry;
            }
        }

        internal static string GetSessionRuntime()
        {
            TimeSpan sessionTimeSpan = SessionRuntime.Elapsed;
            return $"SESSION RUNTIME: {sessionTimeSpan.ToString(SPAN_FMT)}";
        }

        internal static string GetCumulativeSessionRuntime()
        {
            TimeSpan cumulativeSessionTimeSpan = CumulativeSessionRuntime.Elapsed;
            return $"CUMULATIVE SESSION RUNTIME: {cumulativeSessionTimeSpan.ToString(SPAN_FMT)}";
        }

        static void MarkPunchIn() => LastPunchIn = DateTime.Now.ToLongTimeString();
        static void MarkPunchOut() => LastPunchOut = DateTime.Now.ToLongTimeString();
        
    }


}
