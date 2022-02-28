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
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class MergeTemplate
    {
        #region ICategorized

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        int? ICategorized.CategoryId
        {
            get
            {
                return this.CategoryId;
            }
        }

        #endregion

        #region ISecurity

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// Over-riding this to ensure that a person can see only their own personal templates
        /// regardless of security settings.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        /// <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( action == Security.Authorization.VIEW && PersonAlias != null )
            {
                if ( PersonAlias.PersonId == person.Id )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return base.IsAuthorized( action, person );
        }
        #endregion

        #region Private Methods

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the MergeTemplateType MEF Component for the specified MergeTemplate
        /// </summary>
        /// <returns></returns>
        public MergeTemplateType GetMergeTemplateType()
        {
            var mergeTemplateTypeEntityType = EntityTypeCache.Get( this.MergeTemplateTypeEntityTypeId );
            if ( mergeTemplateTypeEntityType == null )
            {
                return null;
            }

            return MergeTemplateTypeContainer.GetComponent( mergeTemplateTypeEntityType.Name );
        }

        #endregion
    }
}
