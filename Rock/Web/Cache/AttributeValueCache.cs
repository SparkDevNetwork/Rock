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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotLiquid;
using Newtonsoft.Json;

using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI.Controls;

namespace Rock.Web.Cache
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    [DotLiquid.LiquidType( "AttributeId", "Value", "ValueFormatted", "AttributeName", "AttributeKey" )]
    public class AttributeValueCache
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueCache"/> class.
        /// </summary>
        public AttributeValueCache()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueCache"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public AttributeValueCache( Rock.Model.AttributeValue model )
        {
            AttributeId = model.AttributeId;
            Value = model.Value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        [DataMember]
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Gets the value using the most appropriate datatype
        /// </summary>
        /// <value>
        /// The field type value.
        /// </value>
        public object ValueAsType
        {
            get
            {
                var attribute = AttributeCache.Read( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.FieldType.Field.ValueAsFieldType( null, Value, attribute.QualifierValues );
                }
                return Value;
            }
        }

        /// <summary>
        /// Get the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <value>
        /// The field type value.
        /// </value>
        public object SortValue
        {
            get
            {
                var attribute = AttributeCache.Read( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.FieldType.Field.SortValue( null, Value, attribute.QualifierValues );
                }

                return Value;
            }
        }

        /// <summary>
        /// Gets the value formatted.
        /// </summary>
        /// <value>
        /// The value formatted.
        /// </value>
        [LavaInclude]
        public virtual string ValueFormatted
        {
            get
            {
                var attribute = AttributeCache.Read( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.FieldType.Field.FormatValue( null, Value, attribute.QualifierValues, false );
                }
                return Value;
            }
        }

        /// <summary>
        /// Gets the name of the attribute 
        /// </summary>
        /// <remarks>
        /// Note: this property is provided specifically for Lava templates when the Attribute property is not available
        /// as a navigable property
        /// </remarks>
        /// <value>
        /// The name of the attribute.
        /// </value>
        [LavaInclude]
        public virtual string AttributeName
        {
            get
            {
                var attribute = AttributeCache.Read( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.Name;
                }
                return Value;
            }
        }

        /// <summary>
        /// Gets the attribute key.
        /// </summary>
        /// <remarks>
        /// Note: this property is provided specifically for Lava templates when the Attribute property is not available
        /// as a navigable property
        /// </remarks>
        /// <value>
        /// The attribute key.
        /// </value>
        [LavaInclude]
        public virtual string AttributeKey
        {
            get
            {
                var attribute = AttributeCache.Read( this.AttributeId );
                if ( attribute != null )
                {
                    return attribute.Key;
                }
                return Value;
            }
        }

        /// <summary>
        /// Returns the Formatted Value of this Attribute Value
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.ValueFormatted;
        }

        #endregion

    }
}