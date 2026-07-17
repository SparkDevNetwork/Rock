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

using Rock.Configuration;
using Rock.Web.Cache;

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
        [Obsolete( "Use the constructor that takes only workflowTypeGuid and name parameters." )]
        [RockObsolete( "20.0" )]
        public WorkflowFieldAttribute( string workflowTypeGuid = "", string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.WORKFLOW.AsGuid(), name, description, required, defaultValue, category, order, key )
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
                            FieldConfigurationValues.AddOrReplace( "workflowtype", configValue );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowFieldAttribute" /> class.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type GUID.</param>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// This is essentially a temporary constructor. Once the constructor
        /// takes multiple parameters is removed, this constructor can be marked
        /// as obsolete and a new constructor that takes only a name parameter
        /// can be added to match the pattern of all other field attributes.
        /// We can't go directly to a single name parameter because it would
        /// conflict with the original constructor that takes the workflow type
        /// guid as the first parameter.
        /// </remarks>
        public WorkflowFieldAttribute( string workflowTypeGuid, string name )
            : base( SystemGuid.FieldType.WORKFLOW.AsGuid(), name )
        {
            WorkflowTypeGuid = workflowTypeGuid;
        }

        /// <summary>
        /// The unique identifier of the workflow type that should be used when
        /// presenting the workflows to pick from.
        /// </summary>
        public string WorkflowTypeGuid
        {
            get
            {
                var configValue = FieldConfigurationValues.GetValueOrNull( "workflowtype" );

                if ( int.TryParse( configValue, out var id ) && RockApp.Current.IsDatabaseAvailable() )
                {
                    var workflowType = WorkflowTypeCache.Get( id );

                    if ( workflowType != null )
                    {
                        return workflowType.Guid.ToString();
                    }
                }

                return null;
            }
            set
            {
                if ( Guid.TryParse( value, out var guid ) && RockApp.Current.IsDatabaseAvailable() )
                {
                    var workflowType = WorkflowTypeCache.Get( guid );

                    if ( workflowType != null )
                    {
                        var configValue = new Field.ConfigurationValue( workflowType.Id.ToString() );
                        FieldConfigurationValues.AddOrReplace( "workflowtype", configValue );
                    }
                }
            }
        }
    }
}
