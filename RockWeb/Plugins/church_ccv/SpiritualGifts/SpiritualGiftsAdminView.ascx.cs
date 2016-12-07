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
    [DisplayName( "Spiritual Gifts Admin View" )]
    [Category( "CCV > Spiritual Gifts" )]
    [Description( "Displays the Spiritual Gifts for a person. Designed to be displayed on a Person Profile page." )]
    [BooleanField( "Enable Debug", "Show lava merge fields.", false, "" )]
    public partial class SpiritualGiftsAdminView : PersonBlock
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
                List<string> giftsList = null;

                // grab the gifts for the person being viewed
                Person.LoadAttributes( );
                var spiritualGiftsAttrib = Person.AttributeValues[ "SpiritualGifts" ];

                // since we're doing string parsing, wrap this in a try/catch so we can display a readable error
                // if the format is off, for some reason
                try
                {
                    // do they have spiritual gifts set?
                    if( string.IsNullOrWhiteSpace( spiritualGiftsAttrib.Value ) == false )
                    {
                        // parse and sort the gifts
                        giftsList = ParseSpiritualGifts( spiritualGiftsAttrib.Value );
                    }

                    // we'll either render the gifts, or a note saying they haven't taken the test.
                    Render( giftsList );
                }
                catch
                {
                    lSpiritualGifts.Text = "Error trying to display Spiritual Gifts.";
                }
            }
        }
        

#endregion
        

        #region Methods
        
        private List<string> ParseSpiritualGifts( string giftsStr )
        {
            // first break them up by comma
            List<string> giftsList = giftsStr.Split( ',' ).ToList( );

            // now sort them by score. we know the format is "gift SPACE score", 
            // so we'll jump to first SPACE + 1, which puts us at the score, and cast that to an int.
            giftsList.Sort( 
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
            
            return giftsList;
        }
        
        private void Render( List<string> spiritualGifts )
        {
            if ( spiritualGifts != null )
            {
                // render the gifts as an ordered (numbered) list
                lSpiritualGifts.Text = "<ol>";

                foreach ( string gift in spiritualGifts )
                {
                    // get just the gift text (they don't want the score portion)
                    string giftName = gift.Substring( 0, gift.IndexOf( ' ' ) );

                    lSpiritualGifts.Text += "<li style='margin: 10px'>" + giftName + "</li>";
                }

                lSpiritualGifts.Text += "</ol>";
            }
            else
            {
                lSpiritualGifts.Text = "Spiritual Gifts test not taken yet.";
            }
        }

        #endregion
    }
}