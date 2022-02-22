// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Used for editing the personal link section.
    /// It only allows Rock Administrator Or CreatedBy to modify any shared section.
    /// </summary>
    [DisplayName( "Personal Link Section Detail" )]
    [Category( "CMS" )]
    [Description( "Edit details of a Personal Link Section" )]

    [BooleanField(
        "Shared Section",
        Description = "When enabled, only shared sections will be displayed.",
        Key = AttributeKey.SharedSection,
        Order = 0)]

    public partial class PersonalLinkSectionDetail : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string SharedSection = "SharedSection";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string SectionId = "SectionId";
        }

        #endregion PageParameterKey

        #region Constants

        // This has a leading space on purpose.
        private const string PANEL_NAME = " Link Section";

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.PersonalLinkSection ) ).Id;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
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
                ShowDetail( PageParameter( PageParameterKey.SectionId ).AsInteger() );
            }
        }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();
            int? sectionId = PageParameter( pageReference, PageParameterKey.SectionId ).AsIntegerOrNull();
            if ( sectionId != null )
            {
                string sectionName = new PersonalLinkSectionService( new RockContext() )
                    .Queryable().Where( b => b.Id == sectionId.Value )
                    .Select( b => b.Name )
                    .FirstOrDefault();

                if ( !string.IsNullOrWhiteSpace( sectionName ) )
                {
                    breadCrumbs.Add( new BreadCrumb( $"{sectionName.FixCase()} Link Section", pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Section", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var personalLinkSection = new PersonalLinkSectionService( new RockContext() ).Get( hfPersonalLinkSectionId.Value.AsInteger() );
            ShowEditDetails( personalLinkSection );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( PageParameterKey.SectionId ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var rockContext = new RockContext();
            var personalLinkSectionService = new PersonalLinkSectionService( rockContext );
            var personalLinkSectionId = hfPersonalLinkSectionId.Value.AsIntegerOrNull();
            PersonalLinkSection personalLinkSection = null;
            if ( personalLinkSectionId.HasValue )
            {
                personalLinkSection = personalLinkSectionService.Get( personalLinkSectionId.Value );
            }

            var isNew = personalLinkSection == null;

            if ( isNew )
            {
                var isShared = GetAttributeValue( AttributeKey.SharedSection ).AsBoolean();
                personalLinkSection = new PersonalLinkSection()
                {
                    Id = 0,
                    IsShared = isShared
                };
                
                if ( !isShared )
                {
                    personalLinkSection.PersonAliasId = CurrentPersonAliasId.Value;
                }

                personalLinkSectionService.Add( personalLinkSection );
            }

            personalLinkSection.Name = tbName.Text;
            rockContext.SaveChanges();

            personalLinkSection = personalLinkSectionService.Get( personalLinkSection.Id );
            if ( personalLinkSection != null )
            {
                if ( personalLinkSection.IsShared )
                {
                    var groupService = new GroupService( rockContext );

                    var communicationAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS.AsGuid() );
                    if ( communicationAdministrators != null )
                    {
                        personalLinkSection.AllowSecurityRole( Authorization.VIEW, communicationAdministrators, rockContext );
                        personalLinkSection.AllowSecurityRole( Authorization.EDIT, communicationAdministrators, rockContext );
                        personalLinkSection.AllowSecurityRole( Authorization.ADMINISTRATE, communicationAdministrators, rockContext );
                    }

                    var webAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_WEB_ADMINISTRATORS.AsGuid() );
                    if ( webAdministrators != null )
                    {
                        personalLinkSection.AllowSecurityRole( Authorization.VIEW, webAdministrators, rockContext );
                        personalLinkSection.AllowSecurityRole( Authorization.EDIT, webAdministrators, rockContext );
                        personalLinkSection.AllowSecurityRole( Authorization.ADMINISTRATE, webAdministrators, rockContext );
                    }

                    var rockAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() );
                    if ( rockAdministrators != null )
                    {
                        personalLinkSection.AllowSecurityRole( Authorization.VIEW, rockAdministrators, rockContext );
                        personalLinkSection.AllowSecurityRole( Authorization.EDIT, rockAdministrators, rockContext );
                        personalLinkSection.AllowSecurityRole( Authorization.ADMINISTRATE, rockAdministrators, rockContext );
                    }
                }
                else
                {
                    personalLinkSection.MakePrivate( Authorization.VIEW, CurrentPerson );
                    personalLinkSection.MakePrivate( Authorization.EDIT, CurrentPerson, rockContext );
                    personalLinkSection.MakePrivate( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                }
            }

            var pageReference = RockPage.PageReference;
            pageReference.Parameters.AddOrReplace( PageParameterKey.SectionId, personalLinkSection.Id.ToString() );
            Response.Redirect( pageReference.BuildUrl(), false );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfPersonalLinkSectionId.Value.Equals( "0" ) )
            {
                // Canceling on Add
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString[PageParameterKey.SectionId] = hfPersonalLinkSectionId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                var personalLinkSection = new PersonalLinkSectionService( new RockContext() ).Get( hfPersonalLinkSectionId.Value.AsInteger() );
                ShowReadonlyDetails( personalLinkSection );
            }
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="personalLinkSectionId">The personal link section element identifier.</param>
        public void ShowDetail( int personalLinkSectionId )
        {
            var rockContext = new RockContext();
            var personalLinkSectionService = new PersonalLinkSectionService( rockContext );
            PersonalLinkSection personalLinkSection = null;
            bool isNew = false;
            if ( !personalLinkSectionId.Equals( 0 ) )
            {
                personalLinkSection = personalLinkSectionService.Get( personalLinkSectionId );
            }

            if ( personalLinkSection == null )
            {
                personalLinkSection = new PersonalLinkSection { Id = 0 };
                isNew = true;
            }

            hfPersonalLinkSectionId.SetValue( personalLinkSection.Id );

            if ( personalLinkSection != null && personalLinkSection.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                hfPersonalLinkSectionId.Value = personalLinkSection.Id.ToString();

                bool readOnly = false;
                bool editAllowed = isNew || personalLinkSection.IsAuthorized( Authorization.EDIT, CurrentPerson );
                nbEditModeMessage.Text = string.Empty;

                if ( !editAllowed )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ContentChannel.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    ShowReadonlyDetails( personalLinkSection );
                }
                else
                {
                    btnEdit.Visible = true;
                    if ( personalLinkSection.Id > 0 )
                    {
                        ShowReadonlyDetails( personalLinkSection );
                    }
                    else
                    {
                        ShowEditDetails( personalLinkSection );
                    }
                }

                btnSecurity.Visible = personalLinkSection.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                btnSecurity.Title = personalLinkSection.Name;
                btnSecurity.EntityId = personalLinkSection.Id;

                btnSave.Visible = !readOnly;
            }
            else
            {
                nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToView( ContentChannel.FriendlyTypeName );
                pnlEditDetails.Visible = false;
                pnlViewDetails.Visible = false;
                this.HideSecondaryBlocks( true );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="personalLinkSection">The personal link section.</param>
        private void ShowReadonlyDetails( PersonalLinkSection personalLinkSection )
        {
            SetEditMode( false );

            if ( personalLinkSection == null )
            {
                return;
            }

            lActionTitle.Text = personalLinkSection.Name.FormatAsHtmlTitle() + PANEL_NAME;

            var descriptionList = new DescriptionList();
            descriptionList.Add( "Name", personalLinkSection.Name );
            lDescription.Text = descriptionList.Html;
            hlShared.Visible = personalLinkSection.IsShared;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="personalLinkSection">The personal link section.</param>
        public void ShowEditDetails( PersonalLinkSection personalLinkSection )
        {
            if ( personalLinkSection.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( PANEL_NAME );
                hlShared.Visible = GetAttributeValue( AttributeKey.SharedSection ).AsBoolean();
            }
            else
            {
                lActionTitle.Text = personalLinkSection.Name.FormatAsHtmlTitle() + PANEL_NAME;
                hlShared.Visible = personalLinkSection.IsShared;
            }

            SetEditMode( true );

            tbName.Text = personalLinkSection.Name;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        #endregion Internal Methods
    }
}