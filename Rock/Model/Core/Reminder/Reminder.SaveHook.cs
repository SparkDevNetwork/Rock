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
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Reminder SaveHook.
    /// </summary>
    public partial class Reminder
    {
        internal class SaveHook : EntitySaveHook<Reminder>
        {
            protected override void PostSave()
            {
                if ( this.State == EntityContextState.Added )
                {
                    this.Entity.PersonAlias.Person.ReminderCount += 1;
                    this.RockContext.SaveChanges();
                }
                else if ( this.State == EntityContextState.Deleted )
                {
                    this.Entity.PersonAlias.Person.ReminderCount -= 1;
                    this.RockContext.SaveChanges();
                }
                else if ( this.State == EntityContextState.Modified )
                {
                    var originalPersonAliasId = ( int ) this.OriginalValues[nameof( this.Entity.PersonAliasId )];
                    if ( this.Entity.PersonAliasId != originalPersonAliasId )
                    {
                        this.Entity.PersonAlias.Person.ReminderCount += 1;
                        var originalPersonAlias = new PersonAliasService( this.RockContext ).Get( originalPersonAliasId );
                        originalPersonAlias.Person.ReminderCount -= 1;
                        this.RockContext.SaveChanges();
                    }
                }    

                base.PostSave();
            }
        }
    }
}
