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

using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "The FinancialStatementGeneratorOptions that are configured on the Statement Generator WPF application" )]
    public class FinancialStatementGeneratorOptions
    {
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the person unique identifier.
        /// NULL means to get all individuals
        /// </summary>
        /// <value>
        /// The person unique identifier.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Person DataViewId to filter the statements to
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        public int? DataViewId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [exclude in active individuals].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [exclude in active individuals]; otherwise, <c>false</c>.
        /// </value>
        public bool ExcludeInActiveIndividuals { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include businesses].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include businesses]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeBusinesses { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialStatementTemplate" /> Id
        /// </summary>
        /// <value>
        /// The financial statement template identifier.
        /// </value>
        public int? FinancialStatementTemplateId { get; set; }

        /// <summary>
        /// An option to pre-determine how many pages a statement will be. This will involve
        /// running the statement twice.  The first time will get the actual page count. This value will be provided to the
        /// 'RenderedPageCount' Lava variable on the second pass to allow the template to modify its logic based on the
        /// value
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable page count predetermination]; otherwise, <c>false</c>.
        /// </value>
        public bool EnablePageCountPredetermination { get; set; } = false;

        /// <summary>
        /// Gets or sets the render medium. This is used as a MergeField that the StatementLava can use.
        /// For example, set RenderMedium 'Block' if this will be rendered in a Block
        /// </summary>
        /// <value>
        /// The render medium.
        /// </value>
        public string RenderMedium { get; set; }

        /// <summary>
        /// Gets or sets the individual save options.
        /// </summary>
        /// <value>
        /// The individual save options.
        /// </value>
        public FinancialStatementIndividualSaveOptions IndividualSaveOptions { get; set; }

        /// <summary>
        /// Gets or sets the report configuration list.
        /// </summary>
        /// <value>
        /// The report configuration list.
        /// </value>
        public List<FinancialStatementReportConfiguration> ReportConfigurationList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [RockClientInclude( "Individual Save Options for the FinancialStatementGeneratorOptions" )]
        public class FinancialStatementIndividualSaveOptions
        {
            /// <summary>
            /// Gets or sets a value indicating whether [save statements for individuals].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [save statements for individuals]; otherwise, <c>false</c>.
            /// </value>
            public bool SaveStatementsForIndividuals { get; set; }

            /// <summary>
            /// Gets or sets the document type identifier. <see cref="Rock.Model.DocumentType" />
            /// </summary>
            /// <value>
            /// The document type identifier.
            /// </value>
            public int? DocumentTypeId { get; set; }

            /// <summary>
            /// Gets or sets the name of the document. <see cref="Rock.Model.Document.Name" />
            /// </summary>
            /// <value>
            /// The name of the document.
            /// </value>
            public string DocumentName { get; set; }

            /// <summary>
            /// Gets or sets the document description. <see cref="Rock.Model.Document.Description" />
            /// </summary>
            /// <value>
            /// The document description.
            /// </value>
            public string DocumentDescription { get; set; }

            /// <summary>
            /// Gets or sets the document purpose key.
            /// </summary>
            /// <value>
            /// The document purpose key.
            /// </value>
            public string DocumentPurposeKey { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [overwrite documents of this type created on same date].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [overwrite documents of this type created on same date]; otherwise, <c>false</c>.
            /// </value>
            public bool OverwriteDocumentsOfThisTypeCreatedOnSameDate { get; set; } = true;

            /// <summary>
            /// Gets or sets the document save for.
            /// </summary>
            /// <value>
            /// The document save for.
            /// </value>
            public FinancialStatementIndividualSaveOptionsSaveFor DocumentSaveFor { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [RockClientInclude( "Individual SaveFor Options for the FinancialStatementGeneratorOptions" )]
            public enum FinancialStatementIndividualSaveOptionsSaveFor
            {
                /// <summary>
                /// All active adults ( within the same giving group )
                /// Note that want to limit to Giving Group because some members
                /// of the Family may giving individually and have a different statement
                /// </summary>
                AllActiveAdultsInGivingGroup,

                /// <summary>
                /// The primary giver if Giving Group,
                /// or the Individual if person gives individually
                /// </summary>
                PrimaryGiver,

                /// <summary>
                /// All active family members ( within the same giving group ).
                /// Note that want to limit to Giving Group because some members
                /// of the Family may giving individually and have a different statement
                /// </summary>
                AllActiveFamilyMembersInGivingGroup
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [RockClientInclude( "Report configuration for the StatementGeneratorOptions ReportConfigurationList." )]
        public class FinancialStatementReportConfiguration
        {
            /// <summary>
            /// Gets or sets the primary sort order.
            /// </summary>
            /// <value>
            /// The primary sort order.
            /// </value>
            public FinancialStatementOrderBy PrimarySortOrder { get; set; } = FinancialStatementOrderBy.PostalCode;

            /// <summary>
            /// Gets or sets the secondary sort order.
            /// </summary>
            /// <value>
            /// The secondary sort order.
            /// </value>
            public FinancialStatementOrderBy SecondarySortOrder { get; set; } = FinancialStatementOrderBy.LastName;

            /// <summary>
            /// Where the bulk reports should be saved to
            /// </summary>
            /// <value>
            /// The destination folder.
            /// </value>
            public string DestinationFolder { get; set; }

            /// <summary>
            /// This is the pattern that will be used to create the individual bulk reports. The value 
            /// here is the value that the files will all start with.
            /// </summary>
            /// <value>
            /// The filename prefix.
            /// </value>
            public string FilenamePrefix { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [split files on primary sort value].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [split files on primary sort value]; otherwise, <c>false</c>.
            /// </value>
            /// <remarks>
            /// Much of the logic for filenames will depend on the "Split files on the primary sort value" setting. When 
            /// enabled the file name will contain the primary sort value.When not enabled, and chaptering enabled, the
            /// filename will contain the starting primary sort value and ending primary sort value.
            /// 
            /// Example with "Split files on the primary sort value" enabled (zip code sort ):
            /// statements-85024-1.pdf(-1 at the end is the chapter )
            /// 
            /// Example with "Split files on the primary sort value" disabled( last name sort) :
            /// statements-abbot-benes.pdf
            /// 
            /// If "Prevent Primary Sort Values Orphans" is not enabled, you could have overlap on a single value:
            /// statements-abbot-benes.pdf
            /// statements-benes-carrol.pdf
            /// </remarks>
            public bool SplitFilesOnPrimarySortValue { get; set; }

            /// <summary>
            /// This is the upper threshold of statements to include in a chapter.
            /// </summary>
            /// <value>
            /// The size of the chapter.
            /// </value>
            public int? MaxStatementsPerChapter { get; set; }

            /// <summary>
            ///  When chaptering, this setting will keep the primary sort values 
            ///  from being split across a chapter. When this is enabled, it could mean that more statements are in a
            ///  chapter that the max statements per chapter setting allows.
            /// </summary>
            /// <value>
            ///   <c>true</c> if [prevent splitting primary sort values across chapters]; otherwise, <c>false</c>.
            /// </value>
            public bool PreventSplittingPrimarySortValuesAcrossChapters { get; set; }

            /// <summary>
            /// The minimum number of total contributions a statement must have 
            /// before it's added to the bulk report.
            /// </summary>
            /// <value>
            /// The minimum contribution amount.
            /// </value>
            public decimal? MinimumContributionAmount { get; set; }

            /// <summary>
            /// Determines if individuals with addresses should be added to the 
            /// report.Addresses with countries the same as the Organization's address( Global Attribute ) or
            /// addresses with blank countries will be considered local.
            /// </summary>
            /// <value>
            ///   <c>true</c> if [include international addresses]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeInternationalAddresses { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [exclude opted out individuals].
            /// If this is a statement for a GivingGroup, exclude if *any* members of the giving group have opted out.
            /// </summary>
            /// <value>
            ///   <c>true</c> if [exclude opted out individuals]; otherwise, <c>false</c>.
            /// </value>
            public bool ExcludeOptedOutIndividuals { get; set; } = true;

            /// <summary>
            /// Gets or sets a value indicating whether [exclude recipients that have an incomplete address].
            /// Either the recipient has no mailing address, or the address is missing <seealso cref="Rock.Model.Location.PostalCode"/> or <seealso cref="Rock.Model.Location.Street1"/> 
            /// </summary>
            /// <value>
            ///   <c>true</c> if [exclude recipients that have an incomplete address]; otherwise, <c>false</c>.
            /// </value>
            public bool ExcludeRecipientsThatHaveAnIncompleteAddress { get; set; } = true;

            /// <summary>
            /// Gets or sets the created date time.
            /// Use this for Sorting.
            /// </summary>
            /// <value>
            /// The created date time.
            /// </value>
            public DateTime? CreatedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the configuration unique identifier.
            /// </summary>
            /// <value>
            /// The configuration unique identifier.
            /// </value>
            public Guid Guid { get; set; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "FinancialStatement OrderBy options" )]
    public enum FinancialStatementOrderBy
    {
        /// <summary>
        /// Sort by postal code
        /// </summary>
        PostalCode = 0,

        /// <summary>
        /// Sort by Last Name
        /// </summary>
        LastName = 1,

        /// <summary>
        /// Sort by Page Count
        /// </summary>
        PageCount = 2
    }
}
