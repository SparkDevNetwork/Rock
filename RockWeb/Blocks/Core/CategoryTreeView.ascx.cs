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
    [TextField("Default Icon CSS Class", "The icon CSS class to use when the treeview displays items that do not have an IconCSSClass property", false, "fa fa-list-ol" )]

    [CategoryField( "Root Category", "Select the root category to use as a starting point for the tree view.", false, Category = "CustomSetting" )]
    [CategoryField( "Exclude Categories", "Select any category that you need to exclude from the tree view", true, Category = "CustomSetting" )]
    public partial class CategoryTreeView : RockBlockCustomSettings
    {
        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public override string SettingsToolTip
        {
            get
            {
                return "Set Category Options";
            }
        }

        /// <summary>
        /// The RestParams (used by the Markup)
        /// </summary>
        protected string RestParms;

        /// <summary>
        /// The page parameter name (used by the Markup)
        /// </summary>
        protected string PageParameterName;

        /// <summary>
        /// Gets or sets the selected category identifier.
        /// </summary>
        /// <value>
        /// The selected category identifier.
        /// </value>
        protected int? SelectedCategoryId { get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upCategoryTree );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToPage( this.RockPage.Guid, new Dictionary<string, string>() );
        }

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

            var detailPageReference = new Rock.Web.PageReference( GetAttributeValue( "DetailPage" ) );
            
            // NOTE: if the detail page is the current page, use the current route instead of route specified in the DetailPage (to preserve old behavior)
            if ( detailPageReference == null || detailPageReference.PageId == this.RockPage.PageId )
            {
                hfPageRouteTemplate.Value = ( this.RockPage.RouteData.Route as System.Web.Routing.Route ).Url;
                hfDetailPageUrl.Value = new Rock.Web.PageReference( this.RockPage.PageId ).BuildUrl().RemoveLeadingForwardslash();
            }
            else
            {
                hfPageRouteTemplate.Value = string.Empty;
                var pageCache = PageCache.Read( detailPageReference.PageId );
                if ( pageCache != null )
                {
                    var route = pageCache.PageRoutes.FirstOrDefault( a => a.Id == detailPageReference.RouteId );
                    if ( route != null )
                    {
                        hfPageRouteTemplate.Value = route.Route;
                    }
                }

                hfDetailPageUrl.Value = detailPageReference.BuildUrl().RemoveLeadingForwardslash();
            }

            // Get EntityTypeName
            Guid? entityTypeGuid = GetAttributeValue( "EntityType" ).AsGuidOrNull();
            nbWarning.Text = "Please select an entity type in the block settings.";
            nbWarning.Visible = !entityTypeGuid.HasValue;
            if ( entityTypeGuid.HasValue )
            {
                int entityTypeId = Rock.Web.Cache.EntityTypeCache.Read( entityTypeGuid.Value ).Id;
                string entityTypeQualiferColumn = GetAttributeValue( "EntityTypeQualifierProperty" );
                string entityTypeQualifierValue = GetAttributeValue( "EntityTypeQualifierValue" );
                bool showUnnamedEntityItems = GetAttributeValue( "ShowUnnamedEntityItems" ).AsBooleanOrNull() ?? true;

                string parms = string.Format( "?getCategorizedItems=true&showUnnamedEntityItems={0}", showUnnamedEntityItems.ToTrueFalse().ToLower() );
                parms += string.Format( "&entityTypeId={0}", entityTypeId );

                var rootCategory = CategoryCache.Read( this.GetAttributeValue( "RootCategory" ).AsGuid() );

                // make sure the rootCategory matches the EntityTypeId (just in case they changed the EntityType after setting RootCategory
                if ( rootCategory != null && rootCategory.EntityTypeId == entityTypeId )
                {
                    parms += string.Format( "&rootCategoryId={0}", rootCategory.Id );
                }

                if ( !string.IsNullOrEmpty( entityTypeQualiferColumn ) )
                {
                    parms += string.Format( "&entityQualifier={0}", entityTypeQualiferColumn );

                    if ( !string.IsNullOrEmpty( entityTypeQualifierValue ) )
                    {
                        parms += string.Format( "&entityQualifierValue={0}", entityTypeQualifierValue );
                    }
                }

                var excludeCategoriesGuids = this.GetAttributeValue( "ExcludeCategories" ).SplitDelimitedValues().AsGuidList();
                List<int> excludedCategoriesIds = new List<int>();
                if ( excludeCategoriesGuids != null && excludeCategoriesGuids.Any() )
                {
                    foreach ( var excludeCategoryGuid in excludeCategoriesGuids )
                    {
                        var excludedCategory = CategoryCache.Read( excludeCategoryGuid );
                        if (excludedCategory != null)
                        {
                            excludedCategoriesIds.Add( excludedCategory.Id );
                        }
                    }
                    
                    parms += string.Format( "&excludedCategoryIds={0}", excludedCategoriesIds.AsDelimited(",") );
                }

                string defaultIconCssClass = GetAttributeValue("DefaultIconCSSClass");
                if ( !string.IsNullOrWhiteSpace( defaultIconCssClass ) )
                {
                    parms += string.Format( "&defaultIconCssClass={0}", defaultIconCssClass );
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
                int? itemId = PageParameter( PageParameterName ).AsIntegerOrNull();
                string selectedEntityType = cachedEntityType.Name;
                if ( !itemId.HasValue )
                {
                    itemId = PageParameter( "CategoryId" ).AsIntegerOrNull();
                    selectedEntityType = "category";
                }

                lbAddCategoryRoot.Enabled = true;
                lbAddCategoryChild.Enabled = false;
                lbAddItem.Enabled = false;

                CategoryCache selectedCategory = null;

                if ( itemId.HasValue )
                {
                    hfSelectedItemId.Value = itemId.Value.ToString();
                    List<string> parentIdList = new List<string>();

                    if ( selectedEntityType.Equals( "category" ) )
                    {
                        selectedCategory = CategoryCache.Read( itemId.Value );
                    }
                    else
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
                                ICategorized entity = getMethod.Invoke( serviceInstance, new object[] { itemId } ) as ICategorized;

                                if ( entity != null )
                                {
                                    lbAddCategoryChild.Enabled = false;
                                    if ( entity.CategoryId.HasValue )
                                    {
                                        selectedCategory = CategoryCache.Read( entity.CategoryId.Value );
                                        if ( selectedCategory != null )
                                        {
                                            parentIdList.Insert( 0, selectedCategory.Id.ToString() );
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // get the parents of the selected item so we can tell the treeview to expand those
                    var category = selectedCategory;
                    while ( category != null )
                    {
                        category = category.ParentCategory;
                        if ( category != null )
                        {
                            if ( !parentIdList.Contains( category.Id.ToString() ) )
                            {
                                parentIdList.Insert( 0, category.Id.ToString() );
                            }
                            else
                            {
                                // infinite recursion
                                break;
                            }
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

                selectedCategory = selectedCategory ?? rootCategory;

                if ( selectedCategory != null )
                {
                    lbAddItem.Enabled = true;
                    lbAddCategoryChild.Enabled = true;
                    this.SelectedCategoryId = selectedCategory.Id;
                }
                else
                {
                    this.SelectedCategoryId = null;
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
            int parentCategoryId = this.SelectedCategoryId ?? 0;
            if ( parentCategoryId > 0 )
            {
                qryParams.Add( "parentCategoryId", parentCategoryId.ToString() );
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
            int parentCategoryId = this.SelectedCategoryId ?? 0;

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
            // if a rootCategory is set, set that as the parentCategory when they select "add top-level"
            var rootCategory = new CategoryService( new RockContext() ).Get( this.GetAttributeValue( "RootCategory" ).AsGuid() );
            int parentCategoryId = rootCategory != null ? rootCategory.Id : 0;

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
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            var entityType = EntityTypeCache.Read( this.GetAttributeValue( "EntityType" ).AsGuid() );
            var rootCategory = new CategoryService( new RockContext() ).Get( this.GetAttributeValue( "RootCategory" ).AsGuid() );
            

            cpRootCategory.EntityTypeId = entityType != null ? entityType.Id : 0;
            

            // make sure the rootCategory matches the EntityTypeId (just in case they changed the EntityType after setting RootCategory
            if ( rootCategory != null && cpRootCategory.EntityTypeId == rootCategory.EntityTypeId )
            {
                cpRootCategory.SetValue( rootCategory );
            }
            else
            {
                cpRootCategory.SetValue( null );
            }

            cpRootCategory.Enabled = entityType != null;
            nbRootCategoryEntityTypeWarning.Visible = entityType == null;

            var excludedCategories = new CategoryService( new RockContext() ).GetByGuids( this.GetAttributeValue( "ExcludeCategories" ).SplitDelimitedValues().AsGuidList() );
            cpExcludeCategories.EntityTypeId = entityType != null ? entityType.Id : 0;

            // make sure the excluded categories matches the EntityTypeId (just in case they changed the EntityType after setting excluded categories
            if ( excludedCategories != null && excludedCategories.All( a => a.EntityTypeId == cpExcludeCategories.EntityTypeId)  )
            {
                cpExcludeCategories.SetValues( excludedCategories );
            }
            else
            {
                cpExcludeCategories.SetValue( null );
            }

            mdCategoryTreeConfig.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdCategoryTreeConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdCategoryTreeConfig_SaveClick( object sender, EventArgs e )
        {
            var selectedCategory = CategoryCache.Read( cpRootCategory.SelectedValue.AsInteger() );
            this.SetAttributeValue( "RootCategory", selectedCategory != null ? selectedCategory.Guid.ToString() : string.Empty );

            var excludedCategoryIds = cpExcludeCategories.SelectedValuesAsInt();
            var excludedCategoryGuids = new List<Guid>();
            foreach (int excludedCategoryId in excludedCategoryIds)
            {
                var excludedCategory = CategoryCache.Read( excludedCategoryId );
                if (excludedCategory != null)
                {
                    excludedCategoryGuids.Add( excludedCategory.Guid );
                }
            }

            this.SetAttributeValue( "ExcludeCategories", excludedCategoryGuids.AsDelimited( "," ) );

            this.SaveAttributeValues();

            mdCategoryTreeConfig.Hide();
            Block_BlockUpdated( sender, e );
        }
    }
}