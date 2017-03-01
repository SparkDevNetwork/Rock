
using System;
using Rock;
using Rock.Attribute;
using com.centralaz.RoomManagement.Model;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.centralaz.RoomManagement.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or more ReservationStatuses stored as a comma-delimited list of ReservationStatus.Guid
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class ReservationStatusesFieldAttribute : FieldAttribute
    {
        private const string INCLUDE_INACTIVE_KEY = "includeInactive";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationStatusesFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultReservationStatusGuids">The default reservation status guids.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public ReservationStatusesFieldAttribute( string name = "ReservationStatuses", string description = "", bool required = true, string defaultReservationStatusGuids = "", string category = "", int order = 0, string key = null, string fieldTypeAssembly = "com.centralaz.RoomManagement")
            : base( name, description, required, defaultReservationStatusGuids, category, order, key, typeof( com.centralaz.RoomManagement.Field.Types.ReservationStatusesFieldType ).FullName, fieldTypeAssembly )
        {
            var includeInactiveConfigValue = new Rock.Field.ConfigurationValue( "False" );
            FieldConfigurationValues.Add( INCLUDE_INACTIVE_KEY, includeInactiveConfigValue );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationStatusesFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultReservationStatusGuids">The default reservation status guids.</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public ReservationStatusesFieldAttribute( string name = "ReservationStatuses", string description = "", bool required = true, string defaultReservationStatusGuids = "", bool includeInactive = false, string category = "", int order = 0, string key = null, string fieldTypeAssembly = "com.centralaz.RoomManagement" )
            : base( name, description, required, defaultReservationStatusGuids, category, order, key, typeof( com.centralaz.RoomManagement.Field.Types.ReservationStatusesFieldType ).FullName, fieldTypeAssembly )
        {
            var includeInactiveConfigValue = new Rock.Field.ConfigurationValue( includeInactive.ToString() );
            FieldConfigurationValues.Add( INCLUDE_INACTIVE_KEY, includeInactiveConfigValue );
        }
    }
}