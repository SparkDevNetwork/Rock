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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control/Container to manage Edit/Display AttributeValues for an IHasAttributes
    /// </summary>
    public class AttributeValuesContainer : CompositeControl, IHasValidationGroup, INamingContainer
    {
        #region ViewStateKeys

        private static class ViewStateKey
        {
            public const string IncludedAttributeIds = "IncludedAttributeIds";
            public const string RequiredAttributeIds = "RequiredAttributeIds";
            public const string ExcludedAttributeIds = "ExcludedAttributeIds";
            public const string ValidationGroup = "ValidationGroup";
            public const string SuppressOrderingWithinCategory = "SuppressOrderingWithinCategory";
            public const string ShowCategoryLabel = "ShowCategoryLabel";
            public const string IncludedCategoryNames = "IncludedCategoryNames";
            public const string ExcludedCategoryNames = "ExcludedCategoryNames";
            public const string NumberOfColumns = "NumberOfColumns";
            public const string LimitToShowInGridAttributes = "LimitToShowInGridAttributes";
            public const string DisplayAsTabs = "DisplayAsTabs";
            public const string ShowPrePostHtml = "ShowPrePostHtml";
            public const string EntityId = "EntityId";
            public const string EntityTypeId = "EntityTypeId";
            public const string DisplayModeAttributeIdValuesState = "DisplayModeAttributeIdValuesState";
            public const string EditModeAttributeIdsState = "EditModeAttributeIdsState";
        }

        #endregion ViewStateKeys

        #region Controls

        /// <summary>
        /// The placeholder for attributes
        /// </summary>
        private DynamicPlaceholder _phAttributes;

        #endregion Controls

        #region Private fields

        // Keeps track of EntityTypeId of the Item we created edit/display controls for, so we can re-create them on PostBack
        private int? _entityTypeId;

        // Keeps track of Entity.Id of the Item we created edit/display controls for, so we can re-create them on PostBack
        private int? _entityId;

        // Keeps track of which attributes we created edit controls for, so we can re-create them on PostBack
        private List<int> _editModeAttributeIdsState;

        // Keeps track of which attributes and values we created display controls for, so we can re-create them on PostBack
        private Dictionary<int, string> _displayModeAttributeIdValuesState { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the validation group. (Default is RockBlock's BlockValidationGroup)
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get => ViewState[ViewStateKey.ValidationGroup] as string ?? this.RockBlock()?.BlockValidationGroup;
            set => ViewState[ViewStateKey.ValidationGroup] = value;
        }

        /// <summary>
        /// Attribute Display/Edit controls are sorted by Category (if there is a category). Then, by default, they are sorted by EntityTypeQualifier, Order, and Name.
        /// To keep the order that the Attributes are in (as a result of LoadAttributes), set SuppressOrderingWithinCategory to False.
        /// For example, if these are Group or GroupMemberAttributes, LoadAttributes will order the attributes based on Inheritance,
        /// so you might want to use that ordering instead of reordering them by EntityTypeQualifier, Order, Name
        /// </summary>
        /// <value>
        ///   <c>true</c> if [suppress ordering]; otherwise, <c>false</c>.
        /// </value>
        public bool SuppressOrderingWithinCategory
        {
            get => ViewState[ViewStateKey.SuppressOrderingWithinCategory] as bool? ?? false;
            set => ViewState[ViewStateKey.SuppressOrderingWithinCategory] = value;
        }

        /// <summary>
        /// Gets or sets a list of Attributes to include when creating Display/Edit controls. Leave null to include all attributes.
        /// </summary>
        /// <value>
        /// The included attributes.
        /// </value>
        public AttributeCache[] IncludedAttributes
        {
            get => ( ViewState[ViewStateKey.IncludedAttributeIds] as int[] )?.Select( a => AttributeCache.Get( a ) ).ToArray();
            set => ViewState[ViewStateKey.IncludedAttributeIds] = value?.Select( a => a.Id ).ToArray();
        }

        /// <summary>
        /// Overrides which Attributes are required. Leave null to use normal IsRequired for the attribute
        /// </summary>
        /// <value>
        /// The required attributes.
        /// </value>
        public AttributeCache[] RequiredAttributes
        {
            get => ( ViewState[ViewStateKey.RequiredAttributeIds] as int[] )?.Select( a => AttributeCache.Get( a ) ).ToArray();
            set => ViewState[ViewStateKey.RequiredAttributeIds] = value?.Select( a => a.Id ).ToArray();
        }

        /// <summary>
        /// Gets or sets a list of Attributes to exclude when creating Display/Edit controls
        /// </summary>
        /// <value>
        /// The excluded attributes.
        /// </value>
        public AttributeCache[] ExcludedAttributes
        {
            get => ( ViewState[ViewStateKey.ExcludedAttributeIds] as int[] )?.Select( a => AttributeCache.Get( a ) ).ToArray() ?? new AttributeCache[0];
            set => ViewState[ViewStateKey.ExcludedAttributeIds] = value?.Select( a => a.Id ).ToArray();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the category header/label should be displayed (defaults to true)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show category label]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCategoryLabel
        {
            get => ViewState[ViewStateKey.ShowCategoryLabel] as bool? ?? true;
            set => ViewState[ViewStateKey.ShowCategoryLabel] = value;
        }

        /// <summary>
        /// Set this to limit attributes by category name (case-insensitive). Leave null to not limit by category.
        /// </summary>
        /// <value>
        /// The included categories.
        /// </value>
        public string[] IncludedCategoryNames
        {
            get => ViewState[ViewStateKey.IncludedCategoryNames] as string[];
            set => ViewState[ViewStateKey.IncludedCategoryNames] = value;
        }

        /// <summary>
        /// Set this to exclude attributes by category name (case-insensitive).
        /// </summary>
        /// <value>
        /// The excluded category names.
        /// </value>
        public string[] ExcludedCategoryNames
        {
            get => ViewState[ViewStateKey.ExcludedCategoryNames] as string[];
            set => ViewState[ViewStateKey.ExcludedCategoryNames] = value;
        }

        /// <summary>
        /// Gets or sets the number of columns to put in each bootstrap row. If this is left NULL (the default), controls will be added without generating bootstrap row/cols.
        /// </summary>
        /// <value>
        /// The number of columns.
        /// </value>
        public int? NumberOfColumns
        {
            get => ViewState[ViewStateKey.NumberOfColumns] as int?;
            set => ViewState[ViewStateKey.NumberOfColumns] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to only create Edit/Display controls for Attributes that have 'Show In Grid' set to true
        /// </summary>
        /// <value>
        ///   <c>true</c> if [limit to show in grid attributes]; otherwise, <c>false</c>.
        /// </value>
        public bool LimitToShowInGridAttributes
        {
            get => ViewState[ViewStateKey.LimitToShowInGridAttributes] as bool? ?? false;
            set => ViewState[ViewStateKey.LimitToShowInGridAttributes] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the category header/label should be displayed as tabs
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display as tabs]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayAsTabs
        {
            get => ViewState[ViewStateKey.DisplayAsTabs] as bool? ?? false;
            set => ViewState[ViewStateKey.DisplayAsTabs] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show pre/post HTML] (if EntityType supports it)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show pre/post HTML]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPrePostHtml
        {
            get => ViewState[ViewStateKey.ShowPrePostHtml] as bool? ?? true;
            set => ViewState[ViewStateKey.ShowPrePostHtml] = value;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _phAttributes = new DynamicPlaceholder();
            _phAttributes.ID = this.ID + "_phAttributes";
            Controls.Add( _phAttributes );
        }

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            this._entityId = ViewState[ViewStateKey.EntityId] as int?;
            this._entityTypeId = ViewState[ViewStateKey.EntityTypeId] as int?;
            this._displayModeAttributeIdValuesState = ( ViewState[ViewStateKey.DisplayModeAttributeIdValuesState] as string ).FromJsonOrNull<Dictionary<int, string>>();
            this._editModeAttributeIdsState = ( ViewState[ViewStateKey.EditModeAttributeIdsState] as string ).FromJsonOrNull<List<int>>();

            if ( _entityId.HasValue && _entityTypeId.HasValue )
            {
                if ( _editModeAttributeIdsState?.Any() == true )
                {
                    EntityTypeCache entityTypeCache = EntityTypeCache.Get( _entityTypeId.Value );
                    var tempHasAttributes = Activator.CreateInstance( entityTypeCache.GetEntityType() ) as IHasAttributes;
                    if ( tempHasAttributes != null )
                    {
                        tempHasAttributes.Attributes = this._editModeAttributeIdsState.Select( a => AttributeCache.Get( a ) ).ToDictionary( k => k.Key, v => v );
                    }

                    this.AddEditControls( tempHasAttributes, false );
                }
                else if ( _displayModeAttributeIdValuesState?.Any() == true )
                {
                    EntityTypeCache entityTypeCache = EntityTypeCache.Get( _entityTypeId.Value );
                    var tempHasAttributes = Activator.CreateInstance( entityTypeCache.GetEntityType() ) as IHasAttributes;
                    if ( tempHasAttributes != null )
                    {
                        tempHasAttributes.Attributes = this._displayModeAttributeIdValuesState.Select( a => AttributeCache.Get( a.Key ) ).ToDictionary( k => k.Key, v => v );
                        tempHasAttributes.AttributeValues = this._displayModeAttributeIdValuesState.Select( a => new { AttributeCache = AttributeCache.Get( a.Key ), Value = a.Value } ).ToDictionary(
                            k => k.AttributeCache.Key,
                            v => new AttributeValueCache { EntityId = _entityId, AttributeId = v.AttributeCache.Id, Value = v.Value } );
                    }

                    this.AddDisplayControls( tempHasAttributes );
                }
            }
        }

        /// <summary>
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.EntityId] = this._entityId;
            ViewState[ViewStateKey.EntityTypeId] = this._entityTypeId;
            ViewState[ViewStateKey.DisplayModeAttributeIdValuesState] = this._displayModeAttributeIdValuesState.ToJson();
            ViewState[ViewStateKey.EditModeAttributeIdsState] = this._editModeAttributeIdsState.ToJson();

            return base.SaveViewState();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the entity.
        /// </summary>
        /// <param name="item">The item.</param>
        private void SetEntity( Attribute.IHasAttributes item )
        {
            this._entityId = item.Id;

            Type entityType = item.GetType();
            if ( entityType.IsDynamicProxyType() )
            {
                entityType = entityType.BaseType;
            }

            this._entityTypeId = EntityTypeCache.GetId( entityType );
        }

        #endregion Private Methods

        #region Methods

        /// <summary>
        /// Adds the edit controls (and set the edit values) while honoring security for the given Rock.Security.Authorization action and person.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="action">The <see cref="Rock.Security.Authorization"/> action to use when checking attribute security.</param>
        /// <param name="person">The person to check authorization against.</param>
        public void AddEditControls( Rock.Attribute.IHasAttributes item, string action, Rock.Model.Person person )
        {
            if ( item.Attributes == null )
            {
                item.LoadAttributes();
            }

            ExcludedAttributes = item.Attributes.Where( a => !a.Value.IsAuthorized( action, person ) ).Select( a => a.Value ).ToArray();

            this.AddEditControls( item, true );
        }

        /// <summary>
        /// Adds the edit controls (and set the edit values)
        /// </summary>
        /// <param name="item">The item.</param>
        public void AddEditControls( Rock.Attribute.IHasAttributes item )
        {
            this.AddEditControls( item, true );
        }

        /// <summary>
        /// Determines whether all the Edit Controls for the specified item have already been added
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if [has edit controls] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasEditControls( Rock.Attribute.IHasAttributes item )
        {
            EnsureChildControls();
            if ( item.Attributes == null )
            {
                item.LoadAttributes();
            }

            var createdEditControls = Rock.Attribute.Helper.GetAttributeEditControls( _phAttributes, item );
            return ( this._editModeAttributeIdsState?.All( a => createdEditControls.Any( c => c.Key.Id == a ) ) ) ?? false;
        }

        /// <summary>
        /// Adds edit controls for each of the item's attributes, with an option to set the edit control values.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        public void AddEditControls( Rock.Attribute.IHasAttributes item, bool setValue )
        {
            EnsureChildControls();
            _phAttributes.Controls.Clear();

            if ( item == null )
            {
                return;
            }

            this.SetEntity( item );

            if ( item.Attributes == null )
            {
                item.LoadAttributes();
            }

            var excludedAttributeGuids = this.ExcludedAttributes.Select( a => a.Guid ).ToList();
            _editModeAttributeIdsState = new List<int>();
            if ( item != null && item.Attributes != null )
            {
                var categoryAttributes = GetDistinctAttributesByCategory( item );

                foreach ( var attributeCategory in categoryAttributes.OrderBy( a => a.Category == null ? 0 : a.Category.Order ) )
                {
                    var attributes = attributeCategory.Attributes;

                    if ( attributes.Any() )
                    {
                        var attributeKeys = attributes.Select( a => a.Key ).ToList();

                        // keep track of which attributes we created edit controls for, so we can re-create them on postback
                        _editModeAttributeIdsState.AddRange( attributes.Select( a => a.Id ) );

                        AttributeAddEditControlsOptions options = new AttributeAddEditControlsOptions
                        {
                            NumberOfColumns = this.NumberOfColumns,
                            IncludedAttributes = attributes.ToList(),
                            RequiredAttributes = this.RequiredAttributes?.ToList(),
                            ShowCategoryLabel = ShowCategoryLabel,
                            ShowPrePostHtml = this.ShowPrePostHtml
                        };

                        Rock.Attribute.Helper.AddEditControlsForCategory(
                            attributeCategory.CategoryName,
                            item,
                            _phAttributes,
                            this.ValidationGroup,
                            setValue,
                            options );
                    }
                }
            }
        }

        /// <summary>
        /// Update the Attribute Values for the item from the Attribute Value Editors associated with this item
        /// </summary>
        /// <param name="item">The item.</param>
        public void GetEditValues( Rock.Attribute.IHasAttributes item )
        {
            if ( item == null )
            {
                return;
            }

            if ( item.Attributes == null )
            {
                item.LoadAttributes();
            }

            EnsureChildControls();

            Rock.Attribute.Helper.GetEditValues( _phAttributes, item );
        }

        /// <summary>
        /// Adds the display controls while honoring security for the given Rock.Security.Authorization action and person.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="action">The <see cref="Rock.Security.Authorization"/> action to use when checking attribute security.</param>
        /// <param name="person">The person to check authorization against.</param>
        public void AddDisplayControls( Rock.Attribute.IHasAttributes item, string action, Rock.Model.Person person )
        {
            if ( item.Attributes == null )
            {
                item.LoadAttributes();
            }

            ExcludedAttributes = item.Attributes.Where( a => !a.Value.IsAuthorized( action, person ) ).Select( a => a.Value ).ToArray();

            this.AddDisplayControls( item );
        }

        /// <summary>
        /// Adds the display controls.
        /// </summary>
        /// <param name="item">The item.</param>
        public void AddDisplayControls( Rock.Attribute.IHasAttributes item )
        {
            EnsureChildControls();
            _phAttributes.Controls.Clear();

            if ( item == null )
            {
                return;
            }

            this.SetEntity( item );

            if ( item.Attributes == null )
            {
                item.LoadAttributes();
            }

            _displayModeAttributeIdValuesState = item.AttributeValues.ToDictionary( k => k.Value.AttributeId, v => v.Value.Value );

            var attributeCategories = GetDistinctAttributesByCategory( item );

            // only show heading labels if ShowCategoryLabel and there is at least attribute to show
            bool showCategoryLabel = this.ShowCategoryLabel && attributeCategories.SelectMany( a => a.Attributes ).Any();

            // only show heading labels if ShowCategoryLabel and there is at least one attribute with category name
            bool displayAsTabs = this.DisplayAsTabs & attributeCategories.Where( a => a.CategoryName.IsNotNullOrWhiteSpace() ).SelectMany( a => a.Attributes ).Any();

            var exclude = ( ExcludedAttributes != null && ExcludedAttributes.Count() != 0 ) ? ExcludedAttributes.Select( k => k.Key ).ToList() : null;

            if ( displayAsTabs )
            {
                HtmlGenericControl tabs = new HtmlGenericControl( "ul" );
                tabs.AddCssClass( "nav nav-tabs margin-b-lg" );
                _phAttributes.Controls.Add( tabs );

                HtmlGenericControl tabContent = new HtmlGenericControl( "div" );
                tabContent.AddCssClass( "tab-content" );
                _phAttributes.Controls.Add( tabContent );

                int tabIndex = 0;
                foreach ( var attributeCategory in attributeCategories.OrderBy( a => a.Category == null ? 0 : a.Category.Order ) )
                {
                    string categoryName = "Attributes";
                    string id = "Attributes";
                    if ( attributeCategory.Category != null )
                    {
                        categoryName = attributeCategory.Category.Name.Trim();
                        id = attributeCategory.Category.Id.ToString();
                    }

                    HtmlGenericControl parentControl = new HtmlGenericControl( "div" );
                    parentControl.ID = id;
                    parentControl.AddCssClass( "tab-pane fade in" );
                    tabContent.Controls.Add( parentControl );
                    var tabClientId = parentControl.ClientID;

                    // Add the tabs
                    HtmlGenericControl li = new HtmlGenericControl( "li" );
                    HtmlGenericControl a = new HtmlGenericControl( "a" );
                    a.Attributes.Add( "data-toggle", "tab" );
                    a.Attributes.Add( "href", "#" + tabClientId );

                    a.InnerText = categoryName;
                    li.Controls.Add( a );
                    tabs.Controls.Add( li );

                    if ( tabIndex == 0 )
                    {
                        parentControl.AddCssClass( "active" );
                        li.AddCssClass( "active" );
                    }

                    tabIndex++;

                    Rock.Attribute.Helper.AddDisplayControls( item, new List<AttributeCategory>() { attributeCategory }, parentControl, exclude, false );
                }
            }
            else
            {
                AttributeAddDisplayControlsOptions attributeAddDisplayControlsOptions = new AttributeAddDisplayControlsOptions
                {
                    ExcludedAttributes = this.ExcludedAttributes.ToList(),
                    NumberOfColumns = this.NumberOfColumns,
                    ShowCategoryLabel = showCategoryLabel
                };

                Rock.Attribute.Helper.AddDisplayControls( item, attributeCategories, _phAttributes, attributeAddDisplayControlsOptions );
            }
        }

        /// <summary>
        /// Gets the attributes that ended up getting displayed as a result of AddDisplayControls
        /// </summary>
        /// <returns></returns>
        public List<AttributeCache> GetDisplayedAttributes()
        {
            return Rock.Attribute.Helper.GetDisplayedAttributes( _phAttributes );
        }

        private List<AttributeCategory> GetDistinctAttributesByCategory( IHasAttributes item )
        {
            var attributes = new List<AttributeCategory>();
            var currentAttributes = new HashSet<string>();

            foreach ( var category in GetFilteredAttributeCategories( item ) )
            {
                var categoryAttributes = GetFilteredAttributesForCategory( category );
                var distinctCategoryAttributes = categoryAttributes.Where( a => !currentAttributes.Contains( a.Key ) );
                if ( distinctCategoryAttributes.Any() )
                {
                    category.Attributes = distinctCategoryAttributes.ToList();
                    attributes.Add( category );
                    currentAttributes.UnionWith( categoryAttributes.Select( a => a.Key ) );
                }
            }

            return attributes;
        }

        /// <summary>
        /// Gets the Attributes for the AttributeCategory that should be included based the filters (IncludedAttributes, ExcludedAttributes, etc)
        /// </summary>
        /// <param name="attributeCategory">The attribute category.</param>
        /// <returns></returns>
        private IEnumerable<AttributeCache> GetFilteredAttributesForCategory( AttributeCategory attributeCategory )
        {
            var attributes = attributeCategory.Attributes.Where( a => a.IsActive );
            if ( this.IncludedAttributes != null )
            {
                attributes = attributes.Where( a => this.IncludedAttributes.Any( c => c.Guid == a.Guid ) ).ToList();
            }

            if ( this.ExcludedAttributes != null )
            {
                attributes = attributes.Where( a => !this.ExcludedAttributes.Any( c => c.Guid == a.Guid ) ).ToList();
            }

            return attributes;
        }

        /// <summary>
        /// Gets the AttributeCategories that should be included based the filters (IncludedCategoryNames, ExcludedCategoryNames, etc)
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private List<AttributeCategory> GetFilteredAttributeCategories( IHasAttributes item )
        {
            var attributeCategories = Rock.Attribute.Helper.GetAttributeCategories( item, LimitToShowInGridAttributes, true, this.SuppressOrderingWithinCategory );
            if ( this.IncludedCategoryNames != null )
            {
                attributeCategories = attributeCategories.Where( a => this.IncludedCategoryNames.Any( c => c.Equals( a.CategoryName, StringComparison.OrdinalIgnoreCase ) ) ).ToList();
            }

            if ( this.ExcludedCategoryNames != null )
            {
                attributeCategories = attributeCategories.Where( a => !this.ExcludedCategoryNames.Any( c => c.Equals( a.CategoryName, StringComparison.OrdinalIgnoreCase ) ) ).ToList();
            }

            return attributeCategories;
        }
        #endregion Methods
    }
}