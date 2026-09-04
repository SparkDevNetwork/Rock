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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Newtonsoft.Json;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility.EntityCoding;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Workflow.ShareWorkflow;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Workflow
{
    /// <summary>
    /// Exports and imports workflow types as portable JSON files.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Share Workflow" )]
    [Category( "Workflow" )]
    [Description( "Export and import workflows from Rock." )]
    [IconCssClass( "ti ti-share" )]

    [Rock.SystemGuid.EntityTypeGuid( "1366E38D-3434-4044-BB6E-9F1A5B88A76F" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "D5D75422-42F9-41CD-B2B1-53DB411BAF2B" )]
    [Rock.SystemGuid.BlockTypeGuid( "DA262642-A07E-43B0-BE27-8CEF6070C9B8" )]
    public class ShareWorkflow : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string WorkflowType = "WorkflowTypeId";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// The user value key the decoder uses to determine which category an
        /// imported workflow type should be placed in.
        /// </summary>
        private const string WorkflowCategoryUserValueKey = "WorkflowCategory";

        /// <summary>
        /// Entity types that are excluded from the export preview because they are
        /// implementation details rather than meaningful to the person reviewing the export.
        /// </summary>
        private static readonly HashSet<string> ExcludedPreviewTypes = new HashSet<string>
        {
            "Attribute",
            "AttributeValue",
            "AttributeQualifier",
            "WorkflowActionFormAttribute"
        };

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<ShareWorkflowBag, ShareWorkflowOptionsBag>
            {
                Bag = new ShareWorkflowBag()
            };

            var workflowTypeKey = PageParameter( PageParameterKey.WorkflowType );

            if ( workflowTypeKey.IsNotNullOrWhiteSpace() )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( workflowType != null )
                {
                    box.Bag.InitialWorkflowType = new ListItemBag
                    {
                        Value = workflowType.Guid.ToString(),
                        Text = workflowType.Name
                    };
                }
            }

            return box;
        }

        /// <summary>
        /// Resolves a workflow type from a picker value that may be an Id, IdKey, or Guid.
        /// </summary>
        /// <param name="workflowTypeValue">The value emitted by the workflow type picker.</param>
        /// <returns>The matching <see cref="WorkflowType"/>, or <c>null</c> when it could not be resolved.</returns>
        private WorkflowType GetWorkflowType( string workflowTypeValue )
        {
            if ( workflowTypeValue.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new WorkflowTypeService( RockContext ).Get( workflowTypeValue, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets a friendly name that identifies the entity to the user. The entity's
        /// <see cref="object.ToString"/> value is used when it looks like a usable name;
        /// otherwise the unique identifier is used as a fallback.
        /// </summary>
        /// <param name="entity">The entity whose display name is needed.</param>
        /// <returns>A name suitable for display.</returns>
        private static string GetEntityFriendlyName( IEntity entity )
        {
            var name = entity.ToString();

            if ( name.Length > 40 || name.Contains( "<" ) )
            {
                name = entity.Guid.ToString();
            }

            return name;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Builds a preview of the entities that will be included when the specified
        /// workflow type is exported.
        /// </summary>
        /// <param name="workflowTypeValue">The workflow type to preview.</param>
        /// <returns>The list of entities that would be exported.</returns>
        [BlockAction]
        public BlockActionResult Preview( string workflowTypeValue )
        {
            var workflowType = GetWorkflowType( workflowTypeValue );

            if ( workflowType == null )
            {
                return ActionBadRequest( "Select a workflow type to preview." );
            }

            var coder = new EntityCoder( RockContext );
            coder.EnqueueEntity( workflowType, new WorkflowTypeExporter() );

            var previewEntities = new List<ShareWorkflowPreviewEntityBag>();

            foreach ( var queuedEntity in coder.Entities )
            {
                var shortType = CodingHelper.GetEntityType( queuedEntity.Entity ).Name;

                if ( ExcludedPreviewTypes.Contains( shortType ) )
                {
                    continue;
                }

                previewEntities.Add( new ShareWorkflowPreviewEntityBag
                {
                    Guid = queuedEntity.Entity.Guid.ToString(),
                    Name = GetEntityFriendlyName( queuedEntity.Entity ),
                    ShortType = shortType,
                    IsCritical = queuedEntity.IsCritical,
                    IsNewGuid = queuedEntity.RequiresNewGuid,
                    Paths = queuedEntity.ReferencePaths.Select( path => path.ToString() ).ToList()
                } );
            }

            return ActionOk( previewEntities );
        }

        /// <summary>
        /// Exports the specified workflow type to a JSON file the browser can download.
        /// </summary>
        /// <param name="workflowTypeValue">The workflow type to export.</param>
        /// <returns>The file name and JSON content of the export.</returns>
        [BlockAction]
        public BlockActionResult Export( string workflowTypeValue )
        {
            var workflowType = GetWorkflowType( workflowTypeValue );

            if ( workflowType == null )
            {
                return ActionBadRequest( "Select a workflow type to export." );
            }

            var coder = new EntityCoder( RockContext );
            coder.EnqueueEntity( workflowType, new WorkflowTypeExporter() );

            var container = coder.GetExportedEntities();

            return ActionOk( new ShareWorkflowExportResultBag
            {
                FileName = $"{workflowType.Name.MakeValidFileName()}_{RockDateTime.Now:yyyyMMddHHmm}.json",
                Json = JsonConvert.SerializeObject( container, Formatting.Indented )
            } );
        }

        /// <summary>
        /// Imports a previously exported workflow type file into the selected category.
        /// </summary>
        /// <param name="bag">The file, category, and test-only flag for the import.</param>
        /// <returns>Whether the import succeeded along with the messages it produced.</returns>
        [BlockAction]
        public BlockActionResult Import( ShareWorkflowImportRequestBag bag )
        {
            var fileGuid = bag?.File?.Value.AsGuidOrNull();
            var categoryGuid = bag?.Category?.Value.AsGuidOrNull();

            if ( !fileGuid.HasValue )
            {
                return ActionBadRequest( "Select a file to import." );
            }

            if ( !categoryGuid.HasValue )
            {
                return ActionBadRequest( "Select a category for the imported workflow." );
            }

            var binaryFile = new BinaryFileService( RockContext ).Get( fileGuid.Value );

            if ( binaryFile == null )
            {
                return ActionBadRequest( "The uploaded file could not be found. Please re-upload it." );
            }

            var category = new CategoryService( RockContext ).Get( categoryGuid.Value );

            if ( category == null )
            {
                return ActionBadRequest( "The selected category could not be found." );
            }

            ExportedEntitiesContainer container;

            try
            {
                container = JsonConvert.DeserializeObject<ExportedEntitiesContainer>( binaryFile.ContentsToString() );
            }
            catch ( JsonException )
            {
                return ActionBadRequest( "The uploaded file is not a valid workflow export file." );
            }

            if ( container == null )
            {
                return ActionBadRequest( "The uploaded file is not a valid workflow export file." );
            }

            var decoder = new EntityDecoder( RockContext );
            decoder.UserValues.Add( WorkflowCategoryUserValueKey, category );

            var isSuccess = decoder.Import( container, bag.IsTestOnly, out var messages );

            return ActionOk( new ShareWorkflowImportResultBag
            {
                IsSuccess = isSuccess,
                Messages = messages ?? new List<string>()
            } );
        }

        #endregion Block Actions
    }
}
