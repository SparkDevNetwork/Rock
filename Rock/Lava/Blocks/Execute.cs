using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;
using Rock.Data;
using Rock.Model;
using CSScriptLibrary;

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

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            string[] parms = markup.Split( ' ' );

            foreach ( var parm in parms )
            {
                string[] setting = parm.Split( '=' );

                if ( setting.Length == 2 )
                {
                    switch ( setting[0] )
                    {
                        case "type":
                            {
                                string value = CleanInput( setting[1] ).ToUpper();
                                _runtimeType = value.ConvertToEnum<RuntimeType>( RuntimeType.SCRIPT );
                                break;
                            }
                        case "import":
                            {
                                _imports = setting[1].Split( ',' ).ToList();
                                break;
                            }
                    }
                }
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
                result.Write( string.Format( "The Lava command '{0}' is not configured for this template.", this.Name ) );
                base.Render( context, result );
                return;
            }

            string userScript = @"return ""Watson, can you hear me?"";"; // inital script here was just for testing

            using ( TextWriter temp = new StringWriter() )
            {
                base.Render( context, temp );

                userScript = temp.ToString();

                if ( _runtimeType == RuntimeType.SCRIPT )
                {
                    // add default convience import
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
                    dynamic csScript = CSScript.Evaluator.LoadCode<ILavaScript>( userScript );
                    string scriptResult = csScript.Execute();
                    result.Write( scriptResult );
                }


            }
        }

        private string CleanInput( string input )
        {
            return input.Replace( "\"", "" ).Replace( @"\", "" );
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
