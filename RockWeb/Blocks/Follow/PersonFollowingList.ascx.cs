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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Follow
{
    /// <summary>
    /// Block for displaying people that current person follows.
    /// </summary>
    [DisplayName( "Person Following List" )]
    [Category( "Follow" )]
    [Description( "Block for displaying people that current person follows." )]

    [Rock.SystemGuid.BlockTypeGuid( "BD548744-DC6D-4870-9FED-BB9EA24E709B" )]
    public partial class PersonFollowingList : RockBlock, ICustomGridColumns
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            base.OnInit( e );

            gFollowings.DataKeyNames = new string[] { "Id" };
            gFollowings.IsDeleteEnabled = false;
            gFollowings.GridRebind += gFollowings_GridRebind;
            gFollowings.RowDataBound += new GridViewRowEventHandler( gFollowings_RowDataBound );
            gFollowings.PersonIdField = "Id";

            var lbUnfollow = new LinkButton();
            lbUnfollow.ID = "lbUnfollow";
            lbUnfollow.CssClass = "btn btn-default btn-sm pull-left btn-grid-custom-action js-unfollow";
            lbUnfollow.Text = "<i class='fa fa-flag-o'></i> Unfollow";
            lbUnfollow.Click += lbUnfollow_Click;
            gFollowings.Actions.AddCustomActionControl( lbUnfollow );

            string unfollowConfirmScript = @"
    $('a.js-unfollow').on('click', function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to unfollow the selected people?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gFollowings, gFollowings.GetType(), "unfollowConfirmScript", unfollowConfirmScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the RowDataBound event of the gFollowings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gFollowings_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.Header )
            {
                e.Row.Cells[4].Text = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) ).Value;
                e.Row.Cells[5].Text = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ).Value;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbUnfollow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void lbUnfollow_Click( object sender, EventArgs e )
        {
            var itemsSelected = new List<int>();
            gFollowings.SelectedKeys.ToList().ForEach( f => itemsSelected.Add( f.ToString().AsInteger() ) );

            if ( itemsSelected.Any() )
            {
                var rockContext = new RockContext();
                var personAliasService = new PersonAliasService( rockContext );
                var followingService = new FollowingService( rockContext );

                var paQry = personAliasService.Queryable()
                    .Where( p => itemsSelected.Contains( p.PersonId ) )
                    .Select( p => p.Id );

                int personAliasEntityTypeId = EntityTypeCache.Get( "Rock.Model.PersonAlias" ).Id;
                foreach ( var following in followingService.Queryable()
                    .Where( f =>
                        f.EntityTypeId == personAliasEntityTypeId &&
                        string.IsNullOrEmpty( f.PurposeKey ) &&
                        paQry.Contains( f.EntityId ) &&
                        f.PersonAliasId == CurrentPersonAlias.Id ) )
                {
                    followingService.Delete( following );
                }

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gFollowings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gFollowings_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the UserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( CurrentPersonAlias != null )
            {
                var rockContext = new RockContext();
                var followingService = new FollowingService( rockContext );

                var qry = followingService.GetFollowedPersonItems( CurrentPersonAlias.Id );

                // Sort
                SortProperty sortProperty = gFollowings.SortProperty;
                if ( sortProperty == null )
                {
                    sortProperty = new SortProperty( new GridViewSortEventArgs( "LastName,NickName", SortDirection.Ascending ) );
                }

                Guid homePhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
                Guid cellPhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();
                Guid adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
                Guid marriedGuid = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();

                gFollowings.DataSource = qry.Sort( sortProperty )
                    .Select( p => new
                        {
                            Person = p,
                            p.Id,
                            p.LastName,
                            p.NickName,
                            p.BirthDate,
                            p.Email,
                            HomePhone = p.PhoneNumbers
                                .Where( n => n.NumberTypeValue.Guid.Equals(homePhoneGuid))
                                .Select( n => n.NumberFormatted)
                                .FirstOrDefault(),
                            CellPhone = p.PhoneNumbers
                                .Where( n => n.NumberTypeValue.Guid.Equals( cellPhoneGuid ) )
                                .Select( n => n.NumberFormatted )
                                .FirstOrDefault(),
                            Spouse = p.Members
                                .Where( m =>
                                    p.MaritalStatusValue.Guid.Equals(marriedGuid) &&
                                    m.GroupRole.Guid.Equals(adultGuid))
                                .SelectMany( m => m.Group.Members)
                                .Where( m =>
                                    m.PersonId != p.Id &&
                                    m.GroupRole.Guid.Equals(adultGuid) &&
                                    m.Person.MaritalStatusValue.Guid.Equals(marriedGuid) )
                                .Select( s => s.Person )
                                .FirstOrDefault()
                        } ).ToList();

                gFollowings.DataBind();
            }
        }

        #endregion
}
}