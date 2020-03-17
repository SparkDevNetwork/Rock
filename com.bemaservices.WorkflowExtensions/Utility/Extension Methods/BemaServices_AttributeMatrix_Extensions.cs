// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bemaservices.WorkflowExtensions
{
    /// <summary>
    /// BEMA Attribute Matrix Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="attributeMatrixItem">The attributeMatrixItem.</param>
        /// <param name="key">The key.</param>
        /// <param name="checkWorkflowAttributeValue">if set to <c>true</c> and the returned value is a guid, check to see if the workflow 
        /// or activity contains an attribute with that guid. This is useful when using the WorkflowTextOrAttribute field types to get the 
        /// actual value or workflow value.</param>
        /// <returns></returns>
        public static string GetMatrixAttributeValue( this AttributeMatrixItem attributeMatrixItem, WorkflowAction action, string key, bool checkWorkflowAttributeValue )
        {
            string value = attributeMatrixItem.GetAttributeValue( key );
            if ( checkWorkflowAttributeValue )
            {
                Guid? attributeGuid = value.AsGuidOrNull();
                if ( attributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Get( attributeGuid.Value );
                    if ( attribute != null )
                    {
                        value = action.GetWorklowAttributeValue( attributeGuid.Value );
                        if ( !string.IsNullOrWhiteSpace( value ) )
                        {
                            if ( attribute.FieldTypeId == FieldTypeCache.Get( Rock.SystemGuid.FieldType.ENCRYPTED_TEXT.AsGuid() ).Id )
                            {
                                value = Rock.Security.Encryption.DecryptString( value );
                            }
                            else if ( attribute.FieldTypeId == FieldTypeCache.Get( Rock.SystemGuid.FieldType.SSN.AsGuid() ).Id )
                            {
                                value = Rock.Field.Types.SSNFieldType.UnencryptAndClean( value );
                            }
                        }
                    }
                }
            }

            return value;
        }

    }
}
