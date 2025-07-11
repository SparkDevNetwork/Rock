﻿// <copyright>
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

namespace Rock.SystemKey
{
    /// <summary>
    /// This class holds Rock's well known attribute keys for devices.
    /// </summary>
    public class DeviceAttributeKey
    {
        /// <summary>
        /// The device has cutter
        /// </summary>
        public const string DEVICE_HAS_CUTTER = "core_device_HasCutter";

        /// <summary>
        /// The printer device DPI setting for rendering labels.
        /// </summary>
        public const string DEVICE_PRINTER_DPI = "core_device_PrinterDpi";

        /// <summary>
        /// The kiosk device will enable registration mode and allow adding families.
        /// </summary>
        public const string DEVICE_KIOSK_ALLOW_ADDING_FAMILIES = "core_device_KioskAllowAddingFamilies";

        /// <summary>
        /// The kiosk device will enable registration mode and allow editing families.
        /// </summary>
        public const string DEVICE_KIOSK_ALLOW_EDITING_FAMILIES = "core_device_KioskAllowEditingFamilies";
    }
}
