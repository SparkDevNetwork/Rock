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
using System.Runtime.Serialization;

using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Streak
    {
        /// <summary>
        /// Gets or sets the IsActive.
        /// </summary>
        [DataMember]
        public virtual bool IsActive
        {
            get => !InactiveDateTime.HasValue;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                var isValid = base.IsValid;
                var streakTypeCache = StreakTypeCache.Get( StreakTypeId );

                if ( streakTypeCache != null && EnrollmentDate < streakTypeCache.StartDate )
                {
                    ValidationResults.Add( new ValidationResult( $"The enrollment date cannot be before the streak type start date, {streakTypeCache.StartDate.ToShortDateString()}." ) );
                    isValid = false;
                }

                return isValid;
            }
        }

        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => StreakType ?? base.ParentAuthority;

        #endregion
    }
}