namespace FrameToolkit.Extensions;

public interface IEndpointDefinition
{
    void RegisterEndpoints(IEndpointRouteBuilder app);
}

public static class Endpoints
{
    /// <summary>
    /// Registers all IEndpointDefinition implementations in the DI container.
    /// </summary>
    public static IServiceCollection AddEndpointDefinitions(this IServiceCollection services)
    {
        var endpointDefinitionType = typeof(IEndpointDefinition);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var endpointDefinitions = assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t != null)!;
                }
            })
            .Where(t => endpointDefinitionType.IsAssignableFrom(t)
                        && t is { IsAbstract: false, IsInterface: false, ContainsGenericParameters: false })
            .Distinct()
            .ToList();

        foreach (var defType in endpointDefinitions)
        {
            if (defType is not null)
                services.AddTransient(endpointDefinitionType, defType);
        }

        return services;
    }

    /// <summary>
    /// Resolves all registered IEndpointDefinition implementations from DI and registers their endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpointsFromDefinitions(this IEndpointRouteBuilder app)
    {
        var endpointDefinitions = app.ServiceProvider.GetServices<IEndpointDefinition>();

        foreach (var instance in endpointDefinitions)
        {
            instance.RegisterEndpoints(app);
        }

        return app;
    }
}
