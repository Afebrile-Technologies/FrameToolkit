namespace FrameToolkit.Extensions;

public static class EventHandlerRegistrations
{
    public static IServiceCollection AddDomainEventService(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Type interfaceType = typeof(IDomainEventHandler<>);

        var types = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => interfaceType.IsAssignableFrom(t)
                        && t is { IsAbstract: false, IsInterface: false, ContainsGenericParameters: false })
            .Distinct()
            .ToList();

        foreach (var type in types)
        {
            services.AddScoped(interfaceType, type);
        }

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        return services;
    }
}
