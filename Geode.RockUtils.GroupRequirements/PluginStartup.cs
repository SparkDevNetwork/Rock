using System;
using System.Linq;

using DotLiquid;

using Rock.Model;
using Rock.Utility;

namespace Geode.RockUtils.GroupRequirements
{
    public class PluginStartup : IRockStartup
    {
        /// <summary>
        /// The startup order.
        /// For this plugin, we don't care what order we're started in.
        /// </summary>
        public int StartupOrder => 0;

        /// <summary>
        /// Runs when Rock starts up.
        /// </summary>
        public void OnStartup()
        {
            // Register safe types for lava.
            // We use these in our templates but Rock doesn't include them by default.
            RegisterSafeType( typeof( PersonGroupRequirementStatus ) );
            RegisterSafeType( typeof( GroupRequirementStatus ) );
        }

        /// <summary>
        /// Registers a type with the Lava templating system.
        /// </summary>
        /// <param name="type"></param>
        private void RegisterSafeType( Type type )
        {
            Template.RegisterSafeType( type, type.GetProperties().Select( p => p.Name ).ToArray() );
        }
    }
}
