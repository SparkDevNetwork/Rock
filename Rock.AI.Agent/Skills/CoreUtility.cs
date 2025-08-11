using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.AI.Agent.Utilities;
using Rock.Cms.ContentCollection.Search;
using Rock.SystemGuid;
using Rock.Web.Cache;

using static Rock.AI.Agent.Skills.PersonSkill;
using static Rock.Web.Cache.CampusCache;

namespace Rock.AI.Agent.Skills
{
    [Description(
        "🎯 Purpose:\r\n" +
        "Provides common, non-domain-specific helper functions that can be used across multiple skills.\r\n" +
        "These include utilities for working with dates, times, and simple data conversions."
    )]
    [AgentSkillGuid( "3406D2DC-6718-45A2-99D3-1DAA32BF2EFD" )]
    [EntityTypeGuid( "35CD02D0-1FF7-4256-B495-FBBFBC9A2C9C" )]
    internal sealed class CoreUtility : AgentSkillComponent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreUtility"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public CoreUtility( ILogger<CoreUtility> logger )
        {
        }

        #endregion

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

        public LookupFunctionResult<CampusLookupResult> LookupCampuses()
        {
            // TODOs:  
            // 1. We need to add CampusSchedules to the CampusCache so that we can use it here.  

            var campusResults = CampusCache.All()
                .Select( c => new CampusLookupResult
                {
                    CampusKey = c.IdKey,
                    IsActive = c.IsActive ?? false,
                    Abbreviation = c.ShortCode,
                    CampusType = c.CampusTypeValue?.Value ?? string.Empty,
                    CampusTypeKey = c.CampusTypeValue.IdKey,
                    CampusStatus = c.CampusStatusValue?.Value ?? string.Empty,
                    CampusStatusKey = c.CampusStatusValue.IdKey,
                    Location = c.Location,
                    ServiceTimes = c.ServiceTimes
                } );

            return LookupFunctionResult<CampusLookupResult>.Success( campusResults );
        }

        // BC: We have not proven to need this function yet.
        // If we do, we can uncomment it (or delete it if we never need it).
        //[KernelFunction( "GetCurrentDateTime" )]
        //[Description( "🎯 Purpose:\r\n1. Determines the current date. Usage Guidance:\r\n1. Use this function any time you need to know the current date." )]
        //[AgentFunctionGuid( "E83F9B5A-53B0-4FE5-8DA3-0898BDA767D2" )]
        //public DateTime GetCurrentDateTime()
        //{
        //    return DateTime.Now;
        //}

        #endregion

        #region POCOs
        public class CampusLookupResult
        {
            public string CampusKey { get; set; }
            public bool IsActive { get; set; }
            public string Abbreviation { get; set; }
            public string CampusType { get; set; }
            public string CampusTypeKey { get; set; }
            public string CampusStatus { get; set; }
            public string CampusStatusKey { get; set; }
            public CampusLocation Location { get; set; }
            public List<ServiceTime> ServiceTimes { get; set; }
            public Dictionary<string,string> Attributes { get; set; } = new Dictionary<string, string>();
        }

        #endregion
    }
}
