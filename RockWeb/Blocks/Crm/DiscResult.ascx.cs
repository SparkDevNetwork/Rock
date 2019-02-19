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
using System.Data;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Web.UI.HtmlControls;

namespace Rockweb.Blocks.Crm
{
    /// <summary>
    /// View a person's DISC assessment.
    /// This is used with permission from Greg Wiens: http://www.gregwiens.com/scid/
    /// </summary>
    [DisplayName( "DISC Result" )]
    [Category( "CRM" )]
    [Description( "View the results of a DISC assessment." )]
    public partial class DiscResult : Rock.Web.UI.RockBlock
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
                DiscService.AssessmentResults savedScores = DiscService.LoadSavedAssessmentResults( _targetPerson );
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
        private void ShowResults( DiscService.AssessmentResults savedScores )
        {
            // Plot the Natural graph
            DiscService.PlotOneGraph( discNaturalScore_D, discNaturalScore_I, discNaturalScore_S, discNaturalScore_C,
                savedScores.NaturalBehaviorD, savedScores.NaturalBehaviorI, savedScores.NaturalBehaviorS, savedScores.NaturalBehaviorC, 100 );
            ShowExplaination( savedScores.PersonalityType );

            hlAssessmentDate.Text = String.Format( "Assessment Date: {0}", savedScores.LastSaveDate.ToShortDateString() );
            lPersonName.Text = _targetPerson.FullName;

            lHeading.Text = string.Format( "<div class='disc-heading'><h1>{0}</h1><h4>Personality Type: {1}</h4></div>", _targetPerson.FullName, savedScores.PersonalityType );
        }

        /// <summary>
        /// Shows the explaination for the given personality type as defined in one of the
        /// DefinedValues of the DISC Results DefinedType.
        /// </summary>
        /// <param name="personalityType">The one or two letter personality type.</param>
        private void ShowExplaination( string personalityType )
        {
            var personalityValue = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.DISC_RESULTS_TYPE.AsGuid() ).DefinedValues.Where( v => v.Value == personalityType ).FirstOrDefault();
            if ( personalityValue != null )
            {
                lDescription.Text = personalityValue.Description;
                lStrengths.Text = personalityValue.GetAttributeValue( "Strengths" );
                lChallenges.Text = personalityValue.GetAttributeValue( "Challenges" );
                lUnderPressure.Text = personalityValue.GetAttributeValue( "UnderPressure" );
                lMotivation.Text = personalityValue.GetAttributeValue( "Motivation" );
                lTeamContribution.Text = personalityValue.GetAttributeValue( "TeamContribution" );
                lLeadershipStyle.Text = personalityValue.GetAttributeValue( "LeadershipStyle" );
                lFollowerStyle.Text = personalityValue.GetAttributeValue( "FollowerStyle" );
            }
        }

        #endregion
    }
}