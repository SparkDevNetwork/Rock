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

namespace Rock.Data
{
    /// <summary>
    /// <para>
    /// Specifies that the property can be used for qualifying attributes
    /// when matching against the entity.
    /// </para>
    /// <para>
    /// When used on a class you must specify the names of the properties that
    /// can be used for qualifying attributes.
    /// </para>
    /// </summary>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Class, Inherited = false )]
    public class EnableAttributeQualificationAttribute : System.Attribute
    {
        internal string[] PropertyNames { get; }

        /// <summary>
        /// Specifies that the property this attribute is placed on supports
        /// qualification.
        /// </summary>
        public EnableAttributeQualificationAttribute()
        {
            PropertyNames = new string[0];
        }

        /// <summary>
        /// Used on class definitions to specify additional property names that
        /// are available for qualification.
        /// </summary>
        /// <param name="propertyNames">The names of the properties.</param>
        public EnableAttributeQualificationAttribute( params string[] propertyNames )
        {
            PropertyNames = propertyNames;
        }
    }
}
