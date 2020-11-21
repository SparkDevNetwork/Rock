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
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a Layout that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class LayoutCache : ModelCache<LayoutCache, Layout>
    {

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the site id.
        /// </summary>
        /// <value>
        /// The site id.
        /// </value>
        [DataMember]
        public int SiteId { get; private set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        [DataMember]
        public string FileName { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the layout mobile tablet.
        /// </summary>
        /// <value>
        /// The layout mobile tablet.
        /// </value>
        [DataMember]
        public string LayoutMobilePhone { get; private set; }

        /// <summary>
        /// Gets or sets the layout mobile tablet.
        /// </summary>
        /// <value>
        /// The layout mobile tablet.
        /// </value>
        [DataMember]
        public string LayoutMobileTablet { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the site.
        /// </summary>
        /// <value>
        /// The site.
        /// </value>
        public SiteCache Site => SiteCache.Get( SiteId );

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority => Site;

        #endregion

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var layout = entity as Layout;
            if ( layout == null ) return;

            IsSystem = layout.IsSystem;
            SiteId = layout.SiteId;
            FileName = layout.FileName;
            Name = layout.Name;
            LayoutMobilePhone = layout.LayoutMobilePhone;
            LayoutMobileTablet = layout.LayoutMobileTablet;
            Description = layout.Description;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

    }
}