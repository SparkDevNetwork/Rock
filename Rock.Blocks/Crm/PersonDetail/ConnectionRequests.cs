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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDetail.ConnectionRequests;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Displays the connection requests of a particular person.
    /// </summary>

    [DisplayName( "Connection Requests" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to view connection requests of a particular person." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [EnumsField(
        "Hide Connection Requests With These States",
        Key = AttributeKey.HideRequestStates,
        Description = "Any of the states you select here will be excluded from the list.",
        EnumSourceType = typeof( ConnectionState ),
        IsRequired = false,
        Order = 0 )]

    [LinkedPage(
        "Connection Request Detail",
        Key = AttributeKey.ConnectionRequestDetail,
        Description = "The Connection Request Detail page.",
        Order = 1 )]

    [BooleanField(
        "Use Connection Request Detail Page From Connection Type",
        Key = AttributeKey.UseConnectionRequestDetailPageFromConnectionType,
        Description = "If enabled, the Connection Request Detail page defined by the Connection Type will be used to view the request (if it's not empty/unset). Otherwise the Connection Request Detail page configured on this block will be used.",
        DefaultBooleanValue = true,
        Order = 2 )]

    #endregion

    [ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "AD99370A-9F28-40FF-8DAF-F71ACA194447" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "7953AFDB-25B5-4283-A089-1C6933999FCB" )]
    [Rock.SystemGuid.BlockTypeGuid( "39C53B93-C75A-45DE-B9E7-DFA4EE6B7027" )]
    public class ConnectionRequests : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string HideRequestStates = "HideRequestStates";
            public const string ConnectionRequestDetail = "ConnectionRequestDetail";
            public const string UseConnectionRequestDetailPageFromConnectionType = "UseConnectionRequestDetailPageFromConnectionType";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
            public const string Request = "Request";
            public const string ConnectionOpportunity = "ConnectionOpportunity";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<ConnectionRequestsBag, ConnectionRequestsOptionsBag>();

            var person = GetPerson();

            box.Bag = new ConnectionRequestsBag
            {
                IsVisible = person != null,
                ConnectionRequests = person != null
                    ? GetConnectionRequests( person )
                    : new List<ConnectionRequestItemBag>()
            };

            return box;
        }

        /// <summary>
        /// Gets the person either from the context or the page parameter.
        /// </summary>
        /// <returns>The resolved person or <c>null</c>.</returns>
        private Person GetPerson()
        {
            var person = RequestContext.GetContextEntity<Person>();

            if ( person != null )
            {
                return person;
            }

            var personKey = PageParameter( PageParameterKey.PersonId );

            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                return new PersonService( RockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
            }

            return null;
        }

        /// <summary>
        /// Gets the connection request rows to display for the person, grouped
        /// by connection type and filtered by the block settings and the
        /// current person's authorization.
        /// </summary>
        /// <param name="person">The person whose connection requests are displayed.</param>
        /// <returns>The ordered list of connection request rows.</returns>
        private List<ConnectionRequestItemBag> GetConnectionRequests( Person person )
        {
            var hiddenStates = GetAttributeValue( AttributeKey.HideRequestStates )
                .SplitDelimitedValues()
                .Select( v => v.ConvertToEnumOrNull<ConnectionState>() )
                .Where( s => s.HasValue )
                .Select( s => s.Value )
                .ToList();

            var qry = new ConnectionRequestService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( r => r.ConnectionOpportunity )
                .Include( r => r.ConnectionStatus )
                .Where( r => r.PersonAlias.PersonId == person.Id );

            if ( hiddenStates.Any() )
            {
                qry = qry.Where( r => !hiddenStates.Contains( r.ConnectionState ) );
            }

            // Hiding the Inactive state also hides requests whose opportunity is
            // no longer active, since those requests can no longer move forward.
            if ( hiddenStates.Contains( ConnectionState.Inactive ) )
            {
                qry = qry.Where( r => r.ConnectionOpportunity.IsActive );
            }

            var currentPerson = RequestContext.CurrentPerson;

            return qry
                .ToList()
                .Select( r => new
                {
                    Request = r,
                    ConnectionType = ConnectionTypeCache.Get( r.ConnectionOpportunity.ConnectionTypeId )
                } )
                .Where( a => a.ConnectionType != null
                    && a.ConnectionType.IsAuthorized( Authorization.VIEW, currentPerson )
                    && a.Request.ConnectionOpportunity.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .OrderBy( a => a.ConnectionType.Order )
                .ThenBy( a => a.ConnectionType.Name )
                .ThenBy( a => a.Request.ConnectionOpportunity.Name )
                .Select( a => new ConnectionRequestItemBag
                {
                    ConnectionTypeName = a.ConnectionType.Name,
                    Name = GetRequestName( a.Request ),
                    StatusText = a.Request.ConnectionState == ConnectionState.Connected
                        ? "Connected"
                        : a.Request.ConnectionStatus?.Name,
                    DetailUrl = GetDetailUrl( a.Request, a.ConnectionType )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the display name for a connection request, which is the
        /// opportunity name followed by the campus name in parentheses when the
        /// request has a campus.
        /// </summary>
        /// <param name="request">The connection request.</param>
        /// <returns>The display name for the request.</returns>
        private string GetRequestName( ConnectionRequest request )
        {
            var opportunityName = request.ConnectionOpportunity.Name;
            var campusName = request.CampusId.HasValue
                ? CampusCache.Get( request.CampusId.Value )?.Name
                : null;

            return campusName.IsNotNullOrWhiteSpace()
                ? $"{opportunityName} ({campusName})"
                : opportunityName;
        }

        /// <summary>
        /// Gets the URL of the detail page for a connection request. The page
        /// defined by the connection type is preferred when the block is
        /// configured to use it, otherwise the block's own linked page is used.
        /// </summary>
        /// <param name="request">The connection request.</param>
        /// <param name="connectionType">The cached connection type of the request.</param>
        /// <returns>The detail page URL, or <c>null</c> when no detail page is configured.</returns>
        private string GetDetailUrl( ConnectionRequest request, ConnectionTypeCache connectionType )
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.Request] = request.IdKey,
                [PageParameterKey.ConnectionOpportunity] = request.ConnectionOpportunity.IdKey
            };

            var useConnectionTypePage = GetAttributeValue( AttributeKey.UseConnectionRequestDetailPageFromConnectionType ).AsBoolean();

            if ( useConnectionTypePage
                && ( connectionType.ConnectionRequestDetailPageId.HasValue || connectionType.ConnectionRequestDetailPageRouteId.HasValue ) )
            {
                var pageReference = new PageReference(
                    connectionType.ConnectionRequestDetailPageId ?? 0,
                    connectionType.ConnectionRequestDetailPageRouteId ?? 0,
                    queryParams );

                return pageReference.PageId > 0
                    ? pageReference.BuildUrl()
                    : null;
            }

            var url = this.GetLinkedPageUrl( AttributeKey.ConnectionRequestDetail, queryParams );

            return url.IsNotNullOrWhiteSpace() ? url : null;
        }

        #endregion Methods
    }
}
