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
using System.Text.RegularExpressions;

namespace Rock.Communication
{
    /// <summary>
    /// The result of an email field validation.
    /// </summary>
    public enum EmailFieldValidationResultSpecifier
    {
        /// <summary>
        /// The field content is valid.
        /// </summary>
        Valid = 0,
        /// <summary>
        /// One or more email addresses in the field have an invalid format.
        /// </summary>
        InvalidEmailAddressFormat = 1,
        /// <summary>
        /// Lava code is not allowed, but has been detected in the field content.
        /// </summary>
        InvalidLavaNotAllowed = 2,
        /// <summary>
        /// Multiple addresses are not allowed, but more than one address has been detected.
        /// </summary>
        InvalidMultipleAddressesNotAllowed = 3,
    }

    /// <summary>
    /// Validates an email address field, with the option to allow multiple addresses and Lava code.
    /// </summary>
    public class EmailAddressFieldValidator
    {
        /*
         * DV 19-Jan-2022
         *
         * When updating this regex make sure to also update the Rock.Model.Person.Email
         * RegularExpression attribute to the same value. Otherwise, this will pass the
         * UI check and fail to save to the database. #4829, #4867
         *
         */
        private const string _emailAddressRegex = @"\s*(\w+(?:[-+.]*\w+)*@\w+(?:[-.]\w+)*\.\w+(?:[-.]\w+)*)\s*";
        private const string _lavaVariableRegex = @"({{\s*[^}}]+\s*}})";
        private const string _lavaTagRegex = @"({%\s*[^%}]+\s*%})";
        private const string _lavaBlockRegex = @"(({%\s*(?<tagName>(\w+)\s*).*%})).*({%\s*end\k<tagName>\s*%})";
        private const string _lavaShortcodeRegex = @"({\[\s*[^\]}]+\s*\]})";

        private Regex _regex = null;

        private bool _allowLava = false;
        private bool _allowMultipleAddresses = false;

        #region Static Methods

        /// <summary>
        /// Validate the content of an email address field with the specified settings.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="allowMultipleAddresses"></param>
        /// <param name="allowLava"></param>
        /// <returns></returns>
        public static EmailFieldValidationResultSpecifier Validate( string content, bool allowMultipleAddresses, bool allowLava )
        {
            var validator = new EmailAddressFieldValidator { MultipleAddressesAreAllowed = allowMultipleAddresses, LavaIsAllowed = allowLava };

            return validator.Validate( content );
        }

        /// <summary>
        /// Gets a regular expression to validate an email address field with the specified settings.
        /// </summary>
        /// <param name="allowMultipleAddresses"></param>
        /// <param name="allowLava"></param>
        /// <returns></returns>
        public static string GetRegularExpression( bool allowMultipleAddresses, bool allowLava )
        {
            var validator = new EmailAddressFieldValidator { MultipleAddressesAreAllowed = allowMultipleAddresses, LavaIsAllowed = allowLava };

            return validator.GetRegularExpression();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a flag indicating if Lava syntax is allowed in the email field.
        /// </summary>
        public bool LavaIsAllowed
        {
            get
            {
                return _allowLava;
            }
            set
            {
                _allowLava = value;

                InvalidateCurrentRegex();
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating if multiple addresses are allowed in the email field.
        /// </summary>
        public bool MultipleAddressesAreAllowed
        {
            get
            {
                return _allowMultipleAddresses;
            }
            set
            {
                _allowMultipleAddresses = value;

                InvalidateCurrentRegex();
            }
        }

        #endregion

        /// <summary>
        /// Validate the specified field content and return the validation result.
        /// </summary>
        /// <param name="fieldContent"></param>
        /// <returns></returns>
        public EmailFieldValidationResultSpecifier Validate( string fieldContent )
        {
            var regex = GetRegex();

            if ( regex.IsMatch( fieldContent ) )
            {
                return EmailFieldValidationResultSpecifier.Valid;
            }

            if ( !_allowMultipleAddresses )
            {
                // Check if the field is invalid because it contains multiple addresses.
                if ( fieldContent.Contains( "," ) )
                {
                    return EmailFieldValidationResultSpecifier.InvalidMultipleAddressesNotAllowed;
                }
            }

            if ( !_allowLava )
            {
                // Check if the field is invalid because it contains Lava.
                if ( fieldContent.Contains( "{{" ) )
                {
                    return EmailFieldValidationResultSpecifier.InvalidLavaNotAllowed;
                }
            }

            // The field contains a single invalid email address.
            return EmailFieldValidationResultSpecifier.InvalidEmailAddressFormat;
        }

        /// <summary>
        /// Get a regular expression to validate an email address field for the current settings.
        /// </summary>
        /// <returns></returns>
        public string GetRegularExpression()
        {
            // Create a new Regex using the current settings, and compile it for re-use.
            // There is a small performance hit for compilation, but substantial gains when re-using the validator for multiple validations.
            if ( _allowMultipleAddresses )
            {
                if ( _allowLava )
                {
                    // Zero or more repeating groups of Lava tags, or email addresses separated by whitespace or commas.
                    return $@"(({_lavaBlockRegex}|{_lavaTagRegex}|{_lavaShortcodeRegex}|{_lavaVariableRegex})(({_emailAddressRegex})([\s,])*)*)";
                }

                // One or more email addresses, separated by whitespace or commas.
                return $@"(({_emailAddressRegex}[\s,]*)*)";
            }

            if ( _allowLava )
            {
                // Zero or more repeating groups of Lava tags, or an email address.
                return $@"^\s*(({_lavaBlockRegex}|{_lavaTagRegex}|{_lavaShortcodeRegex}|{_lavaVariableRegex}|{_emailAddressRegex})*)\s*$";
            }

            // A single email address.
            return $@"^{_emailAddressRegex}$";
        }

        private void InvalidateCurrentRegex()
        {
            _regex = null;
        }

        private Regex GetRegex()
        {
            if ( _regex == null )
            {
                _regex = new Regex( GetRegularExpression(), RegexOptions.Compiled );
            }

            return _regex;
        }
    }
}
