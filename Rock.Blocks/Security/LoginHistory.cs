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
using System.Linq;

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

            var historyLoginQry = new HistoryLoginService( this.RockContext ).Queryable();

            if ( bag.StartDateTime.HasValue )
            {
                historyLoginQry = historyLoginQry
                    .Where( h => h.LoginAttemptDateTime >= bag.StartDateTime.Value.LocalDateTime );
            }

            if ( bag.EndDateTime.HasValue )
            {
                historyLoginQry = historyLoginQry
                    .Where( h => h.LoginAttemptDateTime < bag.EndDateTime.Value.LocalDateTime );
            }

            if ( this.IsPersonContextEnabled )
            {
                historyLoginQry = historyLoginQry
                    .Where( h =>
                        h.PersonAlias.PersonId == ContextPerson.Id
                    );
            }

            // Cache the provider [name] by entity type identifiers for this request, so we only have to look them up
            // once per identifier, to be shared across rows.
            var providerByEntityTypeIds = new Dictionary<int, string>();

            var loginHistoryRows = historyLoginQry
                .Select( h => new
                {
                    h.Guid,
                    h.UserName,
                    UserLoginEntityTypeId = h.UserLogin != null ? h.UserLogin.EntityTypeId : null,
                    h.LoginAttemptDateTime,
                    h.ClientIpAddress,
                    h.ExternalSource,
                    SourceSiteName = h.SourceSite != null ? h.SourceSite.Name : null,
                    h.RelatedDataJson,
                    h.LoginFailureReason,
                    h.LoginFailureMessage,
                    // Cherry-pick only the person fields that are needed for the Obsidian grid.
                    PersonId = h.PersonAlias != null ? h.PersonAlias.PersonId : ( int? ) null,
                    PersonNickName = h.PersonAlias != null ? h.PersonAlias.Person.NickName : null,
                    PersonLastName = h.PersonAlias != null ? h.PersonAlias.Person.LastName : null,
                    PersonConnectionStatusValueId = h.PersonAlias != null ? h.PersonAlias.Person.ConnectionStatusValueId : null,
                    // Needed for the runtime PhotoUrl property:
                    PersonPhotoId = h.PersonAlias != null ? h.PersonAlias.Person.PhotoId : null,
                    PersonBirthDay = h.PersonAlias != null ? h.PersonAlias.Person.BirthDay : null,
                    PersonBirthMonth = h.PersonAlias != null ? h.PersonAlias.Person.BirthMonth : null,
                    PersonBirthYear = h.PersonAlias != null ? h.PersonAlias.Person.BirthYear : null,
                    PersonGender = h.PersonAlias != null ? h.PersonAlias.Person.Gender : ( Gender? ) null,
                    PersonRecordTypeValueId = h.PersonAlias != null ? h.PersonAlias.Person.RecordTypeValueId : null,
                    PersonAgeClassification = h.PersonAlias != null ? h.PersonAlias.Person.AgeClassification : ( AgeClassification? ) null
                } )
                .AsEnumerable() // Materialize the query.
                .Select( h => new LoginHistoryRow( providerByEntityTypeIds )
                {
                    HistoryLoginGuid = h.Guid,
                    UserName = h.UserName,
                    UserLoginEntityTypeId = h.UserLoginEntityTypeId,
                    Person = !h.PersonId.HasValue ? null : new Person
                    {
                        Id = h.PersonId.Value,
                        NickName = h.PersonNickName,
                        LastName = h.PersonLastName,
                        ConnectionStatusValueId = h.PersonConnectionStatusValueId,
                        PhotoId = h.PersonPhotoId,
                        BirthDay = h.PersonBirthDay,
                        BirthMonth = h.PersonBirthMonth,
                        BirthYear = h.PersonBirthYear,
                        Gender = h.PersonGender.Value,
                        RecordTypeValueId = h.PersonRecordTypeValueId,
                        AgeClassification = h.PersonAgeClassification.Value
                    },
                    LoginAttemptDateTime = h.LoginAttemptDateTime,
                    ClientIpAddress = h.ClientIpAddress,
                    ExternalSource = h.ExternalSource,
                    SourceSiteName = h.SourceSiteName,
                    RelatedDataJson = h.RelatedDataJson,
                    LoginFailureReason = h.LoginFailureReason,
                    LoginFailureMessage = h.LoginFailureMessage
                } )
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
        /// A POCO to represent a login history row, based on the <see cref="HistoryLogin"/> model.
        /// </summary>
        private class LoginHistoryRow
        {
            #region Fields

            /// <summary>
            /// A local cache for provider [name] by entity type identifiers.
            /// </summary>
            private readonly Dictionary<int, string> _providerByEntityTypeIds;

            #endregion Fields

            #region Database Properties

            /// <summary>
            /// Gets or sets the <see cref="HistoryLogin"/> unique identifier.
            /// </summary>
            public Guid HistoryLoginGuid { get; set; }

            /// <inheritdoc cref="HistoryLogin.UserName"/>
            public string UserName { get; set; }

            /// <inheritdoc cref="UserLogin.EntityTypeId"/>
            public int? UserLoginEntityTypeId { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Person"/> represented by the login history.
            /// </summary>
            /// <remarks>
            /// This will NOT be a full <see cref="Rock.Model.Person"/> entity, but instead: only the minimal fields
            /// necessary to properly display this person in the Obsidian grid.
            /// </remarks>
            public Person Person { get; set; }

            /// <inheritdoc cref="HistoryLogin.LoginAttemptDateTime"/>
            public DateTime LoginAttemptDateTime { get; set; }

            /// <inheritdoc cref="HistoryLogin.ClientIpAddress"/>
            public string ClientIpAddress { get; set; }

            /// <inheritdoc cref="HistoryLogin.ExternalSource"/>
            public string ExternalSource { get; set; }

            /// <inheritdoc cref="Site.Name"/>
            public string SourceSiteName { get; set; }

            /// <inheritdoc cref="HistoryLogin.RelatedDataJson"/>
            public string RelatedDataJson { get; set; }

            /// <inheritdoc cref="HistoryLogin.LoginFailureReason"/>
            public LoginFailureReason? LoginFailureReason { get; set; }

            /// <inheritdoc cref="HistoryLogin.LoginFailureMessage"/>
            public string LoginFailureMessage { get; set; }

            #endregion Database Properties

            #region Runtime Properties

            /// <summary>
            /// The backing field for the <see cref="RelatedData"/> property.
            /// </summary>
            private HistoryLoginRelatedData _relatedData;

            /// <summary>
            /// The related data for the login history.
            /// </summary>
            public HistoryLoginRelatedData RelatedData
            {
                get
                {
                    if ( _relatedData == null )
                    {
                        _relatedData = HistoryLogin.GetRelatedDataOrNull( this.RelatedDataJson );
                    }

                    return _relatedData;
                }
            }

            /// <summary>
            /// The backing field for the <see cref="Provider"/> property.
            /// </summary>
            private string _provider;

            /// <summary>
            /// Gets the provider represented by the login history.
            /// </summary>
            public string Provider
            {
                get
                {
                    if ( _provider.IsNullOrWhiteSpace() )
                    {
                        if ( this.UserLoginEntityTypeId.HasValue )
                        {
                            // Was this provider already encountered in a previous row?
                            if ( _providerByEntityTypeIds.TryGetValue( this.UserLoginEntityTypeId.Value, out _provider ) )
                            {
                                return _provider;
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

                                _provider = componentName;
                            }
                        }

                        if ( _provider.IsNullOrWhiteSpace() )
                        {
                            // Do we have a login context we can fall back on?
                            _provider = this.RelatedData?.LoginContext;
                        }

                        if ( _provider.IsNotNullOrWhiteSpace() && this.UserLoginEntityTypeId.HasValue )
                        {
                            // If we found a value and have an entity type identifier, cache it for subsequent rows.
                            _providerByEntityTypeIds.TryAdd( this.UserLoginEntityTypeId.Value, _provider );
                        }
                    }

                    return _provider;
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
            /// The backing field for the <see cref="ToolTip"/> property.
            /// </summary>
            private string _tooltip;

            /// <summary>
            /// Gets or sets the tooltip to show when hovering over the row.
            /// </summary>
            public string Tooltip
            {
                get
                {
                    if ( _tooltip == null )
                    {
                        var tooltips = new List<string>();

                        if ( this.RelatedData?.ImpersonatedByPersonFullName.IsNotNullOrWhiteSpace() == true )
                        {
                            tooltips.Add( $"Impersonated by {this.RelatedData.ImpersonatedByPersonFullName}." );
                        }

                        if ( this.LoginFailureMessage.IsNotNullOrWhiteSpace() )
                        {
                            tooltips.Add( this.LoginFailureMessage );
                        }

                        _tooltip = tooltips.Any()
                            ? tooltips.AsDelimited( " " )
                            : string.Empty;
                    }

                    return _tooltip;
                }
            }

            #endregion Runtime Properties

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="LoginHistoryRow"/> class.
            /// </summary>
            /// <param name="providerByEntityTypeIds">The cached provider [name] by entity type identifiers to be shared among rows.</param>
            public LoginHistoryRow( Dictionary<int, string> providerByEntityTypeIds )
            {
                _providerByEntityTypeIds = providerByEntityTypeIds;
            }

            #endregion Constructors
        }

        #endregion Supporting Classes
    }
}
