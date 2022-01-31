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

namespace Rock.Attribute
{
    /// <summary>
    /// Marks an API as internal to Rock. These APIs are not subject to the same
    /// compatibility standards as public APIs. It may be changed or removed
    /// without notice in any release. You should not use such APIs directly in
    /// any plug-ins. Doing so can result in application failures when updating
    /// to a new Rock release.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Enum
        | AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Interface
        | AttributeTargets.Event
        | AttributeTargets.Field
        | AttributeTargets.Method
        | AttributeTargets.Delegate
        | AttributeTargets.Property
        | AttributeTargets.Constructor )]
    public sealed class RockInternalAttribute : System.Attribute
    {
    }
}
