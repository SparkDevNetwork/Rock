//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.DataFilters;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:FilterField runat=server></{0}:FilterField>" )]
    public class FilterField : CompositeControl
    {
        Dictionary<string, string> AuthorizedComponents;

        protected DropDownList ddlFilterType;
        protected LinkButton lbDelete;
        protected HiddenField hfExpanded;
        protected Control[] filterControls;

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

                AuthorizedComponents = null;

                if ( !string.IsNullOrWhiteSpace( value ) )
                {
                    string itemKey = "FilterFieldComponents:" + value;
                    if ( HttpContext.Current.Items.Contains( itemKey ) )
                    {
                        AuthorizedComponents = HttpContext.Current.Items[itemKey] as Dictionary<string, string>;
                    }
                    else
                    {
                        AuthorizedComponents = new Dictionary<string, string>();
                        RockPage rockPage = this.Page as RockPage;
                        if ( rockPage != null )
                        {

                            foreach ( var component in DataFilterContainer.GetComponentsByFilteredEntityName( value ) )
                            {
                                if ( component.IsAuthorized( "View", rockPage.CurrentPerson ) )
                                {
                                    AuthorizedComponents.Add( component.TypeName, component.Title );
                                }
                            }

                        }

                        HttpContext.Current.Items.Add( itemKey, AuthorizedComponents );
                    }
                }

                RecreateChildControls();
            }
        }

        /// <summary>
        /// Gets or sets the name of the filter entity type.  This is a DataFilter type
        /// that applies to the FilteredEntityType
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public string FilterEntityTypeName
        {
            get
            {
                return ViewState["FilterEntityTypeName"] as string;
            }

            set
            {
                ViewState["FilterEntityTypeName"] = value;
                RecreateChildControls();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FilterField" /> is expanded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expanded; otherwise, <c>false</c>.
        /// </value>
        public bool Expanded
        {
            get
            {
                EnsureChildControls();

                bool expanded = true;
                if (!bool.TryParse(hfExpanded.Value, out expanded))
                    expanded = true;
                return expanded;
            }
            set
            {
                EnsureChildControls();
                hfExpanded.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the selection.
        /// </summary>
        /// <value>
        /// The selection.
        /// </value>
        public string Selection
        {
            get
            {
                EnsureChildControls();

                var component = Rock.DataFilters.DataFilterContainer.GetComponent( FilterEntityTypeName );
                if ( component != null )
                {
                    return component.GetSelection( filterControls );
                }

                return string.Empty;
            }

            set
            {
                EnsureChildControls();

                var component = Rock.DataFilters.DataFilterContainer.GetComponent( FilterEntityTypeName );
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
            Controls.Clear();

            ddlFilterType = new DropDownList();
            Controls.Add( ddlFilterType );
            ddlFilterType.ID = this.ID + "_ddlFilter";

            int i = 0;
            var component = Rock.DataFilters.DataFilterContainer.GetComponent( FilterEntityTypeName );
            if ( component != null )
            {
                filterControls = component.CreateChildControls();
                if ( filterControls != null )
                {
                    foreach ( var filterControl in filterControls )
                    {
                        Controls.Add( filterControl );
                        filterControl.ID = string.Format( "{0}_fc_{1}", this.ID, i++ );
                    }
                }
            }
            else
            {
                filterControls = new Control[0];
            }

            ddlFilterType.AutoPostBack = true;
            ddlFilterType.SelectedIndexChanged += ddlFilterType_SelectedIndexChanged;

            ddlFilterType.Items.Clear();
            ddlFilterType.Items.Add( new ListItem( string.Empty ) );
            foreach ( var item in AuthorizedComponents )
            {
                ListItem li = new ListItem( item.Value, item.Key );
                li.Selected = item.Key == FilterEntityTypeName;
                ddlFilterType.Items.Add( li );
            }

            hfExpanded = new HiddenField();
            Controls.Add( hfExpanded );
            hfExpanded.ID = this.ID + "_hfExpanded";

            lbDelete = new LinkButton();
            Controls.Add( lbDelete );
            lbDelete.ID = this.ID + "_lbDelete";
            lbDelete.CssClass = "btn btn-mini btn-danger ";
            lbDelete.Click += lbDelete_Click;

            var iDelete = new HtmlGenericControl( "i" );
            lbDelete.Controls.Add( iDelete );
            iDelete.AddCssClass( "icon-remove" );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            DataFilterComponent component = null;
            if (!string.IsNullOrWhiteSpace(FilterEntityTypeName))
            {
                component = Rock.DataFilters.DataFilterContainer.GetComponent( FilterEntityTypeName );
            }

            if ( component == null )
            {
                hfExpanded.Value = "True";
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget filter-item" );
            writer.RenderBeginTag( "article" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "clearfix" );
            writer.RenderBeginTag( "header" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-expanded" );
            hfExpanded.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-item-description" );
            if ( Expanded )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( component != null ? component.FormatSelection( Selection ) : "Select Filter" );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-item-select" );
            if ( !Expanded )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( "Filter Type " );
            ddlFilterType.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-mini filter-view-state" );
            writer.RenderBeginTag( HtmlTextWriterTag.A );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, Expanded ? "icon-chevron-up" : "icon-chevron-down" );
            writer.RenderBeginTag( HtmlTextWriterTag.I );
            writer.RenderEndTag();
            writer.RenderEndTag();
            lbDelete.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute( "class", "widget-content" );
            if ( !Expanded )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            if ( component != null )
            {
                component.RenderControls( writer, filterControls );
            }
            writer.RenderEndTag();

            writer.RenderEndTag();

        }

        void ddlFilterType_SelectedIndexChanged( object sender, EventArgs e )
        {
            FilterEntityTypeName = ( (DropDownList)sender ).SelectedValue;

            if ( SelectionChanged != null )
            {
                SelectionChanged( this, e );
            }
        }
        
        void lbDelete_Click( object sender, EventArgs e )
        {
            if ( DeleteClick != null )
            {
                DeleteClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [delete click].
        /// </summary>
        public event EventHandler DeleteClick;

        public event EventHandler SelectionChanged;


    }
}