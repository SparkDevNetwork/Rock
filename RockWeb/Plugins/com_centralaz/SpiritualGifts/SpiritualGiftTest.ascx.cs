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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.Web.UI.HtmlControls;

namespace Rockweb.Plugins.com_centralaz.SpiritualGifts
{
    /// <summary>
    /// Calculates a person's DISC score based on a series of question answers.
    /// This is used with permission from Greg Wiens: http://www.gregwiens.com/scid/
    /// </summary>
    [DisplayName( "Spiritual Gifts Test" )]
    [Category( "com_centralaz > Spiritual Gifts" )]
    [Description( "Allows you to take a spiritual gift test and saves your spiritual gift score." )]
    [IntegerField( "Min Days To Retake", "The number of days that must pass before the test can be taken again.", false, 30 )]
    [CodeEditorField( "Instructions", "The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, true, @"
            <h2>Welcome!</h2>
            <p>
                {{ Person.NickName }}, in this assessment you are given a series of questions, each containing four phrases.
                Select one phrase that MOST describes you and one phrase that LEAST describes you.
            </p>
            <p>
                This assessment is environmentally sensitive, which means that you may score differently
                in different situations. In other words, you may act differently at home than you
                do on the job. So, as you complete the assessment you should focus on one environment
                for which you are seeking to understand yourself. For instance, if you are trying
                to understand yourself in marriage, you should only think of your responses to situations
                in the context of your marriage. On the other hand, if you want to know your behavioral
                needs on the job, then only think of how you would respond in the job context.
            </p>
            <p>
                One final thought as you give your responses. On these kinds of assessments, it
                is often best and easiest if you respond quickly and do not deliberate too long
                on each question. Your response on one question will not unduly influence your scores,
                so simply answer as quickly as possible and enjoy the process. Don't get too hung
                up, if none of the phrases describe you or if there are some phrases that seem too
                similar, just go with your instinct.
            </p>
            <p>
                When you are ready, click the 'Start' button to proceed.
            </p>
" )]
    public partial class SpiritualGiftTest : Rock.Web.UI.RockBlock
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

            string personKey = PageParameter( "rckipid" );
            if ( !string.IsNullOrEmpty( personKey ) )
            {
                try
                {
                    _targetPerson = new PersonService( new RockContext() ).GetByUrlEncodedKey( personKey );
                }
                catch
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
                SpiritualGiftService.TestResults savedScores = SpiritualGiftService.LoadSavedTestResults( _targetPerson );

                if ( savedScores.LastSaveDate <= DateTime.MinValue )
                {
                    ShowInstructions();
                }
                else
                {
                    ShowResults( savedScores );
                }
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
        /// <summary>
        /// Handles the Click event of the btnStart button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnStart_Click( object sender, EventArgs e )
        {
            pnlInstructions.Visible = false;
            pnlQuestions.Visible = true;
            BindRepeater();
        }

        /// <summary>
        /// Scores test, and displays results.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnScoreTest_Click( object sender, EventArgs e )
        {
            try
            {
                int prophecy = 0;
                int ministry = 0;
                int teaching = 0;
                int encouragement = 0;
                int giving = 0;
                int leadership = 0;
                int mercy = 0;
                int placebo = 0;

                foreach ( RepeaterItem rItem in rQuestions.Items )
                {
                    RockRadioButtonList rblAnswer = rItem.FindControl( "rblAnswer" ) as RockRadioButtonList;
                    HiddenField hfGiftType = rItem.FindControl( "hfGiftType" ) as HiddenField;

                    int? selectedValue = rblAnswer.SelectedValueAsInt();
                    if ( !selectedValue.HasValue )
                    {
                        selectedValue = 0;
                    }

                    switch ( hfGiftType.Value )
                    {
                        case "P":
                            prophecy += selectedValue.Value;
                            break;
                        case "S":
                            ministry += selectedValue.Value;
                            break;
                        case "T":
                            teaching += selectedValue.Value;
                            break;
                        case "E":
                            encouragement += selectedValue.Value;
                            break;
                        case "G":
                            giving += selectedValue.Value;
                            break;
                        case "L":
                            leadership += selectedValue.Value;
                            break;
                        case "M":
                            mercy += selectedValue.Value;
                            break;
                        case "N":
                            placebo += selectedValue.Value;
                            break;
                        default:
                            break;
                    }
                }

                // Score the responses and return the results
                SpiritualGiftService.TestResults results = SpiritualGiftService.Score( prophecy, ministry, teaching, encouragement, giving, leadership, mercy, placebo );

                // Now save the results for this person
                SpiritualGiftService.SaveTestResults(
                    _targetPerson,
                    results.Prophecy.ToString(),
                    results.Ministry.ToString(),
                    results.Teaching.ToString(),
                    results.Encouragement.ToString(),
                    results.Giving.ToString(),
                    results.Leadership.ToString(),
                    results.Mercy.ToString(),
                    results.Gifting
                );

                // Show the results
                ShowResults( results );
            }
            catch ( Exception ex )
            {
                nbError.Visible = true;
                nbError.Title = "We're Sorry...";
                nbError.Text = "Something went wrong while trying to save your test results.";
                LogException( ex );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rQuestions repeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rQuestions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var responseItem = e.Item.DataItem as SpiritualGiftService.ResponseItem?;
            if ( responseItem != null )
            {
                Literal lQuestion = e.Item.FindControl( "lQuestion" ) as Literal;
                HiddenField hfGiftType = e.Item.FindControl( "hfGiftType" ) as HiddenField;
                lQuestion.Text = responseItem.Value.QuestionText;
                hfGiftType.Value = responseItem.Value.GiftScore;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRetake control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRetakeTest_Click( object sender, EventArgs e )
        {
            btnRetakeTest.Visible = false;
            ShowInstructions();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Plots the graphs using the Disc score results.
        /// </summary>
        /// <param name="results">The results.</param>
        private void PlotGraph( SpiritualGiftService.TestResults results )
        {
            // Plot the Natural graph
            SpiritualGiftService.PlotOneGraph( giftScore_Prophecy, giftScore_Ministry, giftScore_Teaching, giftScore_Encouragement, giftScore_Giving, giftScore_Leadership, giftScore_Mercy,
                results.Prophecy, results.Ministry, results.Teaching, results.Encouragement, results.Giving, results.Leadership, results.Mercy, 20 );
        }

        /// <summary>
        /// Shows the instructions.
        /// </summary>
        private void ShowInstructions()
        {
            pnlInstructions.Visible = true;
            pnlQuestions.Visible = false;
            pnlResults.Visible = false;

            // Resolve the text field merge fields
            var mergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( _targetPerson );
            if ( _targetPerson != null )
            {
                mergeFields.Add( "Person", _targetPerson );
            }
            //TODO
            //Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
            //    .ToList()
            //    .ForEach( v => mergeFields.Add( v.Key, v.Value ) );

            lInstructions.Text = GetAttributeValue( "Instructions" ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the results of the assessment test.
        /// </summary>
        /// <param name="savedScores">The saved scores.</param>
        private void ShowResults( SpiritualGiftService.TestResults savedScores )
        {
            pnlInstructions.Visible = false;
            pnlQuestions.Visible = false;
            pnlResults.Visible = true;

            if ( CurrentPersonId == _targetPerson.Id )
            {
                lPrintTip.Visible = true;
            }
            lHeading.Text = string.Format( "<div class='disc-heading'><h1>{0}</h1><h4>Spiritual Gift: {1}</h4></div>", _targetPerson.FullName, savedScores.Gifting );

            // Show re-take test button if MinDaysToRetake has passed...
            double days = GetAttributeValue( "MinDaysToRetake" ).AsDouble();
            if ( savedScores.LastSaveDate.AddDays( days ) <= RockDateTime.Now )
            {
                btnRetakeTest.Visible = true;
            }

            PlotGraph( savedScores );

            ShowExplaination( savedScores.Gifting );
        }

        /// <summary>
        /// Shows the explaination for the given gifting as defined in one of the
        /// DefinedValues of the Spiritual Gifts Results DefinedType.
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

        /// <summary>
        /// Binds the question data to the rQuestions repeater control.
        /// </summary>
        private void BindRepeater()
        {
            List<SpiritualGiftService.ResponseItem> responseItemList = SpiritualGiftService.GetResponses();
            rQuestions.DataSource = responseItemList;
            rQuestions.DataBind();
        }

        #endregion
    }
}