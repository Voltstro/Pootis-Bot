using System;
using System.Collections.Generic;
using Pootis_Bot.Config;
using Pootis_Bot.Module.Reminders.Entities;

namespace Pootis_Bot.Module.Reminders;

public class RemindersConfig : Config<RemindersConfig>
{
    public TimeSpan ReminderMinTime { get; set; } = new(0, 0, 1); 
        
    public List<Reminder> Reminders { get; set; } = new();

    public Reminder AddReminder(ulong userId, string message, string messageUrl, DateTime startTime, DateTime endTime)
    {
        Reminder reminder = new (userId, message, messageUrl, startTime, endTime);
        Reminders.Add(reminder);
        return reminder;
    }
}