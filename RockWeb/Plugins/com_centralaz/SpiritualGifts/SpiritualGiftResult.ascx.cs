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
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.centralaz.SpiritualGifts.Data;
using com.centralaz.SpiritualGifts.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Web.UI.HtmlControls;

namespace Rockweb.Plugins.com_centralaz.SpiritualGifts
{
    /// <summary>
    /// View a person's spiritual gifts.
    /// This is used with permission from Greg Wiens: http://www.gregwiens.com/scid/
    /// </summary>
    [DisplayName( "Spiritual Gifts Result" )]
    [Category( "com_centralaz > Spiritual Gifts" )]
    [Description( "Allows you to view your spiritual gift score." )]
    public partial class SpiritualGiftResult : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        Person _targetPerson = null;

        #endregion

        #region Properties

        // used for public / protected properties

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
                SpiritualGiftService.TestResults savedScores = SpiritualGiftService.LoadSavedTestResults( _targetPerson );
                ShowResults( savedScores );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
        }

        #endregion

        #region Events

        #endregion

        #region Methods

        /// <summary>
        /// Shows the results of the assessment test.
        /// </summary>
        /// <param name="savedScores">The saved scores.</param>
        private void ShowResults( SpiritualGiftService.TestResults savedScores )
        {
            // Plot the Natural graph
            SpiritualGiftService.PlotOneGraph( giftScore_Prophecy, giftScore_Ministry, giftScore_Teaching, giftScore_Encouragement, giftScore_Giving, giftScore_Leadership, giftScore_Mercy,
                savedScores.Prophecy, savedScores.Ministry, savedScores.Teaching, savedScores.Encouragement, savedScores.Giving, savedScores.Leadership, savedScores.Mercy, 20 );
            ShowExplaination( savedScores.Gifting );

            hlTestDate.Text = String.Format( "Test Date: {0}", savedScores.LastSaveDate.ToShortDateString() );
            lPersonName.Text = _targetPerson.FullName;

            lHeading.Text = string.Format( "<div class='disc-heading'><h1>{0}</h1><h4>Spiritual Gifting: {1}</h4></div>", _targetPerson.FullName, savedScores.Gifting );
        }

        /// <summary>
        /// Shows the explaination for the given personality type as defined in one of the
        /// DefinedValues of the DISC Results DefinedType.
        /// </summary>
        /// <param name="gifting">The gifting.</param>
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