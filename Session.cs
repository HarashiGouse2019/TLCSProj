using System;
using TLCSProj.Net;
using TLCSProj.EntryInfo;
using TLCSProj.Core.Time;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;

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
        internal const string REG_SUB_KEY_ALIAS = "Software\\TLCS\\Alias";
        internal const char MULTI_ALIAS_DELIMITER = '|';

        internal string _commandString;

        static SessionCallback[] _SessionCallbacks;
        internal static Stopwatch SessionRuntime { get; private set; }
        internal static Stopwatch CumulativeSessionRuntime { get; private set; }
        static bool _IncludeSystemEvents = false;
        static bool _inSession = false;
        static Process[] _TrackedProcesses;
        static RegistryKey? aliasKey;
        static RegistrySecurity sec = new RegistrySecurity();
        static bool IsAdmin
        {
            get
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        static readonly SessionCallbackMethods[] CommandMethodList = {
                //Post command (args: string)
                () =>
                {
                    _storedArgs[1] = Instance._commandString.Split('\"', StringSplitOptions.None)[1];
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

                    _storedArgs[1] = Instance._commandString.Split('\"', StringSplitOptions.None)[1];

                    /*TODO: MultiProcess Alias Feature
                     * It's really easy. It's the same thing as a normal alias,
                     * except you combine your links into a single string, being sure
                     * to seperate each file with |
                     * EXAMPLE 
                     * newali 
                     * "E:\SteamLibrary\steamapps\common\琴葉姉妹とライサント島の伝説\kotonoha\kotonoha.exe|
                     * C:\Users\tclte\AppData\Local\Programs\Microsoft VS Code\Code.exe|
                     * C:\Users\tclte\AppData\Local\Discord\Update.exe" multiAlias
                     * Have command detect | And if so, split and enter a foreach loop.
                            */
                    SessionTimeLog.AddNewEntry(EntryType.PROCESSSTARTREQUEST, (_IncludeSystemEvents ?
                        $"Opening {_storedArgs[1]}" : null) + "...");

                    OpenProcess(_storedArgs[1]);
                },

                //Close Command (args: string)
                () =>
                {
                    if (!_IncludeSystemEvents) return;

                    _storedArgs[1] = Instance._commandString.Split('\"', StringSplitOptions.None)[1];

                    //TODO: Find Service Name, and close
                    SessionTimeLog.AddNewEntry(EntryType.SYSTEMPOST, (_IncludeSystemEvents ?
                        $"Closing {_storedArgs[1]}" : null) + "...");

                    CloseProcess(_storedArgs[1]);
                },

                //Hotkey Command (args: char, int)
                () =>
                {
                    if (!_IncludeSystemEvents) return;

                    SessionTimeLog.AddNewEntry(EntryType.SYSTEMPOST, _IncludeSystemEvents ?
                        $"Key {_storedArgs[1]} set to {(CommandList)Convert.ToInt32(_storedArgs[2])} command.\n" +
                        $"Hold ALT then the hotkey you've registered." : null);
                },

                //System Listen Command (args: bool)
                () =>
                {
                    //TODO: Enable Log to listen to System Events or Entry Types
                    _IncludeSystemEvents = bool.Parse(_storedArgs[1]);

                    SessionTimeLog.AddNewEntry(EntryType.SYSTEMPOST, _IncludeSystemEvents ?
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

                //New Alias Command (params string[], string)
                () =>
                {
                    if (!_IncludeSystemEvents) return;

                    try
                    {
                        _storedArgs[1] = Instance._commandString.Split('\"', StringSplitOptions.None)[1];
                        _storedArgs[2] = Instance._commandString.Split('\"', StringSplitOptions.None)[2];

                        //Add New SubKey if opening fails
                        aliasKey = Registry.LocalMachine.OpenSubKey(REG_SUB_KEY_ALIAS, IsAdmin);

                        aliasKey.SetValue(_storedArgs[2], _storedArgs[1]);

                        aliasKey.Close();

                        SessionTimeLog.AddNewEntry(EntryType.SYSTEMPOST, $"Alias {_storedArgs[2]} added successfully!\nProcess(s): {_storedArgs[1]}");
                    }
                    catch (AccessViolationException ave)
                    {
                        SessionTimeLog.AddNewEntry(EntryType.SYSTEMERROR, $"Failed to add new alias {_storedArgs[2]}.\nREASON CODE: {ave.Message}");
                    }
                    catch (Exception e)
                    {
                        SessionTimeLog.AddNewEntry(EntryType.SYSTEMERROR, $"Failed to add new alias {_storedArgs[2]}.\nREASON CODE: {e.Message}\nDoes AliasKey exist?\nIf not, use command \"genalikey\"");
                    }
                },

                //Generate Alias Registry (void)
                () =>
                {
                    try
                    {
                        if (!_IncludeSystemEvents) return;

                        if(Registry.LocalMachine.OpenSubKey(REG_SUB_KEY_ALIAS, true) == null ){
                            aliasKey = Registry.LocalMachine.CreateSubKey(REG_SUB_KEY_ALIAS, true);
                            aliasKey.SetAccessControl(sec);
                            aliasKey.Close();
                            SessionTimeLog.AddNewEntry(EntryType.SYSTEMPOST, $"Alias Registry Key has been generated...");
                        }

                        SessionTimeLog.AddNewEntry(EntryType.SYSTEMPOST, $"Alias Registry Key has already been generated.");
                    }
                    catch (Exception e)
                    {
                        SessionTimeLog.AddNewEntry(EntryType.SYSTEMERROR, $"Failed to generate Alias Registry Key. Are you an Admin?\nREASON CODE: {e.Message}");
                    }
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

        private static void OpenProcess(string applicationPath)
        {
            //Start application
            try
            {
                aliasKey = Registry.LocalMachine.OpenSubKey(REG_SUB_KEY_ALIAS, IsAdmin);

                string input = applicationPath;

                string processFromAlias = GetAliasProcessString(aliasKey, _storedArgs[1]);

                applicationPath = processFromAlias == string.Empty ? input : processFromAlias;

                
                aliasKey.Close();
                bool isMultiProcessAlias = applicationPath.Contains(MULTI_ALIAS_DELIMITER);
                if (isMultiProcessAlias)
                {
                    string[] processes = applicationPath.Split(MULTI_ALIAS_DELIMITER);
                    foreach(string process in processes)
                    {
                        Process.Start(process);

                        _TrackedProcesses = Process.GetProcesses();

                        SessionTimeLog.AddNewEntry(EntryType.SYSTEMPOST, $"Process {process} started successfully!");
                    }
                } else
                {
                    Process.Start(applicationPath);

                    _TrackedProcesses = Process.GetProcesses();

                    SessionTimeLog.AddNewEntry(EntryType.SYSTEMPOST, $"Process {applicationPath} started successfully!");
                }
            }
            catch
            {
                if (!_IncludeSystemEvents) return;
                SessionTimeLog.AddNewEntry(EntryType.SYSTEMERROR, $"Failed to execute process {applicationPath}...");
            }
        }

        private static void CloseProcess(string applicationPath)
        {
            //Close Application
            foreach (Process process in _TrackedProcesses)
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
            NEWALI,
            GENALIKEY,
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
            sec.AddAccessRule(
                new RegistryAccessRule($"{Environment.UserDomainName}\\{Environment.UserName}",
                    RegistryRights.ReadKey |
                    RegistryRights.WriteKey |
                    RegistryRights.Delete,

                    InheritanceFlags.None,

                    PropagationFlags.None,

                    AccessControlType.Allow
                    )
                );

            SessionTimeLog = new TimeLog();

            UserID = Environment.UserName + (IsAdmin ? $" (Admin)" : " (User)");
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
                if (index > 0)
                    newCommandEntry = SessionCallback.Create(((CommandList)index).ToString(), CommandMethodList[index - 1]);
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

        static string GetAliasProcessString(RegistryKey aliasKey, string aliasName)
        {
            for(int index = 0; index < aliasKey.GetValueNames().Length; index++)
            {
                string currentKeyName = aliasKey.GetValueNames()[index];
                if (currentKeyName.Contains(aliasName))
                {
                    return aliasKey.GetValue(currentKeyName) as string;
                }
            }

            return string.Empty;
        }

    }


}
