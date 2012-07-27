﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.Extension
{
    /// <summary>
    /// Helper class for wrapping the properties of a MEF class to use in databinding
    /// </summary>
    public class ComponentDescription
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ComponentDescription"/> is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentDescription"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="service">The service.</param>
        public ComponentDescription( int id, Rock.Attribute.IHasAttributes service )
        {
            Id = id;

            Type type = service.GetType();

            Name = type.Name;

            // Look for a DescriptionAttribute on the class and if found, use its value for the description
            // property of this class
            var descAttributes = type.GetCustomAttributes( typeof( System.ComponentModel.DescriptionAttribute ), false );
            if ( descAttributes != null )
                foreach ( System.ComponentModel.DescriptionAttribute descAttribute in descAttributes )
                    Description = descAttribute.Description;

            // If the class has an PropertyAttribute with 'Active' as the key get it's value for the property
            // otherwise default to true
            if ( service.AttributeValues.ContainsKey( "Active" ) )
                IsActive = bool.Parse( service.AttributeValues["Active"].Value[0].Value );
            else
                IsActive = true;
        }
    }
}