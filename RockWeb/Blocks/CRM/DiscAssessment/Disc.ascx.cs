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
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rockweb.Blocks.Crm
{
    [DisplayName( "Disc" )]
    [Category( "CRM > DiscAssessment" )]
    [Description( "Allows you to take a DISC test and saves your DISC score." )]
    public partial class Disc : Rock.Web.UI.RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            DiscService.AssessmentResults savedScores = DiscService.LoadSavedAssessmentResults( CurrentPerson );

            if ( savedScores.LastSaveDate > DateTime.MinValue )
            {
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
            }

            BindRepeater();
        }

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

        protected void Page_Load( object sender, EventArgs e )
        {
            // 20140926 - Don't think my .js is doing anything any longer
            //
            //Checks if RockPage IsPostBack (making the assumption that the PostBack is because the
            //  'Score Test' button was clicked.
            //Tell Javascript that the page is posted back or not.
            // See:  http://stackoverflow.com/questions/59719/how-can-i-check-for-ispostback-in-javascript
            string script = IsPostBack ? "var isScored = true;" : "var isScored = false;";
            Page.ClientScript.RegisterStartupScript( GetType(), "IsScored", script, true );

            //Add reference to my JS file
            RockPage.AddScriptLink( "~/Blocks/Crm/DiscAssessment/scripts/disc.js" );

            if ( IsPostBack )
            {
                btnSaveResults.Enabled = true;
            }
            else
            {
                btnSaveResults.Enabled = false;
            }
        }

        /// <summary>
        /// Scores test, and displays results.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnScoreTest_Click( object sender, EventArgs e )
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
        }

        protected void btnSaveResults_Click( object sender, EventArgs e )
        {
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
        }

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
    }
}