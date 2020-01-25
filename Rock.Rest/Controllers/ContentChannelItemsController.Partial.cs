﻿// <copyright>
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
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ContentChannelItemsController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The content channel item identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ContentChannelItems/GetChildren/{id}" )]
        public IQueryable<ContentChannelItem> GetChildren( int id )
        {
            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

            List<ContentChannelItem> childContentChannelItems = new List<ContentChannelItem>();

            ContentChannelItemAssociationService contentChannelItemAssociationService = new ContentChannelItemAssociationService( ( Rock.Data.RockContext ) Service.Context );

            var childItems = contentChannelItemAssociationService
                        .Queryable()
                        .Where( a => a.ContentChannelItemId == id )
                        .Select( a => a.ChildContentChannelItem )
                        .AsNoTracking();

            var person = GetPerson();
            foreach ( var item in childItems )
            {
                if ( item.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    childContentChannelItems.Add( item );
                }
            }

            return childContentChannelItems.AsQueryable();
        }

        /// <summary>
        /// Gets the parents.
        /// </summary>
        /// <param name="id">The content channel item identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ContentChannelItems/GetParents/{id}" )]
        public IQueryable<ContentChannelItem> GetParents( int id )
        {
            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

            List<ContentChannelItem> parentContentChannelItems = new List<ContentChannelItem>();
            ContentChannelItemAssociationService contentChannelItemAssociationService = new ContentChannelItemAssociationService( ( Rock.Data.RockContext ) Service.Context );

            var parentItems = contentChannelItemAssociationService
                        .Queryable()
                        .Where( a => a.ChildContentChannelItemId == id )
                        .Select( a => a.ContentChannelItem )
                        .AsNoTracking();

            var person = GetPerson();
            foreach ( var item in parentItems )
            {
                if ( item.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                {
                    parentContentChannelItems.Add( item );
                }
            }

            return parentContentChannelItems.AsQueryable();
        }
    }
}
