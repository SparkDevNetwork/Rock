using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationAddressPicker : Panel
    {
        #region Controls

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
                return "TODO!";
            }
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

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( ShowDropDown )
            {
                EnsureChildControls();
                _pnlPickerMenu.Style[HtmlTextWriterStyle.Display] = "block";
            }
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

            _btnPickerLabel = new HtmlAnchor { ID = "btnPickerLabel" };
            _btnPickerLabel.Attributes["class"] = "picker-label";
            _btnPickerLabel.InnerHtml = string.Format( "<i class='icon-user'></i>{0}<b class='caret pull-right'></b>", this.AddressSummaryText );
            _btnPickerLabel.HRef = "#";
            this.Controls.Add( _btnPickerLabel );

            // PickerMenu (DropDown menu)
            _pnlPickerMenu = new Panel { ID = "pnlPickerMenu" };
            _pnlPickerMenu.CssClass = "picker-menu dropdown-menu";
            _pnlPickerMenu.Style[HtmlTextWriterStyle.Display] = "none";
            this.Controls.Add( _pnlPickerMenu );
            _btnPickerLabel.Attributes["onclick"] = string.Format( "$('#{0}').toggle(); return false;", _pnlPickerMenu.ClientID );

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
            _btnCancel = new LinkButton { ID = "btnCancel", CssClass = "btn btn-xs", Text = "Cancel" };
            _btnCancel.OnClientClick = string.Format( "$('#{0}').hide();", _pnlPickerMenu.ClientID );
            _pnlPickerActions.Controls.Add( _btnCancel );
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
            // TODO
        }
    }
}
