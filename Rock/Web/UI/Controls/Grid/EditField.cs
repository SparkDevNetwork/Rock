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
using System.Web.UI.HtmlControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for editing a row in a grid
    /// </summary>
    [ToolboxData( "<{0}:EditField runat=server></{0}:EditField>" )]
    public class EditField : RockTemplateField, INotRowSelectedField
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
                if (string.IsNullOrWhiteSpace(iconCssClass))
                {
                    iconCssClass = "fa fa-pencil";
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
        /// Gets or sets the tool tip.
        /// </summary>
        /// <value>
        /// The tool tip.
        /// </value>
        public string ToolTip
        {
            get
            {
                string toolTip = ViewState["ToolTip"] as string;
                if ( string.IsNullOrWhiteSpace( toolTip ) )
                {
                    toolTip = "Edit";
                    ViewState["ToolTip"] = toolTip;
                }
                return toolTip;
            }
            set
            {
                ViewState["ToolTip"] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditField" /> class.
        /// </summary>
        public EditField()
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
            EditFieldTemplate editFieldTemplate = new EditFieldTemplate();
            editFieldTemplate.LinkButtonClick += editFieldTemplate_LinkButtonClick;
            this.ItemTemplate = editFieldTemplate;
            ParentGrid = control as Grid;

            return base.Initialize( sortingEnabled, control );
        }

        /// <summary>
        /// Handles the LinkButtonClick event of the editFieldTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        void editFieldTemplate_LinkButtonClick( object sender, RowEventArgs e )
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
                Click( this, e );
        }
    }

    /// <summary>
    /// Template used by the <see cref="EditField"/> control
    /// </summary>
    public class EditFieldTemplate : ITemplate
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
                EditField editField = cell.ContainingField as EditField;
                ParentGrid = editField.ParentGrid;

                LinkButton lbEdit = new LinkButton();
                lbEdit.CausesValidation = false;
                lbEdit.CssClass = "btn btn-default btn-sm";
                lbEdit.ToolTip = editField.ToolTip;
                
                HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
                buttonIcon.Attributes.Add("class", editField.IconCssClass);
                lbEdit.Controls.Add( buttonIcon );

                lbEdit.Click += lbEdit_Click;
                cell.Controls.Add( lbEdit );
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
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void lbEdit_Click( object sender, EventArgs e )
        {
            if ( LinkButtonClick != null )
            {
                GridViewRow row = ( GridViewRow )( ( LinkButton )sender ).Parent.Parent;
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