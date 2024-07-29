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
    /// Extension methods to validate any type of target value.
    /// </summary>
    internal static class ValidationContextExtensions
    {
        /// <summary>
        /// Validates the <paramref name="target"/> with the <paramref name="validator"/>.
        /// </summary>
        /// <typeparam name="TTarget">The validation target type.</typeparam>
        /// <param name="target">The target to validation.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="errorMessageBuilder">The delegate used to generate an error message if validation fails.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        private static bool Validate<TTarget>( this TTarget target, Func<TTarget, bool> validator, Func<TTarget, string> errorMessageBuilder, out ValidationResult validationResult )
        {
            if ( validator( target ) )
            {
                validationResult = ValidationResult.Success;
                return true;
            }
            else
            {
                validationResult = new ValidationResult( errorMessageBuilder( target ) );
                return false;
            }
        }

        /// <summary>
        /// Validates that the target value is <see langword="true"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsTrue( this ValidationContext<bool> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( o => $"{o.FriendlyName} must be true." )
                .Validate( o => o.Target, validationContext.ErrorMessageBuilder, out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is <see langword="false"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsFalse( this ValidationContext<bool> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( o => $"{o.FriendlyName} must be false." )
                .Validate( o => !o.Target, validationContext.ErrorMessageBuilder, out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is not <see langword="null"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotNull<TTarget>( this ValidationContext<TTarget> validationContext, out ValidationResult validationResult ) where TTarget : class
        {
            return validationContext.WithDefaultErrorMessage( o => $"{o.FriendlyName} is required." )
                .Validate( o => o.Target != null, validationContext.ErrorMessageBuilder, out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is not <see langword="null"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotNull<TTarget>( this ValidationContext<TTarget?> validationContext, out ValidationResult validationResult ) where TTarget : struct
        {
            return validationContext.WithDefaultErrorMessage( o => $"{o.FriendlyName} is required." )
                .Validate( o => o.Target.HasValue, validationContext.ErrorMessageBuilder, out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is not <see langword="null"/> or white space.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotNullOrWhiteSpace( this ValidationContext<string> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( o => $"{o.FriendlyName} is required." )
                .Validate( o => o.Target.IsNotNullOrWhiteSpace(), validationContext.ErrorMessageBuilder, out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is now or a future date time.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNowOrFutureDateTime( this ValidationContext<DateTimeOffset> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( o => $"{o.FriendlyName} must be a future date/time." )
                .Validate( o => o.Target.DateTime.CompareTo( RockDateTime.Now ) >= 0, validationContext.ErrorMessageBuilder, out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is not an empty collection.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotEmpty<TTargetItem>( this ValidationContext<IEnumerable<TTargetItem>> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( o => $"At least one {o.FriendlyName.Singularize()} is required." )
                .Validate( o => o.Target?.Any() == true, validationContext.ErrorMessageBuilder, out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is not an empty collection.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotEmpty<TTargetItem>( this ValidationContext<List<TTargetItem>> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( o => $"At least one {o.FriendlyName.Singularize()} is required." )
                .Validate( o => o.Target?.Any() == true, validationContext.ErrorMessageBuilder, out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is not an empty <see cref="Guid"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotEmpty( this ValidationContext<Guid> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( o => $"{o.FriendlyName} is required." )
                .Validate( o => !o.Target.IsEmpty(), validationContext.ErrorMessageBuilder, out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is greater than or equal to <paramref name="minValue"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="minValue">The minimum valid value.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsGreaterThanOrEqualTo( this ValidationContext<int> validationContext, int minValue, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( o => $"{o.FriendlyName} must be greater than or equal to {minValue}." )
                .Validate( o => o.Target >= minValue, validationContext.ErrorMessageBuilder, out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is <see langword="null"/> or has a length that is less than or equal to <paramref name="maxLength"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool HasMaxLength( this ValidationContext<string> validationContext, int maxLength, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( o => $"{o.FriendlyName} must be less than or equal to {maxLength} characters." )
                .Validate( o => o.Target == null || o.Target.Length <= maxLength, validationContext.ErrorMessageBuilder, out validationResult );
        }
        
        /// <summary>
        /// Overrides the error message builder of the validation context.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="errorMessageBuilder">The error message builder.</param>
        /// <returns>The modified <paramref name="validationContext"/>.</returns>
        internal static ValidationContext<TTarget> WithErrorMessage<TTarget>( this ValidationContext<TTarget> validationContext, Func<ValidationContext<TTarget>, string> errorMessageBuilder )
        {
            validationContext.ErrorMessageBuilder = errorMessageBuilder;
            return validationContext;
        }

        /// <summary>
        /// Sets the default error message builder for the validation context if it has not yet been assigned.
        /// </summary>
        /// <typeparam name="TTarget">The validation target type.</typeparam>
        /// <param name="validationContext">The validation target.</param>
        /// <param name="errorMessageBuilder">The default error message builder.</param>
        /// <returns>The modified <paramref name="validationContext"/>.</returns>
        private static ValidationContext<TTarget> WithDefaultErrorMessage<TTarget>( this ValidationContext<TTarget> validationContext, Func<ValidationContext<TTarget>, string> errorMessageBuilder )
        {
            if ( validationContext.ErrorMessageBuilder == null )
            {
                validationContext.ErrorMessageBuilder = errorMessageBuilder;
            }

            return validationContext;
        }

        /// <summary>
        /// Begins a new validation context for the target value.
        /// </summary>
        /// <typeparam name="TTarget">The validation target type.</typeparam>
        /// <param name="target">The target to validate.</param>
        /// <param name="friendlyName">The validation target friendly name that will be used in error messages if validation fails.</param>
        /// <returns>A new <see cref="ValidationContext{TTarget}"/> instance.</returns>
        internal static ValidationContext<TTarget> Validate<TTarget>( this TTarget target, string friendlyName )
        {
            return new ValidationContext<TTarget>( target, friendlyName );
        }
    }
}
