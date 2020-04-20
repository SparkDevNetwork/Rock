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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Blocks.Types.Mobile.Cms;
using Rock.Data;
using Rock.Mobile.JsonFields;
using Rock.Model;
using Rock.Reporting;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

using AttributeKeys = Rock.Blocks.Types.Mobile.Cms.ContentChannelItemList.AttributeKeys;

namespace RockWeb.Blocks.Mobile
{
    /// <summary>
    /// Handles the Basic Settings panel for the ContentChannelItemList block.
    /// </summary>
    /// <seealso cref="System.Web.UI.UserControl" />
    /// <seealso cref="Rock.Web.IRockCustomSettingsUserControl" />
    public partial class ContentChannelListSettings : System.Web.UI.UserControl, IRockCustomSettingsUserControl
    {
        #region Private members

        private readonly string ITEM_TYPE_NAME = "Rock.Model.ContentChannelItem";

        #endregion
        
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var channelId = ViewState["ChannelId"] as int?;

            using ( var rockContext = new RockContext() )
            {
                var channel = new ContentChannelService( rockContext ).Queryable( "ContentChannelType" )
                    .FirstOrDefault( c => c.Id == channelId.Value );
                if ( channel != null )
                {
                    CreateFilterControl( channel, DataViewFilter.FromJson( ViewState["DataViewFilter"].ToString() ), false, rockContext );
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["DataViewFilter"] = ReportingHelper.GetFilterFromControls( phFilters ).ToJson();
            ViewState["ChannelId"] = ddlContentChannel.SelectedValueAsId();

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlContentChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlContentChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadAttributeListForContentChannel();
            ConfigureChannelSpecificSettings();
        }

        #endregion

        #region Filter Events

        /// <summary>
        /// Handles the AddFilterClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_AddFilterClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterField filterField = new FilterField();
            Guid? channelGuid = ddlContentChannel.SelectedValueAsGuid();
            if ( channelGuid.HasValue )
            {
                var contentChannel = ContentChannelCache.Get( channelGuid.Value );
                if ( contentChannel != null )
                {
                    filterField.Entity = new ContentChannelItem
                    {
                        ContentChannelId = contentChannel.Id,
                        ContentChannelTypeId = contentChannel.ContentChannelTypeId
                    };
                }
            }

            filterField.DataViewFilterGuid = Guid.NewGuid();
            groupControl.Controls.Add( filterField );
            filterField.ID = string.Format( "ff_{0}", filterField.DataViewFilterGuid.ToString( "N" ) );

            // Remove the 'Other Data View' Filter as it doesn't really make sense to have it available in this scenario
            filterField.ExcludedFilterTypes = new string[] { typeof( Rock.Reporting.DataFilter.OtherDataViewFilter ).FullName };
            filterField.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            filterField.Expanded = true;

            filterField.DeleteClick += filterControl_DeleteClick;
        }

        /// <summary>
        /// Handles the AddGroupClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_AddGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterGroup childGroupControl = new FilterGroup
            {
                DataViewFilterGuid = Guid.NewGuid(),
                FilteredEntityTypeName = groupControl.FilteredEntityTypeName,
                FilterType = FilterExpressionType.GroupAll
            };
            childGroupControl.ID = string.Format( "fg_{0}", childGroupControl.DataViewFilterGuid.ToString( "N" ) );
            groupControl.Controls.Add( childGroupControl );

            childGroupControl.AddFilterClick += groupControl_AddFilterClick;
            childGroupControl.AddGroupClick += groupControl_AddGroupClick;
            childGroupControl.DeleteGroupClick += groupControl_DeleteGroupClick;
        }

        /// <summary>
        /// Handles the DeleteClick event of the filterControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void filterControl_DeleteClick( object sender, EventArgs e )
        {
            FilterField fieldControl = sender as FilterField;
            fieldControl.Parent.Controls.Remove( fieldControl );
        }

        /// <summary>
        /// Handles the DeleteGroupClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_DeleteGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            groupControl.Parent.Controls.Remove( groupControl );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Setup the form bindings.
        /// </summary>
        private void SetupFormBindings()
        {
            var rockContext = new RockContext();

            var contentChannels = new ContentChannelService( rockContext ).Queryable().AsNoTracking()
                                    .Select( c => new
                                    {
                                        c.Id,
                                        Value = c.Name
                                    } )
                                    .OrderBy( c => c.Value )
                                    .ToList();

            ddlContentChannel.DataSource = contentChannels;
            ddlContentChannel.DataTextField = "Value";
            ddlContentChannel.DataValueField = "Id";
            ddlContentChannel.DataBind();

            var directions = new Dictionary<string, string>
            {
                { "", "" },
                { SortDirection.Ascending.ConvertToInt().ToString(), "Ascending" },
                { SortDirection.Descending.ConvertToInt().ToString(), "Descending" }
            };
            kvlOrder.CustomValues = directions;
            kvlOrder.Required = true;
        }

        /// <summary>
        /// Configures the channel specific settings.
        /// </summary>
        private void ConfigureChannelSpecificSettings()
        {
            int? filterId = hfDataFilterId.Value.AsIntegerOrNull();
            int? channelId = ddlContentChannel.SelectedValueAsId();

            if ( channelId.HasValue )
            {
                var rockContext = new RockContext();
                var channel = new ContentChannelService( rockContext ).Queryable( "ContentChannelType" )
                    .FirstOrDefault( c => c.Id == channelId.Value );

                if ( channel != null )
                {
                    //cblStatus.Visible = channel.RequiresApproval && !channel.ContentChannelType.DisableStatus;

                    var filterService = new DataViewFilterService( rockContext );
                    DataViewFilter filter = null;

                    if ( filterId.HasValue )
                    {
                        filter = filterService.Get( filterId.Value );
                    }

                    if ( filter == null || filter.ExpressionType == FilterExpressionType.Filter )
                    {
                        filter = new DataViewFilter
                        {
                            Guid = new Guid(),
                            ExpressionType = FilterExpressionType.GroupAll
                        };
                    }

                    CreateFilterControl( channel, filter, true, rockContext );

                    //
                    // Setup the available order-by keys.
                    //
                    kvlOrder.CustomKeys = new Dictionary<string, string>
                    {
                        { "", "" },
                        { "Title", "Title" },
                        { "Priority", "Priority" },
                        { "Status", "Status" },
                        { "StartDateTime", "Start" },
                        { "ExpireDateTime", "Expire" },
                        { "Order", "Order" }
                    };

                    //
                    // Add item attributes to the ordery-by keys.
                    //
                    AttributeService attributeService = new AttributeService( rockContext );
                    var itemAttributes = attributeService.GetByEntityTypeId( new ContentChannelItem().TypeId, false ).AsQueryable()
                        .Where( a => (
                                a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                                a.EntityTypeQualifierValue.Equals( channel.ContentChannelTypeId.ToString() )
                            ) || (
                                a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                                a.EntityTypeQualifierValue.Equals( channel.Id.ToString() )
                            ) )
                        .OrderByDescending( a => a.EntityTypeQualifierColumn )
                        .ThenBy( a => a.Order )
                        .ToAttributeCacheList();

                    foreach ( var attribute in itemAttributes )
                    {
                        string attrKey = "Attribute:" + attribute.Key;
                        if ( !kvlOrder.CustomKeys.ContainsKey( attrKey ) )
                        {
                            kvlOrder.CustomKeys.Add( "Attribute:" + attribute.Key, attribute.Name );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( ContentChannel channel, DataViewFilter filter, bool setSelection, RockContext rockContext )
        {
            phFilters.Controls.Clear();
            if ( filter != null )
            {
                CreateFilterControl( phFilters, filter, setSelection, rockContext, channel );
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="contentChannel">The content channel.</param>
        private void CreateFilterControl( Control parentControl, DataViewFilter filter, bool setSelection, RockContext rockContext, ContentChannel contentChannel )
        {
            try
            {
                if ( filter.ExpressionType == FilterExpressionType.Filter )
                {
                    var filterControl = new FilterField
                    {
                        Entity = new ContentChannelItem
                        {
                            ContentChannelId = contentChannel.Id,
                            ContentChannelTypeId = contentChannel.ContentChannelTypeId
                        }
                    };

                    parentControl.Controls.Add( filterControl );
                    filterControl.DataViewFilterGuid = filter.Guid;
                    filterControl.ID = string.Format( "ff_{0}", filterControl.DataViewFilterGuid.ToString( "N" ) );

                    // Remove the 'Other Data View' Filter as it doesn't really make sense to have it available in this scenario
                    filterControl.ExcludedFilterTypes = new string[] { typeof( Rock.Reporting.DataFilter.OtherDataViewFilter ).FullName };
                    filterControl.FilteredEntityTypeName = ITEM_TYPE_NAME;

                    if ( filter.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Get( filter.EntityTypeId.Value, rockContext );
                        if ( entityTypeCache != null )
                        {
                            filterControl.FilterEntityTypeName = entityTypeCache.Name;
                        }
                    }

                    filterControl.Expanded = filter.Expanded;
                    if ( setSelection )
                    {
                        try
                        {
                            filterControl.SetSelection( filter.Selection );
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( new Exception( "Exception setting selection for DataViewFilter: " + filter.Guid, ex ) );
                        }
                    }

                    filterControl.DeleteClick += filterControl_DeleteClick;
                }
                else
                {
                    var groupControl = new FilterGroup();
                    parentControl.Controls.Add( groupControl );
                    groupControl.DataViewFilterGuid = filter.Guid;
                    groupControl.ID = string.Format( "fg_{0}", groupControl.DataViewFilterGuid.ToString( "N" ) );
                    groupControl.FilteredEntityTypeName = ITEM_TYPE_NAME;
                    groupControl.IsDeleteEnabled = parentControl is FilterGroup;
                    if ( setSelection )
                    {
                        groupControl.FilterType = filter.ExpressionType;
                    }

                    groupControl.AddFilterClick += groupControl_AddFilterClick;
                    groupControl.AddGroupClick += groupControl_AddGroupClick;
                    groupControl.DeleteGroupClick += groupControl_DeleteGroupClick;
                    foreach ( var childFilter in filter.ChildFilters )
                    {
                        CreateFilterControl( groupControl, childFilter, setSelection, rockContext, contentChannel );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( "Exception creating FilterControl for DataViewFilter: " + filter.Guid, ex ) );
            }
        }

        /// <summary>
        /// Loads the attribute list for content channel.
        /// </summary>
        private void LoadAttributeListForContentChannel()
        {
            int contentChannelId = ddlContentChannel.SelectedValue.AsInteger();
            var contentChannelEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM ).Id;

            var attributes = new List<AttributeCache>();
            if ( contentChannelId != 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var channel = new ContentChannelService( rockContext ).Get( contentChannelId );

                    attributes = AttributeCache.All()
                        .Where( a => a.EntityTypeId == contentChannelEntityTypeId )
                        .Where( a => ( a.EntityTypeQualifierColumn == "ContentChannelId" && a.EntityTypeQualifierValue == channel.Id.ToString() )
                            || ( a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == channel.ContentChannelTypeId.ToString() ) )
                        .ToList();
                }
            }

            jfBuilder.AvailableAttributes = attributes;
        }

        /// <summary>
        /// Saves the data view filter and removes the old one.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// A reference to the new DataViewFilter.
        /// </returns>
        private DataViewFilter SaveDataViewFilter( RockContext rockContext )
        {
            var dataViewFilter = ReportingHelper.GetFilterFromControls( phFilters );

            // update Guids since we are creating a new dataFilter and children and deleting the old one
            SetNewDataFilterGuids( dataViewFilter );

            DataViewFilterService dataViewFilterService = new DataViewFilterService( rockContext );

            int? dataViewFilterId = hfDataFilterId.Value.AsIntegerOrNull();
            if ( dataViewFilterId.HasValue )
            {
                var oldDataViewFilter = dataViewFilterService.Get( dataViewFilterId.Value );
                DeleteDataViewFilter( oldDataViewFilter, dataViewFilterService );
            }

            dataViewFilterService.Add( dataViewFilter );

            rockContext.SaveChanges();

            return dataViewFilter;
        }

        /// <summary>
        /// Recursively create new Guid values for each DataViewFilter and child filter.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        private void SetNewDataFilterGuids( DataViewFilter dataViewFilter )
        {
            if ( dataViewFilter != null )
            {
                dataViewFilter.Guid = Guid.NewGuid();
                foreach ( var childFilter in dataViewFilter.ChildFilters )
                {
                    SetNewDataFilterGuids( childFilter );
                }
            }
        }

        /// <summary>
        /// Deletes the data view filter and all child filters.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        /// <param name="service">The service.</param>
        private void DeleteDataViewFilter( DataViewFilter dataViewFilter, DataViewFilterService service )
        {
            if ( dataViewFilter != null )
            {
                foreach ( var childFilter in dataViewFilter.ChildFilters.ToList() )
                {
                    DeleteDataViewFilter( childFilter, service );
                }

                service.Delete( dataViewFilter );
            }
        }

        #endregion

        #region IRockCustomSettingsUserControl implementation

        /// <summary>
        /// Update the custom UI to reflect the current settings found in the entity.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        public void ReadSettingsFromEntity( IHasAttributes attributeEntity )
        {
            SetupFormBindings();

            var fieldSettings = attributeEntity.GetAttributeValue( AttributeKeys.FieldSettings );
            if ( fieldSettings.IsNotNullOrWhiteSpace() )
            {
                jfBuilder.FieldSettings = JsonConvert.DeserializeObject<List<FieldSetting>>( fieldSettings );
            }
            jfBuilder.SourceType = typeof( Rock.Model.ContentChannelItem );

            ddlContentChannel.SelectedValue = attributeEntity.GetAttributeValue( AttributeKeys.ContentChannel );
            nbPageSize.Text = attributeEntity.GetAttributeValue( AttributeKeys.PageSize );
            cbIncludeFollowing.Checked = attributeEntity.GetAttributeValue( AttributeKeys.IncludeFollowing ).AsBoolean();
            cbCheckItemSecurity.Checked = attributeEntity.GetAttributeValue( AttributeKeys.CheckItemSecurity ).AsBoolean();
            cbShowChildrenOfParent.Checked = attributeEntity.GetAttributeValue( AttributeKeys.ShowChildrenOfParent ).AsBoolean();
            var pageCache = PageCache.Get( attributeEntity.GetAttributeValue( AttributeKeys.DetailPage ).AsGuid() );
            ppDetailPage.SetValue( pageCache != null ? ( int? ) pageCache.Id : null );
            hfDataFilterId.Value = attributeEntity.GetAttributeValue( AttributeKeys.FilterId );
            cbQueryParamFiltering.Checked = attributeEntity.GetAttributeValue( AttributeKeys.QueryParameterFiltering ).AsBoolean();
            kvlOrder.Value = attributeEntity.GetAttributeValue( AttributeKeys.Order );

            LoadAttributeListForContentChannel();

            ConfigureChannelSpecificSettings();
        }

        /// <summary>
        /// Update the entity with values from the custom UI.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <remarks>
        /// Do not save the entity, it will be automatically saved later. This call will be made inside
        /// a SQL transaction for the passed rockContext. If you need to make changes to the database
        /// do so on this context so they can be rolled back if something fails during the final save.
        /// </remarks>
        public void WriteSettingsToEntity( IHasAttributes attributeEntity, RockContext rockContext )
        {
            attributeEntity.SetAttributeValue( AttributeKeys.FieldSettings, jfBuilder.FieldSettings.ToJson() );
            attributeEntity.SetAttributeValue( AttributeKeys.ContentChannel, ddlContentChannel.SelectedValue );
            attributeEntity.SetAttributeValue( AttributeKeys.PageSize, nbPageSize.Text );
            attributeEntity.SetAttributeValue( AttributeKeys.IncludeFollowing, cbIncludeFollowing.Checked.ToString() );
            attributeEntity.SetAttributeValue( AttributeKeys.QueryParameterFiltering, cbQueryParamFiltering.Checked.ToString() );
            attributeEntity.SetAttributeValue( AttributeKeys.ShowChildrenOfParent, cbShowChildrenOfParent.Checked.ToString() );
            attributeEntity.SetAttributeValue( AttributeKeys.CheckItemSecurity, cbCheckItemSecurity.Checked.ToString() );
            attributeEntity.SetAttributeValue( AttributeKeys.Order, kvlOrder.Value );

            string detailPage = string.Empty;
            if ( ppDetailPage.SelectedValueAsId().HasValue )
            {
                detailPage = PageCache.Get( ppDetailPage.SelectedValueAsId().Value ).Guid.ToString();
            }
            attributeEntity.SetAttributeValue( AttributeKeys.DetailPage, detailPage );

            attributeEntity.SetAttributeValue( AttributeKeys.FilterId, SaveDataViewFilter( rockContext ).Id.ToString() );
        }

        #endregion
    }
}
