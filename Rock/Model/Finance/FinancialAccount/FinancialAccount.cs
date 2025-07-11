﻿// <copyright>
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

using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents an account that gifts/donations and other <see cref="Rock.Model.FinancialTransaction">Financial Transactions</see> are posted to.  
    /// FinancialAccounts are hierarchical and are orderable.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialAccount" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.FINANCIAL_ACCOUNT )]
    public partial class FinancialAccount : Model<FinancialAccount>, IOrdered, IHasActiveFlag, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the FinancialAccountId of the parent FinancialAccount to this FinancialAccount. If this
        /// FinancialAccount does not have a parent, this property will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the FinancialAccountId of the parent FinancialAccount to this FinancialAccount. 
        /// This property will be null if the FinancialAccount does not have a parent.
        /// </value>
        [DataMember]
        [EnableAttributeQualification]
        public int? ParentAccountId { get; set; }

        /// <summary>
        /// Gets or sets the CampusId of the <see cref="Rock.Model.Campus"/> that this FinancialAccount is associated with. If this FinancialAccount is not
        /// associated with a <see cref="Rock.Model.Campus"/> this property will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CampusId of the <see cref="Rock.Model.Campus"/> that the FinancialAccount is associated with.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.CAMPUS )]
        [EnableAttributeQualification]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the (internal) Name of the FinancialAccount. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the (internal) name of the FinancialAccount.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public name of the Financial Account.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the public name of the FinancialAccount.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string PublicName
        {
            get
            {
                if ( string.IsNullOrWhiteSpace( _publicName ) )
                {
                    return this.Name;
                }
                else
                {
                    return _publicName;
                }
            }

            set
            {
                _publicName = value;
            }
        }

        private string _publicName = string.Empty;

        /// <summary>
        /// Gets or sets the user defined description of the FinancialAccount.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined description of the FinancialAccount.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the user defined public description of the FinancialAccount.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined public description of the FinancialAccount.
        /// </value>
        [DataMember]
        public string PublicDescription { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if transactions posted to this FinancialAccount are tax-deductible.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if transactions posted to this FinancialAccount are tax-deductible; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsTaxDeductible { get; set; }

        /// <summary>
        /// Gets or sets the General Ledger account code for this FinancialAccount.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the General Ledger account code for this FinancialAccount.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string GlCode { get; set; }

        /// <summary>
        /// Gets or sets the sort and display order of the FinancialAccount.  This is an ascending order, so the lower the value the higher the sort priority.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the sort order of the FinancialAccount.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this FinancialAccount is active.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> that is <c>true</c> if this FinancialAccount is active, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating if this FinancialAccount is public.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> that is <c>true</c> if this FinancialAccount is public, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsPublic
        {
            get { return _isPublic; }
            set { _isPublic = value; }
        }
        private bool? _isPublic = true;

        /// <summary>
        /// Gets or sets the opening date for this FinancialAccount. This is the first date that transactions can be posted to this account. 
        /// If there isn't a start date for this account, transactions can be posted as soon as the account is created until the <see cref="EndDate"/> (if applicable).
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the first day that transactions can posted to this account. If there is no start date, this property will be null.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the closing/end date for this FinancialAccount. This is the last day that transactions can be posted to this account. If there is not a end date
        /// for this account, transactions can be posted for an indefinite period of time.  Ongoing FinancialAccounts will not have an end date.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the closing/end date for this FinancialAccounts. Transactions can be posted to this account until this date.  If this is 
        /// an ongoing account, this property will be null.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the <see cref="Rock.Model.DefinedValue"/> that represents the FinancialAccountType for this FinancialAccount.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing DefinedValueId of the FinancialAccountType's <see cref="Rock.Model.DefinedValue"/> for this FinancialAccount.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_ACCOUNT_TYPE )]
        public int? AccountTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the Image Id that can be used when displaying this Financial Account
        /// </summary>
        /// <value>
        /// The image binary file identifier.
        /// </value>
        [DataMember]
        public int? ImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the URL which could be used to generate a link to a 'More Info' page
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// <para>
        /// Determines if this account will use child account matching logic
        /// using a campus. When <c>true</c>, a supported block will be able
        /// to automatically determine the child account to use based on the campus.
        /// </para>
        /// <para>
        /// When possible, use the TODO: <see cref="FinancialAccountCache.GetMappedAccountForCampus(CampusCache)"/>
        /// method to perform the matching logic.
        /// </para>
        /// <para>
        /// <list type="bullet">
        ///   <item>If no campus is specified or available, then this (the parent) account will be used.</item>
        ///   <item>If an active direct child account has a campus that matches the specified campus, then the first matching child account will be used.</item>
        ///   <item>If no active direct child account matches the specified campus, then this (the parent) account will be used.</item>
        /// </list>
        /// </para>
        /// <remark>
        /// If this value is <c>true</c> then it implies that the <see cref="CampusId"/>
        /// on this account is not supported and should be ignored. The UI will
        /// ensure that both can't be set at the same time.
        /// </remark>
        /// </summary>
        [DataMember]
        public bool UsesCampusChildAccounts { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the parent FinancialAccount.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the parent FinancialAccount.
        /// </value>
        [LavaVisible]
        public virtual FinancialAccount ParentAccount { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that this FinancialAccount is associated with.
        /// </summary>
        /// <value>
        /// the <see cref="Rock.Model.Campus"/> that this FinancialAccount is associated with.
        /// </value>
        [LavaVisible]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the Account Type <see cref="Rock.Model.DefinedValue"/> for this FinancialAccount.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DefinedValue"/> that represents the AccountType for this FinancialAccount.
        /// </value>
        [DataMember]
        public virtual DefinedValue AccountTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the Image that can be used when displaying this Financial Account
        /// </summary>
        /// <value>
        /// The image binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile ImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the FinancialAccounts that are sub accounts/child accounts of this account.  This is not a recursive search.
        /// </summary>
        /// <value>
        /// A collection containing all FinancialAccounts that are sub accounts/child accounts of this account.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialAccount> ChildAccounts
        {
            get { return _childAccounts ?? ( _childAccounts = new Collection<FinancialAccount>() ); }
            set { _childAccounts = value; }
        }
        private ICollection<FinancialAccount> _childAccounts;

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// FinancialAccount Configuration class.
    /// </summary>
    public partial class FinancialAccountConfiguration : EntityTypeConfiguration<FinancialAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialAccountConfiguration"/> class.
        /// </summary>
        public FinancialAccountConfiguration()
        {
            this.HasOptional( a => a.ParentAccount ).WithMany( a => a.ChildAccounts ).HasForeignKey( a => a.ParentAccountId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Campus ).WithMany().HasForeignKey( a => a.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.AccountTypeValue ).WithMany().HasForeignKey( a => a.AccountTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.ImageBinaryFile ).WithMany().HasForeignKey( a => a.ImageBinaryFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}