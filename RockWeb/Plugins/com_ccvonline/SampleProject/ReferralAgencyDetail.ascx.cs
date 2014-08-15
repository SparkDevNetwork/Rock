using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.ccvonline.SampleProject.Data;
using com.ccvonline.SampleProject.Model;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Plugins.com_ccvonline.SampleProject
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Referral Agency Detail" )]
    [Category( "CCV > Sample Project" )]
    [Description( "Displays the details of a Referral Agency." )]

    public partial class ReferralAgencyDetail : Rock.Web.UI.RockBlock
    {
        #region Fields

        private ReferralAgency _referralAgency = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            var definedType = DefinedTypeCache.Read( com.ccvonline.SampleProject.SystemGuid.DefinedType.REFERRAL_AGENCY_TYPE.AsGuid() );
            if (definedType != null)
            {
                ddlAgencyType.BindToDefinedType( definedType, true );
            }
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
                var campusi = new CampusService( new RockContext() ).Queryable().OrderBy( a => a.Name ).ToList();
                cpCampus.Campuses = campusi;
                cpCampus.Visible = campusi.Any();

                ShowDetail();
            }
        }

        public override List<BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            string crumbName = ActionTitle.Add( ReferralAgency.FriendlyTypeName );

            int? referralAgencyId = PageParameter( "referralAgencyId" ).AsIntegerOrNull();
            if ( referralAgencyId.HasValue )
            {
                _referralAgency = new ReferralAgencyService( new SampleProjectContext() ).Get( referralAgencyId.Value );
                if ( _referralAgency != null )
                {
                    crumbName = _referralAgency.Name;
                }
            }

            breadCrumbs.Add( new BreadCrumb( crumbName, pageReference ) );

            return breadCrumbs;
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

        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ReferralAgency referralAgency;
            var dataContext = new SampleProjectContext();
            var service = new ReferralAgencyService( dataContext );

            int campusId = int.Parse( hfReferralAgencyId.Value );

            if ( campusId == 0 )
            {
                referralAgency = new ReferralAgency();
                service.Add( referralAgency );
            }
            else
            {
                referralAgency = service.Get( campusId );
            }

            referralAgency.Name = tbName.Text;
            referralAgency.Description = tbDescription.Text;
            referralAgency.CampusId = cpCampus.SelectedCampusId;
            referralAgency.AgencyTypeValueId = ddlAgencyType.SelectedValueAsId();
            referralAgency.ContactName = tbContactName.Text;
            referralAgency.PhoneNumber = tbPhoneNumber.Text;
            referralAgency.Website = tbWebsite.Text;

            if ( !referralAgency.IsValid || !Page.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            dataContext.SaveChanges();

            NavigateToParentPage();
        
        }
        
        #endregion

        #region Methods

        private void ShowDetail()
        {
            pnlDetails.Visible = true;

            int? referralAgencyId = PageParameter( "referralAgencyId" ).AsIntegerOrNull();
            int? campusId = PageParameter( "campusId" ).AsIntegerOrNull();
            int? agencyTypeValueId = PageParameter( "agencyTypeId" ).AsIntegerOrNull();

            ReferralAgency referralAgency = null;
            if (referralAgencyId.HasValue)
            {
                referralAgency = _referralAgency ?? new ReferralAgencyService( new SampleProjectContext() ).Get( referralAgencyId.Value );
            }

            if (referralAgency != null)
            {
                RockPage.PageTitle = referralAgency.Name;
                lActionTitle.Text = ActionTitle.Edit( referralAgency.Name ).FormatAsHtmlTitle();
            }
            else
            {
                referralAgency = new ReferralAgency { Id = 0, CampusId = campusId, AgencyTypeValueId = agencyTypeValueId };
                RockPage.PageTitle = ActionTitle.Add( ReferralAgency.FriendlyTypeName );
                lActionTitle.Text = ActionTitle.Add( ReferralAgency.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            hfReferralAgencyId.Value = referralAgency.Id.ToString();
            tbName.Text = referralAgency.Name;
            tbDescription.Text = referralAgency.Description;
            cpCampus.SelectedCampusId = referralAgency.CampusId;
            ddlAgencyType.SetValue( referralAgency.AgencyTypeValueId );
            tbContactName.Text = referralAgency.ContactName;
            tbPhoneNumber.Text = referralAgency.PhoneNumber;
            tbWebsite.Text = referralAgency.Website;

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if (!IsUserAuthorized(Rock.Security.Authorization.EDIT))
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ReferralAgency.FriendlyTypeName );
            }
            
            if (readOnly)
            {
                lActionTitle.Text = ActionTitle.View( ReferralAgency.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            tbContactName.ReadOnly = readOnly;
            tbPhoneNumber.ReadOnly = readOnly;
            tbWebsite.ReadOnly = readOnly;

            btnSave.Visible = !readOnly;
        }

        #endregion
    }
}