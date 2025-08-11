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
using Rock.Enums.AI.Agent;
using Rock.Model;

namespace Rock.Web.Cache.Entities
{
    /// <inheritdoc cref="AISkillFunction"/>
    [Serializable]
    [DataContract]
    public class AISkillFunctionCache : ModelCache<AISkillFunctionCache, AISkillFunction>, IHasReadOnlyAdditionalSettings
    {
        #region Entity Properties

        /// <inheritdoc cref="AISkillFunction.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="AISkillFunction.Description"/>
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="AISkillFunction.UsageHint"/>
        [DataMember]
        public string UsageHint { get; private set; }

        /// <inheritdoc cref="AISkillFunction.FunctionType"/>
        [DataMember]
        public FunctionType FunctionType { get; private set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; private set; }

        #endregion

        #region Navigation Properties

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is AISkillFunction function ) )
            {
                return;
            }

            Name = function.Name;
            Description = function.Description;
            UsageHint = function.UsageHint;
            FunctionType = function.FunctionType;
            AdditionalSettingsJson = function.AdditionalSettingsJson;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
