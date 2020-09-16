using DotLiquid;
using Rock.Utility;

namespace com.centralaz.RoomManagement.Lava
{
    /// <summary>
    ///
    /// </summary>
    public class LavaFilters : IRockStartup
    {
        /// <summary>
        /// All IRockStartup classes will be run in order by this value. If class does not depend on an order, return zero.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int StartupOrder { get { return 0; } }

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public virtual void OnStartup()
        {
            Template.RegisterFilter( GetType() );
        }
    }
}

