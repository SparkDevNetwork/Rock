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
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    public partial class Step
    {
        #region Entity Properties
        /// <summary>
        /// Gets the start date key.
        /// </summary>
        /// <value>
        /// The start date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? StartDateKey
        {
            get => ( StartDateTime == null || StartDateTime.Value == default ) ?
                        ( int? ) null :
                        StartDateTime.Value.ToString( "yyyyMMdd" ).AsInteger();

            private set { }
        }

        /// <summary>
        /// Gets the end date key.
        /// </summary>
        /// <value>
        /// The end date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? EndDateKey
        {
            get => ( EndDateTime == null || EndDateTime.Value == default ) ?
                        ( int? ) null :
                        EndDateTime.Value.ToString( "yyyyMMdd" ).AsInteger();

            private set { }
        }

        /// <summary>
        /// Gets the completed date key.
        /// </summary>
        /// <value>
        /// The completed date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? CompletedDateKey
        {
            get => ( CompletedDateTime == null || CompletedDateTime.Value == default ) ?
                        ( int? ) null :
                        CompletedDateTime.Value.ToString( "yyyyMMdd" ).AsInteger();

            private set { }
        }
        #endregion Entity Properties

        /// <summary>
        /// Indicates if this step has been completed
        /// </summary>
        [DataMember]
        public virtual bool IsComplete
        {
            get => StepStatus != null && StepStatus.IsCompleteStatus;
        }

        #region Overrides
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            RockContext rockContext = null;

            // Get PersonAlias.
            var person = this.PersonAlias?.Person;
            if ( person == null && this.PersonAliasId != 0 )
            {
                if ( rockContext == null )
                {
                    rockContext = new RockContext();
                }

                var personAlias = new PersonAliasService( rockContext ).Get( this.PersonAliasId );
                person = personAlias?.Person;
            }

            // Get StepType.
            var stepType = this.StepType;
            if ( stepType == null )
            {
                if ( rockContext == null )
                {
                    rockContext = new RockContext();
                }

                stepType = new StepTypeService( rockContext ).Get( this.StepTypeId );
            }

            if ( person != null && stepType != null )
            {
                if ( stepType.AllowMultiple )
                {
                    // If "AllowMultiple" is set, include the Order to distinguish this from duplicate steps.
                    return $"{stepType.Name} {this.Order} - {person.FullName}";
                }

                return $"{stepType.Name} - {person.FullName}";
            }

            return base.ToString();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                var isValid = base.IsValid;

                if ( StartDateTime.HasValue && EndDateTime.HasValue && StartDateTime.Value > EndDateTime.Value )
                {
                    ValidationResults.Add( new ValidationResult( "The StartDateTime must occur before the EndDateTime" ) );
                    isValid = false;
                }

                if ( StartDateTime.HasValue && CompletedDateTime.HasValue && StartDateTime.Value > CompletedDateTime.Value )
                {
                    ValidationResults.Add( new ValidationResult( "The StartDateTime must occur before the CompletedDateTime" ) );
                    isValid = false;
                }

                return isValid;
            }
        }

        #endregion Overrides
    }
}
