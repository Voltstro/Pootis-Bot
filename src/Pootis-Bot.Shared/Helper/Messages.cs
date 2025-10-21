namespace Pootis_Bot.Shared.Helper;

public static class Messages
{
    // 
    // Creating
    //
    
    public static string CreateSuccessful(string objectName) =>
        $"✔️ Successfully created new {objectName}!";
    
    public static string CreatedFailed(string objectName) =>
        $"❌ Failed creating {objectName}! Please try again later.";
    
    //
    // Setting
    //
    
    public static string SetSuccessful(string objectName, string value) =>
        $"✔️ Successfully set {objectName} to {value}!";
    
    public static string SetFailed(string objectName) =>
        $"❌ Failed setting {objectName}!";
    
    //
    // Deleting
    //
    
    public static string DeleteSuccessful(string objectName) =>
        $"🗑️ Successfully deleted {objectName}!";
    
    public static string DeleteFailed(string objectName) =>
        $"❌ Failed deleting {objectName}! Please try again later.";

    //
    // Validation
    //
    
    public static string ValidationFailed(string parameterName, string requirement) =>
        $"❌ {parameterName} is not valid! Needs to be {requirement}.";
    
    //
    // Features
    //
    
    public static string FeatureEnabled(string featureName) =>
        $"✔️ Successfully enabled {featureName}!";
    
    public static string FeatureDisabled(string featureName) =>
        $"✔️ Successfully disabled {featureName}!";
    
    public static string FeatureCannotBeEnable(string featureName, string requirement) =>
        $"❌ {featureName} cannot be enabled! Requires {requirement}.";
}