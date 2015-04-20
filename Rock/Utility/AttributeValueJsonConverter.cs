// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using Rock.Model;

namespace Rock.Utility
{
    /// <summary>
    /// Serializes only the specified fields depending on the LoadAttributes parameter of a REST call
    /// if the parameter value is 'simple' or True, only the specified fields will be specified
    /// if the paremter value is 'expanded', the object will be serialized normally
    /// </summary>
    public class AttributeValueJsonConverter : SimpleModeJsonConverter<AttributeValue>
    {
        /// <summary>
        /// Gets the properties to serialize in simple mode.
        /// </summary>
        /// <value>
        /// The properties to serialize in simple mode.
        /// </value>
        public override string[] PropertiesToSerializeInSimpleMode
        {
            get
            {
                return new string[] { "Id", "Value" };
            }
        }
    }
}
