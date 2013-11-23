using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationAddressPicker : Panel, IRockControl
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
        private Panel _pnlPickerMenu;
        private Panel _pnlAddressEntry;
        private RockTextBox _tbAddress1;
        private RockTextBox _tbAddress2;
        private HtmlGenericContainer _divCityStateZipRow;
        private HtmlGenericContainer _divCityColumn;
        private RockTextBox _tbCity;
        private HtmlGenericContainer _divStateColumn;
        private StateDropDownList _ddlState;
        private HtmlGenericContainer _divZipColumn;
        private RockTextBox _tbZip;

        private Panel _pnlPickerActions;
        private LinkButton _btnSelect;
        private LinkButton _btnCancel;

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
                    return this.Location.ToString();
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
        public Location Location {
            get
            {
                EnsureChildControls();
                return new LocationService().Get( _hfLocationId.ValueAsInt() );
            }

            private set
            {
                EnsureChildControls();
                if ( value != null )
                {
                    _hfLocationId.Value = value.Id.ToString();
                    _tbAddress1.Text = value.Street1;
                    _tbAddress2.Text = value.Street2;
                    _tbCity.Text = value.City;
                    _ddlState.SelectedValue = value.State;
                    _tbZip.Text = value.Zip;
                }
                else
                {
                    _hfLocationId.Value = string.Empty;
                    _tbAddress1.Text = string.Empty;
                    _tbAddress2.Text = string.Empty;
                    _tbCity.Text = string.Empty;
                    _ddlState.SelectedValue = string.Empty;
                    _tbZip.Text = string.Empty;
                }
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
        public bool ShowDropDown { get; set; }

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
            if ( ShowDropDown )
            {
                _pnlPickerMenu.Style[HtmlTextWriterStyle.Display] = "block";
            }

            ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( _btnSelect );
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

            base.Render( writer );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            this.CssClass = "picker picker-select";

            _hfLocationId = new HiddenField { ID = "hfLocationId" };
            this.Controls.Add( _hfLocationId );

            _btnPickerLabel = new HtmlAnchor { ID = "btnPickerLabel" };
            _btnPickerLabel.Attributes["class"] = "picker-label";
            _btnPickerLabel.InnerHtml = string.Format( "<i class='fa fa-user'></i>{0}<b class='caret pull-right'></b>", this.AddressSummaryText );
            this.Controls.Add( _btnPickerLabel );

            // PickerMenu (DropDown menu)
            _pnlPickerMenu = new Panel { ID = "pnlPickerMenu" };
            _pnlPickerMenu.CssClass = "picker-menu dropdown-menu";
            _pnlPickerMenu.Style[HtmlTextWriterStyle.Display] = "none";
            this.Controls.Add( _pnlPickerMenu );
            SetPickerOnClick();

            // Address Entry
            _pnlAddressEntry = new Panel { ID = "pnlAddressEntry" };
            _pnlAddressEntry.CssClass = "locationpicker-address-entry";
            _pnlPickerMenu.Controls.Add( _pnlAddressEntry );

            _tbAddress1 = new RockTextBox { ID = "tbAddress1" };
            _tbAddress1.Label = "Address Line 1";
            _pnlAddressEntry.Controls.Add( _tbAddress1 );

            _tbAddress2 = new RockTextBox { ID = "tbAddress2" };
            _tbAddress2.Label = "Address Line 2";
            _pnlAddressEntry.Controls.Add( _tbAddress2 );

            // Address Entry - City State Zip
            _divCityStateZipRow = new HtmlGenericContainer( "div", "row" );
            _pnlAddressEntry.Controls.Add( _divCityStateZipRow );

            _divCityColumn = new HtmlGenericContainer( "div", "col-lg-7" );
            _divCityStateZipRow.Controls.Add( _divCityColumn );
            _tbCity = new RockTextBox { ID = "tbCity" };
            _tbCity.Label = "City";
            _divCityColumn.Controls.Add( _tbCity );

            _divStateColumn = new HtmlGenericContainer( "div", "col-lg-2" );
            _divCityStateZipRow.Controls.Add( _divStateColumn );
            _ddlState = new StateDropDownList { ID = "ddlState" };
            _ddlState.UseAbbreviation = true;
            _ddlState.CssClass = "input-mini";
            _ddlState.Label = "State";
            _divStateColumn.Controls.Add( _ddlState );

            _divZipColumn = new HtmlGenericContainer( "div", "col-lg-3" );
            _divCityStateZipRow.Controls.Add( _divZipColumn );
            _tbZip = new RockTextBox { ID = "tbZip" };
            _tbZip.CssClass = "input-small";
            _tbZip.Label = "Zip";
            _divZipColumn.Controls.Add( _tbZip );

            // picker actions
            _pnlPickerActions = new Panel { ID = "pnlPickerActions", CssClass = "picker-actions" };
            _pnlPickerMenu.Controls.Add( _pnlPickerActions );
            _btnSelect = new LinkButton { ID = "btnSelect", CssClass = "btn btn-xs btn-primary", Text = "Select", CausesValidation = false };
            _btnSelect.Click += _btnSelect_Click;
            _pnlPickerActions.Controls.Add( _btnSelect );
            _btnCancel = new LinkButton { ID = "btnCancel", CssClass = "btn btn-xs btn-link", Text = "Cancel" };
            _btnCancel.OnClientClick = string.Format( "$('#{0}').hide();", _pnlPickerMenu.ClientID );
            _pnlPickerActions.Controls.Add( _btnCancel );
        }

        /// <summary>
        /// Sets onclick script for the PickerLabel btn depending on Enabled
        /// </summary>
        private void SetPickerOnClick()
        {
            if ( this.Enabled )
            {
                _btnPickerLabel.Attributes["onclick"] = string.Format( "$('#{0}').toggle(); return false;", _pnlPickerMenu.ClientID );
                _btnPickerLabel.HRef = "#";
            }
            else
            {
                _btnPickerLabel.Attributes["onclick"] = string.Empty;
                _btnPickerLabel.HRef = string.Empty;
            }
        }

        /// <summary>
        /// Handles the Click event of the _btnPickerLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _btnPickerLabel_Click( object sender, EventArgs e )
        {
            string currentVal = _pnlPickerMenu.Style[HtmlTextWriterStyle.Display];

            // toggle Display of PickerMenu
            _pnlPickerMenu.Style[HtmlTextWriterStyle.Display] = currentVal != "none" ? "none" : "block";
        }

        /// <summary>
        /// Handles the Click event of the _btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _btnSelect_Click( object sender, EventArgs e )
        {
            LocationService locationService = new LocationService();
            var location = locationService.Get( _tbAddress1.Text, _tbAddress2.Text, _tbCity.Text, _ddlState.SelectedItem.Text, _tbZip.Text );
            Location = location;
            _btnPickerLabel.InnerHtml = string.Format( "<i class='fa fa-user'></i>{0}<b class='caret pull-right'></b>", this.AddressSummaryText );
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
            this.Render( writer );
        }
    }
}
