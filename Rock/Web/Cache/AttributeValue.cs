//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Web.UI;

using Rock.Field;

namespace Rock.Web.Cache
{
    /// <summary>
    /// The value of an attribute
    /// </summary>
    [Serializable]
    public class AttributeValue
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Id.
        /// </summary>
        /// <value>
        /// Attribute Id.
        /// </value>
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the Entity Id.
        /// </summary>
        /// <value>
        /// Entity Id.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        /// <value>
        /// Value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValue"/> class.
        /// </summary>
        public AttributeValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValue"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public AttributeValue ( Rock.Core.AttributeValue model )
        {
            Id = model.Id;
            Value = model.Value;
            Guid = model.Guid;
            IsSystem = model.IsSystem;
            AttributeId = model.AttributeId;
            EntityId = model.EntityId;
            Order = model.Order;
        }
    }
}