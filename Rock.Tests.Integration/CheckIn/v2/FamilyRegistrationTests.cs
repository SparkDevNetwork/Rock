using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.CheckIn.v2
{
    [TestClass]
    public class FamilyRegistrationTests : DatabaseTestsBase
    {
        private readonly Guid DeckerFamilyGuid = new Guid( "53A02527-C2A7-4F36-8585-71A85B8E4601" );
        private readonly Guid EmployerPersonAttributeGuid = new Guid( "bf5f86fd-7bfb-491f-91de-6a2e4e9fd4cc" );
        private readonly Guid AllergyPersonAttributeGuid = new Guid( "dbd192c9-0aa1-46ec-92ab-a3da8e056d31" );
        private readonly Guid SeniorSuffixDefinedValueGuid = new Guid( "852732d0-51a7-4803-bfc9-b50b035457e8" );

        #region GetPersonBag

        [TestMethod]
        public void GetPersonBag_WithNoConfiguredAttributes_DoesNotIncludeAnyAttributeValues()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker );
                var person = registration.GetPersonBag( tedDecker, null );

                Assert.IsEmpty( person.AttributeValues );
            }
        }

        [TestMethod]
        public void GetPersonBag_WithAdult_IncludesOptionalAttributes()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                templateConfigurationDataMock.SetupGet( m => m.OptionalAttributeGuidsForAdults )
                    .Returns( new List<Guid> { EmployerPersonAttributeGuid } );
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker );
                tedDecker.LoadAttributes( rockContext );

                var person = registration.GetPersonBag( tedDecker, null );

                Assert.AreEqual( 1, person.AttributeValues.Count );
                Assert.AreEqual( "Employer", person.AttributeValues.Keys.First() );
                Assert.AreEqual( tedDecker.GetAttributeValue( "Employer" ), person.AttributeValues["Employer"] );
            }
        }

        [TestMethod]
        public void GetPersonBag_WithAdult_IncludesRequiredAttributes()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                templateConfigurationDataMock.SetupGet( m => m.RequiredAttributeGuidsForAdults )
                    .Returns( new List<Guid> { EmployerPersonAttributeGuid } );
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker );
                tedDecker.LoadAttributes( rockContext );

                var person = registration.GetPersonBag( tedDecker, null );

                Assert.AreEqual( 1, person.AttributeValues.Count );
                Assert.AreEqual( "Employer", person.AttributeValues.Keys.First() );
                Assert.AreEqual( tedDecker.GetAttributeValue( "Employer" ), person.AttributeValues["Employer"] );
            }
        }

        [TestMethod]
        public void GetPersonBag_WithChild_IncludesOptionalAttributes()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                templateConfigurationDataMock.SetupGet( m => m.OptionalAttributeGuidsForChildren )
                    .Returns( new List<Guid> { AllergyPersonAttributeGuid } );
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.AlexDecker );
                tedDecker.LoadAttributes( rockContext );

                var person = registration.GetPersonBag( tedDecker, null );

                Assert.AreEqual( 1, person.AttributeValues.Count );
                Assert.AreEqual( "Allergy", person.AttributeValues.Keys.First() );
                Assert.AreEqual( tedDecker.GetAttributeValue( "Allergy" ), person.AttributeValues["Allergy"] );
            }
        }

        [TestMethod]
        public void GetPersonBag_WithChild_IncludesRequiredAttributes()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                templateConfigurationDataMock.SetupGet( m => m.RequiredAttributeGuidsForChildren )
                    .Returns( new List<Guid> { AllergyPersonAttributeGuid } );
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.AlexDecker );
                tedDecker.LoadAttributes( rockContext );

                var person = registration.GetPersonBag( tedDecker, null );

                Assert.AreEqual( 1, person.AttributeValues.Count );
                Assert.AreEqual( "Allergy", person.AttributeValues.Keys.First() );
                Assert.AreEqual( tedDecker.GetAttributeValue( "Allergy" ), person.AttributeValues["Allergy"] );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void GetPersonBag_WithTestPerson_SetsAllBagProperties()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var personSearchKeyService = new PersonSearchKeyService( rockContext );
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );
                var seniorValue = DefinedValueCache.Get( SeniorSuffixDefinedValueGuid, rockContext );
                var numberMobileValue = DefinedValueCache.Get( new Guid( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ), rockContext );
                var marriedValue = DefinedValueCache.Get( new Guid( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED ), rockContext );
                var otherRaceValue = DefinedValueCache.Get( new Guid( SystemGuid.DefinedValue.PERSON_RACE_OTHER ), rockContext );
                var notHispanicValue = DefinedValueCache.Get( new Guid( SystemGuid.DefinedValue.PERSON_ETHNICITY_NOT_HISPANIC_OR_LATINO ), rockContext );
                var activeRecordValue = DefinedValueCache.Get( new Guid( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE ), rockContext );
                var memberValue = DefinedValueCache.Get( new Guid( SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_MEMBER ), rockContext );
                var alternateIdValue = DefinedValueCache.Get( new Guid( SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID ), rockContext );
                var thirdGradeValue = DefinedValueCache.Get( new Guid( "23cc6288-78ed-4849-afc9-417e0da5a4a9" ), rockContext );

                var testPerson = new Person
                {
                    NickName = "Test NickName",
                    FirstName = "Test FirstName",
                    LastName = "Test LastName",
                    SuffixValueId = seniorValue.Id,
                    Gender = Gender.Male,
                    BirthYear = 2013,
                    BirthMonth = 7,
                    BirthDay = 18,
                    Email = "testemail@fakeinbox.com",
                    GradeOffset = thirdGradeValue.Value.AsInteger(),
                    PhoneNumbers = new List<PhoneNumber>
                    {
                        new PhoneNumber
                        {
                            NumberTypeValueId = numberMobileValue.Id,
                            Number = "6235553322",
                            CountryCode = "1",
                            IsMessagingEnabled = true
                        }
                    },
                    AgeClassification = AgeClassification.Adult,
                    MaritalStatusValueId = marriedValue.Id,
                    RaceValueId = otherRaceValue.Id,
                    EthnicityValueId = notHispanicValue.Id,
                    RecordStatusValueId = activeRecordValue.Id,
                    ConnectionStatusValueId = memberValue.Id
                };

                personService.Add( testPerson );
                rockContext.SaveChanges();

                var testSearchKey = new PersonSearchKey
                {
                    PersonAliasId = testPerson.PrimaryAliasId,
                    SearchTypeValueId = alternateIdValue.Id,
                    SearchValue = "test-search"
                };

                personSearchKeyService.DeleteRange( personSearchKeyService
                    .Queryable()
                    .Where( psk => psk.PersonAliasId == testPerson.PrimaryAliasId ) );
                personSearchKeyService.Add( testSearchKey );
                rockContext.SaveChanges();

                var registrationPerson = registration.GetPersonBag( testPerson, null );

                Assert.AreEqual( testPerson.IdKey, registrationPerson.Id );
                Assert.AreEqual( testPerson.NickName, registrationPerson.NickName );
                Assert.AreEqual( testPerson.LastName, registrationPerson.LastName );
                Assert.AreEqual( seniorValue.Guid.ToString(), registrationPerson.Suffix.Value );
                Assert.AreEqual( testPerson.Gender, registrationPerson.Gender );
                Assert.AreEqual( testPerson.BirthDate, registrationPerson.BirthDate?.ToOrganizationDateTime() );
                Assert.AreEqual( testPerson.Email, registrationPerson.Email );
                Assert.AreEqual( thirdGradeValue.Value.ToString(), registrationPerson.Grade.Value );
                Assert.AreEqual( "1", registrationPerson.PhoneNumber.CountryCode );
                Assert.AreEqual( "6235553322", registrationPerson.PhoneNumber.Number );
                Assert.IsTrue( registrationPerson.PhoneNumber.IsMessagingEnabled );
                Assert.IsTrue( registrationPerson.IsAdult );
                Assert.IsTrue( registrationPerson.IsMarried );
                Assert.AreEqual( "test-search", registrationPerson.AlternateId );
                Assert.AreEqual( otherRaceValue.Guid.ToString(), registrationPerson.Race.Value );
                Assert.AreEqual( notHispanicValue.Guid.ToString(), registrationPerson.Ethnicity.Value );
                Assert.AreEqual( activeRecordValue.Guid.ToString(), registrationPerson.RecordStatus.Value );
                Assert.AreEqual( memberValue.Guid.ToString(), registrationPerson.ConnectionStatus.Value );
                Assert.IsNull( registrationPerson.RelationshipToAdult );
            }
        }

        #endregion

        #region GetFamilyBag

        [TestMethod]
        public void GetFamilyBag_WithDeckerFamily_SetsAllBagProperties()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );
                var registrationFamily = registration.GetFamilyBag( deckerFamily );

                Assert.AreEqual( deckerFamily.IdKey, registrationFamily.Bag.Id );
                Assert.AreEqual( deckerFamily.Name, registrationFamily.Bag.FamilyName );
                Assert.AreEqual( "11624 N 31st Dr", registrationFamily.Bag.Address.Street1 );
                Assert.IsNull( registrationFamily.Bag.Address.Street2 );
                Assert.AreEqual( "Phoenix", registrationFamily.Bag.Address.City );
                Assert.AreEqual( "Maricopa", registrationFamily.Bag.Address.Locality );
                Assert.AreEqual( "AZ", registrationFamily.Bag.Address.State );
                Assert.AreEqual( "85029-3202", registrationFamily.Bag.Address.PostalCode );
            }
        }

        [TestMethod]
        public void GetFamilyBag_WithAddressUnavailableInTemplate_DoesNotIncludeAddress()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                templateConfigurationDataMock.SetupGet( m => m.DisplayAddressOnFamilies ).Returns( Enums.Controls.RequirementLevel.Unavailable );

                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );
                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );
                var registrationFamily = registration.GetFamilyBag( deckerFamily );

                Assert.IsNull( registrationFamily.Bag.Address );
            }
        }

        #endregion

        #region SaveRegistration

        [TestMethod]
        [IsolatedTestDatabase]
        public void SaveRegistration_WithoutChanges_Succeeds()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );
                var registrationFamily = registration.GetFamilyBag( deckerFamily );
                var registrationPeople = registration.GetFamilyMemberBags( deckerFamily, null );

                var result = registration.SaveRegistration( registrationFamily, registrationPeople, null, new List<string>() );

                Assert.IsTrue( result.IsSuccess );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void SaveRegistration_WithoutChanges_DoesNotCreateNewRecords()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );
                var registrationFamily = registration.GetFamilyBag( deckerFamily );
                var registrationPeople = registration.GetFamilyMemberBags( deckerFamily, null );

                var result = registration.SaveRegistration( registrationFamily, registrationPeople, null, new List<string>() );

                Assert.IsEmpty( result.NewFamilyList );
                Assert.IsEmpty( result.NewPersonList );
            }
        }

        #endregion

        #region HasAllRequiredValues

        [TestMethod]
        public void HasAllRequiredValues_WithoutFamilyIdKey_ReturnsError()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );
                var registrationFamily = registration.GetFamilyBag( deckerFamily );

                registrationFamily.ValidProperties.Remove( nameof( RegistrationFamilyBag.Id ) );
                var expectedError = $"Family is missing required {nameof( RegistrationFamilyBag.Id )} property.";

                var result = registration.HasAllRequiredValues( registrationFamily, new List<ValidPropertiesBox<RegistrationPersonBag>>(), out var errorMessage );

                Assert.IsFalse( result );
                Assert.AreEqual( expectedError, errorMessage );
            }
        }

        [TestMethod]
        public void HasAllRequiredValues_WithoutFamilyName_ReturnsError()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );
                var registrationFamily = registration.GetFamilyBag( deckerFamily );

                registrationFamily.ValidProperties.Remove( nameof( RegistrationFamilyBag.FamilyName ) );
                var expectedError = $"Family is missing required {nameof( RegistrationFamilyBag.FamilyName )} property.";

                var result = registration.HasAllRequiredValues( registrationFamily, new List<ValidPropertiesBox<RegistrationPersonBag>>(), out var errorMessage );

                Assert.IsFalse( result );
                Assert.AreEqual( expectedError, errorMessage );
            }
        }

        [TestMethod]
        public void HasAllRequiredValues_WithoutPersonIdKey_ReturnsError()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );
                var registrationFamily = registration.GetFamilyBag( deckerFamily );
                var registrationPeople = registration.GetFamilyMemberBags( deckerFamily, null );

                registrationPeople[0].ValidProperties.Remove( nameof( RegistrationPersonBag.Id ) );
                var expectedError = $"Person is missing required {nameof( RegistrationPersonBag.Id )} property.";

                var result = registration.HasAllRequiredValues( registrationFamily, registrationPeople, out var errorMessage );

                Assert.IsFalse( result );
                Assert.AreEqual( expectedError, errorMessage );
            }
        }

        [TestMethod]
        public void HasAllRequiredValues_WithoutPersonNickName_ReturnsError()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );
                var registrationFamily = registration.GetFamilyBag( deckerFamily );

                var registrationPeople = registration.GetFamilyMemberBags( deckerFamily, null );

                registrationPeople[0].ValidProperties.Remove( nameof( RegistrationPersonBag.NickName ) );
                var expectedError = $"Person is missing required {nameof( RegistrationPersonBag.NickName )} property.";

                var result = registration.HasAllRequiredValues( registrationFamily, registrationPeople, out var errorMessage );

                Assert.IsFalse( result );
                Assert.AreEqual( expectedError, errorMessage );
            }
        }

        [TestMethod]
        public void HasAllRequiredValues_WithoutPersonLastName_ReturnsError()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );
                var registrationFamily = registration.GetFamilyBag( deckerFamily );
                var registrationPeople = registration.GetFamilyMemberBags( deckerFamily, null );

                registrationPeople[0].ValidProperties.Remove( nameof( RegistrationPersonBag.LastName ) );
                var expectedError = $"Person is missing required {nameof( RegistrationPersonBag.LastName )} property.";

                var result = registration.HasAllRequiredValues( registrationFamily, registrationPeople, out var errorMessage );

                Assert.IsFalse( result );
                Assert.AreEqual( expectedError, errorMessage );
            }
        }

        [TestMethod]
        public void HasAllRequiredValues_WithoutPersonIsAdult_ReturnsError()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );
                var registrationFamily = registration.GetFamilyBag( deckerFamily );
                var registrationPeople = registration.GetFamilyMemberBags( deckerFamily, null );

                registrationPeople[0].ValidProperties.Remove( nameof( RegistrationPersonBag.IsAdult ) );
                var expectedError = $"Person is missing required {nameof( RegistrationPersonBag.IsAdult )} property.";

                var result = registration.HasAllRequiredValues( registrationFamily, registrationPeople, out var errorMessage );

                Assert.IsFalse( result );
                Assert.AreEqual( expectedError, errorMessage );
            }
        }

        [TestMethod]
        public void HasAllRequiredValues_WithoutPersonRelationshipToAdult_ReturnsError()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );
                var registrationFamily = registration.GetFamilyBag( deckerFamily );
                var registrationPeople = registration.GetFamilyMemberBags( deckerFamily, null );

                registrationPeople[0].ValidProperties.Remove( nameof( RegistrationPersonBag.RelationshipToAdult ) );
                var expectedError = $"Person is missing required {nameof( RegistrationPersonBag.RelationshipToAdult )} property.";

                var result = registration.HasAllRequiredValues( registrationFamily, registrationPeople, out var errorMessage );

                Assert.IsFalse( result );
                Assert.AreEqual( expectedError, errorMessage );
            }
        }

        #endregion

        #region GetPersonMatchQuery

        [TestMethod]
        public void GetPersonMatchQuery_WithoutValidProperties_ExcludesValues()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        NickName = "nickname",
                        LastName = "lastname",
                        Email = "test@fakeinbox.com",
                        PhoneNumber = new PhoneNumberBoxWithSmsControlBag
                        {
                            Number = "6235553322"
                        },
                        Gender = Gender.Male,
                        BirthDate = RockDateTime.New( 2012, 7, 23 ).Value.ToRockDateTimeOffset(),
                        Suffix = new ListItemBag
                        {
                            Value = SeniorSuffixDefinedValueGuid.ToString()
                        }
                    },
                    ValidProperties = new List<string>()
                };

                var query = registration.GetPersonMatchQuery( registrationPerson );

                Assert.IsEmpty( query.Email );
                Assert.IsEmpty( query.MobilePhone );
                Assert.IsNull( query.Gender );
                Assert.IsNull( query.BirthDate );
                Assert.IsNull( query.SuffixValueId );
            }
        }

        [TestMethod]
        public void GetPersonMatchQuery_WithValidProperties_IncludesValues()
        {
            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );
                var expectedNickName = "nickname";
                var expectedLastName = "lastname";
                var expectedEmail = "test@fakeinbox.com";
                var expectedNumber = "6235553322";
                var expectedGender = Gender.Male;
                var expectedBirthDate = RockDateTime.New( 202, 7, 23 ).Value;
                var expectedSuffix = DefinedValueCache.Get( SeniorSuffixDefinedValueGuid, rockContext );

                var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        NickName = expectedNickName,
                        LastName = expectedLastName,
                        Email = expectedEmail,
                        PhoneNumber = new PhoneNumberBoxWithSmsControlBag
                        {
                            Number = expectedNumber
                        },
                        Gender = expectedGender,
                        BirthDate = expectedBirthDate.ToRockDateTimeOffset(),
                        Suffix = new ListItemBag
                        {
                            Value = expectedSuffix.Guid.ToString()
                        }
                    },
                    ValidProperties = new List<string>
                    {
                        nameof( RegistrationPersonBag.Email ),
                        nameof( RegistrationPersonBag.PhoneNumber ),
                        nameof( RegistrationPersonBag.Gender ),
                        nameof( RegistrationPersonBag.BirthDate ),
                        nameof( RegistrationPersonBag.Suffix )
                    }
                };

                var query = registration.GetPersonMatchQuery( registrationPerson );

                Assert.AreEqual( expectedNickName, query.FirstName );
                Assert.AreEqual( expectedLastName, query.LastName );
                Assert.AreEqual( expectedEmail, query.Email );
                Assert.AreEqual( expectedNumber, query.MobilePhone );
                Assert.AreEqual( expectedGender, query.Gender );
                Assert.AreEqual( expectedBirthDate, query.BirthDate );
                Assert.AreEqual( expectedSuffix.Id, query.SuffixValueId );
            }
        }

        #endregion

        #region CreatePrimaryFamily

        [TestMethod]
        public void CreatePrimaryFamily_WithoutValidName_ThrowsException()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
            {
                Bag = new RegistrationFamilyBag(),
                ValidProperties = new List<string>()
            };

            var saveResult = new FamilyRegistrationSaveResult();

            Assert.Throws<Exception>( () =>
            {
                registration.CreatePrimaryFamily( registrationFamily, string.Empty, null, saveResult );
            } );

            rockContextMock.Verify( m => m.SaveChanges(), Times.Never );
        }

        [TestMethod]
        public void CreatePrimaryFamily_WithFamilyName_UsesFamilyName()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var expectedName = "Decker Family";

            var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
            {
                Bag = new RegistrationFamilyBag
                {
                    FamilyName = expectedName
                },
                ValidProperties = new List<string>
                {
                    nameof(RegistrationFamilyBag.FamilyName)
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var family = registration.CreatePrimaryFamily( registrationFamily, string.Empty, null, saveResult );

            rockContextMock.Verify( m => m.SaveChanges() );
            Assert.AreEqual( expectedName, family.Name );
        }

        [TestMethod]
        public void CreatePrimaryFamily_WithoutFamilyName_UsesDefaultFamilyLastName()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var expectedLastName = "Decker";
            var expectedName = $"{expectedLastName} Family";

            var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
            {
                Bag = new RegistrationFamilyBag(),
                ValidProperties = new List<string>()
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var family = registration.CreatePrimaryFamily( registrationFamily, expectedLastName, null, saveResult );

            rockContextMock.Verify( m => m.SaveChanges() );
            Assert.AreEqual( expectedName, family.Name );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void CreatePrimaryFamily_WithAddress_CreatesFamilyAddress()
        {
            // Disable location services.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( @"
DELETE [AV]
    FROM [AttributeValue] AS [AV]
    INNER JOIN [Attribute] AS [A] ON [A].[Id] = [AV].[AttributeId]
    INNER JOIN [EntityType] AS [ET] ON [ET].[Id] = [A].[EntityTypeId]
    WHERE [ET].[Name] = 'Rock.Address.SmartyStreets'
      AND [A].[Key] = 'Active'" );
            }

            int familyId;
            var expectedStreet = "1234 Elm Street";
            var expectedCity = "Nowhere";
            var expectedState = "AZ";
            var expectedPostalCode = "12345";
            var expectedCountry = "US";

            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
                {
                    Bag = new RegistrationFamilyBag
                    {
                        FamilyName = "Decker",
                        Address = new AddressControlBag
                        {
                            Street1 = expectedStreet,
                            City = expectedCity,
                            State = expectedState,
                            PostalCode = expectedPostalCode,
                            Country = expectedCountry
                        }
                    },
                    ValidProperties = new List<string>
                    {
                        nameof( RegistrationFamilyBag.FamilyName ),
                        nameof( RegistrationFamilyBag.Address )
                    }
                };

                var saveResult = new FamilyRegistrationSaveResult();
                familyId = registration.CreatePrimaryFamily( registrationFamily, null, null, saveResult ).Id;
            }

            using ( var rockContext = new RockContext() )
            {
                var family = new GroupService( rockContext ).Get( familyId );
                Assert.IsNotNull( family );
                Assert.IsNotEmpty( family.GroupLocations );
                Assert.AreEqual( expectedStreet, family.GroupLocations.First().Location.Street1 );
                Assert.AreEqual( expectedCity, family.GroupLocations.First().Location.City );
                Assert.AreEqual( expectedState, family.GroupLocations.First().Location.State );
                Assert.AreEqual( expectedPostalCode, family.GroupLocations.First().Location.PostalCode );
                Assert.AreEqual( expectedCountry, family.GroupLocations.First().Location.Country );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void CreatePrimaryFamily_WithAddressUnavailableInTemplate_DoesNotCreateFamilyAddress()
        {
            // Disable location services.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( @"
DELETE [AV]
    FROM [AttributeValue] AS [AV]
    INNER JOIN [Attribute] AS [A] ON [A].[Id] = [AV].[AttributeId]
    INNER JOIN [EntityType] AS [ET] ON [ET].[Id] = [A].[EntityTypeId]
    WHERE [ET].[Name] = 'Rock.Address.SmartyStreets'
      AND [A].[Key] = 'Active'" );
            }

            int familyId;
            var expectedStreet = "1234 Elm Street";
            var expectedCity = "Nowhere";
            var expectedState = "AZ";
            var expectedPostalCode = "12345";
            var expectedCountry = "US";

            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                templateConfigurationDataMock.Setup( m => m.DisplayAddressOnFamilies ).Returns( Enums.Controls.RequirementLevel.Unavailable );
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
                {
                    Bag = new RegistrationFamilyBag
                    {
                        FamilyName = "Decker",
                        Address = new AddressControlBag
                        {
                            Street1 = expectedStreet,
                            City = expectedCity,
                            State = expectedState,
                            PostalCode = expectedPostalCode,
                            Country = expectedCountry
                        }
                    },
                    ValidProperties = new List<string>
                    {
                        nameof( RegistrationFamilyBag.FamilyName ),
                        nameof( RegistrationFamilyBag.Address )
                    }
                };

                var saveResult = new FamilyRegistrationSaveResult();
                familyId = registration.CreatePrimaryFamily( registrationFamily, null, null, saveResult ).Id;
            }

            using ( var rockContext = new RockContext() )
            {
                var family = new GroupService( rockContext ).Get( familyId );
                Assert.IsNotNull( family );
                Assert.IsEmpty( family.GroupLocations );
            }
        }

        [TestMethod]
        public void GetDefaultFamilyLastName_WithAdultsAndChildren_UsesLastNameFromAdult()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var expectedLastName = "Decker";

            var registrationPeople = new List<ValidPropertiesBox<RegistrationPersonBag>>()
            {
                new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        IsAdult = false,
                        LastName = "Unexpected Value"
                    }
                },
                new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        IsAdult = true,
                        LastName = expectedLastName
                    }
                }
            };

            var familyName = registration.GetDefaultFamilyLastName( registrationPeople );

            Assert.AreEqual( expectedLastName, familyName );
        }

        [TestMethod]
        public void GetDefaultFamilyLastName_WithNoAdults_UsesLastNameFromChild()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var expectedLastName = "Decker";

            var registrationPeople = new List<ValidPropertiesBox<RegistrationPersonBag>>()
            {
                new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        IsAdult = false,
                        LastName = expectedLastName
                    }
                }
            };

            var familyName = registration.GetDefaultFamilyLastName( registrationPeople );

            Assert.AreEqual( expectedLastName, familyName );
        }

        #endregion

        #region UpdatePrimaryFamily

        [TestMethod]
        [IsolatedTestDatabase]
        public void UpdatePrimaryFamily_WithAddress_UpdatesExistingFamilyAddress()
        {
            // Disable location services and ensure Ted has a home address.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( @"
DELETE [AV]
    FROM [AttributeValue] AS [AV]
    INNER JOIN [Attribute] AS [A] ON [A].[Id] = [AV].[AttributeId]
    INNER JOIN [EntityType] AS [ET] ON [ET].[Id] = [A].[EntityTypeId]
    WHERE [ET].[Name] = 'Rock.Address.SmartyStreets'
      AND [A].[Key] = 'Active'" );

                var deckerFamily = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker ).PrimaryFamily;
                var homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid(), rockContext ).Id;

                Assert.IsNotEmpty( deckerFamily.GroupLocations );
                Assert.AreEqual( homeLocationTypeId, deckerFamily.GroupLocations.First().GroupLocationTypeValueId );
            }

            var expectedStreet = "1234 Elm Street";
            var expectedCity = "Nowhere";
            var expectedState = "AZ";
            var expectedPostalCode = "12345";
            var expectedCountry = "US";

            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
                {
                    Bag = new RegistrationFamilyBag
                    {
                        Address = new AddressControlBag
                        {
                            Street1 = expectedStreet,
                            City = expectedCity,
                            State = expectedState,
                            PostalCode = expectedPostalCode,
                            Country = expectedCountry
                        }
                    },
                    ValidProperties = new List<string>
                    {
                        nameof( RegistrationFamilyBag.Address )
                    }
                };

                var deckerFamily = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker ).PrimaryFamily;
                var saveResult = new FamilyRegistrationSaveResult();
                registration.UpdatePrimaryFamily( deckerFamily, registrationFamily, saveResult );
            }

            using ( var rockContext = new RockContext() )
            {
                var family = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker ).PrimaryFamily;
                Assert.IsNotNull( family );
                Assert.IsNotEmpty( family.GroupLocations );
                Assert.AreEqual( expectedStreet, family.GroupLocations.First().Location.Street1 );
                Assert.AreEqual( expectedCity, family.GroupLocations.First().Location.City );
                Assert.AreEqual( expectedState, family.GroupLocations.First().Location.State );
                Assert.AreEqual( expectedPostalCode, family.GroupLocations.First().Location.PostalCode );
                Assert.AreEqual( expectedCountry, family.GroupLocations.First().Location.Country );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void UpdatePrimaryFamily_WithEmptyAddress_RemovesFamilyAddress()
        {
            // Disable location services and ensure Ted has a home address.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( @"
DELETE [AV]
    FROM [AttributeValue] AS [AV]
    INNER JOIN [Attribute] AS [A] ON [A].[Id] = [AV].[AttributeId]
    INNER JOIN [EntityType] AS [ET] ON [ET].[Id] = [A].[EntityTypeId]
    WHERE [ET].[Name] = 'Rock.Address.SmartyStreets'
      AND [A].[Key] = 'Active'" );

                var deckerFamily = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker ).PrimaryFamily;
                var homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid(), rockContext ).Id;

                Assert.IsNotEmpty( deckerFamily.GroupLocations );
                Assert.AreEqual( homeLocationTypeId, deckerFamily.GroupLocations.First().GroupLocationTypeValueId );
            }

            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
                {
                    Bag = new RegistrationFamilyBag
                    {
                        Address = new AddressControlBag()
                    },
                    ValidProperties = new List<string>
                    {
                        nameof( RegistrationFamilyBag.Address )
                    }
                };

                var deckerFamily = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker ).PrimaryFamily;
                var saveResult = new FamilyRegistrationSaveResult();
                registration.UpdatePrimaryFamily( deckerFamily, registrationFamily, saveResult );
            }

            using ( var rockContext = new RockContext() )
            {
                var family = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker ).PrimaryFamily;
                Assert.IsNotNull( family );
                Assert.IsEmpty( family.GroupLocations );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void UpdatePrimaryFamily_WithAddressUnavailableInTemplate_DoesNotUpdateExistingFamilyAddress()
        {
            string expectedStreet;
            string expectedCity;
            string expectedState;
            string expectedPostalCode;
            string expectedCountry;

            // Disable location services and ensure Ted has a home address.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( @"
DELETE [AV]
    FROM [AttributeValue] AS [AV]
    INNER JOIN [Attribute] AS [A] ON [A].[Id] = [AV].[AttributeId]
    INNER JOIN [EntityType] AS [ET] ON [ET].[Id] = [A].[EntityTypeId]
    WHERE [ET].[Name] = 'Rock.Address.SmartyStreets'
      AND [A].[Key] = 'Active'" );

                var deckerFamily = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker ).PrimaryFamily;
                var homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid(), rockContext ).Id;

                Assert.IsNotEmpty( deckerFamily.GroupLocations );
                Assert.AreEqual( homeLocationTypeId, deckerFamily.GroupLocations.First().GroupLocationTypeValueId );

                expectedStreet = deckerFamily.GroupLocations.First().Location.Street1;
                expectedCity = deckerFamily.GroupLocations.First().Location.City;
                expectedState = deckerFamily.GroupLocations.First().Location.State;
                expectedPostalCode = deckerFamily.GroupLocations.First().Location.PostalCode;
                expectedCountry = deckerFamily.GroupLocations.First().Location.Country;
            }

            using ( var rockContext = new RockContext() )
            {
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                templateConfigurationDataMock.Setup( m => m.DisplayAddressOnFamilies ).Returns( Enums.Controls.RequirementLevel.Unavailable );

                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
                {
                    Bag = new RegistrationFamilyBag
                    {
                        Address = new AddressControlBag
                        {
                            Street1 = "1234 Elm Street",
                            City = "Nowhere",
                            State = "AZ",
                            PostalCode = "12345",
                            Country = "US"
                        }
                    },
                    ValidProperties = new List<string>
                    {
                        nameof( RegistrationFamilyBag.Address )
                    }
                };

                var deckerFamily = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker ).PrimaryFamily;
                var saveResult = new FamilyRegistrationSaveResult();
                registration.UpdatePrimaryFamily( deckerFamily, registrationFamily, saveResult );
            }

            using ( var rockContext = new RockContext() )
            {
                var family = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker ).PrimaryFamily;
                Assert.IsNotNull( family );
                Assert.IsNotEmpty( family.GroupLocations );
                Assert.AreEqual( expectedStreet, family.GroupLocations.First().Location.Street1 );
                Assert.AreEqual( expectedCity, family.GroupLocations.First().Location.City );
                Assert.AreEqual( expectedState, family.GroupLocations.First().Location.State );
                Assert.AreEqual( expectedPostalCode, family.GroupLocations.First().Location.PostalCode );
                Assert.AreEqual( expectedCountry, family.GroupLocations.First().Location.Country );
            }
        }

        #endregion

        #region CreateOrUpdatePerson

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchablePerson_MatchesExistingRecord()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    LastName = "Decker",
                    Email = "ted@rocksolidchurchdemo.com",
                    BirthDate = RockDateTime.New( 1985, 2, 10 ).Value,
                    IsAdult = true,
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( TestGuids.TestPeople.TedDecker.AsGuid(), person.Guid );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMissingPerson_ThrowsException()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = "lk2j3r1l2k34"
                },
                ValidProperties = new List<string>()
            };

            var saveResult = new FamilyRegistrationSaveResult();

            Assert.That.ThrowsWithMessage<Exception>( () =>
            {
                registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );
            }, "Person was not found." );
        }

        #region PrimaryFamily

        [TestMethod]
        public void CreateOrUpdatePerson_WithNoPrimaryFamilyAndMatchedAdult_UpdatesPrimaryFamily()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    LastName = "Decker",
                    Email = "ted@rocksolidchurchdemo.com",
                    BirthDate = RockDateTime.New( 1985, 2, 10 ).Value,
                    IsAdult = true,
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.IsNotNull( primaryFamily );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithPrimaryFamilyAndMatchedAdult_DoesNotUpdatePrimaryFamily()
        {
            var expectedGuid = new Guid( "4f34fb94-82cb-4ee9-9740-0c0d00164153" );

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = new Group
            {
                Guid = expectedGuid
            };

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    LastName = "Decker",
                    Email = "ted@rocksolidchurchdemo.com",
                    BirthDate = RockDateTime.New( 1985, 2, 10 ).Value,
                    IsAdult = true,
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            // Make sure we matched Ted, but did not update the family.
            Assert.AreEqual( TestGuids.TestPeople.TedDecker.AsGuid(), person.Guid );
            Assert.AreEqual( expectedGuid, primaryFamily.Guid );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithNoPrimaryFamilyAndMatchedChild_DoesNotUpdatePrimaryFamily()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = "Noah",
                    LastName = "Decker",
                    BirthDate = RockDateTime.New( 2013, 3, 10 ).Value,
                    IsAdult = false,
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.BirthDate )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            // Make sure we got a match on Noah, but that it did not update family.
            Assert.AreEqual( TestGuids.TestPeople.NoahDecker.AsGuid(), person.Guid );
            Assert.IsNull( primaryFamily );
        }

        #endregion

        #region Names

        [TestMethod]
        public void CreateOrUpdatePerson_WithNickName_UpdatesNickName()
        {
            var expectedNickName = "Ted";

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = expectedNickName
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.NickName )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedNickName, person.NickName );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithChangedNickName_UpdatesFirstName()
        {
            var expectedFirstName = "Teddy";

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var tedDeckerIdKey = new PersonService( rockContextMock.Object ).Get( TestGuids.TestPeople.TedDecker ).IdKey;
            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDeckerIdKey,
                    NickName = expectedFirstName
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.NickName )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedFirstName, person.FirstName );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndSameNickName_SkipsFirstName()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var tedDecker = new PersonService( rockContextMock.Object ).Get( TestGuids.TestPeople.TedDecker );
            var expectedFirstName = tedDecker.FirstName;

            Assert.AreNotEqual( tedDecker.NickName, tedDecker.FirstName, "Expected Ted Decker's first name and nick name to be different." );

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    NickName = tedDecker.NickName
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.NickName )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedFirstName, person.FirstName );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithLastName_UpdatesLastName()
        {
            var expectedLastName = "Decker";

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    LastName = expectedLastName
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.LastName )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedLastName, person.LastName );
        }

        #endregion

        #region Gender

        [TestMethod]
        public void CreateOrUpdatePerson_WithGender_UpdatesGender()
        {
            var expectedGender = Gender.Male;

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Gender = expectedGender
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Gender )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedGender, person.Gender );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithGenderWithoutValidProperty_DoesNotUpdateGender()
        {
            var expectedGender = Gender.Unknown;

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Gender = Gender.Male
                },
                ValidProperties = new List<string>()
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedGender, person.Gender );
        }

        #endregion

        #region IsMarried

        [TestMethod]
        public void CreateOrUpdatePerson_WithIsMarried_UpdatesMartialStatusToMarried()
        {
            var expectedMaritalStatusValue = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED );

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    IsMarried = true
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.IsMarried )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedMaritalStatusValue.Id, person.MaritalStatusValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithIsMarriedWithoutValidProperty_DoesNotUpdateMartialStatusValueId()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    IsMarried = true
                },
                ValidProperties = new List<string>()
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.IsNull( person.MaritalStatusValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithoutIsMarriedAndCurrentValueIsMarried_UpdatesMartialStatusToSingle()
        {
            var expectedMaritalStatusValue = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE );

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object ).Get( TestGuids.TestPeople.TedDecker );
            tedDecker.MaritalStatusValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED ).Id;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    IsMarried = false
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.IsMarried )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( tedDecker.Id, person.Id );
            Assert.AreEqual( expectedMaritalStatusValue.Id, tedDecker.MaritalStatusValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithoutIsMarriedAndCurrentValueIsNull_UpdatesMartialStatusToSingle()
        {
            var expectedMaritalStatusValue = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE );

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    IsMarried = false
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.IsMarried )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedMaritalStatusValue.Id, person.MaritalStatusValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithoutIsMarriedAndCurrentValueIsNotMarried_DoesNotUpdateMartialStatusValueId()
        {
            var expectedMaritalStatusValue = -1;

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object ).Get( TestGuids.TestPeople.TedDecker );
            tedDecker.MaritalStatusValueId = expectedMaritalStatusValue;

            Group primaryFamily = null;

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    IsMarried = false
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.IsMarried )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( tedDecker.Id, person.Id );
            Assert.AreEqual( expectedMaritalStatusValue, tedDecker.MaritalStatusValueId );
        }

        #endregion

        #region Suffix

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndSuffix_UpdatesSuffixValueId()
        {
            var expectedSuffix = DefinedValueCache.Get( SeniorSuffixDefinedValueGuid );
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.SuffixValueId = null;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    Suffix = new ListItemBag
                    {
                        Value = expectedSuffix.Guid.ToString()
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Suffix )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedSuffix.Id, tedDecker.SuffixValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndBlankSuffix_UpdatesSuffixValueId()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.SuffixValueId = -1;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    Suffix = new ListItemBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Suffix )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.IsNull( tedDecker.SuffixValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndBlankSuffix_DoesNotUpdateSuffixValueId()
        {
            var expectedSuffixId = -1;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.SuffixValueId = expectedSuffixId;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    Suffix = new ListItemBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.Suffix )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedSuffixId, tedDecker.SuffixValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndSuffix_UpdatesSuffixValueId()
        {
            var expectedSuffix = DefinedValueCache.Get( SeniorSuffixDefinedValueGuid );
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.SuffixValueId = null;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    Suffix = new ListItemBag
                    {
                        Value = expectedSuffix.Guid.ToString()
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.Suffix )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedSuffix.Id, tedDecker.SuffixValueId );
        }

        #endregion

        #region Email

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndEmail_UpdatesEmail()
        {
            var expectedEmail = "ted1234@fakeinbox.com";
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.Email = null;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    Email = expectedEmail
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedEmail, tedDecker.Email );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndBlankEmail_UpdatesEmail()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.Email = "ted123@fakeinbox.com";

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    Email = string.Empty
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( string.Empty, tedDecker.Email );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndBlankEmail_DoesNotUpdateEmail()
        {
            var expectedEmail = "ted123@fakeinbox.com";
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.Email = expectedEmail;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = string.Empty,
                    BirthDate = tedDecker.BirthDate,
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedEmail, tedDecker.Email );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndEmail_UpdatesEmail()
        {
            var expectedEmail = "ted123@fakeinbox.com";
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.Email = null;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = expectedEmail,
                    BirthDate = tedDecker.BirthDate
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedEmail, tedDecker.Email );
        }

        #endregion

        #region BirthDate

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndBirthDate_UpdatesBirthDate()
        {
            var expectedBirthDate = RockDateTime.New( 2015, 7, 22 ).Value;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.SetBirthDate( null );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    BirthDate = expectedBirthDate
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.BirthDate )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedBirthDate, tedDecker.BirthDate );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndBlankBirthDate_UpdatesBirthDate()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.SetBirthDate( RockDateTime.New( 2014, 7, 23 ).Value );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    BirthDate = null
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.BirthDate )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.IsNull( tedDecker.BirthDate );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndBlankBirthDate_DoesNotUpdateBirthDate()
        {
            var expectedBirthDate = RockDateTime.New( 2014, 7, 23 ).Value;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.SetBirthDate( expectedBirthDate );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedBirthDate, tedDecker.BirthDate );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndBirthDate_UpdatesBirthDate()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // If we provide a birthdate then at least the month
            // and day must match. So subtract a year from the current
            // birth date so it's easy to spot and we can still have
            // the matching logic work.
            var expectedBirthDate = tedDecker.BirthDate.Value.AddYears( -1 );

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    Gender = tedDecker.Gender,
                    BirthDate = expectedBirthDate
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.Gender ),
                    nameof( RegistrationPersonBag.BirthDate )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedBirthDate, tedDecker.BirthDate );
        }

        #endregion

        #region Grade

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndGrade_UpdatesGradeOffset()
        {
            var expectedGradeOffset = 6;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.GradeOffset = null;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    Grade = new ListItemBag
                    {
                        Value = expectedGradeOffset.ToString()
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Grade )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedGradeOffset, tedDecker.GradeOffset );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndBlankGrade_UpdatesGradeOffset()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.GradeOffset = 6;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    Grade = null
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Grade )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.IsNull( tedDecker.GradeOffset );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndBlankGrade_DoesNotUpdateGradeOffset()
        {
            var expectedGradeOffset = 6;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.GradeOffset = expectedGradeOffset;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    Grade = new ListItemBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.Grade )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedGradeOffset, tedDecker.GradeOffset );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndGrade_UpdatesGradeOffset()
        {
            var expectedGradeOffset = 6;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.GradeOffset = null;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    Grade = new ListItemBag
                    {
                        Value = expectedGradeOffset.ToString()
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.Grade )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedGradeOffset, tedDecker.GradeOffset );
        }

        #endregion

        #region RecordStatus

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndRecordStatus_UpdatesRecordStatusValueId()
        {
            var expectedRecordStatus = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.RecordStatusValueId = null;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    RecordStatus = new ListItemBag
                    {
                        Value = expectedRecordStatus.Guid.ToString()
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.RecordStatus )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedRecordStatus.Id, tedDecker.RecordStatusValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndBlankRecordStatus_UpdatesRecordStatusValueId()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.RecordStatusValueId = -1;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    RecordStatus = new ListItemBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.RecordStatus )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.IsNull( tedDecker.RecordStatusValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndBlankRecordStatus_DoesNotUpdateRecordStatusValueId()
        {
            var expectedRecordStatusId = -1;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.RecordStatusValueId = expectedRecordStatusId;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    RecordStatus = new ListItemBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.RecordStatus )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedRecordStatusId, tedDecker.RecordStatusValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndRecordStatus_DoesNotUpdateRecordStatusValueId()
        {
            var expectedRecordStatusId = -1;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.RecordStatusValueId = expectedRecordStatusId;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    RecordStatus = new ListItemBag
                    {
                        Value = SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.RecordStatus )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedRecordStatusId, tedDecker.RecordStatusValueId );
        }

        #endregion

        #region ConnectionStatus

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndConnectionStatus_UpdatesConnectionStatusValueId()
        {
            var expectedConnectionStatus = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_MEMBER );
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.ConnectionStatusValueId = null;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    ConnectionStatus = new ListItemBag
                    {
                        Value = expectedConnectionStatus.Guid.ToString()
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.ConnectionStatus )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedConnectionStatus.Id, tedDecker.ConnectionStatusValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndBlankConnectionStatus_UpdatesConnectionStatusValueId()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.ConnectionStatusValueId = -1;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    ConnectionStatus = new ListItemBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.ConnectionStatus )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.IsNull( tedDecker.ConnectionStatusValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndBlankConnectionStatus_DoesNotUpdateConnectionStatusValueId()
        {
            var expectedConnectionStatusId = -1;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.ConnectionStatusValueId = expectedConnectionStatusId;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    ConnectionStatus = new ListItemBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.ConnectionStatus )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedConnectionStatusId, tedDecker.ConnectionStatusValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndConnectionStatus_DoesNotUpdateConnectionStatusValueId()
        {
            var expectedConnectionStatusId = -1;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.ConnectionStatusValueId = expectedConnectionStatusId;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    ConnectionStatus = new ListItemBag
                    {
                        Value = SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_MEMBER
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.ConnectionStatus )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedConnectionStatusId, tedDecker.ConnectionStatusValueId );
        }

        #endregion

        #region Ethnicity

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndEthnicity_UpdatesEthnicityValueId()
        {
            var expectedEthnicity = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_ETHNICITY_NOT_HISPANIC_OR_LATINO );
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.EthnicityValueId = null;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    Ethnicity = new ListItemBag
                    {
                        Value = expectedEthnicity.Guid.ToString()
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Ethnicity )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedEthnicity.Id, tedDecker.EthnicityValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndBlankEthnicity_UpdatesEthnicityValueId()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.EthnicityValueId = -1;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    Ethnicity = new ListItemBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Ethnicity )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.IsNull( tedDecker.EthnicityValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndBlankEthnicity_DoesNotUpdateEthnicityValueId()
        {
            var expectedEthnicityId = -1;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.EthnicityValueId = expectedEthnicityId;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    Ethnicity = new ListItemBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.Ethnicity )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedEthnicityId, tedDecker.EthnicityValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndEthnicity_DoesNotUpdateEthnicityValueId()
        {
            var expectedEthnicityId = -1;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.EthnicityValueId = expectedEthnicityId;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    Ethnicity = new ListItemBag
                    {
                        Value = SystemGuid.DefinedValue.PERSON_ETHNICITY_NOT_HISPANIC_OR_LATINO
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.Ethnicity )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedEthnicityId, tedDecker.EthnicityValueId );
        }

        #endregion

        #region Race

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndRace_UpdatesRaceValueId()
        {
            var expectedRace = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RACE_OTHER );
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.RaceValueId = null;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    Race = new ListItemBag
                    {
                        Value = expectedRace.Guid.ToString()
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Race )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreEqual( expectedRace.Id, tedDecker.RaceValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithExistingPersonAndBlankRace_UpdatesRaceValueId()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.RaceValueId = -1;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    Id = tedDecker.IdKey,
                    Race = new ListItemBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Race )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.IsNull( tedDecker.RaceValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndBlankRace_DoesNotUpdateRaceValueId()
        {
            var expectedRaceId = -1;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.RaceValueId = expectedRaceId;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    Race = new ListItemBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.Race )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedRaceId, tedDecker.RaceValueId );
        }

        [TestMethod]
        public void CreateOrUpdatePerson_WithMatchedPersonAndRace_DoesNotUpdateRaceValueId()
        {
            var expectedRaceId = -1;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            // Set a value so we can know if it changed.
            tedDecker.RaceValueId = expectedRaceId;

            Group primaryFamily = null;
            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    NickName = tedDecker.NickName,
                    LastName = tedDecker.LastName,
                    Email = tedDecker.Email,
                    BirthDate = tedDecker.BirthDate,
                    Race = new ListItemBag
                    {
                        Value = SystemGuid.DefinedValue.PERSON_RACE_OTHER
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.Email ),
                    nameof( RegistrationPersonBag.BirthDate ),
                    nameof( RegistrationPersonBag.Race )
                }
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var person = registration.CreateOrUpdatePerson( registrationPerson, ref primaryFamily, saveResult );

            Assert.AreSame( tedDecker, person );
            Assert.AreEqual( expectedRaceId, tedDecker.RaceValueId );
        }

        #endregion

        #endregion

        #region UpdatePersonAlternateId

        [TestMethod]
        public void UpdatePersonAlternateId_WithoutValidProperty_DoesNothing()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag(),
                ValidProperties = new List<string>()
            };

            var updatedPersonSearchKey = registration.UpdatePersonAlternateId( tedDecker, registrationPerson, false );

            Assert.IsNull( updatedPersonSearchKey );
        }

        [TestMethod]
        public void UpdatePersonAlternateId_WithBlankAlternateId_DoesNothing()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    AlternateId = string.Empty
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.AlternateId )
                }
            };

            var updatedPersonSearchKey = registration.UpdatePersonAlternateId( tedDecker, registrationPerson, false );

            Assert.IsNull( updatedPersonSearchKey );
        }

        [TestMethod]
        public void UpdatePersonAlternateId_WithExistingPersonAndNewAlternateId_CreatesNewRecord()
        {
            var expectedAlternateId = "unit-test-value";
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    AlternateId = expectedAlternateId
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.AlternateId )
                }
            };

            var updatedPersonSearchKey = registration.UpdatePersonAlternateId( tedDecker, registrationPerson, false );

            Assert.AreEqual( 0, updatedPersonSearchKey.Id );
            Assert.AreEqual( expectedAlternateId, updatedPersonSearchKey.SearchValue );
        }

        [TestMethod]
        public void UpdatePersonAlternateId_WithExistingPersonAndSameSearchValue_DoesNotCreateNewRecord()
        {
            var searchTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() ).Id;
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object )
                .Get( TestGuids.TestPeople.TedDecker );
            var existingSearchKey = new PersonSearchKey
            {
                Id = 2823,
                PersonAliasId = tedDecker.PrimaryAliasId,
                PersonAlias = tedDecker.PrimaryAlias,
                SearchTypeValueId = searchTypeValueId,
                SearchValue = "mock-test"
            };
            var personSearchKeySource = new List<PersonSearchKey> { existingSearchKey };

            rockContextMock.SetupDbSet( personSearchKeySource );

            // Get the current database object so we can know if it changed.
            var personSearchKey = tedDecker.GetPersonSearchKeys( rockContextMock.Object ).First();
            var expectedCount = tedDecker.GetPersonSearchKeys( rockContextMock.Object ).Count();

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    AlternateId = personSearchKey.SearchValue
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.AlternateId )
                }
            };

            var updatedPersonSearchKey = registration.UpdatePersonAlternateId( tedDecker, registrationPerson, false );
            var newCount = tedDecker.GetPersonSearchKeys( rockContextMock.Object ).Count();

            Assert.AreSame( personSearchKey, updatedPersonSearchKey );
            Assert.AreEqual( expectedCount, newCount );
        }

        [TestMethod]
        public void UpdatePersonAlternateId_WithNewPerson_CreatesNewRecord()
        {
            var expectedAlternateId = "unit-test-value";
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var newPerson = new Person();

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    AlternateId = expectedAlternateId
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.AlternateId )
                }
            };

            var updatedPersonSearchKey = registration.UpdatePersonAlternateId( newPerson, registrationPerson, true );

            Assert.AreEqual( 0, updatedPersonSearchKey.Id );
            Assert.AreEqual( expectedAlternateId, updatedPersonSearchKey.SearchValue );
        }

        #endregion

        #region UpdatePersonMobilePhoneNumber

        [TestMethod]
        public void UpdatePersonMobilePhoneNumber_WithoutValidProperty_DoesNothing()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Mock<Person>( MockBehavior.Loose );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag(),
                ValidProperties = new List<string>()
            };

            registration.UpdatePersonMobilePhoneNumber( person.Object, registrationPerson, false );

            // Best assert we can do in this case is to just make sure that
            // nothing of person was called.
            person.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void UpdatePersonMobilePhoneNumber_WithBlankNumberAndSaveEmptyIsFalse_DoesNothing()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Mock<Person>( MockBehavior.Loose );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    PhoneNumber = new PhoneNumberBoxWithSmsControlBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.PhoneNumber )
                }
            };

            registration.UpdatePersonMobilePhoneNumber( person.Object, registrationPerson, false );

            // Best assert we can do in this case is to just make sure that
            // nothing of person was called.
            person.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void UpdatePersonMobilePhoneNumber_WithBlankNumberAndSaveEmptyIsTrue_RemovesPhoneNumber()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            // Allow mutation operations on the DbSet.
            var phoneNumberSetMock = rockContextMock.SetupDbSet( new List<PhoneNumber>() );

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Mock<Person>( MockBehavior.Loose );
            person.SetupGet( m => m.PhoneNumbers ).Returns( new List<PhoneNumber>
            {
                new PhoneNumber
                {
                    CountryCode = "1",
                    Number = "6235553322",
                    NumberTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id
                }
            } );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    PhoneNumber = new PhoneNumberBoxWithSmsControlBag()
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.PhoneNumber )
                }
            };

            registration.UpdatePersonMobilePhoneNumber( person.Object, registrationPerson, true );

            Assert.IsEmpty( person.Object.PhoneNumbers );
        }

        [TestMethod]
        public void UpdatePersonMobilePhoneNumber_WithMessagingSet_EnablesMessagingOnPhoneNumber()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            templateConfigurationDataMock.SetupGet( m => m.IsSmsButtonVisible ).Returns( true );
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Mock<Person>( MockBehavior.Loose );
            person.SetupGet( m => m.PhoneNumbers ).Returns( new List<PhoneNumber>
            {
                new PhoneNumber
                {
                    CountryCode = "1",
                    Number = "6235553322",
                    IsMessagingEnabled = false,
                    NumberTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id
                }
            } );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    PhoneNumber = new PhoneNumberBoxWithSmsControlBag
                    {
                        CountryCode = "1",
                        Number = "6235553322",
                        IsMessagingEnabled = true
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.PhoneNumber )
                }
            };

            registration.UpdatePersonMobilePhoneNumber( person.Object, registrationPerson, false );

            Assert.AreEqual( 1, person.Object.PhoneNumbers.Count );
            Assert.IsTrue( person.Object.PhoneNumbers.First().IsMessagingEnabled );
        }

        [TestMethod]
        public void UpdatePersonMobilePhoneNumber_WithNewNumber_CreatesPhoneNumber()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            // Allow mutation operations on the DbSet.
            rockContextMock.SetupDbSet( new List<PhoneNumber>() );

            // ZZZ doesn't like that we mock the db set properties, so until
            // that goes away pre-load from the cache so UpdatePhoneNumber
            // will not blow up.
            _ = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Mock<Person>( MockBehavior.Loose );
            person.SetupGet( m => m.PhoneNumbers ).Returns( new List<PhoneNumber>() );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    PhoneNumber = new PhoneNumberBoxWithSmsControlBag
                    {
                        CountryCode = "1",
                        Number = "6235553322"
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.PhoneNumber )
                }
            };

            registration.UpdatePersonMobilePhoneNumber( person.Object, registrationPerson, true );

            Assert.AreEqual( 1, person.Object.PhoneNumbers.Count );
        }

        #endregion

        #region UpdatePersonAttributeValues

        [TestMethod]
        public void UpdatePersonAttributeValues_WithoutValidProperty_DoesNothing()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Mock<Person>( MockBehavior.Loose );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag(),
                ValidProperties = new List<string>()
            };

            registration.UpdatePersonAttributeValues( person.Object, registrationPerson, false );

            // Best assert we can do in this case is to just make sure that
            // nothing of person was called.
            person.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void UpdatePersonAttributeValues_WithNullAttributeValues_DoesNothing()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Mock<Person>( MockBehavior.Loose );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    AttributeValues = null
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.AttributeValues )
                }
            };

            registration.UpdatePersonAttributeValues( person.Object, registrationPerson, false );

            // Best assert we can do in this case is to just make sure that
            // nothing of person was called.
            person.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void UpdatePersonAttributeValues_WithNullPersonAttributes_LoadsAttributes()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Person();

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    AttributeValues = new Dictionary<string, string>
                    {
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.AttributeValues )
                }
            };

            registration.UpdatePersonAttributeValues( person, registrationPerson, false );

            Assert.IsNotNull( person.Attributes );
            Assert.IsNotNull( person.AttributeValues );
        }

        [TestMethod]
        public void UpdatePersonAttributeValues_WithAdult_SetsAdultAttributeValues()
        {
            var expectedAllergyValue = "unit-test-allergy";
            var expectedLegalNotesValue = "unit-test-leagal-notes";

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();

            templateConfigurationDataMock.Setup( m => m.OptionalAttributeGuidsForAdults )
                .Returns( new List<Guid> { SystemGuid.Attribute.PERSON_ALLERGY.AsGuid() } );
            templateConfigurationDataMock.Setup( m => m.RequiredAttributeGuidsForAdults )
                .Returns( new List<Guid> { SystemGuid.Attribute.PERSON_LEGAL_NOTE.AsGuid() } );

            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Person();

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    IsAdult = true,
                    AttributeValues = new Dictionary<string, string>
                    {
                        ["Allergy"] = expectedAllergyValue,
                        ["LegalNotes"] = expectedLegalNotesValue
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.AttributeValues )
                }
            };

            registration.UpdatePersonAttributeValues( person, registrationPerson, false );

            Assert.AreEqual( expectedAllergyValue, person.GetAttributeValue( "Allergy" ) );
            Assert.AreEqual( expectedLegalNotesValue, person.GetAttributeValue( "LegalNotes" ) );
        }

        [TestMethod]
        public void UpdatePersonAttributeValues_WithChild_SetsChildAttributeValues()
        {
            var expectedAllergyValue = "unit-test-allergy";
            var expectedLegalNotesValue = "unit-test-legal-notes";

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();

            templateConfigurationDataMock.Setup( m => m.OptionalAttributeGuidsForChildren )
                .Returns( new List<Guid> { SystemGuid.Attribute.PERSON_ALLERGY.AsGuid() } );
            templateConfigurationDataMock.Setup( m => m.RequiredAttributeGuidsForChildren )
                .Returns( new List<Guid> { SystemGuid.Attribute.PERSON_LEGAL_NOTE.AsGuid() } );

            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Person();

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    IsAdult = false,
                    AttributeValues = new Dictionary<string, string>
                    {
                        ["Allergy"] = expectedAllergyValue,
                        ["LegalNotes"] = expectedLegalNotesValue
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.AttributeValues )
                }
            };

            registration.UpdatePersonAttributeValues( person, registrationPerson, false );

            Assert.AreEqual( expectedAllergyValue, person.GetAttributeValue( "Allergy" ) );
            Assert.AreEqual( expectedLegalNotesValue, person.GetAttributeValue( "LegalNotes" ) );
        }

        [TestMethod]
        public void UpdatePersonAttributeValues_WithUnconfiguredAttributeValues_DoesNotSetAttributeValues()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Person();

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    IsAdult = true,
                    AttributeValues = new Dictionary<string, string>
                    {
                        ["Allergy"] = "unit-test-value"
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.AttributeValues )
                }
            };

            registration.UpdatePersonAttributeValues( person, registrationPerson, false );

            Assert.IsEmpty( person.GetAttributeValue( "Allergy" ) );
        }

        [TestMethod]
        public void UpdatePersonAttributeValues_WithBlankValueAndDontSaveEmptyValues_DoesNotSetAttributeValues()
        {
            var expectedAllergyValue = "unit-test-allergy";

            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();

            templateConfigurationDataMock.Setup( m => m.OptionalAttributeGuidsForChildren )
                .Returns( new List<Guid> { SystemGuid.Attribute.PERSON_ALLERGY.AsGuid() } );

            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Person();
            person.LoadAttributes( rockContextMock.Object );
            person.SetAttributeValue( "Allergy", expectedAllergyValue );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    IsAdult = false,
                    AttributeValues = new Dictionary<string, string>
                    {
                        ["Allergy"] = string.Empty
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.AttributeValues )
                }
            };

            registration.UpdatePersonAttributeValues( person, registrationPerson, false );

            Assert.AreEqual( expectedAllergyValue, person.GetAttributeValue( "Allergy" ) );
        }

        [TestMethod]
        public void UpdatePersonAttributeValues_WithBlankValueAndSaveEmptyValues_SetsAttributeValues()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();

            templateConfigurationDataMock.Setup( m => m.OptionalAttributeGuidsForChildren )
                .Returns( new List<Guid> { SystemGuid.Attribute.PERSON_ALLERGY.AsGuid() } );

            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var person = new Person();
            person.LoadAttributes( rockContextMock.Object );
            person.SetAttributeValue( "Allergy", "unit-test-value" );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    IsAdult = false,
                    AttributeValues = new Dictionary<string, string>
                    {
                        ["Allergy"] = string.Empty
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationPersonBag.AttributeValues )
                }
            };

            registration.UpdatePersonAttributeValues( person, registrationPerson, true );

            Assert.IsEmpty( person.GetAttributeValue( "Allergy" ) );
        }

        #endregion

        #region UpdateFamilyAttributeValues

        [TestMethod]
        public void UpdateFamilyAttributeValues_WithoutValidProperty_DoesNothing()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var family = new Mock<Group>( MockBehavior.Loose );

            var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
            {
                Bag = new RegistrationFamilyBag(),
                ValidProperties = new List<string>()
            };

            registration.UpdateFamilyAttributeValues( family.Object, registrationFamily );

            // Best assert we can do in this case is to just make sure that
            // nothing of family was called.
            family.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void UpdateFamilyAttributeValues_WithNullAttributeValues_DoesNothing()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var family = new Mock<Group>( MockBehavior.Loose );

            var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
            {
                Bag = new RegistrationFamilyBag
                {
                    AttributeValues = null
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationFamilyBag.AttributeValues )
                }
            };

            registration.UpdateFamilyAttributeValues( family.Object, registrationFamily );

            // Best assert we can do in this case is to just make sure that
            // nothing of family was called.
            family.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void UpdateFamilyAttributeValues_WithNullFamilyAttributes_LoadsAttributes()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var family = new GroupService( rockContextMock.Object ).Get( DeckerFamilyGuid );

            var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
            {
                Bag = new RegistrationFamilyBag
                {
                    AttributeValues = new Dictionary<string, string>
                    {
                    }
                },
                ValidProperties = new List<string>
                {
                    nameof( RegistrationFamilyBag.AttributeValues )
                }
            };

            registration.UpdateFamilyAttributeValues( family, registrationFamily );

            Assert.IsNotNull( family.Attributes );
            Assert.IsNotNull( family.AttributeValues );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void UpdateFamilyAttributeValues_WithAttributeValues_SetsAttributeValues()
        {
            var expectedOptionalValue = "unit-test-optional";
            var expectedRequiredValue = "unit-test-required";

            var optionalGuid = new Guid( "56dd71f2-779a-467b-bb71-1eb928aea790" );
            var requiredGuid = new Guid( "5bef7099-99a3-4a19-bb36-a33f0f909337" );

            using ( var rockContext = new RockContext() )
            {
                var attributeService = new AttributeService( rockContext );

                attributeService.Add( new Rock.Model.Attribute
                {
                    FieldTypeId = FieldTypeCache.Get( SystemGuid.FieldType.TEXT.AsGuid(), rockContext ).Id,
                    EntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.GROUP.AsGuid(), rockContext ).Id,
                    EntityTypeQualifierColumn = nameof( Group.GroupTypeId ),
                    EntityTypeQualifierValue = GroupTypeCache.GetFamilyGroupType().Id.ToString(),
                    Key = "OptionalTest",
                    Name = "Optional Test",
                    Guid = optionalGuid
                } );

                attributeService.Add( new Rock.Model.Attribute
                {
                    FieldTypeId = FieldTypeCache.Get( SystemGuid.FieldType.TEXT.AsGuid(), rockContext ).Id,
                    EntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.GROUP.AsGuid(), rockContext ).Id,
                    EntityTypeQualifierColumn = nameof( Group.GroupTypeId ),
                    EntityTypeQualifierValue = GroupTypeCache.GetFamilyGroupType().Id.ToString(),
                    Key = "RequiredTest",
                    Name = "Required Test",
                    Guid = requiredGuid
                } );

                rockContext.SaveChanges();

                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();

                templateConfigurationDataMock.Setup( m => m.OptionalAttributeGuidsForFamilies )
                    .Returns( new List<Guid> { optionalGuid } );
                templateConfigurationDataMock.Setup( m => m.RequiredAttributeGuidsForFamilies )
                    .Returns( new List<Guid> { requiredGuid } );

                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var family = new GroupService( rockContext ).Get( DeckerFamilyGuid );

                var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
                {
                    Bag = new RegistrationFamilyBag
                    {
                        AttributeValues = new Dictionary<string, string>
                        {
                            ["OptionalTest"] = expectedOptionalValue,
                            ["RequiredTest"] = expectedRequiredValue
                        }
                    },
                    ValidProperties = new List<string>
                    {
                        nameof( RegistrationFamilyBag.AttributeValues )
                    }
                };

                registration.UpdateFamilyAttributeValues( family, registrationFamily );

                Assert.AreEqual( expectedOptionalValue, family.GetAttributeValue( "OptionalTest" ) );
                Assert.AreEqual( expectedRequiredValue, family.GetAttributeValue( "RequiredTest" ) );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void UpdateFamilyAttributeValues_WithUnconfiguredAttributeValues_DoesNotSetAttributeValues()
        {
            var optionalGuid = new Guid( "56dd71f2-779a-467b-bb71-1eb928aea790" );

            using ( var rockContext = new RockContext() )
            {
                var attributeService = new AttributeService( rockContext );

                attributeService.Add( new Rock.Model.Attribute
                {
                    FieldTypeId = FieldTypeCache.Get( SystemGuid.FieldType.TEXT.AsGuid(), rockContext ).Id,
                    EntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.GROUP.AsGuid(), rockContext ).Id,
                    EntityTypeQualifierColumn = nameof( Group.GroupTypeId ),
                    EntityTypeQualifierValue = GroupTypeCache.GetFamilyGroupType().Id.ToString(),
                    Key = "OptionalTest",
                    Name = "Optional Test",
                    Guid = optionalGuid
                } );

                rockContext.SaveChanges();

                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var family = new GroupService( rockContext ).Get( DeckerFamilyGuid );

                var registrationFamily = new ValidPropertiesBox<RegistrationFamilyBag>
                {
                    Bag = new RegistrationFamilyBag
                    {
                        AttributeValues = new Dictionary<string, string>
                        {
                            ["OptionalTest"] = "unit-test-value"
                        }
                    },
                    ValidProperties = new List<string>
                    {
                        nameof( RegistrationFamilyBag.AttributeValues )
                    }
                };

                registration.UpdateFamilyAttributeValues( family, registrationFamily );

                Assert.IsEmpty( family.GetAttributeValue( "OptionalTest" ) );
            }
        }

        #endregion

        #region EnsurePeopleInPrimaryFamilyAreMembersOfGroup

        [TestMethod]
        [IsolatedTestDatabase]
        public void EnsurePeopleInPrimaryFamilyAreMembersOfGroup_WithNullRelationshipToAdult_AddsToFamily()
        {
            using ( var rockContext = new RockContext() )
            {
                var expectedRole = GroupTypeCache
                    .Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), rockContext )
                    .Roles
                    .FirstOrDefault( a => a.Guid == SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() );

                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var benJones = new PersonService( rockContext ).Get( TestGuids.TestPeople.BenJones );
                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );

                var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        RelationshipToAdult = null,
                        IsAdult = true
                    },
                    ValidProperties = new List<string>()
                };

                var people = new List<(ValidPropertiesBox<RegistrationPersonBag>, Person)>
                {
                    (registrationPerson, benJones)
                };

                registration.EnsurePeopleInPrimaryFamilyAreMembersOfGroup( deckerFamily, people );

                var groupMember = new GroupMemberService( rockContext )
                    .Queryable()
                    .Where( gm => gm.PersonId == benJones.Id
                        && gm.GroupId == deckerFamily.Id )
                    .SingleOrDefault();

                Assert.IsNotNull( groupMember );
                Assert.AreEqual( expectedRole.Id, groupMember.GroupRoleId );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void EnsurePeopleInPrimaryFamilyAreMembersOfGroup_WithSameFamilyRelationshipToAdult_AddsToFamily()
        {
            using ( var rockContext = new RockContext() )
            {
                var expectedRole = GroupTypeCache
                    .Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), rockContext )
                    .Roles
                    .FirstOrDefault( a => a.Guid == SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() );

                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var brianJones = new PersonService( rockContext ).Get( TestGuids.TestPeople.BrianJones );
                var deckerFamily = new GroupService( rockContext ).Get( DeckerFamilyGuid );

                var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        RelationshipToAdult = new ListItemBag
                        {
                            Value = expectedRole.Guid.ToString()
                        },
                        IsAdult = false
                    },
                    ValidProperties = new List<string>()
                };

                var people = new List<(ValidPropertiesBox<RegistrationPersonBag>, Person)>
                {
                    (registrationPerson, brianJones)
                };

                registration.EnsurePeopleInPrimaryFamilyAreMembersOfGroup( deckerFamily, people );

                var groupMember = new GroupMemberService( rockContext )
                    .Queryable()
                    .Where( gm => gm.PersonId == brianJones.Id
                        && gm.GroupId == deckerFamily.Id )
                    .SingleOrDefault();

                Assert.IsNotNull( groupMember );
                Assert.AreEqual( expectedRole.Id, groupMember.GroupRoleId );
            }
        }

        [TestMethod]
        public void EnsurePeopleInPrimaryFamilyAreMembersOfGroup_WithExistingFamilyMember_DoesNotAddToFamily()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();
            rockContextMock.SetupAllProperties();

            var expectedRole = GroupTypeCache
                .Get( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid(), rockContextMock.Object )
                .Roles
                .FirstOrDefault( a => a.Guid == SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() );

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var tedDecker = new PersonService( rockContextMock.Object ).Get( TestGuids.TestPeople.TedDecker );
            var deckerFamily = new GroupService( rockContextMock.Object ).Get( DeckerFamilyGuid );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    RelationshipToAdult = null,
                    IsAdult = true
                },
                ValidProperties = new List<string>()
            };

            var people = new List<(ValidPropertiesBox<RegistrationPersonBag>, Person)>
            {
                (registrationPerson, tedDecker)
            };

            registration.EnsurePeopleInPrimaryFamilyAreMembersOfGroup( deckerFamily, people );

            // If SaveChanges is not called, then no new GroupMember was created.
            rockContextMock.Verify( m => m.SaveChanges(), Times.Never );
        }

        #endregion

        #region EnsurePeopleNotInPrimaryFamilyHaveAFamily

        [TestMethod]
        public void EnsurePeopleNotInPrimaryFamilyHaveAFamily_WithPersonInFamily_DoesNotCreateFamily()
        {
            var rockContextMock = CreateRockContextWithoutSaveChanges();
            rockContextMock.SetupAllProperties();

            var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
            var registration = new FamilyRegistration( rockContextMock.Object, null, templateConfigurationDataMock.Object );

            var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
            {
                Bag = new RegistrationPersonBag
                {
                    RelationshipToAdult = new ListItemBag
                    {
                        Value = SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD
                    },
                    IsAdult = true
                },
                ValidProperties = new List<string>()
            };

            var saveResult = new FamilyRegistrationSaveResult();
            var people = new List<(ValidPropertiesBox<RegistrationPersonBag>, Person)>
            {
                (registrationPerson, null)
            };

            registration.EnsurePeopleNotInPrimaryFamilyHaveAFamily( people, null, saveResult );

            // If SaveChanges is not called, then no new Group was created.
            rockContextMock.Verify( m => m.SaveChanges(), Times.Never );
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void EnsurePeopleNotInPrimaryFamilyHaveAFamily_WithPersonNotInAnyFamily_CreatesFamily()
        {
            using ( var rockContext = new RockContext() )
            {
                var familyTypeId = GroupTypeCache.GetFamilyGroupType().Id;
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var noahDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.NoahDecker );

                noahDecker.PrimaryFamilyId = null;

                var registrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        RelationshipToAdult = new ListItemBag
                        {
                            Value = SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN
                        },
                        IsAdult = false
                    },
                    ValidProperties = new List<string>()
                };

                var saveResult = new FamilyRegistrationSaveResult();
                var people = new List<(ValidPropertiesBox<RegistrationPersonBag>, Person)>
                {
                    (registrationPerson, noahDecker)
                };

                registration.EnsurePeopleNotInPrimaryFamilyHaveAFamily( people, null, saveResult );

                Assert.AreEqual( 1, saveResult.NewFamilyList.Count );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void EnsurePeopleNotInPrimaryFamilyHaveAFamily_WithCanCheckInRelationship_CreatesKnownRelationship()
        {
            using ( var rockContext = new RockContext() )
            {
                var familyTypeId = GroupTypeCache.GetFamilyGroupType().Id;
                var expectedRelationship = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid(), rockContext )
                        .Roles
                        .Single( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid() );
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var brianJones = new PersonService( rockContext ).Get( TestGuids.TestPeople.BrianJones );
                var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker );

                var brianJonesRegistrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        RelationshipToAdult = new ListItemBag
                        {
                            Value = expectedRelationship.Guid.ToString()
                        },
                        IsAdult = false
                    },
                    ValidProperties = new List<string>()
                };

                var tedDeckerRegistrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        RelationshipToAdult = null,
                        IsAdult = true
                    },
                    ValidProperties = new List<string>()
                };

                var saveResult = new FamilyRegistrationSaveResult();
                var people = new List<(ValidPropertiesBox<RegistrationPersonBag>, Person)>
                {
                    (brianJonesRegistrationPerson, brianJones),
                    (tedDeckerRegistrationPerson, tedDecker)
                };

                registration.EnsurePeopleNotInPrimaryFamilyHaveAFamily( people, null, saveResult );

                var relationship = new GroupMemberService( rockContext )
                    .GetKnownRelationship( tedDecker.Id, expectedRelationship.Id )
                    .Where( r => r.PersonId == brianJones.Id )
                    .FirstOrDefault();

                Assert.IsNotNull( relationship );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void EnsurePeopleNotInPrimaryFamilyHaveAFamily_WithNonCanCheckInRelationship_CreatesCanCheckInRelationship()
        {
            using ( var rockContext = new RockContext() )
            {
                var familyTypeId = GroupTypeCache.GetFamilyGroupType().Id;
                var expectedRelationship = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid(), rockContext )
                        .Roles
                        .Single( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_FACEBOOK_FRIEND.AsGuid() );
                var canCheckInValueId = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid(), rockContext )
                        .Roles
                        .Single( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid() )
                        .Id;

                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();

                templateConfigurationDataMock.Setup( m => m.CanCheckInKnownRelationshipRoleGuids )
                    .Returns( new List<Guid> { expectedRelationship.Guid } );

                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var brianJones = new PersonService( rockContext ).Get( TestGuids.TestPeople.BrianJones );
                var tedDecker = new PersonService( rockContext ).Get( TestGuids.TestPeople.TedDecker );

                var brianJonesRegistrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        RelationshipToAdult = new ListItemBag
                        {
                            Value = expectedRelationship.Guid.ToString()
                        },
                        IsAdult = false
                    },
                    ValidProperties = new List<string>()
                };

                var tedDeckerRegistrationPerson = new ValidPropertiesBox<RegistrationPersonBag>
                {
                    Bag = new RegistrationPersonBag
                    {
                        RelationshipToAdult = null,
                        IsAdult = true
                    },
                    ValidProperties = new List<string>()
                };

                var saveResult = new FamilyRegistrationSaveResult();
                var people = new List<(ValidPropertiesBox<RegistrationPersonBag>, Person)>
                {
                    (brianJonesRegistrationPerson, brianJones),
                    (tedDeckerRegistrationPerson, tedDecker)
                };

                registration.EnsurePeopleNotInPrimaryFamilyHaveAFamily( people, null, saveResult );

                var canCheckInRelationship = new GroupMemberService( rockContext )
                    .GetKnownRelationship( tedDecker.Id, canCheckInValueId )
                    .Where( r => r.PersonId == brianJones.Id )
                    .FirstOrDefault();

                var relationship = new GroupMemberService( rockContext )
                    .GetKnownRelationship( tedDecker.Id, expectedRelationship.Id )
                    .Where( r => r.PersonId == brianJones.Id )
                    .FirstOrDefault();

                // This should create both a can-check-in relationship and the
                // specified relationship.
                Assert.IsNotNull( canCheckInRelationship );
                Assert.IsNotNull( relationship );
            }
        }

        #endregion

        #region RemoveFamilyMembers

        [TestMethod]
        [IsolatedTestDatabase]
        public void RemoveFamilyMembers_WithCanCheckInRelationship_RemovesRelationship()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var familyTypeId = GroupTypeCache.GetFamilyGroupType().Id;
                var knownRelationshipGroupType = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid(), rockContext );
                var canCheckInRoleId = knownRelationshipGroupType
                    .Roles.Single( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid() ).Id;
                var ownerRoleId = knownRelationshipGroupType
                    .Roles.Single( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ).Id;
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker );
                var brianJones = personService.Get( TestGuids.TestPeople.BrianJones );
                var deckerFamily = tedDecker.PrimaryFamily;
                var removedPersonIdKeys = new List<string> { IdHasher.Instance.GetHash( brianJones.Id ) };

                // Create the known relationship.
                groupMemberService.CreateKnownRelationship( tedDecker.Id, brianJones.Id, canCheckInRoleId );

                // Find Ted's known relationship group and make sure we have
                // the expected 1 relationship that we just created.
                var tedDeckerKnownRelationshipGroupId = groupMemberService.Queryable()
                    .Where( m => m.GroupTypeId == knownRelationshipGroupType.Id
                        && m.PersonId == tedDecker.Id
                        && m.GroupRoleId == ownerRoleId )
                    .Select( m => m.GroupId )
                    .Single();

                var tedRelationshipCount = groupMemberService.Queryable()
                    .Where( m => m.GroupTypeId == knownRelationshipGroupType.Id
                        && m.GroupId == tedDeckerKnownRelationshipGroupId
                        && m.PersonId == brianJones.Id )
                    .Count();

                // Make sure our data is valid before the test.
                Assert.AreEqual( 1, tedRelationshipCount );

                var expectedRelationshipCount = groupMemberService.Queryable()
                    .Count( gm => gm.GroupTypeId == knownRelationshipGroupType.Id ) - 1;

                var saveResult = new FamilyRegistrationSaveResult();

                registration.RemoveFamilyMembers( deckerFamily, removedPersonIdKeys, saveResult );

                var actualTedRelationshipCount = groupMemberService.Queryable()
                    .Where( m => m.GroupTypeId == knownRelationshipGroupType.Id
                        && m.GroupId == tedDeckerKnownRelationshipGroupId
                        && m.PersonId == brianJones.Id )
                    .Count();

                var actualRelationshipCount = groupMemberService.Queryable()
                    .Count( gm => gm.GroupTypeId == knownRelationshipGroupType.Id );

                // Make sure we removed that relationship and no others.
                Assert.AreEqual( 0, actualTedRelationshipCount );
                Assert.AreEqual( expectedRelationshipCount, actualRelationshipCount );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void RemoveFamilyMembers_WithFamilyMember_CreatesNewFamilyGroup()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var groupService = new GroupService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var familyTypeId = GroupTypeCache.GetFamilyGroupType().Id;
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker );
                var noahDecker = personService.Get( TestGuids.TestPeople.NoahDecker );
                var removedPersonIdKeys = new List<string> { IdHasher.Instance.GetHash( noahDecker.Id ) };

                var expectedFamilyCount = groupService.Queryable()
                    .Count( g => g.GroupTypeId == familyTypeId ) + 1;
                var oldNoahDeckerFamilyId = groupMemberService.Queryable()
                    .Where( gm => gm.GroupTypeId == familyTypeId
                        && gm.PersonId == noahDecker.Id )
                    .Select( gm => gm.GroupId )
                    .Single();

                var saveResult = new FamilyRegistrationSaveResult();

                registration.RemoveFamilyMembers( tedDecker.PrimaryFamily, removedPersonIdKeys, saveResult );

                var actualFamilyCount = groupService.Queryable()
                    .Count( g => g.GroupTypeId == familyTypeId );
                var actualNoahDeckerFamilyId = groupMemberService.Queryable()
                    .Where( gm => gm.GroupTypeId == familyTypeId
                        && gm.PersonId == noahDecker.Id )
                    .Select( gm => gm.GroupId )
                    .Single();

                Assert.AreEqual( expectedFamilyCount, actualFamilyCount );
                Assert.AreNotEqual( oldNoahDeckerFamilyId, actualNoahDeckerFamilyId );
            }
        }

        [TestMethod]
        [IsolatedTestDatabase]
        public void RemoveFamilyMembers_WithMultiFamilyMember_DoesNotCreateNewFamilyGroup()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var groupService = new GroupService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var familyTypeId = GroupTypeCache.GetFamilyGroupType().Id;
                var templateConfigurationDataMock = GetTemplateConfigurationDataMock();
                var registration = new FamilyRegistration( rockContext, null, templateConfigurationDataMock.Object );

                var sarahSimmons = personService.Get( TestGuids.TestPeople.SarahSimmons );
                var brianJones = personService.Get( TestGuids.TestPeople.BrianJones );
                var removedPersonIdKeys = new List<string> { IdHasher.Instance.GetHash( brianJones.Id ) };

                var preBrianJonesFamilyCount = groupMemberService.Queryable()
                    .Where( gm => gm.GroupTypeId == familyTypeId
                        && gm.PersonId == brianJones.Id )
                    .Count();

                // Verify that Brian is already in two families.
                Assert.AreEqual( 2, preBrianJonesFamilyCount );

                var expectedFamilyCount = groupService.Queryable()
                    .Count( g => g.GroupTypeId == familyTypeId );

                var saveResult = new FamilyRegistrationSaveResult();

                registration.RemoveFamilyMembers( sarahSimmons.PrimaryFamily, removedPersonIdKeys, saveResult );

                var actualFamilyCount = groupService.Queryable()
                    .Count( g => g.GroupTypeId == familyTypeId );
                var actualBrianJonesFamilyCount = groupMemberService.Queryable()
                    .Where( gm => gm.GroupTypeId == familyTypeId
                        && gm.PersonId == brianJones.Id )
                    .Count();

                Assert.AreEqual( expectedFamilyCount, actualFamilyCount );
                Assert.AreEqual( 1, actualBrianJonesFamilyCount );
            }
        }

        #endregion

        #region Support Methods

        private Mock<TemplateConfigurationData> GetTemplateConfigurationDataMock()
        {
            var mock = new Mock<TemplateConfigurationData>();

            mock.SetupGet( m => m.RequiredAttributeGuidsForFamilies ).Returns( new List<Guid>() );
            mock.SetupGet( m => m.OptionalAttributeGuidsForFamilies ).Returns( new List<Guid>() );
            mock.SetupGet( m => m.RequiredAttributeGuidsForAdults ).Returns( new List<Guid>() );
            mock.SetupGet( m => m.OptionalAttributeGuidsForAdults ).Returns( new List<Guid>() );
            mock.SetupGet( m => m.RequiredAttributeGuidsForChildren ).Returns( new List<Guid>() );
            mock.SetupGet( m => m.OptionalAttributeGuidsForChildren ).Returns( new List<Guid>() );

            mock.SetupGet( m => m.KnownRelationshipRoleGuids ).Returns( new List<Guid>
            {
                SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid(),
                SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid()
            } );
            mock.SetupGet( m => m.SameFamilyKnownRelationshipRoleGuids ).Returns( new List<Guid>
            {
                SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid()
            } );
            mock.SetupGet( m => m.CanCheckInKnownRelationshipRoleGuids ).Returns( new List<Guid>
            {
                SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid()
            } );

            return mock;
        }

        /// <summary>
        /// Creates a mock of RockContext that overloads all SaveChanges
        /// methods to be no-ops. This allows for testing code paths that we
        /// just need to verify that it called SaveChanges, not that it actually
        /// modified the database.
        /// </summary>
        /// <returns>A mock of <see cref="RockContext"/> with SaveChanges disabled.</returns>
        private Mock<RockContext> CreateRockContextWithoutSaveChanges()
        {
            var rockContextMock = new Mock<RockContext>( MockBehavior.Loose )
            {
                CallBase = true
            };

            rockContextMock.Setup( m => m.SaveChanges() )
                .Returns( 0 );

            rockContextMock.Setup( m => m.SaveChanges( It.IsAny<bool>() ) )
                .Returns( 0 );

            rockContextMock.Setup( m => m.SaveChanges( It.IsAny<SaveChangesArgs>() ) )
                .Returns( new SaveChangesResult() );

            return rockContextMock;
        }

        #endregion
    }
}
