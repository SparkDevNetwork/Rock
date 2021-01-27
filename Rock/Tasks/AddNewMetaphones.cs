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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Updates metaphone data when a person is added or updated.
    /// </summary>
    public sealed class AddNewMetaphones : BusStartedTask<AddNewMetaphones.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            var names = new List<string>();
            AddName( names, message.FirstName );
            AddName( names, message.LastName );
            AddName( names, message.NickName );

            using ( var rockContext = new RockContext() )
            {
                var metaPhones = rockContext.Metaphones;

                var newMetaphoneNames = names.Where( n => !metaPhones.Any( m => m.Name == n ) ).ToList();

                if ( newMetaphoneNames.Any() )
                {
                    foreach ( string name in newMetaphoneNames )
                    {
                        string mp1 = string.Empty;
                        string mp2 = string.Empty;
                        Rock.Utility.DoubleMetaphone.doubleMetaphone( name, ref mp1, ref mp2 );

                        var metaphone = new Metaphone();
                        metaphone.Name = name;
                        metaphone.Metaphone1 = mp1;
                        metaphone.Metaphone2 = mp2;

                        metaPhones.Add( metaphone );
                    }

                    rockContext.SaveChanges();
                }
            }
        }

        private void AddName( List<string> names, string name )
        {
            if ( !string.IsNullOrWhiteSpace( name ) )
            {
                string ucName = name.Trim();
                if ( !names.Contains( ucName ) )
                {
                    names.Add( ucName );
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the first name of the Person.
            /// </summary>
            /// <value>
            /// A <see cref="System.String"/> representing the first name of the Person.
            /// </value>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the nick name of the Person.
            /// </summary>
            /// <value>
            /// A <see cref="System.String"/> representing the nick name of the Person.
            /// </value>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the Person.
            /// </summary>
            /// <value>
            /// A <see cref="System.String"/> representing the last name of the Person.
            /// </value>
            public string LastName { get; set; }
        }
    }
}