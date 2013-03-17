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
                
        protected FundService fundService = new FundService();
        protected List<Gift> giftList = new List<Gift>();

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptFundList.ItemDataBound += new RepeaterItemEventHandler( rptFundList_ItemDataBound );

            _UseStackedLayout = Convert.ToBoolean( GetAttributeValue( "Stacklayoutvertically" ) );
            _ShowCampusSelect = Convert.ToBoolean( GetAttributeValue( "ShowCampusselection" ) );
            List<string> defaultFunds = ( GetAttributeValue( "DefaultFundstodisplay" ).Any() ) ? GetAttributeValue( "DefaultFundstodisplay" ).Split( ',' ).ToList()
                : new System.Collections.Generic.List<string>();
                        
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
                BindFunds( giftList );
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
        
        protected void rptFundList_ItemDataBound( object sender, RepeaterItemEventArgs e)  
        {
            List<Gift> giftList = (List<Gift>)Session["fundList"];

            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                Gift gift = new Gift();
                gift.fundName = ( (HtmlInputControl) e.Item.FindControl( "btnFundName" ) ).Value;
                //gift.fundAmount = Convert.ToDouble( ( (HtmlInputControl) e.Item.FindControl( "inputFundAmount" ) ).Value);
                giftList.Add( gift );
            }

            Session["fundList"] = giftList;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            
            string msgCnfirmation = string.Format(@"<b>{0}</b>, you're about to give <b>${1}</b> to the 
                <b>{2}</b>. and <b>$50.00</b> to the <b>{3}</b>."
                , txtFirstName.Value + ' ' + txtLastName.Value); 

            //msgCnfirmation += string.Format(@"<br>Your total gift of <b>${0}</b> will be given using 
            //    a <b>{1}</b> credit card ending in <b>{2}</b>.", totalAmount.Value, txtCardNick.Value, txtCardNumber.Value);

            txtConfirmation.InnerText = msgCnfirmation;
            
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
            giftList = (List<Gift>)Session["fundList"];

            foreach ( Gift gift in giftList )
            {
                string giftName = Request.Form[gift.fundName];
                if ( gift.fundName == giftName )
                {
                    gift.fundAmount = Convert.ToDouble( Request.Form["inputFundAmount"] );
                }                
            }

            Gift newGift = new Gift();
            newGift.fundName = btnAddFund.SelectedValue;
            giftList.Add( newGift );

            //btnAddFund.Items.Remove( btnAddFund.SelectedValue );
            BindFunds( giftList );
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
        protected void BindFunds( List<Gift> gifts )
        {
            var queryable = fundService.Queryable().Where( f => f.IsActive )
                .Distinct().OrderBy( f => f.Order );
            
            // move from fund list to now using gift lists
            
            var fundsById = gifts.Any() ? funds.Select( s => int.Parse( s ) ).ToList() : 
                    new List<int>( ( queryable.Select( f => f.Id ).ToList().FirstOrDefault() ) );
            
            if ( ( queryable.Count() - funds.Count ) > 0 )
            {
                btnAddFund.DataSource = queryable.Where( f => !fundsById.Contains( f.Id ) )
                   .Select( f => f.PublicName ).ToList();
                btnAddFund.DataBind();
                btnAddFund.Title = "Add Another Gift";
                divAddFund.Visible = true;                
            }
            else
            {
                divAddFund.Visible = false;
            }

            rptFundList.DataSource = queryable.Where( f => fundsById.Contains( f.Id ) )
                   .Select( f => f.PublicName ).ToList();
            rptFundList.DataBind();
            Session["fundList"] = giftList;
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
        
        public class Gift {
            public string fundName { get; set; }
            public double fundAmount { get; set; }            
        }
    }
}