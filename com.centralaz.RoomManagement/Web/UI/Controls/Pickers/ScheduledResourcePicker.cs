// <copyright>
// Copyright by the Central Christian Church
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
using com.centralaz.RoomManagement.Model;
using Rock;
using Rock.Data;
using Rock.Web.UI.Controls;

namespace com.centralaz.RoomManagement.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:ScheduledResourcePicker runat=server></{0}:ScheduledResourcePicker>" )]
    public class ScheduledResourcePicker : Rock.Web.UI.Controls.ItemPicker
    {
        #region Controls

        public int? CampusId
        {
            get { return ViewState["CampusId"] as int? ?? 0; }
            set
            {
                ViewState["CampusId"] = value;
            }
        }

        /// <summary>
        /// The checkbox to show inactive groups
        /// </summary>
        private RockCheckBox _cbShowAllResources;

        public bool ShowAllResources
        {
            get
            {
                return _cbShowAllResources.Checked;
            }
        }

        #endregion

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _cbShowAllResources = new RockCheckBox();
            _cbShowAllResources.ContainerCssClass = "pull-right";
            _cbShowAllResources.SelectedIconCssClass = "fa fa-check-square-o";
            _cbShowAllResources.UnSelectedIconCssClass = "fa fa-square-o";
            _cbShowAllResources.ID = this.ID + "_cbShowAllResources";
            _cbShowAllResources.Text = "Show All Resources";
            _cbShowAllResources.AutoPostBack = true;
            _cbShowAllResources.CheckedChanged += _cbShowAllResources_CheckedChanged;
            this.Controls.Add( _cbShowAllResources );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            ItemRestUrlExtraParams = "?includeAllCampuses=false&campusId=" + ( CampusId != null ? CampusId.ToString() : "0" );
            this.IconCssClass = "fa fa-cogs";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="resource">The Workflow Type.</param>
        public void SetValue( Resource resource )
        {
            if ( resource != null )
            {
                ItemId = resource.Id.ToString();

                string parentCategoryIds = string.Empty;
                var parentCategory = resource.Category;
                while ( parentCategory != null )
                {
                    parentCategoryIds = parentCategory.Id + "," + parentCategoryIds;
                    parentCategory = parentCategory.ParentCategory;
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemName = resource.Name;
            }
            else
            {
                ItemId = Rock.Constants.None.IdValue;
                ItemName = Rock.Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="resources">The schedules.</param>
        public void SetValues( IEnumerable<Resource> resources )
        {
            var resourceList = resources.ToList();

            if ( resourceList.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var resource in resourceList )
                {
                    if ( resource != null )
                    {
                        ids.Add( resource.Id.ToString() );
                        names.Add( resource.Name );
                        var parentCategory = resource.Category;

                        while ( parentCategory != null )
                        {
                            parentCategoryIds += parentCategory.Id.ToString() + ",";
                            parentCategory = parentCategory.ParentCategory;
                        }
                    }
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Rock.Constants.None.IdValue;
                ItemName = Rock.Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var resource = new ResourceService( new RockContext() ).Get( int.Parse( ItemId ) );
            SetValue( resource );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var resources = new ResourceService( new RockContext() ).Queryable().Where( g => ItemIds.Contains( g.Id.ToString() ) );
            this.SetValues( resources );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/com_centralaz/ScheduledResources/GetChildren/"; }
        }

        /// <summary>
        /// Render any additional picker actions
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderCustomPickerActions( HtmlTextWriter writer )
        {
            base.RenderCustomPickerActions( writer );

            _cbShowAllResources.RenderControl( writer );
        }

        /// <summary>
        /// Sets the extra rest parameters.
        /// </summary>
        public void SetExtraRestParams( bool includeAllCampuses = false )
        {
            ItemRestUrlExtraParams = "?includeAllCampuses=" + includeAllCampuses.ToTrueFalse() + "&campusId=" + ( CampusId != null ? CampusId.ToString() : "0" ) + "&getCategorizedItems=true&showUnnamedEntityItems=true&showCategoriesThatHaveNoChildren=true";
        }

        /// <summary>
        /// Handles the CheckedChanged event of the _cbShowAllResources control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void _cbShowAllResources_CheckedChanged( object sender, EventArgs e )
        {
            ShowDropDown = true;
            SetExtraRestParams( _cbShowAllResources.Checked );
        }
    }
}