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
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Enums.Controls;
using Rock.Enums.Security;
using Rock.Field.Types;
using Rock.Model;
using Rock.Observability;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.LoginHistory;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Grid;
using Rock.Web.UI;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// A block for viewing login activity for all or a single person.
    /// </summary>

    [DisplayName( "Login History" )]
    [Category( "Security" )]
    [Description( "A block for viewing login activity for all or a single person." )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Person ) )]

    #region Block Attributes

    [BooleanField( "Enable Person Context",
        Key = AttributeKey.EnablePersonContext,
        Description = @"If enabled and the page has a person context, its value will be used to limit the grid results to only this person, and the ""Person"" column will be hidden.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        IsRequired = false )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "63507646-F14D-4F2C-A5C4-FA28B3DEB8F0" )]
    [Rock.SystemGuid.BlockTypeGuid( "6C02377F-DD74-4B2C-9BAD-1A010A12A714" )]
    public class LoginHistory : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EnablePersonContext = "EnablePersonContext";
        }

        private static class PreferenceKey
        {
            public const string FilterSlidingDateRange = "filter-sliding-date-range";
        }

        private static class SqlParamKey
        {
            public const string DateTimeStart = "@DateTimeStart";
            public const string DateTimeEnd = "@DateTimeEnd";
            public const string PersonId = "@PersonId";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the sliding date range by which to filter the results.
        /// </summary>
        private SlidingDateRangeBag FilterSendDateRange => this.GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterSlidingDateRange )
            .ToSlidingDateRangeBagOrNull();

        /// <summary>
        /// Gets the context person, if this block should show login history for only a single person.
        /// </summary>
        private Person ContextPerson => this.RequestContext.GetContextEntity<Person>();

        /// <summary>
        /// Gets whether person context is enabled for this block instance.
        /// </summary>
        private bool IsPersonContextEnabled =>
            GetAttributeValue( AttributeKey.EnablePersonContext ).AsBoolean()
            && this.ContextPerson != null;

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<LoginHistoryOptionsBag>();
            var builder = GetGridBuilder();

            box.ExpectedRowCount = 50;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        #endregion RockBlockType Implementation

        #region Actions

        /// <summary>
        /// Gets the login history grid data.
        /// </summary>
        /// <returns>A bag containing the login history grid data.</returns>
        [BlockAction]
        public BlockActionResult GetGridData()
        {
            // Default to the last 30 days if a null/invalid range was selected.
            var defaultSlidingDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Day,
                TimeValue = 30
            };

            var dateRange = FilterSendDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var dateTimeStart = dateRange.Start;
            var dateTimeEnd = dateRange.End;

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter( SqlParamKey.DateTimeStart, dateTimeStart.Value ),
                new SqlParameter( SqlParamKey.DateTimeEnd, dateTimeEnd.Value )
            };

            // Build a dynamic SQL query to project only the needed data into a custom POCO.
            var sqlSb = new StringBuilder( $@"
SELECT
    hl.[Guid] AS [HistoryLoginGuid]

    -- Date
    , hl.[LoginAttemptDateTime]

    -- Person:
    , pa.[PersonId] AS [PersonId]
    , p.[NickName]
    , p.[LastName]
    , p.[ConnectionStatusValueId]
    , p.[PhotoId]
    , p.[BirthDay]
    , p.[BirthMonth]
    , p.[BirthYear]
    , p.[Gender]
    , p.[RecordTypeValueId]
    , p.[AgeClassification]

    -- Provider
    , et.[FriendlyName] AS [EntityTypeFriendlyName]
    , JSON_VALUE(hl.[RelatedDataJson], '$.LoginContext') AS [LoginContext]

    -- Username
    , hl.[UserName]

    -- Source
    , hl.[ExternalSource]
    , s.[Name] AS [SourceSiteName]

    -- Client IP
    , hl.[ClientIpAddress]

    -- Status
    , hl.[LoginFailureReason]

    -- Tooltip:
    , JSON_VALUE(hl.[RelatedDataJson], '$.ImpersonatedByPersonFullName') AS [ImpersonatedBy]
    , hl.[LoginFailureMessage]

FROM [HistoryLogin] hl
LEFT OUTER JOIN [PersonAlias] pa ON pa.[Id] = hl.[PersonAliasId]
LEFT OUTER JOIN [Person] p ON p.[Id] = pa.[PersonId]
LEFT OUTER JOIN [UserLogin] ul ON ul.[Id] = hl.[UserLoginId]
LEFT OUTER JOIN [EntityType] et ON et.[Id] = ul.[EntityTypeId]
LEFT OUTER JOIN [Site] s ON s.[Id] = hl.[SourceSiteId]
WHERE
    hl.[LoginAttemptDateTime] >= {SqlParamKey.DateTimeStart}
    AND hl.[LoginAttemptDateTime] < {SqlParamKey.DateTimeEnd}
" );

            if ( this.IsPersonContextEnabled )
            {
                sqlSb.AppendLine( $"    AND pa.[PersonId] = {SqlParamKey.PersonId}" );
                sqlParams.Add( new SqlParameter( SqlParamKey.PersonId, ContextPerson.Id ) );
            }

            sqlSb.Append( "ORDER BY hl.[LoginAttemptDateTime] DESC;" );

            List<LoginHistoryRow> loginHistoryRows;
            using ( var activity = ObservabilityHelper.StartActivity( "Query [HistoryLogin] Records" ) )
            {
                loginHistoryRows = RockContext.Database
                    .SqlQuery<LoginHistoryRow>( sqlSb.ToString(), sqlParams.ToArray() )
                    .ToList();
            }

            var builder = GetGridBuilder();

            GridDataBag gridDataBag;
            using ( var activity = ObservabilityHelper.StartActivity( "Build Grid Data Bag" ) )
            {
                gridDataBag = builder.Build( loginHistoryRows );
            }

            return ActionOk( gridDataBag );
        }

        #endregion Actions

        #region Private Methods

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private LoginHistoryOptionsBag GetBoxOptions()
        {
            var options = new LoginHistoryOptionsBag();

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the grid builder for the login history grid.
        /// </summary>
        /// <returns>The grid builder for the login history grid.</returns>
        private GridBuilder<LoginHistoryRow> GetGridBuilder()
        {
            // A local cache to prevent repeated reflection overhead.
            var statusCache = new Dictionary<LoginFailureReason, string>();

            var gridBuilder = new GridBuilder<LoginHistoryRow>()
                .AddField( "historyLoginGuid", a => a.HistoryLoginGuid )
                .AddDateTimeField( "dateTime", a => a.LoginAttemptDateTime )
                .AddTextField( "provider", a =>
                {
                    if ( a.EntityTypeFriendlyName.IsNotNullOrWhiteSpace() )
                    {
                        return a.EntityTypeFriendlyName;
                    }

                    return a.LoginContext;
                } )
                .AddTextField( "username", a => a.UserName )
                .AddTextField( "source", a =>
                {
                    if ( a.ExternalSource.IsNotNullOrWhiteSpace() )
                    {
                        return a.ExternalSource;
                    }

                    return a.SourceSiteName;
                } )
                .AddTextField( "clientIp", a => a.ClientIpAddress )
                .AddTextField( "status", a =>
                {
                    if ( !a.LoginFailureReason.HasValue )
                    {
                        return "Success";
                    }

                    if ( statusCache.TryGetValue( a.LoginFailureReason.Value, out var status ) )
                    {
                        return status;
                    }

                    status = a.LoginFailureReason.GetDescription();
                    if ( status.IsNullOrWhiteSpace() )
                    {
                        status = a.LoginFailureReason.ConvertToString();
                    }

                    statusCache.TryAdd( a.LoginFailureReason.Value, status );

                    return status;
                } )
                .AddTextField( "tooltip", a =>
                {
                    var tooltips = new List<string>();

                    if ( a.ImpersonatedBy.IsNotNullOrWhiteSpace() )
                    {
                        tooltips.Add( $"Impersonated by {a.ImpersonatedBy}." );
                    }

                    if ( a.LoginFailureMessage.IsNotNullOrWhiteSpace() )
                    {
                        tooltips.Add( a.LoginFailureMessage );
                    }

                    return tooltips.Any()
                        ? tooltips.AsDelimited( " " )
                        : string.Empty;
                } );

            if ( !IsPersonContextEnabled )
            {
                gridBuilder.AddPersonField( "person", a =>
                {
                    if ( !a.PersonId.HasValue )
                    {
                        return null;
                    }

                    return new Person
                    {
                        Id = a.PersonId.Value,
                        NickName = a.NickName,
                        LastName = a.LastName,
                        ConnectionStatusValueId = a.ConnectionStatusValueId,
                        PhotoId = a.PhotoId,
                        BirthDay = a.BirthDay,
                        BirthMonth = a.BirthMonth,
                        BirthYear = a.BirthYear,
                        Gender = a.Gender ?? Model.Gender.Unknown,
                        RecordTypeValueId = a.RecordTypeValueId,
                        AgeClassification = a.AgeClassification ?? Model.AgeClassification.Unknown
                    };
                } );
            }

            return gridBuilder;
        }

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        /// A POCO to represent a login history SQL projection.
        /// </summary>
        private class LoginHistoryRow
        {
            /// <summary>
            /// Gets or sets the <see cref="HistoryLogin"/> unique identifier.
            /// </summary>
            public Guid HistoryLoginGuid { get; set; }

            /// <inheritdoc cref="HistoryLogin.LoginAttemptDateTime"/>
            public DateTime LoginAttemptDateTime { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Person"/> identifier.
            /// </summary>
            public int? PersonId { get; set; }

            /// <inheritdoc cref="Person.NickName"/>
            public string NickName { get; set; }

            /// <inheritdoc cref="Person.LastName"/>
            public string LastName { get; set; }

            /// <inheritdoc cref="Person.ConnectionStatusValueId"/>
            public int? ConnectionStatusValueId { get; set; }

            /// <inheritdoc cref="Person.PhotoId"/>
            public int? PhotoId { get; set; }

            /// <inheritdoc cref="Person.BirthDay"/>
            public int? BirthDay { get; set; }

            /// <inheritdoc cref="Person.BirthMonth"/>
            public int? BirthMonth { get; set; }

            /// <inheritdoc cref="Person.BirthYear"/>
            public int? BirthYear { get; set; }

            /// <inheritdoc cref="Person.Gender"/>
            public Gender? Gender { get; set; }

            /// <inheritdoc cref="Person.RecordTypeValueId"/>
            public int? RecordTypeValueId { get; set; }

            /// <inheritdoc cref="Person.AgeClassification"/>
            public AgeClassification? AgeClassification { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="EntityType.FriendlyName"/> of the login provider.
            /// </summary>
            public string EntityTypeFriendlyName { get; set; }

            /// <inheritdoc cref="HistoryLoginRelatedData.LoginContext"/>
            public string LoginContext { get; set; }

            /// <inheritdoc cref="HistoryLogin.UserName"/>
            public string UserName { get; set; }

            /// <inheritdoc cref="HistoryLogin.ExternalSource"/>
            public string ExternalSource { get; set; }

            /// <inheritdoc cref="Site.Name"/>
            public string SourceSiteName { get; set; }

            /// <inheritdoc cref="HistoryLogin.ClientIpAddress"/>
            public string ClientIpAddress { get; set; }

            /// <inheritdoc cref="HistoryLogin.LoginFailureReason"/>
            public LoginFailureReason? LoginFailureReason { get; set; }

            /// <inheritdoc cref="HistoryLoginRelatedData.ImpersonatedByPersonFullName"/>
            public string ImpersonatedBy { get; set; }

            /// <inheritdoc cref="HistoryLogin.LoginFailureMessage"/>
            public string LoginFailureMessage { get; set; }
        }

        #endregion Supporting Classes
    }
}
