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
using Rock;
using Rock.Attribute;
using Rock.Model;
using System.Web.UI;
using Rock.Data;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Displays bookmark specific to the currently logged in user along with options to add new to the list.
    /// </summary>
    [DisplayName( "Bookmark" )]
    [Category( "Core" )]
    [Description( "Displays bookmark specific to the currently logged in user along with options to add new to the list." )]

    [EntityTypeField( "Entity Type", "Display categories associated with this type of entity" )]
    [LinkedPage( "Manage Page" )]
    public partial class Bookmarks : Rock.Web.UI.RockBlock
    {
        public const string CategoryNodePrefix = "C";

        /// <summary>
        /// The RestParams (used by the Markup)
        /// </summary>
        protected string RestParms;

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
                var currentPerson = CurrentPerson;
                if ( currentPerson != null )
                {
                    var currentUser = CurrentUser;
                    if ( currentUser == null || !currentUser.IsAuthenticated )
                    {
                        liDropdown.Visible = false;
                        return;
                    }
                }
                SetAddMode( false );
            }

            Guid? entityTypeGuid = GetAttributeValue( "EntityType" ).AsGuidOrNull();

            if ( entityTypeGuid.HasValue )
            {
                var cachedEntityType = Rock.Web.Cache.EntityTypeCache.Read( entityTypeGuid.Value );
                if ( cachedEntityType != null )
                {
                    Type entityType = cachedEntityType.GetEntityType();
                    string parms = string.Format( "?getCategorizedItems=true&defaultIconCssClass={1}&entityTypeId={0}", cachedEntityType.Id, Server.UrlEncode( "fa fa-file-text-o" ) );
                    RestParms = parms;

                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAdd_Click( object sender, EventArgs e )
        {
            SetAddMode( true );

            var currentPage = PageCache.Read( CurrentPageReference.PageId );
            tbName.Text = currentPage.PageTitle;
            tbUrl.Text = Request.Url.ToString();
        }

        /// <summary>
        /// Handles the Click event of the btnManage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnManage_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "ManagePage" );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            PersonBookmark bookmark;
            var rockContext = new RockContext();
            var personBookmarkService = new PersonBookmarkService( rockContext );

            bookmark = new PersonBookmark();
            personBookmarkService.Add( bookmark );
            bookmark.CreatedByPersonAliasId = CurrentPersonAliasId;
            bookmark.CreatedDateTime = RockDateTime.Now;
            bookmark.Name = tbName.Text;
            bookmark.Url = tbUrl.Text;
            bookmark.CategoryId = cpCategory.SelectedValueAsInt();
            bookmark.CreatedDateTime = RockDateTime.Now;
            bookmark.PersonAliasId = CurrentPersonAliasId;
            bookmark.CreatedByPersonAliasId = CurrentPersonAliasId;
            bookmark.ModifiedDateTime = RockDateTime.Now;
            bookmark.ModifiedByPersonAliasId = CurrentPersonAliasId;
            rockContext.SaveChanges();

            SetAddMode( false );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            SetAddMode( false );

            string script = @"$('.popovercontent').first().slideUp(function () {
            scrollbCategory.tinyscrollbar();
            });";
            ScriptManager.RegisterStartupScript( btnCancel, btnCancel.GetType(), "myScript", script, true );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the add mode.
        /// </summary>
        /// <param name="add">if set to <c>true</c> [add].</param>
        private void SetAddMode( bool add )
        {
            pnlBookmarkList.Visible = !add;
            pnlBookmarkDetail.Visible = add;
            btnAdd.Visible = !add;
            btnManage.Visible = !add;
            btnSave.Visible = add;
        }

        #endregion

    }
}