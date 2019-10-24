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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsDimFinancialAccount is a SQL View off of the FinancialAccount table
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsDimFinancialAccount" )]
    [DataContract]
    [HideFromReporting]
    public class AnalyticsDimFinancialAccount : Rock.Data.Entity<AnalyticsDimFinancialAccount>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        [DataMember]
        public int AccountId { get; set; }

        /// <summary>
        /// Gets or sets the (internal) Name of the FinancialAccount. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the (internal) name of the FinancialAccount.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public name of the Financial Account.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the public name of the FinancialAccount.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the FinancialAccount.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined description of the FinancialAccount.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the user defined public description of the FinancialAccount.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined public description of the FinancialAccount.
        /// </value>
        [DataMember]
        public string PublicDescription { get; set; }

        /// <summary>
        /// A string representing the IsTaxable flag of the FinancialAccount.  For example, "Taxable" or "Not Taxable"
        /// </summary>
        [DataMember]
        public string TaxStatus { get; set; }

        /// <summary>
        /// Gets or sets the General Ledger account code for this FinancialAccount.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the General Ledger account code for this FinancialAccount.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string GlCode { get; set; }

        /// <summary>
        /// Gets or sets the sort and display order of the FinancialAccount.  This is an ascending order, so the lower the value the higher the sort priority.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the sort order of the FinancialAccount.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// A string representing the IsActive flag of FinancialAccount: "Active" or "Inactive"
        /// </summary>
        [DataMember]
        public string ActiveStatus { get; set; }

        /// <summary>
        /// A string representing the IsPublic flag of FinancialAccount: "Public" or "Non Public"
        /// </summary>
        [DataMember]
        public string PublicStatus { get; set; }

        /// <summary>
        /// Gets or sets the opening date for this FinancialAccount. This is the first date that transactions can be posted to this account. 
        /// If there isn't a start date for this account, transactions can be posted as soon as the account is created until the <see cref="EndDate"/> (if applicable).
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the first day that transactions can posted to this account. If there is no start date, this property will be null.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the closing/end date for this FinancialAccount. This is the last day that transactions can be posted to this account. If there is not a end date
        /// for this account, transactions can be posted for an indefinite period of time.  Ongoing FinancialAccounts will not have an end date.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the closing/end date for this FinancialAccounts. Transactions can be posted to this account until this date.  If this is 
        /// an ongoing account, this property will be null.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The name of the account type based on the value of FinancialAccount.AccountTypeValueId
        /// </summary>
        /// <value>
        /// The type of the account.
        /// </value>
        [DataMember]
        public string AccountType { get; set; }

        /// <summary>
        /// Gets or sets the image URL
        /// </summary>
        /// <value>
        /// The image URL.
        /// </value>
        [DataMember]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the Image Id that can be used when displaying this Financial Account
        /// </summary>
        /// <value>
        /// The image binary file identifier.
        /// </value>
        [DataMember]
        public int? ImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the URL which could be used to generate a link to a 'More Info' page
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [DataMember]
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the campus short code.
        /// </summary>
        /// <value>
        /// The campus short code.
        /// </value>
        [DataMember]
        public string CampusShortCode { get; set; }

        /// <summary>
        /// Gets or sets the parent account identifier.
        /// </summary>
        /// <value>
        /// The parent account identifier.
        /// </value>
        [DataMember]
        public int? ParentAccountId { get; set; }

        #endregion

        #region Entity Properties specific to Analytics

        /// <summary>
        /// Gets or sets the count.
        /// NOTE: this always has a hardcoded value of 1. It is stored in the table because it is supposed to help do certain types of things in analytics
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [DataMember]
        public int Count { get; set; }

        #endregion
    }
}
