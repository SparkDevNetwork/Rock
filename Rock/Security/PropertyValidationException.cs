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

namespace Rock.Security
{
    /// <summary>
    /// This exception is used by Rock's internal save processing to indicate that
    /// a property contains invalid data and cannot be saved. 
    /// </summary>
    public class PropertyValidationException : Exception
    {
        /// <summary>
        /// The name of the property that contained the invalid value.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The reflected type that contained the invalid value.
        /// </summary>
        public Type ReflectedType { get; }

        /// <summary>
        /// The reason why the property value is invalid.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Creates a new instance of <see cref="PropertyValidationException"/>.
        /// </summary>
        /// <param name="reflectedType">The reflected type that contained the invalid value. Cannot be null.</param>
        /// <param name="propertyName">The name of the property with the invalid value. Cannot be null.</param>
        /// <param name="reason">The reason why the property value is invalid. Should not end with punctuation and should follow a format like "may not contain script tags".</param>
        internal PropertyValidationException( Type reflectedType, string propertyName, string reason )
            : base( $"The value of the '{propertyName.SplitCase()}' property on {GetRealType( reflectedType ).Name} {reason}." )
        {
            ReflectedType = reflectedType ?? throw new ArgumentNullException( nameof( reflectedType ) );
            PropertyName = propertyName ?? throw new ArgumentNullException( nameof( propertyName ) );
            Reason = reason;
        }

        /// <summary>
        /// Gets the real type. This is used to deal with EF dynamic proxies so
        /// we return the friendly name rather than the EF generated name.
        /// </summary>
        /// <param name="type">The type that contained invalid data.</param>
        /// <returns>The real type declared in code.</returns>
        private static Type GetRealType( Type type )
        {
            return type.IsDynamicProxyType()
                ? type.BaseType
                : type;
        }
    }
}
