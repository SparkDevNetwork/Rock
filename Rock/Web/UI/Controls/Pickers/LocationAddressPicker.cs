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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationAddressPicker : Panel, IRockControl, INamingContainer
    {
        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the CSS Icon text.
        /// </summary>
        /// <value>
        /// The CSS icon class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string IconCssClass
        {
            get { return ViewState["IconCssClass"] as string ?? string.Empty; }
            set { ViewState["IconCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }
            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }
            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private HiddenField _hfLocationId;
        private HtmlAnchor _btnPickerLabel;
        private HiddenFieldWithClass _hfPanelIsVisible;

        private Panel _pnlPickerMenu;
        private Panel _pnlAddressEntry;
        private AddressControl _acAddress;

        private Panel _pnlPickerActions;
        private LinkButton _btnSelect;
        private LinkButton _btnCancel;
        private HtmlAnchor _btnSelectNone;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the address summary text.
        /// </summary>
        /// <value>
        /// The address summary text.
        /// </value>
        public string AddressSummaryText
        {
            get
            {
                if ( this.Location != null )
                {
                    return this.Location.GetFullStreetAddress().ConvertCrLfToHtmlBr();
                }
                else
                {
                    return Rock.Constants.None.Text;
                }
            }
        }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public Location Location
        {
            get
            {
                EnsureChildControls();
                return new LocationService( new RockContext() ).Get( _hfLocationId.ValueAsInt() );
            }

            private set
            {
                EnsureChildControls();

                if ( value != null )
                {
                    _btnSelectNone.Attributes["class"] = "picker-select-none rollover-item";
                    _btnSelectNone.Style[HtmlTextWriterStyle.Display] = string.Empty;
                    _hfLocationId.Value = value.Id.ToString();
                }
                else
                {
                    _btnSelectNone.Attributes["class"] = "picker-select-none";
                    _btnSelectNone.Style[HtmlTextWriterStyle.Display] = "none";
                    _hfLocationId.Value = string.Empty;
                }

                _acAddress.SetValues( value );
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValue( Location value )
        {
            Location = value;
        }

        /// <summary>
        /// Gets or sets the mode panel.
        /// </summary>
        /// <value>
        /// The mode panel.
        /// </value>
        public Panel ModePanel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show drop down].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show drop down]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDropDown 
        {
            get
            {
                EnsureChildControls();
                return _hfPanelIsVisible.Value.AsBooleanOrNull() ?? false;
            }
            set
            {
                EnsureChildControls();
                _hfPanelIsVisible.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Web server control is enabled.
        /// </summary>
        /// <returns>true if control is enabled; otherwise, false. The default is true.</returns>
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                SetPickerOnClick();
            }
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            EnsureChildControls();

            ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( _btnSelect );
            ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( _btnSelectNone );
        }

        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            if ( ModePanel != null )
            {
                _pnlPickerMenu.Controls.AddAt( 0, ModePanel );
            }

            _btnPickerLabel.InnerHtml = string.Format( "<i class='fa fa-user'></i>{0}<b class='fa fa-caret-down pull-right'></b>", this.AddressSummaryText );
            
            base.Render( writer );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            this.CssClass = "picker picker-select rollover-container";

            _hfLocationId = new HiddenField { ID = "hfLocationId" };
            this.Controls.Add( _hfLocationId );

            _btnPickerLabel = new HtmlAnchor { ID = "btnPickerLabel" };
            _btnPickerLabel.Attributes["class"] = "picker-label";
            this.Controls.Add( _btnPickerLabel );

            _btnSelectNone = new HtmlAnchor();
            _btnSelectNone.Attributes["class"] = "picker-select-none";
            _btnSelectNone.ID = string.Format( "btnSelectNone_{0}", this.ID );
            _btnSelectNone.InnerHtml = "<i class='fa fa-times'></i>";
            _btnSelectNone.CausesValidation = false;
            _btnSelectNone.Style[HtmlTextWriterStyle.Display] = "none";
            _btnSelectNone.ServerClick += _btnSelectNone_ServerClick;
            this.Controls.Add( _btnSelectNone );

            _hfPanelIsVisible = new HiddenFieldWithClass();
            _hfPanelIsVisible.CssClass = "js-picker-menu-is-visible";
            _hfPanelIsVisible.ID = string.Format( "hfPanelIsVisible_{0}", this.ID );
            this.Controls.Add( _hfPanelIsVisible );

            // PickerMenu (DropDown menu)
            _pnlPickerMenu = new Panel { ID = "pnlPickerMenu" };
            _pnlPickerMenu.CssClass = "picker-menu dropdown-menu";
            this.Controls.Add( _pnlPickerMenu );
            SetPickerOnClick();

            // Address Entry
            _pnlAddressEntry = new Panel { ID = "pnlAddressEntry" };
            _pnlAddressEntry.CssClass = "locationpicker-address-entry";
            _pnlPickerMenu.Controls.Add( _pnlAddressEntry );

            _acAddress = new AddressControl { ID = "acAddress" };
            _pnlAddressEntry.Controls.Add( _acAddress );

            // picker actions
            _pnlPickerActions = new Panel { ID = "pnlPickerActions", CssClass = "picker-actions" };
            _pnlPickerMenu.Controls.Add( _pnlPickerActions );
            _btnSelect = new LinkButton { ID = "btnSelect", CssClass = "btn btn-xs btn-primary", Text = "Select", CausesValidation = false };
            _btnSelect.Click += _btnSelect_Click;
            _pnlPickerActions.Controls.Add( _btnSelect );
            _btnCancel = new LinkButton { ID = "btnCancel", CssClass = "btn btn-xs btn-link", Text = "Cancel" };
            _btnCancel.OnClientClick = string.Format( "$('#{0}').hide(); $('#{1}').val('false'); Rock.dialogs.updateModalScrollBar('{2}'); return false;", _pnlPickerMenu.ClientID, _hfPanelIsVisible.ClientID, this.ClientID );
            _pnlPickerActions.Controls.Add( _btnCancel );
        }

        /// <summary>
        /// Sets onclick script for the PickerLabel btn depending on Enabled
        /// </summary>
        private void SetPickerOnClick()
        {
            if ( this.Enabled )
            {
                _btnPickerLabel.Attributes["onclick"] = string.Format( "$('#{0}').toggle(); $('#{1}').val($('#{0}').is(':visible').toString()); Rock.dialogs.updateModalScrollBar('{2}'); return false;", _pnlPickerMenu.ClientID, _hfPanelIsVisible.ClientID, this.ClientID );
                _btnPickerLabel.HRef = "#";
            }
            else
            {
                _btnPickerLabel.Attributes["onclick"] = string.Empty;
                _btnPickerLabel.HRef = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the select dbGeography.
        /// </summary>
        /// <value>
        /// The select dbGeography.
        /// </value>
        public event EventHandler SelectGeography;

        /// <summary>
        /// Handles the Click event of the _btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _btnSelect_Click( object sender, EventArgs e )
        {
            LocationService locationService = new LocationService( new RockContext() );
            var location = locationService.Get( _acAddress.Street1, _acAddress.Street2, _acAddress.City, _acAddress.State, _acAddress.PostalCode, _acAddress.Country );
            Location = location;
            _btnPickerLabel.InnerHtml = string.Format( "<i class='fa fa-user'></i>{0}<b class='fa fa-caret-down pull-right'></b>", this.AddressSummaryText );
            ShowDropDown = false;

            if ( SelectGeography != null )
            {
                SelectGeography( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the _btnSelectNone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _btnSelectNone_ServerClick( object sender, EventArgs e )
        {
            Location = null;
            _btnPickerLabel.InnerHtml = string.Format( "<i class='fa fa-user'></i>{0}<b class='fa fa-caret-down pull-right'></b>", string.Empty );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {

            RegisterJavaScript();
            
            _pnlPickerMenu.Style[HtmlTextWriterStyle.Display] = ShowDropDown ? "block" : "none";
            this.Render( writer );
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            string script = string.Format( @"
setTimeout(function () {{
  Rock.dialogs.updateModalScrollBar('{0}');
}}, 0);", this.ClientID );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "location_address_picker-script_" + this.ID, script, true );
        }
    }
}
