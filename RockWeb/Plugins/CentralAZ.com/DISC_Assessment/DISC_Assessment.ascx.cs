using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Custom.CentralAZ.DISC_Assessment;

public partial class Plugins_CentralAZcom_DISC_Assessment_DISC_Assessment : Rock.Web.UI.Block
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
	/// Inserts a TabelRow into the Question Table.
	/// </summary>
	/// <param name="response">The question response to add.</param>
	private void buildRadioButtonTableRow( DISC.ResponseItem response )
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
		List<DISC.ResponseItem> responses = DISC.GetResponses();

		TableRow tr = new TableRow();
		TableCell tc = new TableCell();

		foreach ( DISC.ResponseItem response in responses )
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
		PageInstance.AddScriptLink( this.Page, "/RockWeb/Plugins/CentralAZ.com/DISC_Assessment/scripts/disc.js" );
		PageInstance.AddCSSLink( this.Page, "/RockWeb/Plugins/CentralAZ.com/DISC_Assessment/CSS/disc.css" );
		buildQuestionTable();
	}

	/// <summary>
	/// Gets checked RadioButtons, scores test, and displays results.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void btnScoreTest_Click( object sender, EventArgs e )
	{
		List<DISC.ResponseItem> responses = DISC.GetResponses();
		List<string> selectedResponseIDs = new List<string>();

		// Collect selected responses into a string array.
		// All we need is the selected RadioButton's ID (which we set to the ResponseID + ("l" or "m") for each responses record).
		//   Examples: "012m", "130l"
		// We now know the selected response and whether it was 'm'ost or 'l'east.
		foreach ( DISC.ResponseItem response in responses )
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
		// 
		DISC.AssessmentResults Results = DISC.Score( selectedResponseIDs );

		//Display results out to user
		lblABs.Text = "S: " + Results.AdaptiveBehaviorS;
		lblABc.Text = "C: " + Results.AdaptiveBehaviorC;
		lblABi.Text = "I: " + Results.AdaptiveBehaviorI;
		lblABd.Text = "D: " + Results.AdaptiveBehaviorD;

		lblNBs.Text = "S: " + Results.NaturalBehaviorS;
		lblNBc.Text = "C: " + Results.NaturalBehaviorC;
		lblNBi.Text = "I: " + Results.NaturalBehaviorI;
		lblNBd.Text = "D: " + Results.NaturalBehaviorD;
	}
}
