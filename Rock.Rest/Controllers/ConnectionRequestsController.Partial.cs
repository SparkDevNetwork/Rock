// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ConnectionRequestsController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootCategoryId">The root category identifier.</param>
        /// <param name="getCategorizedItems">if set to <c>true</c> [get categorized items].</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityQualifier">The entity qualifier.</param>
        /// <param name="entityQualifierValue">The entity qualifier value.</param>
        /// <param name="showUnnamedEntityItems">if set to <c>true</c> [show unnamed entity items].</param>
        /// <param name="showCategoriesThatHaveNoChildren">if set to <c>true</c> [show categories that have no children].</param>
        /// <param name="includedCategoryIds">The included category ids.</param>
        /// <param name="excludedCategoryIds">The excluded category ids.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/ConnectionRequests/GetChildren/{id}" )]
        public IQueryable<TreeViewItem> GetChildren( string id )
        {
            // Enable proxy creation since child collections need to be navigated
            SetProxyCreation( true );

            var rockContext = (RockContext)Service.Context;
            var list = new List<TreeViewItem>();

            if ( id.StartsWith( "T" ) )
            {
                int connectionTypeId = id.Substring( 1 ).AsInteger();
                foreach ( var opportunity in new ConnectionOpportunityService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( o => o.ConnectionTypeId == connectionTypeId )
                    .OrderBy( o => o.Name ) )
                {
                    var item = new TreeViewItem();
                    item.Id = string.Format( "O{0}", opportunity.Id );
                    item.Name = opportunity.Name;
                    item.HasChildren = opportunity.ConnectionRequests
                        .Any( r => 
                            r.ConnectionState == ConnectionState.Active ||
                            r.ConnectionState == ConnectionState.FutureFollowUp );
                    item.IconCssClass = opportunity.IconCssClass;
                    list.Add( item );
                }
            }

            else if ( id.StartsWith( "O" ) )
            {
                int opportunityId = id.Substring( 1 ).AsInteger();
                foreach ( var request in Service
                    .Queryable().AsNoTracking()
                    .Where( r => 
                        r.ConnectionOpportunityId == opportunityId &&
                        r.PersonAlias != null &&
                        r.PersonAlias.Person != null )
                    .OrderBy( r => r.PersonAlias.Person.LastName )
                    .ThenBy( r => r.PersonAlias.Person.NickName ) )
                {
                    var item = new TreeViewItem();
                    item.Id = request.Id.ToString();
                    item.Name = request.PersonAlias.Person.FullName;
                    item.HasChildren = false;
                    item.IconCssClass = "fa fa-user";
                    list.Add( item );
                }
            }

            else
            {
                int? requestId = id.AsIntegerOrNull();
                if ( !requestId.HasValue || requestId.Value == 0 )
                {
                    foreach ( var connectionType in new ConnectionTypeService( rockContext )
                        .Queryable().AsNoTracking()
                        .OrderBy( t => t.Name ) )
                    {
                        var item = new TreeViewItem();
                        item.Id = string.Format( "T{0}", connectionType.Id );
                        item.Name = connectionType.Name;
                        item.HasChildren = connectionType.ConnectionOpportunities.Any();
                        item.IconCssClass = connectionType.IconCssClass;
                        list.Add( item );
                    }
                }
            }

            return list.AsQueryable();
        }

    }
}
