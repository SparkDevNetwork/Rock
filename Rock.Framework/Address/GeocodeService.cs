using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Address
{
    [Rock.Attribute.Property( 0, "Order", "The order that this service should be used (priority)" )]
    [Rock.Attribute.Property( 0, "Active", "Active", "Should Service be used?", "False", "Rock.Framework", "Rock.FieldTypes.Boolean" )]
    public abstract class GeocodeService : Rock.Attribute.IHasAttributes
    {
        public int Id { get { return 0; } }
        public List<Rock.Cms.Cached.Attribute> Attributes { get; set; }
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; set; }

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

        public abstract bool Geocode( Rock.Models.Crm.Address address, out string result );

        public GeocodeService()
        {
            Rock.Attribute.Helper.LoadAttributes( this );
        }

    }


    public interface IGeocodeServiceData
    {
        string ServiceName { get; }
    }
}
