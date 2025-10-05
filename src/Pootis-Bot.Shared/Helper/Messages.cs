namespace Pootis_Bot.Shared.Helper;

public static class Messages
{
    public static string CreateSuccessful(string objectName) =>
        $"✔️ Successfully created new {objectName}!";
    
    public static string CreatedFailed(string objectName) =>
        $"❌ Failed creating {objectName}! Please try again later.";
    
    public static string DeleteSuccessful(string objectName) =>
        $"🗑️ Successfully deleted {objectName}!";
    
    public static string DeleteFailed(string objectName) =>
        $"❌ Failed deleting {objectName}! Please try again later.";
}