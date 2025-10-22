using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Utilities;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class SystemUtilitySkill
    {
        #region Tool(s)

        /// <summary>
        /// Determines a date range from a natural language string.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        [Description( "Determines a date range from a natural language string." )]
        [AgentToolPreamble( "Determining date range for query." )]
        [AgentPurpose( "Determines a date range from a natural language string." )]
        [AgentUsage( "This function is useful in cases where you need to determine a start date and end date for another function, such as when you want to filter results by a specific date range." )]
        [AgentToolGuid( "87756092-9D52-448E-82EE-556A780DF7CF" )]
        public RockToolResult DetermineDateRange(
            [Description( "A natural language string, such as 'last week', 'tomorrow', or 'March 1st to March 10th'.")]
            string query )
        {

            var dateRange = DateTimeRecognitionHelper.RecognizeDateRange( query, DateTime.Now );

            if ( dateRange == null )
            {
                return RockToolResult.Error( "A date range could not be determined from the query." )
                    .WithInstructions( $"Today is {DateTime.Now}. Using today as a reference date, infer the date range yourself." );
            }

            return RockToolResult.Success( dateRange );
        }

        #endregion
    }
}
