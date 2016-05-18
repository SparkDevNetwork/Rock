using Owin;

namespace Rock.Plugin
{
    /// <summary>
    /// Interface for defining a plugin startup
    /// </summary>
    public interface IStartup
    {
        void Configuration(IAppBuilder app);
    }
}
