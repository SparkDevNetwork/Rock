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
using System.Linq;

using Rock.Attribute;
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

            var historyLoginQry = new HistoryLoginService( this.RockContext )
                .Queryable()
                .Include( h => h.UserLogin )
                .Include( h => h.SourceSite );

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

            var loginHistory = historyLoginQry
                .ToList()
                .OrderByDescending( h => h.LoginAttemptDateTime )
                .Select( h =>
                {
                    var tooltips = new List<string>();

                    var relatedData = h.GetRelatedDataOrNull();
                    if ( relatedData?.ImpersonatedByPersonFullName.IsNotNullOrWhiteSpace() == true )
                    {
                        tooltips.Add( $"Impersonated by {relatedData.ImpersonatedByPersonFullName}." );
                    }

                    if ( h.LoginFailureMessage.IsNotNullOrWhiteSpace() )
                    {
                        tooltips.Add( h.LoginFailureMessage );
                    }

                    var tooltip = tooltips.Any()
                        ? tooltips.AsDelimited( " " )
                        : string.Empty;

                    var provider = GetLoginProviderComponentName( h );
                    if ( provider.IsNullOrWhiteSpace() )
                    {
                        // Do we have a login context we can fall back on?
                        provider = relatedData?.LoginContext;
                    }

                    var source = GetLoginSource( h );

                    var status = h.LoginFailureReason.HasValue
                        ? h.LoginFailureReason.GetDescription()
                        : "Success";

                    if ( status.IsNullOrWhiteSpace() )
                    {
                        status = h.LoginFailureReason.ConvertToString();
                    }

                    return new LoginHistoryRow
                    {
                        HistoryLoginGuid = h.Guid,
                        DateTime = h.LoginAttemptDateTime,
                        Person = h.PersonAlias?.Person,
                        Provider = provider,
                        UserName = h.UserName,
                        Source = source,
                        ClientIp = h.ClientIpAddress,
                        Status = status,
                        Tooltip = tooltip
                    };
                } );

            var builder = GetGridBuilder();
            var gridDataBag = builder.Build( loginHistory );

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
                .AddDateTimeField( "dateTime", a => a.DateTime )
                .AddTextField( "provider", a => a.Provider )
                .AddTextField( "username", a => a.UserName )
                .AddTextField( "source", a => a.Source )
                .AddTextField( "clientIp", a => a.ClientIp )
                .AddTextField( "status", a => a.Status )
                .AddTextField( "tooltip", a => a.Tooltip );

            if ( !this.IsPersonContextEnabled )
            {
                gridBuilder.AddPersonField( "person", a => a.Person );
            }

            return gridBuilder;
        }

        /// <summary>
        /// Gets the login provider component name.
        /// </summary>
        /// <param name="historyLogin">The <see cref="HistoryLogin"/> for which to get the login provider component
        /// name.</param>
        /// <returns>The login provider component name.</returns>
        private string GetLoginProviderComponentName( HistoryLogin historyLogin )
        {
            if ( historyLogin.UserLogin?.EntityTypeId == null )
            {
                return null;
            }

            var entityTypeCache = EntityTypeCache.Get( historyLogin.UserLogin.EntityTypeId.Value );
            if ( entityTypeCache == null )
            {
                return null;
            }

            var componentName = Rock.Reflection.GetDisplayName( entityTypeCache.GetEntityType() );

            // If it has a DisplayName, use it as is; otherwise, look within the container.
            if ( string.IsNullOrWhiteSpace( componentName ) )
            {
                componentName = AuthenticationContainer.GetComponentName( entityTypeCache.Name );
                // If the component name already has a space, then trust that they are using the exact name formatting
                // they want.
                if ( !componentName.Contains( ' ' ) )
                {
                    componentName = componentName.SplitCase();
                }
            }

            return componentName;
        }

        /// <summary>
        /// Gets the login source.
        /// </summary>
        /// <param name="historyLogin">The <see cref="HistoryLogin"/> for which to get the login source.</param>
        /// <returns>The login source.</returns>
        private string GetLoginSource( HistoryLogin historyLogin )
        {
            if ( historyLogin.ExternalSource.IsNotNullOrWhiteSpace() )
            {
                return historyLogin.ExternalSource;
            }

            return historyLogin.SourceSite?.Name;
        }

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        /// A POCO to represent a login history row, based on the <see cref="HistoryLogin"/> model.
        /// </summary>
        private class LoginHistoryRow
        {
            /// <summary>
            /// Gets or sets the <see cref="HistoryLogin"/> unique identifier.
            /// </summary>
            public Guid HistoryLoginGuid { get; set; }

            /// <summary>
            /// Gets or sets the login history date time.
            /// </summary>
            public DateTime DateTime { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Person"/> represented by the login history.
            /// </summary>
            public Person Person { get; set; }

            /// <summary>
            /// Gets or sets the provider represented by the login history.
            /// </summary>
            public string Provider { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="UserLogin.UserName"/> represented by the login history.
            /// </summary>
            public string UserName { get; set; }

            /// <summary>
            /// Gets or sets the source represented by the login history.
            /// </summary>
            /// <remarks>
            /// This will be the Rock <see cref="Site"/> name or the <see cref="AuthClient"/> name for remote/OIDC client apps.
            /// </remarks>
            public string Source { get; set; }

            /// <summary>
            /// Gets or sets the IP address of the device represented by the login history.
            /// </summary>
            public string ClientIp { get; set; }

            /// <summary>
            /// Gets or sets the status of the login history.
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// Gets or sets the tooltip to show when hovering over the row.
            /// </summary>
            public string Tooltip { get; set; }
        }

        #endregion Supporting Classes
    }
}
