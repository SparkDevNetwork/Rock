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
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A pull-down drawer for hiding/exposing additional details on various detail blocks.  
    /// Just call SetEntity() passing in a IModel entity and the Rock RootUrl to create the
    /// the Created By and Last Modified By audit details.
    /// </summary>
    [ToolboxData( "<{0}:PanelDrawer runat=server></{0}:PanelDrawer>" )]
    public class PanelDrawer : PlaceHolder, INamingContainer
    {
        #region Properties

        /// <summary>
        /// The hidden field for tracking expanded
        /// </summary>
        private HiddenField _hfExpanded;

        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        /// <value>
        /// The entity identifier
        /// </value>
        private int? _entityId
        {
            get { return ViewState["EntityId"] as int? ?? null; }
            set { ViewState["EntityId"] = value; }
        }

        /// <summary>
        /// Gets or sets the CreatedAuditHtml.
        /// </summary>
        /// <value>
        /// The Created Audit HTML.
        /// </value>
        private string _createdAuditHtml
        {
            get { return ViewState["CreatedAuditHtml"] as string ?? string.Empty; }
            set { ViewState["CreatedAuditHtml"] = value; }
        }

        /// <summary>
        /// Gets or sets the ModifiedAuditHtml.
        /// </summary>
        /// <value>
        /// The Modified Audit HTML.
        /// </value>
        private string _modifiedAuditHtml
        {
            get { return ViewState["ModifiedAuditHtml"] as string ?? string.Empty; }
            set { ViewState["ModifiedAuditHtml"] = value; }
        }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        public string CssClass
        {
            get { return ViewState["CssClass"] as string ?? string.Empty; }
            set { ViewState["CssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the placeholder text to display inside panel drawer above any audit details.
        /// </summary>
        /// <value>
        /// The placeholder text
        /// </value>
        public string Placeholder
        {
            get { return ViewState["Placeholder"] as string ?? string.Empty; }
            set { ViewState["Placeholder"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PanelDrawer" /> is expanded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expanded; otherwise, <c>false</c>.
        /// </value>
        public bool Expanded
        {
            get
            {
                EnsureChildControls();

                bool expanded = false;
                if ( !bool.TryParse( _hfExpanded.Value, out expanded ) )
                {
                    expanded = false;
                }

                return expanded;
            }

            set
            {
                EnsureChildControls();
                _hfExpanded.Value = value.ToString();
            }
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            EnsureChildControls();
            base.OnInit( e );

            string script = @"
$('.js-drawerpull').on('click', function () {
    $( this ).closest( '.panel-drawer' ).toggleClass( 'open' );
    $( this ).siblings( '.drawer-content' ).slideToggle();

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    var icon = $( this ).find( 'i' );
    var iconOpenClass = icon.attr( 'data-icon-open' ) || 'fa fa-chevron-up';
    var iconCloseClass = icon.attr( 'data-icon-closed' ) || 'fa fa-chevron-down';

    if ($( this ).closest( '.panel-drawer' ).hasClass( 'open' )) {
        icon.attr( 'class', iconOpenClass );
    }
    else {
        icon.attr( 'class', iconCloseClass );
    }
});

$('.js-date-rollover').tooltip();
";
            ScriptManager.RegisterStartupScript( this, typeof( PanelDrawer ), "RockPanelDrawerScript", script, true );
        }

        /// <summary>
        /// Sets the Created By and Modified By details from the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="rootUrl">The Rock root URL.</param>
        public void SetEntity( IModel entity, string rootUrl )
        {
            if ( entity != null )
            {
                _entityId = entity.Id;
                _createdAuditHtml = entity.GetCreatedAuditHtml( rootUrl );
                _modifiedAuditHtml = entity.GetModifiedAuditHtml( rootUrl );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _hfExpanded = new HiddenField();
            _hfExpanded.ID = "_hfExpanded";
            Controls.Add( _hfExpanded );
            _hfExpanded.Value = "False";
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// Control is of the form:
        ///       <div id="divPanelDrawer" class="panel-drawer" runat="server">
        ///          <div class="drawer-content" style="display: none;">
        ///              PLACEHOLDER
        ///              <div class="row">
        ///                  <div class="col-md-6">
        ///                      <dl>
        ///                         <dt>Created By</dt>
        ///                         <dd>Admin Admin (6 days ago)</dd>
        ///                      </dl>
        ///                  </div>
        ///                  <div class="col-md-6">
        ///                      <dl>
        ///                         <dt>Last Modified By</dt>
        ///                         <dd>Admin Admin (46 minutes ago)</dd>
        ///                      </dl>
        ///                  </div>
        ///              </div>
        ///          </div>
        ///          <div class="drawer-pull js-drawerpull"><i class="fa fa-chevron-down" data-icon-closed="fa fa-chevron-down" data-icon-open="fa fa-chevron-up"></i></div>
        ///       </div>
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                // panel-drawer div
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-drawer rock-panel-drawer " + CssClass + ( Expanded ? "open" : string.Empty ) );
                writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // Hidden Field to track expansion
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-expanded" );
                _hfExpanded.RenderControl( writer );

                // drawer-content div
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "drawer-content" );
                if ( ! Expanded )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // PlaceHolder
                if ( ! string.IsNullOrEmpty( Placeholder ) )
                {
                    writer.Write( Placeholder );
                }

                if ( _entityId != null )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    // div col 6 with Created By
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.RenderBeginTag( HtmlTextWriterTag.Dl );
                    writer.RenderBeginTag( HtmlTextWriterTag.Dt );
                    writer.Write( "Created By" );
                    writer.RenderEndTag();
                    writer.RenderBeginTag( HtmlTextWriterTag.Dd );
                    writer.Write( _createdAuditHtml );
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                    writer.RenderEndTag();

                    // div col 6 with Modified By
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.RenderBeginTag( HtmlTextWriterTag.Dl );
                    writer.RenderBeginTag( HtmlTextWriterTag.Dt );
                    writer.Write( "Last Modified By" );
                    writer.RenderEndTag();
                    writer.RenderBeginTag( HtmlTextWriterTag.Dd );
                    writer.Write( _modifiedAuditHtml );
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                    writer.RenderEndTag();

                    writer.RenderEndTag(); // end row
                }

                writer.RenderEndTag(); // end drawer-content div

                // drawer-pull div
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "drawer-pull js-drawerpull" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, Expanded ? "fa fa-chevron-up" : "fa fa-chevron-down" );

                writer.RenderBeginTag( HtmlTextWriterTag.I );

                writer.AddAttribute( "data-icon-closed", "fa fa-chevron-down" );
                writer.AddAttribute( "data-icon-open", "fa fa-chevron-up" );
                writer.RenderEndTag();
                writer.RenderEndTag(); // end drawer-pull div

                writer.RenderEndTag(); // end panel-drawer div
            }
        }

        /// <summary>
        /// Renders the child controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected virtual void RenderChildControls( HtmlTextWriter writer )
        {
            // Render placeholder's child controls
            if ( this.Controls != null )
            {
                List<Control> alreadyRenderedControls = new List<Control>();
                alreadyRenderedControls.Add( _hfExpanded );

                foreach ( Control child in this.Controls )
                {
                    if ( !alreadyRenderedControls.Contains( child ) )
                    {
                        child.RenderControl( writer );
                    }
                }
            }
        }
    }
}