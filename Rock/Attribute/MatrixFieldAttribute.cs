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
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Type to select a Matrix 
    /// Value stored as AttributeMatrix.Guid
    /// </summary>
    public class MatrixFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixFieldAttribute" /> class.
        /// </summary>
        /// <param name="attributeMatrixTemplateGuid">The attribute matrix template unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public MatrixFieldAttribute( string attributeMatrixTemplateGuid, string name, string description = "", bool required = true, string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.MATRIX.AsGuid(), name, description, required, null, category, order, key )
        {
            var attributeMatrixTemplate = new AttributeMatrixTemplateService( new Data.RockContext() ).Get( attributeMatrixTemplateGuid.AsGuid() );
            if ( attributeMatrixTemplate != null )
            {
                FieldConfigurationValues.Add( "attributematrixtemplate", new Field.ConfigurationValue( attributeMatrixTemplate.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MatrixFieldAttribute( string name )
            : base( SystemGuid.FieldType.MATRIX.AsGuid(), name )
        {
        }

        /// <summary>
        /// The unique identifier of the Attribute Matrix Template that should
        /// be used for this field.
        /// </summary>
        public string AttributeMatrixTemplateGuid
        {
            get
            {
                var configValue = FieldConfigurationValues.GetValueOrNull( "attributematrixtemplate" );

                if ( int.TryParse( configValue, out var id ) && RockApp.Current.IsDatabaseAvailable() )
                {
                    using var rockContext = RockApp.Current.CreateRockContext();
                    var attributeMatrixTemplate = new AttributeMatrixTemplateService( rockContext ).Get( id );

                    if ( attributeMatrixTemplate != null )
                    {
                        return attributeMatrixTemplate.Guid.ToString();
                    }
                }

                return null;
            }
            set
            {
                if ( Guid.TryParse( value, out var guid ) && RockApp.Current.IsDatabaseAvailable() )
                {
                    using var rockContext = RockApp.Current.CreateRockContext();
                    var attributeMatrixTemplate = new AttributeMatrixTemplateService( rockContext ).Get( guid );

                    if ( attributeMatrixTemplate != null )
                    {
                        var configValue = new Field.ConfigurationValue( attributeMatrixTemplate.Id.ToString() );
                        FieldConfigurationValues.Add( "attributematrixtemplate", configValue );
                    }
                }
            }
        }
    }
}
