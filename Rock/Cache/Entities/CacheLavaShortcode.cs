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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Rock.Data;
using Rock.Model;

namespace Rock.Cache
{
    /// <summary>
    /// Information about a Lava shortcode that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class CacheLavaShortcode : ModelCache<CacheLavaShortcode, LavaShortcode>
    {

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

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
        /// Gets or sets the documentation.
        /// </summary>
        /// <value>
        /// The documentation.
        /// </value>
        [DataMember]
        public string Documentation { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        /// <value>
        /// The name of the tag.
        /// </value>
        [DataMember]
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets the markup.
        /// </summary>
        /// <value>
        /// The markup.
        /// </value>
        [DataMember]
        public string Markup { get; set; }

        /// <summary>
        /// Gets or sets the type of the tag.
        /// </summary>
        /// <value>
        /// The type of the tag.
        /// </value>
        [DataMember]
        public TagType TagType { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        [DataMember]
        public string Parameters { get; set; }

        /// <summary>
        /// Gets or sets the enabled lava commands.
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        [DataMember]
        public string EnabledLavaCommands { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var shortcode = entity as LavaShortcode;
            if ( shortcode == null ) return;

            IsSystem = shortcode.IsSystem;
            Name = shortcode.Name;
            Description = shortcode.Description;
            IsActive = shortcode.IsActive;
            Documentation = shortcode.Documentation;
            TagName = shortcode.TagName;
            Markup = shortcode.Markup;
            TagType = shortcode.TagType;
            Parameters = shortcode.Parameters;
            EnabledLavaCommands = shortcode.EnabledLavaCommands;
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

        #region Static Methods

        public new static CacheLavaShortcode Get(string tagName)
        {
            return Get(tagName, null);
        }

        public static CacheLavaShortcode Get(string tagName, RockContext rockContext)
        {
            return tagName.IsNotNullOrWhitespace()
                ? GetOrAddExisting(tagName, () => QueryDbByTagName(tagName, rockContext)) : null;
        }

        private static CacheLavaShortcode QueryDbByTagName( string tagName, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return QueryDbByTagNamebWithContext( tagName, rockContext );
            }

            using ( var newRockContext = new RockContext() )
            {
                return QueryDbByTagNamebWithContext( tagName, newRockContext );
            }
        }

        /// <summary>
        /// Queries the database by id with context.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static CacheLavaShortcode QueryDbByTagNamebWithContext( string tagName, RockContext rockContext )
        {
            var service = new LavaShortcodeService( rockContext );
            var entity = service.Queryable().AsNoTracking(  )
                .FirstOrDefault(c => c.TagName == tagName);

            if ( entity == null ) return null;

            var value = new CacheLavaShortcode();
            value.SetFromEntity( entity );
            return value;
        }

        /// <summary>
        /// Returns all Lava shortcodes
        /// </summary>
        /// <returns></returns>
        public static List<CacheLavaShortcode> AllActive()
        {
            return All().Where( s => s.IsActive ).ToList();
        }

        /// <summary>
        /// Removes Lava shortcode from cache
        /// </summary>
        /// <param name="id"></param>
        public new static void Remove( int id )
        {
            Remove( id.ToString() );

            // some of the cached lavatemplates might have a reference to this shortcode, so flush them all just in case
            CacheLavaTemplate.Clear();
        }

        #endregion
    }
}