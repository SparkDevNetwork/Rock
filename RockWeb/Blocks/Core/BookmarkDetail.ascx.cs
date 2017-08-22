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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Bookmark Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given person bookmark." )]

    public partial class BookmarkDetail : RockBlock
    {

        #region Fields

        private bool _canConfigurePerson = false;

        #endregion

        #region Properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // assign attributes grid actions
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'This will delete the bookmark!');", PersonBookmark.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.PersonBookmark ) ).Id;

            _canConfigurePerson = IsUserAuthorized( Authorization.ADMINISTRATE );
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
                ShowDetail();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personBookmark = new PersonBookmarkService( rockContext ).Get( hfPersonBookmarkId.Value.AsInteger() );
            ShowEditDetails( personBookmark, rockContext );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var service = new PersonBookmarkService( rockContext );
            var personBookmark = service.Get( int.Parse( hfPersonBookmarkId.Value ) );

            if ( personBookmark != null )
            {
                if ( !personBookmark.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this workflow type.", ModalAlertType.Information );
                    return;
                }

                service.Delete( personBookmark );
                rockContext.SaveChanges();
            }

            // reload page
            var qryParams = new Dictionary<string, string>();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var service = new PersonBookmarkService( rockContext );

            PersonBookmark personBookmark = null;

            int? personBookmarkId = hfPersonBookmarkId.Value.AsIntegerOrNull();
            if ( personBookmarkId.HasValue )
            {
                personBookmark = service.Get( personBookmarkId.Value );
            }

            if ( personBookmark == null )
            {
                personBookmark = new PersonBookmark();
            }

            personBookmark.Name = tbName.Text;
            personBookmark.Url = tbUrl.Text;
            personBookmark.CategoryId = hfCategoryId.Value.AsIntegerOrNull();
            personBookmark.PersonAliasId = ppPerson.PersonAliasId;
            personBookmark.ModifiedByPersonAliasId = CurrentPersonAliasId;
            personBookmark.ModifiedDateTime = RockDateTime.Now;

            if ( !Page.IsValid || !personBookmark.IsValid )
            {
                return;
            }

            if ( personBookmark.Id.Equals( 0 ) )
            {
                service.Add( personBookmark );
            }
            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["personBookmarkId"] = personBookmark.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfPersonBookmarkId.Value.Equals( "0" ) )
            {
                int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();
                if ( parentCategoryId.HasValue )
                {
                    // Cancelling on Add, and we know the parentCategoryId, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    qryParams["CategoryId"] = parentCategoryId.ToString();
                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                PersonBookmarkService service = new PersonBookmarkService( new RockContext() );
                PersonBookmark item = service.Get( int.Parse( hfPersonBookmarkId.Value ) );
                ShowReadonlyDetails( item );
            }
        }

        #endregion


        #region Methods

        #region Show Details

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            int? personBookmarkId = PageParameter( "personBookmarkId" ).AsIntegerOrNull();
            int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();

            if ( !personBookmarkId.HasValue )
            {
                pnlDetails.Visible = false;
                return;
            }

            var rockContext = new RockContext();
            bool userAllowed = false;

            PersonBookmark personBookmark = null;

            if ( personBookmarkId.Value.Equals( 0 ) )
            {
                personBookmark = new PersonBookmark();
                personBookmark.Id = 0;
                personBookmark.CategoryId = parentCategoryId;
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
                userAllowed = true;
            }
            else
            {
                personBookmark = new PersonBookmarkService( rockContext ).Get( personBookmarkId.Value );
                pdAuditDetails.SetEntity( personBookmark, ResolveRockUrl( "~" ) );
            }

            if ( personBookmark == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfPersonBookmarkId.Value = personBookmark.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            // User must have 'Edit' rights to block, or 'Administrate' rights to workflow type
            if ( !_canConfigurePerson && !userAllowed )
            {
                readOnly = true;
                nbEditModeMessage.Heading = "Information";
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( PersonBookmark.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnSecurity.Visible = false;
                ShowReadonlyDetails( personBookmark );
            }
            else
            {
                btnEdit.Visible = true;

                btnSecurity.Title = "Secure " + personBookmark.Name;
                btnSecurity.EntityId = personBookmark.Id;

                if ( personBookmark.Id > 0 )
                {
                    ShowReadonlyDetails( personBookmark );
                }
                else
                {
                    ShowEditDetails( personBookmark, rockContext );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="personBookmark">Type of the personBookmark.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ShowEditDetails( PersonBookmark personBookmark, RockContext rockContext )
        {
            if ( personBookmark.Id == 0 )
            {

                lReadOnlyTitle.Text = ActionTitle.Add( PersonBookmark.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = personBookmark.Name;
            tbUrl.Text = personBookmark.Url;

            Person person = CurrentPerson;
            if ( personBookmark.PersonAliasId.HasValue )
            {
               var personAlias = new PersonAliasService( rockContext ).Get( personBookmark.PersonAliasId.Value );

                if ( personAlias != null )
                {
                    person = personAlias.Person;
                }
            }

            ppPerson.SetValue( person );
            ppPerson.Enabled = _canConfigurePerson;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="personBookmark">personBookmark.</param>
        private void ShowReadonlyDetails( PersonBookmark personBookmark )
        {
            SetEditMode( false );

            if ( personBookmark.CategoryId.HasValue )
            {
                hfCategoryId.SetValue( personBookmark.CategoryId.Value );
            }
            hfPersonBookmarkId.SetValue( personBookmark.Id );
            lReadOnlyTitle.Text = personBookmark.Name.FormatAsHtmlTitle();
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;
        }

        #endregion

        #endregion

    }
}