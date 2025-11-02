
namespace TaskFlow.BuildingBlocks.EventBus.Configuration;

public class AwsSnsOptions
{
    public const string SectionName = "EventBus:AwsSns";

    public string Region { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string? SessionToken { get; set; }
    public string TopicArn { get; set; } = string.Empty;
}
