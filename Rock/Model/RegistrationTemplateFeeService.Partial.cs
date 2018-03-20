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
using System.Collections.Generic;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for Rock.Model.RegistrationTemplateFee
    /// </summary>
    public partial class RegistrationTemplateFeeService
    {
        /// <summary>
        /// Gets the parsed fee option names without cost.
        /// </summary>
        /// <param name="registrationTemplateFeeId">The registration template fee identifier.</param>
        /// <returns></returns>
        public List<string> GetParsedFeeOptionsWithoutCost( int registrationTemplateFeeId )
        {
            RegistrationTemplateFee registrationTemplateFee = this.Get( registrationTemplateFeeId );

            var options = new List<string>();
            string[] nameValues = registrationTemplateFee.CostValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                if ( nameAndValue.Length == 2 )
                {
                    options.Add( nameAndValue[0] );
                }
            }

            return options;
        }

        /// <summary>
        /// Gets the parsed fee options with cost as number.
        /// </summary>
        /// <param name="registrationTemplateFeeId">The registration template fee identifier.</param>
        /// <returns></returns>
        public List<Tuple<string, decimal>> GetParsedFeeOptionsWithCostAsNumber( int registrationTemplateFeeId )
        {
            RegistrationTemplateFee registrationTemplateFee = this.Get( registrationTemplateFeeId );

            var options = new List<Tuple<string, decimal>>();
            string[] nameValues = registrationTemplateFee.CostValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach (string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                if ( nameAndValue.Length == 1)
                {
                    options.Add( Tuple.Create<string, decimal>( nameAndValue[0], 0.00m ) );
                }
                else if ( nameAndValue.Length == 2 )
                {
                    options.Add( Tuple.Create<string, decimal>( nameAndValue[0], nameAndValue[1].AsDecimal() ) );
                }
            }

            return options;
        }

        /// <summary>
        /// Gets the parsed fee options with name and value string. Key is the option name 
        /// and value is the option name with cost formatted as currency in parens "Large","Large ($10.00)"
        /// </summary>
        /// <param name="registrationTemplateFeeId">The registration template fee identifier.</param>
        /// <returns></returns>
        public Dictionary<string, string> GetParsedFeeOptionsWithNameAndValueString( int registrationTemplateFeeId )
        {
            RegistrationTemplateFee registrationTemplateFee = this.Get( registrationTemplateFeeId );

            var options = new Dictionary<string, string>();
            string[] nameValues = registrationTemplateFee.CostValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                if ( nameAndValue.Length == 1 )
                {
                    options.AddOrIgnore( nameAndValue[0], nameAndValue[0] );
                }
                else if ( nameAndValue.Length == 2 )
                {
                    options.AddOrIgnore( nameAndValue[0], string.Format( "{0} ({1})", nameAndValue[0], nameAndValue[1].AsDecimal().FormatAsCurrency() ) );
                }
            }

            return options;
        }

        /// <summary>
        /// Gets the registration template fee report.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <returns></returns>
        public IEnumerable<TemplateFeeReport> GetRegistrationTemplateFeeReport( int registrationInstanceId )
        {
            string query = @"
			SELECT r.Id AS RegistrationId
				, r.CreatedDateTime AS RegistrationDate
				, r.FirstName + ' ' + r.LastName AS RegisteredByName
				, p.FirstName + ' ' + p.LastName AS RegistrantName
				, rr.Id AS RegistrantId
				, tf.[Name] AS FeeName
				, f.[Option] AS [Option]
				, f.Quantity AS Quanity
				, f.Cost AS Cost
				, f.Quantity* f.Cost AS FeeTotal
			FROM RegistrationInstance i
			JOIN Registration r ON i.id = r.RegistrationInstanceId
			join RegistrationRegistrant rr on r.id = rr.RegistrationId
			join RegistrationRegistrantFee f on rr.id = f.RegistrationRegistrantId
			join RegistrationTemplateFee tf on tf.Id = f.RegistrationTemplateFeeId
			join PersonAlias pa on rr.PersonAliasId = pa.Id
			join Person p on pa.PersonId = p.Id
			WHERE i.Id = @RegistrationInstanceId";

            var param = new System.Data.SqlClient.SqlParameter( "@RegistrationInstanceId", registrationInstanceId );

            return Context.Database.SqlQuery<TemplateFeeReport>( query, param );
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
        /// Gets or sets the name of the registrant.
        /// </summary>
        /// <value>
        /// The name of the registrant.
        /// </value>
        public string RegistrantName { get; set; }

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
        /// Gets or sets the option.
        /// </summary>
        /// <value>
        /// The option.
        /// </value>
        public string Option { get; set; }

        /// <summary>
        /// Gets or sets the quanity.
        /// </summary>
        /// <value>
        /// The quanity.
        /// </value>
        public int Quanity { get; set; }

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
        public decimal FeeTotal { get; set; }
    }
}
