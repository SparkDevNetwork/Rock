using System.Collections.Generic;
using System.Linq;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Data;
using Rock.Model;
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
            using var rockContext = _rockContextFactory.CreateRockContext();

            var spnResults = GetSystemPhoneNumbers( rockContext, smsEnabled );

            // Trim down for history
            var trimmedSpns = spnResults.Select( spn => new KeyNameResult
            {
                Id = spn.Id,
                Name = spn.Name
            } );

            var historyKey = smsEnabled.HasValue ? $"system-phone-numbers-sms-{smsEnabled.Value}" : "system-phone-numbers-all";

            return RockToolResult.Success( spnResults )
                .WithHistoryContent( trimmedSpns, historyKey );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the system phone numbers, optionally filtering to only SMS-enabled numbers.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="smsEnabled"></param>
        /// <returns></returns>
        private List<SystemPhoneNumberResult> GetSystemPhoneNumbers( RockContext rockContext, bool? smsEnabled = null )
        {
            var spns = SystemPhoneNumberCache.All()
                .Where( spn => spn.IsActive )
                .Where( spn => !smsEnabled.HasValue || spn.IsSmsEnabled == smsEnabled.Value );

            // Filter out based on security.
            spns = spns.Where( spn => spn.IsAuthorized( Authorization.VIEW, AgentRequestContext.RockRequestContext.CurrentPerson ) ).ToList();

            var spnResults = new List<SystemPhoneNumberResult>();
            foreach ( var spn in spns )
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
                    var person = new PersonAliasService( rockContext ).GetPerson( spn.AssignedToPersonAliasId.Value );

                    if ( person != null )
                    {
                        spnResult.AssignedToPerson = new PersonResult
                        {
                            FirstName = person.FirstName,
                            LastName = person.LastName,
                            Id = person.Id,
                        };
                    }
                }

                spnResults.Add( spnResult );
            }

            return spnResults;
        }

        #endregion
    }
}
