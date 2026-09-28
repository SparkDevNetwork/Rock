// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Blocks.Core;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestFramework;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.CampusDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using HttpStatusCode = System.Net.HttpStatusCode;

namespace Rock.Blocks.Tests.Core;

/// <summary>
/// Tests for the <see cref="CampusDetail"/> Obsidian block. These run the
/// block's initialization and block actions against a mocked database.
/// </summary>
[TestClass]
public class CampusDetailTests
{
    #region Initialization

    [TestMethod]
    public void Initialization_WithUnknownCampusId_ReturnsNotFoundError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        SeedLookupData( rockContext );

        var block = CreateBlock( rockContext, campusId: "999" );

        var box = GetInitializationBox( block );

        Assert.IsNull( box.Entity );
        Assert.AreEqual( "The Campus was not found.", box.ErrorMessage );
        Assert.IsNull( box.SecurityGrantToken );
    }

    [TestMethod]
    public void Initialization_WithExistingCampus_ReturnsViewBag()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        campus.PhoneNumber = "(623) 555-1212";
        campus.CampusStatusValue = data.OtherDefinedValue;
#pragma warning disable CS0612, CS0618
        campus.ServiceTimes = "Sunday^9:00am|malformed|Sunday^11:00am";
#pragma warning restore CS0612, CS0618

        var block = CreateBlock( rockContext, campusId: campus.Id.ToString() );
        AllowView( rockContext, block );
        DenyEdit( rockContext, block );

        var box = GetInitializationBox( block );

        Assert.IsNull( box.ErrorMessage );
        Assert.IsFalse( box.IsEditable );
        Assert.IsNotNull( box.SecurityGrantToken );
        Assert.AreEqual( nameof( Campus ), box.EntityTypeName );
        Assert.AreEqual( "Main Campus", box.Entity.Name );
        Assert.AreEqual( campus.IdKey, box.Entity.IdKey );
        Assert.AreEqual( "(623) 555-1212", box.Entity.PhoneNumber );
        Assert.AreEqual( data.OtherDefinedValue.Guid.ToString(), box.Entity.CampusStatusValue.Value, "Existing campus should use its own status, not the default." );
        Assert.HasCount( 2, box.Entity.ServiceTimes, "Malformed service time segments should be skipped." );
        Assert.AreEqual( "11:00am", box.Entity.ServiceTimes[1].Text );
        Assert.AreEqual( "Main Campus Location", box.Entity.Location.Text );
        Assert.IsFalse( box.Options.IsMultiTimeZoneSupported );
        Assert.IsNull( box.Options.TimeZoneOptions );
        Assert.IsFalse( string.IsNullOrEmpty( box.NavigationUrls["ParentPage"] ) );
    }

    [TestMethod]
    public void Initialization_WithExistingCampusAndViewDenied_ReturnsNotAuthorizedToView()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext, campusId: campus.Id.ToString() );
        DenyView( rockContext, block );

        var box = GetInitializationBox( block );

        Assert.IsNull( box.Entity );
        Assert.Contains( "not authorized to view", box.ErrorMessage );
    }

    [TestMethod]
    public void Initialization_WithExistingCampusAndEditAllowed_IsEditable()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext, campusId: campus.Id.ToString() );
        AllowEdit( rockContext, block );

        var box = GetInitializationBox( block );

        Assert.IsTrue( box.IsEditable );
        Assert.IsNotNull( box.Entity );
    }

    [TestMethod]
    public void Initialization_WithNewCampusAndEditAllowed_ReturnsEditBagWithDefaults()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );

        var block = CreateBlock( rockContext, campusId: "0" );
        AllowEdit( rockContext, block );

        var box = GetInitializationBox( block );

        Assert.IsNull( box.ErrorMessage );
        Assert.IsTrue( box.IsEditable );
        Assert.AreEqual( data.CampusStatusOpen.Guid.ToString(), box.Entity.CampusStatusValue.Value );
        Assert.AreEqual( data.CampusTypePhysical.Guid.ToString(), box.Entity.CampusTypeValue.Value );
        Assert.IsTrue( box.Entity.IsActive.Value, "A new campus should default to active." );
        Assert.IsEmpty( box.Entity.ServiceTimes );
        Assert.IsEmpty( box.Entity.CampusSchedules );
        Assert.IsEmpty( box.Entity.CampusTopics );
    }

    [TestMethod]
    public void Initialization_WithNewCampusAndNoDefaultValues_LeavesStatusAndTypeEmpty()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        rockContext.Set<DefinedValue>().Remove( data.CampusStatusOpen );
        rockContext.Set<DefinedValue>().Remove( data.CampusTypePhysical );

        var block = CreateBlock( rockContext, campusId: "0" );
        AllowEdit( rockContext, block );

        var box = GetInitializationBox( block );

        Assert.IsNull( box.ErrorMessage );
        Assert.IsNull( box.Entity.CampusStatusValue.Value );
        Assert.IsNull( box.Entity.CampusStatusValue.Text );
        Assert.IsNull( box.Entity.CampusTypeValue.Value );
        Assert.IsNull( box.Entity.CampusTypeValue.Text );
    }

    [TestMethod]
    public void Initialization_WithNewCampusAndEditDenied_ReturnsNotAuthorizedToEdit()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        SeedLookupData( rockContext );

        // No page parameter at all is also treated as a request for a new campus.
        var block = CreateBlock( rockContext, campusId: null );
        DenyEdit( rockContext, block );

        var box = GetInitializationBox( block );

        Assert.IsNull( box.Entity );
        Assert.Contains( "not authorized to edit", box.ErrorMessage );
    }

    [TestMethod]
    public void Initialization_WithMultiTimeZoneEnabled_IncludesTimeZoneOptions()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        SeedSystemSetting( rockContext, Rock.SystemKey.SystemSetting.ENABLE_MULTI_TIME_ZONE_SUPPORT, "True" );

        var block = CreateBlock( rockContext, campusId: campus.Id.ToString() );
        AllowView( rockContext, block );

        var box = GetInitializationBox( block );

        Assert.IsTrue( box.Options.IsMultiTimeZoneSupported );
        Assert.IsNotEmpty( box.Options.TimeZoneOptions );
    }

    [TestMethod]
    public void Initialization_WithSchedulesAndTopics_IncludesThemInBag()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        var schedule = SeedSchedule( rockContext, 1, "Sunday 9am" );
        SeedCampusSchedule( rockContext, campus, 1, schedule );
        SeedCampusTopic( rockContext, campus, 1, data.OtherDefinedValue, "info@rock.example" );

        var block = CreateBlock( rockContext, campusId: campus.Id.ToString() );
        AllowView( rockContext, block );

        var box = GetInitializationBox( block );

        Assert.AreEqual( schedule.Guid.ToString(), box.Entity.CampusSchedules.Single().Schedule.Value );
        Assert.AreEqual( "info@rock.example", box.Entity.CampusTopics.Single().Email );
        Assert.AreEqual( data.OtherDefinedValue.Guid.ToString(), box.Entity.CampusTopics.Single().Type.Value );
    }

    #endregion

    #region Edit

    [TestMethod]
    public void Edit_WithExistingCampus_ReturnsEditBagWithParsedPhoneNumber()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        campus.PhoneNumber = "+44 2079460958";

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );

        var result = block.Edit( campus.IdKey );

        var box = GetContent<ValidPropertiesBox<CampusBag>>( result, HttpStatusCode.OK );
        Assert.AreEqual( "44", box.Bag.PhoneNumberCountryCode );
        Assert.AreEqual( "2079460958", box.Bag.PhoneNumber );
        CollectionAssert.Contains( box.ValidProperties, nameof( CampusBag.Name ) );
        CollectionAssert.Contains( box.ValidProperties, nameof( CampusBag.CoreAttributeValues ) );
    }

    [TestMethod]
    public void Edit_WithNoPhoneNumber_LeavesPhoneNumberEmpty()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );

        var result = block.Edit( campus.IdKey );

        var box = GetContent<ValidPropertiesBox<CampusBag>>( result, HttpStatusCode.OK );
        Assert.IsNull( box.Bag.PhoneNumber );
        Assert.IsNull( box.Bag.PhoneNumberCountryCode );
    }

    [TestMethod]
    public void Edit_WithCountryCodeButNoNumber_ReturnsEmptyPhoneNumber()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        campus.PhoneNumber = "+1";

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );

        var result = block.Edit( campus.IdKey );

        var box = GetContent<ValidPropertiesBox<CampusBag>>( result, HttpStatusCode.OK );
        Assert.AreEqual( string.Empty, box.Bag.PhoneNumber );
    }

    [TestMethod]
    public void Edit_WithCoreAttributes_SeparatesCoreAttributesFromCustomAttributes()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        SeedCampusAttribute( rockContext, "core_CampusColor", "Campus Color" );
        SeedCampusAttribute( rockContext, "Pastor", "Pastor" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );

        var result = block.Edit( campus.IdKey );

        var box = GetContent<ValidPropertiesBox<CampusBag>>( result, HttpStatusCode.OK );
        Assert.IsTrue( box.Bag.CoreAttributes.ContainsKey( "core_CampusColor" ) );
        Assert.IsFalse( box.Bag.CoreAttributes.ContainsKey( "Pastor" ) );
        Assert.IsTrue( box.Bag.Attributes.ContainsKey( "Pastor" ) );
        Assert.IsFalse( box.Bag.Attributes.ContainsKey( "core_CampusColor" ) );
    }

    [TestMethod]
    public void Edit_WithUnknownKey_ReturnsNotFound()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        SeedLookupData( rockContext );

        var block = CreateBlock( rockContext );

        var result = block.Edit( "999" );

        AssertBadRequest( result, "Campus not found." );
    }

    [TestMethod]
    public void Edit_WithoutEditAuthorization_ReturnsNotAuthorized()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        DenyEdit( rockContext, block );

        var result = block.Edit( campus.IdKey );

        AssertBadRequest( result, "Not authorized to edit" );
    }

    #endregion

    #region Save

    [TestMethod]
    public void Save_NewCampus_ReturnsCreatedWithUrlAndNextOrder()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var existingCampus = SeedCampus( rockContext, data, 1, "Main Campus" );
        existingCampus.Order = 4;

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            Name = "North Campus",
            Location = data.CampusLocation.ToListItemBag()
        } );

        var result = block.Save( box );

        var newCampus = rockContext.Set<Campus>().Single( c => c.Name == "North Campus" );
        var url = GetContent<string>( result, HttpStatusCode.Created );
        Assert.Contains( $"CampusId={newCampus.IdKey}", url );
        Assert.AreEqual( 5, newCampus.Order );
        Assert.AreEqual( data.CampusLocation.Id, newCampus.LocationId );
    }

    [TestMethod]
    public void Save_NewCampusWhenNoCampusesExist_SetsOrderToZero()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            Name = "First Campus",
            Location = data.CampusLocation.ToListItemBag()
        } );

        var result = block.Save( box );

        Assert.AreEqual( HttpStatusCode.Created, result.StatusCode );
        Assert.AreEqual( 0, rockContext.Set<Campus>().Single().Order );
    }

    [TestMethod]
    public void Save_ExistingCampus_UpdatesPropertiesAndReturnsViewBag()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        var leader = MockData.CreatePerson( rockContext, "Ted", "Decker" );

        var openedDate = new DateTime( 2020, 1, 5 );
        var closedDate = new DateTime( 2030, 6, 30 );
        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            Name = "Renamed Campus",
            Description = "A description.",
            IsActive = false,
            IsSystem = true,
            ShortCode = "RC",
            TimeZoneId = "Mountain Standard Time",
            Url = "https://rock.example/campus",
            OpenedDate = openedDate,
            ClosedDate = closedDate,
            Location = data.CampusLocation.ToListItemBag(),
            CampusStatusValue = data.OtherDefinedValue.ToListItemBag(),
            CampusTypeValue = data.CampusTypePhysical.ToListItemBag(),
            LeaderPersonAlias = leader.Aliases.First().ToListItemBag(),
            ServiceTimes = new List<ListItemBag>
            {
                new ListItemBag { Value = "Sunday", Text = "9:00am" },
                new ListItemBag { Value = "Sunday", Text = "11:00am" }
            }
        } );

        var result = block.Save( box );

        var responseBox = GetContent<ValidPropertiesBox<CampusBag>>( result, HttpStatusCode.OK );
        Assert.AreEqual( "Renamed Campus", responseBox.Bag.Name );
        Assert.AreEqual( "Renamed Campus", campus.Name );
        Assert.AreEqual( "A description.", campus.Description );
        Assert.IsFalse( campus.IsActive.Value );
        Assert.IsTrue( campus.IsSystem );
        Assert.AreEqual( "RC", campus.ShortCode );
        Assert.AreEqual( "Mountain Standard Time", campus.TimeZoneId );
        Assert.AreEqual( "https://rock.example/campus", campus.Url );
        Assert.AreEqual( openedDate, campus.OpenedDate );
        Assert.AreEqual( closedDate, campus.ClosedDate );
        Assert.AreEqual( data.OtherDefinedValue.Id, campus.CampusStatusValueId );
        Assert.AreEqual( data.CampusTypePhysical.Id, campus.CampusTypeValueId );
        Assert.AreEqual( leader.PrimaryAliasId, campus.LeaderPersonAliasId );
#pragma warning disable CS0612, CS0618
        Assert.AreEqual( "Sunday^9:00am|Sunday^11:00am", campus.ServiceTimes );
#pragma warning restore CS0612, CS0618
    }

    [TestMethod]
    public void Save_WithNullValidProperties_ReturnsInvalidData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = new ValidPropertiesBox<CampusBag>
        {
            Bag = new CampusBag { IdKey = campus.IdKey },
            ValidProperties = null
        };

        var result = block.Save( box );

        AssertBadRequest( result, "Invalid data." );
    }

    [TestMethod]
    public void Save_WithoutEditAuthorization_ReturnsNotAuthorized()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        DenyEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag { IdKey = campus.IdKey, Name = "Renamed" } );

        var result = block.Save( box );

        AssertBadRequest( result, "Not authorized to edit" );
        Assert.AreEqual( "Main Campus", campus.Name );
    }

    [TestMethod]
    public void Save_WithNullCampusSchedules_ReturnsInvalidData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag { IdKey = campus.IdKey }, nameof( CampusBag.CampusSchedules ) );

        var result = block.Save( box );

        AssertBadRequest( result, "Invalid data." );
    }

    [TestMethod]
    public void Save_WithUnknownSchedule_ReturnsInvalidData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            CampusSchedules = new List<CampusScheduleBag>
            {
                new CampusScheduleBag
                {
                    Guid = Guid.NewGuid(),
                    Schedule = new ListItemBag { Value = Guid.NewGuid().ToString() }
                }
            }
        } );

        var result = block.Save( box );

        AssertBadRequest( result, "Invalid data." );
    }

    [TestMethod]
    public void Save_WithCampusSchedules_AddsUpdatesAndRemovesSchedules()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        var scheduleA = SeedSchedule( rockContext, 1, "Sunday 9am" );
        var scheduleB = SeedSchedule( rockContext, 2, "Sunday 11am" );
        var keptCampusSchedule = SeedCampusSchedule( rockContext, campus, 1, scheduleA );
        var removedCampusSchedule = SeedCampusSchedule( rockContext, campus, 2, scheduleB );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            Location = data.CampusLocation.ToListItemBag(),
            CampusSchedules = new List<CampusScheduleBag>
            {
                // Existing schedule is switched to schedule B with a type.
                new CampusScheduleBag
                {
                    Guid = keptCampusSchedule.Guid,
                    Schedule = scheduleB.ToListItemBag(),
                    ScheduleTypeValue = data.OtherDefinedValue.ToListItemBag()
                },

                // New schedule is added for schedule A.
                new CampusScheduleBag
                {
                    Guid = Guid.NewGuid(),
                    Schedule = scheduleA.ToListItemBag()
                }
            }
        } );

        var result = block.Save( box );

        Assert.AreEqual( HttpStatusCode.OK, result.StatusCode );
        Assert.HasCount( 2, campus.CampusSchedules );
        Assert.DoesNotContain( removedCampusSchedule, campus.CampusSchedules );
        Assert.DoesNotContain( removedCampusSchedule, rockContext.Set<CampusSchedule>() );
        Assert.AreEqual( scheduleB.Id, keptCampusSchedule.ScheduleId );
        Assert.AreEqual( data.OtherDefinedValue.Id, keptCampusSchedule.ScheduleTypeValueId );
        Assert.AreEqual( 0, keptCampusSchedule.Order );

        var addedCampusSchedule = campus.CampusSchedules.Single( cs => cs != keptCampusSchedule );
        Assert.AreEqual( scheduleA.Id, addedCampusSchedule.ScheduleId );
        Assert.IsNull( addedCampusSchedule.ScheduleTypeValueId );
        Assert.AreEqual( 1, addedCampusSchedule.Order );
    }

    [TestMethod]
    public void Save_WithNullCampusTopics_ReturnsInvalidData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag { IdKey = campus.IdKey }, nameof( CampusBag.CampusTopics ) );

        var result = block.Save( box );

        AssertBadRequest( result, "Invalid data." );
    }

    [TestMethod]
    public void Save_WithUnknownTopicType_ReturnsInvalidData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            CampusTopics = new List<CampusTopicBag>
            {
                new CampusTopicBag
                {
                    Guid = Guid.NewGuid(),
                    Type = new ListItemBag { Value = Guid.NewGuid().ToString() },
                    IsPublic = true
                }
            }
        } );

        var result = block.Save( box );

        AssertBadRequest( result, "Invalid data." );
    }

    [TestMethod]
    public void Save_WithCampusTopics_AddsUpdatesAndRemovesTopics()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        var keptTopic = SeedCampusTopic( rockContext, campus, 1, data.OtherDefinedValue, "old@rock.example" );
        var removedTopic = SeedCampusTopic( rockContext, campus, 2, data.OtherDefinedValue, "removed@rock.example" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            Location = data.CampusLocation.ToListItemBag(),
            CampusTopics = new List<CampusTopicBag>
            {
                new CampusTopicBag
                {
                    Guid = keptTopic.Guid,
                    Type = data.CampusTypePhysical.ToListItemBag(),
                    Email = "new@rock.example",
                    IsPublic = true
                },
                new CampusTopicBag
                {
                    Guid = Guid.NewGuid(),
                    Type = data.OtherDefinedValue.ToListItemBag(),
                    Email = "added@rock.example",
                    IsPublic = false
                }
            }
        } );

        var result = block.Save( box );

        Assert.AreEqual( HttpStatusCode.OK, result.StatusCode );
        Assert.HasCount( 2, campus.CampusTopics );
        Assert.DoesNotContain( removedTopic, campus.CampusTopics );
        Assert.DoesNotContain( removedTopic, rockContext.Set<CampusTopic>() );
        Assert.AreEqual( "new@rock.example", keptTopic.Email );
        Assert.IsTrue( keptTopic.IsPublic );
        Assert.AreEqual( data.CampusTypePhysical.Id, keptTopic.TopicTypeValue.Id );

        var addedTopic = campus.CampusTopics.Single( t => t != keptTopic );
        Assert.AreEqual( "added@rock.example", addedTopic.Email );
        Assert.IsFalse( addedTopic.IsPublic );
    }

    [TestMethod]
    public void Save_WithNonDefaultPhoneNumberCountryCode_IncludesCountryCodePrefix()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            Location = data.CampusLocation.ToListItemBag(),
            PhoneNumberCountryCode = "44",
            PhoneNumber = "2079460958"
        } );

        var result = block.Save( box );

        Assert.AreEqual( HttpStatusCode.OK, result.StatusCode );
        Assert.AreEqual( "+44 2079460958", campus.PhoneNumber );
    }

    [TestMethod]
    public void Save_WithoutPhoneNumberCountryCode_UsesDefaultCountryCode()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            Location = data.CampusLocation.ToListItemBag(),
            PhoneNumber = "(623) 555-1212"
        } );

        var result = block.Save( box );

        Assert.AreEqual( HttpStatusCode.OK, result.StatusCode );
        Assert.AreEqual( "6235551212", campus.PhoneNumber, "Without the phone country code defined type, digits are stored unformatted." );
    }

    [TestMethod]
    public void Save_WithCoreAttributeValues_OverridesCustomAttributeValues()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        SeedCampusAttribute( rockContext, "core_CampusColor", "Campus Color" );
        SeedCampusAttribute( rockContext, "Pastor", "Pastor" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            Location = data.CampusLocation.ToListItemBag(),
            AttributeValues = new Dictionary<string, string>
            {
                ["Pastor"] = "Pastor Ted",
                ["core_CampusColor"] = "stale"
            },
            CoreAttributeValues = new Dictionary<string, string>
            {
                ["core_CampusColor"] = "#ff0000"
            }
        } );

        var result = block.Save( box );

        Assert.AreEqual( HttpStatusCode.OK, result.StatusCode );
        Assert.AreEqual( "Pastor Ted", campus.GetAttributeValue( "Pastor" ) );
        Assert.AreEqual( "#ff0000", campus.GetAttributeValue( "core_CampusColor" ) );
    }

    [TestMethod]
    public void Save_WithNullAttributeValues_SavesCoreAttributeValues()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        SeedCampusAttribute( rockContext, "core_CampusColor", "Campus Color" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            Location = data.CampusLocation.ToListItemBag(),
            CoreAttributeValues = new Dictionary<string, string>
            {
                ["core_CampusColor"] = "#00ff00"
            }
        }, nameof( CampusBag.AttributeValues ) );

        var result = block.Save( box );

        Assert.AreEqual( HttpStatusCode.OK, result.StatusCode );
        Assert.AreEqual( "#00ff00", campus.GetAttributeValue( "core_CampusColor" ) );
    }

    [TestMethod]
    public void Save_WithNullCoreAttributeValues_SavesCustomAttributeValues()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        SeedCampusAttribute( rockContext, "Pastor", "Pastor" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            Location = data.CampusLocation.ToListItemBag(),
            AttributeValues = new Dictionary<string, string>
            {
                ["Pastor"] = "Pastor Ted"
            },
            CoreAttributeValues = null

            // The block only applies AttributeValues when CoreAttributeValues
            // is also a valid property, so it must be sent as an explicit null.
        }, nameof( CampusBag.CoreAttributeValues ) );

        var result = block.Save( box );

        Assert.AreEqual( HttpStatusCode.OK, result.StatusCode );
        Assert.AreEqual( "Pastor Ted", campus.GetAttributeValue( "Pastor" ) );
    }

    [TestMethod]
    public void Save_WithMissingLocation_ReturnsLocationError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag { IdKey = campus.IdKey }, nameof( CampusBag.Location ) );

        var result = block.Save( box );

        AssertBadRequest( result, "is not a 'Campus' location type." );
    }

    [TestMethod]
    public void Save_WithNonCampusLocationType_ReturnsLocationError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        var buildingLocation = SeedLocation( rockContext, 2, "Building", data.OtherDefinedValue );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag { IdKey = campus.IdKey, Location = buildingLocation.ToListItemBag() } );

        var result = block.Save( box );

        AssertBadRequest( result, "The named location \"Building\" is not a 'Campus' location type." );
    }

    [TestMethod]
    public void Save_NewCampusWithDuplicateName_ReturnsActiveDuplicateError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            Name = "Main Campus",
            Location = data.CampusLocation.ToListItemBag()
        } );

        var result = block.Save( box );

        AssertBadRequest( result, "already in use for an existing active campus." );
    }

    [TestMethod]
    public void Save_ExistingCampusWithDuplicateName_ReturnsInactiveDuplicateError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        var inactiveCampus = SeedCampus( rockContext, data, 2, "Old Campus" );
        inactiveCampus.IsActive = false;

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            Name = "Old Campus",
            Location = data.CampusLocation.ToListItemBag()
        } );

        var result = block.Save( box );

        AssertBadRequest( result, "already in use for an existing inactive campus." );
    }

    [TestMethod]
    public void Save_ExistingCampusKeepingItsOwnName_Succeeds()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            Name = "Main Campus",
            Location = data.CampusLocation.ToListItemBag()
        } );

        var result = block.Save( box );

        Assert.AreEqual( HttpStatusCode.OK, result.StatusCode );
    }

    [TestMethod]
    public void Save_WithInvalidUrl_ReturnsUrlError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );
        var box = CreateSaveBox( new CampusBag
        {
            IdKey = campus.IdKey,
            Location = data.CampusLocation.ToListItemBag(),
            Url = "not a url"
        } );

        var result = block.Save( box );

        AssertBadRequest( result, "The URL 'not a url' is not a valid URL." );
    }

    #endregion

    #region Delete

    [TestMethod]
    public void Delete_WithOtherCampuses_DeletesCampusAndReturnsParentUrl()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        SeedCampus( rockContext, data, 2, "North Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );

        var result = block.Delete( campus.IdKey );

        var url = GetContent<string>( result, HttpStatusCode.OK );
        Assert.AreEqual( $"/page/{block.PageCache.ParentPageId}", url );
        Assert.DoesNotContain( campus, rockContext.Set<Campus>() );
    }

    [TestMethod]
    public void Delete_OnlyCampus_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );

        var result = block.Delete( campus.IdKey );

        AssertBadRequest( result, "Main Campus is the only campus and cannot be deleted" );
        Assert.Contains( campus, rockContext.Set<Campus>() );
    }

    [TestMethod]
    public void Delete_WithDependentRecords_ReturnsCanDeleteError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        SeedCampus( rockContext, data, 2, "North Campus" );
        rockContext.Set<BenevolenceRequest>().Add( new BenevolenceRequest { Id = 1, Guid = Guid.NewGuid(), CampusId = campus.Id } );

        var block = CreateBlock( rockContext );
        AllowEdit( rockContext, block );

        var result = block.Delete( campus.IdKey );

        AssertBadRequest( result, "This Campus is assigned to a Benevolence Request." );
        Assert.Contains( campus, rockContext.Set<Campus>() );
    }

    [TestMethod]
    public void Delete_WithoutEditAuthorization_ReturnsNotAuthorized()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();
        var data = SeedLookupData( rockContext );
        var campus = SeedCampus( rockContext, data, 1, "Main Campus" );
        SeedCampus( rockContext, data, 2, "North Campus" );

        var block = CreateBlock( rockContext );
        DenyEdit( rockContext, block );

        var result = block.Delete( campus.IdKey );

        AssertBadRequest( result, "Not authorized to edit" );
        Assert.Contains( campus, rockContext.Set<Campus>() );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// The lookup rows most tests need, seeded by <see cref="SeedLookupData(RockContext)"/>.
    /// </summary>
    private class LookupData
    {
        public DefinedValue CampusLocationType { get; set; }

        public DefinedValue CampusStatusOpen { get; set; }

        public DefinedValue CampusTypePhysical { get; set; }

        public DefinedValue OtherDefinedValue { get; set; }

        public Location CampusLocation { get; set; }
    }

    /// <summary>
    /// Seeds the defined values, entity types and campus location that the
    /// block looks up while loading and saving a campus.
    /// </summary>
    /// <param name="rockContext">The mocked context to seed.</param>
    /// <returns>The seeded rows.</returns>
    private static LookupData SeedLookupData( RockContext rockContext )
    {
        // ListItemBag.GetEntityId<T>() will not create a missing entity type,
        // so every type the block resolves from a bag must exist up front.
        EntityTypeCache.Get( typeof( Campus ), true, rockContext );
        EntityTypeCache.Get( typeof( DefinedValue ), true, rockContext );
        EntityTypeCache.Get( typeof( Location ), true, rockContext );
        EntityTypeCache.Get( typeof( PersonAlias ), true, rockContext );
        EntityTypeCache.Get( typeof( Schedule ), true, rockContext );

        var data = new LookupData
        {
            CampusLocationType = MockData.CreateDefinedValue( rockContext, Rock.SystemGuid.DefinedValue.LOCATION_TYPE_CAMPUS.AsGuid(), "Campus" ),
            CampusStatusOpen = MockData.CreateDefinedValue( rockContext, Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_OPEN.AsGuid(), "Open" ),
            CampusTypePhysical = MockData.CreateDefinedValue( rockContext, Rock.SystemGuid.DefinedValue.CAMPUS_TYPE_PHYSICAL.AsGuid(), "Physical" ),
            OtherDefinedValue = MockData.CreateDefinedValue( rockContext, Guid.NewGuid(), "Other" )
        };

        data.CampusLocation = SeedLocation( rockContext, 1, "Main Campus Location", data.CampusLocationType );

        return data;
    }

    /// <summary>
    /// Seeds a location of the specified type.
    /// </summary>
    private static Location SeedLocation( RockContext rockContext, int id, string name, DefinedValue locationType )
    {
        var location = new Location
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name,
            LocationTypeValueId = locationType.Id,
            LocationTypeValue = locationType
        };

        rockContext.Set<Location>().Add( location );

        return location;
    }

    /// <summary>
    /// Seeds an active campus at the standard campus location.
    /// </summary>
    private static Campus SeedCampus( RockContext rockContext, LookupData data, int id, string name )
    {
        var campus = new Campus
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name,
            IsActive = true,
            LocationId = data.CampusLocation.Id,
            Location = data.CampusLocation,
            CampusStatusValueId = data.CampusStatusOpen.Id,
            CampusStatusValue = data.CampusStatusOpen,
            CampusTypeValueId = data.CampusTypePhysical.Id,
            CampusTypeValue = data.CampusTypePhysical
        };

        rockContext.Set<Campus>().Add( campus );

        return campus;
    }

    /// <summary>
    /// Seeds a named schedule.
    /// </summary>
    private static Schedule SeedSchedule( RockContext rockContext, int id, string name )
    {
        var schedule = new Schedule
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name
        };

        rockContext.Set<Schedule>().Add( schedule );

        return schedule;
    }

    /// <summary>
    /// Seeds a campus schedule and attaches it to the campus, since the mocked
    /// context performs no navigation-property fixup.
    /// </summary>
    private static CampusSchedule SeedCampusSchedule( RockContext rockContext, Campus campus, int id, Schedule schedule )
    {
        var campusSchedule = new CampusSchedule
        {
            Id = id,
            Guid = Guid.NewGuid(),
            CampusId = campus.Id,
            Campus = campus,
            ScheduleId = schedule.Id,
            Schedule = schedule
        };

        rockContext.Set<CampusSchedule>().Add( campusSchedule );
        campus.CampusSchedules.Add( campusSchedule );

        return campusSchedule;
    }

    /// <summary>
    /// Seeds a campus topic and attaches it to the campus.
    /// </summary>
    private static CampusTopic SeedCampusTopic( RockContext rockContext, Campus campus, int id, DefinedValue topicType, string email )
    {
        var campusTopic = new CampusTopic
        {
            Id = id,
            Guid = Guid.NewGuid(),
            CampusId = campus.Id,
            Campus = campus,
            TopicTypeValueId = topicType.Id,
            TopicTypeValue = topicType,
            Email = email
        };

        rockContext.Set<CampusTopic>().Add( campusTopic );
        campus.CampusTopics.Add( campusTopic );

        return campusTopic;
    }

    /// <summary>
    /// Seeds a text attribute on the Campus entity type.
    /// </summary>
    private static void SeedCampusAttribute( RockContext rockContext, string key, string name )
    {
        var fieldType = rockContext.Set<FieldType>().FirstOrDefault()
            ?? MockData.CreateFieldType( rockContext, Rock.SystemGuid.FieldType.TEXT.AsGuid(), "Text", "Rock.Field.Types.TextFieldType" );
        var campusEntityTypeId = EntityTypeCache.Get( typeof( Campus ), true, rockContext ).Id;

        var attribute = MockData.CreateAttribute( rockContext, key, name, fieldType.Id, campusEntityTypeId );
        attribute.EntityTypeQualifierColumn = string.Empty;
        attribute.EntityTypeQualifierValue = string.Empty;
    }

    /// <summary>
    /// Seeds a system setting value.
    /// </summary>
    private static void SeedSystemSetting( RockContext rockContext, string key, string value )
    {
        var attribute = MockData.CreateAttribute( rockContext, key, key, defaultValue: value );
        attribute.EntityTypeQualifierColumn = Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER;
        attribute.EntityTypeQualifierValue = string.Empty;
    }

    /// <summary>
    /// Creates the block on a freshly seeded page.
    /// </summary>
    /// <param name="rockContext">The mocked context.</param>
    /// <param name="campusId">The value of the CampusId page parameter, or <c>null</c> to omit it.</param>
    private static CampusDetail CreateBlock( RockContext rockContext, string campusId = null )
    {
        var page = MockBlockHelper.SeedPage( rockContext );
        var pageParameters = campusId != null
            ? new Dictionary<string, string> { ["CampusId"] = campusId }
            : null;

        return MockBlockHelper.CreateBlock<CampusDetail>( rockContext, page, pageParameters );
    }

    /*
        9/27/26 - CLAUDE

        Tests state the permission they need ("the current person can edit")
        through these helpers instead of seeding authorization rules
        directly. Whether this block should check security on the block or
        on the campus is an open question: CampusDetail checks the campus
        today, while other detail blocks (MediaElementDetail, for example)
        check the block. Each helper therefore applies the same permission
        to both, so the tests pass under either implementation and none of
        them lock in the current choice. Once the question is decided, add
        a dedicated test for the chosen rule and simplify these helpers.

        The campus rule is seeded at the entity type level, which an
        existing campus inherits through its parent authority. A block
        inherits from its page instead, so the block rule must target the
        specific block, which is why these run after CreateBlock().

        Reason: Keep tests from depending on where the security check lives.
    */

    /// <summary>
    /// Allows the current person to view the campus, on both the block and the campus.
    /// </summary>
    /// <param name="rockContext">The mocked context.</param>
    /// <param name="block">The block under test.</param>
    private static void AllowView( RockContext rockContext, CampusDetail block )
    {
        MockAuthorizationHelper.AllowAllUsers<Campus>( rockContext, Authorization.VIEW );
        MockAuthorizationHelper.AllowAllUsers<Block>( rockContext, Authorization.VIEW, block.BlockId );
    }

    /// <summary>
    /// Denies the current person permission to view the campus, on both the block and the campus.
    /// </summary>
    /// <param name="rockContext">The mocked context.</param>
    /// <param name="block">The block under test.</param>
    private static void DenyView( RockContext rockContext, CampusDetail block )
    {
        MockAuthorizationHelper.DenyAllUsers<Campus>( rockContext, Authorization.VIEW );
        MockAuthorizationHelper.DenyAllUsers<Block>( rockContext, Authorization.VIEW, block.BlockId );
    }

    /// <summary>
    /// Allows the current person to edit the campus, on both the block and the campus.
    /// </summary>
    /// <param name="rockContext">The mocked context.</param>
    /// <param name="block">The block under test.</param>
    private static void AllowEdit( RockContext rockContext, CampusDetail block )
    {
        MockAuthorizationHelper.AllowAllUsers<Campus>( rockContext, Authorization.EDIT );
        MockAuthorizationHelper.AllowAllUsers<Block>( rockContext, Authorization.EDIT, block.BlockId );
    }

    /// <summary>
    /// Denies the current person permission to edit the campus, on both the block and the campus.
    /// </summary>
    /// <param name="rockContext">The mocked context.</param>
    /// <param name="block">The block under test.</param>
    private static void DenyEdit( RockContext rockContext, CampusDetail block )
    {
        MockAuthorizationHelper.DenyAllUsers<Campus>( rockContext, Authorization.EDIT );
        MockAuthorizationHelper.DenyAllUsers<Block>( rockContext, Authorization.EDIT, block.BlockId );
    }

    /// <summary>
    /// Runs the block's initialization and returns the typed box.
    /// </summary>
    private static DetailBlockBox<CampusBag, CampusDetailOptionsBag> GetInitializationBox( CampusDetail block )
    {
        return ( DetailBlockBox<CampusBag, CampusDetailOptionsBag> ) block.GetObsidianBlockInitialization();
    }

    /// <summary>
    /// Wraps a bag in a save box that marks every non-null property of the
    /// bag as valid, mirroring the client which only sends the properties it
    /// has values for. Properties that should be sent as an explicit
    /// <c>null</c> are named in <paramref name="nullProperties"/>.
    /// </summary>
    /// <param name="bag">The bag to be saved.</param>
    /// <param name="nullProperties">Additional property names to mark valid even though their value is <c>null</c>.</param>
    private static ValidPropertiesBox<CampusBag> CreateSaveBox( CampusBag bag, params string[] nullProperties )
    {
        var validProperties = typeof( CampusBag ).GetProperties()
            .Where( p => p.Name != nameof( CampusBag.IdKey ) )
            .Where( p => p.GetValue( bag ) != null || nullProperties.Contains( p.Name ) )
            .Select( p => p.Name )
            .ToList();

        return new ValidPropertiesBox<CampusBag>
        {
            Bag = bag,
            ValidProperties = validProperties
        };
    }

    /// <summary>
    /// Asserts the result has the expected status code and returns the typed content.
    /// </summary>
    private static T GetContent<T>( BlockActionResult result, HttpStatusCode expectedStatusCode )
    {
        Assert.AreEqual( expectedStatusCode, result.StatusCode, result.Error );

        return ( T ) result.Content;
    }

    /// <summary>
    /// Asserts the result is a bad request whose error contains the expected text.
    /// </summary>
    private static void AssertBadRequest( BlockActionResult result, string expectedErrorText )
    {
        Assert.AreEqual( HttpStatusCode.BadRequest, result.StatusCode );
        Assert.IsNotNull( result.Error );
        Assert.Contains( expectedErrorText, result.Error );
    }

    #endregion
}
