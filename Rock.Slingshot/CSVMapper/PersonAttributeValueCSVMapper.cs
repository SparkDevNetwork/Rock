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

        // This is duplicated from CSVImport.ascx.cs
        RockContext rockContext = new RockContext();
        int entityTypeIdPerson = EntityTypeCache.GetId<Person>().Value;
        AttributeService attributeService = new AttributeService( rockContext );

        string[] rockAttributeKeys = attributeService.GetByEntityTypeId( entityTypeIdPerson )
            .Select( a => a.Name )
            .ToArray();

        foreach ( var rockAttributeKey in rockAttributeKeys )
        {
            if ( !csvHeaderMapper.TryGetValue( rockAttributeKey, out string csvColumnRockAttributeKey ) )
            {
                continue;
            }
            string attributeValue = csvEntryLookup
                .GetValueOrNull( csvColumnRockAttributeKey )
                .ToStringSafe();

            var personAttributeValue = new Slingshot.Core.Model.PersonAttributeValue
            {
                AttributeKey = rockAttributeKey,
                AttributeValue = attributeValue,
                PersonId = personId
            };
            personAttributeValues.Add( personAttributeValue );
        }

        return personAttributeValues;
    }
}