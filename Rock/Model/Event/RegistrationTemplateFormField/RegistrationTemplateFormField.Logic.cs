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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RegistrationTemplateFormField
    {
        /// <summary>
        /// Gets or sets the field visibility rules.
        /// </summary>
        /// <value>
        /// The field visibility rules.
        /// </value>
        [NotMapped]
        public virtual Rock.Field.FieldVisibilityRules FieldVisibilityRules { get; set; } = new Rock.Field.FieldVisibilityRules();

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return RegistrationTemplateFormFieldCache.Get( Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            RegistrationTemplateFormFieldCache.UpdateCachedEntity( Id, entityState );
        }

        #endregion ICacheable

        #region Methods

        /// <summary>
        /// Adds a note identifying this field to the provided <paramref name="missingFieldsByFormId"/> collection if this field is
        /// required, non-conditional and missing a value.
        /// </summary>
        /// <param name="missingFieldsByFormId">The collection of field details to which to add a note.</param>
        /// <param name="fieldValue">The value - if any - that was provided for this field.</param>
        [RockInternal( "1.15.2" )]
        public void NoteFieldDetailsIfRequiredAndMissing( Dictionary<int, Dictionary<int, string>> missingFieldsByFormId, object fieldValue )
        {
            /*
                8/15/2023 - JPH

                Several individuals have reported seeing missing registrant data within completed registrations. This helper method was added
                to make it easier to take note of any required, non-conditional Field values that should have been enforced by the UI.

                Note that this method will NOT take into consideration any Fields that have visibility rules, and are therefore conditional.
                To do so would require the aggregation and processing of more data. The goal - instead - is to quickly spot if there are
                scenarios in which the always-required fields are somehow not being passed back to the server for saving, so we know whether
                to look into the issue further from this angle.

                Reason: Registration entries are sometimes missing registration form data.
                https://github.com/SparkDevNetwork/Rock/issues/5091
             */
            if ( missingFieldsByFormId == null
                || !this.IsRequired                                     // Field is not required.
                || this.Id <= 0                                         // No ID to report; not helpful.
                || this.RegistrationTemplateFormId <= 0                 // No form ID to report; not helpful.
                || this.FieldVisibilityRules?.RuleList?.Any() == true   // This field is conditional; not enough info to determine if it's currently required.
                || fieldValue.ToStringSafe().IsNotNullOrWhiteSpace()    // Field has a value (of some kind).
            )
            {
                return;
            }

            // Find or add the parent form's collection.
            missingFieldsByFormId.TryGetValue( this.RegistrationTemplateFormId, out Dictionary<int, string> formFields );
            if ( formFields == null )
            {
                formFields = new Dictionary<int, string>();
                missingFieldsByFormId.AddOrReplace( this.RegistrationTemplateFormId, formFields );
            }

            // Get the field details based on field source.
            var detailsSb = new StringBuilder( $"{this.FieldSource.ConvertToString()}: " );

            if ( this.FieldSource == RegistrationFieldSource.PersonField )
            {
                detailsSb.Append( this.PersonFieldType.ConvertToString() );
            }
            else
            {
                if ( this.Attribute == null )
                {
                    detailsSb.Append( "Error - Attribute property is not defined" );
                }
                else
                {
                    detailsSb.Append( this.Attribute.Name );
                }
            }

            // Add or replace this field's value.
            formFields.AddOrReplace( this.Id, detailsSb.ToString() );
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( FieldSource == RegistrationFieldSource.PersonField )
            {
                return PersonFieldType.ConvertToString();
            }

            if ( Attribute != null )
            {
                return Attribute.Name;
            }

            return base.ToString();
        }

        #endregion Methods
    }
}
