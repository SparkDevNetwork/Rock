//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:FilterGroup runat=server></{0}:FilterGroup>" )]
    public class FilterGroup : CompositeControl
    {
        Toggle toggleAllAny;
        HtmlButton btnAddFilter;
        HtmlButton btnAddGroup;
        LinkButton lbDelete;

        /// <summary>
        /// Gets or sets the name of entity type that is being filtered.
        /// </summary>
        /// <value>
        /// The name of the filtered entity type.
        /// </value>
        public string FilteredEntityTypeName
        {
            get
            {
                return ViewState["FilteredEntityTypeName"] as string;
            }

            set
            {
                ViewState["FilteredEntityTypeName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the filter.
        /// </summary>
        /// <value>
        /// The type of the filter.
        /// </value>
        public FilterExpressionType FilterType
        {
            get
            {
                EnsureChildControls();
                return toggleAllAny.Checked ? FilterExpressionType.GroupAll : FilterExpressionType.GroupAny;
            }

            set
            {
                if ( value != FilterExpressionType.Filter )
                {
                    EnsureChildControls();
                    toggleAllAny.Checked = value == FilterExpressionType.GroupAll;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is delete enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is delete enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeleteEnabled
        {
            get
            {
                bool? b = ViewState["IsDeleteEnabled"] as bool?;
                return ( b == null ) ? true : b.Value;
            }
            set
            {
                ViewState["IsDeleteEnabled"] = value;
            }
        }


        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            toggleAllAny = new Toggle();
            Controls.Add( toggleAllAny );
            toggleAllAny.AddCssClass( "switch-mini" );
            toggleAllAny.OnText = "All";
            toggleAllAny.OffText = "Any";

            btnAddGroup = new HtmlButton();
            Controls.Add( btnAddGroup );
            btnAddGroup.ServerClick += btnAddGroup_ServerClick;
            btnAddGroup.AddCssClass( "btn btn-inverse" );

            var iAddGroup = new HtmlGenericControl( "i" );
            iAddGroup.AddCssClass( "icon-list-alt" );
            btnAddGroup.Controls.Add( iAddGroup );
            btnAddGroup.Controls.Add( new LiteralControl( " Add Filter Group" ) );

            btnAddFilter = new HtmlButton();
            Controls.Add( btnAddFilter );
            btnAddFilter.ServerClick += btnAddFilter_ServerClick;
            btnAddFilter.AddCssClass( "btn btn-inverse" );

            var iAddFilter = new HtmlGenericControl( "i" );
            iAddFilter.AddCssClass( "icon-filter" );
            btnAddFilter.Controls.Add( iAddFilter );
            btnAddFilter.Controls.Add( new LiteralControl( " Add Filter" ) );

            lbDelete = new LinkButton();
            Controls.Add( lbDelete );
            lbDelete.Click += lbDelete_Click;
            lbDelete.AddCssClass( "btn btn-mini btn-danger" );

            var iDeleteGroup = new HtmlGenericControl( "i" );
            iDeleteGroup.AddCssClass( "icon-remove" );
            lbDelete.Controls.Add( iDeleteGroup );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget widget-dark" );
            writer.RenderBeginTag( "section" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "clearfix" );
            writer.RenderBeginTag( "header" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-toogle pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( "Show if " );
            toggleAllAny.RenderControl( writer );
            writer.Write( " of these are true" );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn-group pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            btnAddGroup.RenderControl( writer );

            btnAddFilter.RenderControl( writer );

            if ( IsDeleteEnabled )
            {
                lbDelete.Visible = true;
                lbDelete.RenderControl( writer );
            }
            else
            {
                lbDelete.Visible = false;
            }

            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget-content" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            foreach ( Control control in this.Controls )
            {
                if ( control is FilterGroup || control is FilterField )
                {
                    control.RenderControl( writer );
                }
            }

            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        void btnAddGroup_ServerClick( object sender, EventArgs e )
        {
            if ( AddGroupClick != null )
            {
                AddGroupClick( this, e );
            }
        }

        void btnAddFilter_ServerClick( object sender, EventArgs e )
        {
            if ( AddFilterClick != null )
            {
                AddFilterClick( this, e );
            }
        }

        void lbDelete_Click( object sender, EventArgs e )
        {
            if ( DeleteGroupClick != null )
            {
                DeleteGroupClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [add filter click].
        /// </summary>
        public event EventHandler AddFilterClick;
        
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