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

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupPicker : ItemPicker
    {
        #region Controls

        /// <summary>
        /// The checkbox to show inactive groups
        /// </summary>
        private RockCheckBox _cbShowInactiveGroups;

        /// <summary>
        /// The Select All button
        /// </summary>
        private HyperLink _btnSelectAll;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPicker"/> class.
        /// </summary>
        public GroupPicker(): base()
        {
            this.ShowSelectChildren = true;
        }

        /// <summary>
        /// Gets or sets the root group identifier.
        /// </summary>
        /// <value>
        /// The root group identifier.
        /// </value>
        public int? RootGroupId
        {
            get
            {
                return ViewState["RootGroupId"] as int?;
            }

            set
            {
                ViewState["RootGroupId"] = value;
                SetExtraRestParams();
            }
        }

        /// <summary>
        /// Gets or sets the included group type ids.
        /// </summary>
        /// <value>
        /// The included group type ids.
        /// </value>
        public List<int> IncludedGroupTypeIds
        {
            get
            {
                return ViewState["IncludedGroupTypeIds"] as List<int> ?? new List<int>();
            }

            set
            {
                ViewState["IncludedGroupTypeIds"] = value;
                SetExtraRestParams();
            }
        }


        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _cbShowInactiveGroups = new RockCheckBox();
            _cbShowInactiveGroups.ContainerCssClass = "pull-right";
            _cbShowInactiveGroups.SelectedIconCssClass = "fa fa-check-square-o";
            _cbShowInactiveGroups.UnSelectedIconCssClass = "fa fa-square-o";
            _cbShowInactiveGroups.ID = this.ID + "_cbShowInactiveGroups";
            _cbShowInactiveGroups.Text = "Show Inactive";
            _cbShowInactiveGroups.AutoPostBack = true;
            _cbShowInactiveGroups.CheckedChanged += _cbShowInactiveGroups_CheckedChanged;
            this.Controls.Add( _cbShowInactiveGroups );

            _btnSelectAll = new HyperLink();
            _btnSelectAll.ID = "_btnSelectAll";
            _btnSelectAll.CssClass = "btn btn-default btn-xs js-select-all pull-right margin-l-sm";
            _btnSelectAll.Text = "Select All";

            this.Controls.Add( _btnSelectAll );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            SetExtraRestParams();
            this.IconCssClass = "fa fa-users";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="group">The group.</param>
        public void SetValue( Group group )
        {
            if ( group != null )
            {
                ItemId = group.Id.ToString();

                var parentIds = GetGroupAncestorsIdList( group.ParentGroup );
                InitialItemParentIds = parentIds.AsDelimited( "," );
                ItemName = group.Name;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Returns a list of the ancestor Groups of the specified Group.
        /// If the ParentGroup property of the Group is not populated, it is assumed to be a top-level node.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="ancestorGroupIds"></param>
        /// <returns></returns>
        private List<int> GetGroupAncestorsIdList( Group group, List<int> ancestorGroupIds = null )
        {
            if ( ancestorGroupIds == null )
            {
                ancestorGroupIds = new List<int>();
            }

            if ( group == null )
            {
                return ancestorGroupIds;
            }

            // If we have encountered this node previously in our tree walk, there is a recursive loop in the tree.
            if ( ancestorGroupIds.Contains( group.Id ) )
            {
                return ancestorGroupIds;
            }

            // Create or add this node to the history stack for this tree walk.
            ancestorGroupIds.Insert(0, group.Id );

            ancestorGroupIds = this.GetGroupAncestorsIdList( group.ParentGroup, ancestorGroupIds );

            return ancestorGroupIds;
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="groups">The groups.</param>
        public void SetValues( IEnumerable<Group> groups )
        {
            if ( groups != null && groups.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentIds = new List<int>();

                foreach ( var group in groups )
                {
                    if ( group != null )
                    {
                        ids.Add( group.Id.ToString() );
                        names.Add( group.Name );
                        if ( group.ParentGroup != null && !parentIds.Contains( group.ParentGroup.Id ) )
                        {
                            var parentGroup = group.ParentGroup;
                            var groupParentIds = GetGroupAncestorsIdList( parentGroup );
                            foreach ( var groupParentId in groupParentIds )
                            {
                                if ( !parentIds.Contains( groupParentId ) )
                                {
                                    parentIds.Add( groupParentId );
                                }
                            }
                        }
                    }
                }

                // NOTE: Order is important (parents before children) since the GroupTreeView loads on demand
                InitialItemParentIds = parentIds.AsDelimited( "," );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var group = new GroupService( new RockContext() ).Get( int.Parse( ItemId ) );
            SetValue( group );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var groupIds = ItemIds.Where( i => i != "0" ).AsIntegerList();
            if ( groupIds.Any() )
            {
                var groups = new GroupService( new RockContext() )
                    .Queryable()
                    .Where( g => groupIds.Contains( g.Id ) )
                    .ToList();
                this.SetValues( groups );
            }
            else
            {
                this.SetValues( null );
            }
        }

        /// <summary>
        /// Gets or sets the selected group id.
        /// </summary>
        /// <value>
        /// The group id.
        /// </value>
        public int? GroupId
        {
            get
            {
                int selectedId = this.SelectedValue.AsInteger();
                if ( selectedId > 0 )
                {
                    return selectedId;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                SetValue( value );
            }
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/groups/getchildren/"; }
        }

        /// <summary>
        /// Render any additional picker actions
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderCustomPickerActions( HtmlTextWriter writer )
        {
            base.RenderCustomPickerActions( writer );

            if ( this.AllowMultiSelect )
            {
                _btnSelectAll.RenderControl( writer );
            }

            _cbShowInactiveGroups.RenderControl( writer );
        }

        /// <summary>
        /// Sets the extra rest parameters.
        /// </summary>
        private void SetExtraRestParams( bool includeInactiveGroups = false )
        {
            var extraParams = new System.Text.StringBuilder();
            extraParams.Append( $"?includeInactiveGroups={includeInactiveGroups.ToTrueFalse()}" );
            if ( RootGroupId.HasValue )
            {
                extraParams.Append( $"&rootGroupId={RootGroupId.Value}" );
            }
            if ( IncludedGroupTypeIds != null && IncludedGroupTypeIds.Any() )
            {
                extraParams.Append( $"&includedGroupTypeIds={IncludedGroupTypeIds.AsDelimited(",")}" );
            }

            ItemRestUrlExtraParams = extraParams.ToString();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the _cbShowInactiveGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void _cbShowInactiveGroups_CheckedChanged( object sender, EventArgs e )
        {
            ShowDropDown = true;
            SetExtraRestParams( _cbShowInactiveGroups.Checked );
        }
    }
}