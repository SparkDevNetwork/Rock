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

/**
 * The install state of a purchased package. The server sends the matching
 * member name in the package bag's `installState` property. Kept block-local
 * (rather than in Rock.Enums) because it is purely a view concern; it is
 * mirrored by the private `InstallState` enum in PurchasedPackages.cs.
 */
export const enum PurchasedPackageInstallState {
    /** No version of the package is compatible with this Rock version. */
    NotAvailable = "NotAvailable",

    /** The package is not currently installed. */
    Install = "Install",

    /** The package is installed but a newer compatible version exists. */
    Update = "Update",

    /** The package is installed and current. */
    Installed = "Installed"
}
