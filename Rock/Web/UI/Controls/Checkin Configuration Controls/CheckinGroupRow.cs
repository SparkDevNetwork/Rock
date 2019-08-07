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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:CheckinGroupRow runat=server></{0}:CheckinGroupRow>" )]
    public class CheckinGroupRow : CompositeControl
    {
        private Group _group;
        private HiddenFieldWithClass _hfExpanded;
        private HiddenField _hfGroupGuid;

        private Label _lblGroupRowName;

        private LinkButton _lbAddGroup;

        /// <summary>
        /// Gets the group type unique identifier.
        /// </summary>
        /// <value>
        /// The group type unique identifier.
        /// </value>
        public Guid GroupGuid
        {
            get
            {
                return new Guid( _hfGroupGuid.Value );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool Selected
        {
            get
            {
                bool? b = ViewState["Selected"] as bool?;
                return ( b == null ) ? false : b.Value;
            }

            set
            {
                ViewState["Selected"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckinGroupRow"/> is expanded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expanded; otherwise, <c>false</c>.
        /// </value>
        public bool Expanded
        {
            get
            {
                EnsureChildControls();
                return _hfExpanded.Value.AsBooleanOrNull() ?? false;
            }

            set
            {
                EnsureChildControls();
                _hfExpanded.Value = value.ToString();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
// checkin-group animation
//$('section.checkin-group').click(function () {
//    $(this).siblings('div').slideToggle();
//    $expanded = $(this).children('input.group-expanded');
//    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');
//});

// fix so that the Remove button will fire its event, but not the parent event 
$('.checkin-group a.btn-danger').click(function (event) {
    event.stopImmediatePropagation();
    if ( isDirty() ) {{
        return false;
    }}
});

// fix so that the Edit Group button will fire its event, but not the parent event 
$('.checkin-group a.checkin-group-edit-group').click(function (event) {
    event.stopImmediatePropagation();
    if ( isDirty() ) {{
        return false;
    }}
});

// fix so that the Ad Check-in Group button will fire its event, but not the parent event 
$('.checkin-group a.checkin-group-add-group').click(function (event) {
    event.stopImmediatePropagation();
    if ( isDirty() ) {{
        return false;
    }}
});";

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "CheckinGroupRow", script, true );
        }

        /// <summary>
        /// Sets the type of the group.
        /// </summary>
        /// <param name="group">Type of the group.</param>
        public void SetGroup( Group group )
        {
            if ( group != null )
            {
                _group = group;
                EnsureChildControls();
                _hfGroupGuid.Value = group.Guid.ToString();
                _lblGroupRowName.Text = group.Name;
                if ( !group.IsActive )
                {
                    _lblGroupRowName.Text += " <small>(Inactive)</small>";
                }
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfExpanded = new HiddenFieldWithClass();
            Controls.Add( _hfExpanded );
            _hfExpanded.ID = this.ID + "_hfExpanded";
            _hfExpanded.CssClass = "group-expanded";
            _hfExpanded.Value = "False";

            _hfGroupGuid = new HiddenField();
            _hfGroupGuid.ID = this.ID + "_hfGroupGuid";
            Controls.Add( _hfGroupGuid );

            _lblGroupRowName = new Label();
            _lblGroupRowName.ClientIDMode = ClientIDMode.Static;
            _lblGroupRowName.ID = this.ID + "_lblGroupRowName";
            Controls.Add( _lblGroupRowName );


            _lbAddGroup = new LinkButton();
            _lbAddGroup.ID = this.ID + "_lbAddGroup";
            _lbAddGroup.CssClass = "btn btn-xs btn-default checkin-group-add-group";
            _lbAddGroup.Click += lbAddGroup_Click;
            _lbAddGroup.CausesValidation = false;
            _lbAddGroup.ToolTip = "Add New Group";
            _lbAddGroup.Controls.Add( new LiteralControl { Text = "<i class='fa fa-plus'></i> <i class='fa fa-check-circle'></i>" } );
            Controls.Add( _lbAddGroup );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "data-key", _hfGroupGuid.Value );
            writer.RenderBeginTag( HtmlTextWriterTag.Li );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, string.Format( "checkin-item{0} checkin-group rollover-container {1}", Selected ? " checkin-item-selected" : "", !_group.IsActive ? " is-inactive" : "" ) );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ID + "_section" );
            writer.RenderBeginTag( "section" );

            // Hidden Field to track expansion
            _hfExpanded.RenderControl( writer );

            writer.WriteLine( "<a class='checkin-group-reorder'><i class='fa fa-bars'></i></a>" );
            writer.WriteLine( "<a class='checkin-group-expand'><i class='checkin-group-state fa fa-check-circle'></i></a>" );

            _lblGroupRowName.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right checkin-item-actions rollover-item" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( _lbAddGroup != null )
            {
                _lbAddGroup.RenderControl( writer );
                writer.Write( " " );
            }

            writer.RenderEndTag();  // Div
            writer.RenderEndTag();  // Section

            if ( !Expanded )
            {
                writer.AddStyleAttribute( "display", "none" );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            var groupRows = this.Controls.OfType<CheckinGroupRow>();
            if ( groupRows.Any() )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "checkin-list js-checkin-group-list" );
                writer.RenderBeginTag( HtmlTextWriterTag.Ul );
                foreach ( var groupRow in groupRows )
                {
                    groupRow.RenderControl( writer );
                }
                writer.RenderEndTag();
            }

            writer.RenderEndTag();  // Div

            writer.RenderEndTag();  // Li
        }

        /// <summary>
        /// Handles the Click event of the lbAddGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddGroup_Click( object sender, EventArgs e )
        {
            if ( AddGroupClick != null )
            {
                AddGroupClick( this, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lblDeleteGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lblDeleteGroup_Click( object sender, EventArgs e )
        {
            if ( DeleteGroupClick != null )
            {
                DeleteGroupClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [add group click].
        /// </summary>
        public event EventHandler AddGroupClick;

        /// <summary>
        /// Occurs when [delete group click].
        /// </summary>
        public event EventHandler DeleteGroupClick;

    }
}