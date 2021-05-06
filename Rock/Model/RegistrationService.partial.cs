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

using Rock.Data;
using Rock.ViewModel.Blocks;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RegistrationService
    {
        /// <summary>
        /// Gets the payments.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <returns></returns>
        public IQueryable<FinancialTransactionDetail> GetPayments( int registrationId )
        {
            int registrationEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
            return new FinancialTransactionDetailService( (RockContext)this.Context )
                .Queryable( "Transaction" )
                .Where( t =>
                    t.EntityTypeId == registrationEntityTypeId &&
                    t.EntityId == registrationId );
        }

        /// <summary>
        /// Gets the total payments.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <returns></returns>
        public decimal GetTotalPayments( int registrationId )
        {
            return GetPayments( registrationId )
                .Select( p => p.Amount ).DefaultIfEmpty()
                .Sum();
        }

        /// <summary>
        /// Validates the arguments.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="errorMessage">The error result.</param>
        /// <returns></returns>
        public RegistrationContext GetRegistrationContext( int registrationInstanceId, out string errorMessage )
        {
            var rockContext = Context as RockContext;
            errorMessage = string.Empty;

            // Load the instance and template
            var registrationInstance = GetActiveRegistrationInstance( registrationInstanceId );
            var registrationTemplate = registrationInstance?.RegistrationTemplate;

            if ( registrationTemplate == null )
            {
                errorMessage = "The registration template or instance was not found";
                return null;
            }

            // Validate that there are enough spots left for this registration
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var context = new RegistrationContext
            {
                RegistrationTemplate = registrationTemplate,
                RegistrationInstance = registrationInstance,
                Registration = null,
                Discount = null,
                SpotsRemaining = null
            };

            var spotsRemaining = registrationInstanceService.GetSpotsAvailable( context );
            context.SpotsRemaining = spotsRemaining;
            return context;
        }

        /// <summary>
        /// Validates the arguments.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="errorMessage">The error result.</param>
        /// <returns></returns>
        public RegistrationContext GetRegistrationContext( int registrationInstanceId, Person currentPerson, RegistrationEntryBlockArgs args, out string errorMessage )
        {
            var rockContext = Context as RockContext;
            var context = GetRegistrationContext( registrationInstanceId, out errorMessage );

            if (!errorMessage.IsNullOrWhiteSpace())
            {
                return null;
            }

            // Basic check on the args to see that they appear valid
            if ( args == null )
            {
                errorMessage = "The args cannot be null";
                return null;
            }

            if ( args.Registrants?.Any() != true )
            {
                errorMessage = "At least one registrant is required";
                return null;
            }

            if ( args.Registrar == null )
            {
                errorMessage = "A registrar is required";
                return null;
            }

            // Look up and validate the discount by the code
            var registrationTemplateDiscountService = new RegistrationTemplateDiscountService( rockContext );
            var discount = registrationTemplateDiscountService.GetDiscountByCodeIfValid( registrationInstanceId, args.DiscountCode );

            if ( !args.DiscountCode.IsNullOrWhiteSpace() && discount == null )
            {
                errorMessage = "The discount code is not valid";
                return null;
            }

            // Get or create the registration
            var registrationService = new RegistrationService( rockContext );
            var registration = args.RegistrationGuid.HasValue ? registrationService.Get( args.RegistrationGuid.Value ) : null;

            if ( registration == null && args.RegistrationGuid.HasValue )
            {
                errorMessage = "The registration was not found";
                return null;
            }

            // Validate the amount to pay today
            var isNewRegistration = registration == null;
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var cost = registrationInstanceService.GetBaseRegistrantCost( context.RegistrationTemplate, context.RegistrationInstance );

            // Cannot pay less than 0
            if ( args.AmountToPayNow < 0 )
            {
                args.AmountToPayNow = 0;
            }

            // Cannot pay more than is owed
            if ( args.AmountToPayNow > cost )
            {
                args.AmountToPayNow = cost;
            }

            // Validate the charge amount is not too low according to the initial payment amount
            if ( isNewRegistration && cost > 0 )
            {
                var minimumInitialPayment = registrationInstanceService.GetMinimumInitialPaymentAmount( context.RegistrationTemplate, context.RegistrationInstance ) ?? cost;

                if ( args.AmountToPayNow < minimumInitialPayment )
                {
                    args.AmountToPayNow = minimumInitialPayment;
                }
            }

            if ( !isNewRegistration )
            {
                if ( registration.PersonAliasId.HasValue && registration.PersonAliasId.Value != currentPerson?.PrimaryAliasId )
                {
                    // This existing registration does not belong to this person
                    errorMessage = "Your existing registration was not found";
                }
                else if ( registration.RegistrationInstanceId != registrationInstanceId )
                {
                    // This existing registration is not for this instance
                    errorMessage = "Your existing registration was not found";
                }
            }

            context.Discount = discount;
            context.Registration = registration;
            return context;
        }

        /// <summary>
        /// Gets the active registration instance.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <returns></returns>
        private RegistrationInstance GetActiveRegistrationInstance( int registrationInstanceId )
        {
            var now = RockDateTime.Now;
            var registrationInstanceService = new RegistrationInstanceService( Context as RockContext );
            var registrationInstance = registrationInstanceService.Get( registrationInstanceId );
            var registrationTemplate = registrationInstance?.RegistrationTemplate;

            // Ensure that the registration entities are active
            if ( registrationInstance is null || registrationTemplate is null || !registrationTemplate.IsActive || !registrationInstance.IsActive )
            {
                return null;
            }

            // Make sure the registration is open
            var isBeforeRegistrationOpens = registrationInstance.StartDateTime.HasValue && registrationInstance.StartDateTime > now;
            var isAfterRegistrationCloses = registrationInstance.EndDateTime.HasValue && registrationInstance.EndDateTime < now;

            if ( isBeforeRegistrationOpens || isAfterRegistrationCloses )
            {
                return null;
            }

            return registrationInstance;
        }

        /// <summary>
        /// Gets the first name.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <returns></returns>
        public string GetFirstName( RegistrationTemplate template, Rock.ViewModel.Blocks.RegistrantInfo registrantInfo )
        {
            object value = GetPersonFieldValue( template, registrantInfo, RegistrationPersonFieldType.FirstName );

            if ( value == null )
            {
                // if FirstName isn't prompted for in a registration form, and using an existing Person, get the person's FirstName/NickName from the database
                if ( registrantInfo.PersonGuid.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( registrantInfo.PersonGuid.Value, s => s.NickName ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <returns></returns>
        public string GetLastName( RegistrationTemplate template, Rock.ViewModel.Blocks.RegistrantInfo registrantInfo )
        {
            object value = GetPersonFieldValue( template, registrantInfo, RegistrationPersonFieldType.LastName );

            if ( value == null )
            {
                // if LastName isn't prompted for in a registration form, and using an existing Person, get the person's lastname from the database
                if ( registrantInfo.PersonGuid.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( registrantInfo.PersonGuid.Value, s => s.LastName ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <returns></returns>
        public string GetEmail( RegistrationTemplate template, Rock.ViewModel.Blocks.RegistrantInfo registrantInfo )
        {
            object value = GetPersonFieldValue( template, registrantInfo, RegistrationPersonFieldType.Email );

            if ( value == null )
            {
                // if Email isn't prompted for in a registration form, and using an existing Person, get the person's email from the database
                if ( registrantInfo.PersonGuid.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( registrantInfo.PersonGuid.Value, s => s.Email ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a person field value.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <param name="personFieldType">Type of the person field.</param>
        /// <returns></returns>
        public object GetPersonFieldValue( RegistrationTemplate template, Rock.ViewModel.Blocks.RegistrantInfo registrantInfo, RegistrationPersonFieldType personFieldType )
        {
            if ( template != null && template.Forms != null )
            {
                var fieldGuid = template.Forms
                    .SelectMany( t => t.Fields
                        .Where( f =>
                            f.FieldSource == RegistrationFieldSource.PersonField &&
                            f.PersonFieldType == personFieldType )
                        .Select( f => f.Guid ) )
                    .FirstOrDefault();

                return registrantInfo.FieldValues.GetValueOrNull( fieldGuid );
            }

            return null;
        }
    }

    /// <summary>
    /// Context
    /// </summary>
    public sealed class RegistrationContext
    {
        /// <summary>
        /// Gets or sets the registration template.
        /// </summary>
        /// <value>
        /// The registration template.
        /// </value>
        public RegistrationTemplate RegistrationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the registration instance.
        /// </summary>
        /// <value>
        /// The registration instance.
        /// </value>
        public RegistrationInstance RegistrationInstance { get; set; }

        /// <summary>
        /// Gets or sets the registration.
        /// </summary>
        /// <value>
        /// The registration.
        /// </value>
        public Registration Registration { get; set; }

        /// <summary>
        /// Gets or sets the discount.
        /// </summary>
        /// <value>
        /// The discount.
        /// </value>
        public RegistrationTemplateDiscountWithUsage Discount { get; set; }

        /// <summary>
        /// Gets or sets the spots remaining.
        /// </summary>
        /// <value>
        /// The spots remaining.
        /// </value>
        public int? SpotsRemaining { get; set; }
    }
}