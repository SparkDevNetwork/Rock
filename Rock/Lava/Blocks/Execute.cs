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
    public class Execute : DotLiquid.Block
    {
        private RuntimeType _runtimeType = RuntimeType.SCRIPT;
        private List<string> _imports = new List<string>();

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

        public override void Render( Context context, TextWriter result )
        {
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
        string Execute();
    }
}
