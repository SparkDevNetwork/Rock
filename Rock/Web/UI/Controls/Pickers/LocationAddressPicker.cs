using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationAddressPicker : Panel
    {
        #region Controls

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

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            this.CssClass = "picker-menu dropdown-menu";

            // Address Entry
            _pnlAddressEntry = new Panel { ID = "pnlAddressEntry" };
            _pnlAddressEntry.CssClass = "locationpicker-address-entry";
            Controls.Add( _pnlAddressEntry );

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
            this.Controls.Add( _pnlPickerActions );
            _btnSelect = new LinkButton { ID = "btnSelect", CssClass = "btn btn-xs btn-primary", Text = "Select", CausesValidation = false };
            _btnSelect.Click += _btnSelect_Click;
            _pnlPickerActions.Controls.Add( _btnSelect );
            _btnCancel = new LinkButton { ID = "btnCancel", CssClass = "btn btn-xs", Text = "Cancel" };
            _btnCancel.OnClientClick = string.Format( "$('#{0}').hide();", this.ClientID );
            _pnlPickerActions.Controls.Add( _btnCancel );

        }

        /// <summary>
        /// Handles the Click event of the _btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void _btnSelect_Click( object sender, EventArgs e )
        {
            // TODO
        }

        
    }
}
