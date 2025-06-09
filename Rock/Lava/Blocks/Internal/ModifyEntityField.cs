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
using System.Text.RegularExpressions;

using Humanizer;

namespace Rock.Lava.Blocks.Internal
{
    /// <summary>
    /// This class represents information about a field (property or attribute) that is being
    /// updated via the modify entity command.
    /// </summary>
    internal class ModifyEntityField
    {
        #region Fields

        /// <summary>
        /// Regex pattern to detect HTML injection in values.
        /// </summary>
        private static readonly Regex scriptInjectionPattern = new Regex( @"<[/0-9a-zA-Z]" );

        /// <summary>
        /// The configuration values this field was constructed with.
        /// </summary>
        private readonly Dictionary<string, object> _fieldConfiguration;

        #endregion

        #region Properties

        /// <summary>
        /// The name of the property or the attribute key being updated.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The new value to set the field to.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// After instance creation, this will be set to <c>false</c> if
        /// the value is invalid.
        /// </summary>
        public bool IsValid { get; private set; } = true;

        /// <summary>
        /// If the value is invalid this will contain the error message
        /// describing the reason.
        /// </summary>
        public string ValidationMessage { get; private set; } = string.Empty;

        /// <summary>
        /// The name of the control this value came from.
        /// </summary>
        public string ControlName { get; }

        /// <summary>
        /// If <c>true</c> then HTML values will be prevented during validation.
        /// </summary>
        public bool IsInjectionPreventionEnabled { get; }

        #endregion

        #region Keys

        private static class FieldConfigurationKey
        {
            public const string Name = "name";
            public const string Key = "key";
            public const string Value = "content";
            public const string Control = "control";
            public const string IsInjectionPreventionEnabled = "injectionpreventionenabled";
            public const string IsRequired = "isrequired";
            public const string Min = "min";
            public const string Max = "max";
            public const string MinLength = "minlength";
            public const string MaxLength = "maxlength";
            public const string Pattern = "pattern";
            public const string ValidationMessage = "validationmessage";
        }

        #endregion Keys

        #region Constructors

        /// <summary>
        /// Constructs a new instance of the <see cref="ModifyEntityField"/> class.
        /// </summary>
        /// <param name="isAttribute"><c>true</c> if this is an attribute field; <c>false</c> if it is a property field.</param>
        /// <param name="fieldConfiguration">The configuration that describes the field and values.</param>
        private ModifyEntityField( bool isAttribute, Dictionary<string, object> fieldConfiguration )
        {
            _fieldConfiguration = fieldConfiguration;

            // Determine the field type and name
            if ( !isAttribute )
            {
                Name = GetConfigurationValue( FieldConfigurationKey.Name );
            }
            else
            {
                Name = GetConfigurationValue( FieldConfigurationKey.Key );
            }

            // Get the field values
            Value = GetConfigurationValue( FieldConfigurationKey.Value );
            IsInjectionPreventionEnabled = GetConfigurationValue( FieldConfigurationKey.IsInjectionPreventionEnabled ).AsBoolean( true );
            ControlName = GetConfigurationValue( FieldConfigurationKey.Control );

            // Perform Validation
            ValidateValue();

            // Set Validation Message
            SetValidationMessage();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ModifyEntityField"/> class
        /// for modifying a property value.
        /// </summary>
        /// <param name="fieldConfiguration">The configuration that describes the property and value.</param>
        /// <returns>An instance of <see cref="ModifyEntityField"/> that represents the change request.</returns>
        public static ModifyEntityField Property( Dictionary<string, object> fieldConfiguration )
        {
            return new ModifyEntityField( false, fieldConfiguration );
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ModifyEntityField"/> class
        /// for modifying an attribute value.
        /// </summary>
        /// <param name="fieldConfiguration">The configuration that describes the attribute and value.</param>
        /// <returns>An instance of <see cref="ModifyEntityField"/> that represents the change request.</returns>
        public static ModifyEntityField Attribute( Dictionary<string, object> fieldConfiguration )
        {
            return new ModifyEntityField( true, fieldConfiguration );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines if the value provided matches the configured validation rules.
        /// </summary>
        private void ValidateValue()
        {
            // Validate that it does not have a script injection. This is the same logic used in Obsidian and Webforms
            if ( IsInjectionPreventionEnabled )
            {
                if ( scriptInjectionPattern.IsMatch( this.Value ) )
                {
                    IsValid = false;
                    ValidationMessage = "Value contains an invalid or unsafe value.";
                    return;
                }
            }

            // Validate required
            if ( GetConfigurationValue( FieldConfigurationKey.IsRequired ).AsBoolean() )
            {
                if ( Value.IsNullOrWhiteSpace() )
                {
                    IsValid = false;
                    return;
                }
            }

            // Validate minlength
            var minLength = GetConfigurationValue( FieldConfigurationKey.MinLength ).AsIntegerOrNull();
            if ( minLength.HasValue && Value.Length < minLength )
            {
                IsValid = false;
                return;
            }

            // Validate maxlength
            var maxLength = GetConfigurationValue( FieldConfigurationKey.MaxLength ).AsIntegerOrNull();
            if ( maxLength.HasValue && Value.Length > maxLength )
            {
                IsValid = false;
                return;
            }

            // Validate pattern
            var pattern = GetConfigurationValue( FieldConfigurationKey.Pattern );
            if ( pattern.IsNotNullOrWhiteSpace() )
            {
                if ( !Regex.IsMatch( Value, pattern ) )
                {
                    IsValid = false;
                    return;
                }
            }

            // Get the value as a decimal for final conversions
            var valueAsNumber = Value.AsDecimalOrNull();

            if ( !valueAsNumber.HasValue )
            {
                return;
            }

            // Validate min
            var minValue = GetConfigurationValue( FieldConfigurationKey.Min ).AsDecimalOrNull();
            if ( minValue.HasValue && valueAsNumber < minValue )
            {
                IsValid = false;
                return;
            }

            // Validate max
            var maxValue = GetConfigurationValue( FieldConfigurationKey.Max ).AsDecimalOrNull();
            if ( maxValue.HasValue && valueAsNumber > maxValue )
            {
                IsValid = false;
                return;
            }
        }

        /// <summary>
        /// Sets the appropriate validation message.
        /// </summary>
        private void SetValidationMessage()
        {
            // Only provide a message if the value is not valid or if there isn't already a value.
            if ( IsValid || ValidationMessage.IsNotNullOrWhiteSpace() )
            {
                return;
            }

            var validationMessage = GetConfigurationValue( FieldConfigurationKey.ValidationMessage );

            if ( validationMessage.IsNotNullOrWhiteSpace() )
            {
                ValidationMessage = validationMessage;
                return;
            }

            // Set default message
            ValidationMessage = $"Please enter a valid value for {Name.Humanize( LetterCasing.Title )}.";
        }

        /// <summary>
        /// Gets the configuration item provided in a safe way.
        /// </summary>
        /// <param name="key">The configuration key to retrieve.</param>
        /// <returns>The value for <paramref name="key"/> or <c>null</c> if it was not found.</returns>
        private string GetConfigurationValue( string key )
        {
            if ( !_fieldConfiguration.ContainsKey( key ) )
            {
                return null;
            }

            return _fieldConfiguration[key].ToString();
        }

        #endregion
    }
}
