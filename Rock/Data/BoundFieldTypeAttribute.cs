using System;
using System.Web.UI.WebControls;

namespace Rock.Data
{
    /// <summary>
    /// Apply BoundFieldType to a property to specify a specific BoundField type to use when this value 
    /// is displayed in DataView and Report grids
    /// </summary>
    [AttributeUsage( AttributeTargets.Property )]
    public class BoundFieldTypeAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the type of the bound field.
        /// </summary>
        /// <value>
        /// The type of the bound field.
        /// </value>
        public Type BoundFieldType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundFieldTypeAttribute"/> class.
        /// </summary>
        /// <param name="boundFieldType">Type of the bound field.</param>
        /// <exception cref="Rock.Data.BoundFieldTypeException"></exception>
        public BoundFieldTypeAttribute( Type boundFieldType )
        {
            if ( !( typeof( BoundField ).IsAssignableFrom( boundFieldType ) ) )
            {
                throw new BoundFieldTypeException();
            }

            this.BoundFieldType = boundFieldType;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BoundFieldTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundFieldTypeException"/> class.
        /// </summary>
        public BoundFieldTypeException()
            : base( "boundFieldType must be a BoundField type" )
        {

        }
    }
}
