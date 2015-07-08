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
        /// The optional checkbox which can be used to disable/enable the filter for the current run of the report
        /// </summary>
        public RockCheckBox cbIncludeFilter;

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

            ReportingHelper.RegisterJavascriptInclude( this );
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
        /// Gets or sets a value indicating whether [hide filter type picker].
        /// </summary>
        /// <value>
        /// <c>true</c> if [hide filter type picker]; otherwise, <c>false</c>.
        /// </value>
        public bool HideFilterTypePicker
        {
            get
            {
                return ViewState["HideFilterTypePicker"] as bool? ?? false;
            }
            set
            {
                ViewState["HideFilterTypePicker"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [hide filter criteria].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide filter criteria]; otherwise, <c>false</c>.
        /// </value>
        public bool HideFilterCriteria
        {
            get
            {
                return ViewState["HideFilterCriteria"] as bool? ?? false;
            }
            set
            {
                ViewState["HideFilterCriteria"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show a checkbox that enables/disables the filter for the current run
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show checkbox]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCheckbox
        {
            get
            {
                return ViewState["ShowCheckbox"] as bool? ?? false;
            }
            set
            {
                ViewState["ShowCheckbox"] = value;
            }
        }

        /// <summary>
        /// Gets whether the Checkbox is checked or not (not factoring in if it is showing)
        /// </summary>
        /// <value>
        /// The CheckBox checked.
        /// </value>
        public bool? CheckBoxChecked
        {
            get
            {
                if ( cbIncludeFilter != null )
                {
                    return cbIncludeFilter.Checked;
                }

                return null;
            }
        }

        /// <summary>
        /// Sets the CheckBox checked (if it is showing)
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetCheckBoxChecked( bool value )
        {
            EnsureChildControls();

            if ( cbIncludeFilter != null )
            {
                cbIncludeFilter.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string Label
        {
            get
            {
                return ViewState["Label"] as string;
            }
            set
            {
                ViewState["Label"] = value;
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
        [Obsolete("Use GetSelection or SetSelection instead")]
        public string Selection
        {
            get
            {
                return GetSelection();
            }
            
            set
            {
                SetSelection( value );
            }
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetSelection( string value )
        {
            EnsureChildControls();

            var component = Rock.Reporting.DataFilterContainer.GetComponent( FilterEntityTypeName );
            if ( component != null )
            {
                component.SetSelection( FilteredEntityType, filterControls, value );
            }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <returns></returns>
        public string GetSelection()
        {
            EnsureChildControls();

            var component = Rock.Reporting.DataFilterContainer.GetComponent( FilterEntityTypeName );
            if ( component != null )
            {
                return component.GetSelection( FilteredEntityType, filterControls );
            }

            return string.Empty;
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

            cbIncludeFilter = new RockCheckBox();
            cbIncludeFilter.ContainerCssClass = "filterfield-checkbox";
            Controls.Add( cbIncludeFilter );
            cbIncludeFilter.ID = this.ID + "_cbIncludeFilter";
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

            if ( !this.HideFilterTypePicker )
            {
                // only render this stuff if the filter type picker is shown
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget filter-item" );

                writer.RenderBeginTag( "article" );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix" );
                if ( !string.IsNullOrEmpty( clientFormatString ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Onclick, clientFormatString );
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
                writer.Write( component != null ? component.FormatSelection( FilteredEntityType, this.GetSelection() ) : "Select Filter" );
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
            }

            writer.AddAttribute( "class", "row js-filter-row filterfield" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( "class", "col-md-12" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            
            if ( ShowCheckbox )
            {
                //// EntityFieldFilter renders the checkbox itself (see EntityFieldFilter.cs), 
                //// so only render the checkbox if we are hiding filter criteria and it isn't an entity field filter
                if ( !( component is Rock.Reporting.DataFilter.EntityFieldFilter ) || HideFilterCriteria)
                {
                    cbIncludeFilter.Text = this.Label;
                    cbIncludeFilter.RenderControl( writer );
                }
            }
            else if ( !string.IsNullOrWhiteSpace( this.Label ) )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                writer.AddAttribute( HtmlTextWriterAttribute.For, this.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Label );
                writer.Write( Label );
                writer.RenderEndTag();  // label
            }

            if ( component != null && !HideFilterCriteria )
            {
                component.RenderControls( FilteredEntityType, this, writer, filterControls );
            }

            writer.RenderEndTag(); // "col-md-12"
            writer.RenderEndTag(); // "row js-filter-row filter-row"

            if ( !HideFilterTypePicker )
            {
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
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