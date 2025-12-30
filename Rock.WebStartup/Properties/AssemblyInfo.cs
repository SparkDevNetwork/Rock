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
using System.Runtime.CompilerServices;
using System.Web;

// The following attribute will set an initializer to be run early in the ASP.Net pipeline on application
// start. Your can find more information on how this works at: https://haacked.com/archive/2010/05/16/three-hidden-extensibility-gems-in-asp-net-4.aspx/
[assembly: PreApplicationStartMethod( typeof( Rock.AssemblyInitializer ), "Initialize" )]

[assembly: InternalsVisibleTo( "Rock.Tests.Integration" )]
