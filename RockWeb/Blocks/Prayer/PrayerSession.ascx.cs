//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [TextField( "Welcome Introduction Text", "Some text (or HTML) to display on the first step.", false, "<h2>Welcome to our Prayer Ministry.</h2>", "", 1 )]
    [CategoryField( "Category", "A top level category. This controls which categories are shown when starting a prayer session.", false, "Rock.Model.PrayerRequest", "", "", false, "", "Filtering", 2, "CategoryGuid" )]
    [BooleanField( "Enable Community Flagging", "If enabled, members of the prayer team will be able to a flag prayer request if they feel it is inappropriate.", false, "Flagging", 3 )]
    [IntegerField( "Flag Limit", "The number of flags a prayer request has to get from the community before it is automatically unapproved.", false, 1, "Flagging", 4 )]
    [TextField( "Note Type", "The note type name for these prayer request comments.", false, "Prayer Comment", "Advanced", 0, "NoteType" )]

    public partial class PrayerSession : RockBlock
    {
        #region Fields
        private string _sessionKey = "Rock.PrayerRequestIDs";
        private bool _enableCommunityFlagging = false;
        private string _categoryGuidString = "";
        private int? _flagLimit = 1;
        private string[] _savedCategoryIdsSetting;
        #endregion

        #region Properties
        protected int PrayerCommentNoteTypeId
        {
            get { return ViewState["PrayerCommentNoteTypeId"] as int? ?? 0; }
            set { ViewState["PrayerCommentNoteTypeId"] = value; }
        }
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            mdFlag.SaveClick += mdFlag_SaveClick;
            rptComments.ItemDataBound += rptComments_ItemDataBound;
            rptComments.ItemCommand += rptComments_ItemCommand;

            _flagLimit = GetAttributeValue( "FlagLimit" ).AsInteger();
            _categoryGuidString = GetAttributeValue( "CategoryGuid" );
            _enableCommunityFlagging = GetAttributeValue( "EnableCommunityFlagging" ).AsBoolean();
            lWelcomeInstructions.Text = GetAttributeValue( "WelcomeIntroductionText" );
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                DisplayCategories();
                GetNoteType();
                lbFlag.Visible = _enableCommunityFlagging;
            }
        }

        #endregion

        #region Events

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler that saves the user's category preferences and starts a prayer session
        /// for their selected categories.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbStart_Click( object sender, EventArgs e )
        {
            // Make sure they selected at least one category
            if ( cblCategories.SelectedValues.Count == 0 )
            {
                nbSelectCategories.Visible = true;
                return;
            }
            else
            {
                nbSelectCategories.Visible = false;
            }

            pnlChooseCategories.Visible = false;

            string settingPrefix = string.Format( "prayer-categories-{0}-", this.BlockId );
            SaveUserPreferences( settingPrefix );

            SetAndDisplayPrayerRequests( cblCategories );
        }

        /// <summary>
        /// Handler that gets the next prayer request and updates its prayer count.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbNext_Click( object sender, EventArgs e )
        {
            int index = hfPrayerIndex.ValueAsInt();

            index++;

            List<int> prayerRequestIds  = (List<int>) Session[ _sessionKey ];
            if ( prayerRequestIds.Count - 1 >= index )
            {
                hlblNumber.Text = ( index + 1 ).ToString();
                hlblNumber.Text = string.Format( "# {0}", hlblNumber.Text, prayerRequestIds.Count );
                hlblNumber.ToolTip = string.Format( "{0} of {1}", hlblNumber.Text, prayerRequestIds.Count );

                hfPrayerIndex.Value = index.ToString();
                PrayerRequestService service = new PrayerRequestService();
                PrayerRequest request = service.Get( prayerRequestIds[index] );
                ShowPrayerRequest( request );
                // save because the prayer count was just modified.
                service.Save( request, this.CurrentPersonId );
            }
            else
            {
                pnlFinished.Visible = true;
                pnlPrayer.Visible = false;
            }
        }

        /// <summary>
        /// Handler for when the user has decided to call it quits for their current prayer session.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbStop_Click( object sender, EventArgs e )
        {
            pnlFinished.Visible = true;
            pnlPrayer.Visible = false;
        }

        /// <summary>
        /// Handler for when/if the user wants to start a new prayer session.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbStartAgain_Click( object sender, EventArgs e )
        {
            pnlChooseCategories.Visible = true;
            pnlFinished.Visible = false;
            pnlNoPrayerRequestsMessage.Visible = false;
            pnlPrayer.Visible = false;
            
            DisplayCategories();
        }

        /// <summary>
        /// Called when the user clicks on the "Flag" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbFlag_Click( object sender, EventArgs e )
        {
            mdFlag.SaveButtonText = "Yes, Flag This Request";
            mdFlag.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdFlag control and flags the prayer request and moves to the next.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdFlag_SaveClick( object sender, EventArgs e )
        {
            int prayerRequestId = hfIdValue.ValueAsInt();

            var service = new PrayerRequestService();

            PrayerRequest request = service.Get( prayerRequestId );

            if ( request != null )
            {
                request.FlagCount = ( request.FlagCount ?? 0 ) + 1;
                if ( request.FlagCount >= _flagLimit )
                {
                    request.IsApproved = false;
                }
                service.Save( request, this.CurrentPersonId );
            }

            mdFlag.Hide();
            lbNext_Click( sender, e );
        }

        /// <summary>
        /// Called when an item is clicked in the repeater.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        protected void rptComments_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if (e.CommandName == "Delete")
            {
                int noteId = Convert.ToInt32(e.CommandArgument);
                NoteService noteService = new NoteService();
                Note note = noteService.Get( noteId );
                noteService.Delete( note, CurrentPersonId );
                noteService.Save( note, CurrentPersonId );

                int prayerRequestId = hfIdValue.ValueAsInt();
                ShowComments( prayerRequestId );
            }
        }

        /// <summary>
        /// Called for each comment (Note) that is bound to the repeater.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptComments_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var note = e.Item.DataItem as Rock.Model.Note;

                if ( note != null )
                {
                    var lCommentBy = e.Item.FindControl( "lCommentBy" ) as Literal;
                    var lCommentDate = e.Item.FindControl( "lCommentDate" ) as Literal;
                    var lCommentText = e.Item.FindControl( "lCommentText" ) as Literal;
                    var bbtnDeleteComment = e.Item.FindControl( "bbtnDeleteComment" ) as BootstrapButton;
                    var lCommenterIcon = e.Item.FindControl( "lCommenterIcon" ) as Literal;

                    note.LoadAttributes();
                    var personId = note.GetAttributeValue( "NoteCreator" );
                    if ( personId != null )
                    {
                        lCommenterIcon.Text = GetPhotoUrl( new PersonService().Get( personId.AsInteger() ?? 0 ), useIconIfEmpty: false, useBlankImageIfEmpty: true, cssClassName: "media-object" );
                    }
                    else
                    {
                        lCommenterIcon.Text = "<i class='fa fa-user'></i> ";
                    }

                    lCommentBy.Text = Server.HtmlEncode( note.Caption );
                    lCommentDate.Text = note.CreationDateTime.ToRelativeDateString( 6 ) ;
                    lCommentText.Text = Server.HtmlEncode( note.Text );

                    // TODO figure out how to determine who the note "owner" is.
                    bbtnDeleteComment.Visible = note.IsAuthorized( "Administrate", CurrentPerson );
                    bbtnDeleteComment.CommandArgument = note.Id.ToStringSafe();
                    bbtnDeleteComment.CommandName = "Delete";
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Displays any 'active' prayer categories or shows a message if there are none.
        /// </summary>
        private void DisplayCategories()
        {
            // If there are no categories, then it means there are no prayer requests (in those categories)
            if ( !BindCategories( _categoryGuidString ) )
            {
                cblCategories.Visible = false;
                pnlChooseCategories.Visible = false;
                pnlNoPrayerRequestsMessage.Visible = true;
            }
        }

        /// <summary>
        /// Binds the 'active' categories for the given top-level category GUID to the list for 
        /// the user to choose.
        /// </summary>
        /// <param name="categoryGuid">the guid string of a top-level prayer category</param>
        /// <returns>true if there were active categories or false if there were none</returns>
        private bool BindCategories( string categoryGuid )
        {
            string settingPrefix = string.Format( "prayer-categories-{0}-", this.BlockId );

            var prayerRequestQuery = new PrayerRequestService().GetActiveApprovedUnexpired();

            // Filter categories if one has been selected in the configuration
            if ( !string.IsNullOrEmpty( categoryGuid ) )
            {
                Guid guid = new Guid( categoryGuid );
                var filterCategory = CategoryCache.Read( guid );
                if ( filterCategory != null )
                {
                    prayerRequestQuery = prayerRequestQuery.Where( p => p.Category.ParentCategoryId == filterCategory.Id );
                }
            }

            var inUseCategories = prayerRequestQuery
                .Where( p => p.Category != null )
                .Select( p => new { p.Category.Id, p.Category.Name } )
                .GroupBy( g => new { g.Id, g.Name } )
                .OrderBy( g => g.Key.Name )
                .Select( a => new
                {
                    Id = a.Key.Id,
                    Name = a.Key.Name + " (" + System.Data.Entity.SqlServer.SqlFunctions.StringConvert( (double)a.Count() ).Trim() + ")",
                    Count = a.Count()
                    //,Checked = selectedIDs.Contains( a.Key.Id )
                } );

            cblCategories.DataTextField = "Name";
            cblCategories.DataValueField = "Id";
            cblCategories.DataSource = inUseCategories.ToList();
            cblCategories.DataBind();

            // use the users preferences to set which items are checked.
            _savedCategoryIdsSetting = this.GetUserPreference( settingPrefix ).SplitDelimitedValues();
            for ( int i = 0; i < cblCategories.Items.Count; i++ )
            {
                ListItem item = (ListItem)cblCategories.Items[i];
                item.Selected = _savedCategoryIdsSetting.Contains( item.Value );
            }

            return ( inUseCategories.Count() > 0 );
        }

        /// <summary>
        /// Saves the users selected prayer categories for use during the next prayer session.
        /// </summary>
        /// <param name="settingPrefix"></param>
        private void SaveUserPreferences( string settingPrefix )
        {
            var previouslyCheckedIds = this.GetUserPreference( settingPrefix ).SplitDelimitedValues();

            IEnumerable<string> allIds = cblCategories.Items.Cast<ListItem>()
                              .Select( i => i.Value );

            // find the items that were previously saved but are no longer in the checkboxlist...
            // because we want to retain those as they may be active categories again in the future.
            var itemsToKeep = previouslyCheckedIds.Except( allIds );

            string categoryValues = cblCategories.SelectedValuesAsInt
                .Where( v => v != 0 )
                .Select( c => c.ToString() )
                .Concat( itemsToKeep )
                .ToList()
                .AsDelimited( "," );

            this.SetUserPreference( settingPrefix, categoryValues );
        }

        /// <summary>
        /// Finds all approved prayer requests for the given selected categories and orders them by least prayed-for.
        /// Also updates the prayer count for the first item in the list.
        /// </summary>
        /// <param name="categoriesList"></param>
        private void SetAndDisplayPrayerRequests( RockCheckBoxList categoriesList )
        {
            PrayerRequestService service = new PrayerRequestService();
            var prayerRequests = service.GetByCategoryIds( categoriesList.SelectedValuesAsInt ).OrderByDescending( p => p.IsUrgent ).ThenBy( p => p.PrayerCount );
            List<int> list = prayerRequests.Select( p => p.Id ).ToList<int>();

            Session[ _sessionKey ] = list;
            if ( list.Count > 0 )
            {
                hlblNumber.Text = "1";
                hlblNumber.Text = string.Format( "# {0}", hlblNumber.Text, list.Count );
                hlblNumber.ToolTip = string.Format( "{0} of {1}", hlblNumber.Text, list.Count );
                hfPrayerIndex.Value = "0";
                PrayerRequest request = prayerRequests.First();
                ShowPrayerRequest( request );
                // save because the prayer count was just modified.
                service.Save( request, this.CurrentPersonId );
            }
        }

        /// <summary>
        /// Displays the details for a single, given prayer request.
        /// </summary>
        /// <param name="prayerRequest"></param>
        private void ShowPrayerRequest( PrayerRequest prayerRequest )
        {
            pnlPrayer.Visible = true;
            pPrayerAnswer.Visible = false;

            prayerRequest.PrayerCount = ( prayerRequest.PrayerCount ?? 0 ) + 1;
            hlblPrayerCountTotal.Text = prayerRequest.PrayerCount.ToString() + " prayers";
            badgePrayerCountTotal.Text = prayerRequest.PrayerCount.ToString();
            hlblUrgent.Visible = prayerRequest.IsUrgent ?? false;
            lTitle.Text = prayerRequest.FullName.FormatAsHtmlTitle();
            lPrayerText.Text = Server.HtmlEncode( prayerRequest.Text );
            hlblCategory.Text = prayerRequest.Category.Name;

            // Answer?
            if ( !string.IsNullOrWhiteSpace( prayerRequest.Answer ) )
            {
                pPrayerAnswer.Visible = true;
                lPrayerAnswerText.Text = prayerRequest.Answer;
            }

            // put the request's id in the hidden field in case it needs to be flagged.
            hfIdValue.SetValue( prayerRequest.Id );

            lPersonIconHtml.Text = GetPhotoUrl( prayerRequest.RequestedByPerson );

            pnlComments.Visible = prayerRequest.AllowComments ?? false;
            if ( rptComments.Visible )
            {
                ShowComments( prayerRequest );
            }
        }

        /// <summary>
        /// Gets an appropriate URL for a photo of the given person.  Uses the person's photo if available
        /// or can use an Icon or the 'blank' (no photo) image from Rock Assets.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="useIconIfEmpty">set to true to use a bootstrap icon if the person has no photo.</param>
        /// <param name="useBlankImageIfEmpty">set to true to use the no-photo image if the person has no photo.</param>
        /// <param name="cssClassName">If supplied, will be used for the css class of the image </param>
        /// <returns></returns>
        private string GetPhotoUrl( Person person, bool useIconIfEmpty = false, bool useBlankImageIfEmpty = false, string cssClassName = "" )
        {
            string personIconHtml = string.Empty;
            if ( person != null && person.PhotoId.HasValue )
            {
                var getImageUrl = ResolveRockUrl( "~/GetImage.ashx" );
                string imageUrlFormat = "<img src='" + getImageUrl + "?id={0}&width=50&height=50' class='{1}'/>";
                personIconHtml = string.Format( imageUrlFormat, person.PhotoId.Value.ToString(), cssClassName );
            }
            else if ( useIconIfEmpty )
            {
                personIconHtml = string.Format( "<i class='fa fa-user {0}'></i> ", cssClassName );
            }
            else if ( useBlankImageIfEmpty )
            {
                var getImageUrl = ResolveRockUrl( "~/Assets/Images/person-no-photo.svg" );
                string imageUrlFormat = "<img src='" + getImageUrl + "' style='width:50px; height:50px alt='{0}' class='{1}' />";

                var fullname = ( person != null ) ? person.FullName : "";
                personIconHtml = string.Format( imageUrlFormat, Server.HtmlEncode( fullname ), cssClassName );
            }

            return personIconHtml;
        }

        /// <summary>
        /// Gets the configured NoteType for the PrayerRequest entity.
        /// </summary>
        private void GetNoteType()
        {
            string noteTypeName = GetAttributeValue( "NoteType" );

            var _prayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( new PrayerRequest().GetType().FullName );
            var noteTypeService = new NoteTypeService();
            var noteType = noteTypeService.Get( (int)_prayerRequestEntityTypeId, noteTypeName );
            
            // all that work to get this... for use later.
            PrayerCommentNoteTypeId = noteType.Id;
        }

        /// <summary>
        /// Binds all comments for the given prayer request Id.
        /// </summary>
        /// <param name="prayerRequestId">the id of a prayer request</param>
        private void ShowComments( int prayerRequestId )
        {
            PrayerRequestService prayerRequestService = new PrayerRequestService();
            var prayerRequest = prayerRequestService.Get( prayerRequestId );
            ShowComments( prayerRequest );
        }

        /// <summary>
        /// Binds all comments for the given prayer request.
        /// </summary>
        /// <param name="prayerRequest">a prayer request</param>
        private void ShowComments( PrayerRequest prayerRequest )
        {
            var notes = new List<Note>();
            notes = new NoteService().Get( (int)PrayerCommentNoteTypeId, prayerRequest.Id ).ToList();
            lMeIconHtml.Text = GetPhotoUrl( CurrentPerson, useIconIfEmpty: true, cssClassName: "media-object" );
            rptComments.DataSource = notes;
            rptComments.DataBind();
        }

        #endregion

        /// <summary>
        /// Saves the comment for the prayer request being viewed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void bbtnSaveComment_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( tbComment.Text ) )
            {
                return;
            }

            int prayerRequestId = hfIdValue.ValueAsInt();

            NoteService service = new NoteService();

            var note = new Note();
            note.IsSystem = false;
            note.IsAlert = false;
            note.NoteTypeId = PrayerCommentNoteTypeId;
            note.EntityId = prayerRequestId;
            note.CreationDateTime = DateTime.Now;
            note.Text = tbComment.Text;
            note.Caption = CurrentPerson.FullName;

            service.Add( note, CurrentPersonId );
            service.Save( note, CurrentPersonId );

            note.LoadAttributes();
            note.SetAttributeValue( "NoteCreator", CurrentPersonId.ToStringSafe() );
            Rock.Attribute.Helper.SaveAttributeValues( note, CurrentPersonId );

            tbComment.Text = "";

            ShowComments( prayerRequestId );
        }
}
}