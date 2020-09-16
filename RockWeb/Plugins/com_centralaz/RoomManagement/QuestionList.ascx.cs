using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Security;

using com.centralaz.RoomManagement.Model;
using com.centralaz.RoomManagement.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.RoomManagement
{
    /// <summary>
    /// User control for managing note types
    /// </summary>
    [DisplayName( "Question List" )]
    [Category( "com_centralaz > Room Management" )]
    [Description( "A list of questions tied to a resource or location" )]
    public partial class QuestionList : RockBlock, ISecondaryBlock
    {
        #region Properties
        public int ResourceId
        {
            get
            {
                return PageParameter( "ResourceId" ).AsInteger();
            }
        }

        public int LocationId
        {
            get
            {
                return PageParameter( "LocationId" ).AsInteger();
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rGrid.DataKeyNames = new string[] { "QuestionId" };
            rGrid.Actions.ShowAdd = true;

            rGrid.Actions.AddClick += rGrid_Add;
            rGrid.GridReorder += rGrid_GridReorder;
            rGrid.GridRebind += rGrid_GridRebind;

            var securityField = rGrid.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) ).Id;
            }

            modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );

            var lbCopyQuestions = new LinkButton();
            lbCopyQuestions.ID = "lbCopyQuestions";
            lbCopyQuestions.CssClass = "btn btn-default btn-sm pull-left";
            lbCopyQuestions.Text = "<i class='fa fa-clone'></i> Copy Questions From...";
            lbCopyQuestions.ToolTip = "Copies questions from another resource/location to this one.";
            lbCopyQuestions.Click += lbCopyQuestions_Click;
            rGrid.Actions.AddCustomActionControl( lbCopyQuestions );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( hfIdValue.Value ) )
                {
                    modalDetails.Show();
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var questionService = new QuestionService( rockContext );
                var attributeService = new AttributeService( rockContext );
                var question = questionService.Get( e.RowKeyId );
                var attribute = question.Attribute;
                if ( question != null )
                {
                    questionService.Delete( question );
                    attributeService.Delete( attribute );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( null );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var attributes = GetAttributeList();
            if ( attributes != null )
            {
                var attributeService = new AttributeService( rockContext );
                var databaseAttributes = attributeService.GetByIds( attributes.Select( a => a.Id ).ToList() ).OrderBy( a => a.Order ).ToList();
                attributeService.Reorder( databaseAttributes, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void modalDetails_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var questionService = new QuestionService( rockContext );
            var attributeService = new AttributeService( rockContext );

            // Save the question
            int questionId = 0;
            if ( hfIdValue.Value != string.Empty && !int.TryParse( hfIdValue.Value, out questionId ) )
            {
                questionId = 0;
            }

            Question question = null;

            if ( questionId != 0 )
            {
                question = questionService.Get( questionId );
            }

            if ( question == null )
            {
                question = new Question();
                if ( ResourceId != 0 )
                {
                    question.ResourceId = ResourceId;
                }
                else
                {
                    if ( LocationId != 0 )
                    {
                        question.LocationId = LocationId;
                    }
                }
                questionService.Add( question );
            }

            var newAttribute = GetAttribute( rockContext );

            var entityTypeId = 0;
            if ( ResourceId != 0 )
            {
                entityTypeId = new EntityTypeService( rockContext ).Get( com.centralaz.RoomManagement.SystemGuid.EntityType.RESERVATION_RESOURCE.AsGuid() ).Id;
            }
            else if ( LocationId != 0 )
            {
                entityTypeId = new EntityTypeService( rockContext ).Get( com.centralaz.RoomManagement.SystemGuid.EntityType.RESERVATION_LOCATION.AsGuid() ).Id;
            }
            newAttribute.EntityTypeId = entityTypeId;

            if ( newAttribute.AbbreviatedName == "" || ( newAttribute.AbbreviatedName == newAttribute.Name && newAttribute.Name.Length > 100 ) )
            {
                newAttribute.AbbreviatedName = newAttribute.Name.Substring( 0, 100 );
            }

            // Controls will show warnings
            if ( !newAttribute.IsValid )
            {
                return;
            }

            Rock.Model.Attribute savedAttribute = Helper.SaveAttributeEdits( newAttribute, entityTypeId, null, null, rockContext );

            AttributeCache.RemoveEntityAttributes();

            question.AttributeId = savedAttribute.Id;

            if ( question.IsValid )
            {
                rockContext.SaveChanges();

                hfIdValue.Value = string.Empty;
                modalDetails.Hide();
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCopyQuestions control by opening up a modal with 
        /// a list of resources or locations to copy questions from.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void lbCopyQuestions_Click( object sender, EventArgs e )
        {
            ddlResourceCopySource.Items.Clear();

            var rockContext = new RockContext();
            var qryResources = new ResourceService( rockContext ).Queryable().AsNoTracking();
            var itemList = new List<ListItem>();
            var list = new ResourceService( new RockContext() ).Queryable().AsNoTracking()
                .Where( r => r.Id != ResourceId )
                .OrderBy( r => r.Name )
                .Select( r => new
                {
                    r.Id,
                    r.Name,
                    CampuName = r.Campus.Name
                } );

            foreach ( var item in list )
            {
                var listItem = new ListItem( item.Name, item.Id.ToString() );
                listItem.Attributes.Add( "OptionGroup", item.CampuName );
                ddlResourceCopySource.Items.Add( listItem );
            }

            // Add an empty item
            ddlResourceCopySource.Items.Insert( 0, new ListItem() );
            ddlResourceCopySource.SelectedIndex = -1;
            ddlResourceCopySource.DataBind();

            mdCopyQuestions.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdCopyQuestions control copying the questions from the source Resource
        /// to the Location or Resource that is being edited.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdCopyQuestions_SaveClick( object sender, EventArgs e )
        {
            try
            {
                var sourceResourceId = ddlResourceCopySource.SelectedValue.AsIntegerOrNull();
                int? sourceLocationId = null;
                if ( locpLocations != null && locpLocations.Location != null )
                {
                    sourceLocationId = locpLocations.Location.Id;
                }

                var rockContext = new RockContext();
                var questionService = new QuestionService( rockContext );
                var attributeService = new AttributeService( rockContext );

                // Build a dictionary of all existing resource attribute keys
                var keyMap = new Dictionary<string, bool>();
                var originalAttributeList = new List<Rock.Model.Attribute>();

                if ( ResourceId != 0 )
                {
                    originalAttributeList = questionService.Queryable().Where( q => q.ResourceId == ResourceId ).Select( q => q.Attribute ).ToList();
                }
                else if ( LocationId != 0 )
                {
                    originalAttributeList = questionService.Queryable().Where( q => q.LocationId == LocationId ).Select( q => q.Attribute ).ToList();
                }

                foreach ( var attribute in originalAttributeList )
                {
                    keyMap.AddOrReplace( attribute.Key, true );
                }

                var nextOrder = attributeService.Queryable().AsNoTracking().Count() + 1;

                // Copy questions from a source Resource..
                if ( sourceResourceId != null )
                {
                    var sourceQuestionList = questionService.Queryable().Where( q => q.ResourceId == sourceResourceId ).ToList();
                    nextOrder = CopyQuestionAttributes( rockContext, questionService, attributeService, keyMap, nextOrder, sourceQuestionList );
                }

                // Copy questions from a source Location... (as long as the source location is not the same as the location being edited)
                if ( sourceLocationId != null && sourceLocationId != LocationId )
                {
                    var sourceQuestionList = questionService.Queryable().Where( q => q.LocationId == sourceLocationId ).ToList();
                    nextOrder = CopyQuestionAttributes( rockContext, questionService, attributeService, keyMap, nextOrder, sourceQuestionList );
                }

                AttributeCache.RemoveEntityAttributes();


                BindGrid();
                mdCopyQuestions.Hide();
            }
            catch ( Exception ex )
            {
                nbMessage.Text = ex.Message;
                nbMessage.Visible = true;
            }
        }

        /// <summary>
        /// Copies the question attributes to either a resource or a location.  
        /// NOTE: This method calls the SaveChanges() method on the rockContext multiple times.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="questionService">The question service where the new questions are being added.</param>
        /// <param name="attributeService">The attribute service.</param>
        /// <param name="keyMap">The key map.</param>
        /// <param name="nextOrder">The next order.</param>
        /// <param name="sourceQuestionList">The source question list.</param>
        /// <returns></returns>
        private int CopyQuestionAttributes( RockContext rockContext, QuestionService questionService, AttributeService attributeService, Dictionary<string, bool> keyMap, int nextOrder, List<Question> sourceQuestionList )
        {
            // Set the entityType based on the type being edited (resource or location)
            var entityTypeId = 0;
            if ( ResourceId != 0 )
            {
                entityTypeId = new EntityTypeService( rockContext ).Get( com.centralaz.RoomManagement.SystemGuid.EntityType.RESERVATION_RESOURCE.AsGuid() ).Id;
            }
            else if ( LocationId != 0 )
            {
                entityTypeId = new EntityTypeService( rockContext ).Get( com.centralaz.RoomManagement.SystemGuid.EntityType.RESERVATION_LOCATION.AsGuid() ).Id;
            }

            foreach ( Question question in sourceQuestionList )
            {
                var cloneQuestion = question.Clone() as Question;
                cloneQuestion.CreatedByPersonAlias = null;
                cloneQuestion.CreatedByPersonAliasId = CurrentPersonAliasId;
                cloneQuestion.CreatedDateTime = RockDateTime.Now;
                cloneQuestion.ModifiedByPersonAlias = null;
                cloneQuestion.ModifiedByPersonAliasId = CurrentPersonAliasId;
                cloneQuestion.ModifiedDateTime = RockDateTime.Now;
                cloneQuestion.Id = 0;
                cloneQuestion.Guid = Guid.NewGuid();
                cloneQuestion.Location = null;
                cloneQuestion.LocationId = null;
                cloneQuestion.Resource = null;
                cloneQuestion.ResourceId = null;
                cloneQuestion.Attribute = null;

                // Make a copy of the question's corresponding attribute.
                var cloneAttribute = question.Attribute.Clone( false ) as Rock.Model.Attribute;
                cloneAttribute.Id = 0;
                cloneAttribute.Guid = Guid.NewGuid();
                cloneAttribute.IsSystem = false;
                cloneAttribute.EntityTypeId = entityTypeId;
                cloneAttribute.AttributeQualifiers.Clear();

                // Set the cloned question to this resource's Id.
                if ( ResourceId != 0 )
                {
                    cloneQuestion.ResourceId = ResourceId;
                    cloneAttribute.Key = String.Format( "Q{0}_ResourceId{1}", nextOrder, ResourceId );

                }
                else if ( LocationId != 0 )
                {
                    cloneQuestion.LocationId = LocationId;
                    cloneAttribute.Key = String.Format( "Q{0}_LocationId{1}", nextOrder, LocationId );
                }

                nextOrder++;

                foreach ( var qualifier in question.Attribute.AttributeQualifiers )
                {
                    var newQualifier = qualifier.Clone( false );
                    newQualifier.Id = 0;
                    newQualifier.Guid = Guid.NewGuid();
                    newQualifier.IsSystem = false;
                    newQualifier.Attribute = null;
                    newQualifier.AttributeId = 0;
                    cloneAttribute.AttributeQualifiers.Add( newQualifier );
                }

                // Verify the key is unique
                string key = cloneAttribute.Key;
                if ( keyMap.ContainsKey( key ) )
                {
                    int count = 0;
                    while ( keyMap.ContainsKey( key ) )
                    {
                        count++;
                        key = cloneAttribute.Key + count;
                    }
                    cloneAttribute.Key = key;
                }

                attributeService.Add( cloneAttribute );
                rockContext.SaveChanges();

                cloneQuestion.AttributeId = cloneAttribute.Id;
                questionService.Add( cloneQuestion );
                rockContext.SaveChanges();
            }

            return nextOrder;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            List<Question> questionList = GetQuestions();

            rGrid.DataSource = questionList.Select( q => new
            {
                Id = q.Attribute.Id,
                QuestionId = q.Id,
                Order = q.Attribute.Order,
                Question = q.Attribute.Name,
                FieldType = q.Attribute.FieldType.Name
            } )
            .OrderBy( q => q.Order )
            .ToList();
            

            rGrid.DataBind();
        }

        /// <summary>
        /// Gets the questions.
        /// </summary>
        /// <returns></returns>
        private List<Question> GetQuestions()
        {
            var questionList = new List<Question>();
            var questionService = new QuestionService( new RockContext() );
            if ( ResourceId != 0 )
            {
                questionList = questionService.Queryable().Where( q => q.ResourceId == ResourceId ).ToList();
            }
            else
            {
                if ( LocationId != 0 )
                {
                    questionList = questionService.Queryable().Where( q => q.LocationId == LocationId ).ToList();
                }
            }

            return questionList;
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="questionId">The question identifier.</param>
        protected void ShowEdit( int? questionId )
        {
            Question question = null;
            if ( questionId.HasValue )
            {
                question = new QuestionService( new RockContext() ).Get( questionId.Value );
            }
            else
            {
                question = new Question();
                question.Attribute = new Rock.Model.Attribute();
                question.Attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
            }

            var reservedKeyNames = new List<string>();
            GetAttributeList().Where( a => !a.Guid.Equals( question.Attribute.Guid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );

            //var includeFields = new List<FieldTypeCache>();
            //includeFields.Add( FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT.AsGuid() ) );
            //includeFields.Add( FieldTypeCache.Read( Rock.SystemGuid.FieldType.MEMO.AsGuid() ) );
            //includeFields.Add( FieldTypeCache.Read( Rock.SystemGuid.FieldType.DATE.AsGuid() ) );
            //includeFields.Add( FieldTypeCache.Read( Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid() ) );
            //edtQuestion.IncludedFieldTypes = includeFields.ToArray();

            edtQuestion.ReservedKeyNames = reservedKeyNames.ToList();
            edtQuestion.IsKeyEditable = false;

            Type objectType = null;
            if ( ResourceId != 0 )
            {
                objectType = typeof( ReservationResource );
            }
            else
            {
                if ( LocationId != 0 )
                {
                    objectType = typeof( ReservationLocation );
                }
            }

            edtQuestion.SetAttributeProperties( question.Attribute, objectType );

            hfIdValue.Value = questionId.ToString();
            modalDetails.Show();
        }

        /// <summary>
        /// Saves the attribute.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Rock.Model.Attribute GetAttribute( RockContext rockContext )
        {
            List<Rock.Model.Attribute> attributeList = GetAttributeList();

            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtQuestion.GetAttributeProperties( attribute );

            if ( attributeList.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = attributeList.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
            }
            else
            {
                attribute.Order = attributeList.Any() ? attributeList.Count() + 1 : 0;
            }



            // Create and update a new attribute object with new values
            rockContext = rockContext ?? new RockContext();
            var internalAttributeService = new AttributeService( rockContext );

            Rock.Model.Attribute oldAttribute = null;
            var newAttribute = new Rock.Model.Attribute();

            if ( edtQuestion.AttributeId.HasValue )
            {
                oldAttribute = internalAttributeService.Get( edtQuestion.AttributeId.Value );
            }

            if ( oldAttribute != null )
            {
                newAttribute.CopyPropertiesFrom( oldAttribute );
            }
            else
            {
                newAttribute.Order = internalAttributeService.Queryable().AsNoTracking().Count() + 1;
            }

            edtQuestion.GetAttributeProperties( newAttribute );

            if ( !newAttribute.Key.Contains( "_ResourceId" ) && !newAttribute.Key.Contains( "_LocationId" ) )
            {
                if ( ResourceId != 0 )
                {
                    newAttribute.Key = String.Format( "Q{0}_ResourceId{1}", newAttribute.Order, ResourceId );
                }
                else if ( LocationId != 0 )
                {
                    newAttribute.Key = String.Format( "Q{0}_LocationId{1}", newAttribute.Order, LocationId );
                }
            }


            return newAttribute;
        }

        /// <summary>
        /// Gets the attribute list.
        /// </summary>
        /// <returns></returns>
        private List<Rock.Model.Attribute> GetAttributeList()
        {
            var rockContext = new RockContext();
            var questionService = new QuestionService( rockContext );
            var attributeList = new List<Rock.Model.Attribute>();
            if ( ResourceId != 0 )
            {
                attributeList = questionService.Queryable().Where( q => q.ResourceId == ResourceId ).Select( q => q.Attribute ).ToList();
            }
            else
            {
                if ( LocationId != 0 )
                {
                    attributeList = questionService.Queryable().Where( q => q.LocationId == LocationId ).Select( q => q.Attribute ).ToList();
                }
            }

            return attributeList;
        }

        #endregion
    }
}