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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.BugFixes
{
    /// <summary>
    /// Tests that verify correct functionality after fixing github issue #1808.
    /// </summary>
    [TestClass]
    public class Issue1808
    {
        private const string ReferenceDateValue = "2023-10-09T00:00:00.0000000";

        #region LessThan

        [TestMethod]
        public void LessThanFilterDoesNotIncludeMissingValues()
        {
            var tedDeckerPersonId = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ).Id;
            var baptismDateAttributeGuid = new Guid( "d42763fa-28e9-4a55-a25a-48998d7d7fef" );

            WithAttributeValueRestorer( tedDeckerPersonId, baptismDateAttributeGuid, () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    DeleteAttributeValue( rockContext, baptismDateAttributeGuid, tedDeckerPersonId );

                    var containsPersonId = BaptismDateFilterContainsPersonId( rockContext, ComparisonType.LessThan, tedDeckerPersonId );

                    Assert.That.IsFalse( containsPersonId );
                }
            } );
        }

        [TestMethod]
        public void LessThanFilterDoesNotIncludeNullValues()
        {
            var tedDeckerPersonId = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ).Id;
            var baptismDateAttributeGuid = new Guid( "d42763fa-28e9-4a55-a25a-48998d7d7fef" );

            WithAttributeValueRestorer( tedDeckerPersonId, baptismDateAttributeGuid, () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    UpdateAttributeValue( rockContext, baptismDateAttributeGuid, tedDeckerPersonId, null );

                    var containsPersonId = BaptismDateFilterContainsPersonId( rockContext, ComparisonType.LessThan, tedDeckerPersonId );

                    Assert.That.IsFalse( containsPersonId );
                }
            } );
        }

        [TestMethod]
        public void LessThanFilterDoesNotIncludeEmptyValues()
        {
            var tedDeckerPersonId = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ).Id;
            var baptismDateAttributeGuid = new Guid( "d42763fa-28e9-4a55-a25a-48998d7d7fef" );

            WithAttributeValueRestorer( tedDeckerPersonId, baptismDateAttributeGuid, () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    UpdateAttributeValue( rockContext, baptismDateAttributeGuid, tedDeckerPersonId, string.Empty );

                    var containsPersonId = BaptismDateFilterContainsPersonId( rockContext, ComparisonType.LessThan, tedDeckerPersonId );

                    Assert.That.IsFalse( containsPersonId );
                }
            } );
        }

        [TestMethod]
        public void LessThanFilterDoesNotIncludeLargerValues()
        {
            var tedDeckerPersonId = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ).Id;
            var baptismDateAttributeGuid = new Guid( "d42763fa-28e9-4a55-a25a-48998d7d7fef" );

            WithAttributeValueRestorer( tedDeckerPersonId, baptismDateAttributeGuid, () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    UpdateAttributeValue( rockContext, baptismDateAttributeGuid, tedDeckerPersonId, "2030-12-31" );

                    var containsPersonId = BaptismDateFilterContainsPersonId( rockContext, ComparisonType.LessThan, tedDeckerPersonId );

                    Assert.That.IsFalse( containsPersonId );
                }
            } );
        }

        [TestMethod]
        public void LessThanFilterIncludesSmallerValues()
        {
            var tedDeckerPersonId = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ).Id;
            var baptismDateAttributeGuid = new Guid( "d42763fa-28e9-4a55-a25a-48998d7d7fef" );

            WithAttributeValueRestorer( tedDeckerPersonId, baptismDateAttributeGuid, () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    UpdateAttributeValue( rockContext, baptismDateAttributeGuid, tedDeckerPersonId, "2001-09-13" );

                    var containsPersonId = BaptismDateFilterContainsPersonId( rockContext, ComparisonType.LessThan, tedDeckerPersonId );

                    Assert.That.IsTrue( containsPersonId );
                }
            } );
        }

        #endregion

        #region LessThanOrEqualTo

        [TestMethod]
        public void LessThanOrEqualToFilterDoesNotIncludeMissingValues()
        {
            var tedDeckerPersonId = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ).Id;
            var baptismDateAttributeGuid = new Guid( "d42763fa-28e9-4a55-a25a-48998d7d7fef" );

            WithAttributeValueRestorer( tedDeckerPersonId, baptismDateAttributeGuid, () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    DeleteAttributeValue( rockContext, baptismDateAttributeGuid, tedDeckerPersonId );

                    var containsPersonId = BaptismDateFilterContainsPersonId( rockContext, ComparisonType.LessThanOrEqualTo, tedDeckerPersonId );

                    Assert.That.IsFalse( containsPersonId );
                }
            } );
        }

        [TestMethod]
        public void LessThanOrEqualToFilterDoesNotIncludeNullValues()
        {
            var tedDeckerPersonId = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ).Id;
            var baptismDateAttributeGuid = new Guid( "d42763fa-28e9-4a55-a25a-48998d7d7fef" );

            WithAttributeValueRestorer( tedDeckerPersonId, baptismDateAttributeGuid, () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    UpdateAttributeValue( rockContext, baptismDateAttributeGuid, tedDeckerPersonId, null );

                    var containsPersonId = BaptismDateFilterContainsPersonId( rockContext, ComparisonType.LessThanOrEqualTo, tedDeckerPersonId );

                    Assert.That.IsFalse( containsPersonId );
                }
            } );
        }

        [TestMethod]
        public void LessThanOrEqualToFilterDoesNotIncludeEmptyValues()
        {
            var tedDeckerPersonId = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ).Id;
            var baptismDateAttributeGuid = new Guid( "d42763fa-28e9-4a55-a25a-48998d7d7fef" );

            WithAttributeValueRestorer( tedDeckerPersonId, baptismDateAttributeGuid, () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    UpdateAttributeValue( rockContext, baptismDateAttributeGuid, tedDeckerPersonId, string.Empty );

                    var containsPersonId = BaptismDateFilterContainsPersonId( rockContext, ComparisonType.LessThanOrEqualTo, tedDeckerPersonId );

                    Assert.That.IsFalse( containsPersonId );
                }
            } );
        }

        [TestMethod]
        public void LessThanOrEqualToFilterDoesNotIncludeLargerValues()
        {
            var tedDeckerPersonId = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ).Id;
            var baptismDateAttributeGuid = new Guid( "d42763fa-28e9-4a55-a25a-48998d7d7fef" );

            WithAttributeValueRestorer( tedDeckerPersonId, baptismDateAttributeGuid, () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    UpdateAttributeValue( rockContext, baptismDateAttributeGuid, tedDeckerPersonId, "2030-12-31" );

                    var containsPersonId = BaptismDateFilterContainsPersonId( rockContext, ComparisonType.LessThanOrEqualTo, tedDeckerPersonId );

                    Assert.That.IsFalse( containsPersonId );
                }
            } );
        }

        [TestMethod]
        public void LessThanOrEqualToFilterIncludesSmallerValues()
        {
            var tedDeckerPersonId = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker ).Id;
            var baptismDateAttributeGuid = new Guid( "d42763fa-28e9-4a55-a25a-48998d7d7fef" );

            WithAttributeValueRestorer( tedDeckerPersonId, baptismDateAttributeGuid, () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    UpdateAttributeValue( rockContext, baptismDateAttributeGuid, tedDeckerPersonId, "2001-09-13" );

                    var containsPersonId = BaptismDateFilterContainsPersonId( rockContext, ComparisonType.LessThanOrEqualTo, tedDeckerPersonId );

                    Assert.That.IsTrue( containsPersonId );
                }
            } );
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Helper method that updates (and saves) an attribute value.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <param name="attributeGuid">The related attribute.</param>
        /// <param name="personId">The person identifier whose value should be updated.</param>
        /// <param name="value">The new value.</param>
        private void UpdateAttributeValue( RockContext rockContext, Guid attributeGuid, int personId, string value )
        {
            var attributeValueService = new AttributeValueService( rockContext );
            var attributeValue = attributeValueService.Queryable()
                .Where( av => av.EntityId == personId && av.Attribute.Guid == attributeGuid )
                .SingleOrDefault();

            if ( attributeValue != null )
            {
                attributeValue.Value = value;
                attributeValue.UpdateValueAsProperties( rockContext );
            }
            else
            {
                attributeValue = new AttributeValue
                {
                    AttributeId = AttributeCache.Get( attributeGuid ).Id,
                    EntityId = personId,
                    Value = value
                };

                attributeValueService.Add( attributeValue );
            }

            rockContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
        }

        /// <summary>
        /// Helper method that deletes (and saves) an attribute value.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <param name="attributeGuid">The related attribute.</param>
        /// <param name="personId">The person identifier whose value should be updated.</param>
        private void DeleteAttributeValue( RockContext rockContext, Guid attributeGuid, int personId )
        {
            var attributeValueService = new AttributeValueService( rockContext );
            var attributeValue = attributeValueService.Queryable()
                .Where( av => av.EntityId == personId && av.Attribute.Guid == attributeGuid )
                .SingleOrDefault();

            if ( attributeValue == null )
            {
                return;
            }

            attributeValueService.Delete( attributeValue );

            rockContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
        }

        /// <summary>
        /// Helper method to check if the BaptismDate filter would contain the
        /// specified person.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <param name="comparisonType">The type of comparison to use with the filter.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns><c>true</c> if the filter contains the person; otherwise <c>false</c>.</returns>
        private bool BaptismDateFilterContainsPersonId( RockContext rockContext, ComparisonType comparisonType, int personId )
        {
            var personService = new PersonService( rockContext );
            var parameterExpression = personService.ParameterExpression;
            var propertyFilter = new Rock.Reporting.DataFilter.PropertyFilter();
            var selection = new List<string>
            {
                "Attribute_BaptismDate_d42763fa28e94a55a25a48998d7d7fef",
                comparisonType.ConvertToInt().ToString(),
                $"{ReferenceDateValue}\tAll||||"
            }.ToJson();

            var expression = propertyFilter.GetExpression( typeof( Person ), personService, parameterExpression, selection );

            return personService.Queryable()
                .Where( parameterExpression, expression )
                .Select( p => p.Id )
                .Contains( personId );
        }

        /// <summary>
        /// Helper method that restores an attribute value after the action
        /// has run.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="action">The action to execute.</param>
        private void WithAttributeValueRestorer( int personId, Guid attributeGuid, Action action )
        {
            AttributeValue originalAttributeValue = null;

            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );

                originalAttributeValue = attributeValueService
                    .Queryable()
                    .Where( av => av.EntityId == personId && av.Attribute.Guid == attributeGuid )
                    .SingleOrDefault();
            }

            try
            {
                action();
            }
            finally
            {
                using ( var rockContext = new RockContext() )
                {
                    var attributeValueService = new AttributeValueService( rockContext );
                    var currentAttributeValue = attributeValueService
                        .Queryable()
                        .Where( av => av.EntityId == personId && av.Attribute.Guid == attributeGuid )
                        .FirstOrDefault();

                    if ( currentAttributeValue != null && originalAttributeValue == null )
                    {
                        attributeValueService.Delete( currentAttributeValue );
                    }
                    else if ( currentAttributeValue == null && originalAttributeValue != null )
                    {
                        currentAttributeValue = originalAttributeValue.Clone( false );
                        currentAttributeValue.UpdateValueAsProperties( rockContext );

                        currentAttributeValue.Id = 0;
                        attributeValueService.Add( currentAttributeValue );
                    }
                    else if ( currentAttributeValue != null && originalAttributeValue != null )
                    {
                        var id = currentAttributeValue.Id;

                        currentAttributeValue.CopyPropertiesFrom( originalAttributeValue );
                        currentAttributeValue.UpdateValueAsProperties( rockContext );
                        currentAttributeValue.Id = id;
                    }

                    rockContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
                }
            }
        }

        #endregion
    }
}
