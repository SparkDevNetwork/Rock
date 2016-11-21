using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;

using Rock.Utility;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="DotLiquid.Block" />
    public class RockLavaBlockBase : DotLiquid.Block, IRockStartup
    {
        /// <summary>
        /// Determines whether the specified command is authorized.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected bool IsAuthorized( Context context )
        {
            if ( context.Scopes != null )
            {
                foreach ( var scopeHash in context.Scopes )
                {
                    if ( scopeHash.ContainsKey( "EnabledCommands" ) )
                    {
                        var enabledCommands = scopeHash["EnabledCommands"].ToString().Split( ',' ).ToList();

                        if ( enabledCommands.Contains( "All" ) || enabledCommands.Contains( this.GetType().Name ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            base.Render( context, result );
        }

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
        }

    }
}
