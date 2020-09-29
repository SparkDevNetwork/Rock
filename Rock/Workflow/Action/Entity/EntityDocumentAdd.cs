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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Adds a document to the provided entity.
    /// </summary>
    [ActionCategory( "Entity" )]
    [Description( "Adds a document to the provided entity." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Entity Document Add" )]

    #region Block Attributes

    [EntityTypeField(
        "Entity Type",
        Description = "The type of entity the document will be applied to. Rock only supports Person, Group, and Group Member at this time.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.EntityType )]

    [WorkflowTextOrAttribute(
        "Entity Id or Guid",
        "Entity Attribute",
        Description = "The Id or Guid of the Entity. Rock only supports Person, Group, and Group Member at this time. <span class='tip tip-lava'></span>.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.EntityIdOrGuid )]

    [DocumentTypeField(
        "Document Type",
        Description = "The type of document that should be used for the file.  The type selected must be a type that is valid for the selected Entity Type above.",
        IsRequired = true,
        AllowMultiple = false,
        Order = 2,
        Key = AttributeKey.DocumentType )]

    [WorkflowAttribute(
        "Document Attribute",
        Description = "The workflow attribute that contains the document.",
        IsRequired = true,
        Order = 3,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.FileFieldType" },
        Key = AttributeKey.DocumentAttribute )]

    [TextField(
        "Document Name",
        Key = AttributeKey.DocumentName,
        Description = "The name to the use for the document. <span class='tip tip-lava'></span>.",
        IsRequired = false,
        Order = 4 )]

    [MemoField(
        "Document Description",
        Description = "The description to use for the document. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 5,
        Key = AttributeKey.DocumentDescription )]

    #endregion
    public class EntityDocumentAdd : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string EntityType = "EntityType";
            public const string EntityIdOrGuid = "EntityIdOrGuid";
            public const string DocumentType = "DocumentType";
            public const string DocumentAttribute = "DocumentAttribute";
            public const string DocumentName = "DocumentName";
            public const string DocumentDescription = "DocumentDescription";
        }

        #endregion

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // Get the entity type
            EntityTypeCache entityType = null;
            var entityTypeGuid = GetAttributeValue( action, AttributeKey.EntityType ).AsGuidOrNull();
            if ( entityTypeGuid.HasValue )
            {
                entityType = EntityTypeCache.Get( entityTypeGuid.Value );
            }

            if ( entityType == null )
            {
                var message = $"Entity Type could not be found for selected value ('{entityTypeGuid.ToString()}')!";
                errorMessages.Add( message );
                action.AddLogEntry( message, true );
                return false;
            }

            var mergeFields = GetMergeFields( action );
            var _rockContext = new RockContext();

            // Get the entity
            var entityTypeService = new EntityTypeService( _rockContext );
            IEntity entityObject = null;
            var entityIdGuidString = GetAttributeValue( action, AttributeKey.EntityIdOrGuid, true ).ResolveMergeFields( mergeFields ).Trim();
            var entityId = entityIdGuidString.AsIntegerOrNull();
            if ( entityId.HasValue )
            {
                entityObject = entityTypeService.GetEntity( entityType.Id, entityId.Value );
            }
            else
            {
                var entityGuid = entityIdGuidString.AsGuidOrNull();
                if ( entityGuid.HasValue )
                {
                    entityObject = entityTypeService.GetEntity( entityType.Id, entityGuid.Value );
                }
            }

            if ( entityObject == null )
            {
                var value = GetActionAttributeValue( action, AttributeKey.EntityIdOrGuid );
                entityObject = action.GetEntityFromAttributeValue( value, rockContext );
            }

            if ( entityObject == null )
            {
                var message = $"Entity could not be found for selected value ('{entityIdGuidString}')!";
                errorMessages.Add( message );
                action.AddLogEntry( message, true );
                return false;
            }

            var documentypesForContextEntityType = DocumentTypeCache.GetByEntity( entityType.Id, false );
            var attributeFilteredDocumentType = GetAttributeValue( action, AttributeKey.DocumentType ).Split( ',' ).Select( int.Parse ).FirstOrDefault();
            var documentype = documentypesForContextEntityType.FirstOrDefault( d => attributeFilteredDocumentType == d.Id );
            if ( documentype == null )
            {
                var message = "The Document Type does not match the selected entity type.";
                errorMessages.Add( message );
                action.AddLogEntry( message, true );
                return false;
            }

            var binaryFile = new BinaryFileService( rockContext ).Get( GetAttributeValue( action, AttributeKey.DocumentAttribute, true ).AsGuid() );

            if ( binaryFile == null )
            {
                action.AddLogEntry( "The document to add to the entity was not found.", true );

                // returning true here to allow the action to run 'successfully' without a document.
                // This allows the action to be easily used when the document is optional without a bunch of action filter tests.
                return true;
            }

            var documentName = GetAttributeValue( action, AttributeKey.DocumentName ).ResolveMergeFields( mergeFields );
            if ( documentName.IsNullOrWhiteSpace() )
            {
                documentName = Path.GetFileNameWithoutExtension( binaryFile.FileName );
            }

            var document = new Document();
            document.Name = documentName;
            document.Description = GetAttributeValue( action, AttributeKey.DocumentDescription ).ResolveMergeFields( mergeFields );
            document.EntityId = entityObject.Id;
            document.DocumentTypeId = documentype.Id;
            document.SetBinaryFile( binaryFile.Id, rockContext );

            var documentService = new DocumentService( rockContext );
            documentService.Add( document );
            rockContext.SaveChanges();

            action.AddLogEntry( "Added document to the Entity." );
            return true;
        }
    }
}

