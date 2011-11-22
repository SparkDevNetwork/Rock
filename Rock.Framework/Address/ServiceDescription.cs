using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Address
{
    public class ServiceDescription
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }

        public ServiceDescription( int id, Rock.Attribute.IHasAttributes service )
        {
            Id = id;

            Type type = service.GetType();

            Name = type.Name;

            var descAttributes = type.GetCustomAttributes( typeof( System.ComponentModel.DescriptionAttribute ), false );
            if ( descAttributes != null )
                foreach ( System.ComponentModel.DescriptionAttribute descAttribute in descAttributes )
                    Description = descAttribute.Description;

            if ( service.AttributeValues.ContainsKey( "Active" ) )
                Active = bool.Parse( service.AttributeValues["Active"].Value );
            else
                Active = true;
        }
    }
}