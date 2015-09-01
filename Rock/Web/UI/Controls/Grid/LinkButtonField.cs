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
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for showing a LinkButton in a grid
    /// </summary>
    [ToolboxData( "<{0}:LinkButtonField runat=server></{0}:LinkButtonField>" )]
    public class LinkButtonField : RockTemplateField, INotRowSelectedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkButtonField"/> class.
        /// </summary>
        public LinkButtonField()
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
            LinkButtonFieldTemplate linkButtonFieldTemplate = new LinkButtonFieldTemplate();
            linkButtonFieldTemplate.LinkButtonClick += linkButtonFieldTemplate_LinkButtonClick;
            this.ItemTemplate = linkButtonFieldTemplate;
            this.ParentGrid = control as Grid;
            return base.Initialize( sortingEnabled, control );
        }

        /// <summary>
        /// Handles the LinkButtonClick event of the linkButtonFieldTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void linkButtonFieldTemplate_LinkButtonClick( object sender, RowEventArgs e )
        {
            OnClick( e );
        }

        /// <summary>
        /// Gets or sets the CSS class of the button
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        public string CssClass
        {
            get
            {
                return ( ViewState["CssClass"] as string ) ?? string.Empty;

            }
            set
            {
                ViewState["CssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the tooltip of the button
        /// </summary>
        /// <value>
        /// The tooltip.
        /// </value>
        public string ToolTip
        {
            get
            {
                return ( ViewState["ToolTip"] as string ) ?? string.Empty;

            }
            set
            {
                ViewState["ToolTip"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the text of the button
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get
            {
                return ( ViewState["Text"] as string ) ?? string.Empty;

            }
            set
            {
                ViewState["Text"] = value;
            }
        }

        /// <summary>
        /// Gets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        public Grid ParentGrid { get; internal set; }

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
    /// Template used by the <see cref="LinkButtonField"/> control
    /// </summary>
    public class LinkButtonFieldTemplate : ITemplate
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
                LinkButtonField linkButtonField = cell.ContainingField as LinkButtonField;
                ParentGrid = linkButtonField.ParentGrid;
                LinkButton linkButton = new LinkButton();
                linkButton.CausesValidation = false;
                linkButton.CssClass = linkButtonField.CssClass;
                linkButton.Text = linkButtonField.Text;
                linkButton.ToolTip = linkButtonField.ToolTip;

                linkButton.Click += linkButton_Click;

                // make sure button is registered for async postback (needed just in case the grid was created at runtime)
                var sm = ScriptManager.GetCurrent( this.ParentGrid.Page );
                sm.RegisterAsyncPostBackControl( linkButton );

                cell.Controls.Add( linkButton );
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
        /// Handles the Click event of the linkButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void linkButton_Click( object sender, EventArgs e )
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