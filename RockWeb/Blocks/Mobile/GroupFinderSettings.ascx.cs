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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

using AttributeKey = Rock.Blocks.Types.Mobile.Groups.GroupFinder.AttributeKey;

namespace RockWeb.Blocks.Mobile
{
    /// <summary>
    /// Custom settings UI for the <see cref="Rock.Blocks.Types.Mobile.Groups.GroupFinder"/> block.
    /// </summary>
    /// <seealso cref="System.Web.UI.UserControl" />
    /// <seealso cref="Rock.Web.IRockCustomSettingsUserControl" />
    public partial class GroupFinderSettings : UserControl, IRockCustomSettingsUserControl
    {
        #region Properties

        /// <summary>
        /// Gets or sets the group type locations.
        /// </summary>
        /// <value>
        /// The group type locations.
        /// </value>
        private Dictionary<int, int> GroupTypeLocations { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            GroupTypeLocations = ViewState[nameof( GroupTypeLocations )] as Dictionary<int, int> ?? new Dictionary<int, int>();
        }

        /// <inheritdoc/>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !IsPostBack )
            {
                GroupTypeCache.All()
                    .ForEach( t =>
                    {
                        rlbGroupTypes.Items.Add( new ListItem( t.Name, t.Guid.ToString() ) );
                    } );
            }
        }

        /// <inheritdoc/>
        protected override object SaveViewState()
        {
            ViewState[nameof( GroupTypeLocations )] = GroupTypeLocations;

            return base.SaveViewState();
        }

        /// <inheritdoc/>
        public void ReadSettingsFromEntity( IHasAttributes attributeEntity )
        {
            // Set the value for the Group Type control.
            var groupTypeGuids = attributeEntity.GetAttributeValue( AttributeKey.GroupTypes )
                .SplitDelimitedValues()
                .AsGuidList();

            foreach ( ListItem li in rlbGroupTypes.Items )
            {
                li.Selected = groupTypeGuids.Contains( li.Value.AsGuid() );
            }

            GroupTypeLocations = attributeEntity.GetAttributeValue( AttributeKey.GroupTypesLocationType ).FromJsonOrNull<Dictionary<int, int>>() ?? new Dictionary<int, int>();

            BindGroupTypeLocationGrid();
        }

        /// <inheritdoc/>
        public void WriteSettingsToEntity( IHasAttributes attributeEntity, RockContext rockContext )
        {
            attributeEntity.SetAttributeValue( AttributeKey.GroupTypes, rlbGroupTypes.SelectedValues.AsDelimited( "," ) );
            attributeEntity.SetAttributeValue( AttributeKey.GroupTypesLocationType, GroupTypeLocations.ToJson() );
        }

        /// <summary>
        /// Binds the group type location grid.
        /// </summary>
        private void BindGroupTypeLocationGrid()
        {
            var groupTypeIds = rlbGroupTypes.SelectedValues.AsGuidList() ?? new List<Guid>();
            var source = GroupTypeCache.All()
                .Where( gt => groupTypeIds.Contains( gt.Guid ) )
                .ToList();

            gGroupTypesLocationType.DataSource = source;
            gGroupTypesLocationType.DataBind();

            rcwGroupTypesLocationType.Visible = source.Any();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the SelectedIndexChanged event of the GroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void GroupTypes_SelectedIndexChanged( object sender, System.EventArgs e )
        {
            BindGroupTypeLocationGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the GroupTypesLocationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void GroupTypesLocationType_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var dropDownList = e.Row.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault();
            var groupType = e.Row.DataItem as GroupTypeCache;

            if ( dropDownList == null || groupType == null )
            {
                return;
            }

            dropDownList.Attributes.Add( "group-type-id", groupType.Id.ToString() );
            var locationTypeValues = groupType.LocationTypeValues;
            if ( locationTypeValues != null )
            {
                dropDownList.Items.Add( new ListItem( "All", string.Empty ) );
                foreach ( var locationTypeValue in locationTypeValues )
                {
                    dropDownList.Items.Add( new ListItem( locationTypeValue.Value, locationTypeValue.Id.ToString() ) );
                }

                if ( GroupTypeLocations != null && GroupTypeLocations.ContainsKey( groupType.Id ) )
                {
                    dropDownList.SelectedValue = GroupTypeLocations[groupType.Id].ToString();
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the LocationList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void LocationList_SelectedIndexChanged( object sender, EventArgs e )
        {
            var dropDownList = sender as RockDropDownList;
            if ( dropDownList == null )
            {
                return;
            }

            var groupTypeId = dropDownList.Attributes["group-type-id"].AsIntegerOrNull();
            if ( groupTypeId == null )
            {
                return;
            }

            var groupTypeLocations = GroupTypeLocations;

            if ( groupTypeLocations == null )
            {
                groupTypeLocations = new Dictionary<int, int>();
            }

            var groupTypeLocationId = dropDownList.SelectedValue.AsIntegerOrNull();

            if ( groupTypeLocationId == null )
            {
                groupTypeLocations.Remove( groupTypeId.Value );
            }
            else
            {
                groupTypeLocations.AddOrReplace( groupTypeId.Value, groupTypeLocationId.Value );
            }

            GroupTypeLocations = groupTypeLocations;
        }

        #endregion
    }
}
