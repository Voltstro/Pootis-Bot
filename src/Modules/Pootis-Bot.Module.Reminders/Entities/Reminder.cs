using System;

namespace Pootis_Bot.Module.Reminders.Entities;

public readonly struct Reminder
{
    public Reminder(ulong userId, string message, string messageUrl, DateTime endTime, DateTime startTime)
    {
        UserId = userId;
        Message = message;
        MessageUrl = messageUrl;
        EndTime = endTime;
        StartTime = startTime;
    }
        
    public ulong UserId { get; }
        
    public string Message { get; }

    public string MessageUrl { get; }
        
    public DateTime EndTime { get; }
        
    public DateTime StartTime { get; } }