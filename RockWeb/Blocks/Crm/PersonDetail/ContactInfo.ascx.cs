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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Model;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    [DisplayName( "Person Contact Info" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Person contact information(Person Detail Page)." )]

    public partial class ContactInfo : Rock.Web.UI.PersonBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var service = new PhoneNumberService();
            foreach ( dynamic item in service.Queryable()
                .Where( n => n.PersonId == Person.Id )
                .OrderBy( n => n.NumberTypeValue.Order )
                .Select( n => new
                {
                    Number = n.Number,
                    Unlisted = n.IsUnlisted,
                    Type = n.NumberTypeValue.Name
                }
                ) )
            {
                var li = new HtmlGenericControl( "li" );
                ulPhoneNumbers.Controls.Add( li );

                var anchor = new HtmlGenericControl( "a" );
                li.Controls.Add( anchor );
                anchor.Attributes.Add("href", "#");
                anchor.AddCssClass( "highlight" );

                var icon = new HtmlGenericControl( "i" );
                anchor.Controls.Add( icon );
                icon.AddCssClass( "fa fa-phone" );

                if ( item.Unlisted )
                {
                    var span = new HtmlGenericControl( "span" );
                    anchor.Controls.Add( span );
                    span.AddCssClass( "phone-unlisted" );
                    span.Attributes.Add( "data-value", PhoneNumber.FormattedNumber( item.Number ) );
                    span.InnerText = "Unlisted";
                }
                else
                {
                    anchor.Controls.Add( new LiteralControl( PhoneNumber.FormattedNumber( item.Number ) ) );
                }

                anchor.Controls.Add( new LiteralControl( " " ) );

                var small = new HtmlGenericControl( "small" );
                anchor.Controls.Add( small );
                small.InnerText = item.Type;
            }

            if ( !String.IsNullOrWhiteSpace( Person.Email ) )
            {
                hlEmail.Text = Person.Email;
                hlEmail.NavigateUrl = "mailto:" + Person.Email;
            }

        }
    }
}