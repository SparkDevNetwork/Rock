using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.AI.Agent.Utilities;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    [Description( "Used for a variety of standard functions, such as retrieving the current date or converting simple data types." )]
    [AgentSkillGuid( "7620AA36-A2FF-4BE7-8E51-14CB45C34392" )]
    [EntityTypeGuid( "BBFFFB6E-3568-4D42-B9A6-D6BF521E4C06" )]
    internal class UtilitySkill : AgentSkillComponent
    {

        public UtilitySkill( ILogger<UtilitySkill> logger )
        {
        }

        #region Native Functions

        [KernelFunction( "DetermineDateRange" )]
        [Description( "🎯 Purpose:\r\n1. Determines a date range from a natural language string.\r\n\r\n\U0001f9ed Usage Guidance:\r\n1. This function is useful in cases where you need to determine a start date and end date for another\r\n   function, such as when you want to filter results by a specific date range." )]
        [AgentFunctionGuid( "87756092-9D52-448E-82EE-556A780DF7CF" )]
        public DateRangeResult DetermineDateRange(
            [Description( "A natural language string, such as 'last week', 'tomorrow', or 'March 1st to March 10th'.")]
            string query )
        {

            return DateTimeRecognitionHelper.RecognizeDateRange( query, DateTime.Now );
        }

        [KernelFunction( "GetCurrentDateTime" )]
        [Description( "🎯 Purpose:\r\n1. Determines the current date. Usage Guidance:\r\n1. Use this function any time you need to know the current date." )]
        [AgentFunctionGuid( "E83F9B5A-53B0-4FE5-8DA3-0898BDA767D2" )]
        public DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }

        #endregion
    }
}
