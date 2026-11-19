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

/*
    10/29/26 - CLAUDE

    On .NET the legacy enums in the Rock.Enums project live in Rock.Enums.[Domain]
    instead of Rock.Model (and a few others such as Rock.Utility). Existing code
    finds them through "using Rock.Model;", so these global usings make them
    resolve the same way without touching every file that uses them. This
    project does not reference Rock.dll, so the old namespaces are declared
    empty to keep those using statements valid.

    Reason: Keep legacy enum references compiling on .NET without per-file usings.
*/
#if !WEBFORMS
global using Rock.Enums.Cms;
global using Rock.Enums.Communication;
global using Rock.Enums.Connection;
global using Rock.Enums.Core;
global using Rock.Enums.Crm;
global using Rock.Enums.Engagement;
global using Rock.Enums.Event;
global using Rock.Enums.Finance;
global using Rock.Enums.Group;
global using Rock.Enums.Reporting;
global using Rock.Enums.Security;
global using Rock.Enums.Workflow;

namespace Rock.Model { }
namespace Rock.Utility { }
namespace Rock.Utility.Enums { }
#endif
