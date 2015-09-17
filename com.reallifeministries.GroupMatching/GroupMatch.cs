using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;
using System.Runtime.Serialization;
using System.Reflection;

using Rock.Data;



namespace com.reallifeministries.GroupMatching
{
    [DataContract]
    public class GroupMatch : Rock.Lava.ILiquidizable
    {
        [DataMember]
        public Group Group { get; set; }

        [DataMember]
        public Location Location { get; set; }

        [DataMember]
        public double? Distance { get; set; }

        [DataMember]
        public int MemberCount { get; set; }

        [DataMember]
        public Schedule Schedule { get; set; }

       

        #region iLiquidizable

        [LavaIgnore]
        public object ToLiquid()
        {
            return this;
        }

        /// <summary>
        /// Gets the available keys (for debuging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaIgnore]
        public virtual List<string> AvailableKeys
        {
            get
            {
                var availableKeys = new List<string>();

                foreach (var propInfo in GetType().GetProperties())
                {
                    if (propInfo != null && LiquidizableProperty( propInfo ))
                    {
                        availableKeys.Add( propInfo.Name );
                    }
                }

                return availableKeys;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [LavaIgnore]
        public virtual object this[object key]
        {
            get
            {
                var propInfo = GetType().GetProperty( key.ToString() );
                if (propInfo != null && LiquidizableProperty( propInfo ))
                {
                    try
                    {
                        object propValue = propInfo.GetValue( this, null );
                        if (propValue is Guid)
                        {
                            return ((Guid)propValue).ToString();
                        }
                        else
                        {
                            return propValue;
                        }
                    }
                    catch { }
                }

                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [LavaIgnore]
        public bool ContainsKey( object key )
        {
            var propInfo = GetType().GetProperty( key.ToString() );
            if (propInfo != null && LiquidizableProperty( propInfo ))
            {
                return true;
            }

            return false;
        }
        [LavaIgnore]
        private bool LiquidizableProperty( PropertyInfo propInfo )
        {
            // If property has a [LavaIgnore] attribute return false
            if (propInfo.GetCustomAttributes( typeof( Rock.Data.LavaIgnoreAttribute ) ).Count() > 0)
            {
                return false;
            }

            // If property has a [DataMember] attribute return true
            if (propInfo.GetCustomAttributes( typeof( System.Runtime.Serialization.DataMemberAttribute ) ).Count() > 0)
            {
                return true;
            }

            // If property has a [LavaInclude] attribute return true
            if (propInfo.GetCustomAttributes( typeof( Rock.Data.LavaIncludeAttribute ) ).Count() > 0)
            {
                return true;
            }

            // otherwise return false
            return false;

        }
        #endregion
    }
}
