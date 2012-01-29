using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Extension
{
    /// <summary>
    /// Abstract class that custom component classes should derive from.  
    /// <example>
    /// The derived class should define the following type attributes
    /// </example>
    /// <code>
    ///     [Description("<i>description of component</i>")]
    ///     [Export( typeof( Component ) )]
    ///     [ExportMetadata( "ComponentName", "<i>Name of Component</i>" )]
    /// </code>
    /// <example>
    /// The derived class can also optionally define one or more property type attributes
    /// </example>
    /// <code>
    ///     [Rock.Attribute.Property( 1, "License Key", "The Required License Key" )]
    /// </code>
    /// <example>
    /// To get the value of a property, the derived class can use the AttributeValues property
    /// </example>
    /// <code>
    ///     string licenseKey = AttributeValues["LicenseKey"].Value;
    /// </code>
    /// </summary>
    [Rock.Attribute.Property( 0, "Order", "", "The order that this service should be used (priority)", false, "0" )]
    [Rock.Attribute.Property( 0, "Active", "", "Should Service be used?", false, "False", "Rock", "Rock.FieldTypes.Boolean" )]
    public abstract class Component : Rock.Attribute.IHasAttributes
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get { return 0; } }

        /// <summary>
        /// List of attributes associated with the object grouped by category.  This property will not include
        /// the attribute values. The <see cref="AttributeValues"/> property should be used to get attribute values
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public SortedDictionary<string, List<Rock.Web.Cache.Attribute>> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; set; }

        /// <summary>
        /// Gets the order.
        /// </summary>
        public int Order
        {
            get
            {
                int order = 0;
                if ( AttributeValues.ContainsKey( "Order" ) )
                    if ( !( Int32.TryParse( AttributeValues["Order"].Value, out order ) ) )
                        order = 0;
                return order;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Component"/> class.
        /// </summary>
        public Component()
        {
            Rock.Attribute.Helper.LoadAttributes( this );
        }
    }
}