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
    [ToolboxData( "<{0}:CheckinAreaRow runat=server></{0}:CheckinAreaRow>" )]
    public class CheckinAreaRow : CompositeControl
    {
        private HiddenFieldWithClass _hfExpanded;
        private HiddenField _hfGroupTypeGuid;

        private Label _lblAreaRowName;

        private LinkButton _lbAddArea;
        private LinkButton _lbAddGroup;

        /// <summary>
        /// Gets the group type unique identifier.
        /// </summary>
        /// <value>
        /// The group type unique identifier.
        /// </value>
        public Guid GroupTypeGuid
        {
            get
            {
                return new Guid( _hfGroupTypeGuid.Value );
            }
        }

        /// <summary>
        /// Gets the parent row.
        /// </summary>
        /// <value>
        /// The parent area row.
        /// </value>
        public CheckinAreaRow ParentRow
        {
            get
            {
                return this.Parent as CheckinAreaRow;
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
        /// Gets or sets a value indicating whether this <see cref="CheckinAreaRow"/> is expanded.
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
// checkin-area animation
//$('section.checkin-area').click(function () {
//    $(this).siblings('div').slideToggle();
//    $expanded = $(this).children('input.area-expanded');
//    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');
//});

// fix so that the Remove button will fire its event, but not the parent event 
$('.checkin-area a.btn-danger').click(function (event) {
    event.stopImmediatePropagation();
    if ( isDirty() ) {{
        return false;
    }}
});

// fix so that the Reorder button will fire its event, but not the parent event 
$('.checkin-area a.checkin-area-reorder').click(function (event) {
    event.stopImmediatePropagation();
    if ( isDirty() ) {{
        return false;
    }}
});

// fix so that the Add Sub-Area button will fire its event, but not the parent event 
$('.checkin-area a.checkin-area-add-area').click(function (event) {
    event.stopImmediatePropagation();
    if ( isDirty() ) {{
        return false;
    }}
});

// fix so that the Ad Check-in Group button will fire its event, but not the parent event 
$('.checkin-area a.checkin-area-add-group').click(function (event) {
    event.stopImmediatePropagation();
    if ( isDirty() ) {{
        return false;
    }}
});";

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "CheckinAreaRow", script, true );
        }

        /// <summary>
        /// Sets the type of the group.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        public void SetGroupType( GroupType groupType )
        {
            EnsureChildControls();
            if ( groupType != null )
            {
                _hfGroupTypeGuid.Value = groupType.Guid.ToString();
                _lblAreaRowName.Text = groupType.Name;
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
            _hfExpanded.CssClass = "area-expanded";
            _hfExpanded.Value = "False";

            _hfGroupTypeGuid = new HiddenField();
            _hfGroupTypeGuid.ID = this.ID + "_hfGroupTypeGuid";

            _lblAreaRowName = new Label();
            _lblAreaRowName.ClientIDMode = ClientIDMode.Static;
            _lblAreaRowName.ID = this.ID + "_lblAreaRowName";

            _lbAddArea = new LinkButton();
            _lbAddArea.ID = this.ID + "_lblbAddArea";
            _lbAddArea.CssClass = "btn btn-xs btn-default checkin-area-add-area";
            _lbAddArea.Click += lbAddArea_Click;
            _lbAddArea.CausesValidation = false;
            _lbAddArea.ToolTip = "Add New Area";
            _lbAddArea.Controls.Add( new LiteralControl { Text = "<i class='fa fa-plus'></i> <i class='fa fa-folder-open'></i>" } );

            _lbAddGroup = new LinkButton();
            _lbAddGroup.ID = this.ID + "_lbAddGroup";
            _lbAddGroup.CssClass = "btn btn-xs btn-default checkin-area-add-group";
            _lbAddGroup.Click += lbAddGroup_Click;
            _lbAddGroup.CausesValidation = false;
            _lbAddGroup.ToolTip = "Add New Group";
            _lbAddGroup.Controls.Add( new LiteralControl { Text = "<i class='fa fa-plus'></i> <i class='fa fa-check-circle'></i>" } );

            Controls.Add( _hfGroupTypeGuid );
            Controls.Add( _lblAreaRowName );

            Controls.Add( _lbAddArea );
            Controls.Add( _lbAddGroup );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "data-key", _hfGroupTypeGuid.Value );
            writer.RenderBeginTag( HtmlTextWriterTag.Li );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, string.Format( "checkin-item{0} checkin-area rollover-container", Selected ? " checkin-item-selected" : "" ) );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ID + "_section" );
            writer.RenderBeginTag( "section" );

            // Hidden Field to track expansion
            _hfExpanded.RenderControl( writer );

            writer.WriteLine( "<a class='checkin-area-reorder'><i class='fa fa-bars'></i></a>" );
            writer.WriteLine( "<a class='checkin-area-expand'><i class='checkin-area-state fa fa-folder-open'></i></a>" );

            _lblAreaRowName.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right checkin-item-actions rollover-item" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _lbAddArea.RenderControl( writer );
            writer.Write( " " );
            _lbAddGroup.RenderControl( writer );

            writer.RenderEndTag();  // Div
            writer.RenderEndTag();  // Section

            if ( !Expanded )
            {
                writer.AddStyleAttribute( "display", "none" );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Render child area rows
            var areaRows = this.Controls.OfType<CheckinAreaRow>();
            if ( areaRows.Any() )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "checkin-list js-checkin-area-list" );
                writer.RenderBeginTag( HtmlTextWriterTag.Ul );
                foreach ( var areaRow in areaRows )
                {
                    areaRow.RenderControl( writer );
                }
                writer.RenderEndTag();
            }
            
            // Render child group rows
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
        /// Handles the Click event of the lbAddArea control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddArea_Click( object sender, EventArgs e )
        {
            if ( AddAreaClick != null )
            {
                AddAreaClick( this, e );
            }
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
        /// Handles the Click event of the lblDeleteArea control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lblDeleteArea_Click( object sender, EventArgs e )
        {
            if ( DeleteAreaClick != null )
            {
                DeleteAreaClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [add area click].
        /// </summary>
        public event EventHandler AddAreaClick;

        /// <summary>
        /// Occurs when [add group click].
        /// </summary>
        public event EventHandler AddGroupClick;

        /// <summary>
        /// Occurs when [delete area click].
        /// </summary>
        public event EventHandler DeleteAreaClick;

    }
}