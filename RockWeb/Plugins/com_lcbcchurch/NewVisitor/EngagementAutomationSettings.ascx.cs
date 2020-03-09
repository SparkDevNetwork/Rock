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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Utility.Settings.DataAutomation;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using com.lcbcchurch.NewVisitor.Settings;
using com.lcbcchurch.NewVisitor.SystemKey;
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_lcbcchurch.NewVisitor
{
    /// <summary>
    /// Engagement Automation Settings
    /// </summary>
    [DisplayName( "Engagement Automation Settings" )]
    [Category( "LCBC > New Visitor" )]
    [Description( "Block used to set values specific to engagement automation." )]
    public partial class EngagementAutomationSettings : RockBlock
    {
        #region View State Keys

        private const string _ViewStateKeyEngagementAutomationSetting = "EngagementAutomationSetting";

        #endregion View State Keys

        #region private variables

        private RockContext _rockContext = new RockContext();

        private EngagementAutomationSetting _engagementAutomationSetting = new EngagementAutomationSetting();

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _engagementAutomationSetting = ViewState[_ViewStateKeyEngagementAutomationSetting] as EngagementAutomationSetting;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gScoringItems.DataKeyNames = new string[] { "Guid" };
            gScoringItems.Actions.ShowAdd = true;
            gScoringItems.Actions.AddClick += gScoringItems_Add;

            mdScoringItem.SaveClick += mdScoringItem_SaveClick;
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindControls();
                GetSettings();
            }
            else
            {
                AddAttributeControl( false );
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
            ViewState[_ViewStateKeyEngagementAutomationSetting] = _engagementAutomationSetting;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the SystemConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlScoringType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlScoringType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedValue = ddlScoringType.SelectedValueAsEnum<ScoringItemType>();
            BindControlOnScoringItem( selectedValue, null, string.Empty );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlAttribute_SelectedIndexChanged( object sender, EventArgs e )
        {
            var scoringType = ddlScoringType.SelectedValueAsEnum<ScoringItemType>();
            if ( scoringType == ScoringItemType.MemberOfGroupWithGroupTypeHavingAnAttribute )
            {
                AddAttributeControl( true, string.Empty );
            }
        }

        /// <summary>
        /// Handles the Add event of the gScoringItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gScoringItems_Add( object sender, EventArgs e )
        {
            ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gScoringItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gScoringItems_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( Guid ) e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gScoringItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gScoringItems_Delete( object sender, RowEventArgs e )
        {
            var scoringItem = _engagementAutomationSetting.ScoringItems.FirstOrDefault( a => a.Guid == ( Guid ) e.RowKeyValue );
            if ( scoringItem != null )
            {
                _engagementAutomationSetting.ScoringItems.Remove( scoringItem );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles saving all the data by btnSaveConfig click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSaveConfig_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }
            _engagementAutomationSetting.BeginDateAttributeGuid = ddlEngagementBeginAttribute.SelectedValueAsGuid();
            _engagementAutomationSetting.WeeksInEngagementWindow = nbEEngagementWeeks.Text.AsInteger();
            _engagementAutomationSetting.ScoreAttributeGuid = ddlEngagementScoreAttribute.SelectedValueAsGuid();

            Rock.Web.SystemSettings.SetValue( SystemSetting.LCBC_ENGAGEMENTSCORING_CONFIGURATION, _engagementAutomationSetting.ToJson() );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdScoringItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdScoringItem_SaveClick( object sender, EventArgs e )
        {
            ScoringItem scoringItem = _engagementAutomationSetting.ScoringItems.SingleOrDefault( a => a.Guid == hfScoringItemGuid.Value.AsGuid() );

            if ( _engagementAutomationSetting.ScoringItems.Any( a => a.Guid != hfScoringItemGuid.Value.AsGuid() && a.Name.IsNotNullOrWhiteSpace() && a.Name.Equals( tbName.Text, StringComparison.OrdinalIgnoreCase ) ) )
            {
                nbError.Text = "Name already exists for different scoring item.";
                nbError.Visible = true;
                return;
            }

            var type = ddlScoringType.SelectedValueAsEnum<ScoringItemType>();
            string attributeValue = string.Empty;

            if ( type == ScoringItemType.MemberOfGroupWithGroupTypeHavingAnAttribute )
            {
                attributeValue = GetAttributeValue( phAttributes1 );
                if ( attributeValue.IsNullOrWhiteSpace() )
                {
                    nbError.Text = "With Value field is required.";
                    nbError.Visible = true;
                    return;
                }
            }

            if ( scoringItem == null )
            {
                scoringItem = new ScoringItem();
                scoringItem.Guid = Guid.NewGuid();
                _engagementAutomationSetting.ScoringItems.Add( scoringItem );
            }

            scoringItem.Type = ddlScoringType.SelectedValueAsEnum<ScoringItemType>();
            scoringItem.Score = numPoints.Value;
            scoringItem.Name = tbName.Text;
            scoringItem.IconCssClass = tbIconCssClass.Text;

            if ( scoringItem.Type == ScoringItemType.PersonAttribute )
            {
                scoringItem.EntityItemsGuid = new List<Guid>() { ddlAttribute.SelectedValue.AsGuid() };
            }
            else if ( scoringItem.Type == ScoringItemType.MemberOfGroupWithGroupTypeHavingAnAttribute )
            {
                scoringItem.EntityItemsGuid = new List<Guid>() { ddlAttribute.SelectedValue.AsGuid() };
                scoringItem.EntityItemQualifierValue = attributeValue;
            }
            else if ( scoringItem.Type == ScoringItemType.GivenToAnAccount )
            {
                var accountsId = apAccount.SelectedValuesAsInt().ToList();
                var accounts = new FinancialAccountService( _rockContext ).GetListByIds( accountsId );
                if ( accounts != null )
                {
                    scoringItem.EntityItemsGuid = accounts.Select( a => a.Guid ).ToList();
                }
            }
            else if ( scoringItem.Type == ScoringItemType.InDataView )
            {
                var dataViewId = dvpDataView.SelectedValue.AsInteger();
                var dataView = new DataViewService( _rockContext ).Get( dataViewId );
                scoringItem.EntityItemsGuid = new List<Guid>() { dataView.Guid };
            }
            else
            {
                scoringItem.EntityItemsGuid = new List<Guid>() { gtpGroupType.SelectedGroupTypeGuid.Value };
            }

            BindGrid();

            mdScoringItem.Hide();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the adult attribute values.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        private string GetAttributeValue( Control parentControl )
        {
            string value = string.Empty;
            var attributeGuid = ddlAttribute.SelectedValueAsGuid();
            var attribute = AttributeCache.Get( attributeGuid.Value );
            var filterControl = phAttributes1.FindControl( "filter_" + attribute.Id.ToString() );
            if ( filterControl != null )
            {
                try
                {
                    value = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson();
                }
                catch
                {
                    // intentionally ignore
                }
            }
            return value;
        }

        /// <summary>
        /// Binds the controls.
        /// </summary>
        private void BindControls()
        {
            var personAttributesQry = new AttributeService( _rockContext )
                .GetByEntityTypeId( new Person().TypeId, false );

            var dateFieldTypeId = FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.DATE ) ).Id;
            var datePersonAttributes = personAttributesQry
                .Where( a => a.FieldTypeId == dateFieldTypeId )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name )
                .Select( t => new { value = t.Guid, text = t.Name } )
                .ToList();
            ddlEngagementBeginAttribute.DataSource = datePersonAttributes;
            ddlEngagementBeginAttribute.DataTextField = "text";
            ddlEngagementBeginAttribute.DataValueField = "value";
            ddlEngagementBeginAttribute.DataBind();
            ddlEngagementBeginAttribute.Items.Insert( 0, new ListItem() );


            var integerFieldTypeId = FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.INTEGER ) ).Id;
            var integerPersonAttributes = personAttributesQry
               .Where( a => a.FieldTypeId == integerFieldTypeId )
               .OrderBy( t => t.Order )
               .ThenBy( t => t.Name )
               .Select( t => new { value = t.Guid, text = t.Name } )
               .ToList();
            ddlEngagementScoreAttribute.DataSource = integerPersonAttributes;
            ddlEngagementScoreAttribute.DataTextField = "text";
            ddlEngagementScoreAttribute.DataValueField = "value";
            ddlEngagementScoreAttribute.DataBind();
            ddlEngagementScoreAttribute.Items.Insert( 0, new ListItem() );

            gtpGroupType.GroupTypes = new GroupTypeService( _rockContext )
                .Queryable().AsNoTracking()
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name )
                .ToList();

            ddlScoringType.BindToEnum<ScoringItemType>( true );
        }

        private void BindControlOnScoringItem( ScoringItemType selectedValue, List<Guid> entityTypeItemsId, string entityQualifierValue )
        {
            phAttributes1.Controls.Clear();
            ddlAttribute.Visible = false;
            gtpGroupType.Visible = false;
            apAccount.Visible = false;
            dvpDataView.Visible = false;
            Guid? entityTypeItemId = null;
            if ( entityTypeItemsId != null && entityTypeItemsId.Any() )
            {
                entityTypeItemId = entityTypeItemsId.FirstOrDefault();
            }

            switch ( selectedValue )
            {
                case ScoringItemType.AttendanceInGroupOfType:
                    {
                        nbScoringMessage.Text = "Determines if the individual attended a group of a specific type during the engagement period. Once points are given, no additional points are gained for this item (the rule no longer needs to be checked).";
                        gtpGroupType.Visible = true;
                        gtpGroupType.SelectedGroupTypeGuid = entityTypeItemId;
                    }
                    break;
                case ScoringItemType.AttendanceInGroupOfTypeCumulative:
                    {
                        nbScoringMessage.Text = "Determines if the individual attended a group of a specific type during the engagement period. If so, they will get points for each attendance.";
                        gtpGroupType.Visible = true;
                        gtpGroupType.SelectedGroupTypeGuid = entityTypeItemId;
                    }
                    break;
                case ScoringItemType.MemberOfGroupType:
                    {
                        nbScoringMessage.Text = "Determines if the individual is an active member of a group of a specific type during the engagement period. Once points are given, no additional points are gained for this item (the rule no longer needs to be checked).";
                        gtpGroupType.Visible = true;
                        gtpGroupType.SelectedGroupTypeGuid = entityTypeItemId;
                    }
                    break;
                case ScoringItemType.PersonAttribute:
                    {
                        nbScoringMessage.Text = "The individual will get the points defined below if the person has an attribute value record whose ‘created date’ is within the engagement period. Once points are given, no additional points are gained for this item (the rule no longer needs to be checked.)";
                        ddlAttribute.Visible = true;
                        var personAttributes = new AttributeService( _rockContext )
                            .GetByEntityTypeId( new Person().TypeId, false )
                            .OrderBy( t => t.Order )
                            .ThenBy( t => t.Name )
                            .Select( t => new { value = t.Guid, text = t.Name } )
                            .ToList();
                        ddlAttribute.DataSource = personAttributes;
                        ddlAttribute.DataBind();
                        ddlAttribute.SetValue( entityTypeItemId );
                    }
                    break;
                case ScoringItemType.GivenToAnAccount:
                    {
                        nbScoringMessage.Text = "The individual will get the points defined below if they (or anyone in their ‘giving group’) have given to a specific financial account (or child of the selected financial account). Once points are given, no additional points are gained for this item (the rule no longer needs to be checked).";
                        apAccount.Visible = true;
                        if ( entityTypeItemsId != null && entityTypeItemsId.Any() )
                        {
                            var accounts = new FinancialAccountService( _rockContext ).GetListByGuids( entityTypeItemsId );
                            apAccount.SetValues( accounts );
                        }
                    }
                    break;
                case ScoringItemType.InDataView:
                    {
                        nbScoringMessage.Text = "Determines if the individual is in the configured Data View. Once points are given, no additional points are gained for this item (the rule no longer needs to be checked).";
                        dvpDataView.Visible = true;
                        dvpDataView.EntityTypeId = new Person().TypeId;
                        if ( entityTypeItemsId != null && entityTypeItemsId.Any() )
                        {
                            var dataView = new DataViewService( _rockContext ).Get( entityTypeItemsId.First() );
                            dvpDataView.SetValue( dataView );
                        }
                    }
                    break;
                case ScoringItemType.MemberOfGroupWithGroupTypeHavingAnAttribute:
                    {
                        nbScoringMessage.Text = "The individual will get the points defined below if the person is found as an active member of the group whose type has an attribute with the selected value below. Once points are given, no additional points are gained for this item.";
                        ddlAttribute.Visible = true;


                        var qryAttributes = new AttributeService( _rockContext )
                            .GetByEntityTypeId( new GroupType().TypeId, false );

                        qryAttributes = qryAttributes.Where( a => string.IsNullOrEmpty( a.EntityTypeQualifierColumn ) && string.IsNullOrEmpty( a.EntityTypeQualifierValue ) );

                        var cacheAttributeList = qryAttributes.ToAttributeCacheList();

                        var listItems = new List<ListItem>();
                        foreach ( var attributeCache in cacheAttributeList )
                        {
                            // Make sure that the attributes field type actually renders a filter control if limitToFilterableAttributes
                            var fieldType = FieldTypeCache.Get( attributeCache.FieldTypeId );
                            if ( fieldType != null && fieldType.Field.HasFilterControl() )
                            {
                                listItems.Add( new ListItem() { Text = attributeCache.Name, Value = attributeCache.Guid.ToString() } );
                            }
                        }

                        ddlAttribute.DataSource = listItems;
                        ddlAttribute.DataBind();
                        ddlAttribute.SetValue( entityTypeItemId );

                        AddAttributeControl( true, entityQualifierValue );
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Create the control.
        /// </summary>
        private void AddAttributeControl( bool setValues, string value = "" )
        {
            phAttributes1.Controls.Clear();
            var scoringType = ddlScoringType.SelectedValueAsEnumOrNull<ScoringItemType>();
            var attributeGuid = ddlAttribute.SelectedValueAsGuid();
            if ( attributeGuid.HasValue && scoringType.HasValue && scoringType == ScoringItemType.MemberOfGroupWithGroupTypeHavingAnAttribute )
            {
                var attribute = AttributeCache.Get( attributeGuid.Value );
                var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), true, Rock.Reporting.FilterMode.SimpleFilter );
                if ( control != null )
                {
                    if ( control is IRockControl )
                    {
                        var rockControl = ( IRockControl ) control;
                        rockControl.Label = attribute.Name;
                        rockControl.Help = attribute.Description;
                        rockControl.ValidationGroup = "ScoringItem";
                        phAttributes1.Controls.Add( control );
                    }
                    else
                    {
                        var wrapper = new RockControlWrapper();
                        wrapper.ID = control.ID + "_wrapper";
                        wrapper.Label = attribute.Name;
                        wrapper.ValidationGroup = "ScoringItem";
                        wrapper.Controls.Add( control );
                        phAttributes1.Controls.Add( wrapper );
                    }

                    if ( !string.IsNullOrWhiteSpace( value ) )
                    {
                        try
                        {
                            var values = JsonConvert.DeserializeObject<List<string>>( value );
                            attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                        }
                        catch
                        {
                            // intentionally ignore
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        private void GetSettings()
        {
            _engagementAutomationSetting = Rock.Web.SystemSettings.GetValue( SystemSetting.LCBC_ENGAGEMENTSCORING_CONFIGURATION ).FromJsonOrNull<EngagementAutomationSetting>() ?? new EngagementAutomationSetting();

            ddlEngagementBeginAttribute.SetValue( _engagementAutomationSetting.BeginDateAttributeGuid );
            ddlEngagementScoreAttribute.SetValue( _engagementAutomationSetting.ScoreAttributeGuid );
            nbEEngagementWeeks.Text = _engagementAutomationSetting.WeeksInEngagementWindow.ToString();

            BindGrid();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="scoringItemGuid">The scoring item id.</param>
        protected void ShowEdit( Guid scoringItemGuid )
        {
            var scoringItem = _engagementAutomationSetting.ScoringItems.FirstOrDefault( a => a.Guid == scoringItemGuid );
            List<Guid> entityTypeItemsGuid = null;
            if ( scoringItem == null )
            {
                mdScoringItem.Title = "Add Engagement Scoring Item".FormatAsHtmlTitle();
                scoringItem = new ScoringItem();
                hfScoringItemGuid.Value = string.Empty;
            }
            else
            {
                mdScoringItem.Title = "Edit Engagement Scoring Item".FormatAsHtmlTitle();
                hfScoringItemGuid.Value = scoringItemGuid.ToString();
                entityTypeItemsGuid = scoringItem.EntityItemsGuid;
            }

            ddlScoringType.SetValue( scoringItem.Type.ConvertToInt() );
            BindControlOnScoringItem( scoringItem.Type, entityTypeItemsGuid, scoringItem.EntityItemQualifierValue );
            numPoints.Value = scoringItem.Score;
            tbName.Text = scoringItem.Name;
            tbIconCssClass.Text = scoringItem.IconCssClass;
            nbError.Visible = false;
            mdScoringItem.Show();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            gScoringItems.DataSource = _engagementAutomationSetting
                                  .ScoringItems
                                  .Select( a => new { Guid = a.Guid, Name = a.Name } )
                                  .ToList();
            gScoringItems.DataBind();
        }

        #endregion
    }
}