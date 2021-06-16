using System;
using TLCSProj.EntryInfo;

namespace TLCSProj.Core.Time
{
    internal class TimeEntry
    {
        internal EntryType Type { get; private set; } = EntryType.NULL;
        internal string EntryMessage { get; private set; }
        string[] _entryHeader =
        {
            "",
            "POST",
            "PUNCH IN RECEIVED",
            "PUNCH OUT RECIEVED",
            "SYSTEM POST RECEIVED",
            "SYSTEM ERROR RECEIVED",
            "PROCESS START REQUEST"
        };
        internal TimeEntry(EntryType type, object value = null)
        {
            if(type == default)
            {
                new TimeEntry(value.ToString());
                return;
            }
            EntryMessage = $">> {GetTimeStamp()} {_entryHeader[(int)type % _entryHeader.Length]}" + ((value != null) ? $" | {value}" : "");
            Type = type;
        }
        internal TimeEntry(string entryHeader, object value = null)
        {
            EntryMessage = $">> {GetTimeStamp()} {entryHeader} | {value}";
        }

        internal TimeEntry(string userEntryMessage)
        {
            EntryMessage = $">> {GetTimeStamp()} {userEntryMessage}";
        }

        string GetTimeStamp()
        {
            return $"({DateTime.Now.ToLongTimeString()})";
        }
    }

    
}
