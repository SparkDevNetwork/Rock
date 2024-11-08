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
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestData;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Communications
{
    /// <summary>
    /// Provides actions to manage Communications data.
    /// </summary>
    public class CommunicationsDataManager
    {
        private static Lazy<CommunicationsDataManager> _dataManager = new Lazy<CommunicationsDataManager>();
        public static CommunicationsDataManager Instance => _dataManager.Value;

        public bool DeleteSystemCommunication( string communicationIdentifier )
        {
            var context = new RockContext();
            var success = DeleteSystemCommunication( communicationIdentifier, context );
            context.SaveChanges();

            return success;
        }

        public bool DeleteSystemCommunication( string communicationIdentifier, RockContext rockContext )
        {
            var service = new SystemCommunicationService( rockContext );
            var entity = service.Get( communicationIdentifier );

            if ( entity == null )
            {
                return false;
            }

            var success = service.Delete( entity );
            return success;
        }

        /// <summary>
        /// Creates a new instance with the required fields.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="tagName"></param>
        /// <param name="tagType"></param>
        /// <param name="markup"></param>
        /// <returns></returns>
        public SystemCommunication NewSystemCommunication( Guid guid, string title, string subject, int categoryId )
        {
            var communication = new SystemCommunication();

            communication.Guid = guid;
            communication.Title = title;
            communication.Subject = subject;
            communication.IsActive = true;
            communication.CategoryId = categoryId;

            return communication;
        }

        /// <summary>
        /// Save a new instance to the data store.
        /// </summary>
        /// <param name="newCommunication"></param>
        /// <returns></returns>
        public void SaveSystemCommunication( SystemCommunication newCommunication, CreateExistingItemStrategySpecifier existingItemStrategy = CreateExistingItemStrategySpecifier.Replace )
        {
            var rockContext = new RockContext();

            rockContext.WrapTransaction( () =>
            {
                var service = new SystemCommunicationService( rockContext );
                if ( newCommunication.Guid != Guid.Empty )
                {
                    var existingCommunication = service.Get( newCommunication.Guid );
                    if ( existingCommunication != null )
                    {
                        if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                        {
                            throw new Exception( "Item exists." );
                        }
                        else if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                        {
                            var isDeleted = DeleteSystemCommunication( existingCommunication.Guid.ToString(), rockContext );

                            if ( !isDeleted )
                            {
                                throw new Exception( "Could not replace existing item." );
                            }
                        }
                    }
                }

                service.Add( newCommunication );

                rockContext.SaveChanges();
            } );
        }

        /// <summary>
        /// Get the Communication MediumComponent that is responsible for processing communications of the specified type.
        /// </summary>
        /// <param name="communicationType"></param>
        /// <returns></returns>
        public MediumComponent GetCommunicationMediumComponent( CommunicationType communicationType )
        {
            Guid? mediumEntityTypeGuid = null;
            if ( communicationType == CommunicationType.Email )
            {
                mediumEntityTypeGuid = SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid();
            }
            else if (communicationType == CommunicationType.SMS )
            {
                mediumEntityTypeGuid = SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid();
            }

            var mediumEntity = EntityTypeCache.Get( mediumEntityTypeGuid.Value );
            if ( mediumEntity != null )
            {
                var medium = MediumContainer.GetComponent( mediumEntity.Name );
                return medium;
            }

            return null;
        }
    }
}
