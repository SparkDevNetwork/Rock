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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
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
    public partial class Disc : Rock.Web.UI.RockBlock
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
                DiscService.AssessmentResults savedScores = DiscService.LoadSavedAssessmentResults( _targetPerson );

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
                    RockRadioButtonList rblMore1 = rItem.FindControl( "rblMore1" ) as RockRadioButtonList;
                    RockRadioButtonList rblMore2 = rItem.FindControl( "rblMore2" ) as RockRadioButtonList;
                    RockRadioButtonList rblMore3 = rItem.FindControl( "rblMore3" ) as RockRadioButtonList;
                    RockRadioButtonList rblMore4 = rItem.FindControl( "rblMore4" ) as RockRadioButtonList;

                    RockRadioButtonList rblLess1 = rItem.FindControl( "rblLess1" ) as RockRadioButtonList;
                    RockRadioButtonList rblLess2 = rItem.FindControl( "rblLess2" ) as RockRadioButtonList;
                    RockRadioButtonList rblLess3 = rItem.FindControl( "rblLess3" ) as RockRadioButtonList;
                    RockRadioButtonList rblLess4 = rItem.FindControl( "rblLess4" ) as RockRadioButtonList;

                    string selectedMoreValue = GetSelectedValue( rblMore1, rblMore2, rblMore3, rblMore4 );
                    string selectedLessValue = GetSelectedValue( rblLess1, rblLess2, rblLess3, rblLess4 );

                    switch ( selectedMoreValue )
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

                    switch ( selectedLessValue )
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

                // Now save the results for this person
                DiscService.SaveAssessmentResults(
                    _targetPerson,
                    results.AdaptiveBehaviorD.ToString(),
                    results.AdaptiveBehaviorI.ToString(),
                    results.AdaptiveBehaviorS.ToString(),
                    results.AdaptiveBehaviorC.ToString(),
                    results.NaturalBehaviorD.ToString(),
                    results.NaturalBehaviorI.ToString(),
                    results.NaturalBehaviorS.ToString(),
                    results.NaturalBehaviorC.ToString(),
                    results.PersonalityType
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
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                Literal lQuestion1 = e.Item.FindControl( "lQuestion1" ) as Literal;
                Literal lQuestion2 = e.Item.FindControl( "lQuestion2" ) as Literal;
                Literal lQuestion3 = e.Item.FindControl( "lQuestion3" ) as Literal;
                Literal lQuestion4 = e.Item.FindControl( "lQuestion4" ) as Literal;

                lQuestion1.Text = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[0].ToString();
                lQuestion2.Text = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[1].ToString();
                lQuestion3.Text = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[2].ToString();
                lQuestion4.Text = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[3].ToString();

                RockRadioButtonList rblMore1 = e.Item.FindControl( "rblMore1" ) as RockRadioButtonList;
                RockRadioButtonList rblMore2 = e.Item.FindControl( "rblMore2" ) as RockRadioButtonList;
                RockRadioButtonList rblMore3 = e.Item.FindControl( "rblMore3" ) as RockRadioButtonList;
                RockRadioButtonList rblMore4 = e.Item.FindControl( "rblMore4" ) as RockRadioButtonList;

                RockRadioButtonList rblLess1 = e.Item.FindControl( "rblLess1" ) as RockRadioButtonList;
                RockRadioButtonList rblLess2 = e.Item.FindControl( "rblLess2" ) as RockRadioButtonList;
                RockRadioButtonList rblLess3 = e.Item.FindControl( "rblLess3" ) as RockRadioButtonList;
                RockRadioButtonList rblLess4 = e.Item.FindControl( "rblLess4" ) as RockRadioButtonList;


                ListItem m1 = new ListItem();
                ListItem m2 = new ListItem();
                ListItem m3 = new ListItem();
                ListItem m4 = new ListItem();
                m1.Text = m2.Text = m3.Text = m4.Text = "&nbsp;";

                m1.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[4].ToString().Substring( 0, 1 );
                m2.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[4].ToString().Substring( 1, 1 );
                m3.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[4].ToString().Substring( 2, 1 );
                m4.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[4].ToString().Substring( 3, 1 );

                rblMore1.Items.Add( m1 );
                rblMore2.Items.Add( m2 );
                rblMore3.Items.Add( m3 );
                rblMore4.Items.Add( m4 );

                ListItem l1 = new ListItem();
                ListItem l2 = new ListItem();
                ListItem l3 = new ListItem();
                ListItem l4 = new ListItem();
                l1.Text = l2.Text = l3.Text = l4.Text = "&nbsp;";

                l1.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[5].ToString().Substring( 0, 1 );
                l2.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[5].ToString().Substring( 1, 1 );
                l3.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[5].ToString().Substring( 2, 1 );
                l4.Value = ( (System.Data.DataRowView)( e.Item.DataItem ) ).Row.ItemArray[5].ToString().Substring( 3, 1 );

                rblLess1.Items.Add( l1 );
                rblLess2.Items.Add( l2 );
                rblLess3.Items.Add( l3 );
                rblLess4.Items.Add( l4 );
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
        /// Gets the selected value from the given radiobuttonlists.
        /// </summary>
        /// <param name="rbl1">The first RadioButtonList.</param>
        /// <param name="rbl2">The second RadioButtonList.</param>
        /// <param name="rbl3">The third RadioButtonList.</param>
        /// <param name="rbl4">The fourth RadioButtonList.</param>
        /// <returns>the value from the first non-empty RadioButtonList</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">One of the RadioButtonList must be selected.</exception>
        private string GetSelectedValue( RadioButtonList rbl1,  RadioButtonList rbl2, RadioButtonList rbl3, RadioButtonList rbl4 )
        {
            if ( ! string.IsNullOrEmpty( rbl1.SelectedValue ) )
            {
                return rbl1.SelectedValue;
            }
            else if (! string.IsNullOrEmpty( rbl2.SelectedValue ))
            {
                return rbl2.SelectedValue;
            }
            else if ( !string.IsNullOrEmpty( rbl3.SelectedValue ) )
            {
                return rbl3.SelectedValue;
            }
            else if ( !string.IsNullOrEmpty( rbl4.SelectedValue ) )
            {
                return rbl4.SelectedValue;
            }
            else
            {
                throw new ArgumentOutOfRangeException( "One of the RadioButtonList must be selected." );
            }
        }

        /// <summary>
        /// Plots the graphs using the Disc score results.
        /// </summary>
        /// <param name="results">The results.</param>
        private void PlotGraph( DiscService.AssessmentResults results )
        {
            // Plot the Natural graph
            DiscService.PlotOneGraph( discNaturalScore_D, discNaturalScore_I, discNaturalScore_S, discNaturalScore_C,
                results.NaturalBehaviorD, results.NaturalBehaviorI, results.NaturalBehaviorS, results.NaturalBehaviorC, 35 );
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

            if ( CurrentPersonId == _targetPerson.Id )
            {
                lPrintTip.Visible = true;
            }
            lHeading.Text = string.Format( "<div class='disc-heading'><h1>{0}</h1><h4>Personality Type: {1}</h4></div>", _targetPerson.FullName, savedScores.PersonalityType );

            // Show re-take test button if MinDaysToRetake has passed...
            double days = GetAttributeValue( "MinDaysToRetake" ).AsDouble();
            if ( savedScores.LastSaveDate.AddDays( days ) <= RockDateTime.Now )
            {
                btnRetakeTest.Visible = true;
            }

            PlotGraph( savedScores );

            ShowExplaination( savedScores.PersonalityType );
        }

        /// <summary>
        /// Shows the explaination for the given personality type as defined in one of the
        /// DefinedValues of the DISC Results DefinedType.
        /// </summary>
        /// <param name="personalityType">The one or two letter personality type.</param>
        private void ShowExplaination( string personalityType )
        {
            var personalityValue = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.DISC_RESULTS_TYPE.AsGuid() ).DefinedValues.Where( v => v.Value == personalityType ).FirstOrDefault();
            if ( personalityValue != null )
            {
                lDescription.Text = personalityValue.Description;
                lStrengths.Text = personalityValue.GetAttributeValue( "Strengths" );
                lChallenges.Text = personalityValue.GetAttributeValue( "Challenges" );
            }
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