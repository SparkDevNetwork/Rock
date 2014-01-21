// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Security.Application;
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
    [DisplayName( "Prayer Session" )]
    [Category( "Prayer" )]
    [Description( "Allows a user to start a session to pray for active, approved prayer requests." )]

    [TextField( "Welcome Introduction Text", "Some text (or HTML) to display on the first step.", false, "<h2>Let's get ready to pray...</h2>", "", 1 )]
    [CategoryField( "Category", "A top level category. This controls which categories are shown when starting a prayer session.", false, "Rock.Model.PrayerRequest", "", "", false, "", "Filtering", 2, "CategoryGuid" )]
    [BooleanField( "Enable Prayer Team Flagging", "If enabled, members of the prayer team can flag a prayer request if they feel the request is inappropriate and needs review by an administrator.", false, "Flagging", 3, "EnableCommunityFlagging" )]
    [IntegerField( "Flag Limit", "The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.", false, 1, "Flagging", 4 )]
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

        public int? NoteTypeId
        {
            get { return ViewState["NoteTypeId"] as int?; }
            set { ViewState["NoteTypeId"] = value; }
        }

        public int? CurrentPrayerRequestId
        {
            get { return ViewState["CurrentPrayerRequestId"] as int?; }
            set { ViewState["CurrentPrayerRequestId"] = value; }
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
                SetNoteType();
                lbStart.Focus();
                lbFlag.Visible = _enableCommunityFlagging;
            }

            notesComments.NoteTypeId = NoteTypeId;
            notesComments.EntityId = CurrentPrayerRequestId;

            if ( lbNext.Visible )
            {
                lbNext.Focus();
            }
        }

        #endregion

        #region Events

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

            lbNext.Focus();

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
            int currentNumber = index + 1;
            if ( currentNumber <= prayerRequestIds.Count )
            {
                UpdateSessionCountLabel( currentNumber, prayerRequestIds.Count );

                hfPrayerIndex.Value = index.ToString();
                PrayerRequestService service = new PrayerRequestService();
                PrayerRequest request = service.Get( prayerRequestIds[index] );
                ShowPrayerRequest( request, service );
            }
            else
            {
                pnlFinished.Visible = true;
                pnlPrayer.Visible = false;
                lbStartAgain.Focus();
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
            lbStart.Focus();                

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

        #endregion

        #region Methods

        private void SetNoteType()
        {
            var entityTypeId = EntityTypeCache.Read( typeof( PrayerRequest ) ).Id;
            string noteTypeName = GetAttributeValue( "NoteType" );

            var service = new NoteTypeService();
            var noteType = service.Get( entityTypeId, noteTypeName );

            // If a note type with the specified name does not exist, create one
            if ( noteType == null )
            {
                noteType = new NoteType();
                noteType.IsSystem = false;
                noteType.EntityTypeId = entityTypeId;
                noteType.EntityTypeQualifierColumn = string.Empty;
                noteType.EntityTypeQualifierValue = string.Empty;
                noteType.Name = noteTypeName;
                service.Add( noteType, CurrentPersonId );
                service.Save( noteType, CurrentPersonId );
            }

            NoteTypeId = noteType.Id;
        }

        /// <summary>
        /// Updates the Hightlight label that shows how many prayers have been made out of the total for this session.
        /// </summary>
        /// <param name="currentNumber"></param>
        /// <param name="total"></param>
        private void UpdateSessionCountLabel( int currentNumber, int total )
        {
            hlblNumber.Text = string.Format( "{0} of {1}", currentNumber, total );
            //hlblNumber.ToolTip = string.Format( "You've prayed for {0} out of {1} requests.", currentNumber, total );
        }

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
                UpdateSessionCountLabel( 1, list.Count );
                hfPrayerIndex.Value = "0";
                PrayerRequest request = prayerRequests.First();
                ShowPrayerRequest( request, service );
            }
        }

        /// <summary>
        /// Displays the details for a single, given prayer request.
        /// </summary>
        /// <param name="prayerRequest"></param>
        /// <param name="service"></param>
        private void ShowPrayerRequest( PrayerRequest prayerRequest, PrayerRequestService service )
        {
            pnlPrayer.Visible = true;
            pPrayerAnswer.Visible = false;

            prayerRequest.PrayerCount = ( prayerRequest.PrayerCount ?? 0 ) + 1;
            hlblPrayerCountTotal.Text = prayerRequest.PrayerCount.ToString() + " team prayers";
            hlblUrgent.Visible = prayerRequest.IsUrgent ?? false;
            lTitle.Text = prayerRequest.FullName.FormatAsHtmlTitle();

            //lPrayerText.Text = prayerRequest.Text.EncodeHtmlThenConvertCrLfToHtmlBr();
            lPrayerText.Text = ScrubHtmlAndConvertCrLfToBr( prayerRequest.Text );
            hlblCategory.Text = prayerRequest.Category.Name;

            // Show their answer if there is one on the request.
            if ( !string.IsNullOrWhiteSpace( prayerRequest.Answer ) )
            {
                pPrayerAnswer.Visible = true;
                lPrayerAnswerText.Text = prayerRequest.Answer.EncodeHtml().ConvertCrLfToHtmlBr();
            }

            // put the request's id in the hidden field in case it needs to be flagged.
            hfIdValue.SetValue( prayerRequest.Id );

            lPersonIconHtml.Text = Person.GetPhotoImageTag( prayerRequest.RequestedByPerson, 50, 50 );

            notesComments.Visible = prayerRequest.AllowComments ?? false;
            if (notesComments.Visible)
            {
                notesComments.EntityId = prayerRequest.Id;
                notesComments.RebuildNotes( true );
            }
            CurrentPrayerRequestId = prayerRequest.Id;

            // save because the prayer count was just modified.
            service.Save( prayerRequest, this.CurrentPersonId );
        }

        #endregion

        # region Possible Extension Method -- but it depends on Microsoft.Security.Application.Sanitizer

        /// <summary>
        /// Scrubs any html from the string but converts carriage returns into html &lt;br/&gt; suitable for web display.
        /// </summary>
        /// <param name="str">a string that may contain unsanitized html and carriage returns</param>
        /// <returns>a string that has been scrubbed of any html with carriage returns converted to html br</returns>
        public static string ScrubHtmlAndConvertCrLfToBr( string str )
        {
            if ( str == null )
            {
                return string.Empty;
            }

            // Note: \u00A7 is the section symbol

            // First we convert newlines and carriage returns to a character that can
            // pass through the Sanitizer.
            str = str.Replace( Environment.NewLine, "\u00A7" ).Replace( "\x0A", "\u00A7" );

            // Now we pass it to sanitizer and then convert those section-symbols to <br/>
            return Sanitizer.GetSafeHtmlFragment( str ).ConvertCrLfToHtmlBr().Replace( "\u00A7", "<br/>" );
        }

        #endregion

    }
}