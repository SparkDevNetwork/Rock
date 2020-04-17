using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_bemadev.Cms
{
    [DisplayName( "Page Parameter Filter" )]
    [Category( "com_bemadev > Cms" )]
    [Description( "A collection of filters that will pass its values as page parameters." )]

    [TextField( "Boolean Select", "", false, "", "CustomSetting", 1 )]
    [TextField( "Boolean Select 2", "", false, "", "CustomSetting", 2 )]
    [TextField( "Boolean Select 3", "", false, "", "CustomSetting", 3 )]

    [TextField( "Date Range Label", "", false, "", "CustomSetting", 1 )]
    [TextField( "Date Range Label 2", "", false, "", "CustomSetting", 2 )]
    [TextField( "Date Range Label 3", "", false, "", "CustomSetting", 3 )]

    [TextField( "Date Label", "", false, "", "CustomSetting", 1 )]

    [TextField( "Multi Select Label", "", false, "", "CustomSetting", 1 )]
    [KeyValueListField( "Multi Select List", "", false, "", "Name", "Value", "", "", "CustomSetting", 2 )]
    [TextField( "Multi Select Label 2", "", false, "", "CustomSetting", 3 )]
    [KeyValueListField( "Multi Select List 2", "", false, "", "Name", "Value", "", "", "CustomSetting", 4 )]

    [TextField( "Number Range Label", "", false, "", "CustomSetting", 1 )]
    [TextField( "Number Range Label 2", "", false, "", "CustomSetting", 2 )]
    [TextField( "Number Range Label 3", "", false, "", "CustomSetting", 3 )]

    [BooleanField( "Enable Campuses Filter", "Enables the campus filter.", false, "CustomSetting", 1 )]
    [BooleanField( "Enable Accounts Filter", "Enables the accounts filter.", false, "CustomSetting", 2 )]
	[BooleanField( "Enable Accounts 2 Filter", "Enables the accounts filter.", false, "CustomSetting", 3 )]
	[BooleanField( "Enable Account Filter", "Enables the account filter.", false, "CustomSetting", 4 )]
	[TextField( "Accounts Title", "", false, "", "CustomSetting", 5 )]
	[TextField( "Accounts 2 Title", "", false, "", "CustomSetting", 6 )]
	[TextField( "Account Title", "", false, "", "CustomSetting", 7 )]
    [BooleanField( "Enable Person Filter", "Enables the person filter.", false, "CustomSetting", 8 )]

    [LinkedPage( "Page Redirect", "If set, the filter button will redirect to the selected page.", false, "", "CustomSetting", 1)]
    public partial class PageParameterFilter : RockBlockCustomSettings
    {
        private List<string> _boolOptions = new List<string>();

        private string _booleanLabel;
        private string _booleanLabel2;
        private string _booleanLabel3;
        private string _dateRangeLabel;
        private string _dateRangeLabel2;
        private string _dateRangeLabel3;
        private string _dateLabel;
        private string _multiSelectLabel;
        private string _multiSelectLabel2;
        private string _numberRangeLabel;
        private string _numberRangeLabel2;
        private string _numberRangeLabel3;
        private bool _enableCampuses;
        private bool _enableAccounts;
		private bool _enableAccounts2;
		private bool _enableAccount;
		private string _accountsTitle;
		private string _accounts2Title;
		private string _accountTitle;
        private bool _enablePersonPicker;

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // load up bool options
            _boolOptions.Add( "" );
            _boolOptions.Add( "True" );
            _boolOptions.Add( "False" );

            _booleanLabel = GetAttributeValue( "BooleanSelect" );
            _booleanLabel2 = GetAttributeValue( "BooleanSelect2" );
            _booleanLabel3 = GetAttributeValue( "BooleanSelect3" );

            _dateRangeLabel = GetAttributeValue( "DateRangeLabel" );
            _dateRangeLabel2 = GetAttributeValue( "DateRangeLabel2" );
            _dateRangeLabel3 = GetAttributeValue( "DateRangeLabel3" );

            _dateLabel = GetAttributeValue( "DateLabel" );

            _multiSelectLabel = GetAttributeValue( "MultiSelectLabel" );
            _multiSelectLabel2 = GetAttributeValue( "MultiSelectLabel2" );

            _numberRangeLabel = GetAttributeValue( "NumberRangeLabel" );
            _numberRangeLabel2 = GetAttributeValue( "NumberRangeLabel2" );
            _numberRangeLabel3 = GetAttributeValue( "NumberRangeLabel3" );

            _enableCampuses = GetAttributeValue( "EnableCampusesFilter" ).AsBoolean();
            _enableAccounts = GetAttributeValue( "EnableAccountsFilter" ).AsBoolean();
			_enableAccounts2 = GetAttributeValue( "EnableAccounts2Filter" ).AsBoolean();
			_enableAccount = GetAttributeValue( "EnableAccountFilter" ).AsBoolean();
			_accountsTitle = GetAttributeValue( "AccountsTitle" );
			_accounts2Title = GetAttributeValue( "Accounts2Title" );
			_accountTitle = GetAttributeValue( "AccountTitle" );
            _enablePersonPicker = GetAttributeValue( "EnablePersonFilter" ).AsBoolean();

            // this event gets fired after block settings are updated.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadFilters();
            }
        }

        #endregion

        #region Methods

        private void LoadFilters()
        {
            // Boolean 1
            if ( !String.IsNullOrWhiteSpace( _booleanLabel ) )
            {
                ddlBoolean1Div.Visible = true;
                ddlBoolean1.Label = _booleanLabel;
                ddlBoolean1.DataSource = _boolOptions;
                ddlBoolean1.DataBind();
            }

            var selectedCustomBool = Request.QueryString[_booleanLabel.RemoveSpaces()];
            if ( selectedCustomBool != null )
            {
                ddlBoolean1.SetValue( selectedCustomBool );
            }

            // Boolean 2
            if ( !String.IsNullOrWhiteSpace( _booleanLabel2 ) )
            {
                ddlBoolean2Div.Visible = true;
                ddlBoolean2.Label = _booleanLabel2;
                ddlBoolean2.DataSource = _boolOptions;
                ddlBoolean2.DataBind();
            }

            var selectedCustomBool2 = Request.QueryString[_booleanLabel2.RemoveSpaces()];
            if ( selectedCustomBool2 != null )
            {
                ddlBoolean2.SetValue( selectedCustomBool2 );
            }

            // Boolean 3
            if ( !String.IsNullOrWhiteSpace( _booleanLabel3 ) )
            {
                ddlBoolean3Div.Visible = true;
                ddlBoolean3.Label = _booleanLabel3;
                ddlBoolean3.DataSource = _boolOptions;
                ddlBoolean3.DataBind();
            }

            var selectedCustomBool3 = Request.QueryString[_booleanLabel3.RemoveSpaces()];
            if ( selectedCustomBool3 != null )
            {
                ddlBoolean3.SetValue( selectedCustomBool3 );
            }

            // Date Range 1
            if ( !String.IsNullOrWhiteSpace( _dateRangeLabel ) )
            {
                drpDateRangeDiv.Visible = true;
                drpDateRange.Label = _dateRangeLabel;
            }

            var dateRange = Request.QueryString[_dateRangeLabel.RemoveSpaces()];
            if ( dateRange != null )
            {
                drpDateRange.DelimitedValues = dateRange;
            }

            // Date Range 2
            if ( !String.IsNullOrWhiteSpace( _dateRangeLabel2 ) )
            {
                drpDateRange2Div.Visible = true;
                drpDateRange2.Label = _dateRangeLabel2;
            }

            var dateRange2 = Request.QueryString[_dateRangeLabel2.RemoveSpaces()];
            if ( dateRange2 != null )
            {
                drpDateRange2.DelimitedValues = dateRange2;
            }

            // Date Range 3
            if ( !String.IsNullOrWhiteSpace( _dateRangeLabel3 ) )
            {
                drpDateRange3Div.Visible = true;
                drpDateRange3.Label = _dateRangeLabel3;
            }

            var dateRange3 = Request.QueryString[_dateRangeLabel3.RemoveSpaces()];
            if ( dateRange3 != null )
            {
                drpDateRange3.DelimitedValues = dateRange3;
            }

            // Date
            if ( !String.IsNullOrWhiteSpace( _dateLabel ) )
            {
                dpDateDiv.Visible = true;
                dpDate.Label = _dateLabel;
            }

            var date = Request.QueryString[_dateLabel.RemoveSpaces()];
            if ( date != null )
            {
                dpDate.SelectedDate = date.AsDateTime();
            }

            //  Multi Select 1
            var selections1 = GetCustomMultiSelectListData( "MultiSelectList" );
            if ( selections1.Any() )
            {
                cbMultiSelectList.Label = _multiSelectLabel;
                cbMultiSelectListDiv.Visible = true;
                cbMultiSelectList.Items.Clear();

                foreach ( var selection in selections1 )
                {
                    cbMultiSelectList.Items.Add( new ListItem( selection.Key, selection.Value.ToString() ) );
                }
            }

            var selectedItems1 = Request.QueryString[_multiSelectLabel.RemoveSpaces()];
            if ( selectedItems1 != null )
            {
                cbMultiSelectList.SetValues( selectedItems1.SplitDelimitedValues( false ) );
            }

            //  Multi Select 2
            var selections2 = GetCustomMultiSelectListData( "MultiSelectList2" );
            if ( selections2.Any() )
            {
                cbMultiSelectList2.Label = _multiSelectLabel2;
                cbMultiSelectList2Div.Visible = true;
                cbMultiSelectList2.Items.Clear();

                foreach ( var selection in selections2 )
                {
                    cbMultiSelectList2.Items.Add( new ListItem( selection.Key, selection.Value.ToString() ) );
                }
            }

            var selectedItems2 = Request.QueryString[_multiSelectLabel2.RemoveSpaces()];
            if ( selectedItems2 != null )
            {
                cbMultiSelectList2.SetValues( selectedItems2.SplitDelimitedValues( false ) );
            }

            // Number Range
            if ( !String.IsNullOrWhiteSpace( _numberRangeLabel ) )
            {
                nrNumberRange.Label = _numberRangeLabel;
                nrNumberRangeDiv.Visible = true;
            }

            var numberRangeStart = Request.QueryString[( _numberRangeLabel + "Start" ).RemoveSpaces()];
            if ( numberRangeStart.AsDecimalOrNull() != null )
            {
                nrNumberRange.LowerValue = numberRangeStart.AsDecimalOrNull();
            }
            var numberRangeEnd = Request.QueryString[( _numberRangeLabel + "End" ).RemoveSpaces()];
            if ( numberRangeEnd.AsDecimalOrNull() != null )
            {
                nrNumberRange.UpperValue = numberRangeEnd.AsDecimalOrNull();
            }

            // Number Range 2
            if ( !String.IsNullOrWhiteSpace( _numberRangeLabel2 ) )
            {
                nrNumberRange2.Label = _numberRangeLabel2;
                nrNumberRangeDiv2.Visible = true;
            }

            var numberRangeStart2 = Request.QueryString[( _numberRangeLabel2 + "Start" ).RemoveSpaces()];
            if ( numberRangeStart2.AsDecimalOrNull() != null )
            {
                nrNumberRange2.LowerValue = numberRangeStart2.AsDecimalOrNull();
            }
            var numberRangeEnd2 = Request.QueryString[( _numberRangeLabel2 + "End" ).RemoveSpaces()];
            if ( numberRangeEnd2.AsDecimalOrNull() != null )
            {
                nrNumberRange2.UpperValue = numberRangeEnd2.AsDecimalOrNull();
            }

            // Number Range 3
            if ( !String.IsNullOrWhiteSpace( _numberRangeLabel3 ) )
            {
                nrNumberRange3.Label = _numberRangeLabel3;
                nrNumberRangeDiv3.Visible = true;
            }

            var numberRangeStart3 = Request.QueryString[( _numberRangeLabel3 + "Start" ).RemoveSpaces()];
            if ( numberRangeStart3.AsDecimalOrNull() != null )
            {
                nrNumberRange3.LowerValue = numberRangeStart3.AsDecimalOrNull();
            }
            var numberRangeEnd3 = Request.QueryString[( _numberRangeLabel3 + "End" ).RemoveSpaces()];
            if ( numberRangeEnd3.AsDecimalOrNull() != null )
            {
                nrNumberRange3.UpperValue = numberRangeEnd3.AsDecimalOrNull();
            }

            // Campus
            if ( _enableCampuses )
            {
                cpCampusesDiv.Visible = true;
                cpCampuses.Campuses = CampusCache.All().Where( c => c.IsActive == true ).ToList();
            }

            var selectedCampusIds = Request.QueryString["CampusIds"];
            if ( selectedCampusIds != null )
            {
                cpCampuses.SetValues( selectedCampusIds.SplitDelimitedValues( false ) );
            }

            // Accounts
            if ( _enableAccounts )
            {
                cpAcountsDiv.Visible = true;
				
				if(!String.IsNullOrWhiteSpace( _accountsTitle ) )
				{
					apAccounts.Label = _accountsTitle;
				}
            }
			
			// Accounts 2
            if ( _enableAccounts2 )
            {
                cpAcountsDiv2.Visible = true;
				
				if(!String.IsNullOrWhiteSpace( _accounts2Title ) )
				{
					apAccounts2.Label = _accounts2Title;
				}
            }
			
			var selectedAccount2Ids = Request.QueryString["Account2Ids"];
            if ( selectedAccount2Ids != null )
            {
                var accounts2 = new FinancialAccountService( new RockContext() );

                apAccounts2.SetValues( accounts2.GetByIds( selectedAccount2Ids.SplitDelimitedValues( false ).AsIntegerList() ) );
            }

			
			// Account
            if ( _enableAccount )
            {
                cpAcountDiv.Visible = true;
				
				if(!String.IsNullOrWhiteSpace( _accountTitle ) )
				{
					apAccount.Label = _accountTitle;
				}
            }

            var selectedAccountIds = Request.QueryString["AccountIds"];
            if ( selectedAccountIds != null )
            {
                var accounts = new FinancialAccountService( new RockContext() );

                apAccounts.SetValues( accounts.GetByIds( selectedAccountIds.SplitDelimitedValues( false ).AsIntegerList() ) );
            }

            // Person
            if ( _enablePersonPicker )
            {
                cpPersonDiv.Visible = true;
            }

            var selectedPersonId = Request.QueryString["PersonId"].AsIntegerOrNull();
            if ( selectedPersonId.HasValue )
            {
                ppPersonPicker.SetValue( new PersonService( new RockContext() ).Queryable().Where( p => p.Id == selectedPersonId.Value ).FirstOrDefault() );
            }
        }

        private Dictionary<string, object> GetCustomMultiSelectListData( string attributeKey )
        {
            var properties = new Dictionary<string, object>();

            var selectionString = GetAttributeValue( attributeKey );

            if ( !String.IsNullOrWhiteSpace( selectionString ) )
            {
                selectionString = selectionString.TrimEnd( '|' );
                var selections = selectionString.Split( '|' )
                                .Select( s => s.Split( '^' ) )
                                .Select( p => new { Name = p[0], Value = p[1] } );

                StringBuilder sbPageMarkup = new StringBuilder();
                foreach ( var selection in selections )
                {
                    properties.Add( selection.Name, selection.Value );
                }
            }
            return properties;
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void ShowSettings()
        {
            pnlEditModel.Visible = true;
            upnlContent.Update();
            mdEdit.Show();

            tbBooleanFilter.Text = GetAttributeValue( "BooleanSelect" );
            tbBooleanFilter2.Text = GetAttributeValue( "BooleanSelect2" );
            tbBooleanFilter3.Text = GetAttributeValue( "BooleanSelect3" );

            tbDateRange.Text = GetAttributeValue( "DateRangeLabel" );
            tbDateRange2.Text = GetAttributeValue( "DateRangeLabel2" );
            tbDateRange3.Text = GetAttributeValue( "DateRangeLabel3" );

            tbDate.Text = GetAttributeValue( "DateLabel" );

            tbMultiSelectLabel.Text = GetAttributeValue( "MultiSelectLabel" );
            kvMultiSelect.Value = GetAttributeValue( "MultiSelectList" );
            tbMultiSelectLabel2.Text = GetAttributeValue( "MultiSelectLabel2" );
            kvMultiSelect2.Value = GetAttributeValue( "MultiSelectList2" );

            tbNumberRangeLabel.Text = GetAttributeValue( "NumberRangeLabel" );
            tbNumberRangeLabel2.Text = GetAttributeValue( "NumberRangeLabel2" );
            tbNumberRangeLabel3.Text = GetAttributeValue( "NumberRangeLabel3" );

            ddlEnableCampuses.SetValue( GetAttributeValue( "EnableCampusesFilter" ) );
            ddlEnableAccounts.SetValue( GetAttributeValue( "EnableAccountsFilter" ) );
			ddlEnableAccounts2.SetValue( GetAttributeValue( "EnableAccounts2Filter" ) );
			ddlEnableAccount.SetValue( GetAttributeValue( "EnableAccountFilter" ) );
			tbAccountsTitle.Text = GetAttributeValue( "AccountsTitle" );
			tbAccounts2Title.Text = GetAttributeValue( "Accounts2Title" );
			tbAccountTitle.Text = GetAttributeValue( "AccountTitle" );
            ddlEnablePersonPicker.SetValue( GetAttributeValue( "EnablePersonFilter" ) );

            var pageId = GetAttributeValue( "PageRedirect" ).AsIntegerOrNull();
            if ( pageId > 0 )
            {
                ppRedirectPage.SetValue( new PageService( new RockContext() ).Queryable().Where( p => p.Id == pageId ).FirstOrDefault() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadFilters();
        }

        protected void btnFilter_Click( object sender, EventArgs e )
        {
            var queryString = HttpUtility.ParseQueryString( String.Empty );

            // Boolean Select 1
            if ( !String.IsNullOrWhiteSpace( ddlBoolean1.SelectedValue ) )
            {
                queryString.Set( _booleanLabel.RemoveSpaces().RemoveSpecialCharacters(), ddlBoolean1.SelectedValue.ToString() );
            }

            // Boolean Select 1
            if ( !String.IsNullOrWhiteSpace( ddlBoolean2.SelectedValue ) )
            {
                queryString.Set( _booleanLabel2.RemoveSpaces().RemoveSpecialCharacters(), ddlBoolean2.SelectedValue.ToString() );
            }

            // Boolean Select 1
            if ( !String.IsNullOrWhiteSpace( ddlBoolean3.SelectedValue ) )
            {
                queryString.Set( _booleanLabel3.RemoveSpaces().RemoveSpecialCharacters(), ddlBoolean3.SelectedValue.ToString() );
            }

            // Date Range 1
            if ( drpDateRange.LowerValue.HasValue && drpDateRange.UpperValue.HasValue )
            {
                queryString.Set( _dateRangeLabel.RemoveSpaces().RemoveSpecialCharacters(), 
                    drpDateRange.LowerValue.Value.ToShortDateString() + "," + drpDateRange.UpperValue.Value.ToShortDateString() );
            }

            // Date Range 2
            if ( drpDateRange2.LowerValue.HasValue && drpDateRange2.UpperValue.HasValue )
            {
                queryString.Set( _dateRangeLabel2.RemoveSpaces().RemoveSpecialCharacters(),
                    drpDateRange2.LowerValue.Value.ToShortDateString() + "," + drpDateRange2.UpperValue.Value.ToShortDateString() );
            }

            // Date Range 3
            if ( drpDateRange3.LowerValue.HasValue && drpDateRange3.UpperValue.HasValue )
            {
                queryString.Set( _dateRangeLabel3.RemoveSpaces().RemoveSpecialCharacters(),
                    drpDateRange3.LowerValue.Value.ToShortDateString() + "," + drpDateRange3.UpperValue.Value.ToShortDateString() );
            }

            // Date            
            if ( dpDate.SelectedDate.HasValue )
            {
                queryString.Set( _dateLabel.RemoveSpaces().RemoveSpecialCharacters(),
                    dpDate.SelectedDate.Value.ToShortDateString() );
            }

            // Multi Select List 1
            if ( cbMultiSelectList.SelectedValues.AsDelimited( "," ).Any() )
            {
                queryString.Set( _multiSelectLabel.RemoveSpaces().RemoveSpecialCharacters(), cbMultiSelectList.SelectedValues.AsDelimited( "," ) );
            }

            // Multi Select List 2
            if ( cbMultiSelectList2.SelectedValues.AsDelimited( "," ).Any() )
            {
                queryString.Set( _multiSelectLabel2.RemoveSpaces().RemoveSpecialCharacters(), cbMultiSelectList2.SelectedValues.AsDelimited( "," ) );
            }

            // Number Range
            if ( nrNumberRange.LowerValue != null )
            {
                queryString.Set( _numberRangeLabel.RemoveSpaces().RemoveSpecialCharacters() + "Start", nrNumberRange.LowerValue.ToString() );
            }
            if ( nrNumberRange.UpperValue != null )
            {
                queryString.Set( _numberRangeLabel.RemoveSpaces().RemoveSpecialCharacters() + "End", nrNumberRange.UpperValue.ToString() );
            }

            // Number Range 2
            if ( nrNumberRange2.LowerValue != null )
            {
                queryString.Set( _numberRangeLabel2.RemoveSpaces().RemoveSpecialCharacters() + "Start", nrNumberRange2.LowerValue.ToString() );
            }
            if ( nrNumberRange2.UpperValue != null )
            {
                queryString.Set( _numberRangeLabel2.RemoveSpaces().RemoveSpecialCharacters() + "End", nrNumberRange2.UpperValue.ToString() );
            }

            // Number Range 3
            if ( nrNumberRange3.LowerValue != null )
            {
                queryString.Set( _numberRangeLabel3.RemoveSpaces().RemoveSpecialCharacters() + "Start", nrNumberRange3.LowerValue.ToString() );
            }
            if ( nrNumberRange3.UpperValue != null )
            {
                queryString.Set( _numberRangeLabel3.RemoveSpaces().RemoveSpecialCharacters() + "End", nrNumberRange3.UpperValue.ToString() );
            }

            // Campus 
            if ( !String.IsNullOrWhiteSpace( cpCampuses.SelectedValue ) )
            {
                queryString.Set( "CampusIds", cpCampuses.SelectedCampusIds.AsDelimited( "," ) );
            }

            // Accounts
            if ( !String.IsNullOrWhiteSpace( apAccounts.SelectedValue ) && apAccounts.SelectedValue != "0" )
            {
                queryString.Set( "AccountIds", apAccounts.SelectedValues.ToList().AsDelimited( "," ) );
            }
			
			// Accounts 2
            if ( !String.IsNullOrWhiteSpace( apAccounts2.SelectedValue ) && apAccounts2.SelectedValue != "0" )
            {
                queryString.Set( "Account2Ids", apAccounts2.SelectedValues.ToList().AsDelimited( "," ) );
            }
			
			// Account 
            if ( !String.IsNullOrWhiteSpace( apAccount.SelectedValue ) && apAccount.SelectedValue != "0" )
            {
                queryString.Set( "AccountId", apAccount.SelectedValues.ToList().AsDelimited( "," ) );
            }

            // Person 
            if ( ppPersonPicker.PersonId.HasValue )
            {
                queryString.Set( "PersonId", ppPersonPicker.PersonId.Value.ToString() );
            }

            string url = Request.Url.AbsolutePath;
            if ( GetAttributeValue( "PageRedirect" ).AsIntegerOrNull() > 0 )
            {
                int pageId = GetAttributeValue( "PageRedirect" ).AsInteger();

                url = System.Web.VirtualPathUtility.ToAbsolute( string.Format( "~/page/{0}", pageId ) );
            }


            if ( queryString.AllKeys.Any() )
            {
                Response.Redirect( string.Format( "{0}?{1}", url, queryString ), false );
            }
            else
            {
                Response.Redirect( url, false );
            }
        }

        protected void lbSave_Click( object sender, EventArgs e )
        {
            SetAttributeValue( "BooleanSelect", tbBooleanFilter.Text );
            SetAttributeValue( "BooleanSelect2", tbBooleanFilter2.Text );
            SetAttributeValue( "BooleanSelect3", tbBooleanFilter3.Text );

            SetAttributeValue( "DateRangeLabel", tbDateRange.Text );
            SetAttributeValue( "DateRangeLabel2", tbDateRange2.Text );
            SetAttributeValue( "DateRangeLabel3", tbDateRange3.Text );

            SetAttributeValue( "DateLabel", tbDate.Text );

            SetAttributeValue( "MultiSelectLabel",  tbMultiSelectLabel.Text );
            SetAttributeValue( "MultiSelectList", kvMultiSelect.Value );
            SetAttributeValue( "MultiSelectLabel2", tbMultiSelectLabel2.Text );
            SetAttributeValue( "MultiSelectList2", kvMultiSelect2.Value );

            SetAttributeValue( "NumberRangeLabel", tbNumberRangeLabel.Text );
            SetAttributeValue( "NumberRangeLabel2", tbNumberRangeLabel2.Text );
            SetAttributeValue( "NumberRangeLabel3", tbNumberRangeLabel3.Text );

            SetAttributeValue( "EnableCampusesFilter", ddlEnableCampuses.Text );
            SetAttributeValue( "EnableAccountsFilter", ddlEnableAccounts.Text );
			SetAttributeValue( "EnableAccounts2Filter", ddlEnableAccounts2.Text );
			SetAttributeValue( "EnableAccountFilter", ddlEnableAccount.Text );
			SetAttributeValue( "AccountsTitle", tbAccountsTitle.Text );
			SetAttributeValue( "Accounts2Title", tbAccounts2Title.Text );
			SetAttributeValue( "AccountTitle", tbAccountTitle.Text );
            SetAttributeValue( "EnablePersonFilter", ddlEnablePersonPicker.Text );

            SetAttributeValue( "PageRedirect", ppRedirectPage.SelectedValue );

            SaveAttributeValues();

            LoadFilters();

            mdEdit.Hide();
            pnlEditModel.Visible = false;
            upnlContent.Update();

            Response.Redirect( Request.Url.AbsolutePath );
        }

        #endregion
    }
}