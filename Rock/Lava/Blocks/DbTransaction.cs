// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System.Collections.Generic;
using System.IO;

using Rock.Lava.Blocks.Internal;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Renders the inner content inside a database transaction. Other blocks,
    /// tags and filters must specifically support being used inside a
    /// transaction in order to take advantage of the commit/rollback behavior.
    /// </summary>
    public class DbTransaction : LavaBlockBase
    {
        #region Fields

        /// <summary>
        /// The markup for the block describing the properties being passed
        /// to the block logic.
        /// </summary>
        private string _blockPropertiesMarkup = null;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            base.OnInitialize( tagName, markup, tokens );

            _blockPropertiesMarkup = markup;
        }

        /// <inheritdoc/>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            var settings = GetAttributesFromMarkup( _blockPropertiesMarkup, context );

            // Create a new Rock Context and set it into the Lava context
            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );
            var transactionResult = new DbTransactionResult();

            context.SetInternalField( "rock_dbtransaction", transactionResult );

            // Create transaction (ReadCommitted is the default Isolation Level.)
            using ( var dbContextTransaction = rockContext.Database.BeginTransaction() )
            {
                // Render the inner content of the block.
                string output;

                using ( TextWriter innerContentWriter = new StringWriter() )
                {
                    base.OnRender( context, innerContentWriter );
                    output = innerContentWriter.ToString();
                }

                context.SetMergeField( "TransactionResult", transactionResult );

                if ( transactionResult.Success == false || settings["forcerollback"].AsBoolean() )
                {
                    dbContextTransaction.Rollback();
                }
                else
                {
                    dbContextTransaction.Commit();
                    result.Write( output );
                }
            }
        }

        /// <summary>
        /// Gets a list of parameter attributes from the block command.
        /// </summary>
        /// <param name="markup">The markup that describes the command parameters.</param>
        /// <param name="context">The rendering context used to parse the parameters.</param>
        /// <returns>An instance of <see cref="LavaElementAttributes"/> that contains the parameters.</returns>
        private static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            // Create default settings
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            settings.AddOrIgnore( "forcerollback", "false" );

            return settings;
        }

        #endregion
    }
}
