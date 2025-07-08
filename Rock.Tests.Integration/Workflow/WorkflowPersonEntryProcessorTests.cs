using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Workflow;
using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.Tests.Integration.Workflow
{
    [TestClass]
    public class WorkflowPersonEntryProcessorTests : DatabaseTestsBase
    {
        private const int DatabaseWorkflowTypeId = 1_000;

        [TestMethod]
        [IsolatedTestDatabase]
        public void SameNameOfLoggedInPersonAndSpouse_UsesExistingPeople()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var config = PrepareTest( rockContext );

                var personEntryValues = new PersonEntryValuesBag
                {
                    Person = new PersonBasicEditorBag
                    {
                        FirstName = "Ted",
                        LastName = "Decker",
                        Email = "ted@rocksolidchurchdemo.com"
                    },
                    Spouse = new PersonBasicEditorBag
                    {
                        FirstName = "Cynthia",
                        LastName = "Decker",
                        Email = "cindy@fakeinbox.com"
                    }
                };

                var processor = new WorkflowPersonEntryProcessor( config.Action, rockContext );

                var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker );
                var cindyDecker = personService.Get( TestGuids.TestPeople.CindyDecker );

                processor.SetFormPersonEntryValues( config.Form, tedDecker.Id, personEntryValues, tedDecker, cindyDecker );

                var personAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.PersonAttribute.Key ).AsGuid();
                var spouseAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.SpouseAttribute.Key ).AsGuid();
                var formPerson = personAliasService.GetPerson( personAliasGuid );
                var formSpouse = personAliasService.GetPerson( spouseAliasGuid );

                // The specific test, is that Ted Decker is logged in. In the UI
                // we don't change anything. Ted and Cindy should be used.
                Assert.That.AreEqual( tedDecker.Id, formPerson.Id );
                Assert.That.AreEqual( cindyDecker.Id, formSpouse.Id );
                Assert.That.AreEqual( tedDecker.PrimaryFamilyId.Value, formPerson.PrimaryFamilyId.Value );
                Assert.That.AreEqual( tedDecker.PrimaryFamilyId.Value, formSpouse.PrimaryFamilyId.Value );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void MatchedNameOfPersonAndSpouse_UsesExistingPeople()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var config = PrepareTest( rockContext );

                var personEntryValues = new PersonEntryValuesBag
                {
                    Person = new PersonBasicEditorBag
                    {
                        FirstName = "Ted",
                        LastName = "Decker",
                        Email = "ted@rocksolidchurchdemo.com"
                    },
                    Spouse = new PersonBasicEditorBag
                    {
                        FirstName = "Cynthia",
                        LastName = "Decker",
                        Email = "cindy@fakeinbox.com"
                    }
                };

                var processor = new WorkflowPersonEntryProcessor( config.Action, rockContext );

                var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker );
                var cindyDecker = personService.Get( TestGuids.TestPeople.CindyDecker );

                processor.SetFormPersonEntryValues( config.Form, tedDecker.Id, personEntryValues, null, null );

                var personAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.PersonAttribute.Key ).AsGuid();
                var spouseAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.SpouseAttribute.Key ).AsGuid();
                var formPerson = personAliasService.GetPerson( personAliasGuid );
                var formSpouse = personAliasService.GetPerson( spouseAliasGuid );

                // The specific test, nobody is logged in. In the UI we fill out
                // information for Ted and Cindy Decker. Ted and Cindy should be
                // matched and used.
                Assert.That.AreEqual( tedDecker.Id, formPerson.Id );
                Assert.That.AreEqual( cindyDecker.Id, formSpouse.Id );
                Assert.That.AreEqual( tedDecker.PrimaryFamilyId.Value, formPerson.PrimaryFamilyId.Value );
                Assert.That.AreEqual( tedDecker.PrimaryFamilyId.Value, formSpouse.PrimaryFamilyId.Value );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void ChangingNameOfLoggedInPerson_CreatesNewPersonAndSpouseInNewFamily()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var config = PrepareTest( rockContext );

                var personEntryValues = new PersonEntryValuesBag
                {
                    Person = new PersonBasicEditorBag
                    {
                        FirstName = "Scott",
                        LastName = "Smith",
                        Email = "ssmith@rocksolidchurchdemo.com"
                    },
                    Spouse = new PersonBasicEditorBag
                    {
                        FirstName = "Cynthia",
                        LastName = "Decker",
                        Email = "cindy@fakeinbox.com"
                    }
                };

                var processor = new WorkflowPersonEntryProcessor( config.Action, rockContext );

                var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker );
                var cindyDecker = personService.Get( TestGuids.TestPeople.CindyDecker );

                processor.SetFormPersonEntryValues( config.Form, tedDecker.Id, personEntryValues, tedDecker, cindyDecker );

                var personAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.PersonAttribute.Key ).AsGuid();
                var spouseAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.SpouseAttribute.Key ).AsGuid();
                var formPerson = personAliasService.GetPerson( personAliasGuid );
                var formSpouse = personAliasService.GetPerson( spouseAliasGuid );

                // The specific test, is that Ted Decker is logged in. In the UI
                // we change his name to Scott Smith, but leave the spouse alone.
                // Scott Smith should be created as a new person in a new family.
                // The existing Cindy Decker record is left intact, no new spouse
                // is created for Scott.
                //
                // NOTE: We kno wthis might not be the ideal behavior, but it is
                // the best we could come up with and mirrors the legacy behavior.
                // Any change to this behavior should be reviewed carefully.
                Assert.That.AreNotEqual( tedDecker.Id, formPerson.Id );
                Assert.That.AreEqual( cindyDecker.Id, formSpouse.Id );
                Assert.That.AreNotEqual( tedDecker.PrimaryFamilyId.Value, formPerson.PrimaryFamilyId.Value );
                Assert.That.AreEqual( tedDecker.PrimaryFamilyId.Value, formSpouse.PrimaryFamilyId.Value );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void ChangingNameOfLoggedInSpouse_CreatesNewSpouseInSameFamily()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var config = PrepareTest( rockContext );

                var personEntryValues = new PersonEntryValuesBag
                {
                    Person = new PersonBasicEditorBag
                    {
                        FirstName = "Ted",
                        LastName = "Decker",
                        Email = "ted@rocksolidchurchdemo.com"
                    },
                    Spouse = new PersonBasicEditorBag
                    {
                        FirstName = "Sandy",
                        LastName = "Smith",
                        Email = "sandy@fakeinbox.com"
                    }
                };

                var processor = new WorkflowPersonEntryProcessor( config.Action, rockContext );

                var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker );
                var cindyDecker = personService.Get( TestGuids.TestPeople.CindyDecker );

                processor.SetFormPersonEntryValues( config.Form, tedDecker.Id, personEntryValues, tedDecker, cindyDecker );

                var personAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.PersonAttribute.Key ).AsGuid();
                var spouseAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.SpouseAttribute.Key ).AsGuid();
                var formPerson = personAliasService.GetPerson( personAliasGuid );
                var formSpouse = personAliasService.GetPerson( spouseAliasGuid );

                // The specific test, is that Ted Decker is logged in. In the UI
                // we change the spouse to Sandy Smith. Ted and Cindy's records
                // should be left alone, but Sandy should be created as a new record
                // in the same family as Ted and Cindy.
                //
                // NOTE: We kno wthis might not be the ideal behavior, but it is
                // the best we could come up with and mirrors the legacy behavior.
                // Any change to this behavior should be reviewed carefully.
                Assert.That.AreEqual( tedDecker.Id, formPerson.Id );
                Assert.That.AreNotEqual( cindyDecker.Id, formSpouse.Id );
                Assert.That.AreEqual( tedDecker.PrimaryFamilyId.Value, formPerson.PrimaryFamilyId.Value );
                Assert.That.AreEqual( tedDecker.PrimaryFamilyId.Value, formSpouse.PrimaryFamilyId.Value );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void ChangingNameOfLoggedInPersonToMatchedPerson_CreatesNewSpouseInMatchedFamily()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var config = PrepareTest( rockContext );

                var personEntryValues = new PersonEntryValuesBag
                {
                    Person = new PersonBasicEditorBag
                    {
                        FirstName = "Bill",
                        LastName = "Marble",
                        Email = "bill.marble@fakeinbox.com"
                    },
                    Spouse = new PersonBasicEditorBag
                    {
                        FirstName = "Cynthia",
                        LastName = "Decker",
                        Email = "cindy@fakeinbox.com"
                    }
                };

                var processor = new WorkflowPersonEntryProcessor( config.Action, rockContext );

                var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker );
                var cindyDecker = personService.Get( TestGuids.TestPeople.CindyDecker );
                var billMarble = personService.Get( TestGuids.TestPeople.BillMarble );
                var alishaMarble = personService.Get( TestGuids.TestPeople.AlishaMarble );

                processor.SetFormPersonEntryValues( config.Form, tedDecker.Id, personEntryValues, tedDecker, cindyDecker );

                var personAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.PersonAttribute.Key ).AsGuid();
                var spouseAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.SpouseAttribute.Key ).AsGuid();
                var formPerson = personAliasService.GetPerson( personAliasGuid );
                var formSpouse = personAliasService.GetPerson( spouseAliasGuid );

                // The specific test, is that Ted Decker is logged in. In the UI
                // we change his name to Bill Marble, but leave the spouse alone.
                // We expect that Ted and Cindy are not modified. Bill is matched
                // to the existing Bill, and then a new spouse is created for him.
                // This results in 3 adults in the Marble family.
                //
                // NOTE: We kno wthis might not be the ideal behavior, but it is
                // the best we could come up with and mirrors the legacy behavior.
                // Any change to this behavior should be reviewed carefully.
                Assert.That.AreEqual( billMarble.Id, formPerson.Id );
                Assert.That.AreNotEqual( cindyDecker.Id, formSpouse.Id );
                Assert.That.AreNotEqual( alishaMarble.Id, formSpouse.Id );
                Assert.That.AreEqual( billMarble.PrimaryFamilyId.Value, formPerson.PrimaryFamilyId.Value );
                Assert.That.AreEqual( billMarble.PrimaryFamilyId.Value, formSpouse.PrimaryFamilyId.Value );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void ChangingNameOfLoggedInSpouseToMatchedPerson_CreatesNewSpouseInOriginalFamily()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var config = PrepareTest( rockContext );

                var personEntryValues = new PersonEntryValuesBag
                {
                    Person = new PersonBasicEditorBag
                    {
                        FirstName = "Ted",
                        LastName = "Decker",
                        Email = "ted@rocksolidchurchdemo.com"
                    },
                    Spouse = new PersonBasicEditorBag
                    {
                        FirstName = "Alisha",
                        LastName = "Marble",
                        Email = "alisha.marble@rocksolidchurchdemo.com"
                    }
                };

                var processor = new WorkflowPersonEntryProcessor( config.Action, rockContext );

                var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker );
                var cindyDecker = personService.Get( TestGuids.TestPeople.CindyDecker );
                var alishaMarble = personService.Get( TestGuids.TestPeople.AlishaMarble );

                processor.SetFormPersonEntryValues( config.Form, tedDecker.Id, personEntryValues, tedDecker, cindyDecker );

                var personAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.PersonAttribute.Key ).AsGuid();
                var spouseAliasGuid = config.Action.Activity.Workflow.GetAttributeValue( config.SpouseAttribute.Key ).AsGuid();
                var formPerson = personAliasService.GetPerson( personAliasGuid );
                var formSpouse = personAliasService.GetPerson( spouseAliasGuid );

                // The specific test, is that Ted Decker is logged in. In the UI
                // we change the spouse to Alisha Marble. Ted, Cindy and Alisha's
                // records should be left alone. A new Alisha Marble should be
                // created, in the same family as Ted and Cindy.
                //
                // NOTE: We kno wthis might not be the ideal behavior, but it is
                // the best we could come up with and mirrors the legacy behavior.
                // Any change to this behavior should be reviewed carefully.
                Assert.That.AreEqual( tedDecker.Id, formPerson.Id );
                Assert.That.AreNotEqual( cindyDecker.Id, formSpouse.Id );
                Assert.That.AreNotEqual( alishaMarble.Id, formSpouse.Id );
                Assert.That.AreEqual( tedDecker.PrimaryFamilyId.Value, formPerson.PrimaryFamilyId.Value );
                Assert.That.AreEqual( tedDecker.PrimaryFamilyId.Value, formSpouse.PrimaryFamilyId.Value );
            }
        }

        private static TestConfiguration PrepareTest( RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );

            var personFormAttribute = new Model.Attribute
            {
                EntityTypeId = EntityTypeCache.Get<Model.Workflow>( true, rockContext ).Id,
                FieldTypeId = 1,
                Name = "Test Person",
                Key = "TestPerson",
                EntityTypeQualifierColumn = "WorkflowTypeId",
                EntityTypeQualifierValue = DatabaseWorkflowTypeId.ToString()
            };

            var spouseFormAttribute = new Model.Attribute
            {
                EntityTypeId = EntityTypeCache.Get<Model.Workflow>( true, rockContext ).Id,
                FieldTypeId = 1,
                Name = "Test Spouse",
                Key = "TestSpouse",
                EntityTypeQualifierColumn = "WorkflowTypeId",
                EntityTypeQualifierValue = DatabaseWorkflowTypeId.ToString()
            };

            attributeService.Add( personFormAttribute );
            attributeService.Add( spouseFormAttribute );

            rockContext.SaveChanges();

            var form = new WorkflowActionForm
            {
                PersonEntryPersonAttributeGuid = personFormAttribute.Guid,
                PersonEntrySpouseAttributeGuid = spouseFormAttribute.Guid,
                PersonEntryAutofillCurrentPerson = true,
                PersonEntryRecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid(), rockContext ).Id,
                PersonEntryConnectionStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT.AsGuid(), rockContext ).Id,
                PersonEntryCampusIsVisible = true,
                PersonEntryGenderEntryOption = WorkflowActionFormPersonEntryOption.Required,
                PersonEntryEmailEntryOption = WorkflowActionFormPersonEntryOption.Required,
                PersonEntrySpouseLabel = "Spouse",
                PersonEntrySpouseEntryOption = WorkflowActionFormPersonEntryOption.Required,
            };

            var formCache = new WorkflowActionFormCache();
            formCache.SetFromEntity( form );

            var action = new WorkflowAction
            {
                Activity = new WorkflowActivity
                {
                    Workflow = new Model.Workflow
                    {
                        WorkflowTypeId = DatabaseWorkflowTypeId
                    }
                }
            };
            action.Activity.Workflow.LoadAttributes( rockContext );

            return new TestConfiguration
            {
                PersonAttribute = personFormAttribute,
                SpouseAttribute = spouseFormAttribute,
                Action = action,
                Form = formCache
            };
        }

        private class TestConfiguration
        {
            public Model.Attribute PersonAttribute { get; set; }

            public Model.Attribute SpouseAttribute { get; set; }

            public WorkflowAction Action { get; set; }

            public WorkflowActionFormCache Form { get; set; }
        }
    }
}
