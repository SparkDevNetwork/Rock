//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;

using Rock.Web.UI;

namespace Rock.Extension
    
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
    [BlockProperty( 0, "Order", "", "The order that this service should be used (priority)", false, "0" )]
    [BlockProperty( 0, "Active", "", "Should Service be used?", false, "False", "Rock", "Rock.Field.Types.Boolean" )]
    public abstract class Component : Rock.Attribute.IHasAttributes
        
        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id      get      return 0; } }

        /// <summary>
        /// List of attributes associated with the object grouped by category.  This property will not include
        /// the attribute values. The <see cref="AttributeValues"/> property should be used to get attribute values
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public SortedDictionary<string, List<Rock.Web.Cache.AttributeCache>> Attributes      get; set; }

        /// <summary>
        /// Dictionary of all attributes and their values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, KeyValuePair<string, List<Rock.Core.AttributeValueDto>>> AttributeValues      get; set; }

        /// <summary>
        /// Gets the first value for an Attributes
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string AttributeValue( string key )
            
            if ( this.AttributeValues != null &&
                this.AttributeValues.ContainsKey( key ) )
                return this.AttributeValues[key].Value[0].Value;

            return null;
        }


        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order
            
            get
                
                int order = 0;
                if (!AttributeValues.ContainsKey( "Order" ) || !( Int32.TryParse( AttributeValues["Order"].Value[0].Value, out order ) ) )
                    
                    foreach(var attributeCategory in Attributes)
                        
                        foreach(var attribute in attributeCategory.Value)
                            
                            if ( attribute.Key == "Order" )
                                
                                if ( Int32.TryParse( attribute.DefaultValue, out order ) )
                                    return order;
                                else
                                    return 0;
                            }
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
            
            get
                
                bool isActive = false;
                if ( !AttributeValues.ContainsKey( "Active" ) || !( Boolean.TryParse( AttributeValues["Active"].Value[0].Value, out isActive ) ) )
                    
                    foreach ( var attributeCategory in Attributes )
                        
                        foreach ( var attribute in attributeCategory.Value )
                            
                            if ( attribute.Key == "Active" )
                                
                                if ( Boolean.TryParse( attribute.DefaultValue, out isActive ) )
                                    return isActive;
                                else
                                    return false;
                            }
                        }
                    }
                }
                return isActive;
            }
        }


        
        /// <summary>
        /// Initializes a new instance of the <see cref="Component"/> class.
        /// </summary>
        public Component()
            
            Rock.Attribute.Helper.LoadAttributes( this );
        }
    }
}