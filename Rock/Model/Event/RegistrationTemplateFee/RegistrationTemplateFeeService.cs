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

using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for Rock.Model.RegistrationTemplateFee
    /// </summary>
    public partial class RegistrationTemplateFeeService
    {
        /// <summary>
        /// Gets the registration template fee report.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <returns></returns>
        public IEnumerable<TemplateFeeReport> GetRegistrationTemplateFeeReport( int registrationInstanceId )
        {
            var qry = new RegistrationRegistrantFeeService( this.Context as RockContext ).Queryable();
            qry = qry.Where( a => a.RegistrationRegistrant.Registration.RegistrationInstanceId == registrationInstanceId );

            var result = qry.Select( a => new TemplateFeeReport
            {
                RegistrationId = a.RegistrationRegistrant.RegistrationId,
                RegistrationDate = a.RegistrationRegistrant.Registration.CreatedDateTime ?? DateTime.MinValue,
                RegisteredByName = a.RegistrationRegistrant.Registration.FirstName + " " + a.RegistrationRegistrant.Registration.LastName,
                RegistrantPerson = a.RegistrationRegistrant.PersonAlias.Person,
                RegistrantId = a.RegistrationRegistrantId,
                FeeName = a.RegistrationTemplateFee.Name,
                FeeItemOption = a.Option,
                FeeItem = a.RegistrationTemplateFeeItem,
                Quantity = a.Quantity,
                Cost = a.Cost
            } ).ToList();

            return result;
        }
    }

    /// <summary>
    /// The TemplateFeeReport POCO used by GetRegistrationTemplateFeeReport()
    /// </summary>
    public class TemplateFeeReport
    {
        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        public int RegistrationId { get; set; }

        /// <summary>
        /// The registration date
        /// </summary>
        private DateTime _registrationDate;

        /// <summary>
        /// Gets or sets the registration date.
        /// </summary>
        /// <value>
        /// The registration date.
        /// </value>
        public DateTime RegistrationDate
        {
            get
            {
                return _registrationDate.Date;
            }

            set
            {
                _registrationDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the registered by.
        /// </summary>
        /// <value>
        /// The name of the registered by.
        /// </value>
        public string RegisteredByName { get; set; }

        /// <summary>
        /// Gets or sets the registrant person.
        /// </summary>
        /// <value>
        /// The registrant person.
        /// </value>
        public Person RegistrantPerson { get; set; }

        /// <summary>
        /// Gets or sets the name of the registrant.
        /// </summary>
        /// <value>
        /// The name of the registrant.
        /// </value>
        public string RegistrantName => RegistrantPerson.FullName;

        /// <summary>
        /// Gets or sets the registrant identifier.
        /// </summary>
        /// <value>
        /// The registrant identifier.
        /// </value>
        public int RegistrantId { get; set; }

        /// <summary>
        /// Gets or sets the name of the fee.
        /// </summary>
        /// <value>
        /// The name of the fee.
        /// </value>
        public string FeeName { get; set; }

        /// <summary>
        /// Gets the name of the fee item.
        /// </summary>
        /// <value>
        /// The name of the fee item.
        /// </value>
        public string FeeItemName
        {
            get
            {
                if (this.FeeItem != null)
                {
                    return this.FeeItem.Name;
                }
                else
                {
                    return this.FeeItemOption;
                }
            }
        }
       
        /// <summary>
        /// Gets or sets the fee item option.
        /// </summary>
        /// <value>
        /// The fee item option.
        /// </value>
        public string FeeItemOption { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the fee total.
        /// </summary>
        /// <value>
        /// The fee total.
        /// </value>
        public decimal FeeTotal => Quantity * Cost;

        /// <summary>
        /// Gets the fee item.
        /// </summary>
        /// <value>
        /// The fee item.
        /// </value>
        public RegistrationTemplateFeeItem FeeItem { get; internal set; }
    }
}
