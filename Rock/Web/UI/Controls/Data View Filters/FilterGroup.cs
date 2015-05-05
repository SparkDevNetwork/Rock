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
        LinkButton btnAddFilter;
        LinkButton btnAddGroup;
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
        /// Gets or sets a value indicating whether [hide panel header].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide panel header]; otherwise, <c>false</c>.
        /// </value>
        public bool HidePanelHeader
        {
            get
            {
                return ViewState["HidePanelHeader"] as bool? ?? false;
            }

            set
            {
                ViewState["HidePanelHeader"] = value;
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
            toggleAllAny.ID = this.ID + "_toggleAllAny";
            toggleAllAny.ButtonSizeCssClass = "btn-xs";
            toggleAllAny.OnText = "All";
            toggleAllAny.OffText = "Any";
            toggleAllAny.ActiveButtonCssClass = "btn-info";

            btnAddGroup = new LinkButton();
            Controls.Add( btnAddGroup );
            btnAddGroup.ID = this.ID + "_btnAddGroup";
            btnAddGroup.Click += btnAddGroup_ServerClick;
            btnAddGroup.AddCssClass( "btn btn-action" );
            btnAddGroup.CausesValidation = false;

            var iAddGroup = new HtmlGenericControl( "i" );
            iAddGroup.AddCssClass( "fa fa-list-alt" );
            btnAddGroup.Controls.Add( iAddGroup );
            btnAddGroup.Controls.Add( new LiteralControl( " Add Filter Group" ) );

            btnAddFilter = new LinkButton();
            Controls.Add( btnAddFilter );
            btnAddFilter.ID = this.ID + "_btnAddFilter";
            btnAddFilter.Click += btnAddFilter_ServerClick;
            btnAddFilter.AddCssClass( "btn btn-action" );
            btnAddFilter.CausesValidation = false;

            var iAddFilter = new HtmlGenericControl( "i" );
            iAddFilter.AddCssClass( "fa fa-filter" );
            btnAddFilter.Controls.Add( iAddFilter );
            btnAddFilter.Controls.Add( new LiteralControl( " Add Filter" ) );

            lbDelete = new LinkButton();
            Controls.Add( lbDelete );
            lbDelete.ID = this.ID + "_lbDelete";
            lbDelete.Click += lbDelete_Click;
            lbDelete.AddCssClass( "btn btn-xs btn-danger" );
            lbDelete.CausesValidation = false;

            var iDeleteGroup = new HtmlGenericControl( "i" );
            iDeleteGroup.AddCssClass( "fa fa-times" );
            lbDelete.Controls.Add( iDeleteGroup );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget" );
            writer.RenderBeginTag( "section" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix" );
            if ( HidePanelHeader )
            {
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
            }

            writer.RenderBeginTag( "header" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-toggle pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.Write( "Show if" );
            writer.RenderEndTag();
            toggleAllAny.CssClass = "pull-left";
            toggleAllAny.RenderControl( writer );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.Write( "of these are true" );
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn-group btn-group-sm pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            btnAddGroup.Visible = ( AddGroupClick != null );
            btnAddGroup.RenderControl( writer );

            btnAddFilter.Visible = ( AddFilterClick != null );
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

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
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