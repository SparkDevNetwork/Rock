using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal partial class SystemUtilitySkill
    {
        #region Tool

        /// <summary>
        /// Determines a date range from a natural language string.
        /// </summary>
        [Description( "Gets minimal information about the person currently logged in and interacting with the agent." )]
        [AgentPurpose( "Gets minimal information about the user/person currently logged in." )]
        [AgentToolGuid( "cb9f23f1-3d21-4451-80c3-4efbd18a7fbc" )]
        public IAgentToolResult GetCurrentPerson()
        {
            var currentPerson = AgentRequestContext.CurrentPerson;

            if ( currentPerson == null )
            {
                return Error( "There is no person currently logged in." );
            }

            var result = PersonResult.Basic( currentPerson );
            var historyResult = PersonResult.NameOnly( currentPerson );

            result.BirthDay = currentPerson.BirthDay;
            result.BirthMonth = currentPerson.BirthMonth;
            result.BirthYear = currentPerson.BirthYear;
            result.Email = currentPerson.Email.IfEmpty( null );

            return Success( result )
                .WithHistoryContent( historyResult );
        }

        /// <summary>
        /// Gets the phone number result from a phone number. This handles
        /// translating the NumberTypeValueId into a KeyNameResult.
        /// </summary>
        /// <param name="phoneNumber">The phone number to translate.</param>
        /// <returns>An instance of <see cref="PhoneNumberResult"/> that represents <paramref name="phoneNumber"/>.</returns>
        private PhoneNumberResult GetPhoneNumberResult( PhoneNumber phoneNumber )
        {
            var result = new PhoneNumberResult
            {
                Id = phoneNumber.Id,
                PhoneNumber = phoneNumber.NumberFormatted,
                IsMessagingEnabled = phoneNumber.IsMessagingEnabled,
                IsUnlisted = phoneNumber.IsUnlisted,
            };

            if ( phoneNumber.NumberTypeValueId.HasValue )
            {
                var definedValue = DefinedValueCache.Get( phoneNumber.NumberTypeValueId.Value, AgentRequestContext.RockContext );

                if ( definedValue != null )
                {
                    result.PhoneType = new KeyNameResult
                    {
                        Id = definedValue.Id,
                        Name = definedValue.Value,
                    };
                }
            }

            return result;
        }

        #endregion
    }
}
