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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_0111 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddExportAPIsMaxItemsPerPageGlobalAttribute();
            SetRestActionAuthSecurityForExportAPIs();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// MP: Sets the rest action authentication security for export APIs.
        /// </summary>
        private void SetRestActionAuthSecurityForExportAPIs()
        {
            // Ensure the new Export API RestActions exist
            Sql( @"
IF NOT EXISTS (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/FinancialTransactions/Export?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDirection={sortDirection}&dataViewId={dataViewId}&modifiedSince={modifiedSince}&startDateTime={startDateTime}&endDateTime={endDateTime}&attributeKeys={attributeKeys}&attributeReturnType={attributeReturnType}') 
	INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )
		SELECT [Id], 'GET', 'GETapi/FinancialTransactions/Export?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDirection={sortDirection}&dataViewId={dataViewId}&modifiedSince={modifiedSince}&startDateTime={startDateTime}&endDateTime={endDateTime}&attributeKeys={attributeKeys}&attributeReturnType={attributeReturnType}', 'api/FinancialTransactions/Export?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDirection={sortDirection}&dataViewId={dataViewId}&modifiedSince={modifiedSince}&startDateTime={startDateTime}&endDateTime={endDateTime}&attributeKeys={attributeKeys}&attributeReturnType={attributeReturnType}', NEWID()
		FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.FinancialTransactionsController'

IF NOT EXISTS (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/People/Export?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDirection={sortDirection}&dataViewId={dataViewId}&modifiedSince={modifiedSince}&attributeKeys={attributeKeys}&attributeReturnType={attributeReturnType}') 
	INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )
		SELECT [Id], 'GET', 'GETapi/People/Export?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDirection={sortDirection}&dataViewId={dataViewId}&modifiedSince={modifiedSince}&attributeKeys={attributeKeys}&attributeReturnType={attributeReturnType}', 'api/People/Export?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDirection={sortDirection}&dataViewId={dataViewId}&modifiedSince={modifiedSince}&attributeKeys={attributeKeys}&attributeReturnType={attributeReturnType}', NEWID()
		FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.PeopleController'
" );

            // Restrict Export API Endpoint to Rock Administrators
            Sql( @"
IF NOT EXISTS (SELECT * FROM [Auth] WHERE [Guid] = 'A8BCD327-F9C7-4367-8014-84B9BBBD9DCC')
	INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] ) 
		VALUES (
			(SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'), 
			(SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/People/Export?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDirection={sortDirection}&dataViewId={dataViewId}&modifiedSince={modifiedSince}&attributeKeys={attributeKeys}&attributeReturnType={attributeReturnType}'), 
			0, 'View', 'A', 0, 
			(SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E'), 
			'A8BCD327-F9C7-4367-8014-84B9BBBD9DCC')


IF NOT EXISTS (SELECT * FROM [Auth] WHERE [Guid] = '096FBF02-478B-44BB-9CAD-062D3C07B0BA')
	INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] ) 
		VALUES (
			(SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'), 
			(SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/People/Export?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDirection={sortDirection}&dataViewId={dataViewId}&modifiedSince={modifiedSince}&attributeKeys={attributeKeys}&attributeReturnType={attributeReturnType}'), 
			1, 'View', 'D', 1, 
			NULL, 
			'096FBF02-478B-44BB-9CAD-062D3C07B0BA')


IF NOT EXISTS (SELECT * FROM [Auth] WHERE [Guid] = '4A728929-15E9-43EE-9671-E41FEB28B93A')
	INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] ) 
		VALUES (
			(SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'), 
			(SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/FinancialTransactions/Export?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDirection={sortDirection}&dataViewId={dataViewId}&modifiedSince={modifiedSince}&startDateTime={startDateTime}&endDateTime={endDateTime}&attributeKeys={attributeKeys}&attributeReturnType={attributeReturnType}'), 
			0, 'View', 'A', 0, 
			(SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E'), 
			'4A728929-15E9-43EE-9671-E41FEB28B93A')


IF NOT EXISTS (SELECT * FROM [Auth] WHERE [Guid] = '281E9A12-2449-433F-B551-A5C9006EF2C8')
	INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] ) 
		VALUES (
			(SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'), 
			(SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/FinancialTransactions/Export?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDirection={sortDirection}&dataViewId={dataViewId}&modifiedSince={modifiedSince}&startDateTime={startDateTime}&endDateTime={endDateTime}&attributeKeys={attributeKeys}&attributeReturnType={attributeReturnType}'), 
			1, 'View', 'D', 1, 
			NULL, 
			'281E9A12-2449-433F-B551-A5C9006EF2C8')
" );
        }

        /// <summary>
        /// MP: Adds the API maximum items per page global attribute.
        /// </summary>
        private void AddExportAPIsMaxItemsPerPageGlobalAttribute()
        {
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, null, null, "Export APIs Max Items Per Page", "The maximum number of items that can be returned when a page of data is requested thru the REST Export APIs.", 0, "1000", "F80A2DDC-88D0-40BB-8376-1A6323B44886", "core_ExportAPIsMaxItemsPerPage" );
        }
    }
}
