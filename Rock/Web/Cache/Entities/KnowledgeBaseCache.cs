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
    /// <inheritdoc cref="KnowledgeBase"/>
    [Serializable]
    [DataContract]
    internal class KnowledgeBaseCache : ModelCache<KnowledgeBaseCache, KnowledgeBase>, IHasReadOnlyAdditionalSettings
    {
        #region Entity Properties

        /// <inheritdoc cref="KnowledgeBase.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="KnowledgeBase.Description"/>
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="KnowledgeBase.ContextHint"/>
        [DataMember]
        public string ContextHint { get; private set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; private set; }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is KnowledgeBase knowledgeBase ) )
            {
                return;
            }

            Name = knowledgeBase.Name;
            Description = knowledgeBase.Description;
            ContextHint = knowledgeBase.ContextHint;
            AdditionalSettingsJson = knowledgeBase.AdditionalSettingsJson;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
