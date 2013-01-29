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
using Rock.Model;

namespace Rock.Web.UI.Controls.Report
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:FilterGroup runat=server></{0}:FilterGroup>" )]
    public class FilterGroup : CompositeControl
    {
        DropDownList ddlFilter;
        Button btnAddFilter;
        RadioButtonList rblAndOr;
        LinkButton lbAddGroup;
        LinkButton lbDelete;

        [
        Category( "Behavior" ),
        DefaultValue( "And" ),
        Description( "Type of Filter (And/Or)" )
        ]
        public FilterType FilterType
        {
            get
            {
                EnsureChildControls();
                return rblAndOr.SelectedValue.ConvertToEnum<FilterType>();
            }

            set
            {
                if ( value != Model.FilterType.Expression )
                {
                    EnsureChildControls();
                    rblAndOr.SelectedValue = value.ConvertToString();
                }
            }
        }

        [
        Category( "Behavior" ),
        DefaultValue( true ),
        Description( "Delete Enabled" )
        ]
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
            ddlFilter = new DropDownList();
            ddlFilter.AddCssClass( "span5" );
            foreach ( var serviceEntry in Rock.Reporting.FilterContainer.Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                ListItem li = new ListItem( component.Title, component.GetType().FullName );
                ddlFilter.Items.Add( li );
            }
            Controls.Add( ddlFilter );

            btnAddFilter = new Button();
            btnAddFilter.AddCssClass( "btn" );
            btnAddFilter.Text = "Add";
            btnAddFilter.Click += btnAddFilter_Click;
            Controls.Add( btnAddFilter );

            rblAndOr = new RadioButtonList();
            rblAndOr.RepeatLayout = RepeatLayout.Flow;
            rblAndOr.RepeatDirection = RepeatDirection.Horizontal;
            rblAndOr.Items.Add( new ListItem( "And", "And" ) );
            rblAndOr.Items.Add( new ListItem( "Or", "Or" ) );
            Controls.Add( rblAndOr );

            lbAddGroup = new LinkButton();
            lbAddGroup.AddCssClass( "btn" );
            lbAddGroup.Text = "Add New Group";
            lbAddGroup.Click += lbAddGroup_Click;
            Controls.Add( lbAddGroup );

            lbDelete = new LinkButton();
            Controls.Add( lbDelete );
            lbDelete.CssClass = "btn btn-danger btn-mini";
            lbDelete.ToolTip = "Delete";
            lbDelete.Click += lbDelete_Click;

            var i = new HtmlGenericControl( "i" );
            lbDelete.Controls.Add( i );
            i.AddCssClass( "icon-remove" );
        }

        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "class", "well well-small" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            lbDelete.Visible = IsDeleteEnabled;
            lbDelete.RenderControl( writer );

            writer.AddAttribute( "class", "report-filters-heading" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute("class", "controls input-append");
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            ddlFilter.RenderControl( writer );
            btnAddFilter.RenderControl( writer );
            writer.RenderEndTag();
            rblAndOr.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "report-filters" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            foreach ( Control control in this.Controls )
            {
                if ( control is FilterGroup || control is FilterField )
                {
                    control.RenderControl( writer );
                }
            }
            writer.RenderEndTag();

            lbAddGroup.RenderControl( writer );

            writer.RenderEndTag();
        }

        void btnAddFilter_Click( object sender, EventArgs e )
        {
            if ( AddFilterClick != null )
            {
                AddFilterArgs args = new AddFilterArgs();
                args.EntityTypeName = ddlFilter.SelectedValue;
                AddFilterClick( this, args );
            }
        }

        void lbAddGroup_Click( object sender, EventArgs e )
        {
            if ( AddGroupClick != null )
            {
                AddGroupClick( this, e );
            }
        }

        void lbDelete_Click( object sender, EventArgs e )
        {
            if ( DeleteGroupClick != null )
            {
                DeleteGroupClick( this, e );
            }
        }

        public event EventHandler<AddFilterArgs> AddFilterClick;
        public event EventHandler AddGroupClick;
        public event EventHandler DeleteGroupClick;

    }

    public class AddFilterArgs : EventArgs
    {
        public string EntityTypeName { get; set; }
    }
}