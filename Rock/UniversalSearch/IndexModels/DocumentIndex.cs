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
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.UniversalSearch.IndexModels.Attributes;

namespace Rock.UniversalSearch.IndexModels
{
    /// <summary>
    /// Content Channel Item Index
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexModels.IndexModelBase" />
    public class DocumentIndex : IndexModelBase
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [RockIndexField( Boost = 3 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [RockIndexField]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the document type.
        /// </summary>
        /// <value>
        /// The document type.
        /// </value>
        [RockIndexField]
        public string DocumentType { get; set; }

        /// <summary>
        /// Gets or sets the binary file identifier.
        /// </summary>
        /// <value>
        /// The binary file identifier.
        /// </value>
        [RockIndexField( Type = IndexFieldType.Number, Index = IndexType.NotIndexed )]
        public int BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [RockIndexField( Type = IndexFieldType.Number, Index = IndexType.NotIndexed )]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this document is part of Rock core.
        /// </summary>
        /// <value>
        /// <c>true</c> if this document is part of Rock core; otherwise, <c>false</c>.
        /// </value>
        [RockIndexField( Type = IndexFieldType.Boolean )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Loads the by model.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        public static DocumentIndex LoadByModel( Document document )
        {
            var documentIndex = new DocumentIndex();
            documentIndex.SourceIndexModel = "Rock.Model.Document";

            documentIndex.Id = document.Id;
            documentIndex.Name = document.Name;
            documentIndex.Description = document.Description;
            documentIndex.DocumentType = document.DocumentType.Name;
            documentIndex.BinaryFileId = document.BinaryFile.Id;
            documentIndex.EntityId = document.EntityId;
            documentIndex.IsSystem = document.IsSystem;

            AddIndexableAttributes( documentIndex, document );

            return documentIndex;
        }

        /// <summary>
        /// Formats the search result.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="displayOptions">The display options.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns></returns>
        public override FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null, Dictionary<string, object> mergeFields = null )
        {
            var result = base.FormatSearchResult( person, displayOptions );
            result.IsViewAllowed = false;

            bool isSecurityDisabled = displayOptions != null && displayOptions.ContainsKey( "Document-IsSecurityDisabled" ) && displayOptions["Document-IsSecurityDisabled"].ToString().AsBoolean();

            // Check security on the document if security is enabled
            var document = new DocumentService( new RockContext() ).Get( ( int ) this.Id );
            if ( document != null )
            {
                result.IsViewAllowed = document.IsAuthorized( Authorization.VIEW, person ) || isSecurityDisabled;
            }

            return result;
        }
    }
}
