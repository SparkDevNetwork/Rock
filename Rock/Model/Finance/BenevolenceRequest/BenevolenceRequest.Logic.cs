﻿// <copyright>
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

namespace Rock.Model
{
    public partial class BenevolenceRequest
    {
        /// <summary>
        /// Gets  full name of the person for who the benevolence request is about.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the full name of the person who this benevolence request is about.
        /// </value>
        public virtual string FullName
        {
            get
            {
                return string.Format( "{0} {1}", FirstName, LastName );
            }
        }

        /// <summary>
        /// Gets the full name of the person who this benevolence request is about in Last Name, First Name format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the full name of the person who this benevolence request is about in last name first name format.
        /// </value>
        public virtual string FullNameReversed
        {
            get
            {
                return string.Format( "{0}, {1}", LastName, FirstName );
            }
        }

        /// <summary>
        /// Gets the total amount of benevolence given.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> containing the total amount of benevolence given.
        /// </value>
        [Previewable]
        public virtual decimal TotalAmount
        {
            get
            {
                decimal totalAmount = 0;
                foreach ( BenevolenceResult result in BenevolenceResults )
                {
                    if ( result.Amount != null )
                    {
                        totalAmount += result.Amount.Value;
                    }
                }

                return totalAmount;
            }
        }
    }
}
