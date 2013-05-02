//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using Rock.Web.UI.Controls.Pickers;

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
                }
            }

            return string.Format( "{{{{ {0} }}}}", idParts.AsDelimited( "." ) );
        }

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

                    Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
                        .Where( v =>
                            v.Key.StartsWith( "Organization", StringComparison.CurrentCultureIgnoreCase ) ||
                            v.Key.StartsWith( "Email", StringComparison.CurrentCultureIgnoreCase ) )
                        .OrderBy( v => v.Key )
                        .ToList()
                        .ForEach( v => items.Add( new TreeViewItem
                        {
                            Id = "GlobalAttribute," + v.Key,
                            Name = v.Value.Key,
                            HasChildren = false
                        } ) );

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
                                }
                                pathPointer++;
                            }

                            // Add the tree view items
                            foreach ( var propInfo in type.GetProperties() )
                            {
                                var childEntityType = EntityTypeCache.Read( propInfo.PropertyType.FullName, false );
                                items.Add( new TreeViewItem
                                {
                                    Id = id + "," + propInfo.Name,
                                    Name = propInfo.Name.SplitCase(),
                                    HasChildren = childEntityType != null
                                } );
                            }
                        }
                    }
                    break;
            }

            return items.AsQueryable();

        }
    }


}
