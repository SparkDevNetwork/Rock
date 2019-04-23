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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using CSScriptLibrary;

using DotLiquid;

namespace Rock.Lava.Blocks
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="DotLiquid.Block" />
    public class Execute : RockLavaBlockBase
    {
        private RuntimeType _runtimeType = RuntimeType.SCRIPT;
        private List<string> _imports = new List<string>();

        string _markup = string.Empty;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterTag<Execute>( "execute" );
        }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            var parms = ParseMarkup( markup );

            if ( parms.Any( p => p.Key == "type" ) )
            {
                if (parms["type"].ToLower() == "class" )
                {
                    _runtimeType = RuntimeType.CLASS;
                }
                else
                {
                    _runtimeType = RuntimeType.SCRIPT;
                }
            }

            if ( parms.Any( p => p.Key == "import" ) )
            {
                _imports = parms["import"].Split( ',' ).ToList();
            }

            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            // first ensure that entity commands are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( RockLavaBlockBase.NotAuthorizedMessage, this.Name ) );
                base.Render( context, result );
                return;
            }

            string userScript = @"return ""Watson, can you hear me?"";"; // initial script here was just for testing

            using ( TextWriter temp = new StringWriter() )
            {
                base.Render( context, temp );

                userScript = temp.ToString();

                if ( _runtimeType == RuntimeType.SCRIPT )
                {
                    // add default convenience import
                    _imports.Insert( 0, "Rock.Data" );
                    _imports.Insert( 0, "Rock.Model" );
                    _imports.Insert( 0, "Rock" );
                    _imports.Insert( 0, "System" );

                    // treat this as a script
                    string imports = string.Empty;

                    // create needed imports
                    foreach ( string import in _imports )
                    {
                        string importStatement = string.Format( "using {0};", CleanInput( import ).Trim() );

                        // ensure the import statement isn't already used
                        if ( !imports.Contains( importStatement ) )
                        {
                            imports = imports + Environment.NewLine + importStatement;
                        }
                    }


                    // build class for the script
                    string script = string.Format( @"{0}
                                     public class Script
                                     {{
                                         public string Execute()
                                         {{
                                             {1}
                                         }}
                                     }}", imports, userScript );

                    dynamic csScript = CSScript.Evaluator.LoadCode<ILavaScript>( script );

                    string scriptResult = csScript.Execute();
                    result.Write( scriptResult );
                }
                else
                {
                    // treat this like a class

                    // remove any reference to 'using System;' as this will cause an issue
                    var cleanScript = Regex.Replace( userScript, @"\s*using\s*System;", "" );

                    dynamic csScript = CSScript.Evaluator.LoadCode<ILavaScript>( cleanScript );
                    string scriptResult = csScript.Execute();
                    result.Write( scriptResult );
                }


            }
        }

        /// <summary>
        /// Cleans the input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private string CleanInput( string input )
        {
            return input.Replace( "\"", "" ).Replace( @"\", "" );
        }

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <returns></returns>
        private Dictionary<string, string> ParseMarkup( string markup )
        {
            var parms = new Dictionary<string, string>();

            var markupItems = Regex.Matches( markup, @"(\S*?:'[^']+')" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var item in markupItems )
            {
                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    parms.AddOrReplace( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                }
            }
            return parms;
        }
    }

    /// <summary>
    /// Enum for the type of runtime
    /// </summary>
    enum RuntimeType
    {
        SCRIPT, CLASS
    }


    /// <summary>
    /// Generic interface for the lava classes to ensure we call Execute and not care about the class name they use
    /// </summary>
    public interface ILavaScript
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns></returns>
        string Execute();
    }
}
