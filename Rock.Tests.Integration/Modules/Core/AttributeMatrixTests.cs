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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core
{
    /// <summary>
    /// Tests for Entity Attributes.
    /// </summary>
    [TestClass]
    public class AttributeMatrixTests : DatabaseTestsBase
    {
        [ClassInitialize]
        public static void Initialize( TestContext context )
        {
            InitializeTestData();
        }

        /// <summary>
        /// Verify that updating a matrix attribute associated with a Person
        /// also adds entries to the Person History Log.
        /// </summary>
        [TestMethod]
        public void AttributeMatrix_UpdateMatrixAttributeValueForPerson_CreatesHistoryRecords()
        {
            var targetEntity = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            VerifyHistoryForMatrixAttributeValueUpdate( targetEntity,
                PersonMatrixAttribute1Key,
                "ArticleTitle",
                "Ted's Musings Volume 1, Article 1 (Rev 1)",
                historyRecordShouldExist: true );

            VerifyHistoryForMatrixAttributeValueUpdate( targetEntity,
                PersonMatrixAttribute1Key,
                "ArticleTitle",
                "Ted's Musings Volume 1, Article 1 (Rev 2)",
                historyRecordShouldExist: true );
        }

        /// <summary>
        /// Verify that updating a matrix attribute associated with an Entity other than a Person
        /// does not add any entries to the History Log for that entity.
        /// </summary>
        [TestMethod]
        public void AttributeMatrix_UpdateMatrixAttributeValueForGroup_DoesNotCreateHistoryRecords()
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            var deckerGroup = groupService.Get( TestGuids.Groups.SmallGroupDeckerGuid );

            VerifyHistoryForMatrixAttributeValueUpdate( deckerGroup,
                GroupMatrixAttribute1Key,
                "ArticleTitle",
                "Decker Group Thoughts Vol. 1",
                historyRecordShouldExist: false );

            var marbleGroup = groupService.Get( TestGuids.Groups.SmallGroupMarbleGuid );

            VerifyHistoryForMatrixAttributeValueUpdate( marbleGroup,
                GroupMatrixAttribute1Key,
                "ArticleTitle",
                "Marble Group Thoughts Vol. 1",
                historyRecordShouldExist: false );
        }

        private void VerifyHistoryForMatrixAttributeValueUpdate( IHasAttributes targetEntity, string matrixAttributeKey, string matrixColumnAttributeKey, string newAttributeValue, bool historyRecordShouldExist )
        {
            var rockContext = new RockContext();

            // Get the Attribute Matrix corresponding to the specified Attribute of the target entity.
            // The link is stored in the Attribute of the target entity, such that the attribute value is the Guid of the Attribute Matrix.
            targetEntity.LoadAttributes( rockContext );

            var attributeMatrixGuid = targetEntity.GetAttributeValue( matrixAttributeKey );

            var matrixService = new AttributeMatrixService( rockContext );
            var matrix = matrixService.Get( attributeMatrixGuid );

            var targetEntityTypeId = EntityTypeCache.GetId( targetEntity.GetType() );

            var historyService = new HistoryService( rockContext );
            var historyItems = historyService.Queryable()
                .AsNoTracking()
                .Where( h => h.EntityTypeId == targetEntityTypeId
                    && h.EntityId == targetEntity.Id )
                .OrderBy( h => h.CreatedDateTime )
                .ToList();

            var lastHistoryEntryDateTime = historyItems
                .Select( h => h.CreatedDateTime )
                .LastOrDefault();

            // Get the first row in the matrix, and alter one of the column values.
            var item = matrix.AttributeMatrixItems.FirstOrDefault();
            item.LoadAttributes( rockContext );

            var oldAttributeValue = item.GetAttributeValue( matrixColumnAttributeKey );

            item.SetAttributeValue( matrixColumnAttributeKey, newAttributeValue );

            item.SaveAttributeValues( rockContext );

            // Wait to allow the history records to be added, because
            // they are processed on a background thread.
            Task.Delay( 1000 ).Wait();

            // Verify that a new history entry has been added.
            var postUpdateHistoryItems = historyService.Queryable()
                .Where( h => h.EntityTypeId == targetEntityTypeId
                    && h.EntityId == targetEntity.Id
                    && h.CreatedDateTime > lastHistoryEntryDateTime )
                .OrderBy( h => h.CreatedDateTime )
                .ToList();

            var valueChangeHistoryItems = postUpdateHistoryItems
                .Where( h => h.OldValue == oldAttributeValue
                    && h.NewValue == newAttributeValue )
                .ToList();

            var expectedCount = historyRecordShouldExist ? 1 : 0;
            var actualCount = valueChangeHistoryItems.Count();
            Assert.IsTrue( actualCount == expectedCount,
                $"History log is incorrect. Expected {expectedCount} entries, found {actualCount} entries." );
        }

        #region Test Data

        private static string PersonMatrixAttribute1Guid = "ACF1E091-A047-4B28-8B4C-2D3B39C42F63";
        private static string PersonMatrixAttribute1Key = "PersonMatrixAttribute1";
        private static string PersonMatrixTemplateName = "Internal Article List";
        private static string GroupMatrixAttribute1Guid = "2F38607E-D1DD-4188-8B75-7CBE1B3E9B04";
        private static string GroupMatrixAttribute1Key = "GroupMatrixAttribute1";
        private static string GroupMatrixTemplateName = "Internal Article List";
        private static void InitializeTestData()
        {
            InitializePersonAttributeMatrixTestData();
            InitializeGroupAttributeMatrixTestData();
        }

        private static void InitializePersonAttributeMatrixTestData()
        {
            var rockContext = new RockContext();

            // Add an Attribute Matrix for the Person Entity: MatrixA
            var matrixTemplateService = new AttributeMatrixTemplateService( rockContext );
            var matrixTemplate = matrixTemplateService.Queryable()
                .Where( t => t.Name == PersonMatrixTemplateName )
                .FirstOrDefault();

            var personEntityTypeId = EntityTypeCache.GetId( typeof( Person ) );
            var attributeId = AddMatrixAttributeForEntity( matrixTemplate.Guid,
                personEntityTypeId.GetValueOrDefault(),
                PersonMatrixAttribute1Guid.AsGuid(),
                PersonMatrixAttribute1Key );

            var categorySocialId = CategoryCache.Get( SystemGuid.Category.PERSON_ATTRIBUTES_SOCIAL.AsGuid() ).Id;
            AddAttributeCategory( attributeId, categorySocialId );

            // Create an Attribute Matrix and set the values for Ted Decker/Attribute1.
            // Note that the Attribute Matrix is an independent entity that represents a collection of values
            // corresponding to a template; it is not linked to a target entity by default.
            // In this case, the target entity for the Attribute Matrix is Ted Decker.
            rockContext = new RockContext();

            // Link a new Attribute Matrix to Ted Decker.
            // The link is stored in the Person Attribute, and the attribute value is the Guid of the Attribute Matrix.
            var tedAttributeMatrixGuid = "E6CFFBE6-A9AF-4D78-9DC2-784F9EB2D6E0";
            var personTedDecker = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            personTedDecker.LoadAttributes( rockContext );
            personTedDecker.SetAttributeValue( PersonMatrixAttribute1Key, tedAttributeMatrixGuid );
            personTedDecker.SaveAttributeValues( rockContext );

            // Create an Attribute Matrix for Ted Decker/Attribute1
            var tedMatrix1Rows = new List<string>();
            tedMatrix1Rows.Add( "ArticleTitle=Ted's Musings Volume 1, Article 1|ArticleLink=https://rocksolidchurchdemo.com/ted/article1" );
            tedMatrix1Rows.Add( "ArticleTitle=Ted's Musings Volume 1, Article 2|ArticleLink=https://rocksolidchurchdemo.com/ted/article2" );

            var tedDeckerMatrix1 = AddAttributeMatrix( tedAttributeMatrixGuid.AsGuid(), matrixTemplate.Id, tedMatrix1Rows );

            // Create an Attribute Matrix for Bill Marble/Attribute1.
            var billAttributeMatrixGuid = "7CC709CA-1546-423E-8706-95AC31356AA7";
            var personBillMarble = TestDataHelper.GetTestPerson( TestGuids.TestPeople.BillMarble );

            personBillMarble.LoadAttributes( rockContext );
            personBillMarble.SetAttributeValue( PersonMatrixAttribute1Key, billAttributeMatrixGuid );
            personBillMarble.SaveAttributeValues( rockContext );

            var billMatrix1Rows = new List<string>();
            billMatrix1Rows.Add( "ArticleTitle=Bill's Musings Volume 1, Article 1|ArticleLink=https://rocksolidchurchdemo.com/bill/article1" );
            billMatrix1Rows.Add( "ArticleTitle=Bill's Musings Volume 1, Article 2|ArticleLink=https://rocksolidchurchdemo.com/bill/article2" );

            var billMarbleMatrix1 = AddAttributeMatrix( billAttributeMatrixGuid.AsGuid(), matrixTemplate.Id, billMatrix1Rows );
        }

        private static void InitializeGroupAttributeMatrixTestData()
        {
            var rockContext = new RockContext();

            // Add an Attribute Matrix for the Group Entity: MatrixA.
            // Note that although the selected matrix template is not appropriate for this Entity Type, it is the only template
            // currently available in the sample data set. When an API is available, we should create a new matrix template here.
            var matrixTemplateService = new AttributeMatrixTemplateService( rockContext );
            var matrixTemplate = matrixTemplateService.Queryable()
                .Where( t => t.Name == GroupMatrixTemplateName )
                .FirstOrDefault();

            var groupEntityTypeId = EntityTypeCache.GetId( typeof( Group ) );
            var attributeId = AddMatrixAttributeForEntity( matrixTemplate.Guid,
                groupEntityTypeId.GetValueOrDefault(),
                GroupMatrixAttribute1Guid.AsGuid(),
                GroupMatrixAttribute1Key );

            var categoryGroupChangeId = CategoryCache.Get( SystemGuid.Category.HISTORY_GROUP_CHANGES.AsGuid() ).Id;
            AddAttributeCategory( attributeId, categoryGroupChangeId );

            // Create an Attribute Matrix and set the values for Decker/Attribute1.
            rockContext = new RockContext();

            // Link a new Attribute Matrix to the Decker Group.
            // The link is stored in the Group Attribute, and the attribute value is the Guid of the Attribute Matrix.
            var deckerAttributeMatrixGuid = "FBD6ADB3-8DE1-468B-8A48-99B79DE37805";

            var groupService = new GroupService( rockContext );
            var deckerGroup = groupService.Get( TestGuids.Groups.SmallGroupDeckerGuid );

            deckerGroup.LoadAttributes( rockContext );
            deckerGroup.SetAttributeValue( GroupMatrixAttribute1Key, deckerAttributeMatrixGuid );
            deckerGroup.SaveAttributeValues( rockContext );

            // Create an Attribute Matrix for Decker/Attribute1
            var deckerMatrix1Rows = new List<string>();
            deckerMatrix1Rows.Add( "ArticleTitle=Decker Group Thoughts Vol. 1|ArticleLink=https://rocksolidchurchdemo.com/decker/article1" );
            deckerMatrix1Rows.Add( "ArticleTitle=Decker Group Thoughts Vol. 2|ArticleLink=https://rocksolidchurchdemo.com/decker/article2" );

            var deckerMatrix1 = AddAttributeMatrix( deckerAttributeMatrixGuid.AsGuid(), matrixTemplate.Id, deckerMatrix1Rows );

            // Create an Attribute Matrix for Marble/Attribute1.
            var marbleAttributeMatrixGuid = "9EB6556D-24F6-437F-ACE0-0C35E14C3446";

            var marbleGroup = groupService.Get( TestGuids.Groups.SmallGroupMarbleGuid );

            marbleGroup.LoadAttributes( rockContext );
            marbleGroup.SetAttributeValue( GroupMatrixAttribute1Key, marbleAttributeMatrixGuid );
            marbleGroup.SaveAttributeValues( rockContext );

            var marbleMatrix1Rows = new List<string>();
            marbleMatrix1Rows.Add( "ArticleTitle=Marble Group Thoughts Vol. 1|ArticleLink=https://rocksolidchurchdemo.com/marble/article1" );
            marbleMatrix1Rows.Add( "ArticleTitle=Marble Group Thoughts Vol. 2|ArticleLink=https://rocksolidchurchdemo.com/marble/article2" );

            var marbleMatrix1 = AddAttributeMatrix( marbleAttributeMatrixGuid.AsGuid(), matrixTemplate.Id, marbleMatrix1Rows );
        }

        #endregion

        #region Attribute Matrix Data Manager

        /// <summary>
        /// Add a new Attribute Matrix using the specified template.
        /// </summary>
        /// <param name="matrixTemplateId"></param>
        /// <param name="rowValues"></param>
        /// <returns></returns>
        private static AttributeMatrix AddAttributeMatrix( Guid matrixGuid, int matrixTemplateId, List<string> rowValues )
        {
            var rockContext = new RockContext();

            AttributeMatrix matrix = null;

            // If a matrix with the same identifier exists, remove it.
            var matrixService = new AttributeMatrixService( rockContext );
            matrix = matrixService.Get( matrixGuid );
            if ( matrix != null )
            {
                DeleteAttributeMatrix( matrix.Id );
            }

            rockContext.WrapTransaction( () =>
            {
                matrix = new AttributeMatrix();

                matrix.Guid = matrixGuid;
                matrix.AttributeMatrixTemplateId = matrixTemplateId;

                matrix.AttributeMatrixItems = new List<AttributeMatrixItem>();

                matrixService.Add( matrix );

                var matrixItems = new List<AttributeMatrixItem>();

                foreach ( var rowValueList in rowValues )
                {
                    var keyValuePairs = rowValueList.SplitDelimitedValues( "|" );

                    var matrixItem = new AttributeMatrixItem();
                    matrixItem.AttributeMatrix = matrix;

                    matrixItem.LoadAttributes( rockContext );

                    foreach ( var keyValuePair in keyValuePairs )
                    {
                        var keyValuePairElements = keyValuePair.SplitDelimitedValues( "=" );

                        matrixItem.SetAttributeValue( keyValuePairElements[0], keyValuePairElements[1] );
                    }

                    matrix.AttributeMatrixItems.Add( matrixItem );

                    matrixItems.Add( matrixItem );
                }

                // Save the Matrix and MatrixItems.
                rockContext.SaveChanges();

                // Save the values for the MatrixItem rows.
                foreach ( var matrixItem in matrixItems )
                {
                    matrixItem.SaveAttributeValues( rockContext );
                }
            } );

            return matrix;
        }

        /// <summary>
        /// Delete an Attribute Matrix.
        /// </summary>
        /// <param name="matrixId"></param>
        /// <returns></returns>
        private static bool DeleteAttributeMatrix( int matrixId )
        {
            var rockContext = new RockContext();
            var matrixService = new AttributeMatrixService( rockContext );

            var matrix = matrixService.Get( matrixId );

            if ( matrix == null )
            {
                return false;
            }

            var matrixItemService = new AttributeMatrixItemService( rockContext );

            rockContext.WrapTransaction( () =>
            {

                matrixItemService.DeleteRange( matrix.AttributeMatrixItems );

                matrixService.Delete( matrix );

                rockContext.SaveChanges();
            } );

            return true;
        }

        /// <summary>
        /// Adds a matrix attribute to a target entity, using the specified matrix template.
        /// </summary>
        /// <param name="matrixTemplateGuid"></param>
        /// <param name="targetEntityMatrixAttributeGuid">The entity with which the Matrix Attribute is associated.</param>
        private static int AddMatrixAttributeForEntity( Guid matrixTemplateGuid, int entityTypeId, Guid targetEntityMatrixAttributeGuid, string attributeKey )
        {
            var rockContext = new RockContext();

            var matrixFieldTypeId = FieldTypeCache.GetId( SystemGuid.FieldType.MATRIX.AsGuid() ).GetValueOrDefault();

            var attributeService = new AttributeService( rockContext );

            // Add Attribute "MatrixA" to the Person Entity that is a Matrix of type "Internal Article List".
            var attributePersonMatrixA = attributeService.Get( targetEntityMatrixAttributeGuid );
            if ( attributePersonMatrixA != null )
            {
                attributeService.Delete( attributePersonMatrixA );
                rockContext.SaveChanges();
            }
            attributePersonMatrixA = new Rock.Model.Attribute();
            attributePersonMatrixA.EntityTypeId = entityTypeId;
            attributePersonMatrixA.Name = attributeKey;
            attributePersonMatrixA.Key = attributeKey;
            attributePersonMatrixA.Guid = targetEntityMatrixAttributeGuid;
            attributePersonMatrixA.FieldTypeId = matrixFieldTypeId;

            // Set the extended properties for this Matrix Attribute.
            var matrixTemplateService = new AttributeMatrixTemplateService( rockContext );
            var matrixTemplate = matrixTemplateService.Get( matrixTemplateGuid );

            var attributeQualifier = new AttributeQualifier();
            attributeQualifier.IsSystem = false;
            attributeQualifier.Key = MatrixFieldType.ATTRIBUTE_MATRIX_TEMPLATE;
            attributeQualifier.Value = matrixTemplate.Id.ToString();

            attributePersonMatrixA.AttributeQualifiers.Add( attributeQualifier );

            attributeService.Add( attributePersonMatrixA );

            rockContext.SaveChanges();

            return attributePersonMatrixA.Id;
        }

        private static void AddAttributeCategory( int attributeId, int categoryId )
        {
            var rockContext = new RockContext();

            var attributeService = new AttributeService( rockContext );
            var attribute = attributeService.Get( attributeId );

            var categoryService = new CategoryService( rockContext );
            var category = categoryService.Get( categoryId );

            attribute.Categories.Add( category );

            rockContext.SaveChanges();
        }

        #endregion
    }
}
