//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupRolePicker : CompositeControl, ILabeledControl, IRequiredControl
    {
        private Literal _label;
        protected HelpBlock _helpBlock;
        private DropDownList _ddlGroupType;
        private DropDownList _ddlGroupRole;

        public GroupRolePicker()
            : base()
        {
            _label = new Literal();
            _helpBlock = new HelpBlock();
            
            _ddlGroupType = new DropDownList();
            LoadGroupTypes();

            _ddlGroupRole = new DropDownList();
        }

        public string Label
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }

        public string Help
        {
            get
            {
                return _helpBlock.Text;
            }
            set
            {
                _helpBlock.Text = value;
            }
        }

        public int? GroupTypeId
        {
            get { return ViewState["GroupTypeId"] as int?; }
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
                int groupRoleId = int.MinValue;
                if ( int.TryParse( _ddlGroupRole.SelectedValue, out groupRoleId ) && groupRoleId > 0 )
                {
                    return groupRoleId;
                }

                return null;
            }

            set
            {
                int groupRoleId = value.HasValue ? value.Value : 0;
                if ( _ddlGroupRole.SelectedValue != groupRoleId.ToString() )
                {
                    var groupRole = new Rock.Model.GroupRoleService().Get( groupRoleId );
                    if ( groupRole != null && 
                        groupRole.GroupTypeId.HasValue && 
                        _ddlGroupType.SelectedValue != groupRole.GroupTypeId.ToString() )
                    {
                        _ddlGroupType.SelectedValue = groupRole.GroupTypeId.ToString();
                        LoadGroupRoles( groupRole.GroupTypeId.Value );
                    }

                    var selectedItem = _ddlGroupRole.Items.FindByValue( groupRoleId.ToString() );
                    if ( selectedItem != null )
                    {
                        selectedItem.Selected = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IRequiredControl" /> is required.
        /// </summary>
        /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
        public bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set 
            { 
                ViewState["Required"] = value;
                if ( GroupTypeId.HasValue )
                {
                    LoadGroupRoles( GroupTypeId.Value );
                }
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>The required error message.</value>
        public string RequiredErrorMessage
        {
            get { return ViewState["RequiredErrorMessage"] as string ?? "Group Role is Required"; }
            set { ViewState["RequiredErrorMessage"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get { return true; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        //    var sm = ScriptManager.GetCurrent( this.Page );

        //    if ( sm != null )
        //    {
        //        sm.RegisterAsyncPostBackControl( _ddlGroupType );
        //    }
        }


        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _label.ID = this.ID + "_label";
            Controls.Add( _label );

            _helpBlock.ID = this.ID + "_helpblock";
            Controls.Add( _helpBlock );

            _ddlGroupType.ID = this.ID + "_ddlGroupType";
            _ddlGroupType.AutoPostBack = true;
            _ddlGroupType.SelectedIndexChanged += _ddlGroupType_SelectedIndexChanged;
            Controls.Add( _ddlGroupType );

            _ddlGroupRole.ID = this.ID + "_ddlGroupRole";
            Controls.Add( _ddlGroupRole );
        }

        void _ddlGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int groupTypeId = int.MinValue;
            if ( int.TryParse( _ddlGroupType.SelectedValue, out groupTypeId ) && groupTypeId > 0 )
            {
                LoadGroupRoles( groupTypeId );
            }
        }

        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                bool renderControlGroupDiv = ( !string.IsNullOrWhiteSpace( Label ) || !string.IsNullOrWhiteSpace( Help ) );

                if ( renderControlGroupDiv )
                {
                    writer.AddAttribute( "class", "control-group" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( "class", "control-label" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _label.RenderControl( writer );
                    _helpBlock.RenderControl( writer );
                    writer.RenderEndTag();

                    writer.AddAttribute( "class", "controls" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                }

                if ( !GroupTypeId.HasValue )
                {
                    writer.Write( "Group Type " );
                    _ddlGroupType.RenderControl( writer );
                    writer.Write( " Role " );
                }

                _ddlGroupRole.RenderControl(writer);

                if ( renderControlGroupDiv )
                {
                    writer.RenderEndTag();  // controls
                    writer.RenderEndTag();  // control-group
                }
            }
        }

        private void LoadGroupTypes()
        {
            _ddlGroupType.Items.Clear();

            var groupTypeService = new Rock.Model.GroupTypeService();
            var groupTypes = groupTypeService.Queryable().OrderBy( a => a.Name ).ToList();
            groupTypes.ForEach( g => 
                _ddlGroupType.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ))
            );
        }

        private void LoadGroupRoles(int groupTypeId)
        {
            int? groupRoleId = GroupRoleId;

            _ddlGroupRole.Items.Clear();

            if ( !Required )
            {
                _ddlGroupRole.Items.Add( new ListItem( string.Empty, Rock.Constants.None.IdValue ) );
            }

            var groupRoleService = new Rock.Model.GroupRoleService();
            var groupRoles = groupRoleService.Queryable().Where( r => r.GroupTypeId == groupTypeId ).OrderBy( a => a.Name ).ToList();
            groupRoles.ForEach( r =>
                _ddlGroupRole.Items.Add( new ListItem( r.Name, r.Id.ToString().ToUpper() ) )
            );

            GroupRoleId = groupRoleId;
        }

    }
}