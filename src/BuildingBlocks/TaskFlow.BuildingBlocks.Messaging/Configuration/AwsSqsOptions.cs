
namespace TaskFlow.BuildingBlocks.Messaging.Configuration;

public class AwsSqsOptions
{
    public const string SectionName = "Messaging:AwsSqs";

    public string Region { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string? SessionToken { get; set; }
    public string QueueUrl { get; set; } = string.Empty;
    public int MaxMessages { get; set; } = 10;
    public int WaitTimeSeconds { get; set; } = 20;
}
