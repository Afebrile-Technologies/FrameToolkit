namespace FrameToolkit.Extensions;

public static class CommandQueryDefinitions
{
    public static IServiceCollection AddCommandQueryDefinitions(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        RegisterDependencies(services, assemblies, typeof(IDispatcher));
        RegisterDependencies(services, assemblies, typeof(IDispatcher<,>));
        RegisterClosedGenericHandlers(services, assemblies, typeof(ICommandHandler<>));
        RegisterClosedGenericHandlers(services, assemblies, typeof(ICommandHandler<,>));
        RegisterClosedGenericHandlers(services, assemblies, typeof(IQueryHandler<,>));

        return services;
    }

    private static void RegisterDependencies(IServiceCollection services, Assembly[] assemblies, Type interfaceType)
    {
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
    }

    private static void RegisterClosedGenericHandlers(IServiceCollection services, Assembly[] assemblies, Type openGenericInterface)
    {
        var types = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false, ContainsGenericParameters: false })
            .ToList();

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface);

            foreach (var iface in interfaces)
            {
                services.AddScoped(iface, type);
            }
        }
    }
}
