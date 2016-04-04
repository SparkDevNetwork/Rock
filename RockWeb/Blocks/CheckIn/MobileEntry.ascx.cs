﻿// <copyright>
// Copyright by the Spark Development Network
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
using System.Web;
using Rock.CheckIn;
using System.ComponentModel;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// This block is responsible for setting the IsMobile cookie on the client for
    /// subsequent use by other checkin blocks.
    /// </summary>
    [DisplayName("Mobile Entry")]
    [Category("Check-in")]
    [Description("Helps to configure the checkin for mobile devices.")]
    public partial class MobileEntry : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            HttpCookie isMobileCookie = new HttpCookie( CheckInCookie.ISMOBILE, "true" );
            isMobileCookie.Expires = DateTime.MaxValue;
            Response.Cookies.Set( isMobileCookie );
            NavigateToNextPage();
        }
    }
}