using System;
using System.ComponentModel;
using System.Collections.Generic;
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
    public partial class QuestionList : RockBlock
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

        #endregion

        #region Methods

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
    }
}