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

using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Data;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Types.Mobile.Groups
{
    public partial class GroupFinder : RockMobileBlockType
    {
        /// <summary>
        /// Defines the control that will provide the Basic Settings tab content
        /// for the ContentChannelItemList block.
        /// </summary>
        /// <seealso cref="Rock.Web.RockCustomSettingsUserControlProvider" />
        [TargetType( typeof( GroupFinder ) )]
        public class GroupFinderCustomSettingsProvider : RockCustomSettingsProvider
        {
            #region Properties

            /// <inheritdoc/>
            public override string CustomSettingsTitle => "Basic Settings";

            #endregion

            #region Fields

            /// <summary>
            /// The attribute filters control.
            /// </summary>
            private RockCheckBoxList _cblAttributeFilters;

            /// <summary>
            /// The group types control.
            /// </summary>
            private RockListBox _rlbGroupTypes;

            #endregion

            #region Methods

            /// <inheritdoc/>
            public override Control GetCustomSettingsControl( IHasAttributes attributeEntity, Control parent )
            {
                var panel = new Panel();

                // Construct the enhanced list selection box for the Group Types.
                _rlbGroupTypes = new RockListBox
                {
                    ID = "rlbGroupTypes",
                    Label = "Group Types",
                    Help = "Specifies which group types are included in search results.",
                    Required = true,
                    DisplayDropAsAbsolute = true,
                    AutoPostBack = true
                };
                _rlbGroupTypes.SelectedIndexChanged += GroupTypes_SelectedIndexChanged;

                GroupTypeCache.All()
                    .ForEach( t =>
                    {
                        _rlbGroupTypes.Items.Add( new ListItem( t.Name, t.Guid.ToString() ) );
                    } );

                panel.Controls.Add( _rlbGroupTypes );

                // Construct the check box list for the group attributes to provide
                // in the filtering options.
                _cblAttributeFilters = new RockCheckBoxList
                {
                    ID = "cblAttributeFilters",
                    Label = "Attribute Filters",
                    Help = "Attributes to make available for the user to filter groups by.",
                    RepeatDirection = RepeatDirection.Horizontal
                };

                panel.Controls.Add( _cblAttributeFilters );

                return panel;
            }

            /// <inheritdoc/>
            public override void ReadSettingsFromEntity( IHasAttributes attributeEntity, Control control )
            {
                // Validate all the controls are what we expected.
                if ( control.Controls.Count != 2
                    || !( control.Controls[0] is RockListBox rlbGroupTypes )
                    || !( control.Controls[1] is RockCheckBoxList cblAttributeFilters ) )
                {
                    return;
                }

                // Set the value for the Group Type control.
                var groupTypeGuids = attributeEntity.GetAttributeValue( AttributeKey.GroupTypes )
                    .SplitDelimitedValues()
                    .AsGuidList();

                foreach ( ListItem li in rlbGroupTypes.Items )
                {
                    li.Selected = groupTypeGuids.Contains( li.Value.AsGuid() );
                }

                // Set the value for the Attribute Filters control.
                var attributeGuids = attributeEntity.GetAttributeValue( AttributeKey.AttributeFilters )
                    .SplitDelimitedValues()
                    .AsGuidList();

                UpdateAttributeFilterOptions();

                foreach ( ListItem li in cblAttributeFilters.Items )
                {
                    li.Selected = attributeGuids.Contains( li.Value.AsGuid() );
                }
            }

            /// <inheritdoc/>
            public override void WriteSettingsToEntity( IHasAttributes attributeEntity, Control control, RockContext rockContext )
            {
                // Validate all the controls are what we expected.
                if ( control.Controls.Count != 2
                    || !( control.Controls[0] is RockListBox rlbGroupTypes )
                    || !( control.Controls[1] is RockCheckBoxList cblAttributeFilters ) )
                {
                    return;
                }

                // Save the custom settings.
                attributeEntity.SetAttributeValue( AttributeKey.GroupTypes, rlbGroupTypes.SelectedValues.AsDelimited( "," ) );
                attributeEntity.SetAttributeValue( AttributeKey.AttributeFilters, cblAttributeFilters.SelectedValues.AsDelimited( "," ) );
            }

            /// <summary>
            /// Updates the attribute filter options. These are dynamic based on
            /// what Group Types are currently selected.
            /// </summary>
            private void UpdateAttributeFilterOptions()
            {
                var currentValues = _cblAttributeFilters.SelectedValues.AsGuidList();

                _cblAttributeFilters.Items.Clear();

                foreach ( var groupTypeGuid in _rlbGroupTypes.SelectedValues.AsGuidList() )
                {
                    var groupType = GroupTypeCache.Get( groupTypeGuid );

                    if ( groupType == null )
                    {
                        continue;
                    }

                    // Create a fake Group in memory and then load the attributes
                    // so we can see all the options that need to be added.
                    var group = new Model.Group
                    {
                        GroupTypeId = groupType.Id
                    };

                    group.LoadAttributes();

                    // Add a new ListItem for the attribute. If it was previously
                    // selected then select it again.
                    foreach ( var attribute in group.Attributes )
                    {
                        if ( attribute.Value.FieldType.Field.HasFilterControl() )
                        {
                            var listItem = new ListItem( $"{attribute.Value.Name} ({groupType.Name })", attribute.Value.Guid.ToString() )
                            {
                                Selected = currentValues.Contains( attribute.Value.Guid )
                            };

                            _cblAttributeFilters.Items.Add( listItem );
                        }
                    }
                }

                // If we don't have any items to choose from then hide the
                // entire input field.
                _cblAttributeFilters.Visible = _cblAttributeFilters.Items.Count > 0;
            }

            #endregion

            #region Event Handlers

            /// <summary>
            /// Handles the SelectedIndexChanged event of the GroupTypes control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            private void GroupTypes_SelectedIndexChanged( object sender, System.EventArgs e )
            {
                // Update the available attributes to pick from.
                UpdateAttributeFilterOptions();
            }

            #endregion
        }
    }
}
