// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupRolePicker : CompositeControl, IRockControl
    {
        #region IRockControl implementation (Custom implementation)

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get 
            {
                EnsureChildControls();
                return _ddlGroupRole.Required; 
            }
            set 
            {
                EnsureChildControls();
                _ddlGroupRole.Required = value; 
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private RockDropDownList _ddlGroupType;
        private RockDropDownList _ddlGroupRole;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the group type id.
        /// </summary>
        /// <value>
        /// The group type id.
        /// </value>
        public int? GroupTypeId
        {
            get
            {
                return ViewState["GroupTypeId"] as int?;
            }

            set
            {
                ViewState["GroupTypeId"] = value;
                if ( value.HasValue )
                {
                    LoadGroupRoles( value.Value );
                }
            }
        }

        /// <summary>
        /// Gets or sets the role id.
        /// </summary>
        /// <value>
        /// The role id.
        /// </value>
        public int? GroupRoleId
        {
            get
            {
                EnsureChildControls();
                int groupRoleId = int.MinValue;
                if ( int.TryParse( _ddlGroupRole.SelectedValue, out groupRoleId ) && groupRoleId > 0 )
                {
                    return groupRoleId;
                }

                return null;
            }

            set
            {
                EnsureChildControls();
                int groupRoleId = value.HasValue ? value.Value : 0;
                if ( _ddlGroupRole.SelectedValue != groupRoleId.ToString() )
                {
                    if ( !GroupTypeId.HasValue )
                    {
                        var groupRole = new Rock.Model.GroupTypeRoleService( new RockContext() ).Get( groupRoleId );
                        if ( groupRole != null &&
                            groupRole.GroupTypeId.HasValue &&
                            _ddlGroupType.SelectedValue != groupRole.GroupTypeId.ToString() )
                        {
                            _ddlGroupType.SelectedValue = groupRole.GroupTypeId.ToString();

                            LoadGroupRoles( groupRole.GroupTypeId.Value );
                        }
                    }

                    _ddlGroupRole.SetValue( groupRoleId.ToString() );
                }
            }
        }

        /// <summary>
        /// Gets or sets the exclude group roles.
        /// </summary>
        /// <value>
        /// The exclude group roles.
        /// </value>
        public List<int> ExcludeGroupRoles
        {
            get
            {
                var excludeGroupRoles = ViewState["ExcludeGroupRoles"] as List<int>;
                if (excludeGroupRoles == null)
                {
                    excludeGroupRoles = new List<int>();
                    ViewState["ExcludeGroupRoles"] = excludeGroupRoles;
                }
                return excludeGroupRoles;
            }

            set
            {
                ViewState["ExcludeGroupRoles"] = value;
                LoadGroupRoles( GroupTypeId );
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRolePicker"/> class.
        /// </summary>
        public GroupRolePicker()
            : base()
        {
            HelpBlock = new HelpBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _ddlGroupType = new RockDropDownList();
            _ddlGroupType.ID = this.ID + "_ddlGroupType";
            _ddlGroupType.AutoPostBack = true;
            _ddlGroupType.SelectedIndexChanged += _ddlGroupType_SelectedIndexChanged;
            Controls.Add( _ddlGroupType );

            _ddlGroupRole = new RockDropDownList();
            _ddlGroupRole.ID = this.ID + "_ddlGroupRole";
            Controls.Add( _ddlGroupRole );

            LoadGroupTypes();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ddlGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _ddlGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int groupTypeId = _ddlGroupType.SelectedValue.AsInteger();
            LoadGroupRoles( groupTypeId );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( !GroupTypeId.HasValue )
            {
                _ddlGroupType.RenderControl( writer );
                _ddlGroupRole.Label = ( _ddlGroupType.SelectedItem != null ? _ddlGroupType.SelectedItem.Text : this.Label ) + " Role";
            }
            else
            {
                _ddlGroupRole.Label = string.Empty;
            }

            _ddlGroupRole.RenderControl( writer );
        }

        /// <summary>
        /// Loads the group types.
        /// </summary>
        private void LoadGroupTypes()
        {
            _ddlGroupType.Items.Clear();
            
            if ( !Required )
            {
                _ddlGroupType.Items.Add( new ListItem( string.Empty, Rock.Constants.None.IdValue ) );
            }

            using ( var rockContext = new RockContext() )
            {
                var groupTypeService = new Rock.Model.GroupTypeService( rockContext );

                // get all group types that have at least one role
                var groupTypes = groupTypeService.Queryable().Where( a => a.Roles.Any() ).OrderBy( a => a.Name ).ToList();
                foreach ( var g in groupTypes )
                {
                    _ddlGroupType.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ) );
                }
            }
        }

        /// <summary>
        /// Loads the group roles.
        /// </summary>
        /// <param name="groupTypeId">The group type unique identifier.</param>
        private void LoadGroupRoles( int? groupTypeId )
        {
            int? currentGroupRoleId = this.GroupRoleId;
            _ddlGroupRole.SelectedValue = null;
            _ddlGroupRole.Items.Clear();
            if ( groupTypeId.HasValue )
            {
                if ( !Required )
                {
                    _ddlGroupRole.Items.Add( new ListItem( string.Empty, Rock.Constants.None.IdValue ) );
                }

                List<int> excludeGroupRoles = ExcludeGroupRoles;

                var groupRoleService = new Rock.Model.GroupTypeRoleService( new RockContext() );
                var groupRoles = groupRoleService.Queryable()
                    .Where( r => 
                        r.GroupTypeId == groupTypeId.Value &&
                        !excludeGroupRoles.Contains(r.Id))
                    .OrderBy( a => a.Name )
                    .ToList();

                foreach ( var r in groupRoles )
                {
                    var roleItem = new ListItem( r.Name, r.Id.ToString().ToUpper() );
                    roleItem.Selected = r.Id == currentGroupRoleId;
                    _ddlGroupRole.Items.Add( roleItem );
                }
            }
        }
    }
}