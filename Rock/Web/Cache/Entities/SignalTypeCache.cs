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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a signal type that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class SignalTypeCache : ModelCache<SignalTypeCache, SignalType>
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name of the SignalType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the SignalType name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the HTML color of the SignalType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the SignalType color.
        /// </value>
        [DataMember]
        public string SignalColor { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class of the SignalType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the SignalType icon class.
        /// </value>
        [DataMember]
        public string SignalIconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var signalType = entity as SignalType;
            if ( signalType == null ) return;

            Name = signalType.Name;
            Description = signalType.Description;
            SignalColor = signalType.SignalColor;
            SignalIconCssClass = signalType.SignalIconCssClass;
            Order = signalType.Order;
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