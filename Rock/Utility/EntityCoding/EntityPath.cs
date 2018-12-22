using System.Collections.Generic;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// Describes the entity and property path used to reach this point in
    /// the entity tree. This is basically just a helper class to provide some
    /// extra methods we can use to simplify code.
    /// </summary>
    public class EntityPath : List<EntityPathComponent>
    {
        #region Instance Methods

        /// <summary>
        /// Create a duplicate copy of this entity path and return it.
        /// </summary>
        /// <returns>A duplicate of this entity path.</returns>
        public EntityPath Clone()
        {
            EntityPath path = new EntityPath();

            path.AddRange( this );

            return path;
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals( object obj )
        {
            return base.Equals( obj );
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The instance of this object that will be compared.</param>
        /// <param name="b">The string instance we are comparing to.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==( EntityPath a, string b )
        {
            return ( a.ToString() == b );
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The instance of this object that will be compared.</param>
        /// <param name="b">The string instance we are comparing to.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=( EntityPath a, string b )
        {
            return !( a == b );
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">The instance of this object that will is the left side of the operator.</param>
        /// <param name="b">The EntityPathComponent instance that is the right side of the operator.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static EntityPath operator +( EntityPath a, EntityPathComponent b )
        {
            EntityPath path = a.Clone();

            path.Add( b );

            return path;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string value = string.Empty;

            foreach ( var component in this )
            {
                if ( !string.IsNullOrEmpty( value ) )
                {
                    value = value + "." + component.PropertyName;
                }
                else
                {
                    value = component.PropertyName;
                }
            }

            return value;
        }

        #endregion
    }
}
