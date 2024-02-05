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

using Rock.ViewModels.Blocks.Event.RegistrationEntry;

namespace Rock.Model
{
    /// <summary>
    ///
    /// </summary>
    public partial class RegistrationInstanceService
    {
        /// <summary>
        /// Gets the registration instance placement groups.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <returns></returns>
        public IQueryable<Group> GetRegistrationInstancePlacementGroups( RegistrationInstance registrationInstance )
        {
            return this.RelatedEntities.GetRelatedToSourceEntity<Group>( registrationInstance.Id, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement );
        }

        /// <summary>
        /// Gets the registration instance placement groups by placement.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="registrationTemplatePlacementId">The registration template placement identifier.</param>
        /// <returns></returns>
        public IQueryable<Group> GetRegistrationInstancePlacementGroupsByPlacement( RegistrationInstance registrationInstance, int registrationTemplatePlacementId )
        {
            return GetRegistrationInstancePlacementGroupsByPlacement( registrationInstance.Id, registrationTemplatePlacementId );
        }

        /// <summary>
        /// Gets the registration instance placement groups by placement.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="registrationTemplatePlacementId">The registration template placement identifier.</param>
        /// <returns></returns>
        public IQueryable<Group> GetRegistrationInstancePlacementGroupsByPlacement( int registrationInstanceId, int registrationTemplatePlacementId )
        {
            return this.RelatedEntities.GetRelatedToSourceEntityQualifier<Group>( registrationInstanceId, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement, registrationTemplatePlacementId.ToString() );
        }

        /// <summary>
        /// Sets the registration instance placement groups.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="groups">The groups.</param>
        public void SetRegistrationInstancePlacementGroups( RegistrationInstance registrationInstance, List<Group> groups )
        {
            this.RelatedEntities.SetRelatedToSourceEntity( registrationInstance.Id, groups, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement );
        }

        /// <summary>
        /// Sets the registration instance placement groups.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="groups">The groups.</param>
        /// <param name="registrationTemplatePlacementId">The registration template placement identifier.</param>
        public void SetRegistrationInstancePlacementGroups( RegistrationInstance registrationInstance, List<Group> groups, int registrationTemplatePlacementId )
        {
            this.RelatedEntities.SetRelatedToSourceEntity( registrationInstance.Id, groups, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement, registrationTemplatePlacementId.ToString() );
        }

        /// <summary>
        /// Adds the registration instance placement group. Returns false if the group is already a placement group for this instance.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public bool AddRegistrationInstancePlacementGroup( RegistrationInstance registrationInstance, Group group )
        {
            if ( !this.RelatedEntities.RelatedToSourceEntityAlreadyExists( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement ) )
            {
                this.RelatedEntities.AddRelatedToSourceEntity( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds the registration instance placement group. Returns false if the group is already a placement group for this instance.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="group">The group.</param>
        /// <param name="registrationTemplatePlacementId">The registration template placement identifier.</param>
        /// <returns></returns>
        public bool AddRegistrationInstancePlacementGroup( RegistrationInstance registrationInstance, Group group, int registrationTemplatePlacementId )
        {
            string qualifierValue = registrationTemplatePlacementId.ToString();
            if ( !this.RelatedEntities.RelatedToSourceEntityAlreadyExists( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement, qualifierValue ) )
            {
                this.RelatedEntities.AddRelatedToSourceEntity( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement, registrationTemplatePlacementId.ToString() );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes (detaches) the registration instance placement group.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="group">The group.</param>
        public void DeleteRegistrationInstancePlacementGroup( RegistrationInstance registrationInstance, Group group )
        {
            if ( this.RelatedEntities.RelatedToSourceEntityAlreadyExists( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement ) )
            {
                this.RelatedEntities.DeleteRelatedToSourceEntity( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement );
            }
        }

        /// <summary>
        /// Deletes (detaches) the registration instance placement group for the given registration template placement ID.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="group">The group.</param>
        /// <param name="registrationTemplatePlacementId">The registration template placement identifier.</param>
        public void DeleteRegistrationInstancePlacementGroup( RegistrationInstance registrationInstance, Group group, int registrationTemplatePlacementId )
        {
            string qualifierValue = registrationTemplatePlacementId.ToString();
            this.RelatedEntities.DeleteTargetEntityFromSourceEntity( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement, qualifierValue );
        }

        /// <summary>
        /// Gets the registration cost summary information.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="registration">The registration.</param>
        /// <returns></returns>
        // TODO JMH Do we need to keep a RockObsolete overload with the previously named RegistrationEntryBlockArgs view model class?
        public List<RegistrationCostSummaryInfo> GetRegistrationCostSummaryInfo( RegistrationContext context, RegistrationEntryArgsBag registration )
        {
            var minimumInitialPaymentPerRegistrant = context.RegistrationSettings.PerRegistrantMinInitialPayment;
            var defaultPaymentAmountPerRegistrant = context.RegistrationSettings.PerRegistrantDefaultInitialPayment;

            var rockContext = Context as RockContext;
            var registrationService = new RegistrationService( rockContext );

            // Get the cost/fee summary
            var costs = new List<RegistrationCostSummaryInfo>();
            var discountedRegistrantsRemaining = context.Discount?.RegistrationTemplateDiscount?.MaxRegistrants;
            var discountModel = context.Discount?.RegistrationTemplateDiscount;

            // When we first submit the initial registration, context.Registration
            // is null so it can never have a value. When returning to an existing
            // registration, the context.Registration object will exist and may
            // have an admin-applied discount entered. BUT, the person returning
            // to this registration might also enter a discount code they got after
            // registering.
            //
            // So, First we take the discount from the code. If that isn't available
            // then we try to get the discount already applied to the registration.
            // Finally fall back to 0 - no discount.
            //
            // See issue #5691 for more details.
            var discountPercentage = discountModel?.DiscountPercentage ?? context.Registration?.DiscountPercentage ?? 0.0M;
            var discountAmount = discountModel?.DiscountAmount ?? context.Registration?.DiscountAmount ?? 0.0M;

            foreach ( var registrant in registration.Registrants )
            {
                var discountApplies = ( discountAmount > 0.0M || discountPercentage > 0.0M ) && ( !discountedRegistrantsRemaining.HasValue || discountedRegistrantsRemaining.Value > 0 );

                if ( discountedRegistrantsRemaining.HasValue )
                {
                    discountedRegistrantsRemaining--;
                }

                // Use this to hold the amount of discount remaining if the discount is greater than the registrant cost. The remaining dollars can be applied to eligable fees.
                decimal discountAmountRemaining = 0.0m;

                var firstName = registrationService.GetFirstName( context.RegistrationSettings, registrant );
                var lastName = registrationService.GetLastName( context.RegistrationSettings, registrant );

                // The registrant name for the payment summary grid
                var costSummary = new RegistrationCostSummaryInfo
                {
                    Type = RegistrationCostSummaryType.Cost,
                    Description = string.Format( "{0} {1}", firstName, lastName )
                };

                // If the registrant is on the waitlist then set costs to 0 and add a waitlist indicator to the name for the payment summary grid
                if ( registrant.IsOnWaitList )
                {
                    costSummary.Description += " (Waiting List)";
                    costSummary.Cost = 0.0m;
                    costSummary.DiscountedCost = 0.0m;
                    costSummary.MinPayment = 0.0m;
                    costSummary.DefaultPayment = 0.0m;
                }
                else
                {
                    // Add the registrant cost to the cost summary
                    costSummary.Cost = registration.RegistrationGuid == null ? context.RegistrationSettings.PerRegistrantCost : registrant.Cost;

                    // Default the DiscountedCost to the same as the actual cost
                    costSummary.DiscountedCost = costSummary.Cost;

                    // Check if a discount should be applied to the registrant and set the DiscountedCost
                    if ( discountApplies )
                    {
                        // Apply the percentage if it exists
                        if ( discountPercentage > 0.0m )
                        {
                            // If the DiscountPercentage is greater than 100% than set it to 0, otherwise compute the discount and set the DiscountedCost
                            costSummary.DiscountedCost = discountPercentage >= 1.0m ? 0.0m : costSummary.Cost - ( costSummary.Cost * discountPercentage );
                        }
                        else if ( discountAmount > 0 )
                        {
                            // Apply the discount amount
                            // If the DiscountAmount is greater than the cost then set the DiscountedCost to 0 and store the remaining amount to be applied to eligable fees later.
                            if ( discountAmount > costSummary.Cost )
                            {
                                discountAmountRemaining = discountAmount - costSummary.Cost;
                                costSummary.DiscountedCost = 0.0m;
                            }
                            else
                            {
                                // Compute the DiscountedCost using the DiscountAmount
                                costSummary.DiscountedCost = costSummary.Cost - discountAmount;
                            }
                        }
                    }

                    // If registration allows a minimum payment calculate that amount, otherwise use the discounted amount as minimum
                    costSummary.MinPayment = minimumInitialPaymentPerRegistrant.HasValue ? minimumInitialPaymentPerRegistrant.Value : costSummary.DiscountedCost;
                    costSummary.DefaultPayment = defaultPaymentAmountPerRegistrant;
                }

                // Add the cost to the list
                costs.Add( costSummary );

                foreach ( var kvp in registrant.FeeItemQuantities )
                {
                    var feeItemGuid = kvp.Key;
                    var quantity = kvp.Value;

                    if ( quantity < 1 )
                    {
                        // Don't include a line item for things the user didn't choose
                        continue;
                    }

                    // Get the fee from the template
                    var templateFeeItems = context.RegistrationSettings.Fees.SelectMany( f => f.FeeItems );
                    var templateFeeItem = templateFeeItems.First( f => f.Guid == feeItemGuid );
                    var templateFee = templateFeeItem.RegistrationTemplateFee;

                    decimal cost = templateFeeItem.Cost;
                    var desc = GetFeeLineItemDescription( templateFee, templateFeeItem, quantity );

                    var feeCostSummary = new RegistrationCostSummaryInfo
                    {
                        Type = RegistrationCostSummaryType.Fee,
                        Description = desc,
                        Cost = quantity * cost,

                        // Default the DiscountedCost to be the same as the Cost
                        DiscountedCost = quantity * cost
                    };

                    if ( templateFee != null && templateFee.DiscountApplies && discountApplies )
                    {
                        if ( discountPercentage > 0.0m )
                        {
                            feeCostSummary.DiscountedCost = discountPercentage >= 1.0m ? 0.0m : feeCostSummary.Cost - ( feeCostSummary.Cost * discountPercentage );
                        }
                        else if ( discountAmount > 0 && discountAmountRemaining > 0 )
                        {
                            // If there is any discount amount remaining after subracting it from the cost then it can be applied here
                            // If the DiscountAmount is greater than the cost then set the DiscountedCost to 0 and store the remaining amount to be applied to eligable fees later.
                            if ( discountAmountRemaining > feeCostSummary.Cost )
                            {
                                discountAmountRemaining -= feeCostSummary.DiscountedCost;
                                feeCostSummary.DiscountedCost = 0.0m;
                            }
                            else
                            {
                                // Compute the DiscountedCost using the DiscountAmountRemaining
                                feeCostSummary.DiscountedCost = feeCostSummary.Cost - discountAmountRemaining;
                                discountAmountRemaining = 0.0m;
                            }
                        }
                    }

                    // If template allows a minimum payment, then fees are not included, otherwise it is included
                    feeCostSummary.MinPayment = minimumInitialPaymentPerRegistrant.HasValue ? 0 : feeCostSummary.DiscountedCost;

                    // Add the fee cost to the list
                    costs.Add( feeCostSummary );
                }
            }

            return costs;
        }

        /// <summary>
        /// Gets the fee line item description.
        /// </summary>
        /// <param name="fee">The fee.</param>
        /// <param name="item">The item.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns></returns>
        private static string GetFeeLineItemDescription( RegistrationTemplateFee fee, RegistrationTemplateFeeItem item, int quantity )
        {
            var useFeeNameOnly = item.Name.IsNullOrWhiteSpace() || ( fee.FeeType == RegistrationFeeType.Single && fee.Name == item.Name );
            var name = useFeeNameOnly ? fee.Name : $"{fee.Name} - {item.Name}";
            var formattedCost = item.Cost.FormatAsCurrency().Trim();
            var costDesc = fee.AllowMultiple ? $"{quantity} @ {formattedCost}" : formattedCost;
            return $"{name} ({costDesc})";
        }

        /// <summary>
        /// Gets the spots available. If null, then there is no cap.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public int? GetSpotsAvailable( RegistrationContext context )
        {
            // Get the number of slots available
            var spotsRemaining = context.RegistrationSettings.MaxAttendees;

            if ( !spotsRemaining.HasValue )
            {
                // Unlimited capacity
                return null;
            }

            if ( spotsRemaining.HasValue )
            {
                // Check the count of people already registered
                var otherRegistrantsCount = new RegistrationRegistrantService( Context as RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Count( a =>
                        a.Registration.RegistrationInstanceId == context.RegistrationSettings.RegistrationInstanceId &&
                        !a.Registration.IsTemporary );

                spotsRemaining -= otherRegistrantsCount;

                if ( spotsRemaining > 0 && context.RegistrationSettings.IsTimeoutEnabled )
                {
                    // Check the number of people that are in the process of registering right now
                    var sessionRegistrantCount = new RegistrationSessionService( Context as RockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( s => s.RegistrationInstanceId == context.RegistrationSettings.RegistrationInstanceId
                            && s.ExpirationDateTime > RockDateTime.Now )
                        .Select( s => s.RegistrationCount )
                        .DefaultIfEmpty( 0 )
                        .Sum();

                    spotsRemaining -= sessionRegistrantCount;
                }
            }

            if ( spotsRemaining < 0 )
            {
                spotsRemaining = 0;
            }

            return spotsRemaining;
        }

        /// <summary>
        /// Gets the fee item count remaining.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// A dictionary of fee item unique identifier keys with the number of
        /// items remaining as the value. The value will be <c>null</c> if there
        /// is no configured limit.
        /// </returns>
        public Dictionary<Guid, int?> GetFeeItemCountRemaining( RegistrationContext context )
        {
            var feeItems = context.RegistrationSettings.Fees.SelectMany( f => f.FeeItems );
            var map = feeItems.ToDictionary( fi => fi.Guid, fi => fi.MaximumUsageCount );

            var service = new RegistrationRegistrantFeeService( Context as RockContext );
            var quantitiesUsed = service.Queryable()
                .AsNoTracking()
                .Where( rf =>
                     rf.RegistrationRegistrant.Registration.RegistrationInstanceId == context.RegistrationSettings.RegistrationInstanceId &&
                     rf.RegistrationTemplateFeeItem.MaximumUsageCount.HasValue )
                .Select( f => new
                {
                    f.Quantity,
                    f.RegistrationTemplateFeeItem.Guid
                } )
                .ToList();

            foreach ( var quantityUsed in quantitiesUsed )
            {
                var qtyRemaining = Math.Max( 0, ( map.GetValueOrNull( quantityUsed.Guid ) ?? 0 ) - quantityUsed.Quantity );
                map[quantityUsed.Guid] = qtyRemaining;
            }

            return map;
        }
    }
}
