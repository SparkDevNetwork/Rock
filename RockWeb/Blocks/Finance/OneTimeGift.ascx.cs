//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI.HtmlControls;
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
        protected string spanClass;

        protected List<string> _DefaultFunds;
        protected FundService fundService;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !IsPostBack )
            {            
                _UseStackedLayout = Convert.ToBoolean( GetAttributeValue( "Stacklayoutvertically" ) );
                _ShowCampusSelect = Convert.ToBoolean( GetAttributeValue( "ShowCampusselection" ) );
                _DefaultFunds = ( GetAttributeValue( "DefaultFundstodisplay" ).Any() ) ? GetAttributeValue( "DefaultFundstodisplay" ).Split( ',' ).ToList()
                    : new System.Collections.Generic.List<string>();

                fundService = new FundService();

                if ( CurrentPerson != null )
                {
                    _ShowSaveDetails = true;
                }

                if ( _ShowCampusSelect )
                {
                    BindCampuses();
                }
                                
                BindCreditOptions();
                BindFunds();
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
            if ( GetAttributeValue( "DefaultFundstodisplay" ).Split( ',' ).ToList().Count() < 2 )
            {
                btnSecondaryFund.Value = btnAddFund.SelectedValue;
                btnAddFund.Items.Remove( btnAddFund.SelectedValue );
                divSecondaryFund.Visible = true;
            }
            else
            {
                BindFundOption( btnAddFund.SelectedValue );
                btnAddFund.Items.Remove( btnAddFund.SelectedValue );
            }
            
            
            if ( btnAddFund.Items.Count == 0 )
            {
                divAddFund.Visible = false;
            }

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
        protected void BindFunds()
        {
            var funds = fundService.Queryable().Where( a => a.IsActive ).Distinct().OrderBy( a => a.Order );
            var DefaultByID = _DefaultFunds.Select( s => int.Parse( s ) ).ToList();
            var DefaultWhenEmpty = DefaultByID.Any() ? DefaultByID.FirstOrDefault() : funds.Select( a => a.Id ).ToList().FirstOrDefault();

            if ( _DefaultFunds.Count >= 2 )
            {
                var firstFund = DefaultByID.FirstOrDefault();
                btnPrimaryFund.Value = fundService.Get( firstFund ).PublicName;
                
                var secondFund = DefaultByID.Where( a => a != firstFund ).FirstOrDefault();
                btnSecondaryFund.Value = fundService.Get( secondFund ).PublicName;
                divSecondaryFund.Visible = true;

                foreach ( var fund in DefaultByID.Where( a => a != firstFund && a != secondFund ) )
                {
                    BindFundOption( fundService.Get( fund ).PublicName );
                }
            }
            
            else
            {                
                btnPrimaryFund.Value = fundService.Get( DefaultWhenEmpty ).PublicName; 
                DefaultByID.Add( DefaultWhenEmpty );
            }

            if ( ( funds.Count() - _DefaultFunds.Count ) > 0 )
            {
               btnAddFund.DataSource = funds.Where( a => !DefaultByID.Contains( a.Id ) )
                   .Where( a => a.IsActive ).Distinct().OrderBy( a => a.Order )
                   .Select( a => a.PublicName ).ToList();
                btnAddFund.Title = "Add Another Gift";
                btnAddFund.DataBind();
                divAddFund.Visible = true;
            }

            plcNewFunds.DataBind();
            plcNewFunds.Visible = true;
            
        }

        /// <summary>
        /// Binds the credit options.
        /// </summary>
        protected void BindCreditOptions()
        {
            //other options to add here
            
            
            btnYearExpiration.Items.Clear();
            for (int i = 0; i < 10; i++)
            {
                btnYearExpiration.Items.Add(DateTime.Now.AddYears(i).Year.ToString());
            }

            btnYearExpiration.Title = "Year";

        }

        /// <summary>
        /// Binds the fund options to a div.
        /// </summary>
        protected void BindFundOption( string fundName )
        {
            if ( fundName != "" )
            {   
                HtmlGenericControl newFundContainer = new HtmlGenericControl( "div" );
                newFundContainer.ID = "divContainer" + fundName;
                newFundContainer.Attributes.Add( "class", "row-fluid" );

                HtmlGenericControl newInputDiv = new HtmlGenericControl( "div" );
                newInputDiv.ID = "divInput" + fundName;
                newInputDiv.Attributes.Add( "class", "input-prepend" );

                    HtmlGenericControl newButtonDiv = new HtmlGenericControl( "div" );
                    newButtonDiv.ID = "divButton" + fundName;
                    newButtonDiv.Attributes.Add( "class", "btn-group" );
                
                        HtmlInputButton newFundButton = new HtmlInputButton();
                        newFundButton.ID = "btn" + fundName;
                        newFundButton.Attributes.Add( "class", "btn dropdown-toggle" );
                        newFundButton.Attributes.Add( "readonly", "true" );
                        newFundButton.Attributes.Add( "tabindex", "-1" );
                        newFundButton.Value = fundName;
                        newButtonDiv.Controls.Add( newFundButton );
                                
                        HtmlGenericControl newSpan = new HtmlGenericControl( "span" );
                        newSpan.ID = "span" + fundName;
                        newSpan.Attributes.Add( "class", "add-on" );
                        newSpan.InnerText = "$";
                        newButtonDiv.Controls.Add( newSpan );

                        HtmlGenericControl newInput = new HtmlGenericControl( "input" );
                        newInput.ID = "input" + fundName;
                        newInput.Attributes.Add( "class", "input-small calc" );
                        newInput.Attributes.Add( "title", "Enter a number" );
                        newInput.Attributes.Add( "type", "text" );
                        newInput.Attributes.Add( "placeholder", "0.00" );
                        newInput.Attributes.Add( "pattern", "[0-9]*" );
                        newButtonDiv.Controls.Add( newInput );

                    newInputDiv.Controls.Add( newButtonDiv );
                    newFundContainer.Controls.Add( newInputDiv );

                plcNewFunds.Controls.Add( newFundContainer );

            }

        }

        #endregion
    }
}