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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache.Entities
{
    /// <inheritdoc cref="KnowledgeBaseFolder"/>
    [Serializable]
    [DataContract]
    internal class KnowledgeBaseFolderCache : ModelCache<KnowledgeBaseFolderCache, KnowledgeBaseFolder>, IHasReadOnlyAdditionalSettings
    {
        #region Entity Properties

        /// <inheritdoc cref="KnowledgeBaseFolder.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="KnowledgeBaseFolder.Description"/>
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="KnowledgeBaseFolder.ContextHint"/>
        [DataMember]
        public string ContextHint { get; private set; }

        /// <inheritdoc cref="KnowledgeBaseFolder.KnowledgeBaseId"/>
        [DataMember]
        public int KnowledgeBaseId { get; private set; }

        /// <inheritdoc cref="KnowledgeBaseFolder.SourceEntityTypeId"/>
        [DataMember]
        public int? SourceEntityTypeId { get; private set; }

        /// <inheritdoc cref="KnowledgeBaseFolder.SourceKey"/>
        [DataMember]
        public string SourceKey { get; private set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; private set; }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is KnowledgeBaseFolder folder ) )
            {
                return;
            }

            Name = folder.Name;
            Description = folder.Description;
            ContextHint = folder.ContextHint;
            KnowledgeBaseId = folder.KnowledgeBaseId;
            SourceEntityTypeId = folder.SourceEntityTypeId;
            SourceKey = folder.SourceKey;
            AdditionalSettingsJson = folder.AdditionalSettingsJson;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
