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
using System.Collections.Generic;
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Updates metaphone data when a person is added or updated.
    /// </summary>
    public class SaveMetaphoneTransaction : ITransaction
    {
        private List<string> names = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveMetaphoneTransaction"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        public SaveMetaphoneTransaction(Person person)
        {
            AddName( person.FirstName );
            AddName( person.NickName );
            AddName( person.LastName );
        }

        private void AddName( string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string ucName = name.Trim();
                if (!names.Contains(ucName))
                {
                    names.Add(ucName);
                }
            }
        }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var metaPhones = rockContext.Metaphones;

                foreach ( string name in names.Where( n => !metaPhones.Any( m => m.Name == n ) ) )
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
}