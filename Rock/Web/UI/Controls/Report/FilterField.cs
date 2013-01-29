//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;

namespace Rock.Web.UI.Controls.Report
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:FilterField runat=server></{0}:FilterField>" )]
    public class FilterField : CompositeControl
    {
        private Control[] filterControls;
        private LinkButton lbDelete;

        /// <summary>
        /// Gets or sets the name of the entity type.
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        [
        Category( "Behavior" ),
        DefaultValue( "0" ),
        Description( "Entity Type Name" )
        ]
        public string EntityTypeName
        {
            get
            {
                return ViewState["EntityTypeName"] as string;
            }

            set
            {
                ViewState["EntityTypeName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the selection.
        /// </summary>
        /// <value>
        /// The selection.
        /// </value>
        [
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "Filter Selection" )
        ]
        public string Selection
        {
            get
            {
                EnsureChildControls();

                var component = Rock.Reporting.FilterContainer.GetComponent( EntityTypeName );
                if ( component != null )
                {
                    return component.GetSelection( filterControls );
                }

                return string.Empty;
            }

            set
            {
                EnsureChildControls();

                var component = Rock.Reporting.FilterContainer.GetComponent( EntityTypeName );
                if ( component != null )
                {
                    component.SetSelection( filterControls, value );
                }
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            var component = Rock.Reporting.FilterContainer.GetComponent( EntityTypeName );
            if ( component != null )
            {
                filterControls = component.CreateChildControls();
            }

            foreach ( var filterControl in filterControls )
            {
                Controls.Add( filterControl );
            }

            lbDelete = new LinkButton();
            Controls.Add( lbDelete );
            lbDelete.CssClass = "btn btn-danger btn-mini";
            lbDelete.ToolTip = "Delete";
            lbDelete.Click += lbDelete_Click;

            var i = new HtmlGenericControl( "i" );
            lbDelete.Controls.Add( i );
            i.AddCssClass( "icon-remove" );
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            var component = Rock.Reporting.FilterContainer.GetComponent( EntityTypeName );
            if ( component != null )
            {
                lbDelete.RenderControl( writer );

                writer.AddAttribute( "class", "report-filter-caption" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( component.FormatSelection( Selection ) );
                writer.RenderEndTag();

                writer.AddAttribute( "class", "report-filter-edit" );
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                component.RenderControls( writer, filterControls );
                writer.RenderEndTag();
            }
        }

        void lbDelete_Click( object sender, EventArgs e )
        {
            if ( DeleteClick != null )
            {
                DeleteClick( this, e );
            }
        }

        public event EventHandler DeleteClick;
    }
}