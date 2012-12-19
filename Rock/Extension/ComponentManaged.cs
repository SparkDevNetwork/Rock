//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using Rock.Attribute;
using Rock.Web.UI;

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
    [IntegerField( 0, "Order", "", null, "", "The order that this service should be used (priority)" )]
    [BooleanField( 0, "Active", false, null, "", "Should Service be used?")]
    public abstract class ComponentManaged : Rock.Attribute.IHasAttributes
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get { return 0; } }

         /// <summary>
        /// Dictionary of categorized attributes.  Key is the category name, and Value is list of attributes in the category
        /// </summary>
        /// <value>
        /// The attribute categories.
        /// </value>
        public SortedDictionary<string, List<string>> AttributeCategories { get; set; }

        /// <summary>
        /// List of attributes associated with the object.  This property will not include the attribute values.
        /// The <see cref="AttributeValues"/> property should be used to get attribute values.  Dictionary key
        /// is the attribute key, and value is the cached attribute
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<string, Rock.Web.Cache.AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.  Key is the attribute key, and value is the values
        /// associated with the attribute and object instance
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, List<Rock.Model.AttributeValue>> AttributeValues { get; set; }

        /// <summary>
        /// Gets the first value for an Attributes
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string AttributeValue( string key )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) )
                return this.AttributeValues[key][0].Value;

            return null;
        }


        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order
        {
            get
            {
                int order = 0;
                if (!AttributeValues.ContainsKey( "Order" ) || !( Int32.TryParse( AttributeValues["Order"][0].Value, out order ) ) )
                {
                    foreach(var attribute in Attributes)
                    {
                        if ( attribute.Key == "Order" )
                        {
                            if ( Int32.TryParse( attribute.Value.DefaultValue, out order ) )
                                return order;
                            else
                                return 0;
                        }
                    }
                }
                return order;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get
            {
                bool isActive = false;
                if ( !AttributeValues.ContainsKey( "Active" ) || !( Boolean.TryParse( AttributeValues["Active"][0].Value, out isActive ) ) )
                {
                    foreach ( var attribute in Attributes )
                    {
                        if ( attribute.Key == "Active" )
                        {
                            if ( Boolean.TryParse( attribute.Value.DefaultValue, out isActive ) )
                                return isActive;
                            else
                                return false;
                        }
                    }
                }
                return isActive;
            }
        }


        
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentManaged"/> class.
        /// </summary>
        public ComponentManaged()
        {
            this.LoadAttributes();
        }
    }
}