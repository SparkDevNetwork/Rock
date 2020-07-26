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

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    [EntityTypeField( "Entity Type",
        Description = "Display categories associated with this type of entity",
        Key = AttributeKey.EntityType )]

    [TextField( "Entity Type Friendly Name",
        Description = "The text to show for the entity type name. Leave blank to get it from the specified Entity Type",
        IsRequired = false,
        Key = AttributeKey.EntityTypeFriendlyName )]

    [TextField( "Entity Type Qualifier Property",
        IsRequired = false,
        Key = AttributeKey.EntityTypeQualifierProperty )]

    [TextField( "Entity type Qualifier Value",
        IsRequired = false,
        Key = AttributeKey.EntityTypeQualifierValue )]

    [BooleanField( "Show Unnamed Entity Items",
        Description = "Set to false to hide any EntityType items that have a blank name.",
        DefaultValue = "true",
        Key = AttributeKey.ShowUnnamedEntityItems )]

    [TextField( "Page Parameter Key",
        Description = "The page parameter to look for",
        Key = AttributeKey.PageParameterKey )]

    [TextField( "Default Icon CSS Class",
        Description = "The icon CSS class to use when the treeview displays items that do not have an IconCSSClass property",
        IsRequired = false,
        DefaultValue = "fa fa-list-ol",
        Key = AttributeKey.DefaultIconCSSClass )]
    
    [CategoryField( "Root Category",
        Description = "Select the root category to use as a starting point for the tree view.",
        AllowMultiple = false,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.RootCategory )]

    [CategoryField( "Exclude Categories",
        Description = "Select any category that you need to exclude from the tree view",
        AllowMultiple = true,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.ExcludeCategories )]

    public partial class CategoryTreeView : RockBlockCustomSettings
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string EntityType = "EntityType";
            public const string EntityTypeFriendlyName = "EntityTypeFriendlyName";
            public const string EntityTypeQualifierProperty = "EntityTypeQualifierProperty";
            public const string EntityTypeQualifierValue = "EntitytypeQualifierValue";
            public const string ShowUnnamedEntityItems = "ShowUnnamedEntityItems";
            public const string PageParameterKey = "PageParameterKey";
            public const string DefaultIconCSSClass = "DefaultIconCSSClass";
            public const string RootCategory = "RootCategory";
            public const string ExcludeCategories = "ExcludeCategories";
        }

        public const string CategoryNodePrefix = "C";

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

            bool? hideInactiveItems = this.GetUserPreference( "HideInactiveItems" ).AsBooleanOrNull();
            if ( !hideInactiveItems.HasValue )
            {
                hideInactiveItems = false;
            }

            tglHideInactiveItems.Checked = hideInactiveItems ?? false;
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

            mdCategoryTreeConfig.Visible = false;

            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );

            // hide all the actions if user doesn't have EDIT to the block
            divTreeviewActions.Visible = canEditBlock;

            var detailPageReference = new Rock.Web.PageReference( GetAttributeValue( AttributeKey.DetailPage ) );

            // NOTE: if the detail page is the current page, use the current route instead of route specified in the DetailPage (to preserve old behavior)
            if ( detailPageReference == null || detailPageReference.PageId == this.RockPage.PageId )
            {
                hfPageRouteTemplate.Value = ( this.RockPage.RouteData.Route as System.Web.Routing.Route ).Url;
                hfDetailPageUrl.Value = new Rock.Web.PageReference( this.RockPage.PageId ).BuildUrl();
            }
            else
            {
                hfPageRouteTemplate.Value = string.Empty;
                var pageCache = PageCache.Get( detailPageReference.PageId );
                if ( pageCache != null )
                {
                    var route = pageCache.PageRoutes.FirstOrDefault( a => a.Id == detailPageReference.RouteId );
                    if ( route != null )
                    {
                        hfPageRouteTemplate.Value = route.Route;
                    }
                }

                hfDetailPageUrl.Value = detailPageReference.BuildUrl();
            }

            // Get EntityTypeName
            Guid? entityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
            nbWarning.Text = "Please select an entity type in the block settings.";
            nbWarning.Visible = !entityTypeGuid.HasValue;
            if ( entityTypeGuid.HasValue )
            {
                var cachedEntityType = EntityTypeCache.Get( entityTypeGuid.Value );
                if ( cachedEntityType != null )
                {
                    Type entityType = cachedEntityType.GetEntityType();
                    bool isActivatedType = entityType != null && typeof( IHasActiveFlag ).IsAssignableFrom( entityType );
                    pnlConfigPanel.Visible = isActivatedType;
                    pnlRolloverConfig.Visible = isActivatedType;

                    string entityTypeFriendlyName = GetAttributeValue( AttributeKey.EntityTypeFriendlyName );
                    if ( string.IsNullOrWhiteSpace( entityTypeFriendlyName ) )
                    {
                        entityTypeFriendlyName = cachedEntityType.FriendlyName;
                    }

                    lbAddItem.ToolTip = "Add " + entityTypeFriendlyName;
                    lAddItem.Text = entityTypeFriendlyName;

                    string entityTypeQualiferColumn = GetAttributeValue( AttributeKey.EntityTypeQualifierProperty );
                    string entityTypeQualifierValue = GetAttributeValue( AttributeKey.EntityTypeQualifierValue );
                    bool showUnnamedEntityItems = GetAttributeValue( AttributeKey.ShowUnnamedEntityItems ).AsBooleanOrNull() ?? true;

                    string parms = string.Format( "?getCategorizedItems=true&showUnnamedEntityItems={0}", showUnnamedEntityItems.ToTrueFalse().ToLower() );
                    parms += string.Format( "&entityTypeId={0}", cachedEntityType.Id );
                    parms += string.Format( "&includeInactiveItems={0}", ( !tglHideInactiveItems.Checked ).ToTrueFalse() );

                    Guid? rootCategoryGuid = this.GetAttributeValue( AttributeKey.RootCategory ).AsGuidOrNull();

                    CategoryCache rootCategory = null;
                    if ( rootCategoryGuid.HasValue )
                    {
                        rootCategory = CategoryCache.Get( rootCategoryGuid.Value );
                    }

                    // make sure the rootCategory matches the EntityTypeId (just in case they changed the EntityType after setting RootCategory
                    if ( rootCategory != null && rootCategory.EntityTypeId == cachedEntityType.Id )
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

                    var excludeCategoriesGuids = this.GetAttributeValue( AttributeKey.ExcludeCategories ).SplitDelimitedValues().AsGuidList();
                    List<int> excludedCategoriesIds = new List<int>();
                    if ( excludeCategoriesGuids != null && excludeCategoriesGuids.Any() )
                    {
                        foreach ( var excludeCategoryGuid in excludeCategoriesGuids )
                        {
                            var excludedCategory = CategoryCache.Get( excludeCategoryGuid );
                            if ( excludedCategory != null )
                            {
                                excludedCategoriesIds.Add( excludedCategory.Id );
                            }
                        }

                        parms += string.Format( "&excludedCategoryIds={0}", excludedCategoriesIds.AsDelimited( "," ) );
                    }

                    string defaultIconCssClass = GetAttributeValue( AttributeKey.DefaultIconCSSClass );
                    if ( !string.IsNullOrWhiteSpace( defaultIconCssClass ) )
                    {
                        parms += string.Format( "&defaultIconCssClass={0}", defaultIconCssClass );
                    }

                    RestParms = parms;

                    // Attempt to retrieve an EntityId from the Page URL parameters.
                    PageParameterName = GetAttributeValue( AttributeKey.PageParameterKey );

                    string selectedNodeId = null;

                    int? itemId = PageParameter( PageParameterName ).AsIntegerOrNull();
                    string selectedEntityType;
                    if ( itemId.HasValue )
                    {
                        selectedNodeId = itemId.ToString();
                        selectedEntityType = ( cachedEntityType != null ) ? cachedEntityType.Name : string.Empty;
                    }
                    else
                    {
                        // If an EntityId was not specified, check for a CategoryId.
                        itemId = PageParameter( "CategoryId" ).AsIntegerOrNull();

                        selectedNodeId = CategoryNodePrefix + itemId;
                        selectedEntityType = "category";
                    }

                    lbAddCategoryRoot.Enabled = true;
                    lbAddCategoryChild.Enabled = false;
                    lbAddItem.Enabled = false;

                    CategoryCache selectedCategory = null;

                    if ( !string.IsNullOrEmpty( selectedNodeId ) )
                    {
                        hfSelectedItemId.Value = selectedNodeId;
                        List<string> parentIdList = new List<string>();

                        if ( selectedEntityType.Equals( "category" ) )
                        {
                            selectedCategory = CategoryCache.Get( itemId.GetValueOrDefault() );
                            if ( selectedCategory != null && !canEditBlock && selectedCategory.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                            {
                                // Show the action buttons if user has edit rights to category
                                divTreeviewActions.Visible = true;
                            }
                        }
                        else
                        {
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
                                        selectedCategory = CategoryCache.Get( entity.CategoryId.Value );
                                        if ( selectedCategory != null )
                                        {
                                            string categoryExpandedID = CategoryNodePrefix + selectedCategory.Id.ToString();
                                            parentIdList.Insert( 0, CategoryNodePrefix + categoryExpandedID );
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
                                string categoryExpandedID = CategoryNodePrefix + category.Id.ToString();
                                if ( !parentIdList.Contains( categoryExpandedID ) )
                                {
                                    parentIdList.Insert( 0, categoryExpandedID );
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
        }

        protected void tglHideInactiveItems_CheckedChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( "HideInactiveItems", tglHideInactiveItems.Checked.ToTrueFalse() );

            // reload the whole page
            NavigateToPage( this.RockPage.Guid, new Dictionary<string, string>() );
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
                NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
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

            NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbAddCategoryRoot control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCategoryRoot_Click( object sender, EventArgs e )
        {
            // if a rootCategory is set, set that as the parentCategory when they select "add top-level"
            var rootCategory = new CategoryService( new RockContext() ).Get( this.GetAttributeValue( AttributeKey.RootCategory ).AsGuid() );
            int parentCategoryId = rootCategory != null ? rootCategory.Id : 0;

            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "CategoryId", 0.ToString() );
            if ( parentCategoryId > 0 )
            {
                qryParams.Add( "parentCategoryId", parentCategoryId.ToString() );
            }

            qryParams.Add( "ExpandedIds", hfInitialCategoryParentIds.Value );

            NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            mdCategoryTreeConfig.Visible = true;
            var entityType = EntityTypeCache.Get( this.GetAttributeValue( AttributeKey.EntityType ).AsGuid() );
            var rootCategory = new CategoryService( new RockContext() ).Get( this.GetAttributeValue( AttributeKey.RootCategory ).AsGuid() );


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

            var excludedCategories = new CategoryService( new RockContext() ).GetByGuids( this.GetAttributeValue( AttributeKey.ExcludeCategories ).SplitDelimitedValues().AsGuidList() );
            cpExcludeCategories.EntityTypeId = entityType != null ? entityType.Id : 0;

            // make sure the excluded categories matches the EntityTypeId (just in case they changed the EntityType after setting excluded categories
            if ( excludedCategories != null && excludedCategories.All( a => a.EntityTypeId == cpExcludeCategories.EntityTypeId ) )
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
            var selectedCategory = CategoryCache.Get( cpRootCategory.SelectedValue.AsInteger() );
            this.SetAttributeValue( AttributeKey.RootCategory, selectedCategory != null ? selectedCategory.Guid.ToString() : string.Empty );

            var excludedCategoryIds = cpExcludeCategories.SelectedValuesAsInt();
            var excludedCategoryGuids = new List<Guid>();
            foreach ( int excludedCategoryId in excludedCategoryIds )
            {
                var excludedCategory = CategoryCache.Get( excludedCategoryId );
                if ( excludedCategory != null )
                {
                    excludedCategoryGuids.Add( excludedCategory.Guid );
                }
            }

            this.SetAttributeValue( AttributeKey.ExcludeCategories, excludedCategoryGuids.AsDelimited( "," ) );

            this.SaveAttributeValues();

            mdCategoryTreeConfig.Hide();
            Block_BlockUpdated( sender, e );

            mdCategoryTreeConfig.Visible = false;
        }
    }
}