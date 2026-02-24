using System.Collections.Generic;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal partial class CommunicationSkill
    {
        #region Tool(s)

        /// <summary>
        /// Looks up system phone numbers, optionally filtering to only SMS-enabled numbers.
        /// </summary>
        /// <param name="smsEnabled"></param>
        /// <returns></returns>
        [AgentToolGuid( "FD3F160F-ABCA-4A18-B69F-0E21D61B6874" )]
        public RockToolResult LookupSystemPhoneNumbers( bool? smsEnabled = null )
        {
            var spnResults = GetSystemPhoneNumbers( smsEnabled );

            // Trim down for history
            var trimmedSpns = spnResults.Select( spn => new KeyNameResult
            {
                Id = spn.Id,
                Name = spn.Name
            } );

            var historyKey = smsEnabled.HasValue ? $"system-phone-numbers-sms-{smsEnabled.Value}" : "system-phone-numbers-all";

            return Success( spnResults )
                .WithHistoryContent( trimmedSpns, historyKey );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the system phone numbers, optionally filtering to only SMS-enabled numbers.
        /// </summary>
        /// <param name="smsEnabled"></param>
        /// <returns></returns>
        private List<SystemPhoneNumberResult> GetSystemPhoneNumbers( bool? smsEnabled = null )
        {
            return SystemPhoneNumberCache.All( AgentRequestContext.RockContext )
                .Where( spn => spn.IsActive )
                .Where( spn => !smsEnabled.HasValue || spn.IsSmsEnabled == smsEnabled.Value )
                .Where( spn => spn.IsAuthorized( Authorization.VIEW, AgentRequestContext.RockRequestContext.CurrentPerson ) )
                .Select( spn =>
                {
                    var spnResult = new SystemPhoneNumberResult
                    {
                        Id = spn.Id,
                        Name = spn.Name,
                        Description = spn.Description,
                        Number = spn.Number,
                        IsSmsEnabled = spn.IsSmsEnabled,
                    };

                    if ( spn.AssignedToPersonAliasId.HasValue )
                    {
                        var person = new PersonAliasService( AgentRequestContext.RockContext ).GetPerson( spn.AssignedToPersonAliasId.Value );

                        spnResult.AssignedToPerson = PersonResult.NameOnly( person );
                    }

                    return spnResult;
                } )
                .ToList();
        }

        #endregion
    }
}
