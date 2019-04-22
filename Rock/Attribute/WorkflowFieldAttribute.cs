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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to a workflow
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class WorkflowFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute" /> class.
        /// </summary>
        /// <param name="workflowTypeGuid">The group type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public WorkflowFieldAttribute( string workflowTypeGuid = "", string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.WorkflowFieldType ).FullName )
        {
            if ( !string.IsNullOrWhiteSpace( workflowTypeGuid ) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( workflowTypeGuid, out guid ) )
                {
                    using ( var rockContext = new Rock.Data.RockContext() )
                    {
                        var workflowType = new Rock.Model.WorkflowTypeService( rockContext ).Get( guid );
                        if ( workflowType != null )
                        {
                            var configValue = new Field.ConfigurationValue( workflowType.Id.ToString() );
                            FieldConfigurationValues.Add( "workflowtype", configValue );
                        }
                    }
                }
            }
        }
    }
}