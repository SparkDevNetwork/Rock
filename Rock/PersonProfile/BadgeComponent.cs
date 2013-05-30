//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Web.UI;

using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.PersonProfile
{
    /// <summary>
    /// Base class for person profile badges
    /// </summary>
    public abstract class BadgeComponent : Component
    {
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "Active", "True" );
                defaults.Add( "Order", "0" );
                return defaults;
            }
        }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public EntityTypeCache EntityType
        {
            get
            {
                return EntityTypeCache.Read( this.GetType() );
            }
        }

        /// <summary>
        /// Gets or sets the parent person block.
        /// </summary>
        /// <value>
        /// The parent person block.
        /// </value>
        public PersonBlock ParentPersonBlock
        {
            get { return _parentPersonBlock; }
            set { _parentPersonBlock = value; }
        }
        private PersonBlock _parentPersonBlock;

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public virtual Person Person
        {
            get { return _person; }
            set { _person = value; }
        }
        private Person _person;

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeComponent" /> class.
        /// </summary>
        public BadgeComponent()
        {
            this.LoadAttributes();
        }

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public abstract void Render( HtmlTextWriter writer );

    }

}