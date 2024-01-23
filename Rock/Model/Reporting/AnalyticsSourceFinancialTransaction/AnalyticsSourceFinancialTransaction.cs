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
using Rock.Utility;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents the source record for an Analytic Fact Financial Transaction in Rock.
    /// Note that this represents a combination of the FinancialTransaction and the FinancialTransactionDetail, 
    /// so if a person contributed to multiple accounts in a transaction, there will be multiple AnalyticSourceFinancialRecords.
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourceFinancialTransaction" )]
    [DataContract]
    [HideFromReporting]
    [Rock.SystemGuid.EntityTypeGuid( "68E1BB08-B30B-49E2-993E-0B5352BB97C5")]
    public class AnalyticsSourceFinancialTransaction : AnalyticsBaseFinancialTransaction<AnalyticsSourceFinancialTransaction>
    {
        // intentionally blank.  See AnalyticsBaseFinancialTransaction.
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsSourceFinancialTransaction Configuration Class
    /// </summary>
    public partial class AnalyticsSourceFinancialTransactionConfiguration : EntityTypeConfiguration<AnalyticsSourceFinancialTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsSourceFinancialTransactionConfiguration"/> class.
        /// </summary>
        public AnalyticsSourceFinancialTransactionConfiguration()
        {
            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier TransactionDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasRequired( t => t.TransactionDate ).WithMany().HasForeignKey( t => t.TransactionDateKey ).WillCascadeOnDelete( false );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for any of these since they are views
            this.HasOptional( t => t.Batch ).WithMany().HasForeignKey( t => t.BatchId ).WillCascadeOnDelete( false );
            this.HasRequired( t => t.Account ).WithMany().HasForeignKey( t => t.AccountId ).WillCascadeOnDelete( false );
        }
    }

    #endregion AnalyticsBaseFinancialTransaction
}