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
    /// Calculates a person's DISC score based on a series of question answers.
    /// This is used with permission from Greg Wiens: http://www.gregwiens.com/scid/
    /// </summary>
    [DisplayName( "Disc" )]
    [Category( "CRM > DiscAssessment" )]
    [Description( "Allows you to take a DISC test and saves your DISC score." )]
    [IntegerField( "Min Days To Retake", "The number of days that must pass before the test can be taken again.", false, 30 )]
    [CodeEditorField( "Instructions", "The text (HTML) to display at the top of the instructions section.  <span class='tip tip-liquid'></span> <span class='tip tip-html'></span>", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, true, @"
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
    public partial class Disc : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

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

            //Add reference to my JS file
            RockPage.AddScriptLink( "~/Blocks/Crm/DiscAssessment/scripts/disc.js" );

            DiscService.AssessmentResults savedScores = DiscService.LoadSavedAssessmentResults( CurrentPerson );

            if ( savedScores.LastSaveDate <= DateTime.MinValue )
            {
                ShowInstructions();
            }
            else
            {
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
                int moreN = 0;
                int moreD = 0;
                int moreI = 0;
                int moreS = 0;
                int moreC = 0;
                int lessN = 0;
                int lessD = 0;
                int lessI = 0;
                int lessS = 0;
                int lessC = 0;

                foreach ( RepeaterItem rItem in rQuestions.Items )
                {
                    RockRadioButtonList blMore = rItem.FindControl( "rblMore" ) as RockRadioButtonList;
                    RockRadioButtonList blLess = rItem.FindControl( "rblLess" ) as RockRadioButtonList;

                    switch ( blMore.SelectedValue )
                    {
                        case "N":
                            moreN++;
                            break;
                        case "D":
                            moreD++;
                            break;
                        case "I":
                            moreI++;
                            break;
                        case "S":
                            moreS++;
                            break;
                        case "C":
                            moreC++;
                            break;
                        default:
                            break;
                    }

                    switch ( blLess.SelectedValue )
                    {
                        case "N":
                            lessN++;
                            break;
                        case "D":
                            lessD++;
                            break;
                        case "I":
                            lessI++;
                            break;
                        case "S":
                            lessS++;
                            break;
                        case "C":
                            lessC++;
                            break;
                        default:
                            break;
                    }
                }

                // Score the responses and return the results
                DiscService.AssessmentResults results = DiscService.Score( moreN, moreD, moreI, moreS, moreC, lessN, lessD, lessI, lessS, lessC );

                //Display results out to user
                lblABd.Text = results.AdaptiveBehaviorD.ToString();
                lblABi.Text = results.AdaptiveBehaviorI.ToString();
                lblABs.Text = results.AdaptiveBehaviorS.ToString();
                lblABc.Text = results.AdaptiveBehaviorC.ToString();

                lblNBd.Text = results.NaturalBehaviorD.ToString();
                lblNBi.Text = results.NaturalBehaviorI.ToString();
                lblNBs.Text = results.NaturalBehaviorS.ToString();
                lblNBc.Text = results.NaturalBehaviorC.ToString();

                // Now save the results for this person
                DiscService.SaveAssessmentResults(
                    CurrentPerson,
                    lblABd.Text,
                    lblABi.Text,
                    lblABs.Text,
                    lblABc.Text,
                    lblNBd.Text,
                    lblNBi.Text,
                    lblNBs.Text,
                    lblNBc.Text
                );

                // Plot graph
                PlotGraph( results );

                pnlQuestions.Visible = false;
                pnlResults.Visible = true;
            }
            catch ( Exception ex )
            {

            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rQuestions repeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rQuestions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                RockRadioButtonList blMore = e.Item.FindControl( "rblMore" ) as RockRadioButtonList;

                ListItem m1 = new ListItem();
                ListItem m2 = new ListItem();
                ListItem m3 = new ListItem();
                ListItem m4 = new ListItem();

                m1.Text = "&nbsp;";
                m1.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[4].ToString().Substring( 0, 1 );
                m2.Text = "&nbsp;";
                m2.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[4].ToString().Substring( 1, 1 );
                m3.Text = "&nbsp;";
                m3.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[4].ToString().Substring( 2, 1 );
                m4.Text = "&nbsp;";
                m4.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[4].ToString().Substring( 3, 1 );

                blMore.Items.Add( m1 );
                blMore.Items.Add( m2 );
                blMore.Items.Add( m3 );
                blMore.Items.Add( m4 );

                RockRadioButtonList blLess = e.Item.FindControl( "rblLess" ) as RockRadioButtonList;

                ListItem l1 = new ListItem();
                ListItem l2 = new ListItem();
                ListItem l3 = new ListItem();
                ListItem l4 = new ListItem();

                l1.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[5].ToString().Substring( 0, 1 );
                l1.Text = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[0].ToString();
                l2.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[5].ToString().Substring( 1, 1 );
                l2.Text = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[1].ToString();
                l3.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[5].ToString().Substring( 2, 1 );
                l3.Text = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[2].ToString();
                l4.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[5].ToString().Substring( 3, 1 );
                l4.Text = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[3].ToString();

                blLess.Items.Add( l1 );
                blLess.Items.Add( l2 );
                blLess.Items.Add( l3 );
                blLess.Items.Add( l4 );
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
        private void PlotGraph( DiscService.AssessmentResults results )
        {
            // Plot the Natural graph
            PlotOneGraph( discNaturalScore_D, discNaturalScore_I, discNaturalScore_S, discNaturalScore_C,
                results.NaturalBehaviorD, results.NaturalBehaviorI, results.NaturalBehaviorS, results.NaturalBehaviorC );

            // Plot the Adaptive graph
            PlotOneGraph( discAdaptiveScore_D, discAdaptiveScore_I, discAdaptiveScore_S, discAdaptiveScore_C,
                results.AdaptiveBehaviorD, results.AdaptiveBehaviorI, results.AdaptiveBehaviorS, results.AdaptiveBehaviorC );
        }

        /// <summary>
        /// Plots the one DISC graph.
        /// </summary>
        /// <param name="barD">The D bar.</param>
        /// <param name="barI">The I bar.</param>
        /// <param name="barS">The S bar.</param>
        /// <param name="barC">The C bar.</param>
        /// <param name="scoreD">The D score.</param>
        /// <param name="scoreI">The I score.</param>
        /// <param name="scoreS">The S score.</param>
        /// <param name="scoreC">The C score.</param>
        private void PlotOneGraph( HtmlGenericControl barD, HtmlGenericControl barI, HtmlGenericControl barS, HtmlGenericControl barC,
            int scoreD, int scoreI, int scoreS, int scoreC )
        {
            barD.RemoveCssClass( "discbar-primary" );
            barI.RemoveCssClass( "discbar-primary" );
            barS.RemoveCssClass( "discbar-primary" );
            barC.RemoveCssClass( "discbar-primary" );

            // find the max value
            var maxScore = barD;
            var maxValue = scoreD;
            if ( scoreI > maxValue )
            {
                maxScore = barI;
                maxValue = scoreI;
            }
            if ( scoreS > maxValue )
            {
                maxScore = barS;
                maxValue = scoreS;
            }
            if ( scoreC > maxValue )
            {
                maxScore = barC;
                maxValue = scoreC;
            }
            maxScore.AddCssClass( "discbar-primary" );

            barD.Style.Add( "height", scoreD.ToString() + "%" );
            barI.Style.Add( "height", scoreI.ToString() + "%" );
            barS.Style.Add( "height", scoreS.ToString() + "%" );
            barC.Style.Add( "height", scoreC.ToString() + "%" );
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
            var mergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            if ( CurrentPerson != null )
            {
                mergeFields.Add( "Person", CurrentPerson );
            }

            Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
                .ToList()
                .ForEach( v => mergeFields.Add( v.Key, v.Value ) );

            lInstructions.Text = GetAttributeValue( "Instructions" ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the results of the assessment test.
        /// </summary>
        /// <param name="savedScores">The saved scores.</param>
        private void ShowResults( DiscService.AssessmentResults savedScores )
        {
            pnlInstructions.Visible = false;
            pnlQuestions.Visible = false;
            pnlResults.Visible = true;

            // Show re-take test button if MinDaysToRetake has passed...
            double days = GetAttributeValue( "MinDaysToRetake" ).AsDouble();
            if ( savedScores.LastSaveDate.AddDays( days ) <= RockDateTime.Now )
            {
                btnRetakeTest.Visible = true;
            }

            PlotGraph( savedScores );

            //build last results table
            lblLastAssessmentDate.Text = savedScores.LastSaveDate.ToString( "MM/dd/yyyy" );

            lblPrevABd.Text = savedScores.AdaptiveBehaviorD.ToString();
            lblPrevABi.Text = savedScores.AdaptiveBehaviorI.ToString();
            lblPrevABs.Text = savedScores.AdaptiveBehaviorS.ToString();
            lblPrevABc.Text = savedScores.AdaptiveBehaviorC.ToString();

            lblPrevNBd.Text = savedScores.NaturalBehaviorD.ToString();
            lblPrevNBi.Text = savedScores.NaturalBehaviorI.ToString();
            lblPrevNBs.Text = savedScores.NaturalBehaviorS.ToString();
            lblPrevNBc.Text = savedScores.NaturalBehaviorC.ToString();

            BindRepeater();
        }

        /// <summary>
        /// Binds the question data to the rQuestions repeater control.
        /// </summary>
        private void BindRepeater()
        {
            String[,] questionData = DiscService.GetResponsesByQuestion();
            var dataSet = new DataSet();
            var dataTable = dataSet.Tables.Add();
            var iRow = questionData.GetLongLength( 0 );
            var iCol = questionData.GetLongLength( 1 );

            dataTable.Columns.Add( "r1" ); //Response 1
            dataTable.Columns.Add( "r2" ); //Response 2
            dataTable.Columns.Add( "r3" ); //Response 3
            dataTable.Columns.Add( "r4" ); //Response 4
            dataTable.Columns.Add( "ms" ); //Most Scores
            dataTable.Columns.Add( "ls" ); //Least Scores

            //Row
            for ( var r = 0; r < iRow; r++ )
            {
                var row = dataTable.Rows.Add();
                //Column
                for ( var c = 0; c < iCol; c++ )
                {
                    row[c] = questionData[r, c];
                }
            }

            rQuestions.DataSource = dataSet.Tables[0];
            rQuestions.DataBind();
        }

        #endregion
    }
}