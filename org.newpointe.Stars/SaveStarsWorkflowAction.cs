using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using org.newpointe.Stars.Data;
using org.newpointe.Stars.Model;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock;

namespace org.newpointe.Stars
{
    [ActionCategory("Stars")]
    [Description( "Save a Stars to a person's record" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Save Stars" )]

    [WorkflowAttribute("Person", "Workflow attribute that contains the person to update.", true, "", "", 0, null, new string[] { "Rock.Field.Types.PersonFieldType" })]
    [DefinedValueField(SystemGuid.DefinedType.STARS_TYPE, "Type", "The type of item the starts are for (to determine value)", true, false, SystemGuid.DefinedType.STARS_TYPE_VERSE, "", 2)]


    class SaveStarsWorkflowAction : ActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // Get person
            Person person = null;
            int personAliasId = 1;

            string personAttributeValue = GetAttributeValue(action, "Person");
            Guid? guidPersonAttribute = personAttributeValue.AsGuidOrNull();
            if (guidPersonAttribute.HasValue)
            {
                var attributePerson = AttributeCache.Read(guidPersonAttribute.Value, rockContext);
                if (attributePerson != null || attributePerson.FieldType.Class != "Rock.Field.Types.PersonFieldType")
                {
                    string attributePersonValue = action.GetWorklowAttributeValue(guidPersonAttribute.Value);
                    if (!string.IsNullOrWhiteSpace(attributePersonValue))
                    {
                        Guid personAliasGuid = attributePersonValue.AsGuid();
                        if (!personAliasGuid.IsEmpty())
                        {
                            person = new PersonAliasService(rockContext).Queryable()
                                .Where(a => a.Guid.Equals(personAliasGuid))
                                .Select(a => a.Person)
                                .FirstOrDefault();
                            if (person == null)
                            {
                                errorMessages.Add(string.Format("Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString()));
                                return false;
                            }
                        }
                    }
                }
            }

           if (person == null)
            {
                errorMessages.Add("The attribute used to provide the person was invalid, or not of type 'Person'.");
                return false;
            }

            if (person != null)
            {
                PersonAliasService personAliasService = new PersonAliasService(rockContext);
                personAliasId = person.PrimaryAliasId ?? default(int);
            }

            

            //Get DateTime
            DateTime currentDateTime =  DateTime.Now;



            //Get Stars Value
            DefinedValueService definedValueService = new DefinedValueService(rockContext);

            var type = GetAttributeValue(action, "Type");
            Guid typeGuid = type.AsGuid();

            var definedValue = definedValueService.GetByGuid(typeGuid);
            definedValue.LoadAttributes();
            var value = definedValue.GetAttributeValue("StarValue");
            var starsValue = Convert.ToDecimal(value);


            //Save Stars
            SaveStars(currentDateTime, personAliasId, starsValue, definedValue.Value);



            return true;
        }




        public void SaveStars(DateTime dt, int paId, decimal starsValue, string note)
        {
            StarsProjectContext starsProjectContext = new StarsProjectContext();
            StarsService starsService = new StarsService(starsProjectContext);
            org.newpointe.Stars.Model.Stars stars = new org.newpointe.Stars.Model.Stars();

            PersonAliasService personAliasService = new PersonAliasService(new RockContext());
            int campusId = personAliasService.GetByAliasId(paId).Person.GetCampus().Id;


            stars.PersonAliasId = paId;
            stars.CampusId = campusId;
            stars.TransactionDateTime = DateTime.Now;
            stars.Value = starsValue;
            stars.Note = note;

            starsService.Add(stars);

            starsProjectContext.SaveChanges();

        }


    }
}
