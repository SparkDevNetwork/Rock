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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;

using DocumentFormat.OpenXml.Wordprocessing;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Allows you to wrap the provided Lava so that the contents is interpreted with a
    /// particular culture in mind.  The purpose of this is to allow Lava administrators
    /// to decide how/when a date or number should be considered as CultureInvariant
    /// or culture aware (today's existing behavior).
    ///
    /// {% setculture culture:'Invariant' %}
    ///     __ Lava to run __
    /// {% endsetculture %}
    /// </summary>
    class SetCultureBlock : LavaBlockBase
    {
        string _markup = string.Empty;
        string _blockMarkup = null;

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            // Get the internal content of the block. The list of tokens passed in to custom blocks includes the block closing tag,
            // We need to remove the unmatched closing tag to get the valid internal markup for the block.
            if ( tokens.Any() )
            {
                var lastToken = tokens.Last().Replace( " ", "" );

                if ( lastToken.StartsWith( "{%end" ) || lastToken.StartsWith( "{%-end" ) )
                {
                    _blockMarkup = tokens.Take( tokens.Count - 1 ).JoinStrings( string.Empty );
                }
                else
                {
                    // If the final tag is not an (unmatched) closing tag, include it.
                    _blockMarkup = tokens.JoinStrings( string.Empty );
                }
            }
            else
            {
                _blockMarkup = string.Empty;
            }

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            var settings = GetAttributesFromMarkup( _markup, context );
            var parms = settings.Attributes;
            
            if ( ! parms.ContainsKey( "culture" ) )
            {
                return;
            }

            var cultureString = parms["culture"].ToLower();
            var cultureInfo = CultureInfo.CurrentCulture;

            switch ( cultureString )
            {
                case "invariant":
                    {
                        cultureInfo = CultureInfo.InvariantCulture;
                        break;
                    }
                case "server":
                    {
                        // use the culture installed with the operating system.
                        // See https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.installeduiculture?view=net-9.0#system-globalization-cultureinfo-installeduiculture
                        cultureInfo = CultureInfo.InstalledUICulture;
                        break;
                    }
                case "client":
                default:
                    {
                        // just leave as CurrentCulture
                        break;
                    }
            }

            // Alternatively, if we'd rather not have such a wide reaching culture impact, we could set
            // an internal field on the Lava context to hold the culture info. Then, we would need to use
            // that inside a limited list of specific filters (such as AsInteger, AsDecimal, AsDateTime, etc.).
            // 
            //context.SetInternalField( "rock_culture", cultureInfo );

            // We could also allow setting the culture explicitly.  So, instead of "client" we could allow
            // setting this to a specific culture (e.g., "en-GB", "de-DE", etc.).  That could allow
            // someone to capture the client's culture at the time the input was collected so that it uses
            // that culture later when the Lava is processed (such as in the ProcessWorkflows job).

            // Otherwise, we're doing it like this which impacts the entire thread (any/all Lava).
            // Run and return the Lava
            ExecuteWithCulture( () =>
            {
                var lavaResults = MergeLava( _blockMarkup.ToString(), context );
                result.Write( lavaResults );
            }
            , cultureInfo );
            
        }

        internal static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            settings.AddOrIgnore( "culture", string.Empty );

            return settings;
        }

        /// <summary>
        /// Merges the lava.
        /// </summary>
        /// <param name="lavaTemplate">The lava template.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private string MergeLava( string lavaTemplate, ILavaRenderContext context )
        {
            // Resolve the Lava template contained in this block in a new context.
            var engine = context.GetService<ILavaEngine>();

            var newContext = engine.NewRenderContext();

            newContext.SetMergeFields( context.GetMergeFields() );
            newContext.SetInternalFields( context.GetInternalFields() );

            // Resolve the inner template.
            var result = engine.RenderTemplate( lavaTemplate, LavaRenderParameters.WithContext( newContext ) );

            return result.Text;
        }

        /// <summary>
        /// Executes an action within a thread that has a specific <see cref="System.Globalization.CultureInfo"/>.
        /// Captures and rethrows any exceptions that occur inside the action to preserve test behavior.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="cultureInfo">The culture info to apply (e.g., "en-US", "de-DE").</param>
        /// <exception cref="Exception">
        /// Rethrows any exception that occurs inside the thread. Stack trace is preserved if possible.
        /// </exception>
        /// <remarks>
        /// This overload is intended for void-returning methods such as assertions or operations with side effects.
        /// It ensures that the culture context does not affect other tests by running in an isolated thread.
        /// </remarks>
        public static void ExecuteWithCulture( Action action, CultureInfo cultureInfo )
        {
            Exception caughtException = null;

            var thread = new System.Threading.Thread( () =>
            {
                try
                {
                    action();
                }
                catch ( Exception ex )
                {
                    caughtException = ex;
                }
            } );

            // We MUST preserve the original CurrentCulture otherwise
            // this thread will infect the pool and other tests will fail.
            var originalCulture = thread.CurrentCulture;

            try
            {
                thread.CurrentCulture = cultureInfo;
                thread.Start();
                thread.Join();

                if ( caughtException != null )
                {
                    throw new Exception( "Exception occurred in culture-specific thread.", caughtException );
                }

                // Any exceptions thrown from the filter method are wrapped in a TargetInvocationException by the .NET framework.
                // Rethrow the actual exception thrown by the filter, where possible.
                if ( caughtException != null )
                {
                    ExceptionDispatchInfo.Capture( caughtException ).Throw();
                }
            }
            finally
            {
                thread.CurrentCulture = originalCulture;
            }
        }
    }
}

