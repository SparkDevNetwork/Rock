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
using Rock.Enums.Security;
using Rock.Field.Types;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks.Security.LoginHistory;
using Rock.Web.Cache;
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

        #endregion Keys

        #region Properties

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
            var box = new LoginHistoryInitializationBox
            {
                GridDefinition = GetGridBuilder().BuildDefinition()
            };

            return box;
        }

        #endregion RockBlockType Implementation

        #region Actions

        /// <summary>
        /// Gets the login history grid data.
        /// </summary>
        /// <param name="bag">The information needed to get grid data.</param>
        /// <returns>A bag containing the login history grid data.</returns>
        [BlockAction]
        public BlockActionResult GetGridData( LoginHistoryFiltersBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest();
            }

            // Build a dynamic SQL query to project the needed data into a custom POCO.
            var sqlSb = new StringBuilder( @"
SELECT
    hl.[Guid] AS [HistoryLoginGuid]
    , hl.[UserName]
    , ul.[EntityTypeId] AS [UserLoginEntityTypeId]
    , hl.[LoginAttemptDateTime]
    , hl.[ClientIpAddress]
    , hl.[ExternalSource]
    , s.[Name] AS [SourceSiteName]
    , JSON_VALUE(hl.[RelatedDataJson], '$.ImpersonatedByPersonFullName') AS [ImpersonatedBy]
    , JSON_VALUE(hl.[RelatedDataJson], '$.LoginContext') AS [LoginContext]
    , hl.[LoginFailureReason]
    , hl.[LoginFailureMessage]
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
FROM [HistoryLogin] hl
LEFT OUTER JOIN [UserLogin] ul ON ul.[Id] = hl.[UserLoginId]
LEFT OUTER JOIN [Site] s ON s.[Id] = hl.[SourceSiteId]
LEFT OUTER JOIN [PersonAlias] pa ON pa.[Id] = hl.[PersonAliasId]
LEFT OUTER JOIN [Person] p ON p.[Id] = pa.[PersonId]
WHERE 1 = 1
" );

            var sqlParams = new List<SqlParameter>();

            if ( bag.StartDateTime.HasValue )
            {
                sqlSb.AppendLine( "    AND hl.[LoginAttemptDateTime] >= @StartDateTime" );
                sqlParams.Add( new SqlParameter( "@StartDateTime", bag.StartDateTime.Value.LocalDateTime ) );
            }

            if ( bag.EndDateTime.HasValue )
            {
                sqlSb.AppendLine( "    AND hl.[LoginAttemptDateTime] < @EndDateTime" );
                sqlParams.Add( new SqlParameter( "@EndDateTime", bag.EndDateTime.Value.LocalDateTime ) );
            }

            if ( this.IsPersonContextEnabled )
            {
                sqlSb.AppendLine( "    AND pa.[PersonId] = @PersonId" );
                sqlParams.Add( new SqlParameter( "@PersonId", this.ContextPerson.Id ) );
            }

            sqlSb.Append( "ORDER BY hl.[LoginAttemptDateTime] DESC;" );

            // Execute the query to load the rows into memory.
            var sqlResults = RockContext.Database
                .SqlQuery<LoginHistoryRow>( sqlSb.ToString(), sqlParams.ToArray() )
                .ToList();

            // Cache the provider [name] by entity type identifiers for this request, so we only have to look them up
            // once per identifier, to be shared across all rows.
            var providerCache = new Dictionary<int, string>();

            // Attach the provider cache to each row.
            var loginHistoryRows = sqlResults
                .Select( r => r.WithProviderCache( providerCache ) )
                .ToList();

            var builder = GetGridBuilder();
            var gridDataBag = builder.Build( loginHistoryRows );

            return ActionOk( gridDataBag );
        }

        #endregion Actions

        #region Private Methods

        /// <summary>
        /// Gets the grid builder for the login history grid.
        /// </summary>
        /// <returns>The grid builder for the login history grid.</returns>
        private GridBuilder<LoginHistoryRow> GetGridBuilder()
        {
            var gridBuilder = new GridBuilder<LoginHistoryRow>()
                .AddField( "historyLoginGuid", a => a.HistoryLoginGuid )
                .AddDateTimeField( "dateTime", a => a.LoginAttemptDateTime )
                .AddTextField( "provider", a => a.Provider )
                .AddTextField( "username", a => a.UserName )
                .AddTextField( "source", a => a.Source )
                .AddTextField( "clientIp", a => a.ClientIpAddress )
                .AddTextField( "status", a => a.Status )
                .AddTextField( "tooltip", a => a.Tooltip );

            if ( !this.IsPersonContextEnabled )
            {
                gridBuilder.AddPersonField( "person", a => a.Person );
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
            #region Database Properties

            /// <summary>
            /// Gets or sets the <see cref="HistoryLogin"/> unique identifier.
            /// </summary>
            public Guid HistoryLoginGuid { get; set; }

            /// <inheritdoc cref="HistoryLogin.UserName"/>
            public string UserName { get; set; }

            /// <inheritdoc cref="UserLogin.EntityTypeId"/>
            public int? UserLoginEntityTypeId { get; set; }

            /// <inheritdoc cref="HistoryLogin.LoginAttemptDateTime"/>
            public DateTime LoginAttemptDateTime { get; set; }

            /// <inheritdoc cref="HistoryLogin.ClientIpAddress"/>
            public string ClientIpAddress { get; set; }

            /// <inheritdoc cref="HistoryLogin.ExternalSource"/>
            public string ExternalSource { get; set; }

            /// <inheritdoc cref="Site.Name"/>
            public string SourceSiteName { get; set; }

            /// <inheritdoc cref="HistoryLoginRelatedData.ImpersonatedByPersonFullName"/>
            public string ImpersonatedBy { get; set; }

            /// <inheritdoc cref="HistoryLoginRelatedData.LoginContext"/>
            public string LoginContext { get; set; }

            /// <inheritdoc cref="HistoryLogin.LoginFailureReason"/>
            public LoginFailureReason? LoginFailureReason { get; set; }

            /// <inheritdoc cref="HistoryLogin.LoginFailureMessage"/>
            public string LoginFailureMessage { get; set; }

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

            #endregion Database Properties

            #region Runtime Properties

            /// <summary>
            /// Gets or sets a local cache for provider [name] by entity type identifiers.
            /// </summary>
            public Dictionary<int, string> ProviderCache { get; private set; }

            /// <summary>
            /// Gets the provider represented by the login history.
            /// </summary>
            public string Provider
            {
                get
                {
                    string provider = null;

                    if ( this.UserLoginEntityTypeId.HasValue )
                    {
                        // Was this provider already encountered in a previous row?
                        if ( this.ProviderCache?.TryGetValue( this.UserLoginEntityTypeId.Value, out provider ) == true )
                        {
                            return provider;
                        }

                        var entityTypeCache = EntityTypeCache.Get( this.UserLoginEntityTypeId.Value );
                        if ( entityTypeCache != null )
                        {
                            var componentName = Rock.Reflection.GetDisplayName( entityTypeCache.GetEntityType() );

                            // If it has a DisplayName, use it as is; otherwise, look within the container.
                            if ( string.IsNullOrWhiteSpace( componentName ) )
                            {
                                componentName = AuthenticationContainer.GetComponentName( entityTypeCache.Name );

                                // If the component name already has a space, then trust that they are using the
                                // exact name formatting they want.
                                if ( !componentName.Contains( ' ' ) )
                                {
                                    // Otherwise split on spaces for better readability.
                                    componentName = componentName.SplitCase();
                                }
                            }

                            provider = componentName;

                            if ( provider.IsNotNullOrWhiteSpace() )
                            {
                                // If we found a value and have an entity type identifier, cache it for subsequent rows.
                                this.ProviderCache?.TryAdd( this.UserLoginEntityTypeId.Value, provider );
                            }
                        }
                    }

                    if ( provider.IsNullOrWhiteSpace() )
                    {
                        // Do we have a login context we can fall back on?
                        provider = this.LoginContext;
                    }

                    return provider;
                }
            }

            /// <summary>
            /// Gets the source represented by the login history.
            /// </summary>
            public string Source
            {
                get
                {
                    if ( this.ExternalSource.IsNotNullOrWhiteSpace() )
                    {
                        return this.ExternalSource;
                    }

                    return this.SourceSiteName;
                }
            }

            /// <summary>
            /// Gets the status of the login history.
            /// </summary>
            public string Status
            {
                get
                {
                    var status = this.LoginFailureReason.HasValue
                        ? this.LoginFailureReason.GetDescription()
                        : "Success";

                    if ( status.IsNullOrWhiteSpace() )
                    {
                        status = this.LoginFailureReason.ConvertToString();
                    }

                    return status;
                }
            }

            /// <summary>
            /// Gets or sets the tooltip to show when hovering over the row.
            /// </summary>
            public string Tooltip
            {
                get
                {
                    var tooltips = new List<string>();

                    if ( this.ImpersonatedBy.IsNotNullOrWhiteSpace() == true )
                    {
                        tooltips.Add( $"Impersonated by {this.ImpersonatedBy}." );
                    }

                    if ( this.LoginFailureMessage.IsNotNullOrWhiteSpace() )
                    {
                        tooltips.Add( this.LoginFailureMessage );
                    }

                    return tooltips.Any()
                        ? tooltips.AsDelimited( " " )
                        : string.Empty;
                }
            }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Person"/> represented by the login history.
            /// </summary>
            /// <remarks>
            /// This will NOT be a full <see cref="Rock.Model.Person"/> entity, but instead: only the minimal fields
            /// necessary to properly display this person in the Obsidian grid.
            /// </remarks>
            public Person Person
            {
                get
                {
                    if ( !this.PersonId.HasValue )
                    {
                        return null;
                    }

                    return new Person
                    {
                        Id = this.PersonId.Value,
                        NickName = this.NickName,
                        LastName = this.LastName,
                        ConnectionStatusValueId = this.ConnectionStatusValueId,
                        PhotoId = this.PhotoId,
                        BirthDay = this.BirthDay,
                        BirthMonth = this.BirthMonth,
                        BirthYear = this.BirthYear,
                        Gender = this.Gender ?? Model.Gender.Unknown,
                        RecordTypeValueId = this.RecordTypeValueId,
                        AgeClassification = this.AgeClassification ?? Model.AgeClassification.Unknown
                    };
                }
            }

            #endregion Runtime Properties

            #region Methods

            /// <summary>
            /// Sets the <paramref name="providerCache"/> on the <see cref="ProviderCache"/>.
            /// </summary>
            /// <param name="providerCache">The provider cache to set.</param>
            /// <returns>The <see cref="LoginHistoryRow"/> instance on which the provider cache was set.</returns>
            public LoginHistoryRow WithProviderCache( Dictionary<int, string> providerCache )
            {
                this.ProviderCache = providerCache;
                return this;
            }

            #endregion Methods
        }

        #endregion Supporting Classes
    }
}
