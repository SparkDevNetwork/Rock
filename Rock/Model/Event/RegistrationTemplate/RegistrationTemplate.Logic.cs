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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Rock.Attribute;

namespace Rock.Model
{
    public partial class RegistrationTemplate
    {
        /// <summary>
        /// Gets or sets the collection of payment plan frequency value IDs from which a registrant can select.
        /// <para>
        /// This is a convenient property for working with the IDs as a collection instead of the <see cref="PaymentPlanFrequencyValueIds"/> property directly.
        /// Updates made to <see cref="PaymentPlanFrequencyValueIds"/> will require getting this property again.
        /// </para>
        /// </summary>
        /// <value>
        /// The collection of payment plan frequency value IDs.
        /// </value>
        [NotMapped]
        public ICollection<int> PaymentPlanFrequencyValueIdsCollection
        {
            get
            {
                var ids = new ObservableCollection<int>( this.PaymentPlanFrequencyValueIds?
                    .Split( ',' )
                    .Select( id => id.Trim().AsIntegerOrNull() )
                    .Where( id => id.HasValue )
                    .Select(id => id.Value) ?? Enumerable.Empty<int>() );

                // Update the underlying string property whenever an ID is added to or removed from the collection.
                ids.CollectionChanged += ( sender, e ) =>
                {
                    this.PaymentPlanFrequencyValueIdsCollection = ids;
                };

                return ids;
            }
            set
            {
                this.PaymentPlanFrequencyValueIds = value == null ? null : string.Join( ",", value );
            }
        }

        /// <summary>
        /// Retrieves the registrant eligibility settings for the current instance or <c>null</c> if not configured.
        /// </summary>
        /// <returns>A <see cref="RegistrantEligibilitySettings"/> object containing the registrant eligibility settings.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public RegistrantEligibilitySettings GetRegistrantEligibilitySettingsOrNull()
        {
            return this.GetAdditionalSettingsOrNull<RegistrantEligibilitySettings>();
        }

        /// <summary>
        /// Sets the Registrant Eligibility Settings for this Registration Template.
        /// </summary>
        /// <param name="registrantEligibilitySettings"></param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public void SetRegistrantEligibilitySettings( RegistrantEligibilitySettings registrantEligibilitySettings )
        {
            var hasEligibilitySettings =
                registrantEligibilitySettings != null
                && (
                    registrantEligibilitySettings.AgeClassification.HasValue
                    || registrantEligibilitySettings.Gender.HasValue
                    || registrantEligibilitySettings.MaximumAge.HasValue
                    || registrantEligibilitySettings.MinimumAge.HasValue
                    || registrantEligibilitySettings.MaximumGradeOffset.HasValue
                    || registrantEligibilitySettings.MinimumAge.HasValue
                    || registrantEligibilitySettings.EligibilityDataViewGuid.HasValue
                );

            if ( !hasEligibilitySettings )
            {
                // Remove the settings if there are no eligibility settings defined. 
                this.RemoveAdditionalSettings<RegistrantEligibilitySettings>();
            }
            else
            {
                // Normalize the ranges so minimums are less than maximums.
                if ( registrantEligibilitySettings.MinimumAge > registrantEligibilitySettings.MaximumAge )
                {
                    (registrantEligibilitySettings.MaximumAge, registrantEligibilitySettings.MinimumAge)
                        = (registrantEligibilitySettings.MinimumAge, registrantEligibilitySettings.MaximumAge);
                }

                if ( registrantEligibilitySettings.MinimumGradeOffset > registrantEligibilitySettings.MaximumGradeOffset )
                {
                    ( registrantEligibilitySettings.MinimumGradeOffset, registrantEligibilitySettings.MaximumGradeOffset )
                        = ( registrantEligibilitySettings.MaximumGradeOffset, registrantEligibilitySettings.MinimumGradeOffset );
                }

                this.SetAdditionalSettings( registrantEligibilitySettings );
            }
        }
    }
}
