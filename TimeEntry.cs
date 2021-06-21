using System;
using TLCSProj.EntryInfo;

namespace TLCSProj.Core.Time
{
    internal class TimeEntry
    {
        internal EntryType Type { get; private set; } = EntryType.NULL;
        internal ConsoleColor TextColor { get; private set; } = ConsoleColor.White;
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
        internal TimeEntry(EntryType type, object value = null, ConsoleColor textColor = ConsoleColor.White)
        {
            EntryMessage = $">> {GetTimeStamp()} {_entryHeader[(int)type % _entryHeader.Length]}" + ((value != null) ? $" | {value}" : "");
            Type = type;
            TextColor = textColor;
        }

        internal TimeEntry(object userEntryMessage, ConsoleColor textColor = ConsoleColor.White)
        {
            EntryMessage = $">> {GetTimeStamp()} {userEntryMessage}";
            Type = default;
            TextColor = textColor;
        }

        string GetTimeStamp()
        {
            return $"({DateTime.Now.ToLongTimeString()})";
        }
    }

    
}
