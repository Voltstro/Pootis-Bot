using System;

namespace Pootis_Bot.Module.Reminders.Entities
{
    public class Reminder
    {
        public ulong UserId { get; set; }
        
        public string Message { get; set; }

        public string MessageUrl { get; set; }
        
        public DateTime EndTime { get; set; }
        
        public DateTime StartTime { get; set; } }
}