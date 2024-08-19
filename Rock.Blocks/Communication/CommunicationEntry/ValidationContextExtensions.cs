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

using Rock.Communication;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Validation extension methods.
    /// </summary>
    internal static class ValidationContextExtensions
    {
        #region Common Methods
        
        /// <summary>
        /// Begins a new validation context for the target values.
        /// </summary>
        /// <typeparam name="TTarget">The validation target type.</typeparam>
        /// <param name="targets">The targets to validate.</param>
        /// <param name="friendlyName">The validation target friendly name that will be used in error messages if validation fails.</param>
        /// <returns>A new <see cref="ValidationContext{TTarget}"/> instance.</returns>
        internal static ValidationContext<TTarget> ValidateEach<TTarget>( this IEnumerable<TTarget> targets, string friendlyName = null )
        {
            return new ValidationContext<TTarget>
            {
                Targets = targets,
                FriendlyName = friendlyName
            };
        }
        
        /// <summary>
        /// Begins a new validation context for the target value.
        /// </summary>
        /// <typeparam name="TTarget">The validation target type.</typeparam>
        /// <param name="target">The target to validate.</param>
        /// <param name="friendlyName">The validation target friendly name that will be used in error messages if validation fails.</param>
        /// <returns>A new <see cref="ValidationContext{TTarget}"/> instance.</returns>
        internal static ValidationContext<TTarget> Validate<TTarget>( this TTarget target, string friendlyName = null )
        {
            return new ValidationContext<TTarget>
            {
                Targets = new[] { target },
                FriendlyName = friendlyName
            };
        }
        
        /// <summary>
        /// Validates that the <paramref name="validator"/> returns <see langword="true"/>.
        /// </summary>
        /// <typeparam name="TTarget">The validation target type.</typeparam>
        /// <param name="validationContext">The target to validation.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsTrue<TTarget>( this IValidationContext<TTarget> validationContext, Func<TTarget, bool> validator, out ValidationResult validationResult )
        {
            validationContext.SetValidator( new IsValidDelegator<TTarget>( validator ) );
            return validationContext.Validate( out validationResult );
        }

        /// <summary>
        /// Overrides the error message builder of the validation context.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="errorMessageBuilder">The error message builder.</param>
        /// <returns>The modified <paramref name="validationContext"/>.</returns>
        internal static IValidationContext<TTarget> WithErrorMessage<TTarget>( this IValidationContext<TTarget> validationContext, Func<TTarget, string, string> errorMessageBuilder )
        {
            validationContext.SetErrorMessageBuilder( new GetValidationErrorMessageDelegator<TTarget>( () => validationContext.FriendlyName, errorMessageBuilder ) );
            return validationContext;
        }

        /// <summary>
        /// Sets the default error message builder for the validation context if it has not yet been assigned.
        /// </summary>
        /// <typeparam name="TTarget">The validation target type.</typeparam>
        /// <param name="validationContext">The validation target.</param>
        /// <param name="errorMessageBuilder">The default error message builder.</param>
        /// <returns>The modified <paramref name="validationContext"/>.</returns>
        private static IValidationContext<TTarget> WithDefaultErrorMessage<TTarget>( this IValidationContext<TTarget> validationContext, Func<TTarget, string, string> errorMessageBuilder )
        {
            if ( !validationContext.HasErrorMessageBuilder )
            {
                validationContext.SetErrorMessageBuilder( new GetValidationErrorMessageDelegator<TTarget>( () => validationContext.FriendlyName, errorMessageBuilder ) );
            }

            return validationContext;
        }

        #endregion

        #region Boolean Validation Methods

        /// <summary>
        /// Validates that the target value is <see langword="true"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsTrue( this IValidationContext<bool> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} must be true." )
                .IsTrue( target => target, out validationResult );
        }

        #endregion

        #region Class Validation Methods

        /// <summary>
        /// Validates that the target value is not <see langword="null"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotNull<TTarget>( this IValidationContext<TTarget> validationContext, out ValidationResult validationResult ) where TTarget : class
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} is required." )
                .IsTrue( target => target != null, out validationResult );
        }

        #endregion

        #region Collection (IEnumerable, List, Array) Validation Methods
                
        /// <summary>
        /// Validates that the target value is not an empty collection.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotEmpty<TTargetItem>( this IValidationContext<IEnumerable<TTargetItem>> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"At least one {friendlyName?.Singularize() ?? "item"} is required." )
                .IsTrue( target => target?.Any() == true, out validationResult );
        }

        /// <summary>
        /// Validates that the target value is null or an empty collection.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNullOrEmpty<TTargetItem>( this IValidationContext<IEnumerable<TTargetItem>> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName?.Singularize() ?? "item"} should be empty." )
                .IsTrue( target => target?.Any() != true, out validationResult );
        }

        #endregion

        #region Date (DateTime, DateTimeOffset) Validation Methods

        /// <summary>
        /// Validates that the target value is now or a future date time.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNowOrFuture( this IValidationContext<DateTimeOffset> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} must be a future date/time." )
                .IsTrue( target => target.DateTime.CompareTo( RockDateTime.Now ) >= 0, out validationResult );
        }

        #endregion

        #region Guid Validation Methods

        /// <summary>
        /// Validates that the target value is not an empty <see cref="Guid"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotEmpty( this IValidationContext<Guid> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} is required." )
                .IsTrue( target => !target.IsEmpty(), out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is neither <see langword="null"/> nor an empty Guid.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotNullOrEmpty( this IValidationContext<Guid?> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} is required." )
                .IsTrue( target => target.HasValue && !target.Value.IsEmpty(), out validationResult );
        }

        #endregion

        #region Number (int, decimal, double, etc.) Validation Methods

        /// <summary>
        /// Validates that the target value is greater than or equal to <paramref name="minValue"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="minValue">The minimum valid value.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsGreaterThanOrEqualTo( this IValidationContext<int> validationContext, int minValue, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} must be greater than or equal to {minValue}." )
                .IsTrue( target => target >= minValue, out validationResult );
        }

        #endregion

        #region String Validation Methods
        
        /// <summary>
        /// Validates that the target value is <see langword="null"/> or has a length that is less than or equal to <paramref name="maxLength"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="maxLength">The maximum length allowed for the string.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool HasMaxLength( this IValidationContext<string> validationContext, int maxLength, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} must be less than or equal to {maxLength} characters." )
                .IsTrue( target => target == null || target.Length <= maxLength, out validationResult );
        }

        /// <summary>
        /// Validates that the target value is <see langword="null"/> or white space.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNullOrWhiteSpace( this IValidationContext<string> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} should be empty." )
                .IsTrue( target => target.IsNullOrWhiteSpace(), out validationResult );
        }
        
        /// <summary>
        /// Validates that the target value is not <see langword="null"/> or white space.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotNullOrWhiteSpace( this IValidationContext<string> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} is required." )
                .IsTrue( target => target.IsNotNullOrWhiteSpace(), out validationResult );
        }
        
        /// <summary>
        /// Validates that the target is a valid email address.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsEmailAddress( this IValidationContext<string> validationContext, out ValidationResult validationResult )
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} is an invalid email address." )
                .IsTrue( EmailAddressFieldValidator.IsValid, out validationResult );
        }

        #endregion

        #region Struct Validation Methods

        /// <summary>
        /// Validates that the target value is <see langword="null"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNull<TTarget>( this IValidationContext<TTarget?> validationContext, out ValidationResult validationResult ) where TTarget : struct
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} is required." )
                .IsTrue( target => !target.HasValue, out validationResult );
        }

        /// <summary>
        /// Validates that the target value is not <see langword="null"/>.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="validationResult">Set to <see cref="ValidationResult.Success"/> if valid; otherwise a validation result with an error message.</param>
        /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
        internal static bool IsNotNull<TTarget>( this IValidationContext<TTarget?> validationContext, out ValidationResult validationResult ) where TTarget : struct
        {
            return validationContext.WithDefaultErrorMessage( ( _, friendlyName ) => $"{friendlyName ?? "Field"} is required." )
                .IsTrue( target => target.HasValue, out validationResult );
        }

        #endregion

        #region Helper Classes
        
        private class GetValidationErrorMessageDelegator<TTarget> : IGetValidationErrorMessage<TTarget>
        {
            private readonly Func<string> _getFriendlyName;
            private readonly Func<TTarget, string, string> _getErrorMessage;

            public GetValidationErrorMessageDelegator( Func<string> getFriendlyName, Func<TTarget, string, string> getErrorMessage )
            {
                _getFriendlyName = getFriendlyName;
                _getErrorMessage = getErrorMessage;
            }

            public string GetValidationErrorMessage( TTarget target )
            {
                return _getErrorMessage( target, _getFriendlyName() );
            }
        }

        private class IsValidDelegator<TTarget> : IIsValid<TTarget>
        {
            private readonly Func<TTarget, bool> _validator;

            public IsValidDelegator( Func<TTarget, bool> validator )
            {
                _validator = validator;
            }

            public bool IsValid( TTarget target )
            {
                return _validator( target );
            }
        }

        #endregion
    }
}
