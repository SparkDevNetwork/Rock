//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>    
    [CustomCheckboxListField( "Default Funds to display", "Which funds should be displayed by default?",
        "SELECT [Name] AS [Text], [Id] AS [Value] FROM [Fund] WHERE [IsActive] = 1 ORDER BY [Order]", true, "", "Filter", 1 )]
    [BooleanField( "Stack layout vertically", "Should giving UI be stacked vertically or horizontally?", false, "UI Options", 2 )]
    [BooleanField( "Show Campus selection", "Should giving be associated with a specific campus?", false, "UI Options", 3 )]    
    public partial class OneTimeGift : RockBlock
    {
        #region Fields

        protected bool _UseStackedLayout = false;
        protected bool _ShowCampusSelect = false;
        protected bool _ShowSaveDetails = false;
        protected string spanClass = "";

        protected List<FinancialTransaction> _transactionList;        
        protected FundService _fundService = new FundService();
        protected FinancialTransactionService _transactionService = new FinancialTransactionService();
        protected Dictionary<string, decimal> _giftList = new Dictionary<string, decimal>();
        //protected List<Gift> giftList = new List<Gift>();

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _UseStackedLayout = Convert.ToBoolean( GetAttributeValue( "Stacklayoutvertically" ) );
            _ShowCampusSelect = Convert.ToBoolean( GetAttributeValue( "ShowCampusselection" ) );
                        
            if ( CurrentPerson != null )
            {
                _ShowSaveDetails = true;
            }

            if ( _ShowCampusSelect )
            {
                BindCampuses();
            }

            if ( !IsPostBack )
            {
                BindFunds();
                BindCreditOptions();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            base.OnLoad( e );
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            //_giftList.Clear();
            
            //foreach ( RepeaterItem item in rptFundList.Items )
            //{
            //    string fundName = ( (HtmlInputControl)item.FindControl( "btnFundName" ) ).Value;
            //    decimal fundAmount = Convert.ToDecimal( ( (HtmlInputControl)item.FindControl( "inputFundAmount" ) ).Value );
            //    _giftList.Add( fundName, fundAmount );
            //}

            //Session["giftList"] = _giftList;

            //var firstTrans = _transactions.First();
            //lName.Text = person.FullName;
            //lTotal.Text = giftList.Sum( g => g.fundAmount ).ToString( "C" );
            //lCardType.Text = 
            //lLastFour.Text = firstTrans.CardNumber.Substring( firstTrans.CardNumber.length - 4 );
            //rptGiftConfirmation.DataSource = _transactions;
            //rptGiftConfirmation.DataBind();            
            
            pnlDetails.Visible = false;
            pnlConfirm.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnBack_Click( object sender, EventArgs e )
        {            
            pnlConfirm.Visible = false;
            pnlDetails.Visible = true;            
        }
        
        /// <summary>
        /// Handles the Click event of the btnAddFund control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddFund_SelectionChanged( object sender, EventArgs e )
        {
            _giftList.Clear();
            
            foreach (RepeaterItem item in rptFundList.Items)
            {
                _giftList.Add( ( (HtmlInputControl)item.FindControl( "btnFundName" ) ).Value
                    , Convert.ToDecimal( ( (HtmlInputControl)item.FindControl( "inputFundAmount" ) ).Value ));
            }

            // initialize new contribution
            _giftList.Add( btnAddFund.SelectedValue, 0M );

            if ( btnAddFund.Items.Count > 1 )
            {
                btnAddFund.Items.Remove(btnAddFund.SelectedValue);
                btnAddFund.Title = "Add Another Gift";
            }
            else
            {
                divAddFund.Visible = false;
            }

            rptFundList.DataSource = _giftList;
            rptFundList.DataBind();
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the campuses.
        /// </summary>
        protected void BindCampuses()
        {
            ddlCampusList.Items.Clear();
            CampusService campusService = new CampusService();
            var items = campusService.Queryable().OrderBy( a => a.Name ).Select( a => a.Name ).Distinct().ToList();

            foreach ( string item in items )
            {
                ddlCampusList.Items.Add( item + " Campus");                
            }

            ddlCampusList.Title = "Select Your Campus";
        }

        /// <summary>
        /// Binds the funds.
        /// </summary>
        protected void BindFunds( )
        {
            var queryable = _fundService.Queryable().Where( f => f.IsActive )
                .Distinct().OrderBy( f => f.Order );

            List<int> defaultFunds = GetAttributeValue( "DefaultFundstodisplay" ).Any()
                ? GetAttributeValue( "DefaultFundstodisplay" ).Split( ',' ).ToList().Select( s => int.Parse( s ) ).ToList()
                : new List<int>( ( queryable.Select( f => f.Id ).ToList().FirstOrDefault() ) );
            
            if ( ( queryable.Count() - defaultFunds.Count ) > 0 )
            {
                btnAddFund.DataSource = queryable.Where( f => !defaultFunds.Contains( f.Id ) )
                   .Select( f => f.PublicName ).ToList();
                btnAddFund.DataBind();
                btnAddFund.Title = "Add Another Gift";
                divAddFund.Visible = true;             
            }
            else
            {
                divAddFund.Visible = false;
            }

            rptFundList.DataSource = queryable.Where( f => defaultFunds.Contains( f.Id ) ).ToDictionary( f => f.PublicName, f => Convert.ToDecimal( !f.IsActive ) );
            rptFundList.DataBind();
        }

        /// <summary>
        /// Binds the credit options.
        /// </summary>
        protected void BindCreditOptions()
        {
            btnYearExpiration.Items.Clear();
            btnYearExpiration.Items.Add( DateTime.Now.Year.ToString() );

            for (int i = 1; i <= 12; i++)
            {
                btnMonthExpiration.Items.Add( CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName( i ) );
                btnYearExpiration.Items.Add(DateTime.Now.AddYears(i).Year.ToString());
            }

            btnMonthExpiration.DataBind();
            btnYearExpiration.DataBind();
        }

        #endregion
    }
}