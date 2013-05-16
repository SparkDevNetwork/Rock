using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockWeb.Blocks.Administration
{
    [DetailPage]
    [BooleanField("Show Cookies", "Show cookie data on page load. Default is false", false, "Display Settings")]
    [BooleanField("Show Server Variables", "Show server variables on page load. Default is false", false, "Display Settings")]
    public partial class ExceptionDetail : RockBlock, IDetailBlock  

    {
        List<ExceptionLog> exceptions;

        #region Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            exceptions = new List<ExceptionLog>();
            cbShowCookies.Text = "<i class=\"icon-laptop\"> </i> Show Cookies";
            cbShowServerVariables.Text = "<i class=\"icon-hdd\"> </i> Show Server Variables";
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string exceptionId = PageParameter( "ExceptionId" );
                if ( !string.IsNullOrWhiteSpace( exceptionId ) )
                {
                    ShowDetail( "ExceptionId", int.Parse( exceptionId ) );
                }
                else
                {
                    pnlExceptionDetail.Visible = false;
                }
            }
        }
        #endregion

        #region Block Events
        protected void cbShowCookies_CheckedChanged( object sender, EventArgs e )
        {
            pnlCookies.Visible = ( (LabeledCheckBox ) sender ).Checked;
        }
        protected void cbShowServerVariables_CheckedChanged( object sender, EventArgs e )
        {
            pnlServerVariables.Visible = ( ( LabeledCheckBox ) sender ).Checked;
        }
        #endregion

        #region Internal Methods

        private string BuildQueryStringList( string rawQueryString )
        {
            StringBuilder listBuilder = new StringBuilder();

            listBuilder.Append( "<ul type=\"disc\">" );
            foreach ( string value in rawQueryString.Split( "&".ToCharArray() ) )
            {
                string[] valueParts = value.Split( "=".ToCharArray() );

                if ( valueParts.Length > 1 )
                {
                    listBuilder.AppendFormat( "<li>{0}: {1}</li>", valueParts[0], valueParts[1] );
                }
                else
                {
                    listBuilder.AppendFormat( "<li>{0}</li>", valueParts[0] );
                }
            }

            listBuilder.Append( "</ul>" );

            return listBuilder.ToString();
        }

        private void AddExceptionToList( ExceptionLog ex )
        {
            if ( ex == null )
            {
                return;
            }
            exceptions.Add( ex );

            //If has parent
            if ( ex.ParentId != null && ex.ParentId > 0 )
            {
                //if parent doesn't exist in the exceptionList
                if ( exceptions.Where( e => e.Id == ex.ParentId ).Count() == 0 )
                {
                    AddExceptionToList( new ExceptionLogService().Get( (int)ex.ParentId ) );
                }
            }

            //foreach child exception (there should only be a max of 1)
            foreach ( ExceptionLog childException in new ExceptionLogService().GetByParentId(ex.Id) )
            {
                //If child doesn't aleady exist in the list
                if ( exceptions.Where( e => e.Id == childException.Id ).Count() == 0 )
                {
                    AddExceptionToList( childException );
                }
            }
        }

        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "ExceptionId" ) || itemKeyValue.Equals( 0 ) )
            {
                return;
            }


            ExceptionLog exception = new ExceptionLogService().Get( itemKeyValue );
            lblSiteName.Text = exception.Site.Name;
            lblPage.Text = exception.Page.Name;

            hlViewPage.NavigateUrl = exception.PageUrl;

            if ( exception.CreatedByPersonId == null )
            {
                lblUserName.Text = "Anonymous";
            }
            else
            {
                lblUserName.Text = exception.CreatedByPerson.FullName;
            }

            if ( !String.IsNullOrWhiteSpace( exception.QueryString ) )
            {
                litQueryString.Text = BuildQueryStringList( exception.QueryString );
                pnlQueryString.Visible = true;
            }
            else
            {
                pnlQueryString.Visible = false;
            }

            litCookies.Text = exception.Cookies;
            litServerVariables.Text = exception.ServerVariables;

            pnlCookies.Visible = Convert.ToBoolean( GetAttributeValue( "ShowCookies" ) );
            pnlServerVariables.Visible = Convert.ToBoolean( GetAttributeValue( "ShowServerVariables" ) );

            pnlExceptionSummary.Visible = true;
            pnlExceptionDetail.Visible = true;
        }
        #endregion



}
}