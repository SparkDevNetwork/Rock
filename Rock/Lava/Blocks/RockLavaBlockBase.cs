using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;

namespace Rock.Lava.Blocks
{
    public class RockLavaBlockBase : DotLiquid.Block
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
    }
}
