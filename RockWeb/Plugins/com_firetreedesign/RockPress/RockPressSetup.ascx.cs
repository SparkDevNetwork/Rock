using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_firetreedesign.RockPress
{
    [DisplayName( "RockPress Setup" )]
    [Category( "FireTree Design > RockPress" )]
    [Description( "Configures the Rock side of RockPress." )]
    public partial class RockPressSetup : RockBlock
    {
        #region Properties

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Early initialization of the control.
        /// </summary>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Late initialization of the control.
        /// </summary>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                ShowDetails();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        protected void ShowDetails()
        {
            var rockContext = new RockContext();
            var restUserRecordTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
            var activeRecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            var person = new PersonService( rockContext ).Get( Guid.Parse( "320D4229-2307-492C-B0FA-206173FA2C64" ) );
            var user = person != null ? person.Users.FirstOrDefault() : null;

            var descriptionList = new DescriptionList()
                .Add( "REST API User", person != null ? person.FullName : "&nbsp;" )
                .Add( "REST API Key", user != null && user.ApiKey != string.Empty ? user.ApiKey : "&nbsp;" )
                .Add( "Instructions", "Next, you will need to assign the REST API User to the appropriate REST Controllers for the RockPress add-ons that you have installed or purchased. <a href='https://rockpresswp.com/docs/getting-started/'>Please follow these instructions to get started.</a>" )
                .Add( "WordPress Plugin", "Download the free plugin and check out the available add-ons for RockPress at <a href='https://rockpresswp.com/'>https://rockpresswp.com/</a>" );

            lDetails.Text = descriptionList.Html;

            lbDelete.Visible = user != null;
            lbGenerate.Visible = user == null;
        }

        /// <summary>
        /// Generates the API key.
        /// </summary>
        /// <returns></returns>
        protected string GenerateApiKey()
        {
            using ( var sha1 = new SHA1Managed() )
            {
                var hash = sha1.ComputeHash( Encoding.UTF8.GetBytes( Guid.NewGuid().ToString() ) );
                var b64 = Convert.ToBase64String( hash );

                return b64.Replace( "/", "" ).Replace( "+", "" ).Replace( "=", "" );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var userLoginService = new UserLoginService( rockContext );
            var restUser = new PersonService( rockContext ).Get( Guid.Parse( "320D4229-2307-492C-B0FA-206173FA2C64" ) );

            if ( restUser != null )
            {
                restUser.RecordStatusValueId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id;

                // remove all user logins for key
                foreach ( var login in restUser.Users.ToList() )
                {
                    userLoginService.Delete( login );
                }
                rockContext.SaveChanges();
            }

            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the lbGenerate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbGenerate_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var userLoginService = new UserLoginService( rockContext );
            var person = personService.Get( Guid.Parse( "320D4229-2307-492C-B0FA-206173FA2C64" ) );

            if ( person == null )
            {
                person = new Person();
                personService.Add( person );

                person.LastName = "RockPress API";
                person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
                person.Guid = Guid.Parse( "320D4229-2307-492C-B0FA-206173FA2C64" );
            }

            person.RecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            rockContext.SaveChanges();

            var entityType = new EntityTypeService( rockContext ).Get( "Rock.Security.Authentication.Database" );
            var userLogin = new UserLogin();
            userLoginService.Add( userLogin );

            userLogin.UserName = Guid.NewGuid().ToString();
            userLogin.IsConfirmed = true;
            userLogin.ApiKey = GenerateApiKey();
            userLogin.PersonId = person.Id;
            userLogin.EntityTypeId = entityType.Id;

            rockContext.SaveChanges();

            ShowDetails();
        }

        #endregion
    }
}
