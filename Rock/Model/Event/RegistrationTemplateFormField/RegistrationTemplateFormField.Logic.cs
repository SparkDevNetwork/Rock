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

using System.Collections.Generic;
using System.Data.Entity;
using System.Runtime.Serialization;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RegistrationTemplateFormField
    {
        #region Entity Properties

        /// <summary>
        /// JSON Serialized <see cref="FieldVisibilityRules"/>
        /// </summary>
        /// <value>
        /// The field visibility rules json.
        /// </value>
        [DataMember]
        public string FieldVisibilityRulesJSON
        {
            get
            {
                return FieldVisibilityRules?.ToJson();
            }

            set
            {
                Field.FieldVisibilityRules rules = null;
                if ( value.IsNotNullOrWhiteSpace() )
                {
                    rules = value.FromJsonOrNull<Rock.Field.FieldVisibilityRules>();
                    if ( rules == null )
                    {
                        // if can't be deserialized as FieldVisibilityRules, it might have been serialized as an array from an earlier version
                        var rulesList = value.FromJsonOrNull<List<Field.FieldVisibilityRule>>();
                        if ( rulesList != null )
                        {
                            rules = new Field.FieldVisibilityRules();
                            rules.RuleList.AddRange( rulesList );
                        }
                    }
                }

                this.FieldVisibilityRules = rules ?? new Field.FieldVisibilityRules();
            }
        }

        #endregion Entity Properties

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return RegistrationTemplateFormFieldCache.Get( Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            RegistrationTemplateFormFieldCache.UpdateCachedEntity( Id, entityState );
        }

        #endregion ICacheable

        #region Methods

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( FieldSource == RegistrationFieldSource.PersonField )
            {
                return PersonFieldType.ConvertToString();
            }

            if ( Attribute != null )
            {
                return Attribute.Name;
            }

            return base.ToString();
        }

        #endregion Methods
    }
}
