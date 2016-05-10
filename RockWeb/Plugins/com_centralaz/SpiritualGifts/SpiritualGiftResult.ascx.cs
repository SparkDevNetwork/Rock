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
using System.ComponentModel;
using System.Linq;

using com.centralaz.SpiritualGifts.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rockweb.Plugins.com_centralaz.SpiritualGifts
{
    /// <summary>
    /// View a person's spiritual gifts.
    /// This is used with permission from Greg Wiens: http://www.gregwiens.com/scid/
    /// </summary>
    [DisplayName( "Spiritual Gifts Result" )]
    [Category( "com_centralaz > Spiritual Gifts" )]
    [Description( "Allows you to view your spiritual gift score." )]
    [BooleanField( "Show Retake Test Button", "Whether to display the retake test button", false )]
    [IntegerField( "Minimum Days To Retake", "The number of days that must pass before the test can be taken again.", false, 30 )]
    [LinkedPage( "Spiritual Gift Test Page", "Page to take the spiritual gifts test. If blank no link is created.", false )]
    public partial class SpiritualGiftResult : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        Person _targetPerson = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string personKey = PageParameter( "Person" );
            if ( !string.IsNullOrEmpty( personKey ) )
            {
                try
                {
                    _targetPerson = new PersonService( new RockContext() ).GetByUrlEncodedKey( personKey );
                }
                catch ( Exception )
                {
                    nbError.Visible = true;
                }
            }
            else
            {
                // otherwise use the currently logged in person
                if ( CurrentPerson != null )
                {
                    _targetPerson = CurrentPerson;
                }
                else
                {
                    nbError.Visible = true;
                }
            }

            if ( _targetPerson != null )
            {
                pnlResults.Visible = true;
                ShowResults( _targetPerson );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( GetAttributeValue( "ShowRetakeTestButton" ).AsBoolean() )
            {
                if ( _targetPerson != null )
                {
                    SpiritualGiftService.SpiritualGiftTestResults savedScores = SpiritualGiftService.LoadSavedTestResults( _targetPerson );

                    if ( savedScores.LastSaveDate.Date <= DateTime.Now.AddDays( -GetAttributeValue( "MinimumDaysToRetake" ).AsInteger() ).Date )
                    {
                        btnRetakeTest.Visible = true;
                    }
                }
            }
        }

        #endregion

        #region Events

        protected void btnRetakeTest_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "SpiritualGiftTestPage" );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the results of the assessment test.
        /// </summary>
        /// <param name="savedScores">The saved scores.</param>
        private void ShowResults( Person person )
        {
            SpiritualGiftService.SpiritualGiftTestResults savedScores = SpiritualGiftService.LoadSavedTestResults( person );

            // Plot the Natural graph
            SpiritualGiftService.PlotOneGraph( giftScore_Prophecy, giftScore_Ministry, giftScore_Teaching, giftScore_Encouragement, giftScore_Giving, giftScore_Leadership, giftScore_Mercy,
                savedScores.Prophecy, savedScores.Ministry, savedScores.Teaching, savedScores.Encouragement, savedScores.Giving, savedScores.Leadership, savedScores.Mercy, 36 );
            ShowExplaination( savedScores.Gifting );

            hlTestDate.Text = String.Format( "Test Date: {0}", savedScores.LastSaveDate.ToShortDateString() );
            lPersonName.Text = person.FullName;

            lHeading.Text = string.Format( "<div class='disc-heading'><h1>{0}</h1><h4>Spiritual Gifting: {1}</h4></div>", person.FullName, savedScores.Gifting );
        }

        private void ShowExplaination( string gifting )
        {
            var giftingValue = DefinedTypeCache.Read( com.centralaz.SpiritualGifts.SystemGuid.DefinedType.SPRITUAL_GIFTS_DEFINED_TYPE.AsGuid() ).DefinedValues.Where( v => v.Value == gifting ).FirstOrDefault();
            if ( giftingValue != null )
            {
                lDescription.Text = giftingValue.Description;
            }
        }

        #endregion
    }
}