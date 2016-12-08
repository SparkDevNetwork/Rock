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
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock;
using Attribute = Rock.Model.Attribute;
using Rock.Security;
using Rock.Web.Cache;
using Newtonsoft.Json;

namespace RockWeb.Plugins.church_ccv.SpiritualGifts
{
    [DisplayName( "Spiritual Gifts View" )]
    [Category( "CCV > Spiritual Gifts" )]
    [Description( "Displays the Spiritual Gifts for a person. Designed to be displayed on a Person Profile page." )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group details.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "", "", 8 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 10 )]
    public partial class SpiritualGiftsView : RockBlock
    {
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
                // grab the gifts for the person being viewed
                CurrentPerson.LoadAttributes( );
                var spiritualGiftsAttrib = CurrentPerson.AttributeValues[ "SpiritualGifts" ];

                // since we're doing string parsing, wrap this in a try/catch so we can display a readable error
                // if the format is off, for some reason
                try
                {
                    List<string> giftNames = new List<string>( );
                    List<int> giftScores = new List<int>( );

                    // do they have spiritual gifts set?
                    if( string.IsNullOrWhiteSpace( spiritualGiftsAttrib.Value ) == false )
                    {
                        // parse and sort the gifts
                        ParseSpiritualGifts( spiritualGiftsAttrib.Value, ref giftNames, ref giftScores );
                    }

                    // we'll either render the gifts, or a note saying they haven't taken the test.
                    Render( giftNames, giftScores );
                }
                catch
                {
                    lMainContent.Text = "Error trying to display Spiritual Gifts.";
                }
            }
        }
        
#endregion
        

        #region Methods
        
        private void ParseSpiritualGifts( string giftsStr, ref List<string> giftNames, ref List<int> giftScores )
        {
            // first break them up by comma
            List<string> fullGiftsList = giftsStr.Split( ',' ).ToList( );

            // now sort them by score. we know the format is "gift SPACE score", 
            // so we'll jump to first SPACE + 1, which puts us at the score, and cast that to an int.
            fullGiftsList.Sort( 
                delegate( string x, string y )
                {
                    int xScore = int.Parse( x.Substring( x.IndexOf( ' ' ) + 1 ) );
                    int yScore = int.Parse( y.Substring( y.IndexOf( ' ' ) + 1 ) );

                    if( xScore  < yScore )
                    {
                        return 1;
                    }
                    return -1;
                });


            // now remove the trailing scores from each gift, because we simply want to list their gifts
            foreach ( string gift in fullGiftsList )
            {
                // get the index
                int giftSeperatorIndex = gift.IndexOf( ' ' );

                // get just the gift text (they don't want the score portion)
                string giftName = gift.Substring( 0, gift.IndexOf( ' ' ) );

                // now get the score
                int giftScore = int.Parse( gift.Substring( giftSeperatorIndex + 1 ) );

                // and put them in the lists
                giftNames.Add( giftName );
                giftScores.Add( giftScore );
            }
        }
        
        private void Render( List<string> spiritualGiftNames, List<int> spiritualGiftScores )
        {
            // setup merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "SpiritualGiftNames", spiritualGiftNames );
            mergeFields.Add( "SpiritualGiftScores", spiritualGiftScores );

            // show debug info
            bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();
            if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
            {
                string postbackCommands = @"<h5>Available Postback Commands</h5>
                                            <ul>
                                                <li><strong>EditGroup:</strong> Shows a panel for modifing group info. Expects a group id. <code>{{ Group.Id | Postback:'EditGroup' }}</code></li>
                                                <li><strong>AddGroupMember:</strong> Shows a panel for adding group info. Does not require input. <code>{{ '' | Postback:'AddGroupMember' }}</code></li>
                                                <li><strong>SendCommunication:</strong> Sends a communication to all group members on behalf of the Current User. This will redirect them to the communication page where they can author their email. <code>{{ '' | Postback:'SendCommunication' }}</code></li>
                                            </ul>";

                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo( null, string.Empty, postbackCommands );
            }

            // render
            string template = GetAttributeValue( "LavaTemplate" );
            lMainContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlSpiritualGifts.ClientID );
        }

        #endregion
    }
}