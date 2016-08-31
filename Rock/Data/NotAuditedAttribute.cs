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

namespace Rock.Data
{
    /// <summary>
    /// Custom attribute used to decorate model classes or specific model properties that should 
    /// not be audited.  If attributed to class, no class changes will be audited, if attributed
    /// to properties, changes will be audited only if properties withouth attribute have changed.
    /// This would typically include logging tables (i.e. Audit, Exception, UserLogin-LastActivityDate, etc)
    /// Specific properties can also 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property )]
    public class NotAuditedAttribute : System.Attribute
    {
    }
}