//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile blockthe main information about a peron 
    /// </summary>
    public partial class EditFamily : PersonBlock
    {
        public Group _family = null;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int familyId = int.MinValue;
            if ( int.TryParse( PageParameter( "FamilyId" ), out familyId ) )
            {
                _family = new GroupService().Get( familyId );
                if ( _family != null && string.Compare( _family.GroupType.Guid.ToString(), Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, true ) != 0 )
                {
                    nbNotice.Heading = "Invalid Family";
                    nbNotice.Text = "Sorry, but the group selected is not a Family group";
                    nbNotice.NotificationBoxType = NotificationBoxType.Error;
                    nbNotice.Visible = true;

                    _family = null;
                    return;
                }
            }

            var campusi = new CampusService().Queryable().OrderBy( a => a.Name ).ToList();
            cpCampus.Campuses = campusi;
            cpCampus.Visible = campusi.Any();

            ddlRecordStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS ) ) );
            ddlReason.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON ) ), true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack && _family != null )
            {
                ShowDetails();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRecordStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlRecordStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ddlReason.Visible = ( ddlRecordStatus.SelectedValueAsInt() == DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ) ).Id );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( new UnitOfWorkScope() )
            {
                var service = new GroupService();
                _family = service.Get( _family.Id );
                _family.Name = tbFamilyName.Text;
                _family.CampusId = cpCampus.SelectedValueAsInt();
                service.Save( _family, CurrentPersonId );

                if ( _family.Members.Any( m => m.PersonId == Person.Id ) )
                {
                    Response.Redirect( string.Format( "~/Person/{0}", Person.Id ), false );
                }
                else
                {
                    var fm = _family.Members.FirstOrDefault();
                    if ( fm != null )
                    {
                        Response.Redirect( string.Format( "~/Person/{0}", fm.PersonId ), false );
                    }
                    else
                    {
                        Response.Redirect( "~", false );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            Response.Redirect( string.Format( "~/Person/{0}", Person.Id ), false );
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            tbFamilyName.Text = _family.Name;
            cpCampus.SelectedCampusId = _family.CampusId;
        }

    }
}