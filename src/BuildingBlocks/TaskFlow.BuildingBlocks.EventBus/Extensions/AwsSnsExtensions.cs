
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using TaskFlow.BuildingBlocks.EventBus.Abstractions;
//using TaskFlow.BuildingBlocks.EventBus.Adapters;
//using TaskFlow.BuildingBlocks.EventBus.Configuration;

//namespace TaskFlow.BuildingBlocks.EventBus.Extensions;

//public static class AwsSnsExtensions
//{
//    /// <summary>
//    /// Adds the AWS SNS Event Bus implementation. 
//    /// Note: You must also register a concrete IEventPublisher (e.g., AddMediatREventPublisher) for in-process events.
//    /// </summary>
//    public static IServiceCollection AddAwsSnsEventBus(this IServiceCollection services, IConfiguration configuration)
//    {
//        services.Configure<AwsSnsOptions>(configuration.GetSection(AwsSnsOptions.SectionName));

//        services.AddScoped<IEventBus, AwsSnsEventBusAdapter>();

//        return services;
//    }
//}
