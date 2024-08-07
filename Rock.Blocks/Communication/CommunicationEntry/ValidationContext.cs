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
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Gets a validation error message for a validation target value.
    /// </summary>
    /// <typeparam name="TTarget">The type of value to validate.</typeparam>
    internal interface IGetValidationErrorMessage<in TTarget>
    {
        /// <summary>
        /// Gets a validation error message.
        /// </summary>
        /// <param name="invalidTarget">The invalid target value.</param>
        string GetValidationErrorMessage( TTarget invalidTarget );
    }

    /// <summary>
    /// Validates a target value.
    /// </summary>
    /// <typeparam name="TTarget">The type of value to validate.</typeparam>
    internal interface IIsValid<in TTarget>
    {
        /// <summary>
        /// Validates the <paramref name="target"/> value.
        /// </summary>
        /// <param name="target">The target to validate.</param>
        /// <returns><see langword="true"/> if valid; otherwise, returns <see langword="false"/>.</returns>
        bool IsValid( TTarget target );
    }

    /// <summary>
    /// Exposes methods for configuring type validation.
    /// </summary>
    /// <typeparam name="TTarget">
    /// The type of value to validate.
    /// It is covariant to allow for more derived type than specified;
    /// this enables extension methods to be written against a parent type,
    /// such as IEnumerable{T}, and usable by subtypes like List{T}.
    /// </typeparam>
    internal interface IValidationContext<out TTarget>
    {
        /// <summary>
        /// Gets or sets the friendly name for the target being validated.
        /// </summary>
        /// <remarks>
        /// This is used when generating validation error messages.
        /// </remarks>
        string FriendlyName { get; set; }

        /// <summary>
        /// Gets a value determining if an error message builder has been defined.
        /// </summary>
        bool HasErrorMessageBuilder { get; }

        /// <summary>
        /// Sets the validator.
        /// </summary>
        /// <param name="validator">The validator.</param>
        void SetValidator( IIsValid<TTarget> validator );

        /// <summary>
        /// Sets the error message builder.
        /// </summary>
        /// <param name="errorMessageBuilder">The error message builder.</param>
        void SetErrorMessageBuilder( IGetValidationErrorMessage<TTarget> errorMessageBuilder );

        /// <summary>
        /// Executes the validation.
        /// </summary>
        /// <param name="validationResult">Returns the validation result.</param>
        /// <returns><see langword="true"/> if validation is successful; otherwise, returns <see langword="false"/>.</returns>
        bool Validate( out ValidationResult validationResult );
    }

    /// <summary>
    /// Used for configuring and validating values of type <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TTarget">The target type to validate.</typeparam>
    internal class ValidationContext<TTarget> : IValidationContext<TTarget>
    {
        /// <inheritdoc/>
        public string FriendlyName { get; set; }
        
        /// <inheritdoc/>
        public bool HasErrorMessageBuilder => this.ErrorMessageBuilder != null;

        /// <summary>
        /// Gets or sets the targets to validate when validation is executed.
        /// </summary>
        internal IEnumerable<TTarget> Targets { get; set; }

        /// <summary>
        /// Gets or sets the validator used when validation is executed.
        /// </summary>
        private IIsValid<TTarget> Validator { get; set; }

        /// <summary>
        /// Gets or sets the error message builder used when validation is executed and the targets are invalid.
        /// </summary>
        private IGetValidationErrorMessage<TTarget> ErrorMessageBuilder { get; set; }
        
        /// <inheritdoc/>
        public void SetValidator( IIsValid<TTarget> validator )
        {
            this.Validator = validator;
        }
        
        /// <inheritdoc/>
        public void SetErrorMessageBuilder( IGetValidationErrorMessage<TTarget> errorMessageBuilder )
        {
            this.ErrorMessageBuilder = errorMessageBuilder;
        }
        
        /// <inheritdoc/>
        public bool Validate( out ValidationResult validationResult )
        {
            var targets = this.Targets ?? throw new InvalidOperationException( "No validation target has been defined to validate." );

            if ( !targets.Any() )
            {
                // Nothing to validate, so return a successful validation.
                validationResult = ValidationResult.Success;
                return true;
            }

            // Cast so we can access the explicitly defined interfaces.

            foreach ( var target in targets )
            {
                if ( !IsValid( target ) )
                {
                    validationResult = new ValidationResult( GetValidationErrorMessage( target ) );
                    return false;
                }
            }

            // All targets are valid.
            validationResult = ValidationResult.Success;
            return true;
        }

        /// <summary>
        /// Gets the validation error message.
        /// </summary>
        /// <param name="target">The target being validated.</param>
        private string GetValidationErrorMessage( TTarget target )
        {
            if ( this.ErrorMessageBuilder == null )
            {
                return $"{(this.FriendlyName.IsNotNullOrWhiteSpace() ? this.FriendlyName : "Field")} is invalid";
            }
            else
            {
                return this.ErrorMessageBuilder.GetValidationErrorMessage( target );
            }
        }

        /// <summary>
        /// Checks if the target is valid.
        /// </summary>
        /// <param name="target">The target being validated.</param>
        /// <returns><see langword="true"/> if valid; otherwise, returns <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="Validator"/> is not defined.</exception>
        private bool IsValid( TTarget target )
        {
            if ( this.Validator == null )
            {
                throw new InvalidOperationException( "A validator has not been defined." );
            }
            else
            {
                return this.Validator.IsValid( target );
            }
        }
    }
}
