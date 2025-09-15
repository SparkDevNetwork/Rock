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
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic.Core;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Communication;
using Rock.Core.Geography.Classes;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.SystemGuid;
using Rock.SystemKey;
using Rock.Utility;
using Rock.Web.Cache;

using Person = Rock.Model.Person;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides data lookup and analytics functions focused on site activity in Rock RMS,
    /// particularly person-centric website analytics such as page visits, grouped by site.
    /// </summary>
    [Description( "This skill provides a holistic view of a person’s profile, connections, and overall engagement." )]
    [AgentUsage( "Use the SearchPerson function to retrieve a person's IdKey when one is required as a function parameter." )]
    [AgentSkillGuid( "DD5FA7DD-3277-4C31-848D-285CD67AC7CA" )]
    [EntityTypeGuid( "12E7BDEA-B67A-48D7-8D1E-245BF8E9B555" )]
    internal sealed class PersonSkill : AgentSkillComponent
    {
        #region Fields

        private readonly IRockContextFactory _rockContextFactory = new RockContextFactory();
        private readonly ILogger<PersonSkill> _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteSkill"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public PersonSkill( IRockContextFactory rockContextFactory, ILogger<PersonSkill> logger )
        {
            _rockContextFactory = rockContextFactory ?? throw new ArgumentNullException( nameof( rockContextFactory ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Skill Tools

        [AgentToolGuid( "A1198A34-FCF2-4F58-83FA-7D02DD69830E" )]
        [AgentUsage( "Any argument ending with 'ValueIdKey' must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the chosen IdKey." )]
        [AgentUsage( "Include only fields you want to change. Omit or pass null to keep current values. For string fields, pass \"\" (empty) to clear." )]
        [AgentToolExample( "Update Ted Decker's record status to Inactive: 1) call with recordStatusValueIdKey='lookup'; 2) choose the IdKey for 'Inactive'; 3) call again with recordStatusValueIdKey='<IdKey>'" )]
        [AgentToolExample( "Clear middle name: pass middleName=\"\" and leave other fields null." )]
        [AgentToolExample( "Set suffix to Jr.: pass suffixValueIdKey='<IdKey for Jr.>' (or use 'lookup' first to find it)." )]

        public RockToolResult UpdatePerson(
            string personIdKey,

            string nickName = null,
            string firstName = null,
            string middleName = null,
            string lastName = null,
            Gender? gender = null,
            CommunicationType? communicationPreference = null,

            string email = null,
            bool? isEmailActive = null,
            string emailNote = null,

            AgeClassification? ageClassification = null,
            SetOrClear<int?> birthYear = null,
            SetOrClear<int?> birthDay = null,
            SetOrClear<int?> birthMonth = null,
            SetOrClear<DateTime?> anniversaryDate = null,

            [Description("Setting this will also update the person's record status to inactive with a reason of deceased.")]
            SetOrClear<DateTime?> deceasedDate = null,

            string campusIdKey = null,
            string connectionStatusValueIdKey = null, // Not clearable
            string recordStatusValueIdKey = null, // Not clearable

            [Description("This is not required to set a person to inactive.")]
            string inactiveReasonValueIdKey = null, // Clearable

            [Description("This is an optional note to record with the inactive reason.")]
            string inactiveReasonNote = null,
            string raceValueIdKey = null, // Clearable
            string ethnicityValueIdKey = null, // Clearable
            string recordTypeValueIdKey = null, // Not clearable
            string recordSourceValueIdKey = null, // Clearable
            string suffixValueIdKey = null, // Clearable
            string titleValueIdKey = null, // Clearable
            string preferredLanguageValueIdKey = null, // Clearable
            string maritalStatusValueIdKey = null // Clearable
        )
        {
            // Quick pre-scan: build a list of parameters explicitly requesting lookups.
            var possibleLookupRequests = new[]
            {
                (Value: connectionStatusValueIdKey,    DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid(),    ResultKey: "connectionStatusValues"),
                (Value: recordStatusValueIdKey,        DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid(),        ResultKey: "recordStatusValues"),
                (Value: inactiveReasonValueIdKey,      DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid(), ResultKey: "inactiveReasonValues"),
                (Value: ethnicityValueIdKey,           DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_ETHNICITY.AsGuid(),            ResultKey: "ethnicityValues"),
                (Value: raceValueIdKey,                DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RACE.AsGuid(),                 ResultKey: "raceValues"),
                (Value: recordTypeValueIdKey,          DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RECORD_TYPE.AsGuid(),          ResultKey: "recordTypeValues"),
                (Value: recordSourceValueIdKey,        DefinedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE.AsGuid(),          ResultKey: "recordSourceValues"),
                (Value: suffixValueIdKey,              DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid(),               ResultKey: "suffixValues"),
                (Value: titleValueIdKey,               DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid(),                ResultKey: "titleValues"),
                (Value: preferredLanguageValueIdKey,   DefinedTypeGuid: Rock.SystemGuid.DefinedType.LANGUAGES.AsGuid(),                   ResultKey: "languageValues"),
                (Value: maritalStatusValueIdKey,       DefinedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid(),       ResultKey: "maritalStatusValues"),
            }
            .Where( r => r.Value.IsNotNullOrWhiteSpace() )
            .ToList();

            var lookupResults = new Dictionary<string, List<KeyNameResult>>( StringComparer.OrdinalIgnoreCase );

            foreach ( var (value, definedTypeGuid, resultKey) in possibleLookupRequests )
            {
                // If the parameter is anything but a valid id key, we want to return the lookup.
                var isValidIdKey = IdHasher.Instance.GetId( value ).HasValue;

                if ( isValidIdKey )
                {
                    continue; // This is a valid id key, so skip it.
                }

                // Not a valid id key, so lets pre-emptively fetch the defined values.
                var definedType = DefinedTypeCache.Get( definedTypeGuid );
                if ( definedType == null )
                {
                    return RockToolResult.Error( "The system is misconfigured. Please contact your system administrator." );
                }

                var definedValues = definedType.DefinedValues
                    .OrderBy( dv => dv.Order )
                    .ThenBy( dv => dv.Value )
                    .Select( dv => new KeyNameResult { Id = dv.Id, Name = dv.Value } )
                    .ToList();

                lookupResults[resultKey] = definedValues;
            }

            // If we have any lookup results to return, do so now.
            if ( lookupResults.Count > 0 )
            {
                // Return early with the aggregated lookup data.
                return RockToolResult.Error( "Lookups Required" )
                    .WithContent( lookupResults )
                    .WithInstructions( "Use the following data to determine the proper IdKey for the tool." );
            }

            using var rockContext = _rockContextFactory.CreateRockContext();
            var personService = new PersonService( rockContext );
            var person = personService.Get( IdHasher.Instance.GetId( personIdKey ) ?? 0 );
            var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
            var instructions = "";

            if ( person == null )
            {
                return RockToolResult.Error( "No person could be found with the provided personIdKey." );
            }

            // Name properties, gender
            if ( !TryUpdateBasicPersonProperties( person, nickName, firstName, middleName, lastName, gender, out var errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Email & is active
            if ( !TryUpdatePersonEmailProperties( person, email, isEmailActive, emailNote, out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Anniversary Date
            SetOrClearValue<DateTime?>( anniversaryDate, v => person.AnniversaryDate = v, () => person.AnniversaryDate = null );

            // Birth date related values

            SetOrClearValue<int?>( birthYear, v => person.BirthYear = v, () => person.BirthYear = null );
            SetOrClearValue<int?>( birthMonth, v =>
            {
                if ( v.HasValue && ( v < 1 || v > 12 ) )
                {
                    throw new ArgumentException( "If provided, the birth month must be between 1 and 12." );
                }
                person.BirthMonth = v;
            },
            () => person.BirthMonth = null );

            SetOrClearValue<int?>( birthDay, v =>
            {
                if ( v.HasValue && ( v < 1 || v > 31 ) )
                {
                    throw new ArgumentException( "If provided, the birth day must be between 1 and 31." );
                }
                person.BirthDay = v;
            },
            () => person.BirthDay = null );

            // Age Classification
            if ( ageClassification.HasValue )
            {
                person.AgeClassification = ageClassification.Value;
            }

            // Communication Preference
            if ( communicationPreference.HasValue )
            {
                person.CommunicationPreference = communicationPreference.Value;
            }

            // Campus
            if ( campusIdKey.IsNotNullOrWhiteSpace() )
            {
                var campusId = IdHasher.Instance.GetId( campusIdKey );

                if ( !campusId.HasValue || campusId <= 0 )
                {
                    return RockToolResult.Error( "The provided campusIdKey is not valid." );
                }

                var campus = CampusCache.Get( campusId.Value );

                if ( campus == null )
                {
                    return RockToolResult.Error( "No campus could be found with the provided campusIdKey." );
                }

                person.PrimaryCampusId = campus.Id;
            }

            // Connection Status Defined Value
            if ( !TryUpdateDefinedValueProperty(
                idKey: connectionStatusValueIdKey,
                definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
                setAction: ( id ) => person.ConnectionStatusValueId = id,
                clearAction: null,
                allowClear: false,
                out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Record Status Defined Value
            var recordStatusUpdated = false;
            if ( !TryUpdateDefinedValueProperty(
                idKey: recordStatusValueIdKey,
                definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
                setAction: ( id ) =>
                {
                    person.RecordStatusValueId = id;
                    recordStatusUpdated = true;
                },
                clearAction: null,
                allowClear: false,
                out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            Guid inactivePersonRecordStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid();

            if ( recordStatusUpdated )
            {
                // If the record status was updated to inactive and there was no inactive reason provided, append instructions to follow up.
                if ( person.RecordStatusValue?.Guid == inactivePersonRecordStatusGuid && inactiveReasonValueIdKey.IsNullOrWhiteSpace() )
                {
                    instructions += "The person has been marked inactive. Follow up to see if there should be a reason specified.";
                }
            }

            // Inactive Reason Defined Value
            if ( !TryUpdateDefinedValueProperty(
                idKey: inactiveReasonValueIdKey,
                definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON,
                setAction: ( id ) => person.RecordStatusReasonValueId = id,
                clearAction: () => person.RecordStatusReasonValueId = null,
                allowClear: true,
                out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Record type
            if ( !TryUpdateDefinedValueProperty(
                idKey: recordTypeValueIdKey,
                definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RECORD_TYPE,
                setAction: ( id ) => person.RecordTypeValueId = id,
                clearAction: null,
                allowClear: false,
                out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Record Source Reason Defined Value
            if ( !TryUpdateDefinedValueProperty(
                idKey: recordSourceValueIdKey,
                definedTypeGuid: Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
                setAction: ( id ) => person.RecordSourceValueId = id,
                clearAction: () => person.RecordSourceValueId = null,
                allowClear: true,
                out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            if ( inactiveReasonNote.IsNotNullOrWhiteSpace() )
            {
                if ( person.RecordStatusValue?.Guid != inactivePersonRecordStatusGuid )
                {
                    return RockToolResult.Error( "The inactiveReasonNote can only be set if the person's record status is set to Inactive." )
                        .WithInstructions( "Ask the user if they would like you to mark the record inactive." );
                }
                person.InactiveReasonNote = inactiveReasonNote;
            }

            // Ethnicity

            if ( !TryUpdateDefinedValueProperty(
                idKey: ethnicityValueIdKey,
                definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_ETHNICITY,
                setAction: ( id ) => person.EthnicityValueId = id,
                clearAction: () => person.EthnicityValueId = null,
                allowClear: true,
                out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Race
            if ( !TryUpdateDefinedValueProperty(
                idKey: raceValueIdKey,
                definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RACE,
                setAction: ( id ) => person.RaceValueId = id,
                clearAction: () => person.RaceValueId = null,
                allowClear: true,
                out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Suffix
            if ( !TryUpdateDefinedValueProperty(
                idKey: suffixValueIdKey,
                definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_SUFFIX,
                setAction: ( id ) => person.SuffixValueId = id,
                clearAction: () => person.SuffixValueId = null,
                allowClear: true,
                out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Title
            if ( !TryUpdateDefinedValueProperty(
                idKey: titleValueIdKey,
                definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_TITLE,
                setAction: ( id ) => person.TitleValueId = id,
                clearAction: () => person.TitleValueId = null,
                allowClear: true,
                out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Preferred Language
            if ( !TryUpdateDefinedValueProperty(
                idKey: preferredLanguageValueIdKey,
                definedTypeGuid: Rock.SystemGuid.DefinedType.LANGUAGES,
                setAction: ( id ) => person.PreferredLanguageValueId = id,
                clearAction: () => person.PreferredLanguageValueId = null,
                allowClear: true,
                out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }

            // Marital Status
            if ( !TryUpdateDefinedValueProperty(
                idKey: maritalStatusValueIdKey,
                definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS,
                setAction: ( id ) => person.MaritalStatusValueId = id,
                clearAction: () => person.MaritalStatusValueId = null,
                allowClear: true,
                out errorMessage ) )
            {
                return RockToolResult.Error( errorMessage );
            }


            var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
            var deceasedReason = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED.AsGuid() );

            // Deceased Date
            SetOrClearValue<DateTime?>( deceasedDate, v => SetDeceased( person, v ), () => person.DeceasedDate = null );

            void SetDeceased( Rock.Model.Person person, DateTime? dt )
            {
                person.RecordStatusValueId = inactiveStatus.Id;
                person.RecordStatusReasonValueId = deceasedReason.Id;
                person.DeceasedDate = dt;
            }

            // Save changes
            try
            {
                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "UpdatePerson failed for PersonIdKey={PersonIdKey}", personIdKey );
                return RockToolResult.Error( "Failed to update person. " + ex.Message );
            }

            return RockToolResult.Success(
                new PersonResult
                {
                    Id = person.Id,
                    FirstName = person.FirstName,
                    NickName = person.NickName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName,
                    Suffix = person.SuffixValue?.Value,
                    Age = person.Age,
                    BirthDay = person.BirthDay,
                    BirthMonth = person.BirthMonth,
                    BirthYear = person.BirthYear,
                    AgeClassification = person.AgeClassification,
                    Email = person.Email,
                    AnniversaryDate = person.AnniversaryDate,
                    Campus = person.PrimaryCampus != null ?
                    new KeyNameResult
                    {
                        IdKey = person.PrimaryCampus.IdKey,
                        Name = person.PrimaryCampus.Name
                    } : null,
                    Gender = person.Gender,
                    RecordStatus = person.RecordStatusValue?.Value,
                    ConnectionStatus = person.ConnectionStatusValue?.Value,
                }
            );
        }

        /// <summary>
        /// Retrieves a list of personal devices associated with a specific person.
        /// </summary>
        /// <param name="personIdKey">The person.</param>
        /// <returns></returns>
        [AgentToolGuid( "29B7A989-59C4-4956-9C45-1D1297D3E673" )]
        public RockToolResult ListPersonalDevicesForPerson( string personIdKey )
        {
            var personId = IdHasher.Instance.GetId( personIdKey );

            if ( !personId.HasValue )
            {
                return RockToolResult.Error( "The personIdKey is required." )
                    .WithInstructions( "You can call SearchPerson to find the corresponding key." );
            }

            var person = new PersonService( AgentRequestContext.RockContext ).Get( personId.Value );

            if ( person == null )
            {
                return RockToolResult.Error( "No person could be found with the provided personIdKey." );
            }

            var personalDeviceService = new PersonalDeviceService( AgentRequestContext.RockContext );

            var devices = personalDeviceService.Queryable()
                .AsNoTracking()
                .Where( pd => pd.PersonAliasId == person.PrimaryAliasId );

            if ( !devices.Any() )
            {
                return RockToolResult.NoData();
            }

            var results = devices
                .AsEnumerable()
                .Select( pd => new PersonalDeviceResult
                {
                    Id = pd.Id,
                    CreatedDateTime = pd.CreatedDateTime,
                    Name = pd.Name,
                    IsBeaconMonitoringEnabled = pd.IsBeaconMonitoringEnabled,
                    LastSeenDateTime = pd.LastSeenDateTime,
                    Manufacturer = pd.Manufacturer,
                    Model = pd.Model,
                    NotificationsEnabled = pd.NotificationsEnabled,
                    LocationPermissionStatusValue = pd.LocationPermissionStatus,
                    IsPreciseLocationEnabled = pd.IsPreciseLocationEnabled,
                    LocationPermissionDisabledDateTime = pd.LocationPermissionDisabledDateTime,
                    PersonalDeviceTypeValueId = pd.PersonalDeviceTypeValueId,
                    PlatformValueId = pd.PlatformValueId
                } )
                .ToList();

            var historyContent = results.Select(
                d => new KeyNameResult
                {
                    IdKey = d.IdKey,
                    Name = d.Name
                }
            );

            return RockToolResult.Success( results )
                .WithHistoryContent( historyContent, $"{personIdKey}-devices" );
        }

        [AgentToolGuid( "39244A1E-57BF-476B-AF88-65EBC205F25D" )]
        public RockToolResult ListPeerNetworkForPerson( string personIdKey )
        {
            var rockContext = AgentRequestContext.RockContext;

            var peerNetworkService = rockContext.Set<PeerNetwork>();
            var personId = IdHasher.Instance.GetId( personIdKey );

            var results = peerNetworkService
                .Where( pn => pn.SourcePersonId == personId )
                .Join(
                    rockContext.Set<Person>(),
                    pn => pn.TargetPersonId,
                    tp => tp.Id,
                    ( pn, tp ) => new { pn, tp }
                )
                .GroupBy( x => new { x.tp.NickName, x.tp.LastName, x.tp.Id } )
                .Select( g => new
                {
                    TargetName = g.Key.NickName + " " + g.Key.LastName,
                    TargetPersonId = g.Key.Id,
                    RelationshipScore = ( int ) Math.Round( g.Sum( x => x.pn.RelationshipScore ), 0 ),
                    PointDifference = g.Sum( x => x.pn.RelationshipScore ) - g.Sum( x => x.pn.RelationshipScoreLastUpdateValue )
                } )
                .OrderByDescending( x => x.RelationshipScore )
                .ThenBy( x => x.TargetName.Split( ' ' )[1] ) // LastName
                .ThenBy( x => x.TargetName.Split( ' ' )[0] ) // NickName
                .ToList();

            if ( !results.Any() )
            {
                return RockToolResult.NoData();
            }

            return RockToolResult.Success( results );
        }

        /// <summary>
        /// Retrieves website analytics (page visits) for a specific person, optionally filtered by date and/or site.
        /// Results are grouped by site and include visited pages with visit counts.
        /// </summary>
        /// <param name="arguments">Query parameters including person id, optional site id, and optional start/end dates.</param>
        /// <remarks>
        /// Requires <see cref="LookupSites"/> to have been called previously to populate
        /// session context <c>"site-list"</c> for guardrail enforcement.
        /// Defaults the date range to [now - 1 year, now] if neither start nor end are provided.
        /// </remarks>
        [Description( "Lists page visits for a specific person." )]
        [AgentPurpose( "Retrieves page visits for a specific person, optionally filtered by date and/or site." )]
        [AgentPurpose( "Results include the site type (web, mobile, tv), visited pages and visit counts." )]
        [AgentUsage( "The results are paginated (and the 'PageNumber' parameter is required.)" )]
        [AgentUsage( "Do not call this function multiple times per site, unless necessary. It supports all-site aggregation when `siteId` is null." )]
        [AgentToolPrerequisite( "This function depends on context set by `LookupSites`. Ensure it has been called first to set the site list." )]
        [AgentToolExample( "has Ted Decker been active on any of our mobile applications in the last 2 years" )]
        [AgentToolExample( "has Alisha Marble visted the giving page in the past 30 days" )]
        [AgentToolExample( "has Pete been active on our platform?" )]
        [AgentToolGuid( "EFDBC338-CC1C-46D2-A7F6-7AE5081147AE" )]
        public RockToolResult SummarizePageVisitsForPerson( string personIdKey, DateTime? startDate = null, DateTime? endDate = null, string siteIdKey = "", int pageNumber = 1 )
        {
            var errors = new List<string>();


            var personId = IdHasher.Instance.GetId( personIdKey );
            if ( !personId.HasValue || personId <= 0 )
            {
                errors.Add( "There was an invalid key provided for the person." );
            }

            var siteId = IdHasher.Instance.GetId( siteIdKey );
            if ( siteId.HasValue && siteId.Value <= 0 )
            {
                errors.Add( "There was an invalid site key provided." );
            }

            if ( startDate.HasValue && endDate.HasValue && startDate > endDate )
            {
                errors.Add( "Invalid date range. Start date cannot be after end date." );
            }


            if ( errors.Count > 0 )
            {
                return RockToolResult.Error( errors );
            }

            // Defaults: past year → now (safety in case nothing was provided)
            if ( !startDate.HasValue && !endDate.HasValue )
            {
                endDate = RockDateTime.Now;
                startDate = endDate.Value.AddYears( -1 );
            }
            else if ( startDate.HasValue && !endDate.HasValue )
            {
                endDate = RockDateTime.Now;
            }

            // Paging
            var pgNumber = Math.Max( 1, pageNumber );
            var basePageSize = 100;
            var offset = ( pgNumber - 1 ) * basePageSize;
            var take = basePageSize + 1; // N+1 to compute hasMore

            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonId", personId),
                    GetParameterValueOrDbNull("@SiteId", siteId),
                    GetParameterValueOrDbNull("@StartDate", startDate),
                    GetParameterValueOrDbNull("@EndDate", endDate),
                    new SqlParameter("@PageSize",  take),    // request N+1
                    new SqlParameter("@OffsetRows", offset), // offset uses base size
                };

                var rows = AgentRequestContext.RockContext.Database
                    .SqlQuery<PageVisitResult>( _websiteDataSql, parameters.ToArray() )
                    .ToList();

                // Populate IdKey for each row
                rows.ForEach( r =>
                {
                    var site = SiteCache.Get( r.SiteId );
                    r.SiteIdKey = site?.IdKey;
                } );

                var hasMore = rows.Count > basePageSize;
                if ( hasMore )
                {
                    rows.RemoveAt( rows.Count - 1 ); // drop lookahead row
                }

                var meta = new Dictionary<string, object>
                {
                    { "personKey", personIdKey },
                    { "startDate", startDate },
                    { "endDate", endDate },
                    { "pageNumber", pgNumber },
                    { "pageSize", basePageSize },
                    { "returnedRows", rows.Count },
                    { "hasMore", hasMore }
                };

                if ( !rows.Any() )
                {
                    return RockToolResult.NoData()
                        .WithMetadata( meta );
                }

                return RockToolResult.Success( rows )
                    .WithMetadata( meta )
                    .WithoutHistoryContent();
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "SummarizePageVisitsForPerson failed for PersonId={PersonId}, SiteId={SiteId}", personId, siteId );
                return RockToolResult.Error( "Failed to retrieve site analytics. " + ex.Message );
            }
        }

        [Description( "Lists page visits for a specific person." )]
        [AgentPurpose( "Retrieves media views for a specific person, optionally filtered by date and/or site." )]
        [AgentUsage( "The results are paginated (and the 'PageNumber' parameter is required.)" )]
        [AgentToolGuid( "AB6CB80C-352A-F895-4233-09BA9DA69CCC" )]
        public RockToolResult ListMediaViewsForPerson( string personIdKey, int pageNumber = 1, DateTime? startDate = null, DateTime? endDate = null )
        {
            // Validate person
            var personId = IdHasher.Instance.GetId( personIdKey );
            if ( !personId.HasValue || personId <= 0 )
            {
                RockToolResult.Error( "The personIdKey is not valid. Please provide a valid value." );
            }

            // Validate date range
            if ( startDate.HasValue && endDate.HasValue && startDate > endDate )
            {
                RockToolResult.Error( "Invalid date range. Start date cannot be after end date." );
            }

            // Defaults: past year → now
            if ( !startDate.HasValue && !endDate.HasValue )
            {
                endDate = RockDateTime.Now;
                startDate = endDate.Value.AddYears( -1 );
            }
            else if ( startDate.HasValue && !endDate.HasValue )
            {
                endDate = RockDateTime.Now;
            }

            // Paging
            var basePageSize = 100;
            var offset = ( pageNumber - 1 ) * basePageSize;
            var take = basePageSize + 1; // N+1 to compute hasMore

            // Run query
            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@PersonId", personId),
                    GetParameterValueOrDbNull("@StartDate", startDate),
                    GetParameterValueOrDbNull("@EndDate", endDate),
                    new SqlParameter("@PageSize",  take),    // request N+1
                    new SqlParameter("@OffsetRows", offset), // offset uses base size
                };

                var rows = AgentRequestContext.RockContext.Database
                    .SqlQuery<MediaViewResult>( _mediaViewsDataSql, parameters.ToArray() )
                    .ToList();

                var hasMore = rows.Count > basePageSize;
                if ( hasMore )
                {
                    rows.RemoveAt( rows.Count - 1 ); // drop lookahead row
                }

                var meta = new Dictionary<string, object>
                {
                    { "personKey", personIdKey },
                    { "startDate", startDate },
                    { "endDate", endDate },
                    { "pageNumber", pageNumber },
                    { "pageSize", basePageSize },
                    { "returnedRows", rows.Count },
                    { "hasMore", hasMore }
                };

                if ( !rows.Any() )
                {
                    return RockToolResult.NoData()
                        .WithMetadata( meta );
                }

                // Do some quick clean-up of the media data
                CleanMediaViews( rows );

                return RockToolResult.Success( rows )
                    .WithMetadata( meta );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "ListMediaViewsForPerson failed for PersonId={PersonId}", personId );
                return RockToolResult.Error( "Failed to retrieve media views. " + ex.Message );
            }
        }

        /// <summary>
        /// Searches for persons by full name, returning a list of matching persons with their details.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [Description( "Does a full name sounds like search for the person." )]
        [AgentPurpose( "Searches for matching people by name. This will search by exact match as well as 'Sounds Like'." )]
        [AgentUsage( "Suffixes should be provide in the format of Sr., Jr., III, IV." )]
        [AgentUsage( "Only use this function if a full name is provided." )]
        [AgentToolReturnDescription( "A collection of summaries about the matched people. These are not full profiles. Call `GetPersonProfile` passing the personIdKey to get a person's full profile. " )]
        [AgentToolGuid( "03093B11-A02D-F794-4A5E-9AEA2C6EF63E" )]
        public RockToolResult SearchPerson( string fullName, int maxResults = 20, string campusIdKey = null )
        {
            if ( fullName == null || fullName.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "Full name is required." )
                    .WithInstructions( "The FullName parameter is required. You may also provide optional filters for CampusKey to filter by a specific campus and MaxResults to limit the results." );
            }

            // Get queryable with the metaphone and full name search.
            var searchQueryable = new PersonService( AgentRequestContext.RockContext )
                .GetSimilarPersons( fullName );

            // If the queryable is null that means that no individuals with that first name or last name were found
            if ( searchQueryable == null )
            {
                var specificErrorInstructions = string.Empty;

                // Provide some special instructions for senior and junior as these can commonly be used by voice ai.
                if ( fullName.EndsWith( "Senior" ) )
                {
                    specificErrorInstructions = "Please retry providing the suffix of Sr. instead of Senior";
                }

                if ( fullName.EndsWith( "Junior" ) )
                {
                    specificErrorInstructions = "Please retry providing the suffix of Jr. instead of Junior";
                }

                return RockToolResult.Error( "Could not find anyone with the name provided." )
                    .WithInstructions( "Could not find anyone with the name provided. You must now call SearchPersonPartial with a modified search term. Provide only the first two characters of the first name and three characters of the last name. Example: If the term was 'ted decker' pass 'te dec' to SearchPersonPartial. You must describe the results as possible matches." );
            }

            // Append campus filter if provided
            if ( campusIdKey.IsNotNullOrWhiteSpace() )
            {
                var campusId = IdHasher.Instance.GetId( campusIdKey );

                if ( !campusId.HasValue || campusId <= 0 )
                {
                    return RockToolResult.Error( "Invalid CampusIdKey provided." );
                }

                // Confirm that the campusId is valid and filter the search results.
                var campus = CampusCache.Get( campusId.Value );

                if ( campus == null )
                {
                    return RockToolResult.Error( "Invalid CampusIdKey provided." );
                }

                searchQueryable = searchQueryable
                    .Where( p => p.PrimaryCampusId == campusId.Value );
            }

            // Get search results. Returning an anonymous type as some of the values needed will
            // not be returned to the client.
            var results = searchQueryable
                .Select( p => new PersonResult
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    NickName = p.NickName,
                    LastName = p.LastName,
                    Suffix = p.SuffixValue != null ? p.SuffixValue.Value : "",
                    PrimaryFamilyId = p.PrimaryFamilyId,
                    AgeClassification = p.AgeClassification,
                    Campus = p.PrimaryCampus != null ? new KeyNameResult { Id = p.PrimaryCampus.Id, Name = p.PrimaryCampus.Name } : null,
                    PhotoId = p.PhotoId,
                    RecordTypeValueId = p.RecordTypeValueId,
                    ConnectionStatus = p.ConnectionStatusValue.Value,
                    RecordStatus = p.RecordStatusValue != null ? p.RecordStatusValue.Value : "",
                    MaritalStatusGuid = p.MaritalStatusValue != null ? p.MaritalStatusValue.Guid : Guid.Empty,
                    MaritalStatus = p.MaritalStatusValue != null ? p.MaritalStatusValue.Value : "",
                    Age = p.Age,
                    Email = p.Email,
                    Gender = p.Gender,
                    IncludePublicProfile = true
                } )
                .OrderBy( p => p.LastName )
                .ThenBy( p => p.NickName )
                .Take( maxResults + 1 )
                .ToList();

            // Provide indication of more results.
            var hasMore = results.Count > maxResults;

            if ( hasMore )
            {
                results.RemoveAt( results.Count - 1 );
            }

            results = AppendExtendedProperties( results );

            // Define meta data
            var meta = new Dictionary<string, object>
                {
                    { "returnedRows", results.Count },
                    { "hasMore", hasMore }
                };

            return RockToolResult.Success( results )
                .WithInstructions( "This data represents results that match the search query. These are both exact matches and those that are similar based on metaphone sounds like. All results should be displayed, even if they don't match exactly what was provided." )
                .WithReferenceRoute( AgentRequestContext.RockRequestContext, "Additional Search Options", $"/Person/Search/name/?SearchTerm={fullName}" )
                .WithMetadata( meta );
        }

        /// <summary>
        /// Searches for a person using a partial name.
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <param name="maxResults"></param>
        /// <param name="campusIdKey"></param>
        /// <returns></returns>
        [AgentPurpose( "Searches for matching people by a partial first name or last name." )]
        [AgentUsage( "Use this for partial name searches including searches for only last names." )]
        [AgentToolExample( "t decker would search the database for people who's first name starts with t and last name starts with decker." )]
        [AgentToolReturnDescription( "A collection of summaries about the matched people. These are not full profiles. Call `GetPersonProfile` passing the personIdKey to get a person's full profile." )]
        [Description( "Does a name search based on a partial search (e.g. 't dec')." )]
        [AgentToolGuid( "873AFC46-1872-999F-4E6C-94409654F6BC" )]
        public RockToolResult SearchPersonPartial( string searchPattern, int maxResults = 20, string campusIdKey = null )
        {
            if ( searchPattern == null || searchPattern.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "Search pattern is required." )
                    .WithInstructions( "The searchPattern parameter is required. You may also provide optional filters for CampusKey to filter by a specific campus and MaxResults to limit the results." );
            }

            var searchQueryable = new PersonService( AgentRequestContext.RockContext )
                .GetByFullNameOrdered( searchPattern, true, false, false, out _ );

            // Append campus filter if provided
            if ( campusIdKey.IsNotNullOrWhiteSpace() )
            {
                var campusId = IdHasher.Instance.GetId( campusIdKey );

                if ( !campusId.HasValue || campusId <= 0 )
                {
                    return RockToolResult.Error( "Invalid CampusIdKey provided." );
                }

                // Confirm that the campusId is valid and filter the search results.
                var campus = CampusCache.Get( campusId.Value );

                if ( campus == null )
                {
                    return RockToolResult.Error( "Invalid CampusIdKey provided." );
                }

                searchQueryable = ( IOrderedQueryable<Model.Person> ) searchQueryable
                    .Where( p => p.PrimaryCampusId == campusId.Value );
            }

            // Get results
            var results = searchQueryable
                .Select( p => new PersonResult
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    NickName = p.NickName,
                    LastName = p.LastName,
                    Suffix = p.SuffixValue != null ? p.SuffixValue.Value : "",
                    PrimaryFamilyId = p.PrimaryFamilyId,
                    AgeClassification = p.AgeClassification,
                    Campus = p.PrimaryCampus != null ? new KeyNameResult { Id = p.PrimaryCampus.Id, Name = p.PrimaryCampus.Name } : null,
                    PhotoId = p.PhotoId,
                    RecordTypeValueId = p.RecordTypeValueId,
                    ConnectionStatus = p.ConnectionStatusValue.Value,
                    RecordStatus = p.RecordStatusValue != null ? p.RecordStatusValue.Value : "",
                    MaritalStatusGuid = p.MaritalStatusValue != null ? p.MaritalStatusValue.Guid : Guid.Empty,
                    MaritalStatus = p.MaritalStatusValue != null ? p.MaritalStatusValue.Value : "",
                    Age = p.Age,
                    Email = p.Email,
                    Gender = p.Gender,
                    IncludePublicProfile = true
                } )
                .OrderBy( p => p.LastName )
                .ThenBy( p => p.NickName )
                .Take( maxResults + 1 )
                .ToList();

            // Provide indication of more results.
            var hasMore = results.Count > maxResults;

            if ( hasMore )
            {
                results.RemoveAt( results.Count - 1 );
            }

            results = AppendExtendedProperties( results );

            // Define meta data
            var meta = new Dictionary<string, object>
                {
                    { "returnedRows", results.Count },
                    { "hasMore", hasMore }
                };

            return RockToolResult.Success( results )
                .WithInstructions( "This data represents results that match the search query. All results should be displayed, even if they don't match exactly what was provided." )
                .WithReferenceRoute( RockRequestContextAccessor.Current, "Additional Search Options", $"/Person/Search/name/?SearchTerm={searchPattern}" )
                .WithMetadata( meta );
        }

        [Description( "Returns a comprehensive profile for a single person, including contact details, demographics, household, and key insights." )]
        [AgentPurpose( "Retrieves the complete profile of a person." )]
        [AgentPurpose( "Serves as the primary entry point for gaining insights into an individual." )]
        [AgentToolGuid( "2142A382-6AB2-0995-4480-69B641AE2CDC" )]
        public RockToolResult GetPersonProfile( string personIdKey )
        {
            if ( personIdKey.IsNullOrWhiteSpace() )
            {
                return RockToolResult.Error( "The personIdKey is required." );
            }

            var person = new PersonService( AgentRequestContext.RockContext ).Get( personIdKey );

            if ( person == null )
            {
                return RockToolResult.Error( "No person could be found with the provided personIdKey." );
            }

            // Get the request context
            var requestContext = AgentRequestContext.RockRequestContext;
            var currentPerson = requestContext?.CurrentPerson;

            var profileResult = new PersonResult( AgentRequestContext );

            profileResult.Id = person.Id;
            profileResult.FirstName = person.FirstName;
            profileResult.NickName = person.NickName;
            profileResult.LastName = person.LastName;
            profileResult.MiddleName = person.MiddleName;
            profileResult.Suffix = person.SuffixValue != null ? person.SuffixValue.Value : null;
            profileResult.Age = person.Age;
            profileResult.AgeClassification = person.AgeClassification;
            profileResult.Gender = person.Gender;
            profileResult.BirthMonth = person.BirthMonth;
            profileResult.BirthDay = person.BirthDay;
            profileResult.BirthYear = person.BirthYear;
            profileResult.AnniversaryDate = person.AnniversaryDate;
            profileResult.GraduationYear = person.GraduationYear;
            profileResult.MaritalStatus = person.MaritalStatusValue != null ? person.MaritalStatusValue.Value : null;
            profileResult.PhotoId = person.PhotoId;
            profileResult.PrimaryFamilyId = person.PrimaryFamilyId;
            profileResult.PrimaryFamilyId = person.PrimaryFamilyId;
            profileResult.MaritalStatusGuid = person.MaritalStatusValue != null ? person.MaritalStatusValue.Guid : Guid.Empty;
            profileResult.RecordTypeValueId = person.RecordTypeValueId;
            profileResult.RecordStatus = person.RecordStatusValue != null ? person.RecordStatusValue.Value : null;
            profileResult.ConnectionStatus = person.ConnectionStatusValue != null ? person.ConnectionStatusValue.Value : null;
            profileResult.Email = person.Email;
            profileResult.MaritalStatus = person.MaritalStatusValue != null ? person.MaritalStatusValue.Value : null;
            profileResult.Campus = person.PrimaryCampus != null ? new KeyNameResult { Id = person.PrimaryCampus.Id, Name = person.PrimaryCampus.Name } : null;
            profileResult.IncludePublicProfile = true;

            profileResult.PreviousLastNames = person.GetPreviousNames()
                .Select( p => p.LastName )
                .ToList();

            var family = person.GetFamily();

            // Add phone numbers
            profileResult.PhoneNumbers = person.PhoneNumbers
                .Select( n => new PhoneNumberResult
                {
                    IsUnlisted = n.IsUnlisted,
                    PhoneNumber = n.NumberFormatted,
                    PhoneType = new KeyNameResult
                    {
                        Id = n.NumberTypeValueId ?? 0,
                        Name = n.NumberTypeValue != null ? n.NumberTypeValue.Value : string.Empty
                    },
                    IsMessagingEnabled = n.IsMessagingEnabled
                } ).ToList();

            // Add addresses
            profileResult.Addresses = family
                .GroupLocations.Select( l => new LocationResult
                {
                    Street1 = l.Location.Street1,
                    Street2 = l.Location.Street2,
                    City = l.Location.City,
                    State = l.Location.State,
                    PostalCode = l.Location.PostalCode,
                    Country = l.Location.Country,
                    LocationType = l.GroupLocationTypeValue != null ? l.GroupLocationTypeValue.Value : string.Empty,
                    IsMailingAddress = l.IsMailingLocation,
                    IsMappedLocation = l.IsMappedLocation,
                    GeographyPoint = ( l.Location.Latitude.HasValue && l.Location.Longitude.HasValue ) ? new GeographyPoint( l.Location.Latitude.Value, l.Location.Longitude.Value ) : null
                } ).ToList();

            // Add adults
            profileResult.AdultsInFamily = family.Members.Where( m => m.Person.AgeClassification == AgeClassification.Adult )
                .Select( m => new PersonResult
                {
                    Id = m.Person.Id,
                    FirstName = m.Person.FirstName,
                    NickName = m.Person.NickName,
                    PhotoId = m.Person.PhotoId,
                    LastName = m.Person.LastName,
                    Age = m.Person.Age,
                    Suffix = m.Person.SuffixValue != null ? m.Person.SuffixValue.Value : null
                } ).ToList();

            // Add children
            profileResult.ChildrenInFamily = family.Members.Where( m => m.Person.AgeClassification == AgeClassification.Child )
                .Select( m => new PersonResult
                {
                    Id = m.Person.Id,
                    FirstName = m.Person.FirstName,
                    NickName = m.Person.NickName,
                    PhotoId = m.Person.PhotoId,
                    LastName = m.Person.LastName,
                    Age = m.Person.Age,
                    Suffix = m.Person.SuffixValue != null ? m.Person.SuffixValue.Value : null
                } ).ToList();

            // Add spouse
            var spouse = person.GetSpouse();

            if ( spouse != null )
            {
                profileResult.Spouse = new PersonResult
                {
                    Id = spouse.Id,
                    FirstName = spouse.FirstName,
                    NickName = spouse.NickName,
                    PhotoId = spouse.PhotoId,
                    Email = spouse.Email,
                    LastName = spouse.LastName,
                    Age = spouse.Age,
                    Suffix = spouse.SuffixValue != null ? spouse.SuffixValue.Value : null
                };
            }

            // Add Attributes
            person.LoadAttributes();
            profileResult.Attributes = person.AttributeValues
                        .Where( v => v.Value != null && v.Value.Value != null & v.Value.Value != string.Empty )
                        .Where( v => person.Attributes[v.Key].IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                        .Select( a => new AttributeResult
                        {
                            Id = a.Value.AttributeId,
                            Key = a.Key,
                            Value = a.Value.PersistedTextValue,
                            Category = a.Value.AttributeCategoryIds.Select( cId => CategoryCache.Get( cId ) ).Where( c => c != null ).Select( c => c.Name ).FirstOrDefault()
                        } )
                        .ToList();

            // Add Known Relationships
            var groupMemberService = new GroupMemberService( AgentRequestContext.RockContext );
            var knownRelationshipOwnerRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();

            // Get the known relationship group for the person (if any)
            var knownRelationshipGroup = groupMemberService.Queryable()
                            .Where( m =>
                                m.PersonId == person.Id &&
                                m.GroupRole.Guid == knownRelationshipOwnerRoleGuid )
                            .Select( m => m.Group )
                            .FirstOrDefault();

            if ( knownRelationshipGroup != null )
            {
                // Get the members of the known relationship group
                profileResult.KnownRelationships = knownRelationshipGroup.Members
                    .Where( m => m.PersonId != person.Id )
                    .Select( m => new GroupMemberResult
                    {
                        Role = new KeyNameResult { Id = m.GroupRoleId, Name = m.GroupRole.Name },
                        Person = new PersonResult { Id = person.Id, FirstName = m.Person.FirstName, LastName = m.Person.LastName, NickName = m.Person.NickName, Suffix = m.Person.SuffixValue != null ? m.Person.SuffixValue.Value : null }
                    } )
                    .ToList();
            }

            // Add latest notes
            var currentPersonId = currentPerson != null ? currentPerson.Id : 0;
            var personEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>().Value;

            profileResult.Notes = new NoteService( AgentRequestContext.RockContext ).Queryable()
                .Where( n =>
                    n.NoteType.EntityTypeId == personEntityTypeId
                    && n.EntityId == person.Id
                    && ( n.IsPrivateNote == false || n.CreatedByPersonAlias.PersonId == currentPersonId ) )
                .OrderByDescending( n => n.CreatedDateTime )
                .Take( 20 )
                .ToList()
                .Where( x => x.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Select( x => new NoteResult
                {
                    Id = x.Id,
                    Caption = x.Caption,
                    Text = x.Text,
                    Author = x.CreatedByPersonAlias != null ? new PersonResult { Id = x.CreatedByPersonAlias.Person.Id, NickName = x.CreatedByPersonAlias.Person.NickName, LastName = x.CreatedByPersonAlias.Person.LastName } : null,
                    CreatedDateTime = x.CreatedDateTime
                } )
                .Take( 5 )
                .ToList();

            // Add latest prayer requests
            profileResult.PrayerRequests = new PrayerRequestService( AgentRequestContext.RockContext ).Queryable()
                .Where( pr =>
                    pr.RequestedByPersonAlias != null &&
                    pr.RequestedByPersonAlias.PersonId == person.Id )
                .Select( pr => new PrayerRequestResult
                {
                    Id = pr.Id,
                    Text = pr.Text,
                    IsApproved = pr.IsApproved,
                    IsUrgent = pr.IsUrgent,
                    IsPublic = pr.IsPublic,
                    Category = new KeyNameResult { Id = pr.Category.Id, Name = pr.Category.Name },
                    PrayerCount = pr.PrayerCount,
                    EnteredDateTime = pr.EnteredDateTime
                } )
                .OrderByDescending( pr => pr.EnteredDateTime )
                .Take( 5 )
                .ToList();

            // Run security on profile result
            var securityCheckPassed = profileResult.SanitizeForSecurity( currentPerson );

            if ( !securityCheckPassed )
            {
                return RockToolResult.Error( "You do not have permission to view this person's profile." );
            }

            return RockToolResult.Success( profileResult )
                .WithReferenceRoute( requestContext, "View Profile", $"/person/{profileResult.IdKey}", false );
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Wrapper indicating an intent to either set a value or clear an existing value.
        /// </summary>
        /// <typeparam name="T">The value type. Usually nullable for clearing semantics.</typeparam>
        /// <remarks>
        /// Null instance ⇒ no change. <see cref="ClearValue"/> takes precedence over <see cref="Value"/>.
        /// </remarks>
        public class SetOrClear<T>
        {
            /// <summary>
            /// The value to set when <see cref="ClearValue"/> is false. Ignored when clearing.
            /// </summary>
            public T Value { get; set; }

            /// <summary>
            /// When true, clear the target field regardless of <see cref="Value"/>.
            /// </summary>
            public bool ClearValue { get; set; }
        }

        /// <summary>
        /// Trims and applies basic person name fields and optional gender, preventing illegal clears.
        /// </summary>
        /// <param name="person">The person to mutate.</param>
        /// <param name="nickName">New nickname; trimmed. Null = no change.</param>
        /// <param name="firstName">New first name; trimmed. Empty string is rejected.</param>
        /// <param name="middleName">New middle name; trimmed. Empty clears middle name.</param>
        /// <param name="lastName">New last name; trimmed. Empty string is rejected.</param>
        /// <param name="gender">Optional gender to set.</param>
        /// <param name="errorMessage">Populated when returning false.</param>
        /// <returns>
        /// True if updated or nothing to change; false if validation failed (with <paramref name="errorMessage"/>).
        /// </returns>
        private bool TryUpdateBasicPersonProperties(
            Rock.Model.Person person,
            string nickName,
            string firstName,
            string middleName,
            string lastName,
            Rock.Model.Gender? gender,
            out string errorMessage )
        {
            errorMessage = null;

            // If nothing needs to be updated, we are done.
            if ( nickName == null && firstName == null && middleName == null && lastName == null )
            {
                return true;
            }

            if ( nickName != null )
            {
                nickName = nickName.Trim();
                person.NickName = nickName;
            }

            if ( firstName != null )
            {
                firstName = firstName.Trim();

                if ( firstName == string.Empty )
                {
                    errorMessage = "Clearing out a person's first name is not allowed.";
                    return false;
                }

                person.FirstName = firstName;
            }

            if ( middleName != null )
            {
                middleName = middleName.Trim();
                person.MiddleName = middleName;
            }

            if ( lastName != null )
            {
                lastName = lastName.Trim();
                if ( lastName == "" )
                {
                    errorMessage = "Clearing out a person's last name is not allowed.";
                    return false;
                }

                person.LastName = lastName;
            }

            if ( gender.HasValue )
            {
                person.Gender = gender.Value;
            }

            return true;
        }

        /// <summary>
        /// Applies email and active state with API-consistent rules.
        /// </summary>
        /// <param name="person">The person to mutate.</param>
        /// <param name="email">
        /// New email; trimmed. Null = no change; empty string clears email and sets <c>IsEmailActive=false</c>.
        /// </param>
        /// <param name="isEmailActive">
        /// Optional explicit active flag; overrides implicit defaults when supplied.
        /// </param>
        /// <param name="errorMessage">Populated when returning false.</param>
        /// <returns>
        /// True if updated or nothing to change; false if validation failed (with <paramref name="errorMessage"/>).
        /// </returns>
        /// <remarks>
        /// When setting a non-empty email and <paramref name="isEmailActive"/> is not supplied, the address is marked active.
        /// </remarks>
        private bool TryUpdatePersonEmailProperties(
            Rock.Model.Person person,
            string email,
            bool? isEmailActive,
            string emailNote,
            out string errorMessage )
        {
            errorMessage = null;

            if ( email == null && !isEmailActive.HasValue && emailNote == null )
            {
                return true;
            }

            if ( email != null )
            {
                email = email.Trim();

                // If clearing out the email, also mark it inactive.
                if ( email == string.Empty )
                {
                    person.Email = string.Empty;
                    person.IsEmailActive = false;
                    return true;
                }

                if ( !EmailAddressFieldValidator.IsValid( email ) )
                {
                    errorMessage = "The provided email address is not valid.";
                    return false;
                }

                person.Email = email;

                // Unless explicitly specified, if setting an email address, also mark it active.
                if ( !isEmailActive.HasValue )
                {
                    person.IsEmailActive = true;
                }
            }

            if ( emailNote != null )
            {
                person.EmailNote = emailNote.Trim();
            }

            if ( isEmailActive.HasValue )
            {
                person.IsEmailActive = isEmailActive.Value;
            }

            return true;
        }

        /// <summary>
        /// Applies a <see cref="SetOrClear{T}"/> wrapper to a target using delegates.
        /// </summary>
        /// <typeparam name="T">The wrapped value type.</typeparam>
        /// <param name="source">Null = no change. ClearValue=true clears; otherwise Value (when not null) is applied.</param>
        /// <param name="setAction">Invoked to set the value.</param>
        /// <param name="clearAction">Invoked to clear the value.</param>
        private void SetOrClearValue<T>( SetOrClear<T> source, Action<T> setAction, Action clearAction )
        {
            if ( source == null )
            {
                return;
            }
            if ( source.ClearValue )
            {
                clearAction();
            }
            else if ( source.Value != null )
            {
                setAction( source.Value );
            }
        }

        /// <summary>
        /// Resolves and applies a DefinedValue by IdKey, or clears it (if allowed).
        /// </summary>
        /// <param name="idKey">
        /// Null = no change; empty string = clear (when <paramref name="allowClear"/> is true); otherwise must be a valid IdKey.
        /// </param>
        /// <param name="definedTypeGuid">The defined type GUID constant (e.g., <c>Rock.SystemGuid.DefinedType.PERSON_RACE</c>).</param>
        /// <param name="setAction">Called with the resolved DefinedValue Id.</param>
        /// <param name="clearAction">Called when clearing (only if <paramref name="allowClear"/> is true and <paramref name="idKey"/> is empty).</param>
        /// <param name="allowClear">Whether clearing via empty string is permitted.</param>
        /// <param name="errorMessage">Populated when returning false.</param>
        /// <returns>
        /// True if applied/no-op; false if the IdKey is invalid or a prohibited clear was requested.
        /// </returns>
        private bool TryUpdateDefinedValueProperty(
            string idKey,
            string definedTypeGuid,
            Action<int> setAction,
            Action clearAction,
            bool allowClear,
            out string errorMessage )
        {
            errorMessage = null;

            // no matter what null means no change.
            if ( idKey == null )
            {
                return true;
            }

            // clear the value if requested and allowed.
            if ( idKey == string.Empty )
            {
                if ( !allowClear )
                {
                    errorMessage = "Clearing out this value is not allowed";
                    return false;
                }

                clearAction();
                return true;
            }

            if ( !TryGetDefinedValueOfType( idKey, definedTypeGuid, out var dvc, out errorMessage ) )
            {
                return false;
            }

            setAction( dvc.Id );
            return true;
        }

        private static bool TryGetDefinedValueOfType( string definedValueIdKey, string definedTypeGuid, out DefinedValueCache dvc, out string errorMessage )
        {
            dvc = null;
            errorMessage = string.Empty;

            if ( definedValueIdKey.IsNullOrWhiteSpace() )
            {
                errorMessage = "The definedValueIdKey is required.";
                return false;
            }

            var id = IdHasher.Instance.GetId( definedValueIdKey );
            if ( !id.HasValue || id <= 0 )
            {
                errorMessage = "The definedValueIdKey is not valid.";
                return false;
            }

            var definedValue = DefinedValueCache.Get( id.Value );
            if ( definedValue == null )
            {
                errorMessage = "No defined value could be found with the provided definedValueIdKey.";
                return false;
            }

            if ( !definedValue.DefinedType.Guid.Equals( definedTypeGuid.AsGuid() ) )
            {
                errorMessage = "The provided definedValueIdKey is not of the expected type.";
                return false;
            }

            dvc = definedValue;

            return true;
        }

        /// <summary>
        /// Cleans up the media views by determining the medium and adjusting the viewing location URL.
        /// </summary>
        /// <param name="mediaViews"></param>
        private void CleanMediaViews( List<MediaViewResult> mediaViews )
        {
            foreach ( var media in mediaViews )
            {
                if ( media.ViewingLocationUrl.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                if ( media.ViewingLocationUrl.StartsWith( "http", StringComparison.OrdinalIgnoreCase ) )
                {
                    // If the URL starts with http, we can assume it's a full URL.
                    media.Medium = "Web";
                }
                else
                {
                    // Otherwise, we assume it's mobile (e71a7c63-f510-434b-945b-f30c1c18df9d?CategoryGuid=24ae4f53-1bb8-4637-ae0e-5db0b06856b3).
                    var urlParts = media.ViewingLocationUrl.Split( '?' );

                    if ( urlParts.Length != 2 )
                    {
                        continue;
                    }

                    var page = PageCache.Get( urlParts[0].AsGuid() );

                    if ( page == null )
                    {
                        continue;
                    }

                    media.ViewingLocationUrl = $"{page.Site} - {page.PageTitle}";
                    media.Medium = "Mobile";
                }
            }
        }

        /// <summary>
        /// Appends family members (children, adults, and spouse) to the search results.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private List<PersonResult> AppendExtendedProperties( List<PersonResult> results )
        {
            // Get configuration for the family roles and marital status
            var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
            var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();

            var marriedMaritalStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();

            var isBibleStrictSpouse = Rock.Web.SystemSettings.GetValue( SystemSetting.BIBLE_STRICT_SPOUSE ).AsBoolean( true );

            // Get families members for the individuals in the search results
            var familyIds = results.Select( p => p.PrimaryFamilyId ).Distinct().ToList();

            var familyMembers = new GroupMemberService( AgentRequestContext.RockContext ).Queryable()
                .Where( m => familyIds.Contains( m.GroupId ) && m.GroupMemberStatus == GroupMemberStatus.Active )
                .Select( m => new
                {
                    NickName = m.Person.NickName,
                    LastName = m.Person.LastName,
                    GroupRoleGuid = m.GroupRole.Guid,
                    PersonId = m.Person.Id,
                    FamilyId = m.GroupId,
                    Gender = m.Person.Gender,
                    AgeClassification = m.Person.AgeClassification,
                    Age = m.Person.Age,
                    MaritalStatusGuid = m.Person.MaritalStatusValue != null ? m.Person.MaritalStatusValue.Guid : Guid.Empty,
                    Suffix = m.Person.SuffixValue != null ? m.Person.SuffixValue.Value : string.Empty
                } )
                .ToList();

            // Append family members to the search results records
            foreach ( var result in results )
            {
                result.ChildrenInFamily = familyMembers.Where( m => m.FamilyId == result.PrimaryFamilyId
                                                && m.GroupRoleGuid == childGuid
                                                && m.PersonId != result.Id )
                                            .Select( m => new PersonResult { NickName = m.NickName, LastName = m.LastName, Id = m.PersonId, Suffix = m.Suffix, Age = m.Age, IncludePublicProfile = true } )
                                            .ToList();

                result.AdultsInFamily = familyMembers.Where( m => m.FamilyId == result.PrimaryFamilyId
                                                && m.GroupRoleGuid == adultGuid
                                                && m.PersonId != result.Id )
                                            .Select( m => new PersonResult { NickName = m.NickName, LastName = m.LastName, Id = m.PersonId, Suffix = m.Suffix, Age = m.Age, IncludePublicProfile = true } )
                                            .ToList();

                var personRoleInFamily = familyMembers.Where( m => m.FamilyId == result.PrimaryFamilyId && m.PersonId == result.Id )
                                            .Select( m => m.GroupRoleGuid )
                                            .FirstOrDefault();

                // Add spouse. This logic is copies from PersonService.GetSpouse()
                if ( personRoleInFamily == adultGuid && result.MaritalStatusGuid == marriedMaritalStatusGuid )
                {
                    result.Spouse = familyMembers.Where( m => m.FamilyId == result.PrimaryFamilyId
                                                && m.GroupRoleGuid == adultGuid
                                                && m.PersonId != result.Id
                                                && m.MaritalStatusGuid == marriedMaritalStatusGuid
                                                && ( !isBibleStrictSpouse || m.Gender != result.Gender || m.Gender == Gender.Unknown || result.Gender == Gender.Unknown ) )
                                             .Select( m => new PersonResult { NickName = m.NickName, LastName = m.LastName, Id = m.PersonId, Suffix = m.Suffix, Age = m.Age, IncludePublicProfile = true } )
                                             .FirstOrDefault();
                }
            }

            return results;
        }

        /// <summary>
        /// Creates a SQL parameter with the specified key and value, substituting <see cref="DBNull.Value"/> when the value is <c>null</c>.
        /// </summary>
        /// <param name="key">The parameter name (e.g., <c>@SiteId</c>).</param>
        /// <param name="value">The parameter value, or <c>null</c> to emit <see cref="DBNull.Value"/>.</param>
        /// <returns>A <see cref="SqlParameter"/> instance.</returns>
        private static SqlParameter GetParameterValueOrDbNull( string key, object value )
            => new SqlParameter( key, value ?? ( object ) DBNull.Value );

        #endregion

        #region SQL

        private const string _websiteDataSql = @"
;WITH Filtered AS (
    SELECT
        PageName = COALESCE(NULLIF(LTRIM(RTRIM(i.InteractionSummary)), ''), '(Unknown)'),
        i.InteractionDateTime,
        ich.ChannelEntityId AS SiteId
    FROM dbo.Interaction i
    INNER JOIN dbo.InteractionSession   AS inse  ON inse.Id  = i.InteractionSessionId
    INNER JOIN dbo.InteractionComponent AS icomp ON icomp.Id = i.InteractionComponentId
    INNER JOIN dbo.InteractionChannel   AS ich   ON ich.Id   = icomp.InteractionChannelId
    INNER JOIN dbo.PersonAlias          AS pa    ON pa.Id    = i.PersonAliasId
    WHERE (@SiteId IS NULL OR ich.ChannelEntityId = @SiteId)
      AND i.Operation = 'View'
      AND pa.PersonId = @PersonId
      AND (@StartDate IS NULL OR i.InteractionDateTime >= @StartDate)
      AND (@EndDate   IS NULL OR i.InteractionDateTime <= @EndDate)
),
Grouped AS (
    SELECT
        f.SiteId,
        f.PageName,
        VisitCount = COUNT(*),
        LastVisit  = MAX(f.InteractionDateTime)
    FROM Filtered f
    GROUP BY f.SiteId, f.PageName
)
SELECT
    g.SiteId,
    g.PageName,
    g.VisitCount,
    g.LastVisit
FROM Grouped g
ORDER BY
    g.LastVisit DESC,
    g.SiteId,
    g.PageName
OFFSET @OffsetRows ROWS
FETCH NEXT @PageSize ROWS ONLY
OPTION (RECOMPILE);";

        private const string _mediaViewsDataSql = @"
            SELECT 
                me.[Id] AS [MediaElementId]
                , i.[InteractionDateTime] AS [ViewDateTime]
                , i.[ChannelCustomIndexed1] AS [Medium]
                , CAST( ROUND( i.[InteractionLength], 0 ) AS int ) AS [PercentWatched]
                , me.[DurationSeconds] AS [MediaLengthInSeconds]
                , CAST( ROUND(me.[DurationSeconds] * i.[InteractionLength] / 100, 0) AS int) AS [DurationWatchedInSeconds]
                , me.[Name] AS [MediaElementName]
                , mf.[Name] AS [MediaFolderName]
                , ma.[Name] AS [MediaAccountName]
                , i.[InteractionSummary] AS [ViewingLocationUrl]
            FROM [Interaction] i
                INNER JOIN [InteractionComponent] ic ON ic.[Id] = i.[InteractionComponentId]
                INNER JOIN [InteractionChannel] ich ON ich.[Id] = ic.[InteractionChannelId]
                INNER JOIN [PersonAlias] pa ON pa.[Id] = i.[PersonAliasId]
                INNER JOIN [Person] p ON p.[Id] = pa.[PersonId]
                INNER JOIN [MediaElement] me ON me.[Id] = ic.[EntityId]
                INNER JOIN [MediaFolder] mf ON mf.[Id] = me.[MediaFolderId]
                INNER JOIN [MediaAccount] ma ON ma.[Id] = mf.[MediaAccountId]
            WHERE   
                ich.[Guid] = 'd5b9bdaf-6e52-40d5-8e74-4e23973df159'
                AND p.[Id] = @PersonId
                AND i.[InteractionDateTime] >= @StartDate
                AND i.[InteractionDateTime] <= @EndDate
            ORDER BY 
                i.[InteractionDateTime] DESC
                , i.[Id] DESC
            OFFSET @OffsetRows ROWS
            FETCH NEXT @PageSize ROWS ONLY
        ";

        #endregion
    }
}