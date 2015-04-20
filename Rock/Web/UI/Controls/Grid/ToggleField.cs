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
    /// <see cref="Grid"/> Column for toggling the bool/checkbox value of a row in a grid
    /// </summary>
    [ToolboxData( "<{0}:ToggleField BoundField=server runat=server></{0}:ToggleField>" )]
    public class ToggleField : RockTemplateField, INotRowSelectedField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleField" /> class.
        /// </summary>
        public ToggleField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        }

        /// <summary>
        /// Gets or sets the button size CSS class.
        /// </summary>
        /// <value>
        /// The button size CSS class.
        /// </value>
        public string ButtonSizeCssClass {get; set;}

        /// <summary>
        /// Gets or sets the active button CSS class.
        /// </summary>
        /// <value>
        /// The active button CSS class.
        /// </value>
        public string ActiveButtonCssClass { get; set; }

        /// <summary>
        /// Gets or sets the DataField of the BoundField.
        /// </summary>
        /// <value>
        /// The data field.
        /// </value>
        public string DataField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ToggleField" /> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public string Enabled { get; set; }

        /// <summary>
        /// Gets or sets the text that represents the "on" state.
        /// </summary>
        /// <value>
        /// The "on" text.
        /// </value>
        public string OnText { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for the "on" state.
        /// </summary>
        /// <value>
        /// The "on" text.
        /// </value>
        public string OnCssClass { get; set; }

        /// <summary>
        /// Gets or sets the text that represents the "off" state.
        /// </summary>
        /// <value>
        /// The "off" text.
        /// </value>
        public string OffText { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for the "on" state.
        /// </summary>
        /// <value>
        /// The "on" text.
        /// </value>
        public string OffCssClass { get; set; }

        /// <summary>
        /// Gets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        public Grid ParentGrid { get; internal set; }

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
            ToggleFieldTemplate toggleFieldTemplate = new ToggleFieldTemplate();
            toggleFieldTemplate.CheckedChanged += toggleFieldTemplate_CheckedChanged;
            this.ItemTemplate = toggleFieldTemplate;
            this.ParentGrid = control as Grid;
            return base.Initialize( sortingEnabled, control );
        }

        /// <summary>
        /// Handles the CheckedChanged event of the toggleFieldTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        void toggleFieldTemplate_CheckedChanged( object sender, RowEventArgs e )
        {
            OnClick( e );
        }

        /// <summary>
        /// Occurs when [checked changed].
        /// </summary>
        public event EventHandler<RowEventArgs> CheckedChanged;

        /// <summary>
        /// Raises the <see cref="E:Click"/> event.
        /// </summary>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        public virtual void OnClick( RowEventArgs e )
        {
            if ( CheckedChanged != null )
            {
                CheckedChanged( this, e );
            }
        }
    }

    /// <summary>
    /// Template used by the <see cref="ToggleField"/> control
    /// </summary>
    public class ToggleFieldTemplate : ITemplate
    {
        /// <summary>
        /// Gets or sets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        private Grid ParentGrid { get; set; }

        /// <summary>
        /// Gets or sets the DataField to bind to.
        /// </summary>
        /// <value>
        /// The DataField to bind to.
        /// </value>
        private string DataField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ToggleFieldTemplate" /> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        private bool Enabled { get; set; }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control"/>
        /// object that child controls and templates belong to. These child controls are in
        /// turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control"/> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                ToggleField toggleField = cell.ContainingField as ToggleField;
                ParentGrid = toggleField.ParentGrid;
                DataField = toggleField.DataField;
                bool isEnabled = false;
                bool.TryParse(toggleField.Enabled, out isEnabled );
                Enabled = isEnabled;

                Toggle toggle = new Toggle();
                toggle.OnText = toggleField.OnText;
                toggle.OffText = toggleField.OffText;
                toggle.OnCssClass = toggleField.OnCssClass;
                toggle.OffCssClass = toggleField.OffCssClass;

                //toggle.EnableViewState = true; // TODO remove if unnecessary
                toggle.ActiveButtonCssClass = toggleField.ActiveButtonCssClass;
                toggle.ButtonSizeCssClass = toggleField.ButtonSizeCssClass;
                toggle.CheckedChanged += toggle_CheckedChanged;
                toggle.DataBinding += toggle_DataBinding;
                toggle.PreRender += toggle_PreRender;

                cell.Controls.Add( toggle );
            }
        }

        /// <summary>
        /// Handles the PreRender event of the toggle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void toggle_PreRender( object sender, EventArgs e )
        {
            Toggle toggle = sender as Toggle;
            if ( toggle.Enabled && ( !ParentGrid.Enabled || ! this.Enabled ) )
            {
                toggle.AddCssClass( "disabled" );
                toggle.Enabled = false;
                //toggle.AutoPostBack = false; // TODO remove if unnecessary
            }
        }

        /// <summary>
        /// Handles the DataBinding event of the Toggle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void toggle_DataBinding( object sender, EventArgs e )
        {
            Toggle toggle = sender as Toggle;
            GridViewRow dgi = toggle.NamingContainer as GridViewRow;
            if ( dgi.DataItem != null && DataField != null )
            {
                object dataValue = DataBinder.Eval( dgi.DataItem, DataField );

                toggle.Checked = (Boolean?)dataValue ?? false;
            }
            //toggle.AutoPostBack = true;  // TODO remove if unnecessary
        }

        /// <summary>
        /// Handles the CheckedChanged event of the Toggle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void toggle_CheckedChanged( object sender, EventArgs e )
        {
            if ( CheckedChanged != null )
            {
                GridViewRow row = (GridViewRow)( (Toggle)sender ).Parent.Parent;
                RowEventArgs args = new RowEventArgs( row );
                CheckedChanged( sender, args );
            }
        }

        /// <summary>
        /// Occurs when checkbox [checked changed].
        /// </summary>
        internal event EventHandler<RowEventArgs> CheckedChanged;
    }
}