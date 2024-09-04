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
using System.Linq;
using Rock.Attribute;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Client service with methods for configuring payment plans.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.16.6" )]
    public class PaymentPlanConfigurationService
    {
        private static readonly Dictionary<Guid, IFrequencyHelper> FrequencyHelpers = new Dictionary<Guid, IFrequencyHelper>
        {
            [Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY.AsGuid()] = new BiweeklyFrequencyHelper(),
            [Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_FIRST_AND_FIFTEENTH.AsGuid()] = new TwiceMonthlyFrequencyHelper(),
            [Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY.AsGuid()] = new MonthlyFrequencyHelper(),
            [Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid()] = new OneTimeFrequencyHelper(),
            [Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_QUARTERLY.AsGuid()] = new QuarterlyFrequencyHelper(),
            [Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY.AsGuid()] = new TwiceMonthlyFrequencyHelper(),
            [Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEYEARLY.AsGuid()] = new TwiceYearlyFrequencyHelper(),
            [Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY.AsGuid()] = new WeeklyFrequencyHelper(),
            [Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_YEARLY.AsGuid()] = new YearlyFrequencyHelper(),
        };

        /// <summary>
        /// Gets a frequency helper instance.
        /// </summary>
        /// <param name="frequencyDefinedValueGuid">The unique identifier of the frequency helper.</param>
        /// <returns>A frequency helper instance or <see langword="null"/> if not found.</returns>
        private static IFrequencyHelper GetFrequencyHelper( Guid frequencyDefinedValueGuid )
        {
            return FrequencyHelpers.TryGetValue( frequencyDefinedValueGuid, out var frequencyHelper ) ? frequencyHelper : null;
        }

        /// <summary>
        /// Gets a payment plan configuration.
        /// </summary>
        /// <param name="options">The options for the payment plan configuration.</param>
        /// <returns>A payment plan configuration.</returns>
        public PaymentPlanConfiguration Get( PaymentPlanConfigurationOptions options )
        {
            var paymentPlanConfiguration = new PaymentPlanConfiguration
            {
                MinNumberOfPayments = options.MinNumberOfPayments,
            };

            // Get the planned amount in minor units (cents for USD).
            var minorUnitsFactor = Convert.ToInt32( Math.Pow( 10, options.CurrencyPrecision ) );
            var plannedAmountMinorUnits = Convert.ToInt32( options.AmountForPaymentPlan * minorUnitsFactor );

            paymentPlanConfiguration.StartDate = GetStartDate( options.DesiredStartDate );

            paymentPlanConfiguration.AllowedPaymentFrequencyConfigurations = GetAllowedPaymentFrequencyConfigurations(
                options.DesiredAllowedPaymentFrequencies,
                paymentPlanConfiguration.StartDate,
                options.EndDate,
                paymentPlanConfiguration.MinNumberOfPayments,
                plannedAmountMinorUnits );

            paymentPlanConfiguration.PaymentFrequencyConfiguration = GetPaymentFrequencyConfiguration(
                paymentPlanConfiguration.AllowedPaymentFrequencyConfigurations,
                options.DesiredPaymentFrequency );

            paymentPlanConfiguration.NumberOfPayments = GetNumberOfPayments(
                options.DesiredNumberOfPayments,
                options.IsNumberOfPaymentsLimited,
                paymentPlanConfiguration.MinNumberOfPayments,
                paymentPlanConfiguration.PaymentFrequencyConfiguration?.MaxNumberOfPayments );

            paymentPlanConfiguration.AmountPerPayment = GetAmountPerPayment(
                plannedAmountMinorUnits,
                minorUnitsFactor,
                paymentPlanConfiguration.NumberOfPayments,
                paymentPlanConfiguration.MinNumberOfPayments );

            return paymentPlanConfiguration;
        }

        private static decimal GetAmountPerPayment( int plannedAmountMinorUnits, int minorUnitsFactor, int numberOfPayments, int minNumberOfPayments )
        {
            if ( plannedAmountMinorUnits <= 0 || numberOfPayments <= 0 || numberOfPayments < minNumberOfPayments )
            {
                return 0m;
            }
            else
            {
                return Math.Floor( ( decimal ) plannedAmountMinorUnits / numberOfPayments ) / minorUnitsFactor;
            }
        }

        /// <summary>
        /// Gets the number of payments for a payment plan configuration.
        /// </summary>
        /// <param name="desiredNumberOfPayments">The desired number of payments.</param>
        /// <param name="isNumberOfPaymentsLimited">Determines whether the number of payments should be limited by the minimum and maximum values.</param>
        /// <param name="minNumberOfPayments">The minimum number of payments permitted.</param>
        /// <param name="maxNumberOfPayments">The maximum number of payments permitted.</param>
        /// <returns></returns>
        private static int GetNumberOfPayments( int desiredNumberOfPayments, bool isNumberOfPaymentsLimited, int minNumberOfPayments, int? maxNumberOfPayments )
        {
            if ( isNumberOfPaymentsLimited )
            {
                // Ensure the desired number of payments doesn't exceed the maximum allowed for the selected frequency.
                // If no payment frequency is provided, then the number is capped at 0.
                return Math.Min( Math.Max( desiredNumberOfPayments, minNumberOfPayments ), maxNumberOfPayments ?? 0 );
            }
            else
            {
                return desiredNumberOfPayments;
            }
        }

        /// <summary>
        /// Gets allowed payment frequency configurations for a payment plan configuration.
        /// </summary>
        /// <param name="prospectiveAllowedPaymentFrequencies">The prospective, allowed payment frequencies from which the returned list is derived after applying rules.</param>
        /// <param name="startDate">The start date for the payment plan.</param>
        /// <param name="endDate">The end date for the payment plan.</param>
        /// <param name="minNumberOfPayments">The minimum number of payments permitted.</param>
        /// <param name="plannedAmountMinorUnits">The planned amount in minor units (cents for USD).</param>
        /// <returns>Allowed payment frequency configurations.</returns>
        private static List<PaymentFrequencyConfiguration> GetAllowedPaymentFrequencyConfigurations( IEnumerable<DefinedValueCache> prospectiveAllowedPaymentFrequencies, DateTime startDate, DateTime endDate, int minNumberOfPayments, int plannedAmountMinorUnits )
        {
            var allowedPaymentFrequencyConfigurations = new List<PaymentFrequencyConfiguration>();

            foreach ( var prospectiveAllowedPaymentFrequency in prospectiveAllowedPaymentFrequencies )
            {
                // A prospective, allowed payment frequency is only truly allowed
                // if the number of payments for the frequency (between start and end dates)
                // meets the minimum number of payments requirement.

                var frequencyHelper = GetFrequencyHelper( prospectiveAllowedPaymentFrequency.Guid );

                if ( frequencyHelper == null )
                {
                    // An unknown payment frequency was attempted so skip over it.
                    continue;
                }

                var maxNumberOfPayments = frequencyHelper.GetMaxNumberOfPayments( startDate, endDate, plannedAmountMinorUnits );

                if ( maxNumberOfPayments >= minNumberOfPayments )
                {
                    allowedPaymentFrequencyConfigurations.Add( new PaymentFrequencyConfiguration
                    {
                        MaxNumberOfPayments = maxNumberOfPayments,
                        PaymentFrequency = prospectiveAllowedPaymentFrequency,
                    } );
                }
            }

            return allowedPaymentFrequencyConfigurations;
        }

        /// <summary>
        /// Gets a valid payment frequency configuration for a payment plan configuration.
        /// </summary>
        /// <param name="allowedPaymentFrequencyConfigurations">The allowed payment plan frequency configurations.</param>
        /// <param name="desiredPaymentFrequency">The desired payment frequency.</param>
        /// <returns>A valid payment frequency configuration or <see langword="null"/> if the desired frequency is not allowed.</returns>
        private static PaymentFrequencyConfiguration GetPaymentFrequencyConfiguration( IEnumerable<PaymentFrequencyConfiguration> allowedPaymentFrequencyConfigurations, DefinedValueCache desiredPaymentFrequency )
        {
            return allowedPaymentFrequencyConfigurations.FirstOrDefault( p => p.PaymentFrequency?.Guid == desiredPaymentFrequency?.Guid );
        }

        /// <summary>
        /// Gets a valid start date for a payment plan configuration.
        /// </summary>
        /// <param name="desiredStartDate">The desired start date.</param>
        /// <returns>A valid start date.</returns>
        private static DateTime GetStartDate( DateTime desiredStartDate )
        {
            var tomorrow = RockDateTime.Today.AddDays( 1 );
            return desiredStartDate.Date < tomorrow ? tomorrow : desiredStartDate.Date;
        }

        #region Helper Class Helper Classes

        private interface IFrequencyHelper
        {
            int GetMaxNumberOfPayments( DateTime firstDate, DateTime secondDate, long? inclusiveMax );
        }

        private class BiweeklyFrequencyHelper : IFrequencyHelper
        {
            public int GetMaxNumberOfPayments( DateTime firstDate, DateTime secondDate, long? inclusiveMax )
            {
                firstDate = firstDate.Date;
                secondDate = secondDate.Date;

                var numberOfTransactions = 0;
                var date = firstDate;

                while ( date <= secondDate && ( !inclusiveMax.HasValue || numberOfTransactions < inclusiveMax.Value ) )
                {
                    numberOfTransactions++;
                    date = date.AddDays( 14 );
                }

                return numberOfTransactions;
            }
        }

        private class TwiceMonthlyFrequencyHelper : IFrequencyHelper
        {
            public int GetMaxNumberOfPayments( DateTime firstDate, DateTime secondDate, long? inclusiveMax )
            {
                // For twice monthly frequency, this will check how many 1st and 15th days are between the two dates.

                // Add a day to the second date so this function only has to check
                firstDate = firstDate.Date;
                secondDate = secondDate.Date;

                var date = firstDate;

                if ( date.Day > 15 )
                {
                    // Set the date to the 1st of the next month.
                    date = date.AddDays( 1 - date.Day ).AddMonths( 1 );
                }
                else if ( date.Day < 15 && date.Day > 1 )
                {
                    // Set the date to the 15th of the current month.
                    date = date.AddDays( 15 - date.Day );
                }

                var numberOfTransactions = 0;

                while ( date <= secondDate
                    && ( !inclusiveMax.HasValue || numberOfTransactions < inclusiveMax.Value ) )
                {
                    if ( date.Day == 1 || date.Day == 15 )
                    {
                        numberOfTransactions++;
                    }

                    if ( date.Day < 15 )
                    {
                        // Set the date to the 15th of the current month.
                        date = date.AddDays( 15 - date.Day );
                    }
                    else if ( date.Day == 15 ) // We could use an `else` here but this is more readable.
                    {
                        // Set the date to the 1st of the next month.
                        date = date.AddDays( -14 ).AddMonths( 1 );
                    }
                }

                return numberOfTransactions;
            }
        }

        private class MonthlyFrequencyHelper : IFrequencyHelper
        {
            public int GetMaxNumberOfPayments( DateTime firstDate, DateTime secondDate, long? inclusiveMax )
            {
                firstDate = firstDate.Date;
                secondDate = secondDate.Date;

                // If the first date is the last day of the month
                // then this function will increment by 1 months and
                // automatically choose the last day of the month.
                Func<DateTime, DateTime> getNextDate;
                if ( firstDate == firstDate.EndOfMonth().Date )
                {
                    getNextDate = ( DateTime d ) => d.AddMonths( 1 ).EndOfMonth().Date;
                }
                else
                {
                    getNextDate = ( DateTime d ) => d.AddMonths( 1 );
                }

                var date = firstDate;
                var numberOfTransactions = 0;

                while ( date <= secondDate
                    && ( !inclusiveMax.HasValue || numberOfTransactions < inclusiveMax.Value ) )
                {
                    numberOfTransactions++;
                    date = getNextDate( date );
                }

                return numberOfTransactions;
            }
        }

        private class OneTimeFrequencyHelper : IFrequencyHelper
        {
            public int GetMaxNumberOfPayments( DateTime firstDate, DateTime secondDate, long? inclusiveMax )
            {
                firstDate = firstDate.Date;
                secondDate = secondDate.Date;

                if ( firstDate <= secondDate && ( !inclusiveMax.HasValue || inclusiveMax.Value > 0 ) )
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        private class QuarterlyFrequencyHelper : IFrequencyHelper
        {
            public int GetMaxNumberOfPayments( DateTime firstDate, DateTime secondDate, long? inclusiveMax )
            {
                firstDate = firstDate.Date;
                secondDate = secondDate.Date;

                // If the first date is the last day of the month
                // then this function will increment by 3 months and
                // automatically choose the last day of the month.
                Func<DateTime, DateTime> getNextDate;
                if ( firstDate == firstDate.EndOfMonth().Date )
                {
                    getNextDate = ( DateTime d ) => d.AddMonths( 3 ).EndOfMonth().Date;
                }
                else
                {
                    getNextDate = ( DateTime d ) => d.AddMonths( 3 );
                }

                var date = firstDate;
                var numberOfTransactions = 0;

                while ( date <= secondDate
                    && ( !inclusiveMax.HasValue || numberOfTransactions < inclusiveMax.Value ) )
                {
                    numberOfTransactions++;
                    date = getNextDate( date );
                }

                return numberOfTransactions;
            }
        }

        private class TwiceYearlyFrequencyHelper : IFrequencyHelper
        {
            public int GetMaxNumberOfPayments( DateTime firstDate, DateTime secondDate, long? inclusiveMax )
            {
                firstDate = firstDate.Date;
                secondDate = secondDate.Date;

                // If the first date is the last day of the month
                // then this function will increment by 6 months and
                // automatically choose the last day of the month.
                Func<DateTime, DateTime> getNextDate;
                if ( firstDate == firstDate.EndOfMonth().Date )
                {
                    getNextDate = ( DateTime d ) => d.AddMonths( 6 ).EndOfMonth().Date;
                }
                else
                {
                    getNextDate = ( DateTime d ) => d.AddMonths( 6 );
                }

                var date = firstDate;
                var numberOfTransactions = 0;

                while ( date <= secondDate
                    && ( !inclusiveMax.HasValue || numberOfTransactions < inclusiveMax.Value ) )
                {
                    numberOfTransactions++;
                    date = getNextDate( date );
                }

                return numberOfTransactions;
            }
        }

        private class WeeklyFrequencyHelper : IFrequencyHelper
        {
            public int GetMaxNumberOfPayments( DateTime firstDate, DateTime secondDate, long? inclusiveMax )
            {
                firstDate = firstDate.Date;
                secondDate = secondDate.Date;

                var date = firstDate;
                var numberOfTransactions = 0;

                while ( date <= secondDate
                    && ( !inclusiveMax.HasValue || numberOfTransactions < inclusiveMax.Value ) )
                {
                    numberOfTransactions++;
                    date = date.AddDays( 7 );
                }

                return numberOfTransactions;
            }
        }

        private class YearlyFrequencyHelper : IFrequencyHelper
        {
            public int GetMaxNumberOfPayments( DateTime firstDate, DateTime secondDate, long? inclusiveMax )
            {
                firstDate = firstDate.Date;
                secondDate = secondDate.Date;

                var date = firstDate;
                var numberOfTransactions = 0;

                while ( date <= secondDate
                    && ( !inclusiveMax.HasValue || numberOfTransactions < inclusiveMax.Value ) )
                {
                    numberOfTransactions++;
                    date = date.AddYears( 1 );
                }

                return numberOfTransactions;
            }
        }

        #endregion
    }
}