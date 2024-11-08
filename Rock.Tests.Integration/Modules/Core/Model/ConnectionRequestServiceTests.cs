using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    /// <summary>
    /// Tests for ConnectionRequestService that use the database
    /// </summary>
    [TestClass]
    public class ConnectionRequestServiceTests : DatabaseTestsBase
    {
        #region Constants

        private const string ForeignKey = nameof( ConnectionRequestServiceTests );

        private const string SecondCampusGuidString = "91E4E480-FE9D-42E1-9BF6-F37C1E19FD50";

        private const string CareTeamConnectionTypeGuidString = "96825939-5C02-4112-AFF7-4EC71FBCA06D";
        private const string YouthProgramConnectionTypeGuidString = "3830A69F-71FF-4923-8D46-DC2BC71F2815";

        private const string JerryJenkinsPersonGuidString = "813D29C7-7E7A-4D60-B5A7-AA251B8DE12A"; // "B8E6242D-B52E-4659-AB13-751A5F4C0BE4";
        private const string KathyKolePersonGuidString = "4D1AF80C-AAB3-4982-9B90-92E3FA78D16C"; // "64BD1D38-D054-488F-86F6-38040242219E";
        private const string BarryBopPersonGuidString = "6D32EAF3-EB78-4E2A-A879-414EC252025D"; // "DFEFD90E-A993-493D-84D8-6903946523DB";
        private const string SimonSandsPersonGuidString = "6C34FA03-0695-423A-A297-E3A6A8350AB9"; // "D2D57C31-89C4-4A92-8917-894B49A42CAE";

        #endregion Constants

        #region Static Properties

        /// <summary>
        /// The person alias jerry jenkins identifier.
        /// Youth Program Host Second Campus connector.
        /// Denied view of Youth Program Host.
        /// </summary>
        private static int PersonAliasJerryJenkinsId;

        /// <summary>
        /// The person alias Kathy Kole identifier.
        /// Youth Program Host Global connector.
        /// Denied view of Youth Program Host.
        /// </summary>
        private static int PersonAliasKathyKoleId;

        /// <summary>
        /// The person alias barry bop identifier.
        /// Denied view of Care Team Hospital.
        /// Denied view of Youth Program Host.
        /// </summary>
        private static int PersonAliasBarryBopId;

        /// <summary>
        /// The person alias simon sands identifier.
        /// </summary>
        private static int PersonAliasSimonSandsId;

        /// <summary>
        /// The type care team identifier.
        /// Has EnableRequestSecurity.
        /// Requires group placement.
        /// </summary>
        private static int TypeCareTeamId;

        /// <summary>
        /// The type youth program identifier.
        /// Does not have EnableRequestSecurity.
        /// Does not require group placement.
        /// </summary>
        private static int TypeYouthProgramId;

        private static int CareTeamOpportunityPrayerPartnerId;
        private static int CareTeamOpportunityHospitalVisitorId;

        private static int YouthProgramOpportunityGroupLeaderId;
        private static int YouthProgramOpportunityHostId;

        private static int CareTeamStatusAlphaId;
        private static int CareTeamStatusBravoId;
        private static int CareTeamStatusCharlieId;

        private static int YouthProgramStatusAlphaId;

        #endregion Static Properties

        #region Setup Methods

        /// <summary>
        /// Creates the test campuses.
        /// </summary>
        private static void CreateTestCampuses()
        {
            var rockContext = new RockContext();
            var campusService = new CampusService( rockContext );

            var firstCampus = CampusCache.All().First();

            var secondCampus = new Campus
            {
                Name = "Second Campus",
                LocationId = firstCampus.LocationId,
                Guid = SecondCampusGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            campusService.Add( secondCampus );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates the test people.
        /// </summary>
        private static void CreateTestPeople()
        {
            LogHelper.Log( "Executing ConnectionRequestServiceTests.CreateTestPeople..." );

            TestDataHelper.DeletePersonByGuid( new List<Guid> { SimonSandsPersonGuidString.AsGuid(),
                BarryBopPersonGuidString.AsGuid(), KathyKolePersonGuidString.AsGuid(), JerryJenkinsPersonGuidString.AsGuid() } );

            var rockContext = new RockContext();

            // Changed order of delete operations. Is it always the second person?

            // People
            var personKathyKole = new Person
            {
                FirstName = "Kathy",
                LastName = "Kole",
                Guid = KathyKolePersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            rockContext = new RockContext();
            PersonService.SaveNewPerson( personKathyKole, rockContext );
            rockContext.SaveChanges();

            var personSimonSands = new Person
            {
                FirstName = "Simon",
                LastName = "Sands",
                Guid = SimonSandsPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            rockContext = new RockContext();
            PersonService.SaveNewPerson( personSimonSands, rockContext );
            rockContext.SaveChanges();

            var personJerryJenkins = new Person
            {
                FirstName = "Jerry",
                LastName = "Jenkins",
                Guid = JerryJenkinsPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            rockContext = new RockContext();
            PersonService.SaveNewPerson( personJerryJenkins, rockContext );
            rockContext.SaveChanges();

// Problem person?
            var personBarryBop = new Person
            {
                FirstName = "Barry",
                LastName = "Bop",
                Guid = BarryBopPersonGuidString.AsGuid(),
                ForeignKey = ForeignKey
            };

            //try
            //{
                rockContext = new RockContext();
                PersonService.SaveNewPerson( personBarryBop, rockContext );
                rockContext.SaveChanges();
            //}
            //catch (Exception ex)
            //{
                //LogHelper.LogError( ex.Message );
            //}

            //personService.Add( personJerryJenkins );
            //personService.Add( personSimonSands );
            //personService.Add( personKathyKole );
            //personService.Add( personBarryBop );

            PersonAliasJerryJenkinsId = personJerryJenkins.PrimaryAliasId.Value;
            PersonAliasBarryBopId = personBarryBop.PrimaryAliasId.Value;
            PersonAliasKathyKoleId = personKathyKole.PrimaryAliasId.Value;
            PersonAliasSimonSandsId = personSimonSands.PrimaryAliasId.Value;

            LogHelper.Log( "Completed ConnectionRequestServiceTests.CreateTestPeople..." );
        }

        /// <summary>
        /// Creates the test connector groups.
        /// </summary>
        private static void CreateTestConnectorGroups()
        {
            var rockContext = new RockContext();
            var connectorGroupService = new ConnectionOpportunityConnectorGroupService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );
            var groupTypeCache = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_SERVING_TEAM );
            var secondCampusId = CampusCache.Get( SecondCampusGuidString ).Id;

            var globalGroup = new Group
            {
                Name = "Global Connector Group",
                GroupTypeId = groupTypeCache.Id,
                ForeignKey = ForeignKey,
                Members = new List<GroupMember> {
                    new GroupMember {
                        PersonId = personAliasService.GetPersonId( PersonAliasKathyKoleId ) ?? 0,
                        GroupRoleId = groupTypeCache.DefaultGroupRoleId ?? 0,
                        ForeignKey = ForeignKey
                    }
                }
            };

            var secondCampusGroup = new Group
            {
                Name = "Second Campus Connector Group",
                GroupTypeId = groupTypeCache.Id,
                ForeignKey = ForeignKey,
                Members = new List<GroupMember> {
                    new GroupMember {
                        PersonId = personAliasService.GetPersonId( PersonAliasJerryJenkinsId ) ?? 0,
                        GroupRoleId = groupTypeCache.DefaultGroupRoleId ?? 0
                    }
                }
            };

            connectorGroupService.Add( new ConnectionOpportunityConnectorGroup
            {
                ConnectorGroup = globalGroup,
                ConnectionOpportunityId = YouthProgramOpportunityHostId,
                CampusId = null,
                ForeignKey = ForeignKey
            } );

            connectorGroupService.Add( new ConnectionOpportunityConnectorGroup
            {
                ConnectorGroup = secondCampusGroup,
                ConnectionOpportunityId = YouthProgramOpportunityHostId,
                CampusId = secondCampusId,
                ForeignKey = ForeignKey
            } );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates the test connection types.
        /// </summary>
        private static void CreateTestConnectionTypes()
        {
            var rockContext = new RockContext();
            var connectionTypeService = new ConnectionTypeService( rockContext );

            var typeCareTeamGuid = CareTeamConnectionTypeGuidString.AsGuid();
            var typeYouthProgramGuid = YouthProgramConnectionTypeGuidString.AsGuid();

            // Statuses
            var youthProgramStatusAlpha = new ConnectionStatus
            {
                Name = "Youth Program Status: Alpha",
                ForeignKey = ForeignKey
            };

            var careTeamStatusAlpha = new ConnectionStatus
            {
                Name = "Care Team Status: Alpha",
                ForeignKey = ForeignKey
            };

            var careTeamStatusBravo = new ConnectionStatus
            {
                Name = "Care Team Status: Bravo",
                ForeignKey = ForeignKey
            };

            var careTeamStatusCharlie = new ConnectionStatus
            {
                Name = "Care Team Status: Charlie",
                ForeignKey = ForeignKey
            };

            // Opportunities
            var youthProgramOpportunityGroupLeader = new ConnectionOpportunity
            {
                Name = "Youth Program Opportunity: Group Leader",
                PublicName = "Youth Program Opportunity: Group Leader",
                ForeignKey = ForeignKey
            };

            var youthProgramOpportunityHost = new ConnectionOpportunity
            {
                Name = "Youth Program Opportunity: Host",
                PublicName = "Youth Program Opportunity: Host",
                ForeignKey = ForeignKey
            };

            var careTeamOpportunityHospitalVisitor = new ConnectionOpportunity
            {
                Name = "Care Team Opportunity: Hopsital Visitor",
                PublicName = "Care Team Opportunity: Hopsital Visitor",
                ForeignKey = ForeignKey
            };

            var careTeamOpportunityPrayerPartner = new ConnectionOpportunity
            {
                Name = "Care Team Opportunity: PrayerPartner",
                PublicName = "Care Team Opportunity: PrayerPartner",
                ForeignKey = ForeignKey
            };

            // Types
            var typeYouthProgram = new ConnectionType
            {
                Name = "Type: Youth Program",
                Guid = typeYouthProgramGuid,
                RequiresPlacementGroupToConnect = false,
                EnableRequestSecurity = false,
                ConnectionStatuses = new List<ConnectionStatus> {
                    youthProgramStatusAlpha
                },
                ConnectionOpportunities = new List<ConnectionOpportunity> {
                    youthProgramOpportunityGroupLeader,
                    youthProgramOpportunityHost
                },
                ForeignKey = ForeignKey
            };

            var typeCareTeam = new ConnectionType
            {
                Name = "Type: Care Team",
                Guid = typeCareTeamGuid,
                RequiresPlacementGroupToConnect = true,
                EnableRequestSecurity = true,
                ConnectionStatuses = new List<ConnectionStatus> {
                    careTeamStatusAlpha,
                    careTeamStatusBravo,
                    careTeamStatusCharlie
                },
                ConnectionOpportunities = new List<ConnectionOpportunity> {
                    careTeamOpportunityHospitalVisitor,
                    careTeamOpportunityPrayerPartner
                },
                ForeignKey = ForeignKey
            };

            // Save to the database
            connectionTypeService.Add( typeCareTeam );
            connectionTypeService.Add( typeYouthProgram );
            rockContext.SaveChanges();

            // Set static ids
            TypeCareTeamId = typeCareTeam.Id;
            TypeYouthProgramId = typeYouthProgram.Id;

            CareTeamOpportunityHospitalVisitorId = careTeamOpportunityHospitalVisitor.Id;
            CareTeamOpportunityPrayerPartnerId = careTeamOpportunityPrayerPartner.Id;

            YouthProgramOpportunityGroupLeaderId = youthProgramOpportunityGroupLeader.Id;
            YouthProgramOpportunityHostId = youthProgramOpportunityHost.Id;

            CareTeamStatusAlphaId = careTeamStatusAlpha.Id;
            CareTeamStatusBravoId = careTeamStatusBravo.Id;
            CareTeamStatusCharlieId = careTeamStatusCharlie.Id;

            YouthProgramStatusAlphaId = youthProgramStatusAlpha.Id;
        }

        /// <summary>
        /// Creates the test workflow triggers.
        /// </summary>
        private static void CreateTestWorkflowTriggers()
        {
            var rockContext = new RockContext();
            var connectionWorkflowService = new ConnectionWorkflowService( rockContext );
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var firstWorkflowTypeId = workflowTypeService.Queryable().FirstOrDefault()?.Id ?? 0;

            connectionWorkflowService.Add( new ConnectionWorkflow
            {
                WorkflowTypeId = firstWorkflowTypeId,
                TriggerType = ConnectionWorkflowTriggerType.ActivityAdded,
                ConnectionOpportunityId = CareTeamOpportunityHospitalVisitorId
            } );

            connectionWorkflowService.Add( new ConnectionWorkflow
            {
                WorkflowTypeId = firstWorkflowTypeId,
                TriggerType = ConnectionWorkflowTriggerType.StatusChanged,
                QualifierValue = $"|{CareTeamStatusBravoId}|{CareTeamStatusAlphaId}|",
                ConnectionOpportunityId = CareTeamOpportunityHospitalVisitorId
            } );

            connectionWorkflowService.Add( new ConnectionWorkflow
            {
                WorkflowTypeId = firstWorkflowTypeId,
                TriggerType = ConnectionWorkflowTriggerType.StatusChanged,
                QualifierValue = $"||{CareTeamStatusCharlieId}|",
                ConnectionTypeId = TypeCareTeamId
            } );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates the test permissions.
        /// </summary>
        private static void CreateTestPermissions()
        {
            var rockContext = new RockContext();
            var authService = new AuthService( rockContext );

            authService.Add( new Auth
            {
                Action = "View",
                AllowOrDeny = "D",
                EntityTypeId = EntityTypeCache.Get<ConnectionOpportunity>().Id,
                EntityId = YouthProgramOpportunityHostId,
                PersonAliasId = PersonAliasBarryBopId,
                ForeignKey = ForeignKey
            } );

            authService.Add( new Auth
            {
                Action = "View",
                AllowOrDeny = "D",
                EntityTypeId = EntityTypeCache.Get<ConnectionOpportunity>().Id,
                EntityId = YouthProgramOpportunityHostId,
                PersonAliasId = PersonAliasKathyKoleId,
                ForeignKey = ForeignKey
            } );

            authService.Add( new Auth
            {
                Action = "View",
                AllowOrDeny = "D",
                EntityTypeId = EntityTypeCache.Get<ConnectionOpportunity>().Id,
                EntityId = YouthProgramOpportunityHostId,
                PersonAliasId = PersonAliasJerryJenkinsId,
                ForeignKey = ForeignKey
            } );

            authService.Add( new Auth
            {
                Action = "View",
                AllowOrDeny = "D",
                EntityTypeId = EntityTypeCache.Get<ConnectionOpportunity>().Id,
                EntityId = CareTeamOpportunityHospitalVisitorId,
                PersonAliasId = PersonAliasBarryBopId,
                ForeignKey = ForeignKey
            } );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Adds the requests.
        /// </summary>
        /// <param name="opportunityIds">The opportunity ids.</param>
        /// <param name="statusIds">The status ids.</param>
        /// <param name="activityDateSeed">The activity date seed.</param>
        private static void CreateTestRequests( int[] opportunityIds, int[] statusIds, DateTime activityDateSeed )
        {
            var campusIds = CampusCache.All().Select( c => c.Id as int? ).ToList();
            campusIds.Add( null );

            var rockContext = new RockContext();
            var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
            var connectionRequestService = new ConnectionRequestService( rockContext );

            var states = Enum.GetValues( typeof( ConnectionState ) ).Cast<ConnectionState>().ToList();
            var random = new Random();

            var connectionActivityTypeId = connectionActivityTypeService.Queryable()
                .FirstOrDefault( cat => !cat.ConnectionTypeId.HasValue )?
                .Id ?? 0;

            var personAliasIds = new int[] {
                PersonAliasSimonSandsId,
                PersonAliasBarryBopId,
                PersonAliasKathyKoleId,
                PersonAliasJerryJenkinsId
            };

            foreach ( var campusId in campusIds )
            {
                foreach ( var opportunityId in opportunityIds )
                {
                    foreach ( var requesterAliasId in personAliasIds )
                    {
                        foreach ( var connectorAliasId in personAliasIds )
                        {
                            foreach ( var statusId in statusIds )
                            {
                                var state = states[random.Next( states.Count )];

                                var requestWithNoConnector = new ConnectionRequest
                                {
                                    ConnectorPersonAliasId = null,
                                    CampusId = campusId,
                                    ConnectionOpportunityId = opportunityId,
                                    PersonAliasId = requesterAliasId,
                                    ForeignKey = ForeignKey,
                                    ConnectionStatusId = statusId,
                                    ConnectionState = state,
                                    ConnectionRequestActivities = new List<ConnectionRequestActivity> {
                                        new ConnectionRequestActivity {
                                            CreatedDateTime = activityDateSeed.AddYears(1),
                                            ConnectionOpportunityId = opportunityId,
                                            ConnectionActivityTypeId = connectionActivityTypeId,
                                            ForeignKey = ForeignKey
                                        },
                                        new ConnectionRequestActivity {
                                            CreatedDateTime = activityDateSeed.AddMonths(2),
                                            ConnectionOpportunityId = opportunityId,
                                            ConnectionActivityTypeId = connectionActivityTypeId,
                                            ForeignKey = ForeignKey
                                        }
                                    }
                                };

                                var requestWithConnector = new ConnectionRequest
                                {
                                    ConnectorPersonAliasId = connectorAliasId,
                                    CampusId = campusId,
                                    ConnectionOpportunityId = opportunityId,
                                    PersonAliasId = requesterAliasId,
                                    ForeignKey = ForeignKey,
                                    ConnectionStatusId = statusId,
                                    ConnectionState = state,
                                    ConnectionRequestActivities = new List<ConnectionRequestActivity> {
                                        new ConnectionRequestActivity {
                                            CreatedDateTime = activityDateSeed,
                                            ConnectionOpportunityId = opportunityId,
                                            ConnectionActivityTypeId = connectionActivityTypeId,
                                            ForeignKey = ForeignKey
                                        },
                                        new ConnectionRequestActivity {
                                            CreatedDateTime = activityDateSeed.AddMonths(1),
                                            ConnectionOpportunityId = opportunityId,
                                            ConnectionActivityTypeId = connectionActivityTypeId,
                                            ForeignKey = ForeignKey
                                        }
                                    }
                                };

                                // Make about half the future follow up dates past due
                                if ( state == ConnectionState.FutureFollowUp )
                                {
                                    var isPastDue = random.Next( 2 ) == 0;
                                    var followupDate = RockDateTime.Now.AddDays( isPastDue ? -2 : 2 );
                                    requestWithConnector.FollowupDate = followupDate;
                                    requestWithNoConnector.FollowupDate = followupDate;
                                }

                                connectionRequestService.Add( requestWithConnector );
                                connectionRequestService.Add( requestWithNoConnector );
                                activityDateSeed = activityDateSeed.AddDays( 1 );
                            }
                        }

                        rockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Create the data used to test
        /// </summary>
        private static void CreateTestData()
        {
            CreateTestCampuses();
            CreateTestPeople();
            CreateTestConnectionTypes();
            CreateTestWorkflowTriggers();
            CreateTestPermissions();
            CreateTestConnectorGroups();

            // Requests
            var careTeamOpportunityIds = new int[] {
                CareTeamOpportunityHospitalVisitorId,
                CareTeamOpportunityPrayerPartnerId
            };

            var careTeamStatusIds = new int[] {
                CareTeamStatusAlphaId,
                CareTeamStatusBravoId,
                CareTeamStatusCharlieId
            };

            var youthProgramStatusIds = new int[] {
                YouthProgramStatusAlphaId
            };

            var youthProgramOpportunityIds = new int[] {
                YouthProgramOpportunityGroupLeaderId,
                YouthProgramOpportunityHostId
            };

            CreateTestRequests( careTeamOpportunityIds, careTeamStatusIds, new DateTime( 2000, 1, 1 ) );
            CreateTestRequests( youthProgramOpportunityIds, youthProgramStatusIds, new DateTime( 2000, 5, 5 ) );
        }

        /// <summary>
        /// Delete test data
        /// </summary>
        private static void DeleteTestData()
        {
            var campusGuids = new Guid[] {
                SecondCampusGuidString.AsGuid()
            };

            var personGuids = new Guid[] {
                JerryJenkinsPersonGuidString.AsGuid(),
                BarryBopPersonGuidString.AsGuid(),
                KathyKolePersonGuidString.AsGuid(),
                SimonSandsPersonGuidString.AsGuid()
            };

            var typeGuids = new Guid[] {
                CareTeamConnectionTypeGuidString.AsGuid(),
                YouthProgramConnectionTypeGuidString.AsGuid()
            };

            var rockContext = new RockContext();

            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
            var activityQuery = connectionRequestActivityService.Queryable().Where( cra => typeGuids.Contains( cra.ConnectionOpportunity.ConnectionType.Guid ) );
            connectionRequestActivityService.DeleteRange( activityQuery );

            var connectionRequestService = new ConnectionRequestService( rockContext );
            var requestQuery = connectionRequestService.Queryable().Where( cr => typeGuids.Contains( cr.ConnectionOpportunity.ConnectionType.Guid ) );
            connectionRequestService.DeleteRange( requestQuery );

            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var opportunityQuery = connectionOpportunityService.Queryable().Where( co => typeGuids.Contains( co.ConnectionType.Guid ) );
            connectionOpportunityService.DeleteRange( opportunityQuery );

            var connectionTypeService = new ConnectionTypeService( rockContext );
            var typeQuery = connectionTypeService.Queryable().Where( ct => typeGuids.Contains( ct.Guid ) );
            connectionTypeService.DeleteRange( typeQuery );

            //var personSearchKeyService = new PersonSearchKeyService( rockContext );
            //var personSearchKeyQuery = personSearchKeyService.Queryable().Where( psk => personGuids.Contains( psk.PersonAlias.Person.Guid ) );
            //personSearchKeyService.DeleteRange( personSearchKeyQuery );

            //var authService = new AuthService( rockContext );
            //var authQuery = authService.Queryable().Where( a => personGuids.Contains( a.PersonAlias.Person.Guid ) );
            //authService.DeleteRange( authQuery );

            rockContext.SaveChanges();

            TestDataHelper.DeletePersonByGuid( personGuids );

            //var personAliasService = new PersonAliasService( rockContext );
            //var personAliasQuery = personAliasService.Queryable().Where( pa => personGuids.Contains( pa.Person.Guid ) );
            //personAliasService.DeleteRange( personAliasQuery );

            //var personService = new PersonService( rockContext );
            //var personQuery = personService.Queryable().Where( p => personGuids.Contains( p.Guid ) );
            //personService.DeleteRange( personQuery );

            var campusService = new CampusService( rockContext );
            var campusQuery = campusService.Queryable().Where( c => campusGuids.Contains( c.Guid ) );
            campusService.DeleteRange( campusQuery );

            var groupService = new GroupService( rockContext );
            var groupQuery = groupService.Queryable().Where( g => g.ForeignKey == ForeignKey );
            groupService.DeleteRange( groupQuery );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext _ )
        {
            DeleteTestData();
            CreateTestData();
        }

        /// <summary>
        /// Runs after all tests in this class is executed.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            DeleteTestData();
        }

        #endregion Setup Methods

        #region CanConnect

        /// <summary>
        /// Tests CanConnect.
        /// The Care Team requires group placement, but the request doesn't have a group id.
        /// </summary>
        [TestMethod]
        public void CanConnect_RequiredPlacementNotMet()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );
            var result = service.CanConnect(
                new ConnectionRequestViewModel
                {
                    PlacementGroupId = null,
                    ConnectionState = ConnectionState.Active
                },
                new ConnectionOpportunity
                {
                    ShowConnectButton = true
                }, ConnectionTypeCache.Get( TypeCareTeamId ) );

            Assert.That.AreEqual( false, result );
        }

        /// <summary>
        /// Tests CanConnect.
        /// The care team requires placement and the request has a group id. Also, the connection state
        /// is not inactive.
        /// </summary>
        [TestMethod]
        public void CanConnect_RequiredPlacementMet()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.CanConnect(
                new ConnectionRequestViewModel
                {
                    PlacementGroupId = 1,
                    ConnectionState = ConnectionState.Active
                },
                new ConnectionOpportunity
                {
                    ShowConnectButton = true
                },
                ConnectionTypeCache.Get( TypeCareTeamId ) );

            Assert.That.AreEqual( true, result );
        }

        /// <summary>
        /// Tests CanConnect.
        /// Youth Program does not require placement. The state is active.
        /// </summary>
        [TestMethod]
        public void CanConnect_NotRequiredPlacementActive()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.CanConnect(
                new ConnectionRequestViewModel
                {
                    PlacementGroupId = null,
                    ConnectionState = ConnectionState.Active
                },
                new ConnectionOpportunity
                {
                    ShowConnectButton = true
                },
                ConnectionTypeCache.Get( TypeYouthProgramId ) );

            Assert.That.AreEqual( true, result );
        }

        /// <summary>
        /// Tests CanConnect.
        /// Youth program does not require placement, but the request is inactive.
        /// </summary>
        [TestMethod]
        public void CanConnect_NotRequiredPlacementInactive()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.CanConnect(
                new ConnectionRequestViewModel
                {
                    PlacementGroupId = null,
                    ConnectionState = ConnectionState.Inactive
                },
                new ConnectionOpportunity
                {
                    ShowConnectButton = true
                }, ConnectionTypeCache.Get( TypeYouthProgramId ) );

            Assert.That.AreEqual( false, result );
        }

        #endregion CanConnect

        #region DoesStatusChangeCauseWorkflows

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows.
        /// Changing from alpha to bravo does not cause workflows.
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_AlphaToBravo()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( CareTeamOpportunityHospitalVisitorId, CareTeamStatusAlphaId, CareTeamStatusBravoId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( false, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Care Team Status: Alpha", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Bravo", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows.
        /// Changing from bravo to alpha does cause workflows for hospital visitor.
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_BravoToAlpha()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( CareTeamOpportunityHospitalVisitorId, CareTeamStatusBravoId, CareTeamStatusAlphaId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( true, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Care Team Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Alpha", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows.
        /// Any status change to charlie causes workflows for the care team type.
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_BravoToCharlie()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( CareTeamOpportunityHospitalVisitorId, CareTeamStatusBravoId, CareTeamStatusCharlieId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( true, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Care Team Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Charlie", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows.
        /// Any status change to charlie causes workflows for the care team type.
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_AlphaToCharlie()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( CareTeamOpportunityHospitalVisitorId, CareTeamStatusAlphaId, CareTeamStatusCharlieId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( true, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Care Team Status: Alpha", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Charlie", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows
        /// Charlie to alpha does not cause workflows.
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_CharlieToAlpha()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( CareTeamOpportunityHospitalVisitorId, CareTeamStatusCharlieId, CareTeamStatusAlphaId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( false, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Care Team Status: Charlie", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Alpha", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows.
        /// Bravo to alpha workfows only trigger for hospital visitor.
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_BravoToAlpha_PrayerPartner()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( CareTeamOpportunityPrayerPartnerId, CareTeamStatusBravoId, CareTeamStatusAlphaId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( false, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Care Team Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Alpha", result.ToStatusName );
        }

        /// <summary>
        /// Tests DoesStatusChangeCauseWorkflows.
        /// Any change to charlie for the care team type causes workflows.
        /// </summary>
        [TestMethod]
        public void DoesStatusChangeCauseWorkflows_BravoToCharlie_PrayerPartner()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var result = service.DoesStatusChangeCauseWorkflows( CareTeamOpportunityPrayerPartnerId, CareTeamStatusBravoId, CareTeamStatusCharlieId );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( true, result.DoesCauseWorkflows );
            Assert.That.AreEqual( "Care Team Status: Bravo", result.FromStatusName );
            Assert.That.AreEqual( "Care Team Status: Charlie", result.ToStatusName );
        }

        #endregion DoesStatusChangeCauseWorkflows

        #region GetConnectionBoardStatusViewModels

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels.
        /// Simon can see everything, but the count limit should apply to the number of requests returned.
        /// The actual count however should be the number in the database.
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_MaxRequestsPerCol()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var args = new ConnectionRequestViewModelQueryArgs { };
            var maxRequestsPerCol = 3;
            var opportunityId = CareTeamOpportunityHospitalVisitorId;

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasSimonSandsId,
                opportunityId,
                args,
                null,
                maxRequestsPerCol );

            Assert.That.IsNotNull( result );

            foreach ( var statusViewModel in result )
            {
                var actualCount = service.Queryable().Count( cr =>
                    cr.ConnectionStatusId == statusViewModel.Id &&
                    cr.ConnectionOpportunityId == opportunityId );

                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.AreEqual( maxRequestsPerCol, statusViewModel.Requests.Count );
                Assert.That.AreEqual( actualCount, statusViewModel.RequestCount );
            }
        }

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels.
        /// Barry Bop does not have permission to view Host opportunity.
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_AuthDeniesOpportunity()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var args = new ConnectionRequestViewModelQueryArgs { };

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasBarryBopId,
                YouthProgramOpportunityHostId,
                args );

            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.AreEqual( 0, statusViewModel.Requests.Count );
            }
        }

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels.
        /// Barry Bop does not have permission to view Hospital Visitor. However, since the Care Team has
        /// Enable Request Security, Barry can view his assigned requests.
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_EnableRequestSecurityAllowsAssigned()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var args = new ConnectionRequestViewModelQueryArgs { };

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasBarryBopId,
                CareTeamOpportunityHospitalVisitorId,
                args );

            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.IsTrue( statusViewModel.Requests.Count > 0 );

                foreach ( var request in statusViewModel.Requests )
                {
                    Assert.That.AreEqual( PersonAliasBarryBopId, request.ConnectorPersonAliasId );
                }
            }
        }

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels.
        /// Kathy Kole is a global (no specific campus) connector for the Youth Program host opportunity.
        /// Kathy Kole is denied view of that same opportunity, but can view them anyway because she is
        /// a connector.
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_GlobalConnector()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var args = new ConnectionRequestViewModelQueryArgs { };

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasKathyKoleId,
                YouthProgramOpportunityHostId,
                args );

            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.IsTrue( statusViewModel.Requests.Count > 0 );
                Assert.That.IsTrue( statusViewModel.Requests.Any( r => r.ConnectorPersonAliasId != PersonAliasKathyKoleId ) );
            }
        }

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels.
        /// Jerry Jenkins is a second campus connector for the Youth Program host opportunity.
        /// Jerry Jenkins is denied view of that same opportunity, but can view requests of
        /// that campus because he is a connector.
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_CampusConnector()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );
            var campusId = CampusCache.Get( SecondCampusGuidString ).Id;

            var args = new ConnectionRequestViewModelQueryArgs { };

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasJerryJenkinsId,
                YouthProgramOpportunityHostId,
                args );

            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.IsTrue( statusViewModel.Requests.Count > 0 );

                foreach ( var request in statusViewModel.Requests )
                {
                    Assert.That.AreEqual( campusId, request.CampusId );
                }
            }
        }

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels.
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_FiltersStates()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var expectedState = ConnectionState.Active;

            var args = new ConnectionRequestViewModelQueryArgs
            {
                ConnectionStates = new List<ConnectionState> {
                    expectedState
                }
            };

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasSimonSandsId,
                CareTeamOpportunityHospitalVisitorId,
                args );

            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.IsTrue( statusViewModel.Requests.Count > 0 );

                foreach ( var request in statusViewModel.Requests )
                {
                    Assert.That.AreEqual( expectedState, request.ConnectionState );
                }
            }
        }

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels.
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_FiltersPastDueOnly()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var args = new ConnectionRequestViewModelQueryArgs
            {
                IsFutureFollowUpPastDueOnly = true
            };

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasSimonSandsId,
                CareTeamOpportunityHospitalVisitorId,
                args );

            var midnightToday = RockDateTime.Today.AddDays( 1 );
            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            var foundFutureFollowup = false;
            var foundOtherState = false;

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.IsTrue( statusViewModel.Requests.Count > 0 );

                foreach ( var request in statusViewModel.Requests )
                {
                    if ( request.ConnectionState == ConnectionState.FutureFollowUp )
                    {
                        foundFutureFollowup = true;
                        Assert.IsNotNull( request.FollowupDate );
                        Assert.That.IsTrue( request.FollowupDate.Value < midnightToday );
                    }
                    else
                    {
                        foundOtherState = true;
                    }
                }
            }

            Assert.IsTrue( foundOtherState );
            Assert.IsTrue( foundFutureFollowup );
        }

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_FiltersConnectors()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );

            var expectedConnector = PersonAliasKathyKoleId;

            var args = new ConnectionRequestViewModelQueryArgs
            {
                ConnectorPersonAliasId = expectedConnector
            };

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasSimonSandsId,
                CareTeamOpportunityHospitalVisitorId,
                args );

            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.IsTrue( statusViewModel.Requests.Count > 0 );

                foreach ( var request in statusViewModel.Requests )
                {
                    Assert.That.AreEqual( expectedConnector, request.ConnectorPersonAliasId );
                }
            }
        }

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_FiltersDateRange()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );
            var expectedYear = 2001;

            var args = new ConnectionRequestViewModelQueryArgs
            {
                MinDate = new DateTime( expectedYear, 1, 1 ),
                MaxDate = new DateTime( expectedYear, 12, 31 )
            };

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasSimonSandsId,
                CareTeamOpportunityHospitalVisitorId,
                args );

            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.IsTrue( statusViewModel.Requests.Count > 0 );

                foreach ( var request in statusViewModel.Requests )
                {
                    Assert.That.IsNotNull( request.LastActivityDate );
                    Assert.That.AreEqual( expectedYear, request.LastActivityDate.Value.Year );
                }
            }
        }

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_FiltersRequester()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );
            var expectedRequester = PersonAliasJerryJenkinsId;

            var args = new ConnectionRequestViewModelQueryArgs
            {
                RequesterPersonAliasId = expectedRequester
            };

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasSimonSandsId,
                CareTeamOpportunityHospitalVisitorId,
                args );

            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.IsTrue( statusViewModel.Requests.Count > 0 );

                foreach ( var request in statusViewModel.Requests )
                {
                    Assert.That.AreEqual( expectedRequester, request.PersonAliasId );
                }
            }
        }

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_FiltersStatus()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );
            var expectedStatus = CareTeamStatusCharlieId;

            var args = new ConnectionRequestViewModelQueryArgs
            {
                StatusIds = new List<int> {
                    expectedStatus
                }
            };

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasSimonSandsId,
                CareTeamOpportunityHospitalVisitorId,
                args );

            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.IsTrue( statusViewModel.Requests.Count > 0 );

                foreach ( var request in statusViewModel.Requests )
                {
                    Assert.That.AreEqual( expectedStatus, request.StatusId );
                }
            }
        }

        /// <summary>
        /// Tests GetConnectionBoardStatusViewModels
        /// </summary>
        [TestMethod]
        public void GetConnectionBoardStatusViewModels_FiltersCampus()
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );
            var expectedCampusId = CampusCache.Get( SecondCampusGuidString ).Id;

            var args = new ConnectionRequestViewModelQueryArgs
            {
                CampusId = expectedCampusId
            };

            var result = service.GetConnectionBoardStatusViewModels(
                PersonAliasSimonSandsId,
                CareTeamOpportunityHospitalVisitorId,
                args );

            Assert.That.IsNotNull( result );
            Assert.That.IsTrue( result.Count > 0 );

            foreach ( var statusViewModel in result )
            {
                Assert.That.IsNotNull( statusViewModel.Requests );
                Assert.That.IsTrue( statusViewModel.Requests.Count > 0 );

                foreach ( var request in statusViewModel.Requests )
                {
                    Assert.That.AreEqual( expectedCampusId, request.CampusId );
                }
            }
        }

        #endregion GetConnectionBoardStatusViewModels
    }
}
