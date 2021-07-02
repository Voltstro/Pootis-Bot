using System;
using System.Collections.Generic;
using Pootis_Bot.Config;
using Pootis_Bot.Module.Reminders.Entities;

namespace Pootis_Bot.Module.Reminders
{
    public class RemindersConfig : Config<RemindersConfig>
    {
        public List<Reminder> Reminders { get; set; } = new();

        public Reminder AddReminder(ulong userId, string message, string messageUrl, DateTime startTime, DateTime endTime)
        {
            Reminder reminder = new Reminder
            {
                UserId = userId,
                Message = message,
                MessageUrl = messageUrl,
                EndTime = endTime,
                StartTime = startTime
            };
            
            Reminders.Add(reminder);
            return reminder;
        }
    }
}