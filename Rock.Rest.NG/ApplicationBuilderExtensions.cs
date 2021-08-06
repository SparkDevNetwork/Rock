using Microsoft.AspNetCore.Builder;

namespace Rock.Rest
{
    /// <summary>
    /// Extension methods for <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configures the application to use the Rock Rest API.
        /// </summary>
        /// <param name="app">The application to be configured.</param>
        /// <returns>The application.</returns>
        public static IApplicationBuilder UseRockRestApi( this IApplicationBuilder app )
        {
            return app;
        }
    }
}
