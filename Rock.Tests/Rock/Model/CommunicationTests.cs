using RockModel = Rock.Model;
using Xunit;
using System;
using System.Collections.Generic;

namespace Rock.Tests.Rock.Model
{
    public class CommunicationTests
    {
        private int EmailMediumEntityTypeId => 1;
        private int SmsMediumEntityTypeId => 2;

        [Fact]
        public void DetermineMediumEntityTypeId_Should_Throw_Exception_For_Invalid_CommunicationType()
        {
            Assert.Throws<ArgumentException>( () => RockModel.Communication.DetermineMediumEntityTypeId( EmailMediumEntityTypeId,
                                                 SmsMediumEntityTypeId, RockModel.CommunicationType.PushNotification ) );

            Assert.Throws<ArgumentException>( () => RockModel.Communication.DetermineMediumEntityTypeId( EmailMediumEntityTypeId,
                                                 SmsMediumEntityTypeId, ( RockModel.CommunicationType ) 10 ) );
        }

        private struct CommunicationTestCase
        {
            public int ExpectedMediumEntityTypeId;
            public RockModel.CommunicationType[] CommunicationTypes;
        }

        [Fact]
        public void DetermineMediumEntityTypeId_Return_Correct_MediumEntityTypeId()
        {
            var testCases = new List<CommunicationTestCase> {
                new CommunicationTestCase
                {
                    ExpectedMediumEntityTypeId = EmailMediumEntityTypeId,
                    CommunicationTypes = new RockModel.CommunicationType[] {
                        RockModel.CommunicationType.Email,
                        RockModel.CommunicationType.SMS,
                        RockModel.CommunicationType.RecipientPreference
                    }
                },
                new CommunicationTestCase
                {
                    ExpectedMediumEntityTypeId = SmsMediumEntityTypeId,
                    CommunicationTypes = new RockModel.CommunicationType[] {
                        RockModel.CommunicationType.SMS,
                        RockModel.CommunicationType.Email,
                        RockModel.CommunicationType.RecipientPreference
                    }
                },
                new CommunicationTestCase
                {
                    ExpectedMediumEntityTypeId = EmailMediumEntityTypeId,
                    CommunicationTypes = new RockModel.CommunicationType[] {
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.Email,
                        RockModel.CommunicationType.SMS
                    }
                },
                new CommunicationTestCase
                {
                    ExpectedMediumEntityTypeId = SmsMediumEntityTypeId,
                    CommunicationTypes = new RockModel.CommunicationType[] {
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.SMS,
                        RockModel.CommunicationType.Email
                    }
                },
                new CommunicationTestCase
                {
                    ExpectedMediumEntityTypeId = EmailMediumEntityTypeId,
                    CommunicationTypes = new RockModel.CommunicationType[] {
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.Email
                    }
                },
                new CommunicationTestCase
                {
                    ExpectedMediumEntityTypeId = SmsMediumEntityTypeId,
                    CommunicationTypes = new RockModel.CommunicationType[] {
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.SMS
                    }
                },
                new CommunicationTestCase
                {
                    ExpectedMediumEntityTypeId = EmailMediumEntityTypeId,
                    CommunicationTypes = new RockModel.CommunicationType[] {
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference
                    }
                },
                new CommunicationTestCase
                {
                    ExpectedMediumEntityTypeId = EmailMediumEntityTypeId,
                    CommunicationTypes = new RockModel.CommunicationType[] {
                        RockModel.CommunicationType.RecipientPreference
                    }
                },
                new CommunicationTestCase
                {
                    ExpectedMediumEntityTypeId = SmsMediumEntityTypeId,
                    CommunicationTypes = new RockModel.CommunicationType[] {
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.RecipientPreference,
                        RockModel.CommunicationType.SMS
                    }
                }
            };

            foreach ( var testCase in testCases )
            {
                var actualMediumEntityTypeId = RockModel.Communication.DetermineMediumEntityTypeId( EmailMediumEntityTypeId,
                                                SmsMediumEntityTypeId, testCase.CommunicationTypes );

                Assert.Equal( testCase.ExpectedMediumEntityTypeId, actualMediumEntityTypeId );
            }
        }
    }
}