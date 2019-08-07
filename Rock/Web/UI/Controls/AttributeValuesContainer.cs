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
        #region Controls

        /// <summary>
        /// The ph attributes
        /// </summary>
        private DynamicPlaceholder _phAttributes;

        #endregion Controls

        #region Private fields

        // Stores the Entity associated with the attributes
        private IHasAttributes _entity;

        // Keeps track of EntityTypeId of the Item we created edit/display controls for, so we can re-create them on postback
        private int? _entityTypeId;

        // Keeps track of Entity.Id of the Item we created edit/display controls for, so we can re-create them on postback
        private int? _entityId;

        // Keeps track of which attributes we created edit controls for, so we can re-create them on postback
        private List<int> _editModeAttributeIdsState;

        // Keeps track of which attributes and values we created display controls for, so we can re-create them on postback
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
            get => ViewState["ValidationGroup"] as string ?? this.RockBlock()?.BlockValidationGroup;
            set => ViewState["ValidationGroup"] = value;
        }

        /// <summary>
        /// Attribute Display/Edit controls are sorted by Category (if there is a category). Then, by default, they are sorted by EntityTypeQualifier, Order, and Name.
        /// To keep the order that the Attributes are in (as a result of LoadAttributes), set SuppressOrderingWithinCategory to False.
        /// For example, if these are Group or GroupMemberAttributes, LoadAttributes will order the attributes based on Inheritence,
        /// so you might want to use that ordering instead of reordering them by EntityTypeQualifier, Order, Name
        /// </summary>
        /// <value>
        ///   <c>true</c> if [suppress ordering]; otherwise, <c>false</c>.
        /// </value>
        public bool SuppressOrderingWithinCategory
        {
            get => ViewState["SuppressOrderingWithinCategory"] as bool? ?? false;
            set => ViewState["SuppressOrderingWithinCategory"] = value;
        }

        /// <summary>
        /// Gets or sets a list of Attributes to include when creating Display/Edit controls. Leave null to include all attributes.
        /// </summary>
        /// <value>
        /// The included attributes.
        /// </value>
        public AttributeCache[] IncludedAttributes
        {
            get => ViewState["IncludedAttributes"] as AttributeCache[];
            set => ViewState["IncludedAttributes"] = value;
        }

        /// <summary>
        /// Gets or sets a list of Attributes to exclude when creating Display/Edit controls
        /// </summary>
        /// <value>
        /// The excluded attributes.
        /// </value>
        public AttributeCache[] ExcludedAttributes
        {
            get => ViewState["ExcludedAttributes"] as AttributeCache[] ?? new AttributeCache[0];
            set => ViewState["ExcludedAttributes"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the category header/label should be displayed (defaults to true)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show category label]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCategoryLabel
        {
            get => ViewState["ShowCategoryLabel"] as bool? ?? true;
            set => ViewState["ShowCategoryLabel"] = value;
        }

        /// <summary>
        /// Set this to limit attributes by category name (case-insensitive). Leave null to not limit by category.
        /// </summary>
        /// <value>
        /// The included categories.
        /// </value>
        public string[] IncludedCategoryNames
        {
            get => ViewState["IncludedCategoryNames"] as string[];
            set => ViewState["IncludedCategoryNames"] = value;
        }

        /// <summary>
        /// Set this to exclude attributes by category name (case-insensitive).
        /// </summary>
        /// <value>
        /// The excluded category names.
        /// </value>
        public string[] ExcludedCategoryNames
        {
            get => ViewState["ExcludedCategoryNames"] as string[];
            set => ViewState["ExcludedCategoryNames"] = value;
        }

        /// <summary>
        /// Gets or sets the number of columns to put in each bootstrap row. If this is left NULL (the default), controls will be added without generating bootstrap row/cols.
        /// </summary>
        /// <value>
        /// The number of columns.
        /// </value>
        public int? NumberOfColumns
        {
            get => ViewState["NumberOfColumns"] as int?;
            set => ViewState["NumberOfColumns"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to only create Edit/Display controls for Attributes that have 'Show In Grid' set to true
        /// </summary>
        /// <value>
        ///   <c>true</c> if [limit to show in grid attributes]; otherwise, <c>false</c>.
        /// </value>
        public bool LimitToShowInGridAttributes
        {
            get => ViewState["LimitToShowInGridAttributes"] as bool? ?? false;
            set => ViewState["LimitToShowInGridAttributes"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show pre post HTML] (if EntityType supports it)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show pre post HTML]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPrePostHtml
        {
            get => ViewState["ShowPrePostHtml"] as bool? ?? true;
            set => ViewState["ShowPrePostHtml"] = value;
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

            this._entityId = ViewState["_entityId"] as int?;
            this._entityTypeId = ViewState["_entityTypeId"] as int?;
            this._displayModeAttributeIdValuesState = ( ViewState["_displayModeAttributeIdValuesState"] as string ).FromJsonOrNull<Dictionary<int, string>>();
            this._editModeAttributeIdsState = ( ViewState["_editModeAttributeIdsState"] as string ).FromJsonOrNull<List<int>>();

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
            ViewState["_entityId"] = this._entityId;
            ViewState["_entityTypeId"] = this._entityTypeId;
            ViewState["_displayModeAttributeIdValuesState"] = this._displayModeAttributeIdValuesState.ToJson();
            ViewState["_editModeAttributeIdsState"] = this._editModeAttributeIdsState.ToJson();

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
            this._entity = item;

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
                List<AttributeCategory> attributeCategories = GetFilteredAttributeCategories( item );

                foreach ( var attributeCategory in attributeCategories )
                {
                    IEnumerable<AttributeCache> attributes = GetFilteredAttributesForCategory( attributeCategory );

                    if ( attributes.Any() )
                    {
                        var attributeKeys = attributes.Select( a => a.Key ).ToList();

                        // keep track of which attributes we created edit controls for, so we can re-create them on postback
                        _editModeAttributeIdsState.AddRange( attributes.Select( a => a.Id ) );

                        AttributeAddEditControlsOptions options = new AttributeAddEditControlsOptions
                        {
                            NumberOfColumns = this.NumberOfColumns,
                            IncludedAttributes = attributes.ToList(),
                            ShowCategoryLabel = ShowCategoryLabel,
                            ShowPrePostHtml = this.ShowPrePostHtml
                        };

                        Rock.Attribute.Helper.AddEditControlsForCategory(
                            attributeCategory.CategoryName,
                            item,
                            _phAttributes,
                            this.ValidationGroup,
                            setValue,
                            options
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the filtered attributes based the filters (IncludedCategoryNames, ExcludedCategoryNames, IncludedAttributes, ExcludedAttributes, etc)
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private IEnumerable<AttributeCache> GetFilteredAttributes( IHasAttributes item )
        {
            return GetFilteredAttributeCategories( item ).SelectMany( c => GetFilteredAttributesForCategory( c ) );
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
            var attributeCategories = Rock.Attribute.Helper.GetAttributeCategories( item, LimitToShowInGridAttributes, false, this.SuppressOrderingWithinCategory );
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

            _displayModeAttributeIdValuesState = new Dictionary<int, string>();

            List<AttributeCategory> attributeCategories = GetFilteredAttributeCategories( item );

            foreach ( var attributeCategory in attributeCategories )
            {
                attributeCategory.Attributes = GetFilteredAttributesForCategory( attributeCategory ).ToList();
            }

            // only show heading labels if ShowCategoryLabel and there is at least attribute to show
            bool showHeadingLabels = this.ShowCategoryLabel && attributeCategories.SelectMany( a => a.Attributes ).Any();

            Rock.Attribute.Helper.AddDisplayControls( item, attributeCategories, _phAttributes, null, showHeadingLabels );
        }

        #endregion Methods
    }
}
