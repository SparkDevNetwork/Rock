using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using com.shepherdchurch.SurveySystem.Model;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_shepherdchurch.SurveySystem
{
    [DisplayName( "Survey Result Detail" )]
    [Category( "Shepherd Church > Survey System" )]
    [Description( "Displays the details for a survey result." )]

    public partial class SurveyResultDetail : RockBlock
    {
        #region Private Fields

        private bool _canDelete = false;

        #endregion

        #region Properties

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            //
            // Verify that the user is allowed to make edits to the topic/category in question.
            //
            _canDelete = IsUserAuthorized( Authorization.EDIT );
            if ( _canDelete )
            {
                var rockContext = new RockContext();
                var result = new SurveyResultService ( rockContext ).Get( PageParameter( "SurveyResultId" ).AsInteger() );

                if ( result != null )
                {
                    _canDelete = result.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                ShowDetail( PageParameter( "SurveyResultId" ).AsInteger() );
            }
            else
            {
                if ( pnlDetails.Visible )
                {
                    var result = new SurveyResultService( new RockContext() ).Get( PageParameter( "SurveyResultId" ).AsInteger() );

                    result.LoadAttributes();
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddDisplayControls( result, phAttributes, null, false, false );
                }
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="sureyResultId">The result identifier.</param>
        public void ShowDetail( int surveyResultId )
        {
            var rockContext = new RockContext();
            SurveyResult result = null;

            pnlDetails.Visible = true;

            if ( surveyResultId != 0 )
            {
                result = new SurveyResultService( rockContext ).Get( surveyResultId );
            }

            //
            // Ensure the user is allowed to view this result.
            //
            if ( result == null || !result.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbUnauthorized.Text = EditModeMessage.NotAuthorizedToView( SurveyResult.FriendlyTypeName );
                pnlDetails.Visible = false;
                return;
            }

            //
            // Set the panel title.
            //
            lTitle.Text = result.Survey.Name + " Result";

            //
            // Set all the simple field values.
            //
            lCreatedBy.Text = result.CreatedByPersonAlias != null
                ? string.Format( "<a href=\"/Person/{0}\">{1}</a>", result.CreatedByPersonAlias.PersonId, result.CreatedByPersonAlias.Person.FullName )
                : string.Empty;
            lCreatedDate.Text = result.CreatedDateTime.HasValue ? result.CreatedDateTime.Value.ToShortDateString() + " " + result.CreatedDateTime.Value.ToShortTimeString() : string.Empty;

            if ( result.DidPass.HasValue )
            {
                hlDidPass.Text = result.DidPass.Value ? "Passed" : "Failed";
                hlDidPass.LabelType = result.DidPass.Value ? LabelType.Success : LabelType.Danger;
            }

            if ( result.TestResult.HasValue )
            {
                hlTestResult.Text = string.Format( "{0:0.0}%", result.TestResult.Value );
            }

            //
            // Add the attribute controls.
            //
            result.LoadAttributes();
            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddDisplayControls( result, phAttributes, null, false, false );

            BindGrid();

            //
            // Set button states.
            //
            lbDelete.Visible = _canDelete;
        }

        /// <summary>
        /// Bind the grid to the result set we have.
        /// </summary>
        protected void BindGrid()
        {
            var rockContext = new RockContext();
            SurveyResult result = null;

            result = new SurveyResultService( rockContext ).Get( PageParameter( "SurveyResultId" ).AsInteger() );

            result.LoadAttributes( rockContext );

            var answers = result.Survey.Answers;

            var list = result.Attributes.Values
                .Select( a => new
                {
                    Key = a.Name,
                    Value = a.FieldType.Field.FormatValueAsHtml( gAttributes, result.GetAttributeValue( a.Key ), a.QualifierValues, false ),
                    Answer = answers.ContainsKey( a.Key ) ? a.FieldType.Field.FormatValueAsHtml( gAttributes, answers[a.Key], a.QualifierValues, false ) : string.Empty,
                    IsCorrect = ( answers.ContainsKey( a.Key ) ? answers[a.Key] == result.GetAttributeValue( a.Key ) : false ) ? "<i class='text-success fa fa-check'></i>" : "<i class='text-danger fa fa-times'></i>",
                    a.Order
                } )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Key )
                .ToList();

            gAttributes.Columns[2].Visible = gAttributes.Columns[3].Visible = result.TestResult.HasValue;

            gAttributes.DataSource = list;
            gAttributes.DataBind();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( "SurveyResultId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var surveyResultService = new SurveyResultService( rockContext );
            var result = surveyResultService.Get( PageParameter( "SurveyResultId" ).AsInteger() );

            if ( result != null )
            {
                int surveyId = result.SurveyId;

                surveyResultService.Delete( result );

                rockContext.SaveChanges();

                NavigateToParentPage( new Dictionary<string, string> { { "SurveyId", surveyId.ToString() } } );
            }
            else
            {
                ShowDetail( PageParameter( "SurveyResultId" ).AsInteger() );
            }
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDone_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var result = new SurveyResultService( rockContext ).Get( PageParameter( "SurveyResultId" ).AsInteger() );

            NavigateToParentPage( new Dictionary<string, string> { { "SurveyId", result.SurveyId.ToString() } } );
        }

        #endregion
    }
}