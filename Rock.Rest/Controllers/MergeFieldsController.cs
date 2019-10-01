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
using System.Linq;
using System.Reflection;

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
    public partial class MergeFieldsController : ApiControllerBase
    {
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/MergeFields/{id}" )]
        public virtual string Get( string id )
        {
            return Rock.Web.UI.Controls.MergeFieldPicker.FormatSelectedValue( id );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="additionalFields">The additional fields.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/MergeFields/GetChildren/{id}" )]
        public IQueryable<TreeViewItem> GetChildren( string id, string additionalFields )
        {
            var person = GetPerson();

            List<TreeViewItem> items = new List<TreeViewItem>();

            switch ( id )
            {
                case "0":
                    {
                        if ( !string.IsNullOrWhiteSpace( additionalFields ) )
                        {
                            foreach ( string fieldInfo in additionalFields.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                            {
                                string[] parts = fieldInfo.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

                                string fieldId = parts.Length > 0 ? parts[0] : string.Empty;

                                if ( fieldId == "AdditionalMergeFields" )
                                {
                                    if ( parts.Length > 1 )
                                    {
                                        var fieldsTv = new TreeViewItem
                                        {
                                            Id = $"AdditionalMergeFields_{parts[1]}",
                                            Name = "Additional Fields",
                                            HasChildren = true,
                                            Children = new List<TreeViewItem>()
                                        };

                                        foreach ( string fieldName in parts[1].Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries ) )
                                        {
                                            fieldsTv.Children.Add( new TreeViewItem
                                            {
                                                Id = $"AdditionalMergeField_{fieldName}",
                                                Name = fieldName.SplitCase(),
                                                HasChildren = false
                                            } );
                                        }
                                        items.Add( fieldsTv );
                                    }
                                }
                                else
                                {
                                    string[] idParts = fieldId.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                                    string fieldType = idParts.Length > 1 ? idParts[1] : fieldId;

                                    var entityType = EntityTypeCache.Get( fieldType, false );
                                    if ( entityType != null )
                                    {
                                        items.Add( new TreeViewItem
                                        {
                                            Id = fieldId,
                                            Name = parts.Length > 1 ? parts[1] : entityType.FriendlyName,
                                            HasChildren = true
                                        } );
                                    }
                                    else
                                    {
                                        items.Add( new TreeViewItem
                                        {
                                            Id = fieldId,
                                            Name = parts.Length > 1 ? parts[1] : fieldType.SplitCase(),
                                            HasChildren = fieldType == "GlobalAttribute"
                                        } );
                                    }
                                }
                            }
                        }

                        break;
                    }

                case "GlobalAttribute":
                    {
                        var globalAttributes = GlobalAttributesCache.Get();

                        foreach ( var attributeCache in globalAttributes.Attributes.OrderBy( a => a.Key ) )
                        {
                            if ( attributeCache.IsAuthorized( Authorization.VIEW, person ) )
                            {
                                items.Add( new TreeViewItem
                                {
                                    Id = "GlobalAttribute|" + attributeCache.Key,
                                    Name = attributeCache.Name,
                                    HasChildren = false
                                } );
                            }
                        }

                        break;
                    }

                default:
                    {
                        // In this scenario, the id should be a concatenation of a root qualified entity name and then the property path
                        var idParts = id.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                        if ( idParts.Count > 0 )
                        {
                            // Get the root type
                            int pathPointer = 0;
                            EntityTypeCache entityType = null;
                            while ( entityType == null && pathPointer < idParts.Count() )
                            {
                                string item = idParts[pathPointer];
                                string[] itemParts = item.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                                string itemType = itemParts.Length > 1 ? itemParts[1] : item;

                                entityType = EntityTypeCache.Get( itemType, false );
                                pathPointer++;
                            }

                            if ( entityType != null )
                            {
                                Type type = entityType.GetEntityType();

                                // Traverse the Property path
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

                                entityType = EntityTypeCache.Get( type );

                                // Add the tree view items
                                foreach ( var propInfo in type.GetProperties() )
                                {
                                    if ( propInfo.GetCustomAttributes( typeof( System.Runtime.Serialization.DataMemberAttribute ) ).Count() > 0 )
                                    {
                                        var treeViewItem = new TreeViewItem
                                        {
                                            Id = id + "|" + propInfo.Name,
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
                                        if ( EntityTypeCache.Get( propertyType.FullName, false ) != null )
                                        {
                                            foreach ( var childPropInfo in propertyType.GetProperties() )
                                            {
                                                if ( childPropInfo.GetCustomAttributes( typeof( System.Runtime.Serialization.DataMemberAttribute ) ).Count() > 0 )
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

                                if ( type == typeof( Rock.Model.Person ) )
                                {
                                    items.Add( new TreeViewItem
                                    {
                                        Id = id + "|" + "Campus",
                                        Name = "Campus"
                                    } );
                                }

                                if ( entityType.IsEntity )
                                {
                                    foreach ( Rock.Model.Attribute attribute in new AttributeService( new Rock.Data.RockContext() ).GetByEntityTypeId( entityType.Id, false ) )
                                    {
                                        // Only include attributes without a qualifier (since we don't have a specific instance of this entity type)
                                        if ( string.IsNullOrEmpty( attribute.EntityTypeQualifierColumn ) &&
                                            string.IsNullOrEmpty( attribute.EntityTypeQualifierValue ) &&
                                            attribute.IsAuthorized( Authorization.VIEW, person ) )
                                        {
                                            items.Add( new TreeViewItem
                                            {
                                                Id = id + "|" + attribute.Key,
                                                Name = attribute.Name
                                            } );
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }
            }

            return items.OrderBy( i => i.Name ).AsQueryable();
        }


    }
}
