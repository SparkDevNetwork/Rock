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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.ViewModel.Blocks;

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
            return this.RelatedEntities.GetRelatedToSourceEntityQualifier<Group>( registrationInstance.Id, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement, registrationTemplatePlacementId.ToString() );
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
            if ( this.RelatedEntities.RelatedToSourceEntityAlreadyExists( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement, qualifierValue ) )
            {
                this.RelatedEntities.DeleteRelatedToSourceEntity( registrationInstance.Id, group, RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement, qualifierValue );
            }
        }

        /// <summary>
        /// Gets the registration cost summary information.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="registration">The registration.</param>
        /// <returns></returns>
        public List<RegistrationCostSummaryInfo> GetRegistrationCostSummaryInfo( RegistrationContext context, RegistrationEntryBlockArgs registration )
        {
            var registrationInstance = context.RegistrationInstance;
            var registrationTemplate = context.RegistrationTemplate;

            var minimumInitialPaymentPerRegistrant = registrationTemplate.MinimumInitialPayment;
            if ( registrationTemplate.SetCostOnInstance ?? false )
            {
                minimumInitialPaymentPerRegistrant = registrationInstance.MinimumInitialPayment;
            }

            decimal? defaultPaymentAmountPerRegistrant = registrationTemplate.DefaultPayment;
            if ( registrationTemplate.SetCostOnInstance ?? false )
            {
                defaultPaymentAmountPerRegistrant = registrationInstance.DefaultPayment;
            }

            var rockContext = Context as RockContext;
            var registrationService = new RegistrationService( rockContext );

            // Get the cost/fee summary
            var costs = new List<RegistrationCostSummaryInfo>();
            var discountedRegistrantsRemaining = context.Discount?.RegistrationTemplateDiscount?.MaxRegistrants;
            var discountModel = context.Discount?.RegistrationTemplateDiscount;

            foreach ( var registrant in registration.Registrants )
            {
                var discountApplies = discountedRegistrantsRemaining.HasValue && discountedRegistrantsRemaining.Value > 0;

                if ( discountedRegistrantsRemaining.HasValue )
                {
                    discountedRegistrantsRemaining--;
                }

                // Use this to hold the amount of discount remaining if the discount is greater than the registrant cost. The remaining dollars can be applied to eligable fees.
                decimal discountAmountRemaining = 0.0m;

                var firstName = registrationService.GetFirstName( registrationTemplate, registrant );
                var lastName = registrationService.GetLastName( registrationTemplate, registrant );

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
                    costSummary.Cost = ( registrationTemplate.SetCostOnInstance == true ? registrationInstance.Cost : registrationTemplate.Cost ) ?? 0;

                    // Default the DiscountedCost to the same as the actual cost
                    costSummary.DiscountedCost = costSummary.Cost;

                    // Check if a discount should be applied to the registrant and set the DiscountedCost
                    if ( discountApplies )
                    {
                        // Apply the percentage if it exists
                        if ( discountModel.DiscountPercentage > 0.0m )
                        {
                            // If the DiscountPercentage is greater than 100% than set it to 0, otherwise compute the discount and set the DiscountedCost
                            costSummary.DiscountedCost = discountModel.DiscountPercentage >= 1.0m ? 0.0m : costSummary.Cost - ( costSummary.Cost * discountModel.DiscountPercentage );
                        }
                        else if ( discountModel.DiscountAmount > 0 ) // Apply the discount amount
                        {
                            // If the DiscountAmount is greater than the cost then set the DiscountedCost to 0 and store the remaining amount to be applied to eligable fees later.
                            if ( discountModel.DiscountAmount > costSummary.Cost )
                            {
                                discountAmountRemaining = discountModel.DiscountAmount - costSummary.Cost;
                                costSummary.DiscountedCost = 0.0m;
                            }
                            else
                            {
                                // Compute the DiscountedCost using the DiscountAmount
                                costSummary.DiscountedCost = costSummary.Cost - discountModel.DiscountAmount;
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

                    // Get the fee from the template
                    var templateFeeItems = registrationTemplate.Fees.SelectMany( f => f.FeeItems );
                    var templateFeeItem = templateFeeItems.First( f => f.Guid == feeItemGuid );
                    var templateFee = templateFeeItem.RegistrationTemplateFee;

                    decimal cost = templateFeeItem.Cost;
                    string desc = string.Format(
                        "{0}{1} ({2:N0} @ {3})",
                        templateFee.Name,
                        string.IsNullOrWhiteSpace( templateFeeItem.Name ) ? string.Empty : "-" + templateFeeItem.Name,
                        quantity,
                        cost.FormatAsCurrency() );

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
                        if ( discountModel.DiscountPercentage > 0.0m )
                        {
                            feeCostSummary.DiscountedCost = discountModel.DiscountPercentage >= 1.0m ? 0.0m : feeCostSummary.Cost - ( feeCostSummary.Cost * discountModel.DiscountPercentage );
                        }
                        else if ( discountModel.DiscountAmount > 0 && discountAmountRemaining > 0 )
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
        /// Gets the spots available. If null, then there is no cap.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public int? GetSpotsAvailable( RegistrationContext context )
        {
            // Get the number of slots available and if the waitlist is available
            var waitListEnabled = context.RegistrationTemplate.WaitListEnabled;
            var spotsRemaining = context.RegistrationInstance.MaxAttendees;

            if ( spotsRemaining.HasValue )
            {
                var otherRegistrantsCount = new RegistrationRegistrantService( Context as RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Count( a =>
                        a.Registration.RegistrationInstanceId == context.RegistrationInstance.Id &&
                        !a.Registration.IsTemporary );

                spotsRemaining = spotsRemaining.Value - otherRegistrantsCount;
            }

            if ( spotsRemaining < 0 )
            {
                spotsRemaining = 0;
            }

            return spotsRemaining;
        }

        /// <summary>
        /// Gets the registrant cost.
        /// </summary>
        /// <param name="registrationTemplate">The registration template.</param>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <returns></returns>
        public decimal GetBaseRegistrantCost( RegistrationTemplate registrationTemplate, RegistrationInstance registrationInstance )
        {
            var cost = registrationTemplate.SetCostOnInstance == true ?
                        ( registrationInstance.Cost ?? registrationTemplate.Cost ) :
                        registrationTemplate.Cost;

            return cost;
        }

        /// <summary>
        /// Gets the registrant cost.
        /// </summary>
        /// <param name="registrationTemplate">The registration template.</param>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <returns></returns>
        public decimal? GetMinimumInitialPaymentAmount( RegistrationTemplate registrationTemplate, RegistrationInstance registrationInstance )
        {
            var cost = registrationTemplate.SetCostOnInstance == true ?
                        ( registrationInstance.MinimumInitialPayment ?? registrationTemplate.MinimumInitialPayment ) :
                        registrationTemplate.MinimumInitialPayment;

            return cost;
        }
    }
}
