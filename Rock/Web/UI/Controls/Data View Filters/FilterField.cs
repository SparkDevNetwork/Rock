// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Reporting;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:FilterField runat=server></{0}:FilterField>" )]
    public class FilterField : CompositeControl
    {
        Dictionary<string, Dictionary<string, string>> AuthorizedComponents;

        /// <summary>
        /// The filter type dropdown
        /// </summary>
        protected RockDropDownList ddlFilterType;

        /// <summary>
        /// The delte button
        /// </summary>
        protected LinkButton lbDelete;

        /// <summary>
        /// The hidden field for tracking expanded
        /// </summary>
        protected HiddenField hfExpanded;

        /// <summary>
        /// The filter controls
        /// </summary>
        protected Control[] filterControls;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
// activity animation
$('.filter-item > header').click(function () {
    $(this).siblings('.panel-body').slideToggle();
    $(this).children('div.pull-left').children('div').slideToggle();

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    $('a.filter-view-state > i', this).toggleClass('fa-chevron-down');
    $('a.filter-view-state > i', this).toggleClass('fa-chevron-up');
});

// fix so that the Remove button will fire its event, but not the parent event 
$('.filter-item a.btn-danger').click(function (event) {
    event.stopImmediatePropagation();
});

$('.filter-item-select').click(function (event) {
    event.stopImmediatePropagation();
});

";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "FilterFieldEditorScript", script, true );
        }

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
                        AuthorizedComponents = HttpContext.Current.Items[itemKey] as Dictionary<string, Dictionary<string, string>>;
                    }
                    else
                    {
                        AuthorizedComponents = new Dictionary<string, Dictionary<string, string>>();
                        RockPage rockPage = this.Page as RockPage;
                        if ( rockPage != null )
                        {
                            foreach ( var component in DataFilterContainer.GetComponentsByFilteredEntityName( value ).OrderBy( c => c.Order ).ThenBy( c => c.Section ).ThenBy( c => c.GetTitle( FilteredEntityType ) ) )
                            {
                                if ( component.IsAuthorized( Authorization.VIEW, rockPage.CurrentPerson ) )
                                {
                                    if ( !AuthorizedComponents.ContainsKey( component.Section ) )
                                    {
                                        AuthorizedComponents.Add( component.Section, new Dictionary<string, string>() );
                                    }

                                    AuthorizedComponents[component.Section].Add( component.TypeName, component.GetTitle( FilteredEntityType ) );
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
        /// Gets or sets the data view filter unique identifier.
        /// </summary>
        /// <value>
        /// The data view filter unique identifier.
        /// </value>
        public Guid DataViewFilterGuid
        {
            get
            {
                return ViewState["DataViewFilterGuid"] as Guid? ?? Guid.NewGuid();
            }

            set
            {
                ViewState["DataViewFilterGuid"] = value;
            }
        }

        /// <summary>
        /// Gets the type of the filtered entity.
        /// </summary>
        /// <value>
        /// The type of the filtered entity.
        /// </value>
        public Type FilteredEntityType
        {
            get
            {
                var entityTypeCache = EntityTypeCache.Read( FilteredEntityTypeName );
                if ( entityTypeCache != null )
                {
                    return entityTypeCache.GetEntityType();
                }

                return null;
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
                return ViewState["FilterEntityTypeName"] as string ?? "Rock.Reporting.DataFilter.PropertyFilter";
            }
            set
            {
                ViewState["FilterEntityTypeName"] = value;
                RecreateChildControls();
            }
        }

        /// <summary>
        /// Gets or sets optional key/value filter options.
        /// </summary>
        /// <value>
        /// The filter options.
        /// </value>
        public Dictionary<string, object> FilterOptions
        {
            get
            {
                return ViewState["FilterOptions"] as Dictionary<string, object>;
            }

            set
            {
                ViewState["FilterOptions"] = value;
                RecreateChildControls();
            }
        }

        /// <summary>
        /// Gets or sets the excluded filter types.
        /// </summary>
        /// <value>
        /// The excluded filter types.
        /// </value>
        public string[] ExcludedFilterTypes
        {
            get
            {
                return ViewState["ExcludedFilterTypes"] as string[] ?? new string[] { };
            }
            set
            {
                ViewState["ExcludedFilterTypes"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether filter type can be configured
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is filter type configurable; otherwise, <c>false</c>.
        /// </value>
        public bool IsFilterTypeConfigurable
        {
            get
            {
                return ViewState["IsFilterTypeConfigurable"] as bool? ?? true;
            }
            set
            {
                ViewState["IsFilterTypeConfigurable"] = value;
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
                if ( !bool.TryParse( hfExpanded.Value, out expanded ) )
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

                var component = Rock.Reporting.DataFilterContainer.GetComponent( FilterEntityTypeName );
                if ( component != null )
                {
                    return component.GetSelection( FilteredEntityType, filterControls );
                }

                return string.Empty;
            }

            set
            {
                EnsureChildControls();

                var component = Rock.Reporting.DataFilterContainer.GetComponent( FilterEntityTypeName );
                if ( component != null )
                {
                    component.SetSelection( FilteredEntityType, filterControls, value );
                }
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            ddlFilterType = new RockDropDownList();
            Controls.Add( ddlFilterType );
            ddlFilterType.ID = this.ID + "_ddlFilter";

            var component = Rock.Reporting.DataFilterContainer.GetComponent( FilterEntityTypeName );
            if ( component != null )
            {
                component.Options = FilterOptions;
                filterControls = component.CreateChildControls( FilteredEntityType, this );
            }
            else
            {
                filterControls = new Control[0];
            }

            ddlFilterType.AutoPostBack = true;
            ddlFilterType.SelectedIndexChanged += ddlFilterType_SelectedIndexChanged;

            ddlFilterType.Items.Clear();

            foreach ( var section in AuthorizedComponents )
            {
                foreach ( var item in section.Value )
                {
                    if ( !this.ExcludedFilterTypes.Any( a => a == item.Key ) )
                    {
                        ListItem li = new ListItem( item.Value, item.Key );

                        if ( !string.IsNullOrWhiteSpace( section.Key ) )
                        {
                            li.Attributes.Add( "optiongroup", section.Key );
                        }

                        var filterComponent = Rock.Reporting.DataFilterContainer.GetComponent( item.Key );
                        if ( filterComponent != null )
                        {
                            string description = Reflection.GetDescription( filterComponent.GetType() );
                            if ( !string.IsNullOrWhiteSpace( description ) )
                            {
                                li.Attributes.Add( "title", description );
                            }
                        }

                        li.Selected = item.Key == FilterEntityTypeName;
                        ddlFilterType.Items.Add( li );
                    }
                }
            }

            hfExpanded = new HiddenField();
            Controls.Add( hfExpanded );
            hfExpanded.ID = this.ID + "_hfExpanded";
            hfExpanded.Value = "True";

            lbDelete = new LinkButton();
            Controls.Add( lbDelete );
            lbDelete.ID = this.ID + "_lbDelete";
            lbDelete.CssClass = "btn btn-xs btn-danger ";
            lbDelete.Click += lbDelete_Click;
            lbDelete.CausesValidation = false;

            var iDelete = new HtmlGenericControl( "i" );
            lbDelete.Controls.Add( iDelete );
            iDelete.AddCssClass( "fa fa-times" );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            DataFilterComponent component = null;
            string clientFormatString = string.Empty;
            if ( !string.IsNullOrWhiteSpace( FilterEntityTypeName ) )
            {
                component = Rock.Reporting.DataFilterContainer.GetComponent( FilterEntityTypeName );
                if ( component != null )
                {
                    clientFormatString =
                       string.Format( "if ($(this).find('.filter-view-state').children('i').hasClass('fa-chevron-up')) {{ var $article = $(this).parents('article:first'); var $content = $article.children('div.panel-body'); $article.find('div.filter-item-description:first').html({0}); }}", component.GetClientFormatSelection( FilteredEntityType ) );
                }
            }

            if ( component == null )
            {
                hfExpanded.Value = "True";
            }

            if ( this.IsFilterTypeConfigurable )
            {
                // don't style this as a panel if the filter type can't be configured
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget filter-item" );
            }

            writer.RenderBeginTag( "article" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix" );
            if ( !string.IsNullOrEmpty( clientFormatString ) )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Onclick, clientFormatString );
            }

            if ( !this.IsFilterTypeConfigurable )
            {
                // hide the header if the filter type can't be configured
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }

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
            writer.Write( component != null ? component.FormatSelection( FilteredEntityType, Selection ) : "Select Filter" );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-item-select" );
            if ( !Expanded )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.Write( "Filter Type " );
            writer.RenderEndTag();

            ddlFilterType.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();


            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-link btn-xs filter-view-state" );
            writer.RenderBeginTag( HtmlTextWriterTag.A );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, Expanded ? "fa fa-chevron-up" : "fa fa-chevron-down" );
            writer.RenderBeginTag( HtmlTextWriterTag.I );
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.Write( " " );
            lbDelete.Visible = ( this.DeleteClick != null );
            lbDelete.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute( "class", "panel-body" );
            if ( !Expanded )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            if ( component != null )
            {
                component.RenderControls( FilteredEntityType, this, writer, filterControls );
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

        /// <summary>
        /// Occurs when [selection changed].
        /// </summary>
        public event EventHandler SelectionChanged;


    }
}