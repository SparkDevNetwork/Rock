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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for Rock.Model.RegistrationTemplateFee
    /// </summary>
    public partial class RegistrationTemplateDiscountService
    {
        /// <summary>
        /// Gets the discounts available for registration template used by the provided registration instance.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <returns></returns>
        public IQueryable<RegistrationTemplateDiscount> GetDiscountsForRegistrationInstance( int? registrationInstanceId )
        {
            if ( registrationInstanceId == null || registrationInstanceId == 0 )
            {
                return Queryable();
            }

            var registrationInstanceService = new RegistrationInstanceService( ( RockContext ) this.Context );
            int registrationTemplateId = registrationInstanceService
                .Queryable()
                .Where( r => r.Id == registrationInstanceId )
                .Select( r => r.RegistrationTemplateId )
                .FirstOrDefault();

            return Queryable().Where( d => d.RegistrationTemplateId == registrationTemplateId );
        }

        /// <summary>
        /// Gets the registration instance discount code report.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <returns></returns>
        public IEnumerable<TemplateDiscountReport> GetRegistrationInstanceDiscountCodeReport( int registrationInstanceId )
        {
            var currencySymbol = RockCurrencyCodeInfo.GetCurrencySymbol();

            string query = $@"
                WITH
	            cte([RegistrationId], [RegisteredByName], [RegistrationDate], [RegistrantCount], [DiscountCode], [DiscountAmount], [DiscountPercentage], [DiscountType], [TotalCost], [DiscountQualifiedCost])
	            AS
	            (SELECT [Registration].[Id] AS [RegistrationId]
		            , [Registration].[FirstName] + ' ' + [Registration].[LastName] AS [RegisteredByName]
		            , [Registration].[CreatedDateTime] AS [RegistrationDate]
		            , ( SELECT COUNT(*) FROM [RegistrationRegistrant] WHERE [RegistrationId] = [Registration].[Id]) AS [RegistrantCount]
		            , [Registration].[DiscountCode] AS [DiscountCode]
		            , [Registration].[DiscountAmount] AS [DiscountAmount]
		            , [Registration].[DiscountPercentage] * 100 AS [DiscountPercentage]
		            , CASE WHEN [Registration].[DiscountPercentage] > 0 THEN '%' ELSE '$' END AS [DiscountType]
		            , (( SELECT SUM([Cost]) FROM [RegistrationRegistrant] WHERE [RegistrationId] = [Registration].[id])
			            + (COALESCE(
				            (SELECT SUM([RegistrationRegistrantFee].[Cost])
				            FROM [RegistrationRegistrant]
				            LEFT JOIN [RegistrationRegistrantFee] ON [RegistrationRegistrant].[Id] = [RegistrationRegistrantFee].[RegistrationRegistrantId]
				            WHERE [RegistrationId] = [Registration].[id]), 0)
				            )
			            ) AS [TotalCost]
		            , (( SELECT SUM([Cost]) FROM [RegistrationRegistrant] WHERE [RegistrationId] = [Registration].[Id])
			            + (COALESCE(
				            (SELECT SUM([RegistrationRegistrantFee].[Cost])
				            FROM [RegistrationRegistrant]
				            LEFT JOIN [RegistrationRegistrantFee] ON [RegistrationRegistrant].[Id] = [RegistrationRegistrantFee].[RegistrationRegistrantId]
				            JOIN [RegistrationTemplateFee] ON [RegistrationTemplateFee].[Id] = [RegistrationRegistrantFee].[RegistrationTemplateFeeId]
				            WHERE [RegistrationId] = [Registration].[id] AND [RegistrationTemplateFee].[DiscountApplies] = 1)
				            , 0)
				            )
			            ) AS [DiscountQualifiedCost]
	            FROM [Registration]
	            WHERE [Registration].[RegistrationInstanceId] = @registrationInstanceId)

	            , cte2([RegistrationId], [RegisteredByName], [RegistrationDate], [RegistrantCount], [DiscountCode], [DiscountAmount], [DiscountPercentage], [DiscountType], [TotalCost], [DiscountQualifiedCost], [TotalDiscount])
	            AS
	            (SELECT [RegistrationId], [RegisteredByName], [RegistrationDate], [RegistrantCount], [DiscountCode], [DiscountAmount], [DiscountPercentage], [DiscountType], [TotalCost], [DiscountQualifiedCost]
		            , CASE WHEN [DiscountPercentage] > 0 THEN [DiscountQualifiedCost] * ([DiscountPercentage]/100)
			            ELSE ([RegistrantCount] * [DiscountAmount])
			            END AS [TotalDiscount]
	            FROM cte)

            SELECT [RegistrationId]
                , [RegisteredByName]
                , [RegistrationDate]
                , [RegistrantCount]
                , [DiscountCode]
                , [DiscountAmount]
                , [DiscountPercentage]
	            , CASE WHEN DiscountAmount > 0 THEN '{currencySymbol}' + CAST(DiscountAmount AS varchar) ELSE CAST(DiscountPercentage AS varchar) + '%' END AS [Discount]
	            , [DiscountType]
                , [TotalCost]
                , [DiscountQualifiedCost]
                , [TotalDiscount]
                , ([TotalCost] - [TotalDiscount]) AS [RegistrationCost]
            FROM cte2
            WHERE [TotalDiscount] > 0";

            var param = new System.Data.SqlClient.SqlParameter( "@RegistrationInstanceId", registrationInstanceId );

            return Context.Database.SqlQuery<TemplateDiscountReport>( query, param );
        }

        /// <summary>
        /// Gets the discount by the code if valid. This checks if the code exists, is within it's defined date range, and
        /// has usages remaining.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public RegistrationTemplateDiscountWithUsage GetDiscountByCodeIfValid( int registrationInstanceId, string code )
        {
            if ( code.IsNullOrWhiteSpace() )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var discount = Queryable()
                    .FirstOrDefault( d =>
                        d.RegistrationTemplate.Instances.Any( i => i.Id == registrationInstanceId ) &&
                        d.Code.Equals( code, StringComparison.OrdinalIgnoreCase ) );

                if ( discount == null )
                {
                    // The code is not found
                    return null;
                }

                // Validate the date range. The StartDate and EndDate values are inclusive.
                // Meaning, if the discount starts today or ends today, it is still valid.
                var today = RockDateTime.Today;

                if ( discount.StartDate.HasValue && today < discount.StartDate.Value )
                {
                    // Before the discount starts
                    return null;
                }

                if ( discount.EndDate.HasValue && today > discount.EndDate.Value )
                {
                    // Discount has expired
                    return null;
                }

                int? registrationUsagesRemaining = null;

                if ( discount.MaxUsage.HasValue )
                {
                    // Check the number of people that have already used this code
                    var usageCount = new RegistrationService( Context as RockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Count( r =>
                            r.RegistrationInstanceId == registrationInstanceId &&
                            r.DiscountCode.Equals( code, StringComparison.OrdinalIgnoreCase ) );

                    registrationUsagesRemaining = discount.MaxUsage.Value - usageCount;

                    if ( registrationUsagesRemaining <= 0 )
                    {
                        // Discount has been used up
                        return null;
                    }
                }

                return new RegistrationTemplateDiscountWithUsage
                {
                    RegistrationTemplateDiscount = discount,
                    RegistrationUsagesRemaining = registrationUsagesRemaining
                };
            }
        }
    }

    /// <summary>
    /// The TemplateDiscountReport POCO used by GetRegistrationInstanceDiscountReport
    /// </summary>
    public class TemplateDiscountReport
    {
        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        public int RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the Person who completed the Registration.
        /// </summary>
        /// <value>
        /// The name of the Person who completed the Registration.
        /// </value>
        public string RegisteredByName { get; set; }

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
        /// Gets or sets the number of RegistrationRegistrant for the Registration
        /// </summary>
        /// <value>
        /// The registrant count.
        /// </value>
        public int RegistrantCount { get; set; }

        /// <summary>
        /// Gets or sets the discount code used by the Registration
        /// </summary>
        /// <value>
        /// The discount code.
        /// </value>
        public string DiscountCode { get; set; }

        /// <summary>
        /// Gets or sets the discount amount for discounts in absolute amounts
        /// </summary>
        /// <value>
        /// The discount amount.
        /// </value>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage for discounts in relative amounts
        /// </summary>
        /// <value>
        /// The discount percentage.
        /// </value>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount or percentage, whichever is being used.
        /// </summary>
        /// <value>
        /// The discount.
        /// </value>
        public string Discount { get; set; }

        /// <summary>
        /// Gets or sets the type of the discount ('%' or '$').
        /// </summary>
        /// <value>
        /// '%' or '$'
        /// </value>
        public string DiscountType { get; set; }

        /// <summary>
        /// Gets or sets the total cost of the Registration before any discounts.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the total amount that is eligible for the discount
        /// </summary>
        /// <value>
        /// The discount qualified cost.
        /// </value>
        public decimal DiscountQualifiedCost { get; set; }

        /// <summary>
        /// Gets or sets the total discount amount.
        /// </summary>
        /// <value>
        /// The total discount.
        /// </value>
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Gets or sets the registration cost after discounts are applied.
        /// </summary>
        /// <value>
        /// The registration cost.
        /// </value>
        public decimal RegistrationCost { get; set; }
    }

    /// <summary>
    /// RegistrationTemplateDiscount and the Remaining Usages
    /// </summary>
    public sealed class RegistrationTemplateDiscountWithUsage
    {
        /// <summary>
        /// Gets or sets the registration template discount.
        /// </summary>
        /// <value>
        /// The registration template discount.
        /// </value>
        public RegistrationTemplateDiscount RegistrationTemplateDiscount { get; set; }

        /// <summary>
        /// Gets or sets the remaining number of Registrations that can use the discount. This corresponds to the value in RegistrationTemplateDiscount.MaxUsage - [Number of Registrations Using Discount]
        /// </summary>
        /// <value>
        /// The usages remaining.
        /// </value>
        public int? RegistrationUsagesRemaining { get; set; }
    }
}
