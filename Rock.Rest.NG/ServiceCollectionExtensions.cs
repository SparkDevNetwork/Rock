using Microsoft.Extensions.DependencyInjection;

namespace Rock.Rest
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds any required services for the Rock Rest API.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddRockApi( this IServiceCollection serviceCollection )
        {
            serviceCollection.AddScoped<EntityControllerDependencies>();

            return serviceCollection;
        }
    }
}
