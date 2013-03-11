//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

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

        protected bool UseStackedLayout = false;
        protected bool ShowCampusSelect = false;
        protected bool ShowSaveDetails = false;
        protected string spanClass;

        protected System.Collections.Generic.List<Int32> DefaultFunds;
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

            fundService = new FundService();
            UseStackedLayout = Convert.ToBoolean( GetAttributeValue( "Stacklayoutvertically" ) );
            ShowCampusSelect = Convert.ToBoolean( GetAttributeValue( "ShowCampusselection" ) );
            DefaultFunds = ( GetAttributeValue( "DefaultFundstodisplay" ).Split( ',' ).Select( s => int.Parse(s) ).ToList() );

            if ( CurrentPerson != null )
            {
                ShowSaveDetails = true;
            }
            
            if (ShowCampusSelect) {
                BindCampuses();
            }

            BindFunds();
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
        protected void btnAddFund_Click( object sender, EventArgs e )
        {
            
        }

        #endregion

        #region Internal Methods

        protected void BindCampuses()
        {
            ddlCampusList.Items.Clear();
            CampusService campusService = new CampusService();
            var items = campusService.Queryable().OrderBy( a => a.Name ).Select( a => a.Name ).Distinct().ToList();

            foreach ( string item in items )
            {
                ddlCampusList.Items.Add( item );
                
            }

            ddlCampusList.Title = "Select Your Campus";

        }

        protected void BindFunds()
        {
            

            var funds = fundService.Queryable()
                .Where( a => a.IsActive ).Distinct().OrderBy( a => a.Order )
                .Where( a => !DefaultFunds.Contains( a.Id ) )
                .Select( a => a.PublicName ).ToList();
                

            if ( funds.Count() > 0 )
            {
                btnAddFund.DataSource = funds;
                btnAddFund.DataBind();
                btnAddFund.Title = "Add Another Gift";
                divAddFund.Visible = true;
            }

            foreach ( var fund in DefaultFunds )
            {
                BindFundOption( Convert.ToInt32( fund ) );                
            }            
            
            plcNewFunds.DataBind();
            plcNewFunds.Visible = true;            

        }

        /// <summary>
        /// Binds the fund options to a div.
        /// </summary>
        protected void BindFundOption( int fundID )
        {
            Fund thisFund = fundService.Get( fundID);
            
            if ( fundID != 0 )
            {   
                HtmlGenericControl newFundContainer = new HtmlGenericControl( "div" );
                newFundContainer.Attributes.Add( "class", "row-fluid" );

                HtmlGenericControl newInputDiv = new HtmlGenericControl( "div" );
                newInputDiv.Attributes.Add( "class", "input-prepend" );

                    HtmlGenericControl newButtonDiv = new HtmlGenericControl( "div" );
                    newButtonDiv.Attributes.Add( "class", "btn-group" );
                
                        HtmlInputButton newFundButton = new HtmlInputButton();
                        newFundButton.Attributes.Add( "class", "btn dropdown-toggle" );
                        newFundButton.Attributes.Add( "readonly", "true" );
                        newFundButton.Attributes.Add( "tabindex", "-1" );
                        newFundButton.Value = thisFund.PublicName;
                        newButtonDiv.Controls.Add( newFundButton );
                                
                        HtmlGenericControl newSpan = new HtmlGenericControl( "span" );
                        newSpan.Attributes.Add( "class", "add-on" );
                        newSpan.InnerText = "$";
                        newButtonDiv.Controls.Add( newSpan );

                        HtmlGenericControl newInput = new HtmlGenericControl( "input" );
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