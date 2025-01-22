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
using System.ComponentModel.DataAnnotations.Schema;

using Rock.Lava;
using Rock.Security;

namespace Rock.Model
{
    public partial class RegistrationTemplateDiscount
    {
        /// <summary>
        /// Gets the discount string.
        /// </summary>
        /// <value>
        /// The discount string.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual string DiscountString
        {
            get
            {
                if ( DiscountAmount != 0.0m )
                {
                    return DiscountAmount.FormatAsCurrency();
                }
                else if ( DiscountPercentage != 0.0m )
                {
                    return DiscountPercentage.ToString( "P0" );
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// String representation of any discount limits.
        /// </summary>
        /// <value>
        /// The discount limits string.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual string DiscountLimitsString
        {
            get
            {
                var limits = new List<string>();

                if ( MaxUsage.HasValue )
                {
                    limits.Add( string.Format( "Max Usage: {0}", MaxUsage.Value ) );
                }

                if ( MaxRegistrants.HasValue )
                {
                    limits.Add( string.Format( "Max Registrants: {0}", MaxRegistrants.Value ) );
                }

                if ( MinRegistrants.HasValue )
                {
                    limits.Add( string.Format( "Min Registrants: {0}", MinRegistrants.Value ) );
                }

                if ( StartDate.HasValue )
                {
                    limits.Add( string.Format( "Effective: {0}", StartDate.Value.ToShortDateString() ) );
                }

                if ( EndDate.HasValue )
                {
                    limits.Add( string.Format( "Expires: {0}", EndDate.Value.ToShortDateString() ) );
                }

                return limits.AsDelimited( "; " );
            }
        }

        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => RegistrationTemplate ?? base.ParentAuthority;

        #endregion
    }
}
