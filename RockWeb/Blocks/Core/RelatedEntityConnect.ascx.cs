// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Reflection;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Related Entity Connect" )]
    [Category( "Core" )]
    [Description( "Connects two related entities with options." )]

    [EntityTypeField( "Source Entity Type",
        Description = "The type of entity to that will be the source.",
        Order = 0,
        IsRequired = true,
        Key = AttributeKey.SourceEntityType )]

    [EntityTypeField( "Target Entity Type",
        Description = "The type of entity to that will be the target.",
        Order = 1,
        IsRequired = true,
        Key = AttributeKey.TargetEntityType )]

    [TextField( "Purpose Key",
        Description = "The purpose key to use for linking the two entities together. While this is not required, it is highly recommended that you provide one.",
        IsRequired = false,
        DefaultValue = "",
        Order = 2,
        Key = AttributeKey.PurposeKey )]

    [CustomRadioListField( "Parameter Type",
        Description = "Determines the type of the paramters that are being passed in for the source and target. Guids are more secure but requires 2 additional lookups to convert them to integers. The query string parameters are 'Source' and 'Target'",
        IsRequired = true,
        DefaultValue = "Guid",
        ListSource = "Guid, Integer",
        Order = 3,
        Key = AttributeKey.ParameterType )]

    [BooleanField( "Enable Edit",
        Description = "Determines if existing relationships should be editable or if this should always add a new value.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 4,
        Key = AttributeKey.EnableEdit )]

    [BooleanField( "Show Quantity",
        Description = "Determines if the quantity field should be shown.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 5,
        Key = AttributeKey.ShowQuantity )]

    [BooleanField( "Show Note",
        Description = "Determines if the note field should be shown.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 6,
        Key = AttributeKey.ShowNote )]

    [BooleanField( "Enable Attribute Editing",
        Description = "Determines if the attributes of the related entities should be allowed to be edited.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Order = 7,
        Key = AttributeKey.EnableAttributeEditing )]

    [CodeEditorField( "Header Lava Template",
        Description = "The Lava template to use for the header",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        Order = 8,
        Key = AttributeKey.HeaderLavaTemplate )]

    [TextField( "Header Title",
        Description = "The title for the panel heading.",
        IsRequired = false,
        DefaultValue = "Related Entity Connector",
        Order = 9,
        Key = AttributeKey.HeaderTitle )]

    [TextField( "Header Icon CSS Class",
        Description = "The CSS icon for the panel heading.",
        IsRequired = false,
        DefaultValue = "fa fa-link",
        Order = 10,
        Key = AttributeKey.HeaderIconCssClass )]

    [IntegerField( "Attribute Columns",
        Description = "How many columns should the attribute editor use.",
        DefaultIntegerValue = 2,
        IsRequired = true,
        Order = 11,
        Key = AttributeKey.AttributeColumns )]

    [Rock.SystemGuid.BlockTypeGuid( "5F40F4FD-338A-4711-87F7-980ED1FAE615" )]
    public partial class RelatedEntityConnect : RockBlock
    {
        public static class AttributeKey
        {
            public const string SourceEntityType = "SourceEntityType";
            public const string TargetEntityType = "TargetEntityType";
            public const string PurposeKey = "PurposeKey";
            public const string ShowQuantity = "ShowQuantity";
            public const string ShowNote = "ShowNote";
            public const string ParameterType = "ParameterType";
            public const string EnableEdit = "EnableEdit";
            public const string HeaderLavaTemplate = "HeaderLavaTemplate";
            public const string EnableAttributeEditing = "EnableAttributeEditing";
            public const string HeaderTitle = "HeaderTitle";
            public const string HeaderIconCssClass = "HeaderIconCssClass";
            public const string AttributeColumns = "AttributeColumns";
        }

        public static class PageParameterKey
        {
            public const string Source = "Source";
            public const string Target = "Target";
        }

        #region Attribute Properties

        /// <summary>
        /// Gets the type of the source entity.
        /// </summary>
        /// <value>
        /// The type of the source entity.
        /// </value>
        protected Guid SourceEntityType => GetAttributeValue( AttributeKey.SourceEntityType ).AsGuid();

        /// <summary>
        /// Gets the type of the target entity.
        /// </summary>
        /// <value>
        /// The type of the target entity.
        /// </value>
        protected Guid TargetEntityType => GetAttributeValue( AttributeKey.TargetEntityType ).AsGuid();

        /// <summary>
        /// Gets the purpose key.
        /// </summary>
        /// <value>
        /// The purpose key.
        /// </value>
        protected string PurposeKey => GetAttributeValue( AttributeKey.PurposeKey );

        /// <summary>
        /// Gets a value indicating whether [show quantity].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show quantity]; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowQuantity => GetAttributeValue( AttributeKey.ShowQuantity ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether [show note].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show note]; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowNote => GetAttributeValue( AttributeKey.ShowNote ).AsBoolean();

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <value>
        /// The type of the parameter.
        /// </value>
        protected string ParameterType => GetAttributeValue( AttributeKey.ParameterType );

        /// <summary>
        /// Gets a value indicating whether [enable edit].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable edit]; otherwise, <c>false</c>.
        /// </value>
        protected bool EnableEdit => GetAttributeValue( AttributeKey.EnableEdit ).AsBoolean();

        /// <summary>
        /// Gets the header lava template.
        /// </summary>
        /// <value>
        /// The header lava template.
        /// </value>
        protected string HeaderLavaTemplate => GetAttributeValue( AttributeKey.HeaderLavaTemplate );

        /// <summary>
        /// Gets a value indicating whether [enable attribute editing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable attribute editing]; otherwise, <c>false</c>.
        /// </value>
        protected bool EnableAttributeEditing => GetAttributeValue( AttributeKey.EnableAttributeEditing ).AsBoolean();

        /// <summary>
        /// Gets the header icon CSS class.
        /// </summary>
        /// <value>
        /// The header icon CSS class.
        /// </value>
        protected string HeaderIconCssClass => GetAttributeValue( AttributeKey.HeaderIconCssClass );

        /// <summary>
        /// Gets the header title.
        /// </summary>
        /// <value>
        /// The header title.
        /// </value>
        protected string HeaderTitle => GetAttributeValue( AttributeKey.HeaderTitle );

        /// <summary>
        /// Gets the attribute columns.
        /// </summary>
        /// <value>
        /// The attribute columns.
        /// </value>
        protected int AttributeColumns => GetAttributeValue( AttributeKey.AttributeColumns ).AsInteger();

        #endregion

        #region ViewState Properties

        /// <summary>
        /// Gets or sets the current relationship identifier.
        /// </summary>
        /// <value>
        /// The current relationship identifier.
        /// </value>
        public string PreviousUrl
        {
            get
            {
                return ViewState["PreviousUrl"].ToString();
            }

            set
            {
                ViewState["PreviousUrl"] = value;
            }
        }

        #endregion

        #region Control Methods

        private int sourceEntityTypeId = 0;
        private int targetEntityTypeId = 0;

        private int? sourceEntityId = 0;
        private int? targetEntityId = 0;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var configurationCorrect = SetupEnvironment();

            // Ensure we're properly configured
            if ( !configurationCorrect )
            {
                nbMessages.Text = "The configuration of this block is not correct.";
                pnlEdit.Visible = false;
                return;
            }

            ShowEdit();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            pnlEdit.Visible = true;
            var configurationCorrect = SetupEnvironment();

            if ( !Page.IsPostBack )
            {
                // Set previous URL into ViewState so we can return back to it
                PreviousUrl = Request.UrlReferrer.ToString();

                // Ensure we're properly configured
                if ( !configurationCorrect )
                {
                    nbMessages.Text = "The configuration of this block is not correct.";
                    pnlEdit.Visible = false;
                    base.OnLoad( e );
                    return;
                }

                ShowEdit();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            
            var rockContext = new RockContext();
            var relatedEntityService = new RelatedEntityService( rockContext );

            RelatedEntity relatedEntity = null;

            // Check if we should be editing existing records and if so load it
            if ( EnableEdit )
            {
                relatedEntity = GetExistingRelationship( rockContext, relatedEntityService );
            }

            if ( relatedEntity == null )
            {
                relatedEntity = new RelatedEntity();
                relatedEntity.SourceEntityTypeId = sourceEntityTypeId;
                relatedEntity.TargetEntityTypeId = targetEntityTypeId;
                relatedEntity.SourceEntityId = sourceEntityId.Value;
                relatedEntity.TargetEntityId = targetEntityId.Value;
                relatedEntity.PurposeKey = PurposeKey;

                relatedEntityService.Add( relatedEntity );
            }

            // Save quantity
            if ( GetAttributeValue( AttributeKey.ShowQuantity ).AsBoolean() )
            {
                relatedEntity.Quantity = nudQuantity.Value;
            }

            // Save note
            if ( GetAttributeValue( AttributeKey.ShowNote ).AsBoolean() )
            {
                relatedEntity.Note = txtNotes.Text;
            }
            
            rockContext.SaveChanges();

            // Save the attributes
            if( EnableAttributeEditing )
            {
                // Save the attributes
                relatedEntity.LoadAttributes( rockContext );
                avcAttributes.GetEditValues( relatedEntity );

                rockContext.SaveChanges();
                relatedEntity.SaveAttributeValues( rockContext );
            }

            Response.Redirect( PreviousUrl );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            Response.Redirect( PreviousUrl );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Setups the environment on page load.
        /// </summary>
        /// <returns></returns>
        private bool SetupEnvironment()
        {
            // Load information from the query string
            var sourceEntityTypeGuid = GetAttributeValue( AttributeKey.SourceEntityType ).AsGuidOrNull();
            var targetEntityTypeGuid = GetAttributeValue( AttributeKey.TargetEntityType ).AsGuidOrNull();

            // No entities no ids
            if (!sourceEntityTypeGuid.HasValue || !targetEntityTypeGuid.HasValue )
            {
                sourceEntityTypeId = 0;
                targetEntityTypeId = 0;
                return false;
            }

            // Lookup ids from cache
            sourceEntityTypeId = EntityTypeCache.Get( sourceEntityTypeGuid.Value ).Id;
            targetEntityTypeId = EntityTypeCache.Get( targetEntityTypeGuid.Value ).Id;

            // If configuration is Ids then use what was provided
            if ( ParameterType == "Integer" )
            {
                sourceEntityId = PageParameter( PageParameterKey.Source ).AsIntegerOrNull();
                targetEntityId = PageParameter( PageParameterKey.Target ).AsIntegerOrNull();
                return true;
            }
            else
            {
                // We're using guids so we need to convert those to Ids
                var sourceEntityGuid = PageParameter( PageParameterKey.Source ).AsGuid();
                var targetEntityGuid = PageParameter( PageParameterKey.Target ).AsGuid();

                // Convert the guids to ids by looking them up in the database
                sourceEntityId = Reflection.GetEntityIdForEntityType( sourceEntityTypeGuid.Value,  sourceEntityGuid );
                targetEntityId = Reflection.GetEntityIdForEntityType( targetEntityTypeGuid.Value, targetEntityGuid );

                if ( !sourceEntityId.HasValue || !targetEntityId.HasValue )
                {
                    return false; // We don't have valid entity ids
                }

                return true;
            }
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        private void ShowEdit()
        {
            // Check that we have source and target ids from the query string
            if ( !sourceEntityId.HasValue || !targetEntityId.HasValue )
            {
                nbMessages.Text = "Invalid source and target entities provided.";
                pnlEdit.Visible = false;
            }

            lTitle.Text = HeaderTitle;
            lIcon.Text = $"<i class=\"{ HeaderIconCssClass}\"></i> ";
            avcAttributes.NumberOfColumns = AttributeColumns;

            // Render header content
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            lHeaderContent.Text = HeaderLavaTemplate.ResolveMergeFields( mergeFields );

            // Check if quantity should be show
            nudQuantity.Visible = GetAttributeValue( AttributeKey.ShowQuantity ).AsBoolean();

            // Check if notes should be shown
            txtNotes.Visible = GetAttributeValue( AttributeKey.ShowNote ).AsBoolean();

            RelatedEntity relatedEntity = null;

            // Determine if we should show existing values and if so load their values
            if ( EnableEdit )
            {
                // Check for existing values, if there are multiple we'll assume the first item
                relatedEntity = GetExistingRelationship();

                if ( relatedEntity != null )
                {
                    if ( relatedEntity.Quantity.HasValue )
                    {
                        nudQuantity.Value = relatedEntity.Quantity.Value;
                        
                    }

                    txtNotes.Text = relatedEntity.Note;
                }
            }

            // We need to configure the attribute value container
            if ( EnableAttributeEditing )
            {
                if ( relatedEntity == null )
                {
                    // Create a stub of a relationship object so that the attribute editor knows what attributes to show
                    relatedEntity = new RelatedEntity();
                    relatedEntity.SourceEntityTypeId = sourceEntityTypeId;
                    relatedEntity.TargetEntityTypeId = targetEntityTypeId;
                    relatedEntity.PurposeKey = PurposeKey;
                }

                relatedEntity.LoadAttributes();

                avcAttributes.AddEditControls( relatedEntity, Rock.Security.Authorization.EDIT, CurrentPerson );
            }
            
        }

        /// <summary>
        /// Gets the existing relationship.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="relatedEntityService">The related entity service.</param>
        /// <returns></returns>
        private RelatedEntity GetExistingRelationship( RockContext rockContext = null, RelatedEntityService relatedEntityService = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            if ( relatedEntityService == null )
            {
                relatedEntityService = new RelatedEntityService( rockContext );
            }

            // Get entity type for person alias. If either the source or target use person alias
            // we'll want to search using any of the aliases for the person
            var personAliasEntityId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.PERSON_ALIAS ).Id;
            var personAliasService = new PersonAliasService( rockContext );

            var personAliasSourceQry = GetAllPersonAliasIdsForPersonByPersonAliasId( sourceEntityId.Value, personAliasService );
            var personAliasTargetQry = GetAllPersonAliasIdsForPersonByPersonAliasId( targetEntityId.Value, personAliasService );

            var qry = relatedEntityService.Queryable()
                                    .Where( r =>
                                        r.SourceEntityTypeId == sourceEntityTypeId
                                        && r.TargetEntityTypeId == targetEntityTypeId
                                        && r.PurposeKey == PurposeKey
                                    );

            // Add filters for EntityIds
            if( sourceEntityTypeId == personAliasEntityId )
            {
                qry = qry.Where( r => personAliasSourceQry.Contains( r.SourceEntityId ) );
            }
            else
            {
                qry = qry.Where( r => r.SourceEntityId == sourceEntityId);
            }

            if ( targetEntityTypeId == personAliasEntityId )
            {
                qry = qry.Where( r => personAliasSourceQry.Contains( r.TargetEntityId ) );
            }
            else
            {
                qry = qry.Where( r => r.TargetEntityId == targetEntityId );
            }

            // Return results
            return qry.OrderBy( r => r.Order ).FirstOrDefault();
        }

        private IQueryable<int> GetAllPersonAliasIdsForPersonByPersonAliasId( int personAliasId, PersonAliasService personAliasService )
        {
            var qry = personAliasService.Queryable()
                        .Where( pa => pa.Id == personAliasId )
                        .Select( pa => pa.PersonId);

            return personAliasService.Queryable()
                        .Where( pa => qry.Contains( pa.PersonId ) )
                        .Select( pa => pa.Id );
        }

        #endregion
    }
}