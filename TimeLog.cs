using System;
using System.Collections.Generic;
using TLCSProj.EntryInfo;

namespace TLCSProj.Core.Time
{
    internal class TimeLog
    {
        List<TimeEntry> _log;

        internal TimeLog()
        {
            _log = new List<TimeEntry>();
            AddNewEntry("New Time Log Initiated.");
        }

        internal List<TimeEntry> Get() => _log;

        internal TimeEntry AddNewEntry(EntryType type, object value = null, ConsoleColor color = ConsoleColor.White)
        {
            TimeEntry newEntry = type != EntryType.NULL ? new TimeEntry(type, value, color) : new TimeEntry(value, color);
            _log.Add(newEntry);
            return newEntry;
        }

        internal TimeEntry AddNewEntry(string entryMessage, ConsoleColor color = ConsoleColor.White)
        {
            TimeEntry newEntry = new TimeEntry(entryMessage);
            _log.Add(newEntry);
            return newEntry;
        }
    }
}
