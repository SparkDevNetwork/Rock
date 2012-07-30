﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.CRM;

namespace Rockweb.Blocks.Crm
{
	public partial class Disc : Rock.Web.UI.Block
	{
		/// <summary>
		/// Creates a RadioButton for display on the test form.
		/// </summary>
		/// <param name="questionNumber">The question number that this response is associated with. Valied values = 01-30.</param>
		/// <param name="responseNumber">The response number of this particular response (zero-based). Valid values = 0-3.</param>
		/// <param name="MorL">Is this the "m"ost RadioButton, or the "l"east RadioButton?</param>
		/// <returns></returns>
		private RadioButton createRadioButton( string questionNumber, string responseNumber, string MorL )
		{
			string rbID = questionNumber + responseNumber + MorL.ToLower();
			string rbGroupName = "q" + questionNumber + MorL.ToLower();
			RadioButton radioButton = new RadioButton();
			radioButton.ID = rbID;
			radioButton.Attributes.Add( "onClick", "moveOn('" + questionNumber + "');" );
			radioButton.GroupName = rbGroupName;

			return radioButton;
		}

		/// <summary>
		/// Recursively finds a particular RadioButton in the passed-in container.
		/// </summary>
		/// <param name="container">The container to begin the search in.</param>
		/// <param name="name">The name of the RadioButton to find.</param>
		/// <returns>The found RadioButton or null.</returns>
		private RadioButton findRadioButton( Control container, string name )
		{
			if ( container.ID == name )
				return container as RadioButton;

			foreach ( Control control in container.Controls )
			{
				Control foundControl = findRadioButton( control, name );
				if ( foundControl != null )
					return foundControl as RadioButton;
			}

			return null;
		}


		/// <summary>
		/// Inserts a TableRow into the Question Table.
		/// </summary>
		/// <param name="response">The question response to add.</param>
		private void buildRadioButtonTableRow( DiscService.ResponseItem response )
		{
			TableRow tr = new TableRow();
			TableCell tc = new TableCell();
			tc.Text = response.ResponseText;
			tr.Cells.Add( tc );
			tc = new TableCell();
			RadioButton rb = createRadioButton( response.QuestionNumber, response.ResponseNumber, "m" );
			tc.Controls.Add( rb );
			tr.Cells.Add( tc );
			tc = new TableCell();
			rb = createRadioButton( response.QuestionNumber, response.ResponseNumber, "l" );
			tc.Controls.Add( rb );
			tr.Cells.Add( tc );
			tblQuestions.Rows.Add( tr );
		}

		/// <summary>
		/// Builds a table with the questions listed.
		/// </summary>
		private void buildQuestionTable()
		{
			List<DiscService.ResponseItem> responses = DiscService.GetResponses();

			TableRow tr = new TableRow();
			TableCell tc = new TableCell();

			foreach ( DiscService.ResponseItem response in responses )
			{
				// If we are processing the first response in each question, build a question header
				if ( response.ResponseNumber == "1" )
				{
					tr = new TableRow();
					tc = new TableCell();
					tc.ColumnSpan = 3;
					tc.Text = "Question " + response.QuestionNumber;
					tc.BackColor = System.Drawing.Color.LightGray;
					tr.Cells.Add( tc );
					tc.ID = "q" + response.QuestionNumber;
					tblQuestions.Rows.Add( tr );

					tr = new TableRow();
					tc = new TableCell();
					tc.Text = "";
					tr.Cells.Add( tc );
					tc = new TableCell();
					tc.Text = "MOST";
					tr.Cells.Add( tc );
					tc = new TableCell();
					tc.Text = "LEAST";
					tr.Cells.Add( tc );
					tblQuestions.Rows.Add( tr );
				}

				buildRadioButtonTableRow( response );
			}
		}

		protected void Page_Load( object sender, EventArgs e )
		{
			//Checks if Page IsPostBack (making the assumption that the PostBack is because the
			//  'Score Test' button was clicked.
			//Tell Javascript that the page is posted back or not.
			// See:  http://stackoverflow.com/questions/59719/how-can-i-check-for-ispostback-in-javascript
			string script = IsPostBack ? "var isScored = true;" : "var isScored = false;";
			Page.ClientScript.RegisterStartupScript( GetType(), "IsScored", script, true );

			//Adding references to my CSS and JS files
			//The second 'AddCSSLink' line below bypasses a page load error on postback. 
			PageInstance.AddCSSLink( this.Page, "~/Blocks/Crm/DiscAssessment/CSS/disc.css" );
			PageInstance.AddCSSLink( this.Page, "~/Blocks/Crm/DiscAssessment/CSS/disc2.css" );
			PageInstance.AddScriptLink( this.Page, "~/Blocks/Crm/DiscAssessment/scripts/disc.js" );

			//Yup, build question table
			buildQuestionTable();
		}

		/// <summary>
		/// Gets checked RadioButtons, scores test, and displays results.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnScoreTest_Click( object sender, EventArgs e )
		{
			List<DiscService.ResponseItem> responses = DiscService.GetResponses();
			List<string> selectedResponseIDs = new List<string>();

			// Collect selected responses into a string array.
			// All we need is the selected RadioButton's ID (which we set to the ResponseID + ("l" or "m") for each responses record).
			//   Examples: "012m", "130l"
			// We now know the selected response and whether it was 'm'ost or 'l'east.
			foreach ( DiscService.ResponseItem response in responses )
			{
				string rbID = response.ResponseID + "m";
				RadioButton rb = findRadioButton( this, rbID );
				if ( rb.Checked )
					selectedResponseIDs.Add( rb.ID );

				rbID = response.ResponseID + "l";
				rb = findRadioButton( this, rbID );
				if ( rb.Checked )
					selectedResponseIDs.Add( rb.ID );
			}

			// Score the responses and return the results
			DiscService.AssessmentResults results = DiscService.Score( selectedResponseIDs );

			//Display results out to user
			lblABd.Text = "D: " + results.AdaptiveBehaviorD;
			lblABi.Text = "I: " + results.AdaptiveBehaviorI;
			lblABs.Text = "S: " + results.AdaptiveBehaviorS;
			lblABc.Text = "C: " + results.AdaptiveBehaviorC;

			lblNBd.Text = "D: " + results.NaturalBehaviorD;
			lblNBi.Text = "I: " + results.NaturalBehaviorI;
			lblNBs.Text = "S: " + results.NaturalBehaviorS;
			lblNBc.Text = "C: " + results.NaturalBehaviorC;
		}
	}
}