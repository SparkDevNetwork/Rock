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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Person Group History" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Displays a timeline of a person's history in groups" )]

    [GroupTypesField(
        "Group Types",
        Key = AttributeKey.GroupTypes,
        Description = "List of Group Types that this block defaults to, and the user is able to choose from in the options filter. Leave blank to include all group types that have history enabled.",
        IsRequired = false,
        Order = 0 )]

    [Rock.SystemGuid.BlockTypeGuid( "F8E351BC-607E-4897-B732-F590B5155451" )]
    public partial class PersonGroupHistory : PersonBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string GroupTypes = "GroupTypes";
        }
        #endregion Attribute Keys

        private List<int> _blockSettingsGroupTypeIds = null;

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            ApplyBlockSettings();
        }

        /// <summary>
        /// Applies the block settings.
        /// </summary>
        private void ApplyBlockSettings()
        {
            _blockSettingsGroupTypeIds = this.GetAttributeValue( AttributeKey.GroupTypes ).SplitDelimitedValues().AsGuidList().Select( a => GroupTypeCache.Get( a ) ).Where( a => a != null ).Select( a => a.Id ).ToList();

            IEnumerable<GroupTypeCache> groupTypes = GroupTypeCache.All();

            if ( _blockSettingsGroupTypeIds.Any() )
            {
                groupTypes = groupTypes.Where( a => _blockSettingsGroupTypeIds.Contains( a.Id ) );
            }
            else
            {
                groupTypes = groupTypes.Where( a => a.EnableGroupHistory == true );
            }

            gtGroupTypesFilter.SetGroupTypes( groupTypes );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                // first page load, so set the selected group types from user preferences
                var preferences = GetBlockPersonPreferences();
                var userGroupTypes = preferences.GetValue( AttributeKey.GroupTypes ).SplitDelimitedValues().AsIntegerList();
                gtGroupTypesFilter.SetValues( userGroupTypes );

                int? personId = this.Person != null ? this.Person.Id : ( int? ) null;
                if ( personId.HasValue )
                {
                    ShowDetail( personId.Value );
                }
                else
                {
                    // don't show the block if a GroupId isn't in the URL
                    this.Visible = false;
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ApplyBlockSettings();
            ShowDetail( hfPersonId.Value.AsInteger() );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        public void ShowDetail( int personId )
        {
            var startDateTime = DateTime.SpecifyKind( RockDateTime.Now.AddYears( -10 ),
                DateTimeKind.Unspecified );
            hfStartDateTime.Value = startDateTime.ToString( "o" );
            hfStopDateTime.Value = HistoricalTracking.MaxExpireDateTime.ToString( "o" );
            hfPersonId.Value = personId.ToString();
            List<int> groupTypeIds;
            if ( gtGroupTypesFilter.SelectedGroupTypeIds.Any() )
            {
                // if group types are filtered on the user filter, use that
                groupTypeIds = gtGroupTypesFilter.SelectedGroupTypeIds;
            }
            else
            {
                // if no group types are selected in the user filter, restrict grouptypes to the ones in the block settings (if any)
                groupTypeIds = _blockSettingsGroupTypeIds;
            }

            hfGroupTypeIds.Value = groupTypeIds.AsDelimited( "," );

            var legendGroupTypes = GroupTypeCache.All().Where( a => a.EnableGroupHistory );
            if ( groupTypeIds.Any() )
            {
                legendGroupTypes = legendGroupTypes.Where( a => groupTypeIds.Contains( a.Id ) );
            }

            rptGroupTypeLegend.DataSource = legendGroupTypes.OrderBy( a => a.Name );
            rptGroupTypeLegend.DataBind();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnApplyOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApplyOptions_Click( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( AttributeKey.GroupTypes, gtGroupTypesFilter.SelectedGroupTypeIds.AsDelimited( "," ) );
            preferences.Save();

            ShowDetail( hfPersonId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptGroupTypeLegend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptGroupTypeLegend_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            GroupTypeCache cacheGroupType = e.Item.DataItem as GroupTypeCache;
            Literal lGroupTypeBadgeHtml = e.Item.FindControl( "lGroupTypeBadgeHtml" ) as Literal;
            if ( cacheGroupType != null )
            {
                var style = string.Empty;
                if (!string.IsNullOrEmpty(cacheGroupType.GroupTypeColor))
                {
                    style = "background-color:" + cacheGroupType.GroupTypeColor;
                }

                lGroupTypeBadgeHtml.Text = string.Format( "<span class='label label-default' style='{0}'>{1}</span>", style, cacheGroupType.Name );
            }
        }
    }
}