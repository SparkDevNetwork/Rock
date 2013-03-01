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
    [IntegerField( "Order", "The order that this service should be used (priority)" )]
    [BooleanField( "Active", "Should Service be used?", false, "", 0)]
    public abstract class Component : Rock.Attribute.IHasAttributes, Rock.Security.ISecured
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
        public Dictionary<string, List<string>> AttributeCategories { get; set; }

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
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public virtual Dictionary<string, string> AttributeValueDefaults
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the first value of an attribute key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( string key )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) &&
                this.AttributeValues[key].Count > 0 )
            {
                return this.AttributeValues[key][0].Value;
            }
            return null;
        }

        /// <summary>
        /// Sets the first value of an attribute key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetAttributeValue( string key, string value )
        {
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) )
            {
                if ( this.AttributeValues[key].Count == 0 )
                {
                    this.AttributeValues[key].Add( new Rock.Model.AttributeValue() );
                }
                this.AttributeValues[key][0].Value = value;
            }
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

                string value = GetAttributeValue( "Order" );
                if ( Int32.TryParse( value, out order ) )
                {
                    return order;
                }

                if (Attributes.ContainsKey("Order"))
                {
                    if ( Int32.TryParse( Attributes["Order"].DefaultValue, out order ) )
                    {
                        return order;
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

                string value = GetAttributeValue("Active");
                if (value != null)
                {
                    if ( Boolean.TryParse( value, out isActive ) )
                    {
                        return isActive;
                    }
                }

                if (Attributes.ContainsKey("Active"))
                {
                    if ( Boolean.TryParse( Attributes["Active"].DefaultValue, out isActive ) )
                    {
                        return isActive;
                    }
                }
                return isActive;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Component"/> class.
        /// </summary>
        public Component()
        {
            this.LoadAttributes();
        }

        /// <summary>
        /// Gets the Entity Type ID for this entity.
        /// </summary>
        /// <value>
        /// The type id.
        /// </value>
        public int TypeId
        {
            get
            {
                // Read should never return null since it will create entity type if it doesn't exist
                return Rock.Web.Cache.EntityTypeCache.Read( this.GetType() ).Id;
            }
        }

        /// <summary>
        /// The auth entity. Classes that implement the <see cref="ISecured" /> interface should return
        /// a value that is unique across all <see cref="ISecured" /> classes.  Typically this is the
        /// qualified name of the class.
        /// </summary>
        public string TypeName
        {
            get { return this.GetType().FullName; }
        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public Security.ISecured ParentAuthority
        {
            get { return new Rock.Security.GlobalDefault(); }
        }

        /// <summary>
        /// A list of actions that this class supports.
        /// </summary>
        public List<string> SupportedActions
        {
            get { return new List<string>() { "View", "Administrate" }; }
        }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAuthorized( string action, Model.Person person )
        {
            return Security.Authorization.Authorized( this, action, person );
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public bool IsAllowedByDefault( string action )
        {
            return action == "View";
        }

        /// <summary>
        /// Determines whether the specified action is private (Only the current user has access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is private; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPrivate( string action, Model.Person person )
        {
            return Security.Authorization.IsPrivate( this, action, person );
        }

        /// <summary>
        /// Makes the action on the current entity private (Only the current user will have access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="personId">The current person id.</param>
        public void MakePrivate( string action, Model.Person person, int? personId )
        {
            Security.Authorization.MakePrivate( this, action, person, personId );
        }
    }
}