using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using com.shepherdchurch.SurveySystem.Model;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_shepherdchurch.SurveySystem
{
    [DisplayName( "Survey Detail" )]
    [Category( "Shepherd Church > Survey System" )]
    [Description( "Displays the details for a survey." )]

    [LinkedPage( "Results Page", "The page that is used to list results for this survey.", false, order: 0 )]
    [BooleanField( "Record Answers Default", "The default value for Record Answers option when a user creates a new survey.", false, order: 1 )]
    public partial class SurveyDetail : RockBlock
    {
        #region Private Fields

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the state of the survey attributes.
        /// </summary>
        private List<Rock.Model.Attribute> SurveyAttributesState { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["SurveyAttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                SurveyAttributesState = new List<Rock.Model.Attribute>();
            }
            else
            {
                SurveyAttributesState = JsonConvert.DeserializeObject<List<Rock.Model.Attribute>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gSurveyAttributes.DataKeyNames = new string[] { "Guid" };
            gSurveyAttributes.Actions.ShowAdd = true;
            gSurveyAttributes.Actions.AddClick += gSurveyAttributes_Add;
            gSurveyAttributes.GridRebind += gSurveyAttributes_GridRebind;
            gSurveyAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gSurveyAttributes.GridReorder += gSurveyAttributes_GridReorder;

            if ( !IsPostBack )
            {
                lbResults.Visible = !string.IsNullOrWhiteSpace( GetAttributeValue( "ResultsPage" ) );

                ddlLastAttemptDateAttribute.Items.Clear();
                ddlLastPassedDateAttribute.Items.Clear();

                ddlLastAttemptDateAttribute.Items.Add( new ListItem() );
                ddlLastPassedDateAttribute.Items.Add( new ListItem() );

                var attributes = new AttributeService( new RockContext() )
                    .GetByEntityTypeId( new Person().TypeId ).AsQueryable()
                    .OrderBy( a => a.Name )
                    .ToList();

                foreach ( var attribute in attributes )
                {
                    var name = string.Format( "{0} --{1}--", attribute.Name, attribute.FieldType.Name );
                    ddlLastAttemptDateAttribute.Items.Add( new ListItem( name, attribute.Id.ToString() ) );
                    ddlLastPassedDateAttribute.Items.Add( new ListItem( name, attribute.Id.ToString() ) );
                }
            }

            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Survey ) ).Id;

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
                // Fix for github issue #2557.
                pnlDetails.Visible = pnlEdit.Visible = false;

                if ( !string.IsNullOrWhiteSpace( PageParameter( "SurveyId" ) ) )
                {
                    hfId.Value = PageParameter( "SurveyId" );
                    if ( hfId.Value.AsInteger() == 0 )
                    {
                        var survey = new Survey
                        {
                            Id = 0,
                            IsActive = true,
                            RecordAnswers = GetAttributeValue( "RecordAnswersDefault" ).AsBoolean( resultIfNullOrEmpty: false )
                        };
                        survey.CategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();

                        LoadAttributeState( survey );
                        ShowEdit( survey, new RockContext() );
                    }
                    else
                    {
                        ShowDetail( hfId.Value.AsInteger() );
                    }
                }
            }
            else
            {
                if ( pnlEditAnswers.Visible )
                {
                    //
                    // Add the attribute controls.
                    //
                    var surveyResults = new SurveyResult { Id = 0 };
                    surveyResults.SurveyId = hfId.Value.AsInteger();

                    surveyResults.LoadAttributes();
                    phAnswerAttributes.Controls.Clear();
                    Helper.AddEditControls( surveyResults, phAnswerAttributes, false, BlockValidationGroup );
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["SurveyAttributesState"] = JsonConvert.SerializeObject( SurveyAttributesState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Load the attribute state from the survey.
        /// </summary>
        /// <param name="survey">The survey to load state  information from.</param>
        protected void LoadAttributeState( Survey survey )
        {
            //
            // Load attribute data
            //
            SurveyAttributesState = new List<Rock.Model.Attribute>();
            AttributeService attributeService = new AttributeService( new RockContext() );
            string qualifierValue = survey.Id.ToString();

            attributeService.GetByEntityTypeId( new SurveyResult().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "SurveyId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .ToList()
                .ForEach( a => SurveyAttributesState.Add( a ) );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="surveyId">The survey identifier.</param>
        public void ShowDetail( int surveyId )
        {
            var rockContext = new RockContext();
            Survey survey = null;

            pnlDetails.Visible = true;
            pnlEdit.Visible = false;
            HideSecondaryBlocks( false );

            if ( surveyId != 0 )
            {
                survey = new SurveyService( rockContext ).Get( surveyId );
            }

            //
            // Ensure the user is allowed to view this survey.
            //
            if ( survey == null || !survey.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                HideSecondaryBlocks( true );
                nbUnauthorized.Text = EditModeMessage.NotAuthorizedToView( Survey.FriendlyTypeName );
                pnlDetails.Visible = false;
                return;
            }

            //
            // Set all the simple field values.
            //
            pdDetails.SetEntity( survey, ResolveRockUrl( "~" ) );
            lTitle.Text = survey.Name;
            lIconHtml.Text = !string.IsNullOrWhiteSpace( survey.Category.IconCssClass ) ? string.Format( "<i class=\"{0}\"></i>", survey.Category.IconCssClass ) : string.Empty;
            lDescription.Text = survey.Description;
            hlId.Text = string.Format( "#{0}", surveyId );
            hlCategory.Text = survey.Category.Name;
            hlInactive.Visible = !survey.IsActive;

            //
            // Set button states.
            //
            bool canEdit = ( UserCanEdit || UserCanAdministrate ) && survey.IsAuthorized( Authorization.VIEW, CurrentPerson );
            lbEdit.Visible = canEdit;
            lbDelete.Visible = canEdit;
            lbEditAnswers.Visible = survey.PassingGrade.HasValue;
            btnSecurity.Visible = survey.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            lbRun.Visible = survey.IsActive;

            btnSecurity.Title = "Secure " + survey.Name;
            btnSecurity.EntityId = survey.Id;
        }

        /// <summary>
        /// Shows the edit panel.
        /// </summary>
        /// <param name="surveyId">The survey identifier.</param>
        public void ShowEdit( Survey survey, RockContext rockContext )
        {
            pnlDetails.Visible = false;
            pnlEdit.Visible = true;
            HideSecondaryBlocks( true );

            if ( survey.Id != 0 )
            {
                pdAuditDetails.SetEntity( survey, ResolveRockUrl( "~" ) );
            }
            else
            {
                pdAuditDetails.Visible = false;
            }

            if ( ( survey.Id != 0 && !survey.IsAuthorized( Authorization.EDIT, CurrentPerson ) ) )
            {
                HideSecondaryBlocks( true );
                nbUnauthorized.Text = EditModeMessage.NotAuthorizedToEdit( Survey.FriendlyTypeName );
                pnlEdit.Visible = false;
                return;
            }

            string title = survey.Id > 0 ?
                ActionTitle.Edit( Survey.FriendlyTypeName ) :
                ActionTitle.Add( Survey.FriendlyTypeName );
            lEditTitle.Text = title.FormatAsHtmlTitle();

            tbName.Text = survey.Name;
            tbDescription.Text = survey.Description;
            cbIsActive.Checked = survey.IsActive;
            cbIsLoginRequired.Checked = survey.IsLoginRequired;
            cpCategory.SetValue( survey.CategoryId );
            wtpWorkflow.SetValue( survey.WorkflowTypeId );
            cbRecordAnswers.Checked = survey.RecordAnswers;
            ddlLastAttemptDateAttribute.SetValue( survey.LastAttemptDateAttributeId );
            ceInstructionTemplate.Text = survey.InstructionTemplate;
            ceResultTemplate.Text = survey.ResultTemplate;

            cbPassFail.Checked = survey.PassingGrade.HasValue;
            pnlPassFail.Visible = survey.PassingGrade.HasValue;
            if ( survey.PassingGrade.HasValue )
            {
                nbPassingGrade.Text = survey.PassingGrade.Value.ToString();
                ddlLastPassedDateAttribute.SetValue( survey.LastPassedDateAttributeId );
            }

            BindSurveyAttributesGrid();

            pwDetails.Expanded = true;
            pwSurveyAttributes.Expanded = false;
            pwPassFail.Expanded = false;
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="entityId">The survey identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( int entityId, int entityTypeId, List<Rock.Model.Attribute> attributes, RockContext rockContext )
        {
            string qualifierColumn = "SurveyId";
            string qualifierValue = entityId.ToString();

            AttributeService attributeService = new AttributeService( rockContext );

            // Get the existing attributes for this entity type and qualifier value
            var existingAttributes = attributeService.Get( entityTypeId, qualifierColumn, qualifierValue );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                Rock.Web.Cache.AttributeCache.Remove( attr.Id );
                attributeService.Delete( attr );
            }

            rockContext.SaveChanges();

            // Update the Attributes that were assigned in the UI
            foreach ( var attr in attributes )
            {
                Rock.Attribute.Helper.SaveAttributeEdits( attr, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }

            EntityTypeAttributesCache.Clear();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            Survey survey;

            SurveyService surveyService = new SurveyService( rockContext );

            int surveyId = int.Parse( hfId.Value );

            if ( surveyId == 0 )
            {
                survey = new Survey();
                surveyService.Add( survey );
                survey.CreatedByPersonAliasId = CurrentPersonAliasId;
                survey.CreatedDateTime = RockDateTime.Now;
            }
            else
            {
                survey = surveyService.Get( surveyId );
            }

            survey.ModifiedByPersonAliasId = CurrentPersonAliasId;
            survey.ModifiedDateTime = RockDateTime.Now;

            survey.Name = tbName.Text;
            survey.Description = tbDescription.Text;
            survey.IsActive = cbIsActive.Checked;
            survey.IsLoginRequired = cbIsLoginRequired.Checked;
            survey.CategoryId = cpCategory.SelectedValueAsId();
            survey.WorkflowTypeId = wtpWorkflow.SelectedValueAsId();
            survey.RecordAnswers = cbRecordAnswers.Checked;
            survey.LastAttemptDateAttributeId = ddlLastAttemptDateAttribute.SelectedValueAsInt();
            survey.InstructionTemplate = ceInstructionTemplate.Text;
            survey.ResultTemplate = ceResultTemplate.Text;

            if ( cbPassFail.Checked )
            {
                survey.PassingGrade = nbPassingGrade.Text.AsDecimal();
                survey.LastPassedDateAttributeId = ddlLastPassedDateAttribute.SelectedValueAsInt();
            }
            else
            {
                survey.PassingGrade = null;
                survey.LastPassedDateAttributeId = null;
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                // get it back to make sure we have a good Id for it for the Attributes
                survey = surveyService.Get( survey.Guid );

                // Save the Survey Attributes
                int entityTypeId = EntityTypeCache.Get( typeof( SurveyResult ) ).Id;
                SaveAttributes( survey.Id, entityTypeId, SurveyAttributesState, rockContext );
            } );

            if ( surveyId == 0 )
            {
                NavigateToCurrentPage( new Dictionary<string, string>() { { "SurveyId", survey.Id.ToString() } } );
            }
            else
            {
                ShowDetail( surveyId );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( hfId.Value.AsInteger() == 0 )
            {
                NavigateToCurrentPage( new Dictionary<string, string> { { "CategoryId", PageParameter( "ParentCategoryId" ) } } );
            }
            else
            {
                ShowDetail( hfId.Value.AsInteger() );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            int surveyId = hfId.ValueAsInt();

            lbResults.Visible = !string.IsNullOrWhiteSpace( GetAttributeValue( "ResultsPage" ) );

            if ( surveyId != 0 )
            {
                ShowDetail( surveyId );
            }
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var survey = new SurveyService( rockContext ).Get( hfId.Value.AsInteger() );

            LoadAttributeState( survey );

            ShowEdit( survey, rockContext );
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var surveyService = new SurveyService( rockContext );
            int surveyId = hfId.Value.AsInteger();
            var survey = surveyService.Get( surveyId );

            if ( survey.SurveyResults.Any() )
            {
                mdlConfirmDelete.Show();
            }
            else
            {
                int? categoryId = survey.CategoryId;
                surveyService.Delete( survey );

                rockContext.SaveChanges();

                if ( categoryId.HasValue )
                {
                    NavigateToCurrentPage( new Dictionary<string, string> { { "CategoryId", categoryId.Value.ToString() } } );
                }
                else
                {
                    NavigateToCurrentPage();
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdlConfirmDelete_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var surveyService = new SurveyService( rockContext );
            var survey = surveyService.Get( hfId.Value.AsInteger() );
            int? categoryId = survey.CategoryId;

            rockContext.SaveChanges();

            mdlConfirmDelete.Hide();

            if ( categoryId.HasValue )
            {
                NavigateToCurrentPage( new Dictionary<string, string> { { "CategoryId", categoryId.Value.ToString() } } );
            }
            else
            {
                NavigateToCurrentPage();
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbPassFail_CheckedChanged( object sender, EventArgs e )
        {
            pnlPassFail.Visible = cbPassFail.Checked;
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditAnswers_Click( object sender, EventArgs e )
        {
            var surveyResult = new SurveyResult { Id = 0 };

            surveyResult.SurveyId = hfId.Value.AsInteger();
            surveyResult.LoadAttributes();

            using ( var rockContext = new RockContext() )
            {
                var survey = new SurveyService( rockContext ).Get( hfId.Value.AsInteger() );
                IDictionary<string, string> data = null;

                try
                {
                    data = JsonConvert.DeserializeObject<Dictionary<string, string>>( survey.AnswerData );
                }
                catch
                {
                    data = new Dictionary<string, string>();
                }

                foreach ( var kvp in data )
                {
                    surveyResult.SetAttributeValue( kvp.Key, kvp.Value );
                }
            }

            phAnswerAttributes.Controls.Clear();
            Helper.AddEditControls( surveyResult, phAnswerAttributes, true, BlockValidationGroup );

            pnlDetails.Visible = false;
            pnlEditAnswers.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAnswersSave_Click( object sender, EventArgs e )
        {
            var surveyResult = new SurveyResult
            {
                SurveyId = hfId.Value.AsInteger()
            };
            surveyResult.LoadAttributes();

            Helper.GetEditValues( phAnswerAttributes, surveyResult );

            var data = new Dictionary<string, string>();
            foreach ( var attribute in surveyResult.Attributes )
            {
                data.Add( attribute.Value.Key, surveyResult.GetAttributeValue( attribute.Value.Key ) );
            }

            using ( var rockContext = new RockContext() )
            {
                var survey = new SurveyService( rockContext ).Get( hfId.Value.AsInteger() );

                survey.AnswerData = JsonConvert.SerializeObject( data );

                rockContext.SaveChanges();
            }

            pnlEditAnswers.Visible = false;
            ShowDetail( hfId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAnswersCancel_Click( object sender, EventArgs e )
        {
            pnlEditAnswers.Visible = false;

            ShowDetail( hfId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnCopy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var survey = new SurveyService( rockContext ).Get( hfId.Value.AsInteger() );

            if ( survey != null )
            {
                var newSurvey = new Survey();
                newSurvey.CopyPropertiesFrom( survey );
                newSurvey.CreatedByPersonAlias = null;
                newSurvey.CreatedByPersonAliasId = null;
                newSurvey.CreatedDateTime = RockDateTime.Now;
                newSurvey.ModifiedByPersonAlias = null;
                newSurvey.ModifiedByPersonAliasId = null;
                newSurvey.ModifiedDateTime = RockDateTime.Now;
                newSurvey.Id = 0;
                newSurvey.Guid = Guid.NewGuid();
                newSurvey.Name = survey.Name + " - Copy";

                LoadAttributeState( survey );

                //
                // Create temporary state objects for the new survey.
                //
                var newAttributesState = new List<Rock.Model.Attribute>();

                //
                // Clone the survey attributes
                //
                foreach ( var attribute in SurveyAttributesState )
                {
                    var newAttribute = attribute.Clone( false );
                    newAttribute.Id = 0;
                    newAttribute.Guid = Guid.NewGuid();
                    newAttribute.IsSystem = false;
                    newAttributesState.Add( newAttribute );

                    foreach ( var qualifier in attribute.AttributeQualifiers )
                    {
                        var newQualifier = qualifier.Clone( false );
                        newQualifier.Id = 0;
                        newQualifier.Guid = Guid.NewGuid();
                        newQualifier.IsSystem = false;
                        newAttribute.AttributeQualifiers.Add( qualifier );
                    }
                }

                survey = newSurvey;
                SurveyAttributesState = newAttributesState;

                hfId.Value = newSurvey.Id.ToString();
                ShowEdit( newSurvey, rockContext );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbrun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRun_Click( object sender, EventArgs e )
        {
            NavigateToPage( com.shepherdchurch.SurveySystem.SystemGuid.Page.SURVEY_ENTRY.AsGuid(),
                new Dictionary<string, string> { { "SurveyId", hfId.Value } } );
        }

        /// <summary>
        /// Handles the Click event of the lbResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbResults_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "ResultsPage", "SurveyId", hfId.ValueAsInt() );
        }

        #endregion

        #region Survey Attributes

        /// <summary>
        /// Handles the Add event of the gSurveyAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSurveyAttributes_Add( object sender, EventArgs e )
        {
            gSurveyAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gSurveyAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSurveyAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            gSurveyAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Shows the edit dialog for the attribute.
        /// </summary>
        /// <param name="attributeGuid">The attribute guid.</param>
        protected void gSurveyAttributes_ShowEdit( Guid attributeGuid )
        {
            Rock.Model.Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Rock.Model.Attribute
                {
                    FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id
                };
                edtSurveyAttributes.ActionTitle = ActionTitle.Add( tbName.Text + " Survey Question" );

            }
            else
            {
                attribute = SurveyAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtSurveyAttributes.ActionTitle = ActionTitle.Edit( tbName.Text + " Survey Question" );
            }

            edtSurveyAttributes.ReservedKeyNames = SurveyAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();

            edtSurveyAttributes.SetAttributeProperties( attribute, typeof( SurveyResult ) );

            dlgSurveyAttributes.Show();
        }

        /// <summary>
        /// Handles the GridReorder event of the gSurveyAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gSurveyAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            int fixedOrder = 0;
            foreach (var attr in SurveyAttributesState.OrderBy( a => a.Order ) )
            {
                attr.Order = fixedOrder++;
            }

            var movedAttribute = SurveyAttributesState.OrderBy( a => a.Order ).Where( a => a.Order == e.OldIndex ).FirstOrDefault();
            if ( movedAttribute != null )
            {
                if ( e.NewIndex < e.OldIndex )
                {
                    // Moved up
                    foreach ( var otherAttribute in SurveyAttributesState.Where( a => a.Order < e.OldIndex && a.Order >= e.NewIndex ) )
                    {
                        otherAttribute.Order = otherAttribute.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherAttribute in SurveyAttributesState.Where( a => a.Order > e.OldIndex && a.Order <= e.NewIndex ) )
                    {
                        otherAttribute.Order = otherAttribute.Order - 1;
                    }
                }

                movedAttribute.Order = e.NewIndex;
            }

            BindSurveyAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gSurveyAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gSurveyAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            SurveyAttributesState.RemoveEntity( attributeGuid );

            BindSurveyAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSurveyAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSurveyAttributes_GridRebind( object sender, EventArgs e )
        {
            BindSurveyAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgSurveyAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgSurveyAttributes_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtSurveyAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( SurveyAttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = SurveyAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                SurveyAttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = SurveyAttributesState.Any() ? SurveyAttributesState.Max( a => a.Order ) + 1 : 0;
            }
            SurveyAttributesState.Add( attribute );

            BindSurveyAttributesGrid();

            dlgSurveyAttributes.Hide();
        }

        /// <summary>
        /// Binds the survey attribute type grid.
        /// </summary>
        private void BindSurveyAttributesGrid()
        {
            gSurveyAttributes.DataSource = SurveyAttributesState
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
            gSurveyAttributes.DataBind();
        }

        #endregion
    }
}