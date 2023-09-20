using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSScriptLibrary;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace rocks.pillars.Jobs
{
    /// <summary>
    /// Job that executes CSharp code.
    /// </summary>
    [TextField( "Imports", "A comma delimited list of imports to include. Note: Do not include 'System' since it is already included.", false, "System.Linq,Rock,Rock.Model,Rock.Data", "", 0 )]
    [CodeEditorField( "CSharp Code", "CSharp code to be executed. The code must return a single string that will be used as the job status message.", CodeEditorMode.CSharp, CodeEditorTheme.Rock, 300, true, @"return ""Hello World!"";", "", 1 )]

    [DisallowConcurrentExecution]
    public class RunCSharp : IJob
    {
        private List<string> _imports = new List<string>();

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public RunCSharp()
        {
        }

        /// <summary>
        /// Job that will run CSharp code.
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string userImports = dataMap.GetString( "Imports" );
            string userScript = dataMap.GetString( "CSharpCode" );

            if ( userScript.IsNotNullOrWhiteSpace() )
            {
                string imports = string.Empty;
                _imports = userImports.Split( ',' ).ToList();

                // create imports
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

                dynamic csScript = CSScript.Evaluator.LoadCode( script );

                string scriptResult = csScript.Execute();
                context.Result = scriptResult;
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
    }
}
