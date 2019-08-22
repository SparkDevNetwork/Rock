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

namespace Rock.Tests.Integration
{
    public static class TestCategories
    {
        // This Test Category contains methods for adding test data to the current database.
        public const string AddData = "Rock.Setup.AddData";
        // This Test Category contains methods for removing test data from the current database.
        public const string RemoveData = "Rock.Setup.RemoveData";
        // This Test Category contains methods for maintaining specific elements of the test data during the development process.
        public const string DeveloperSetup = "Rock.Setup.Dev";
    }

    public static class TestFeatures
    {
        // Tests having this property relate to data setup for integration tests.
        public const string DataSetup = "Data Setup";
        // Tests having this property relate to data maintenance that may be needed during the development process.
        public const string DataMaintenance = "Data Maintenance";

        // Tests having this property relate to the Steps feature of Rock.
        public const string Steps = "Steps";
    }

    public static  class TestPeople
    {
        public static Guid TedDeckerPersonGuid = new Guid( "8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4" );

        public static Guid BillMarblePersonGuid = new Guid( "1EA811BB-3118-42D1-B020-32A82BC8081A" );

        public static Guid AlishaMarblePersonGuid = new Guid( "69DC0FDC-B451-4303-BD91-EF17C0015D23" );

        public static Guid SarahSimmonsPersonGuid = new Guid( "FC6B9819-EF2E-44C9-93DB-05571B39E58F" );
        public static Guid BrianJonesPersonGuid = new Guid( "3D7F6605-3666-4AB5-9F4E-D7FEBF93278E" );

        public static Guid BenJonesPersonGuid = new Guid( "3C402382-3BD2-4337-A996-9E62F1BAB09D" );
        public static Guid BenJonesStepAlphaAttenderGuid = new Guid( "D5DDBBE4-9D62-4EDE-840D-E9DAB8F99430" );

    }
}
