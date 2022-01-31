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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "Rock" )]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "a2b98b90-6dcb-4049-ad04-353c9b46a113" )]

[assembly: InternalsVisibleTo( "Rock.Blocks" )]
[assembly: InternalsVisibleTo( "Rock.CodeGeneration" )]
[assembly: InternalsVisibleTo( "Rock.Migrations" )]
[assembly: InternalsVisibleTo( "Rock.Rest" )]
[assembly: InternalsVisibleTo( "Rock.Tests.Shared" )]
[assembly: InternalsVisibleTo( "Rock.Tests.UnitTests" )]
[assembly: InternalsVisibleTo( "Rock.Tests.Integration" )]
[assembly: InternalsVisibleTo( "Rock.WebStartup" )]

// The following type forwardings were setup in Rock 1.13.0
[assembly: TypeForwardedTo( typeof( Rock.RockObsolete ) )]
[assembly: TypeForwardedTo( typeof( Rock.RockDateTime ) )]
[assembly: TypeForwardedTo( typeof( Rock.Utility.RockColor ) )]
