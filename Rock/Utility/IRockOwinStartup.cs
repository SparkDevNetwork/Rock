

using Owin;

namespace Rock.Utility
{
    /// <summary>
    /// Interface that classes can implement to have code run at Rock startup
    /// </summary>
    public interface IRockOwinStartup
    {
        /// <summary>
        /// All IRockStartup classes will be run in order by this value. If class does not depend on an order, return zero.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        int StartupOrder { get; }

        /// <summary>
        /// Method that will be run at Rock Owin startup
        /// </summary>
        void OnStartup( IAppBuilder app );
    }
}
