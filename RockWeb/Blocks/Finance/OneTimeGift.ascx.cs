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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>    
    [BooleanField( 0, "Stack layout vertically", false, "UseStackedLayout", "", "Should giving UI be stacked vertically or horizontally?" )]
    [BooleanField( 0, "Show Campus dropdown", false, "ShowCampusSelect", "", "Should giving be associated with a specific campus?" )]
    [IntegerField(0, "Maximum number of funds to display","2")]
    [GroupTypesField(0, "Primary fund name", false)]
    public partial class OneTimeGift : RockBlock
    {
        #region Fields

        protected bool UseStackedLayout = false;
        protected bool ShowCampusSelect = false;
        protected bool ShowSaveDetails = false;
        protected string spanClass;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            UseStackedLayout = Convert.ToBoolean( GetAttributeValue( "UseStackedLayout" ) );
            ShowCampusSelect = Convert.ToBoolean( GetAttributeValue( "ShowCampusSelect" ) );

            if ( CurrentPerson != null )
            {
                ShowSaveDetails = true;
            }

            BindCampuses();
            BindFunds();
            //TestBind();

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

        #endregion

        #region Internal Methods

        protected void BindCampuses()
        {
            
            CampusService campusService = new CampusService();
            var items = campusService.Queryable().OrderBy( a => a.Name).Select( a => a.Name).Distinct().ToList();

            foreach ( string item in items )
            {
                HtmlGenericControl campus = new HtmlGenericControl( "li" );
                listCampuses.Controls.Add( campus );

                HtmlGenericControl anchor = new HtmlGenericControl( "a" );
                anchor.Attributes.Add( "href", "#" );
                anchor.InnerText = item;

                campus.Controls.Add( anchor );
            }            
        }

        protected void BindFunds()
        {
            DefinedTypeService typeService = new DefinedTypeService();
            var items = typeService.Queryable().OrderBy( a => a.Category ).Select( a => a.Category ).Distinct().ToList();
            
            foreach ( string item in items ) {
                                
                HtmlGenericControl fundOption = new HtmlGenericControl( "li" );
                listFunds.Controls.Add( fundOption );
                
                HtmlGenericControl anchor = new HtmlGenericControl( "a" );
                anchor.Attributes.Add( "id", "selectFund" );
                anchor.Attributes.Add( "href", "#" );                
                anchor.InnerText = item;

                fundOption.Controls.Add( anchor );

                
            }

        }

        protected void TestBind()
        {
            DefinedTypeService typeService = new DefinedTypeService();
            var items = typeService.Queryable().OrderBy( a => a.Category ).Select( a => a.Category ).Distinct().ToList();

            foreach ( string item in items )
            {

                //HtmlGenericControl fundOption = new HtmlGenericControl( "li" );
                //listFunds.Controls.Add( fundOption );

                HtmlGenericControl anchor = new HtmlGenericControl( "option" );
                //anchor.Attributes.Add( "id", "selectFund" );
                //anchor.Attributes.Add( "href", "#" );
                anchor.InnerText = item;

                fundSelect.Controls.Add( anchor );


            }

        }
        #endregion  


    }
}