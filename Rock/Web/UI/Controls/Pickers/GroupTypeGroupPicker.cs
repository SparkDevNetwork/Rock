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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a group type and then a group of that group type
    /// </summary>
    public class GroupTypeGroupPicker : CompositeControl, IRockControl
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
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
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
                return _ddlGroup.Required;
            }
            set
            {
                EnsureChildControls();
                _ddlGroup.Required = value;
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
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

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
        private RockDropDownList _ddlGroup;

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
                EnsureChildControls();
                return _ddlGroupType.SelectedValue.AsIntegerOrNull();
            }

            set
            {
                EnsureChildControls();
                _ddlGroupType.SetValue( value );
                if ( value.HasValue )
                {
                    LoadGroups( value.Value );
                }
            }
        }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int? GroupId
        {
            get
            {
                EnsureChildControls();
                return _ddlGroup.SelectedValue.AsIntegerOrNull();
            }

            set
            {
                EnsureChildControls();
                int groupId = value ?? 0;
                if ( _ddlGroup.SelectedValue != groupId.ToString() )
                {
                    if ( !GroupTypeId.HasValue || GroupTypeId.Value == 0 )
                    {
                        var group = new Rock.Model.GroupService( new RockContext() ).Get( groupId );
                        if ( group != null &&
                            _ddlGroupType.SelectedValue != group.GroupTypeId.ToString() )
                        {
                            _ddlGroupType.SelectedValue = group.GroupTypeId.ToString();

                            LoadGroups( group.GroupTypeId );
                        }
                    }

                    _ddlGroup.SelectedValue = groupId.ToString();
                }
            }
        }

        /// <summary>
        /// Gets or sets the group control label.
        /// </summary>
        /// <value>
        /// The group control label.
        /// </value>
        public string GroupControlLabel
        {
            get
            {
                return ( ViewState["GroupControlLabel"] as string ) ?? "Group";
            }

            set
            {
                ViewState["GroupControlLabel"] = value;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeGroupPicker"/> class.
        /// </summary>
        public GroupTypeGroupPicker()
            : base()
        {
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
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
            _ddlGroupType.Label = "Group Type";
            Controls.Add( _ddlGroupType );

            _ddlGroup = new RockDropDownList();
            _ddlGroup.ID = this.ID + "_ddlGroup";
            Controls.Add( _ddlGroup );

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
            LoadGroups( groupTypeId );
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
            _ddlGroup.Visible = GroupTypeId.HasValue;
            _ddlGroupType.RenderControl( writer );
            _ddlGroup.Label = this.GroupControlLabel;
            _ddlGroup.RenderControl( writer );
        }

        /// <summary>
        /// Loads the group types.
        /// </summary>
        private void LoadGroupTypes()
        {
            _ddlGroupType.Items.Clear();
            _ddlGroupType.Items.Add( Rock.Constants.None.ListItem );

            var groupTypeService = new Rock.Model.GroupTypeService( new RockContext() );

            // get all group types that have the ShowInGroupList flag set
            var groupTypes = groupTypeService.Queryable().Where( a => a.ShowInGroupList ).OrderBy( a => a.Name ).ToList();

            foreach ( var g in groupTypes )
            {
                _ddlGroupType.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ) );
            }
        }

        /// <summary>
        /// Loads the groups.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        private void LoadGroups( int? groupTypeId )
        {
            int? currentGroupId = this.GroupId;
            _ddlGroup.SelectedValue = null;
            _ddlGroup.Items.Clear();
            if ( groupTypeId.HasValue )
            {
                _ddlGroup.Items.Add( Rock.Constants.None.ListItem );

                var groupService = new Rock.Model.GroupService( new RockContext() );
                var groups = groupService.Queryable().Where( r => r.GroupTypeId == groupTypeId.Value ).OrderBy( a => a.Name ).ToList();

                foreach ( var r in groups )
                {
                    var item = new ListItem( r.Name, r.Id.ToString().ToUpper() );
                    item.Selected = r.Id == currentGroupId;
                    _ddlGroup.Items.Add( item );
                }
            }
        }
    }
}