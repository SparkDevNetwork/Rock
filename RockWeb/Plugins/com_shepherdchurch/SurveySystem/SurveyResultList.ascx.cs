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
    [DisplayName( "Survey Result List" )]
    [Category( "Shepherd Church > Survey System" )]
    [Description( "Lists survey results in the system." )]

    [LinkedPage( "Detail Page", "The page that allows the user to view the details of a result.", false, "", "", 0 )]
    [LinkedPage( "Chart Page", "The page that allows the user to view the results with charts.", false, "", "", 1 )]
    public partial class SurveyResultList : RockBlock, ICustomGridColumns
    {
        #region Private Fields

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            AddDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gList.DataKeyNames = new string[] { "Id" };
            gList.GridRebind += gList_GridRebind;
            gList.Actions.ShowAdd = false;
            gList.IsDeleteEnabled = true;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "DetailPage" ) ) )
            {
                gList.RowSelected += gList_RowSelected;
            }
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
                var survey = new SurveyService( new RockContext() ).Get( PageParameter( "SurveyId" ).AsInteger() );

                if ( survey == null || !survey.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    nbUnauthorizedMessage.Text = EditModeMessage.NotAuthorizedToView( Survey.FriendlyTypeName );
                    pnlResultList.Visible = false;

                    return;
                }

                ltTitle.Text = string.Format( "{0} Results", survey.Name );

                ViewState["CanDelete"] = survey.IsAuthorized( Authorization.EDIT, CurrentPerson );
                gList.Columns[2].Visible = survey.PassingGrade.HasValue;
                gList.Columns[3].Visible = survey.PassingGrade.HasValue;

                aChartLink.HRef = LinkedPageUrl( "ChartPage", new Dictionary<string, string>
                {
                    { "SurveyId", PageParameter( "SurveyId" ) }
                } );
                aChartLink.Visible = aChartLink.HRef.IsNotNullOrWhiteSpace();

                SetFilter();
                BindGrid();
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
            ViewState["AvailableAttributes"] = AvailableAttributes;

            return base.SaveViewState();
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Bind the data grid to the list of survey results in the system.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var surveyResultService = new SurveyResultService( rockContext );
            var sortProperty = gList.SortProperty;
            int surveyId = PageParameter( "SurveyId" ).AsInteger();

            var qry = surveyResultService.Queryable()
                .Where( r => r.SurveyId == surveyId );

            //
            // Date Completed Range Filter.
            //
            if ( drpDateCompleted.LowerValue.HasValue )
            {
                qry = qry.Where( p => p.CreatedDateTime >= drpDateCompleted.LowerValue.Value );
            }
            if ( drpDateCompleted.UpperValue.HasValue )
            {
                DateTime upperDate = drpDateCompleted.UpperValue.Value.Date.AddDays( 1 );
                qry = qry.Where( p => p.CreatedDateTime < upperDate );
            }

            //
            // Completed By Person Filter.
            //
            int? createdByPersonId = ppCompletedBy.SelectedValue;
            if ( createdByPersonId.HasValue )
            {
                qry = qry.Where( p => p.CreatedByPersonAliasId.HasValue && p.CreatedByPersonAlias.PersonId == createdByPersonId.Value );
            }

            //
            // Filter query by any configured attribute filters
            //
            if ( AvailableAttributes != null && AvailableAttributes.Any() )
            {
                var attributeValueService = new AttributeValueService( rockContext );
                var parameterExpression = attributeValueService.ParameterExpression;

                foreach ( var attribute in AvailableAttributes.Where( a => a.IsGridColumn ) )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                        var filterIsDefault = attribute.FieldType.Field.IsEqualToValue( filterValues, attribute.DefaultValue );
                        var expression = attribute.FieldType.Field.AttributeFilterExpression( attribute.QualifierValues, filterValues, parameterExpression );

                        if ( expression != null && expression.GetType().Name != "NoAttributeFilterExpression" )
                        {
                            var attributeValues = attributeValueService
                                .Queryable()
                                .Where( v => v.Attribute.Id == attribute.Id );

                            var filteredAttributeValues = attributeValues.Where( parameterExpression, expression, null );

                            if ( filterIsDefault )
                            {
                                qry = qry.Where( w =>
                                    !attributeValues.Any( v => v.EntityId == w.Id ) ||
                                    filteredAttributeValues.Select( v => v.EntityId ).Contains( w.Id ) );
                            }
                            else
                            {
                                qry = qry.Where( w => filteredAttributeValues.Select( v => v.EntityId ).Contains( w.Id ) );
                            }
                        }
                    }
                }
            }

            //
            // Store the queried objects in the grid for it to use later.
            //
            var qryList = qry.ToList();
            gList.ObjectList = new Dictionary<string, object>();
            qryList.ForEach( q => gList.ObjectList.Add( q.Id.ToString(), q ) );

            if ( sortProperty != null )
            {
                gList.DataSource = qryList.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gList.DataSource = qryList.OrderByDescending( r => r.Id ).ToList();
            }

            gList.EntityTypeId = EntityTypeCache.Get<SurveyResult>().Id;
            gList.DataBind();
        }

        /// <summary>
        /// Add all the dynamic columns such as attributes and delete fields.
        /// </summary>
        protected void AddDynamicControls()
        {
            // Remove attribute columns
            foreach ( var column in gList.Columns.OfType<AttributeField>().ToList() )
            {
                gList.Columns.Remove( column );
            }

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    if ( attribute.IsGridColumn )
                    {
                        var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                        if ( control != null )
                        {
                            if ( control is IRockControl )
                            {
                                var rockControl = ( IRockControl ) control;
                                rockControl.Label = attribute.Name;
                                rockControl.Help = attribute.Description;
                                phAttributeFilters.Controls.Add( control );
                            }
                            else
                            {
                                var wrapper = new RockControlWrapper
                                {
                                    ID = control.ID + "_wrapper",
                                    Label = attribute.Name
                                };
                                wrapper.Controls.Add( control );
                                phAttributeFilters.Controls.Add( wrapper );
                            }
                        }

                        string savedValue = gfList.GetUserPreference( MakeKeyUniqueToType( attribute.Key ) );
                        if ( !string.IsNullOrWhiteSpace( savedValue ) )
                        {
                            try
                            {
                                var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                            }
                            catch { }
                        }
                    }

                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gList.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField
                        {
                            DataField = dataFieldExpression,
                            AttributeId = attribute.Id,
                            HeaderText = attribute.Name,
                            Condensed = true,
                            Visible = attribute.IsGridColumn
                        };

                        var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gList.Columns.Add( boundField );
                    }
                }
            }

            if ( ( bool ) ViewState["CanDelete"] )
            {
                var deleteField = new DeleteField();
                gList.Columns.Add( deleteField );
                deleteField.Click += gList_Delete;
            }
        }

        /// <summary>
        /// Binds the filter to the last used values by the user.
        /// </summary>
        private void SetFilter()
        {
            var personService = new PersonService( new RockContext() );

            BindAttributes();
            AddDynamicControls();

            drpDateCompleted.DelimitedValues = gfList.GetUserPreference( MakeKeyUniqueToType( "Date Completed" ) );

            int? personId = gfList.GetUserPreference( MakeKeyUniqueToType( "Completed By" ) ).AsIntegerOrNull();
            ppCompletedBy.SetValue( personId.HasValue ? personService.Get( personId.Value ) : null );
        }

        /// <summary>
        /// Build a list of attributes that are defined for the survey
        /// we are currently looking at.
        /// </summary>
        private void BindAttributes()
        {
            AvailableAttributes = new List<AttributeCache>();

            int entityTypeId = new SurveyResult().TypeId;
            string typeQualifier = PageParameter( "SurveyId" );

            //
            // Make a list of all attributes that are defined for this entity.
            //
            foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.EntityTypeQualifierColumn.Equals( "SurveyId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( typeQualifier ) )
                .OrderByDescending( a => a.EntityTypeQualifierColumn )
                .ThenBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                AvailableAttributes.Add( AttributeCache.Get( attributeModel ) );
            }
        }

        /// <summary>
        /// Make a unique key for the survey we are viewing.
        /// </summary>
        /// <param name="key">The key to be made unique.</param>
        /// <returns>A string that is unique for the current survey.</returns>
        private string MakeKeyUniqueToType( string key )
        {
            return string.Format( "{0}-{1}", PageParameter( "SurveyId" ), key );
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
            nbDangerMessage.Text = string.Empty;

            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "SurveyResultId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var surveyResultService = new SurveyResultService( rockContext );
            var surveyResult = surveyResultService.Get( e.RowKeyId );

            if ( surveyResult != null )
            {
                if ( !surveyResult.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    mdGridWarning.Show( "You are not authorized to delete this survey result.", ModalAlertType.Information );
                    return;
                }

                surveyResultService.Delete( surveyResult );

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gList_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfList_ApplyFilterClick( object sender, EventArgs e )
        {
            //
            // Save the simple values.
            //
            gfList.SaveUserPreference( MakeKeyUniqueToType( "Date Completed" ), "Date Completed", drpDateCompleted.DelimitedValues );

            //
            // Save the person picker values.
            //
            int? personId = ppCompletedBy.SelectedValue;
            gfList.SaveUserPreference( MakeKeyUniqueToType( "Completed By" ), "Completed By", personId.HasValue ? personId.Value.ToString() : "" );

            //
            // Save any attribute filter values.
            //
            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            gfList.SaveUserPreference( MakeKeyUniqueToType( attribute.Key ), attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                        }
                        catch { }
                    }
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DisplayFilterValueArgs"/> instance containing the event data.</param>
        protected void gfList_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            //
            // Check if the key we are displaying is an attribute.
            //
            if ( AvailableAttributes != null )
            {
                var attribute = AvailableAttributes.FirstOrDefault( a => MakeKeyUniqueToType( a.Key ) == e.Key );
                if ( attribute != null )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                        return;
                    }
                    catch { }
                }
            }

            if ( e.Key == MakeKeyUniqueToType( "Created Date" ) )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == MakeKeyUniqueToType( "Created By" ) )
            {
                int? personId = e.Value.AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var person = new PersonService( new RockContext() ).Get( personId.Value );
                    if ( person != null )
                    {
                        e.Value = person.FullName;
                    }
                }
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        #endregion
    }
}