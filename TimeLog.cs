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

        internal TimeEntry AddNewEntry(EntryType type, object value = null)
        {
            TimeEntry newEntry = new TimeEntry(type, value);
            _log.Add(newEntry);
            return newEntry;
        }

        internal TimeEntry AddNewEntry(string header, object value = null)
        {
            TimeEntry newEntry = new TimeEntry(header, value);
            _log.Add(newEntry);
            return newEntry;
        }

        internal TimeEntry AddNewEntry(string entryMessage)
        {
            TimeEntry newEntry = new TimeEntry(entryMessage);
            _log.Add(newEntry);
            return newEntry;
        }
    }
}
