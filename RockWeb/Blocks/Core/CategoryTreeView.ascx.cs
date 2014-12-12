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
using System.ComponentModel;
using System.Linq;
using System.Text;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Category Tree View" )]
    [Category( "Core" )]
    [Description( "Displays a tree of categories for the configured entity type." )]

    [LinkedPage( "Detail Page" )]
    [EntityTypeField( "Entity Type", "Display categories associated with this type of entity" )]
    [TextField( "Entity Type Friendly Name", "The text to show for the entity type name. Leave blank to get it from the specified Entity Type" )]
    [TextField( "Entity Type Qualifier Property", "", false )]
    [TextField( "Entity type Qualifier Value", "", false )]
    [BooleanField( "Show Unnamed Entity Items", "Set to false to hide any EntityType items that have a blank name.", true )]
    [TextField( "Page Parameter Key", "The page parameter to look for" )]
    public partial class CategoryTreeView : RockBlock
    {
        /// <summary>
        /// The entity type name
        /// </summary>
        protected string RestParms;

        /// <summary>
        /// The page parameter name
        /// </summary>
        protected string PageParameterName;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );

            // hide all the actions if user doesn't have EDIT to the block
            divTreeviewActions.Visible = canEditBlock;
            hfPageRouteTemplate.Value = ( this.RockPage.RouteData.Route as System.Web.Routing.Route ).Url;

            // Get EntityTypeName
            Guid entityTypeGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "EntityType" ), out entityTypeGuid ) )
            {
                int entityTypeId = Rock.Web.Cache.EntityTypeCache.Read( entityTypeGuid ).Id;
                string entityTypeQualiferColumn = GetAttributeValue( "EntityTypeQualifierProperty" );
                string entityTypeQualifierValue = GetAttributeValue( "EntityTypeQualifierValue" );
                bool showUnnamedEntityItems = GetAttributeValue("ShowUnnamedEntityItems").AsBooleanOrNull() ?? true;

                string parms = string.Format("?getCategorizedItems=true&showUnnamedEntityItems={0}", showUnnamedEntityItems.ToTrueFalse().ToLower());
                parms += string.Format( "&entityTypeId={0}", entityTypeId );

                if ( !string.IsNullOrEmpty( entityTypeQualiferColumn ) )
                {
                    parms += string.Format( "&entityQualifier={0}", entityTypeQualiferColumn );

                    if ( !string.IsNullOrEmpty( entityTypeQualifierValue ) )
                    {
                        parms += string.Format( "&entityQualifierValue={0}", entityTypeQualifierValue );
                    }
                }

                RestParms = parms;

                var cachedEntityType = Rock.Web.Cache.EntityTypeCache.Read( entityTypeId );
                if ( cachedEntityType != null )
                {
                    string entityTypeFriendlyName = GetAttributeValue( "EntityTypeFriendlyName" );
                    if ( string.IsNullOrWhiteSpace( entityTypeFriendlyName ) )
                    {
                        entityTypeFriendlyName = cachedEntityType.FriendlyName;
                    }

                    lbAddItem.ToolTip = "Add " + entityTypeFriendlyName;
                    lAddItem.Text = entityTypeFriendlyName;
                }
 
                PageParameterName = GetAttributeValue( "PageParameterKey" );
                string itemId = PageParameter( PageParameterName );
                string selectedEntityType = cachedEntityType.Name;
                if ( string.IsNullOrWhiteSpace( itemId ) )
                {
                    itemId = PageParameter( "CategoryId" );
                    selectedEntityType = "category";
                }

                lbAddCategoryRoot.Enabled = true;
                lbAddCategoryChild.Enabled = false;
                lbAddItem.Enabled = false;

                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    hfInitialItemId.Value = itemId;
                    hfInitialEntityIsCategory.Value = ( selectedEntityType == "category" ).ToString();
                    hfSelectedCategoryId.Value = itemId;
                    List<string> parentIdList = new List<string>();

                    CategoryCache category = null;
                    if ( selectedEntityType.Equals( "category" ) )
                    {
                        category = CategoryCache.Read( int.Parse( itemId ) );
                        lbAddItem.Enabled = true;
                        lbAddCategoryChild.Enabled = true;
                    }
                    else
                    {
                        int id = 0;
                        if ( int.TryParse( itemId, out id ) )
                        {
                            if ( cachedEntityType != null )
                            {
                                Type entityType = cachedEntityType.GetEntityType();
                                if ( entityType != null )
                                {
                                    Type serviceType = typeof( Rock.Data.Service<> );
                                    Type[] modelType = { entityType };
                                    Type service = serviceType.MakeGenericType( modelType );
                                    var serviceInstance = Activator.CreateInstance( service, new object[] { new RockContext() } );
                                    var getMethod = service.GetMethod( "Get", new Type[] { typeof( int ) } );
                                    ICategorized entity = getMethod.Invoke( serviceInstance, new object[] { id } ) as ICategorized;

                                    if ( entity != null )
                                    {
                                        lbAddCategoryChild.Enabled = false;
                                        if ( entity.CategoryId.HasValue )
                                        {
                                            category = CategoryCache.Read( entity.CategoryId.Value );
                                            if ( category != null )
                                            {
                                                parentIdList.Insert( 0, category.Id.ToString() );
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // get the parents of the selected item so we can tell the treeview to expand those
                    while ( category != null )
                    {
                        category = category.ParentCategory;
                        if ( category != null )
                        {
                            parentIdList.Insert( 0, category.Id.ToString() );
                        }

                    }
                    // also get any additional expanded nodes that were sent in the Post
                    string postedExpandedIds = this.Request.Params["ExpandedIds"];
                    if ( !string.IsNullOrWhiteSpace( postedExpandedIds ) )
                    {
                        var postedExpandedIdList = postedExpandedIds.Split( ',' ).ToList();
                        foreach ( var id in postedExpandedIdList )
                        {
                            if ( !parentIdList.Contains( id ) )
                            {
                                parentIdList.Add( id );
                            }
                        }
                    }

                    hfInitialCategoryParentIds.Value = parentIdList.AsDelimited( "," );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddItem_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( PageParameterName, 0.ToString() );
            int parentCategoryId = hfSelectedCategoryId.Value.AsInteger();
            if (parentCategoryId > 0)
            {
                qryParams.Add( "parentCategoryId", hfSelectedCategoryId.Value );
                qryParams.Add( "ExpandedIds", hfInitialCategoryParentIds.Value );
                NavigateToLinkedPage( "DetailPage", qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddCategoryChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCategoryChild_Click( object sender, EventArgs e )
        {
            int parentCategoryId = hfSelectedCategoryId.ValueAsInt();

            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "CategoryId", 0.ToString() );
            if ( parentCategoryId > 0 )
            {
                qryParams.Add( "parentCategoryId", parentCategoryId.ToString() );
            }
            qryParams.Add( "ExpandedIds", hfInitialCategoryParentIds.Value );

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbAddCategoryRoot control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCategoryRoot_Click( object sender, EventArgs e )
        {
            int parentCategoryId = hfSelectedCategoryId.ValueAsInt();

            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "CategoryId", 0.ToString() );
            qryParams.Add( "ExpandedIds", hfInitialCategoryParentIds.Value );

            NavigateToLinkedPage( "DetailPage", qryParams );
        }
    }
}