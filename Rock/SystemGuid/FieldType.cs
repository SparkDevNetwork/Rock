// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

namespace Rock.SystemGuid
{
    /// <summary>
    /// System Blocks.  NOTE: Some of these are referenced in Migrations to avoid string-typos.
    /// </summary>
    public static class FieldType
    {
        /// <summary>
        /// Binary File field type
        /// </summary>
        public const string BINARY_FILE = "C403E219-A56B-439E-9D50-9302DFE760CF";

        /// <summary>
        /// Boolean field type
        /// </summary>
        public const string BOOLEAN = "1EDAFDED-DFE6-4334-B019-6EECBA89E05A";

        /// <summary>
        /// Date field type
        /// </summary>
        public const string DATE = "6B6AA175-4758-453F-8D83-FCD8044B5F36";

        /// <summary>
        /// Day of Week field type
        /// </summary>
        public const string DAY_OF_WEEK = "67ED79C1-4297-4C2D-93BF-62246B26FDB5";

        /// <summary>
        /// Decimal field type
        /// </summary>
        public const string DECIMAL = "C757A554-3009-4214-B05D-CEA2B2EA6B8F";

        /// <summary>
        /// Defined value field type
        /// </summary>
        public const string DEFINED_VALUE = "59D5A94C-94A0-4630-B80A-BB25697D74C7";

        /// <summary>
        /// Multi Select field type
        /// </summary>
        public const string MULTI_SELECT = "BD0D9B57-2A41-4490-89FF-F01DAB7D4904";

        /// <summary>
        /// Integer field type
        /// </summary>
        public const string INTEGER = "A75DFC58-7A1B-4799-BF31-451B2BBE38FF";

        /// <summary>
        /// Image field type
        /// </summary>
        public const string IMAGE = "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D";

        /// <summary>
        /// Single Select field type
        /// </summary>
        public const string SINGLE_SELECT = "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0";

        /// <summary>
        /// Text field type
        /// </summary>
        public const string TEXT = "9C204CD0-1233-41C5-818A-C5DA439445AA";

        /// <summary>
        /// Time field type
        /// </summary>
        public const string TIME = "2F8F5EC4-57FA-4F6C-AB15-9D6616994580";
    }
}