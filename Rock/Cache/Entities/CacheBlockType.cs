﻿// <copyright>
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
using System.Collections.Concurrent;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Cache
{
    /// <summary>
    /// Information about a block type that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class CacheBlockType : ModelCache<CacheBlockType, BlockType>
    {

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is common.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is common; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsCommon { get; private set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        [DataMember]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is instance properties verified.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is instance properties verified; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsInstancePropertiesVerified { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [checked security actions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [checked security actions]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CheckedSecurityActions { get; set; }

        /// <summary>
        /// Gets or sets the security actions.
        /// </summary>
        /// <value>
        /// The security actions.
        /// </value>
        [DataMember]
        public ConcurrentDictionary<string, string> SecurityActions { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the security actions.
        /// </summary>
        /// <param name="blockControl">The block control.</param>
        public void SetSecurityActions( Web.UI.RockBlock blockControl )
        {
            lock ( _obj )
            {
                if ( CheckedSecurityActions ) return;

                SecurityActions = new ConcurrentDictionary<string, string>();
                foreach ( var action in blockControl.GetSecurityActionAttributes() )
                {
                    SecurityActions.TryAdd( action.Key, action.Value );
                }
                CheckedSecurityActions = true;
            }
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var blockType = entity as BlockType;
            if ( blockType == null ) return;

            IsSystem = blockType.IsSystem;
            IsCommon = blockType.IsCommon;
            Path = blockType.Path;
            Name = blockType.Name;
            Description = blockType.Description;
            IsInstancePropertiesVerified = false;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

    }
}