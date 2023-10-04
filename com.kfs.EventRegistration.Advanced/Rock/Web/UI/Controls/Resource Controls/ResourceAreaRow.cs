using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace com.kfs.EventRegistration.Advanced
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:ResourceAreaRow runat=server></{0}:ResourceAreaRow>" )]
    public class ResourceAreaRow : CompositeControl
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
        public ResourceAreaRow ParentRow
        {
            get
            {
                return this.Parent as ResourceAreaRow;
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
        /// Gets or sets a value indicating whether to enable adding areas.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable adding area]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableAddAreas
        {
            get
            {
                bool? b = ViewState["EnableAddAreas"] as bool?;
                return ( b == null ) ? false : b.Value;
            }

            set
            {
                ViewState["EnableAddAreas"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to enable adding groups.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled or not set; otherwise, <c>false</c>.
        /// </value>
        public bool EnableAddGroups
        {
            get
            {
                bool? b = ViewState["EnableAddGroups"] as bool?;
                return ( b == null ) ? true : b.Value;
            }

            set
            {
                ViewState["EnableAddGroups"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ResourceAreaRow"/> is expanded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled or not set; otherwise, <c>false</c>.
        /// </value>
        public bool Expanded
        {
            get
            {
                EnsureChildControls();
                return _hfExpanded.Value.AsBooleanOrNull() ?? true;
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
// fix so that the Remove button will fire its event, but not the parent event
$('.resource-area a.btn-danger').click(function (event) {
    event.stopImmediatePropagation();
    if ( isDirty() ) {{
        return false;
    }}
});

// fix so that the Reorder button will fire its event, but not the parent event
$('.resource-area a.resource-area-reorder').click(function (event) {
    event.stopImmediatePropagation();
    if ( isDirty() ) {{
        return false;
    }}
});

// fix so that the Add Sub-Area button will fire its event, but not the parent event
$('.resource-area a.resource-area-add-area').click(function (event) {
    event.stopImmediatePropagation();
    if ( isDirty() ) {{
        return false;
    }}
});

// fix so that the Add Resource Group button will fire its event, but not the parent event
$('.resource-area a.resource-area-add-group').click(function (event) {
    event.stopImmediatePropagation();
    if ( isDirty() ) {{
        return false;
    }}
});";

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "ResourceAreaRow", script, true );
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

            Controls.Add( _hfGroupTypeGuid );

            _lblAreaRowName = new Label();
            _lblAreaRowName.ClientIDMode = ClientIDMode.Static;
            _lblAreaRowName.ID = this.ID + "_lblAreaRowName";

            Controls.Add( _lblAreaRowName );

            if ( EnableAddAreas )
            {
                _lbAddArea = new LinkButton();
                _lbAddArea.ID = this.ID + "_lblbAddArea";
                _lbAddArea.CssClass = "btn btn-xs btn-default resource-area-add-area";
                _lbAddArea.Click += lbAddArea_Click;
                _lbAddArea.CausesValidation = false;
                _lbAddArea.ToolTip = "Add New Area";
                _lbAddArea.Controls.Add( new LiteralControl { Text = "<i class='fa fa-plus'></i> <i class='fa fa-folder-open'></i>" } );
                Controls.Add( _lbAddArea );
            }

            if ( EnableAddGroups )
            {
                _lbAddGroup = new LinkButton();
                _lbAddGroup.ID = this.ID + "_lbAddGroup";
                _lbAddGroup.CssClass = "btn btn-xs btn-default resource-area-add-group";
                _lbAddGroup.Click += lbAddGroup_Click;
                _lbAddGroup.CausesValidation = false;
                _lbAddGroup.ToolTip = "Add New Group";
                _lbAddGroup.Controls.Add( new LiteralControl { Text = "<i class='fa fa-plus'></i> <i class='fa fa-check-circle'></i>" } );

                Controls.Add( _lbAddGroup );
            }
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "data-key", _hfGroupTypeGuid.Value );
            writer.RenderBeginTag( HtmlTextWriterTag.Li );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, string.Format( "resource-item{0} resource-area rollover-container", Selected ? " resource-item-selected" : "" ) );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ID + "_section" );
            writer.RenderBeginTag( "section" );

            // Hidden Field to track expansion
            _hfExpanded.RenderControl( writer );

            writer.WriteLine( "<a class='resource-area-reorder'><i class='fa fa-bars'></i></a>" );
            writer.WriteLine( "<a class='resource-area-expand'><i class='resource-area-state fa fa-folder-open'></i></a>" );

            _lblAreaRowName.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right resource-item-actions rollover-item" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _lbAddArea?.RenderControl( writer );
            writer.Write( " " );

            if ( _lbAddGroup != null )
            {
                _lbAddGroup.RenderControl( writer );
            }

            writer.RenderEndTag();  // Div
            writer.RenderEndTag();  // Section

            if ( !Expanded )
            {
                writer.AddStyleAttribute( "display", "none" );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Render child area rows
            var areaRows = this.Controls.OfType<ResourceAreaRow>();
            if ( areaRows.Any() )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "resource-list js-resource-area-list" );
                writer.RenderBeginTag( HtmlTextWriterTag.Ul );
                foreach ( var areaRow in areaRows )
                {
                    areaRow.RenderControl( writer );
                }
                writer.RenderEndTag();
            }

            // Render child group rows
            var groupRows = this.Controls.OfType<ResourceGroupRow>();
            if ( groupRows.Any() )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "resource-list js-resource-group-list" );
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
