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

namespace Rock.Model
{
    /// <summary>
    /// WorkflowTrigger Logic
    /// </summary>
    public partial class WorkflowTrigger
    {
        #region Virtual Properties

        /// <summary>
        /// Indicates if this WorkflowTrigger is looking for a Value that is Changed From one value To another, or just if the Value Equals something
        /// This is determined by the values for EntityTypeQualifierValue and EntityTypeQualifierValuePrevious. If EntityTypeQualifierValue and EntityTypeQualifierValuePrevious are the same value,
        /// this is a trigger that only fires if the ValueEquals on save. Otherwise, it'll only fire if the previous value was Changed From EntityTypeQualifierValuePrevious To EntityTypeQualifierValue
        /// </summary>
        /// <value>
        /// The type of the value change.
        /// </value>
        public virtual WorkflowTriggerValueChangeType WorkflowTriggerValueChangeType
        {
            get
            {
                if ( this.EntityTypeQualifierValuePrevious == this.EntityTypeQualifierValue )
                {
                    return WorkflowTriggerValueChangeType.ValueEqual;
                }
                else
                {
                    return WorkflowTriggerValueChangeType.ChangeFromTo;
                }
            }
        }

        #endregion Virtual Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // TODO ask David if we can EntityType.ToStringSafe() instead of EntityType.Name below
            return this.WorkflowName ?? EntityType.Name + " " + WorkflowTriggerType.ConvertToString();
        }

        #endregion Public Methods
    }
}
