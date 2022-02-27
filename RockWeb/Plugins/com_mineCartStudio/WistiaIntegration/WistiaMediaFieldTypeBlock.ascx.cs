using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

using com.minecartstudio.WistiaIntegration.Model;

using Rock;
using Rock.Data;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_mineCartStudio.WistiaIntegration
{
    public partial class WistiaMediaFieldTypeBlock : RockBlock, IPickerBlock
    {
        #region IPicker Implementation

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string SelectedValue
        {
            get
            {
                EnsureChildControls();
                return _selectedValue;
            }

            set
            {
                EnsureChildControls();

                // The value is the Wistia media hashed ID
                _selectedValue = value;
                if ( _selectedValue.IsNullOrWhiteSpace() )
                {
                    return;
                }

                var rockContext = new RockContext();
                var projectId = new WistiaMediaService( rockContext ).GetByWistiaHashedId( _selectedValue ).WistiaProjectId;
                var accountId = new WistiaProjectService( rockContext ).Get( projectId ).WistiaAccountId;
                _selectedAccountId = accountId.ToString();
                _selectedProjectId = projectId.ToString();

                if ( _configuredAccountId == null )
                {
                    ddlAccount.SelectedValue = _selectedAccountId;
                }

                if ( _configuredProjectId == null )
                {
                    LoadProjectsDropDownList();
                    ddlProject.SelectedValue = projectId.ToString();
                }

                // If the project can't be found then the media is no longer valid.
                if ( _selectedProjectId.IsNullOrWhiteSpace() )
                {
                    _selectedValue = string.Empty;
                }

                LoadMediaDropDownList();
                ddlMedia.SelectedValue = _selectedValue;
            }
        }

        /// <summary>
        /// Occurs when [select item].
        /// </summary>
        /// </exception>
        event EventHandler IPickerBlock.SelectItem
        {
            add
            {
                // not implemented
            }

            remove
            {
                // not implemented
            }
        }

        /// <summary>
        /// Gets or sets the selected text.
        /// </summary>
        /// <value>
        /// The selected text.
        /// </value>
        public string SelectedText
        {
            get
            {
                return string.Empty;
            }

            set
            {
            }
        }

        /// <summary>
        /// Any Picker Settings that be configured
        /// </summary>
        /// <value>
        /// The picker settings.
        /// </value>
        public Dictionary<string, string> PickerSettings
        {
            get
            {
                if ( _pickerSettings == null )
                {
                    _pickerSettings = new Dictionary<string, string>();
                }

                return _pickerSettings;
            }

            set
            {
                _pickerSettings = value;
            }
        }

        private Dictionary<string, string> _pickerSettings;

        /// <summary>
        /// Gets the text representing the selected item.
        /// Ignores the input and gets the value from the SelectedText property.
        /// </summary>
        /// <param name="selectedValue"></param>
        /// <returns></returns>
        /// <value>
        /// The selected text.
        /// </value>
        public string GetSelectedText( string selectedValue )
        {
            return SelectedText;
        }

        #endregion IPicker Implementation

        #region BlockProperties

        private int? _configuredAccountId;
        private int? _configuredProjectId;

        private string _selectedAccountId;
        private string _selectedProjectId;
        private string _selectedValue;

        private const string WISTIA_ACCOUNT = "wistiaAccount";
        private const string WISTIA_PROJECT = "wistiaProject";

        #endregion BlockProperties

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // If these exist then the block is being used as a picker and we need to use the configration values.
            if ( PickerSettings != null && PickerSettings.Any() )
            {
                _configuredAccountId = PickerSettings.GetValueOrNull( WISTIA_ACCOUNT ).AsIntegerOrNull();
                _configuredProjectId = PickerSettings.GetValueOrNull( WISTIA_PROJECT ).AsIntegerOrNull();
            }
            
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( ddlProject == null )
            {
                return;
            }

            LoadAccountsDropDownList();
            LoadProjectsDropDownList();
            LoadMediaDropDownList();
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _configuredAccountId = ViewState["configuredAccountId"] as int?;
            _configuredProjectId = ViewState["configuredProjectId"] as int?;
            _selectedAccountId = ViewState["selectedAccountId"] as string;
            _selectedProjectId = ViewState["selectedProjectId"] as string;
            _selectedValue = ViewState["selectedValue"] as string;
            PickerSettings = ViewState["PickerSettings"] as Dictionary<string, string>;
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["configuredAccountId"] = _configuredAccountId;
            ViewState["configuredProjectId"] = _configuredProjectId;
            ViewState["selectedAccountId"] = _selectedAccountId;
            ViewState["selectedProjectId"] = _selectedProjectId;
            ViewState["selectedValue"] = _selectedValue;
            ViewState["PickerSettings"] = PickerSettings;

            return base.SaveViewState();
        }

        /// <summary>
        /// Loads the accounts drop down list.
        /// </summary>
        private void LoadAccountsDropDownList()
        {
            if ( _configuredAccountId != null )
            {
                ddlAccount.Visible = false;
                return;
            }

            var selectedValue = ddlAccount.SelectedValue;
            ddlAccount.Visible = true;
            ddlAccount.Items.Clear();
            ddlAccount.Items.Add( new ListItem() );
            GetAccountList().ToList().ForEach( a => ddlAccount.Items.Add( new ListItem( a.Value, a.Key.ToString() ) ) );
            ddlAccount.SelectedValue = selectedValue;
        }

        /// <summary>
        /// Gets the account list.
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, string> GetAccountList()
        {
            return new WistiaAccountService( new RockContext() )
                .Queryable()
                .ToDictionary( a => a.Id, a => a.Name );
        }

        /// <summary>
        /// Loads the projects drop down list.
        /// </summary>
        private void LoadProjectsDropDownList()
        {
            if ( _configuredAccountId == null && ddlAccount.SelectedValue.IsNullOrWhiteSpace() )
            {
                ddlProject.Enabled = false;
                return;
            }

            if ( _configuredProjectId != null )
            {
                ddlProject.Visible = false;
                return;
            }

            var selectedValue = ddlProject.SelectedValue;
            ddlProject.Visible = true;
            ddlProject.Enabled = true;
            ddlProject.Items.Clear();
            ddlProject.Items.Add( new ListItem() );
            GetProjectList().ToList().ForEach( p => ddlProject.Items.Add( new ListItem( p.Value, p.Key.ToString() ) ) );
            ddlProject.SelectedValue = selectedValue;
        }

        /// <summary>
        /// Gets the project list.
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, string> GetProjectList()
        {
            var accountId = _configuredAccountId ?? _selectedAccountId.AsIntegerOrNull();

            var projects = new WistiaProjectService( new RockContext() )
                .Queryable()
                .Where( p => p.WistiaAccountId == accountId )
                .ToDictionary( p => p.Id, p => p.Name );

            return projects;
        }

        /// <summary>
        /// Loads the media drop down list.
        /// </summary>
        private void LoadMediaDropDownList()
        {
            if ( _configuredProjectId == null && ddlProject != null && _selectedProjectId.IsNullOrWhiteSpace() )
            {
                ddlMedia.Enabled = false;
                return;
            }

            var selectedValue = ddlMedia.SelectedValue;
            ddlMedia.Enabled = true;
            ddlMedia.Items.Clear();
            ddlMedia.Items.Add( new ListItem() );
            GetMediaList().ToList().ForEach( m => ddlMedia.Items.Add( new ListItem( m.Value, m.Key.ToString() ) ) );
            ddlMedia.SelectedValue = selectedValue;
        }

        /// <summary>
        /// Gets the media list.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetMediaList()
        {
            var projectId = ddlProject.Visible ? ddlProject.SelectedValueAsInt() : _configuredProjectId;

            return new WistiaMediaService( new RockContext() )
                .Queryable()
                .Where( m => m.WistiaProjectId == projectId )
                .ToDictionary( m => m.WistiaHashedId, m => m.Name );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlAccount_SelectedIndexChanged( object sender, EventArgs e )
        {
            _selectedProjectId = ddlAccount.SelectedValue;
            LoadProjectsDropDownList();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlProject_SelectedIndexChanged( object sender, EventArgs e )
        {
            _selectedProjectId = ddlProject.SelectedValue;
            LoadMediaDropDownList();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMedia control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMedia_SelectedIndexChanged( object sender, EventArgs e )
        {
            _selectedValue = ddlMedia.SelectedValue;
        }
    }
}