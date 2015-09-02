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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for deleting a row in a grid
    /// </summary>
    [ToolboxData( "<{0}:DeleteField runat=server></{0}:DeleteField>" )]
    public class DeleteField : RockTemplateField, INotRowSelectedField
    {
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
    /// Template used by the <see cref="DeleteField"/> control
    /// </summary>
    public class DeleteFieldTemplate : ITemplate
    {
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
                ParentGrid = deleteField.ParentGrid;
                LinkButton lbDelete = new LinkButton();
                lbDelete.CausesValidation = false;
                lbDelete.CssClass = "btn btn-danger btn-sm grid-delete-button";
                if ( lbDelete.Enabled && ( !ParentGrid.Enabled || !ParentGrid.IsDeleteEnabled ) )
                {
                    lbDelete.AddCssClass( "disabled" );
                    lbDelete.Enabled = false;
                }

                lbDelete.ToolTip = "Delete";

                HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
                buttonIcon.Attributes.Add( "class", "fa fa-times" );
                lbDelete.Controls.Add( buttonIcon );

                lbDelete.Click += lbDelete_Click;
                lbDelete.DataBinding += lbDelete_DataBinding;

                // make sure delete button is registered for async postback (needed just in case the grid was created at runtime)
                var sm = ScriptManager.GetCurrent( this.ParentGrid.Page );
                sm.RegisterAsyncPostBackControl( lbDelete );

                cell.Controls.Add( lbDelete );
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