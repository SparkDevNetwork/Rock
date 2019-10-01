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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for deleting a row in a grid
    /// </summary>
    [ToolboxData( "<{0}:DeleteField runat=server></{0}:DeleteField>" )]
    public class DeleteField : RockTemplateField, INotRowSelectedField
    {
        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass
        {
            get
            {
                string iconCssClass = ViewState["IconCssClass"] as string;
                if ( string.IsNullOrWhiteSpace( iconCssClass ) )
                {
                    iconCssClass = "fa fa-times";
                    ViewState["IconCssClass"] = iconCssClass;
                }
                return iconCssClass;
            }
            set
            {
                ViewState["IconCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the button CSS class.
        /// </summary>
        /// <value>
        /// The button CSS class.
        /// </value>
        public string ButtonCssClass
        {
            get
            {
                string buttonCssClass = ViewState["ButtonCssClass"] as string;
                if ( string.IsNullOrWhiteSpace( buttonCssClass ) )
                {
                    buttonCssClass = "btn btn-danger btn-sm grid-delete-button";
                    ViewState["ButtonCssClass"] = buttonCssClass;
                }
                return buttonCssClass;
            }
            set
            {
                ViewState["ButtonCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the tooltip.
        /// </summary>
        /// <value>
        /// The tooltip.
        /// </value>
        public string Tooltip
        {
            get
            {
                string tooltip = ViewState["Tooltip"] as string;
                if ( string.IsNullOrWhiteSpace( tooltip ) )
                {
                    tooltip = "Delete";
                    ViewState["Tooltip"] = tooltip;
                }
                return tooltip;
            }
            set
            {
                ViewState["Tooltip"] = value;
            }
        }

        /// <summary>
        /// When exporting a grid with an Export source of ColumnOutput, this property controls whether a column is included
        /// in the export or not
        /// </summary>
        public override ExcelExportBehavior ExcelExportBehavior
        {
            get
            {
                return ExcelExportBehavior.NeverInclude;
            }
            set
            {
                base.ExcelExportBehavior = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteField" /> class.
        /// </summary>
        public DeleteField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.CssClass = "grid-columncommand";
            this.ItemStyle.CssClass = "grid-columncommand";
        }

        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField"/>.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            DeleteFieldTemplate deleteFieldTemplate = new DeleteFieldTemplate();
            deleteFieldTemplate.LinkButtonClick += deleteFieldTemplate_LinkButtonClick;
            this.ItemTemplate = deleteFieldTemplate;
            this.ParentGrid = control as Grid;
            return base.Initialize( sortingEnabled, control );
        }

        /// <summary>
        /// Handles the LinkButtonClick event of the deleteFieldTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void deleteFieldTemplate_LinkButtonClick( object sender, RowEventArgs e )
        {
            OnClick( e );
        }

        /// <summary>
        /// Gets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        public Grid ParentGrid { get; internal set; }

        /// <summary>
        /// Occurs when [click].
        /// </summary>
        public event EventHandler<RowEventArgs> Click;

        /// <summary>
        /// Occurs when [on data bound].
        /// </summary>
        public event EventHandler<RowEventArgs> DataBound;

        /// <summary>
        /// Handles the on data bound.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        internal void HandleOnDataBound( object sender, RowEventArgs e )
        {
            if ( this.DataBound != null )
            {
                this.DataBound( sender, e );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:Click"/> event.
        /// </summary>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        public virtual void OnClick( RowEventArgs e )
        {
            if ( Click != null )
            {
                Click( this, e );
            }
        }
    }

    /// <summary>
    /// Template used by the <see cref="DeleteField" /> control
    /// </summary>
    /// <seealso cref="System.Web.UI.ITemplate" />
    public class DeleteFieldTemplate : ITemplate
    {
        /// <summary>
        /// Gets or sets the delete field.
        /// </summary>
        /// <value>
        /// The delete field.
        /// </value>
        private DeleteField DeleteField { get; set; }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                DeleteField deleteField = cell.ContainingField as DeleteField;
                this.DeleteField = deleteField;

                // only need to do this stuff if the deleteField is actually going to be rendered onto the page
                if ( deleteField.Visible )
                {
                    ParentGrid = deleteField.ParentGrid;
                    LinkButton lbDelete = new LinkButton();
                    lbDelete.CausesValidation = false;
                    lbDelete.CssClass = deleteField.ButtonCssClass;
                    lbDelete.PreRender += ( s, e ) =>
                    {
                        if ( lbDelete.Enabled )
                        {
                            if ( !ParentGrid.Enabled || !ParentGrid.IsDeleteEnabled )
                            {
                                lbDelete.AddCssClass( "disabled" );
                                lbDelete.Enabled = false;
                            }

                            if ( lbDelete.Enabled )
                            {
                                // if the lbDelete button is Enabled, make sure delete button is registered for async postback (needed just in case the grid was created at runtime)
                                var sm = ScriptManager.GetCurrent( this.ParentGrid.Page );

                                // note: this get's slower and slower when the Grid has lots of rows (for example on an Export), so it would be nice to figure out if this is needed
                                sm.RegisterAsyncPostBackControl( lbDelete );
                            }
                        }
                    };

                    lbDelete.ToolTip = deleteField.Tooltip;

                    HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
                    buttonIcon.Attributes.Add( "class", deleteField.IconCssClass );
                    lbDelete.Controls.Add( buttonIcon );

                    lbDelete.Click += lbDelete_Click;
                    lbDelete.DataBinding += lbDelete_DataBinding;

                    cell.Controls.Add( lbDelete );
                }
            }
        }

        /// <summary>
        /// Gets or sets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        private Grid ParentGrid { get; set; }

        /// <summary>
        /// Handles the DataBinding event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDelete_DataBinding( object sender, EventArgs e )
        {
            if ( ParentGrid.HideDeleteButtonForIsSystem )
            {
                LinkButton lbDelete = sender as LinkButton;
                GridViewRow dgi = lbDelete.NamingContainer as GridViewRow;
                if ( dgi.DataItem != null )
                {
                    PropertyInfo pi = dgi.DataItem.GetType().GetProperty( "IsSystem" );
                    if ( pi != null )
                    {
                        bool isSystem = (bool)pi.GetValue( dgi.DataItem );
                        if ( isSystem )
                        {
                            lbDelete.AddCssClass( "disabled" );
                            lbDelete.Enabled = false;
                        }
                    }
                }
            }

            GridViewRow row = ( GridViewRow ) ( ( LinkButton ) sender ).Parent.Parent;
            RowEventArgs args = new RowEventArgs( row );
            this.DeleteField.HandleOnDataBound( sender, args );
        }

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            if ( LinkButtonClick != null )
            {
                GridViewRow row = (GridViewRow)( (LinkButton)sender ).Parent.Parent;
                RowEventArgs args = new RowEventArgs( row );
                LinkButtonClick( sender, args );
            }
        }

        /// <summary>
        /// Occurs when [link button click].
        /// </summary>
        internal event EventHandler<RowEventArgs> LinkButtonClick;
    }
}