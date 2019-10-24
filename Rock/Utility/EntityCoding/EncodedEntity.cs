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
using System.Collections.Generic;

using Rock.Data;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// Describes an Entity object in a portable manner that can be used
    /// to re-create the entity on another Rock installation.
    /// </summary>
    public class EncodedEntity
    {
        #region Properties

        /// <summary>
        /// The entity class name that we are describing.
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// The guid to use to check if this entity already exists.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Specifies if this entity should be given a new Guid during import.
        /// </summary>
        public bool GenerateNewGuid { get; set; }

        /// <summary>
        /// The values that describe the entities properties.
        /// </summary>
        public Dictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// Any processor transform data that is needed to re-create the entity.
        /// </summary>
        public Dictionary<string, object> Transforms { get; private set; }

        /// <summary>
        /// List of references that will be used to re-create inter-entity references.
        /// </summary>
        public List<Reference> References { get; private set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Create a new instance of an encoded entity.
        /// </summary>
        public EncodedEntity()
        {
            Properties = new Dictionary<string, object>();
            Transforms = new Dictionary<string, object>();
            References = new List<Reference>();
        }

        /// <summary>
        /// Replace a "by id" property that references another entity with a reference
        /// object that contains the information we will need to re-create that property
        /// at import time.
        /// </summary>
        /// <param name="originalProperty">The original property name that we are replacing.</param>
        /// <param name="entity">The entity that is being referenced.</param>
        public void MakePropertyIntoReference( string originalProperty, IEntity entity )
        {
            Properties.Remove( originalProperty );

            if ( entity != null )
            {
                Reference reference = new Reference( entity, originalProperty );

                References.Add( reference );
                Properties.Remove( originalProperty );
            }
        }

        /// <summary>
        /// Get the transform data value for the given processor.
        /// </summary>
        /// <param name="name">The full class name of the processor.</param>
        /// <returns>An object containing the data for the processor, or null if none was found.</returns>
        public object GetTransformData( string name )
        {
            if ( Transforms.ContainsKey( name ) )
            {
                return Transforms[name];
            }

            return null;
        }

        /// <summary>
        /// Add a new transform object to this encoded entity. These are used by EntityProcessor
        /// implementations to facilitate in exporting and imported complex entities that need
        /// a little extra customization done to them.
        /// </summary>
        /// <param name="name">The name of the transform, this is the full class name of the processor.</param>
        /// <param name="value">The black box value for the transform.</param>
        public void AddTransformData( string name, object value )
        {
            Transforms.Add( name, value );
        }

        #endregion
    }
}
