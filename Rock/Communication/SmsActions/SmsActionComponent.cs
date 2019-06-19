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
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.Extension;
using Rock.Field.Types;
using Rock.Web.Cache;

namespace Rock.Communication.SmsActions
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Extension.Component" />
    [TextValueFilterField( "Phone Numbers", "The phone numbers that this action will run on.", false, order: 0, category: BaseAttributeCategories.Filters, hideFilterMode: true )]
    public abstract class SmsActionComponent : Component
    {
        /// <summary>
        /// Categories for the attributes
        /// </summary>
        public class BaseAttributeCategories
        {
            /// <summary>
            /// The filters category
            /// </summary>
            public const string Filters = "Filters";
        }

        #region Properties

        /// <summary>
        /// Gets the component title to be displayed to the user.
        /// </summary>
        /// <value>
        /// The component title to be displayed to the user.
        /// </value>
        public virtual string Title
        {
            get
            {
                var metadata = GetType().GetCustomAttributes( typeof( ExportMetadataAttribute ), false )
                    .Cast<ExportMetadataAttribute>()
                    .Where( m => m.Name == "ComponentName" && m.Value != null )
                    .FirstOrDefault();

                if ( metadata != null )
                {
                    return ( string ) metadata.Value;
                }
                else
                {
                    return GetType().Name;
                }
            }
        }

        /// <summary>
        /// Gets the icon CSS class used to identify this component type.
        /// </summary>
        /// <value>
        /// The icon CSS class used to identify this component type.
        /// </value>
        public abstract string IconCssClass { get; }

        /// <summary>
        /// Gets the description of this SMS Action.
        /// </summary>
        /// <value>
        /// The description of this SMS Action.
        /// </value>
        public abstract string Description { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is active. The component is
        /// always active, it is up to the SmsAction entity to decide if the action
        /// is active or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive => true;

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order => 0;

        #endregion

        #region Methods

        /// <summary>
        /// Use GetAttributeValue( SmsActionCache action, string key) instead. SmsAction attribute values are 
        /// specific to the action instance (rather than global).  This method will throw an exception
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">SmsActionComponent attributes are saved specific to the action, which requires that the action is included in order to load or retrieve values. Use the GetAttributeValue( SmsActionCache action, string key ) method instead.</exception>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "SmsActionComponent attributes are saved specific to the action, which requires that the action is included in order to load or retrieve values. Use the GetAttributeValue( SmsActionCache action, string key ) method instead." );
        }

        /// <summary>
        /// Gets the attribute value for the service check type
        /// </summary>
        /// <param name="action">The SMS action.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( SmsActionCache action, string key )
        {
            return action.GetAttributeValue( key );
        }

        /// <summary>
        /// Checks the attributes for this component and determines if the message
        /// should be processed.
        /// </summary>
        /// <param name="action">The action that contains the configuration for this component.</param>
        /// <param name="message">The message that is to be checked.</param>
        /// /// <param name="errorMessage">If there is a problem, this should be set</param>
        /// <returns><c>true</c> if the message should be processed.</returns>
        public virtual bool ShouldProcessMessage( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;

            var attribute = action.Attributes.ContainsKey( "PhoneNumbers" ) ? action.Attributes["PhoneNumbers"] : null;
            var phoneNumbers = GetAttributeValue( action, "PhoneNumbers" );
            var filter = ValueFilterFieldType.GetFilterExpression( attribute?.QualifierValues, phoneNumbers );

            return filter != null ? filter.Evaluate( message, "ToNumber" ) : true;
        }

        /// <summary>
        /// Processes the message that was received from the remote user.
        /// </summary>
        /// <param name="action">The action that contains the configuration for this component.</param>
        /// <param name="message">The message that was received by Rock.</param>
        /// <param name="errorMessage">If there is a problem processing, this should be set</param>
        /// <returns>An SmsMessage that will be sent as the response or null if no response should be sent.</returns>
        public abstract SmsMessage ProcessMessage( SmsActionCache action, SmsMessage message, out string errorMessage );

        #endregion
    }
}
