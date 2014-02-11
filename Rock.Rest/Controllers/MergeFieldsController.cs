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
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MergeFieldsController : ApiController, IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "MergeFieldsGetChildren",
                routeTemplate: "api/MergeFields/GetChildren/{id}/{additionalFields}",
                defaults: new
                {
                    controller = "MergeFields",
                    action = "GetChildren"
                } );
        }

        [Authenticate, Secured]
        public virtual string Get( string id )
        {
            var idParts = id.SplitDelimitedValues().ToList();
            if ( idParts.Count > 0 )
            {
                // Get the root type
                var entityType = EntityTypeCache.Read( idParts[0], false );
                if ( entityType != null )
                {
                    idParts[0] = entityType.FriendlyName.Replace( " ", string.Empty );

                    Type type = entityType.GetEntityType();

                    var workingParts = idParts.Take( 1 ).ToList();
                    var formatString = "{0}";

                    // Traverse the Property path
                    bool itemIsCollection = false;

                    int pathPointer = 1;
                    while ( idParts.Count > pathPointer )
                    {
                        string propertyName =  idParts[pathPointer];
                        workingParts.Add(propertyName);

                        var childProperty = type.GetProperty( propertyName );
                        if ( childProperty != null )
                        {
                            type = childProperty.PropertyType;

                            if ( type.IsGenericType &&
                                type.GetGenericTypeDefinition() == typeof( ICollection<> ) &&
                                type.GetGenericArguments().Length == 1 )
                            {
                                string propertyNameSingularized = propertyName.Singularize();
                                string forString = string.Format( "<% for {0} in {1} %> {{0}} <% endfor %>", propertyNameSingularized, workingParts.AsDelimited( "." ) );
                                workingParts.Clear();
                                workingParts.Add( propertyNameSingularized );
                                formatString = string.Format( formatString, forString );

                                type = type.GetGenericArguments()[0];

                                itemIsCollection = true;
                            }
                            else
                            {
                                itemIsCollection = false;
                            }

                        }
                        pathPointer++;
                    }

                    string itemString = itemIsCollection ? "" : string.Format( "<< {0} >>", workingParts.AsDelimited( "." ) );
                    return string.Format( formatString, itemString ).Replace( "<", "{" ).Replace( ">", "}" );
                }

                return string.Format( "{{{{ {0} }}}}", idParts.AsDelimited(".") );
            }

            return string.Empty;
        }

        [Authenticate, Secured]
        public IQueryable<TreeViewItem> GetChildren( string id, string additionalFields )
        {
            List<TreeViewItem> items = new List<TreeViewItem>();

            switch ( id )
            {
                case "0":
                    
                    if (!string.IsNullOrWhiteSpace(additionalFields))
                    {
                        foreach(string fieldName in additionalFields.SplitDelimitedValues())
                        {
                            var entityType = EntityTypeCache.Read( fieldName, false );
                            if (entityType != null)
                            {
                                items.Add(new TreeViewItem
                                {
                                    Id = fieldName,
                                    Name = entityType.FriendlyName,
                                    HasChildren = true
                                });
                            }
                            else
                            {
                                items.Add(new TreeViewItem
                                {
                                    Id = fieldName,
                                    Name = fieldName.SplitCase(),
                                    HasChildren = fieldName == "GlobalAttribute"
                                });
                            }
                        }
                    }

                    break;
                    
                case "GlobalAttribute":

                    var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();

                    foreach ( var attributeCache in globalAttributes.Attributes.OrderBy( a => a.Key ) )
                    {
                        if ( attributeCache.IsAuthorized( "View", null ) )
                        {
                            items.Add( new TreeViewItem
                            {
                                Id = "GlobalAttribute," + attributeCache.Key,
                                Name = attributeCache.Name,
                                HasChildren = false
                            } );
                        }
                    }

                    break;

                default:

                    // In this scenario, the id should be a concatonatioin of a root qualified entity name
                    // and then the property path
                    var idParts = id.SplitDelimitedValues().ToList();
                    if ( idParts.Count > 0 )
                    {
                        // Get the root type
                        var entityType = EntityTypeCache.Read( idParts[0], false );
                        if ( entityType != null )
                        {
                            Type type = entityType.GetEntityType();

                            // Traverse the Property path
                            int pathPointer = 1;
                            while ( idParts.Count > pathPointer )
                            {
                                var childProperty = type.GetProperty( idParts[pathPointer] );
                                if ( childProperty != null )
                                {
                                    type = childProperty.PropertyType;

                                    if ( type.IsGenericType &&
                                        type.GetGenericTypeDefinition() == typeof( ICollection<> ) &&
                                        type.GetGenericArguments().Length == 1 )
                                    {
                                        type = type.GetGenericArguments()[0];
                                    }
                                }
                                pathPointer++;
                            }

                            entityType = EntityTypeCache.Read( type );

                            // Add the tree view items
                            foreach ( var propInfo in type.GetProperties() )
                            {
                                if ( propInfo.GetCustomAttributes( typeof( Rock.Data.MergeFieldAttribute ) ).Count() > 0 )
                                {
                                    var treeViewItem = new TreeViewItem
                                    {
                                        Id = id + "," + propInfo.Name,
                                        Name = propInfo.Name.SplitCase()
                                    };

                                    Type propertyType = propInfo.PropertyType;

                                    if ( propertyType.IsGenericType &&
                                        propertyType.GetGenericTypeDefinition() == typeof( ICollection<> ) &&
                                        propertyType.GetGenericArguments().Length == 1 )
                                    {
                                        treeViewItem.Name += " (Collection)";
                                        propertyType = propertyType.GetGenericArguments()[0];
                                    }

                                    bool hasChildren = false;
                                    if ( EntityTypeCache.Read( propertyType.FullName, false ) != null )
                                    {
                                        foreach ( var childPropInfo in propertyType.GetProperties() )
                                        {
                                            if ( childPropInfo.GetCustomAttributes( typeof( Rock.Data.MergeFieldAttribute ) ).Count() > 0 )
                                            {
                                                hasChildren = true;
                                                break;
                                            }
                                        }
                                    }
                                    treeViewItem.HasChildren = hasChildren;

                                    items.Add( treeViewItem );
                                }
                            }

                            if ( entityType.IsEntity )
                            {
                                foreach ( Rock.Model.Attribute attribute in new AttributeService().GetByEntityTypeId( entityType.Id ) )
                                {
                                    // Only include attributes without a qualifier (since we don't have a specific instance of this entity type)
                                    if ( string.IsNullOrEmpty( attribute.EntityTypeQualifierColumn ) &&
                                        string.IsNullOrEmpty( attribute.EntityTypeQualifierValue ) &&
                                        attribute.IsAuthorized("View", null))
                                    {
                                        items.Add( new TreeViewItem
                                        {
                                            Id = id + "," + attribute.Key,
                                            Name = attribute.Name
                                        } );
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            return items.OrderBy( i => i.Name).AsQueryable();

        }
    }


}
