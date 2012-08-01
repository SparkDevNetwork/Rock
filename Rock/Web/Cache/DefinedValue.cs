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
    /// The value of an defined type
    /// </summary>
    [Serializable]
    public class DefinedValue
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
        /// Gets or sets the defined type Id.
        /// </summary>
        /// <value>
		/// Defined Type Id.
        /// </value>
        public int DefinedTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
		/// Name.
        /// </value>
        public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		public string Description { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValue"/> class.
        /// </summary>
        public DefinedValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValue"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public DefinedValue ( Rock.Core.DefinedValue model )
        {
			Id = model.Id;
			Guid = model.Guid;
			IsSystem = model.IsSystem;
			DefinedTypeId = model.DefinedTypeId;
			Order = model.Order;
			Name = model.Name;
			Description = model.Description;
        }
    }
}