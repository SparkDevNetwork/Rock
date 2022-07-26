using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

public class PersonAttributeValueCSVMapper
{
    public static List<Slingshot.Core.Model.PersonAttributeValue> Map( IDictionary<string, object> csvEntryLookup, Dictionary<string, string> csvHeaderMapper )
    {
        var personAttributeValues = new List<Slingshot.Core.Model.PersonAttributeValue>();

        string csvColumnId = csvHeaderMapper["Id"];
        int personId = csvEntryLookup[csvColumnId].ToIntSafe();

        IEnumerable<string> rockPersonAttributeKeys = AttributeCache.GetPersonAttributes()
            .Select( a => a.Name );

        foreach ( var rockPersonAttributeKey in rockPersonAttributeKeys )
        {
            if ( !csvHeaderMapper.TryGetValue( rockPersonAttributeKey, out string csvColumnRockAttributeKey ) )
            {
                continue;
            }
            string attributeValue = csvEntryLookup
                .GetValueOrNull( csvColumnRockAttributeKey )
                .ToStringSafe();

            var personAttributeValue = new Slingshot.Core.Model.PersonAttributeValue
            {
                AttributeKey = rockPersonAttributeKey,
                AttributeValue = attributeValue,
                PersonId = personId
            };
            personAttributeValues.Add( personAttributeValue );
        }

        return personAttributeValues;
    }
}