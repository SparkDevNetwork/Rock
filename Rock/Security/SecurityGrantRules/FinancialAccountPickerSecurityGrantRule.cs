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

using Newtonsoft.Json;

namespace Rock.Security.SecurityGrantRules
{
    /// <summary>
    /// Grants the Obsidian Account Picker access to more than the public view
    /// of financial accounts. Without this rule the picker only returns active,
    /// public accounts using their public names.
    /// </summary>
    [Rock.SystemGuid.SecurityGrantRuleGuid( "e2520014-4590-4536-aafd-eb3e0b9240cb" )]
    public sealed class FinancialAccountPickerSecurityGrantRule : SecurityGrantRule
    {
        #region Properties

        /// <summary>
        /// The instance checked to see if accounts that are not public may be returned.
        /// </summary>
        public static object NonPublicAccountsInstance { get; } = new object();

        /// <summary>
        /// The instance checked to see if internal account details (the account
        /// name and GL code) may be returned.
        /// </summary>
        public static object InternalDetailsInstance { get; } = new object();

        /// <summary>
        /// The instance checked to see if inactive accounts may be returned.
        /// </summary>
        public static object InactiveAccountsInstance { get; } = new object();

        /// <summary>
        /// Gets a value indicating whether accounts that are not public may be returned.
        /// </summary>
        [JsonProperty( "np", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public bool AllowNonPublicAccounts { get; private set; }

        /// <summary>
        /// Gets a value indicating whether internal account details (the account
        /// name and GL code) may be returned.
        /// </summary>
        [JsonProperty( "in", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public bool AllowInternalDetails { get; private set; }

        /// <summary>
        /// Gets a value indicating whether inactive accounts may be returned.
        /// </summary>
        [JsonProperty( "ia", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public bool AllowInactiveAccounts { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="FinancialAccountPickerSecurityGrantRule"/> class from being created.
        /// </summary>
        private FinancialAccountPickerSecurityGrantRule()
            : base( Authorization.VIEW )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialAccountPickerSecurityGrantRule"/> class.
        /// </summary>
        /// <param name="allowNonPublicAccounts">If <c>true</c> then accounts that are not public may be returned.</param>
        /// <param name="allowInternalDetails">If <c>true</c> then the account name and GL code may be returned.</param>
        /// <param name="allowInactiveAccounts">If <c>true</c> then inactive accounts may be returned.</param>
        public FinancialAccountPickerSecurityGrantRule( bool allowNonPublicAccounts, bool allowInternalDetails, bool allowInactiveAccounts )
            : base( Authorization.VIEW )
        {
            AllowNonPublicAccounts = allowNonPublicAccounts;
            AllowInternalDetails = allowInternalDetails;
            AllowInactiveAccounts = allowInactiveAccounts;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a rule that gives the Account Picker full access, for use
        /// by administrative blocks.
        /// </summary>
        /// <returns>A new rule that allows everything.</returns>
        public static FinancialAccountPickerSecurityGrantRule CreateFullAccessRule()
        {
            return new FinancialAccountPickerSecurityGrantRule( true, true, true );
        }

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            if ( ReferenceEquals( obj, NonPublicAccountsInstance ) )
            {
                return AllowNonPublicAccounts;
            }

            if ( ReferenceEquals( obj, InternalDetailsInstance ) )
            {
                return AllowInternalDetails;
            }

            if ( ReferenceEquals( obj, InactiveAccountsInstance ) )
            {
                return AllowInactiveAccounts;
            }

            return false;
        }

        #endregion
    }
}
