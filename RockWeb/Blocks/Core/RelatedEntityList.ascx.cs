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
using System.Collections.Generic;
using Rock.Utility;
using System.Web;

/*
    USAGE DOCUMENTATION

    This uses several postback actions that require specific inputs.

    DeleteRelationship
    ---------------------
    RelatedEntityGuid|PurposeKey|ConfirmationMessage

    ConfirmationMessage (optional) - If you don't provide one no confirmation will be displayed. Be sure that this message is escaped if you think it could contain unsafe characters.
    PurposeKey (optional) - If you don't provide one the first key from the block setting will be used.

    AddRelationship
    --------------------
    SourceEntity(Id/Guid)|TargetEntity(Id/Guid)|PurposeKey|ConfirmationMessage

    ConfirmationMessage (optional) - If you don't provide one no confirmation will be displayed. Be sure that this message is escaped if you think it could contain unsafe characters.
    PurposeKey (optional) - If you don't provide one the first key from the block setting will be used.

    Sample:
    {% assign safeMinifigName = minifig.Value | EscapeDataString %}
    {% assign postbackAddParm = CurrentPerson.PrimaryAliasId | Append:'|' | Append:minifig.Id | Append:'|PERSONAL_WANT|Do you really want the ' | Append:safeMinifigName | Append:' minifigure?' %}

*/


namespace RockWeb.Blocks.Core
{
    [DisplayName( "Related Entity List" )]
    [Category( "Core" )]
    [Description( "Lists information about related entities." )]

    [EntityTypeField( "Source Entity Type",
        Description = "The type of entity to that will be the source.",
        Order = 0,
        IsRequired = true,
        Key = AttributeKey.SourceEntityType )]

    [BooleanField( "Source Is Current Person",
        Description = "Determines if the current person should be used as the source. If true the Source Entity Type should be set to PersonAlias.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 1,
        Key = AttributeKey.SourceIsCurrentPerson )]

    [EntityTypeField( "Target Entity Type",
        Description = "The type of entity to that will be the target.",
        Order = 2,
        IsRequired = true,
        Key = AttributeKey.TargetEntityType )]

    [TextField( "Purpose Key",
        Description = "Comma delimited list of purpose key(s) to use for linking the two entities together. While this is not required, it is highly recommended that you provide at least one.",
        IsRequired = false,
        DefaultValue = "",
        Order = 3,
        Key = AttributeKey.PurposeKey )]

    [CustomRadioListField( "Parameter Type",
        Description = "Determines the type of the paramters that are being passed in for the source and target. Guids are more secure but requires 2 additional lookups to convert them to integers. The query string parameters are 'Source' and 'Target'",
        IsRequired = true,
        DefaultValue = "Guid",
        ListSource = "Guid, Integer",
        Order = 4,
        Key = AttributeKey.ParameterType )]

    [CodeEditorField( "Lava Template",
        Description = "The Lava template to use for the header",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        Order = 5,
        Key = AttributeKey.LavaTemplate )]

    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block.",
        IsRequired = false,
        Order = 6,
        Key = AttributeKey.EnabledLavaCommands )]

    [TextField( "Header Title",
        Description = "The title for the panel heading.",
        IsRequired = false,
        DefaultValue = "Related Entity List",
        Order = 7,
        Key = AttributeKey.HeaderTitle )]

    [TextField( "Header Icon CSS Class",
        Description = "The CSS icon for the panel heading.",
        IsRequired = false,
        DefaultValue = "fa fa-link",
        Order = 8,
        Key = AttributeKey.HeaderIconCssClass )]

    [Rock.SystemGuid.BlockTypeGuid( "28516B18-7423-4A97-9223-B97537BD0F79" )]
    public partial class RelatedEntityList : RockBlock
    {
        public static class AttributeKey
        {
            public const string SourceEntityType = "SourceEntityType";
            public const string SourceIsCurrentPerson = "SourceIsCurrentPerson";
            public const string TargetEntityType = "TargetEntityType";
            public const string PurposeKey = "PurposeKey";
            public const string ParameterType = "ParameterType";
            public const string LavaTemplate = "LavaTemplate";
            public const string HeaderTitle = "HeaderTitle";
            public const string HeaderIconCssClass = "HeaderIconCssClass";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        public static class PageParameterKey
        {
            public const string Source = "Source";
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
        /// Gets a value indicating whether [source is current person].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [source is current person]; otherwise, <c>false</c>.
        /// </value>
        protected bool SourceIsCurrentPerson => GetAttributeValue( AttributeKey.SourceIsCurrentPerson ).AsBoolean();

        /// <summary>
        /// Gets the type of the target entity.
        /// </summary>
        /// <value>SourceIsCurrentPerson
        /// The type of the target entity.
        /// </value>SourceIsCurrentPerson
        protected Guid TargetEntityType => GetAttributeValue( AttributeKey.TargetEntityType ).AsGuid();

        /// <summary>
        /// Gets the purpose key.
        /// </summary>
        /// <value>
        /// The purpose key.
        /// </value>
        protected List<string> PurposeKeys
        {
            get
            {
                if ( _purposeKeys == null )
                {
                    _purposeKeys = GetAttributeValue( AttributeKey.PurposeKey ).Split(',').Select( p => p.Trim() ).ToList();
                }

                return _purposeKeys;
            }
        }
        private List<string> _purposeKeys;

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <value>
        /// The type of the parameter.
        /// </value>
        protected string ParameterType => GetAttributeValue( AttributeKey.ParameterType );

        /// <summary>
        /// Gets the header lava template.
        /// </summary>
        /// <value>
        /// The header lava template.
        /// </value>
        protected string LavaTemplate => GetAttributeValue( AttributeKey.LavaTemplate );

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

        #endregion

        #region ViewState Properties

        /// <summary>
        /// Gets or sets the current relationship identifier.
        /// </summary>
        /// <value>
        /// The current relationship identifier.
        /// </value>
        public int CurrentRelationshipId
        {
            get
            {
                return ViewState["CurrentRelationshipId"] as int? ?? 0;
            }

            set
            {
                ViewState["CurrentRelationshipId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current source entity identifier.
        /// </summary>
        /// <value>
        /// The current source entity identifier.
        /// </value>
        public int CurrentSourceEntityId
        {
            get
            {
                return ViewState["CurrentSourceEntityId"] as int? ?? 0;
            }

            set
            {
                ViewState["CurrentSourceEntityId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current target entity identifier.
        /// </summary>
        /// <value>
        /// The current target entity identifier.
        /// </value>
        public int CurrentTargetEntityId
        {
            get
            {
                return ViewState["CurrentTargetEntityId"] as int? ?? 0;
            }

            set
            {
                ViewState["CurrentTargetEntityId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current purpose key.
        /// </summary>
        /// <value>
        /// The current purpose key.
        /// </value>
        public string CurrentPurposeKey
        {
            get
            {
                return ViewState["CurrentPurposeKey"] as string ?? string.Empty;
            }

            set
            {
                ViewState["CurrentPurposeKey"] = value;
            }
        }

        #endregion

        #region Control Methods

        private int sourceEntityTypeId = 0;
        private int targetEntityTypeId = 0;

        private int? sourceEntityId = 0;

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
                return;
            }

            ShowContent();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            var configurationCorrect = SetupEnvironment();

            RouteAction();

            if ( !Page.IsPostBack )
            {

                // Ensure we're properly configured
                if ( !configurationCorrect )
                {
                    nbMessages.Text = "The configuration of this block is not correct.";
                    base.OnLoad( e );
                    return;
                }

                ShowContent();
            }

            base.OnLoad( e );
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

            if ( SourceIsCurrentPerson )
            {
                if ( CurrentPerson == null )
                {
                    return false;
                }

                sourceEntityId = CurrentPerson.PrimaryAliasId;
                return true;
            }

            // If configuration is Ids then use what was provided
            if ( ParameterType == "Integer" )
            {
                sourceEntityId = PageParameter( PageParameterKey.Source ).AsIntegerOrNull();
                return true;
            }
            else
            {
                // We're using guids so we need to convert those to Ids
                var sourceEntityGuid = PageParameter( PageParameterKey.Source ).AsGuid();

                // Convert the guids to ids by looking them up in the database
                sourceEntityId = Reflection.GetEntityIdForEntityType( sourceEntityTypeGuid.Value, sourceEntityGuid );

                if ( !sourceEntityId.HasValue )
                {
                    return false; // We don't have valid source entity id
                }

                return true;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RelatedEntityList" /> class from being created.
        /// </summary>
        private void ShowContent()
        {
            lTitle.Text = HeaderTitle;
            lIcon.Text = $"<i class=\"{ HeaderIconCssClass}\"></i> ";

            // Render content
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

            // Get existing relationships
            var existingRelationships = GetExistingRelationships();
            mergeFields.Add( "ExistingRelationships", existingRelationships );

            lContent.Text = LavaTemplate.ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
        }

        /// <summary>
        /// Gets the existing relationship.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="relatedEntityService">The related entity service.</param>
        /// <returns></returns>
        private List<RelatedEntity> GetExistingRelationships()
        {

            var rockContext = new RockContext();
            var relatedEntityService = new RelatedEntityService( rockContext );

            // Get entity type for person alias. If either the source or target use person alias
            // we'll want to search using any of the aliases for the person
            var personAliasEntityId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.PERSON_ALIAS ).Id;
            var personAliasService = new PersonAliasService( rockContext );

            var personAliasSourceQry = GetAllPersonAliasesForPersonByPersonAlias( sourceEntityId.Value, personAliasService );

            var qry = relatedEntityService.Queryable()
                                    .Where( r =>
                                        r.SourceEntityTypeId == sourceEntityTypeId
                                        && r.TargetEntityTypeId == targetEntityTypeId
                                        && PurposeKeys.Contains( r.PurposeKey )
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

            
            // Return results
            var results = qry.ToList();

            return results;
        }

        /// <summary>
        /// Gets all person aliases for person by person alias.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="personAliasService">The person alias service.</param>
        /// <returns></returns>
        private IQueryable<int> GetAllPersonAliasesForPersonByPersonAlias( int personAliasId, PersonAliasService personAliasService )
        {
            var qry = personAliasService.Queryable()
                        .Where( pa => pa.Id == personAliasId )
                        .Select( pa => pa.PersonId);

            return personAliasService.Queryable()
                        .Where( pa => qry.Contains( pa.PersonId ) )
                        .Select( pa => pa.Id );
        }

        /// <summary>
        /// Routes the action.
        /// </summary>
        private void RouteAction()
        {
            var sm = ScriptManager.GetCurrent( Page );

            if ( Request.Form["__EVENTARGUMENT"] != null )
            {
                string[] eventArgs = Request.Form["__EVENTARGUMENT"].Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    var action = eventArgs[0];
                    var parameters = eventArgs[1].Split( '|' ).Select( p => p.Trim() ).ToList();

                    switch ( action )
                    {
                        case "DeleteRelationship":
                            ProcessDeleteRequest( parameters );
                            break;
                        case "AddRelationship":
                            ProcessAddRequest( parameters );
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Processes the add request.
        /// </summary>
        /// <param name="parms">The parms.</param>
        private void ProcessAddRequest( List<string> parms )
        {
            var confirmationMessage = string.Empty;
            var purposeKey = PurposeKeys.First() ?? string.Empty;

            Guid sourceEntityGuid;
            Guid targetEntityGuid;

            // Get source entity id
            if ( Guid.TryParse( parms[0], out sourceEntityGuid ) )
            {
                var sourceEntityId = Reflection.GetEntityIdForEntityType( sourceEntityTypeId, sourceEntityGuid );
                this.CurrentSourceEntityId = sourceEntityId.HasValue ? sourceEntityId.Value : 0;
            }
            else
            {
                this.CurrentSourceEntityId = parms[0].AsInteger();
            }

            // Get target entity id
            if ( Guid.TryParse( parms[1], out targetEntityGuid ) )
            {
                var targetEntityId = Reflection.GetEntityIdForEntityType( targetEntityTypeId, targetEntityGuid );
                this.CurrentTargetEntityId = targetEntityId.HasValue ? targetEntityId.Value : 0;
            }
            else
            {
                this.CurrentTargetEntityId = parms[1].AsInteger();
            }

            // Check that we have valid entity ids
            if (this.CurrentSourceEntityId == 0 || this.CurrentTargetEntityId == 0 )
            {
                return;
            }

            // Get purpose key
            if ( parms.Count >= 3 )
            {
                this.CurrentPurposeKey = parms[2];
            }

            // Get confirmation message
            if ( parms.Count >= 4 )
            {
                confirmationMessage = parms[3];
            }

            // If no add confirmation requested just add the item
            if ( confirmationMessage.IsNullOrWhiteSpace() )
            {
                AddRelationship();
                return;
            }

            // Show confirmation message
            lConfirmAddMsg.Text = HttpUtility.UrlDecode( confirmationMessage );
            mdConfirmAdd.Show();
        }

        /// <summary>
        /// Adds the relationship.
        /// </summary>
        private void AddRelationship()
        {
            var rockContext = new RockContext();
            var relatedEntityService = new RelatedEntityService( rockContext );

            var relatedEntity = new RelatedEntity();

            relatedEntityService.Add( relatedEntity );

            relatedEntity.SourceEntityTypeId = sourceEntityTypeId;
            relatedEntity.TargetEntityTypeId = targetEntityTypeId;
            relatedEntity.SourceEntityId = CurrentSourceEntityId;
            relatedEntity.TargetEntityId = CurrentTargetEntityId;
            relatedEntity.PurposeKey = CurrentPurposeKey;

            rockContext.SaveChanges();

            ShowContent();
        }

        /// <summary>
        /// Displays the delete relationship.
        /// </summary>
        /// <param name="relationshipId">The relationship identifier.</param>
        private void ProcessDeleteRequest( List<string> parms )
        {
            var confirmationMessage = string.Empty;
            var purposeKey = PurposeKeys.First() ?? string.Empty;

            Guid relationshipGuid ;

            if ( !Guid.TryParse( parms[0], out relationshipGuid ) )
            {
                return; // Invalid related entity guid provided
            }

            // Get purpose key
            if ( parms.Count >= 2 )
            {
                purposeKey = parms[1];
            }

            // Get confirmation message
            if ( parms.Count >= 3 )
            {
                confirmationMessage = parms[2];
            }

            var rockContext = new RockContext();
            var relatedEntityService = new RelatedEntityService( rockContext );

            var relatedEntity = relatedEntityService.Get( relationshipGuid );
            if ( relatedEntity != null )
            {
                // Persist the relationship id for use in partial postbacks
                this.CurrentRelationshipId = relatedEntity.Id;
                this.CurrentPurposeKey = purposeKey;

                // If no confirmation message process delete now.
                if ( confirmationMessage.IsNullOrWhiteSpace() )
                {
                    DeleteExistingRelationship();
                    return;
                }
                else
                {
                    lConfirmDeleteMsg.Text = HttpUtility.UrlDecode( confirmationMessage );
                }

                mdConfirmDelete.Show();
            }
        }

        /// <summary>
        /// Deletes the existing relationship.
        /// </summary>
        private void DeleteExistingRelationship()
        {
            var rockContext = new RockContext();
            var relatedEntityService = new RelatedEntityService( rockContext );

            var relatedEntity = relatedEntityService.Get( this.CurrentRelationshipId );
            if ( relatedEntity != null )
            {
                relatedEntityService.Delete( relatedEntity );
            }

            rockContext.SaveChanges();

            ShowContent();
        }
        #endregion

        #region Events

        /// <summary>
        /// Handles the SaveClick event of the mdConfirmAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfirmAdd_SaveClick( object sender, EventArgs e )
        {
            mdConfirmAdd.Hide();

            AddRelationship();
        }

        /// <summary>
        /// Handles the Click event of the mdConfirmDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdConfirmDelete_Click( object sender, EventArgs e )
        {
            mdConfirmDelete.Hide();

            DeleteExistingRelationship();
        }

        #endregion
    }
}