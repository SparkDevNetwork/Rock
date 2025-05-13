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
using System.ComponentModel.DataAnnotations;

using Rock.Security;

namespace Rock.Model
{
    public partial class StepWorkflowTrigger
    {
        #region Overrides

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                var isValid = base.IsValid;

                if ( !StepProgramId.HasValue && !StepTypeId.HasValue )
                {
                    ValidationResults.Add( new ValidationResult( "The StepProgramId or the StepTypeId must be set" ) );
                    isValid = false;
                }

                return isValid;
            }
        }

        #endregion Overrides

        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => StepType ?? StepProgram ?? base.ParentAuthority;

        #endregion
    }
}
