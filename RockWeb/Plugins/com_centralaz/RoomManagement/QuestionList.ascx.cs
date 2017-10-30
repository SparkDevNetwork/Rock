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

            rGrid.DataKeyNames = new string[] { "Id" };
            rGrid.Actions.ShowAdd = true;

            rGrid.Actions.AddClick += rGrid_Add;
            rGrid.GridReorder += rGrid_GridReorder;
            rGrid.GridRebind += rGrid_GridRebind;
            
            modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );

            var lbCopyQuestions = new LinkButton();
            lbCopyQuestions.ID = "lbCopyQuestions";
            lbCopyQuestions.CssClass = "btn btn-default btn-sm pull-left";
            lbCopyQuestions.Text = "<i class='fa fa-clone'></i> Copy Questions";
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
            var questions = GetQuestions();
            if ( questions != null )
            {
                new QuestionService( rockContext ).Reorder( questions, e.OldIndex, e.NewIndex );
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

            Rock.Model.Attribute savedAttribute = SaveAttribute( rockContext );

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
        /// Handles the Click event of the lbCopyQuestions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void lbCopyQuestions_Click( object sender, EventArgs e )
        {
            ddlCopySource.Items.Clear();

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
                ddlCopySource.Items.Add( listItem );
            }

            // Add an empty item

            ddlCopySource.Items.Insert( 0, new ListItem() );
            ddlCopySource.SelectedIndex = -1;
            ddlCopySource.DataBind();

            mdCopyQuestions.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdCopyQuestions control copying the questions from the source Resource
        /// to this resource being edited.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdCopyQuestions_SaveClick( object sender, EventArgs e )
        {
            try
            {
                var sourceResourceId = ddlCopySource.SelectedValue.AsIntegerOrNull();
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

                // Copy from a source Resource
                if ( sourceResourceId != null )
                {
                    var sourceResource = new ResourceService( rockContext ).Get( sourceResourceId.Value );
                    if ( sourceResource != null )
                    {
                        var sourceQuestionList = questionService.Queryable().Where( q => q.ResourceId == sourceResourceId ).ToList();
                        var resourceEntityTypeId = new EntityTypeService( rockContext ).Get( com.centralaz.RoomManagement.SystemGuid.EntityType.RESERVATION_RESOURCE.AsGuid() ).Id;

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
                            //cloneQuestion.Attributes.Clear();
                            // Set the cloned question to this resource's Id.
                            cloneQuestion.Resource = null;
                            cloneQuestion.ResourceId = ResourceId;
                            cloneQuestion.Attribute = null;

                            // Make a copy of the question's corresponding attribute.
                            var cloneAttribute = question.Attribute.Clone(false) as Rock.Model.Attribute;
                            cloneAttribute.Id = 0;
                            cloneAttribute.Guid = Guid.NewGuid();
                            cloneAttribute.IsSystem = false;
                            cloneAttribute.EntityTypeId = resourceEntityTypeId;
                            cloneAttribute.AttributeQualifiers.Clear();

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
                            
                        }

                        rockContext.SaveChanges();
                    }
                }
                else
                {
                    // Copy from a source Location
                }

                BindGrid();
                mdCopyQuestions.Hide();
            }
            catch( Exception ex )
            {
                nbMessage.Text = ex.Message;
                nbMessage.Visible = true;
            }

            
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
                Id = q.Id,
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
                question.Attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
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
        private Rock.Model.Attribute SaveAttribute( RockContext rockContext )
        {
            List<Rock.Model.Attribute> attributeList = GetAttributeList();

            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtQuestion.GetAttributeProperties( attribute );
            if ( !attribute.IsValid )
            {
                attribute = null;
            }

            if ( attributeList.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = attributeList.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
            }
            else
            {
                attribute.Order = attributeList.Any() ? attributeList.Max( a => a.Order ) + 1 : 0;
            }

            var entityTypeId = 0;
            if ( ResourceId != 0 )
            {
                entityTypeId = new EntityTypeService( rockContext ).Get( com.centralaz.RoomManagement.SystemGuid.EntityType.RESERVATION_RESOURCE.AsGuid() ).Id;
            }
            else
            {
                if ( LocationId != 0 )
                {
                    entityTypeId = new EntityTypeService( rockContext ).Get( com.centralaz.RoomManagement.SystemGuid.EntityType.RESERVATION_LOCATION.AsGuid() ).Id;
                }
            }

            var savedAttribute = SaveAttributeEdits( edtQuestion, entityTypeId, null, null, rockContext );
            AttributeCache.FlushEntityAttributes();
            return savedAttribute;
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

        /// <summary>
        /// Saves the attribute edits.
        /// </summary>
        /// <param name="edtAttribute">The edt attribute.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Rock.Model.Attribute SaveAttributeEdits( SimpleAttributeEditor edtAttribute, int? entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue, RockContext rockContext = null )
        {
            // Create and update a new attribute object with new values
            rockContext = rockContext ?? new RockContext();
            var internalAttributeService = new AttributeService( rockContext );

            Rock.Model.Attribute attribute = null;
            var newAttribute = new Rock.Model.Attribute();

            if ( edtAttribute.AttributeId.HasValue )
            {
                attribute = internalAttributeService.Get( edtAttribute.AttributeId.Value );
            }

            if ( attribute != null )
            {
                newAttribute.CopyPropertiesFrom( attribute );
            }
            else
            {
                newAttribute.Order = internalAttributeService.Queryable().Max( a => a.Order ) + 1;
            }

            edtAttribute.GetAttributeProperties( newAttribute );

            return Helper.SaveAttributeEdits( newAttribute, entityTypeId, entityTypeQualifierColumn, entityTypeQualifierValue, rockContext );
        }

        #endregion
    }
}