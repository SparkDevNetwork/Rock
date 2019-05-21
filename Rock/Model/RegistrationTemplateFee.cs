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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Web.UI.Controls;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "RegistrationTemplateFee" )]
    [DataContract]
    public partial class RegistrationTemplateFee : Model<RegistrationTemplateFee>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        [DataMember]
        public int RegistrationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the fee type ( single option vs multiple options ).
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [DataMember]
        public RegistrationFeeType FeeType { get; set; }

        /// <summary>
        /// Gets or sets the cost(s) of the fee. Value is stored like: single = 20, multiple = L|20,XL|20,XXL|25 or Small^10|Medium^20|Large^30|XXL^40
        /// </summary>
        /// <value>
        /// The discount amount.
        /// </value>
        [MaxLength( 400 )]
        [DataMember]
        [RockObsolete( "1.9" )]
        [Obsolete( "Use FeeItems instead" )]
        public string CostValue { get; set; }

        /// <summary>
        /// Discount codes apply to this fee
        /// </summary>
        /// <value>
        /// The discount percentage.
        /// </value>
        [DataMember]
        public bool DiscountApplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if registrant can select multiple values for this fee.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow multiple]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowMultiple { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRequired { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration template.
        /// </summary>
        /// <value>
        /// The registration template.
        /// </value>
        [LavaInclude]
        public virtual RegistrationTemplate RegistrationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the fee items.
        /// </summary>
        /// <value>
        /// The fee items.
        /// </value>
        [LavaInclude]
        [DataMember]
        public virtual ICollection<RegistrationTemplateFeeItem> FeeItems { get; set; } = new List<RegistrationTemplateFeeItem>();

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Builds the single option single quantity checkbox
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="feeValues">The fee values.</param>
        /// <returns></returns>
        private Control GetFeeSingleOptionSingleQuantityControl( bool setValues, List<FeeInfo> feeValues )
        {
            var fee = this;
            var cb = new RockCheckBox();
            cb.ID = "fee_" + fee.Id.ToString();


            cb.SelectedIconCssClass = "fa fa-check-square-o fa-lg";
            cb.UnSelectedIconCssClass = "fa fa-square-o fa-lg";
            cb.Required = fee.IsRequired;

            if ( fee.IsRequired )
            {
                cb.Checked = true;
                cb.Enabled = false;
            }
            else if ( setValues && feeValues != null && feeValues.Any() )
            {
                cb.Checked = feeValues.First().Quantity > 0;
            }

            return cb;
        }

        /// <summary>
        /// Gets the single option multiple quantity.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="feeValues">The fee values.</param>
        /// <param name="usageCountRemaining">The usage count remaining.</param>
        /// <returns></returns>
        private Control GetFeeSingleOptionMultipleQuantityControl( bool setValues, List<FeeInfo> feeValues, int? usageCountRemaining )
        {
            var fee = this;
            var numUpDown = new NumberUpDown();
            numUpDown.ID = "fee_" + fee.Id.ToString();
            numUpDown.Minimum = fee.IsRequired == true ? 1 : 0;
            numUpDown.Required = fee.IsRequired;

            if ( usageCountRemaining.HasValue )
            {
                if ( usageCountRemaining <= 0 )
                {
                    numUpDown.Enabled = false;
                    numUpDown.Maximum = 0;
                }
                else
                {
                    numUpDown.Maximum = usageCountRemaining.Value;
                }
            }

            if ( setValues && feeValues != null && feeValues.Any() )
            {
                numUpDown.Value = feeValues.First().Quantity;
            }

            return numUpDown;
        }

        /// <summary>
        /// Gets the multiple option single quantity fee control
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="feeValues">The fee values.</param>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="otherRegistrants">The other registrants that have been registered so far in this registration</param>
        /// <returns></returns>
        private Control GetFeeMultipleOptionSingleQuantityControl( bool setValues, List<FeeInfo> feeValues, RegistrationInstance registrationInstance, List<RegistrantInfo> otherRegistrants )
        {
            var fee = this;
            var ddl = new RockDropDownList();
            ddl.ID = "fee_" + fee.Id.ToString();
            ddl.AddCssClass( "input-width-md" );
            ddl.Label = fee.Name;
            ddl.DataValueField = "Key";
            ddl.DataTextField = "Value";
            ddl.Required = fee.IsRequired;

            ddl.Items.Clear();
            ddl.Items.Add( new ListItem() );
            foreach ( var feeItem in fee.FeeItems )
            {
                int? usageCountRemaining = feeItem.GetUsageCountRemaining( registrationInstance, otherRegistrants );
                var listItem = new ListItem( string.Format( "{0} ({1})", feeItem.Name, feeItem.Cost.FormatAsCurrency() ), feeItem.Id.ToString() );
                if ( usageCountRemaining.HasValue )
                {
                    if ( usageCountRemaining <= 0 )
                    {
                        listItem.Enabled = false;
                        listItem.Text += " (none remaining)";
                    }
                    else
                    {
                        listItem.Text += $" ({usageCountRemaining} remaining)";
                    }
                }

                ddl.Items.Add( listItem );
            }

            if ( setValues && feeValues != null && feeValues.Any() )
            {
                var defaultFeeItemId = feeValues.Where( f => f.Quantity > 0 ).Select( f => f.RegistrationTemplateFeeItemId ).FirstOrDefault();

                ddl.SetValue( defaultFeeItemId );
            }

            return ddl;
        }

        /// <summary>
        /// Gets the multiple option multiple quantity numberupdowngroup control
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="feeValues">The fee values.</param>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="otherRegistrants">The other registrants that have been registered so far in this registration</param>
        /// <returns></returns>
        private Control GetFeeMultipleOptionMultipleQuantityControl( bool setValues, List<FeeInfo> feeValues, RegistrationInstance registrationInstance, List<RegistrantInfo> otherRegistrants )
        {
            var fee = this;
            var numberUpDownGroup = new NumberUpDownGroup();
            numberUpDownGroup.ID = "fee_" + fee.Id.ToString();
            numberUpDownGroup.Label = fee.Name;
            numberUpDownGroup.Required = fee.IsRequired;
            numberUpDownGroup.Controls.Clear();

            foreach ( var feeItem in fee.FeeItems )
            {
                var numUpDown = new NumberUpDown
                {
                    ID = $"feeItem_{feeItem.Guid.ToString( "N" )}",
                    Label = string.Format( "{0} ({1})", feeItem.Name, feeItem.Cost.FormatAsCurrency() ),
                    Minimum = 0
                };

                int? usageCountRemaining = feeItem.GetUsageCountRemaining( registrationInstance, otherRegistrants );

                if ( usageCountRemaining.HasValue )
                {
                    if ( usageCountRemaining <= 0 )
                    {
                        numUpDown.Enabled = false;
                        numUpDown.Label += " (none available)";
                        numUpDown.Maximum = 0;
                    }
                    else
                    {
                        numUpDown.Label += $" ({usageCountRemaining} available)";
                        numUpDown.Maximum = usageCountRemaining.Value;
                    }
                }

                numberUpDownGroup.Controls.Add( numUpDown );

                if ( setValues && feeValues != null && feeValues.Any() )
                {
                    numUpDown.Value = feeValues
                        .Where( f => f.RegistrationTemplateFeeItemId == feeItem.Id )
                        .Select( f => f.Quantity )
                        .FirstOrDefault();
                }
            }

            return numberUpDownGroup;
        }

        /// <summary>
        /// Adds the fee control with a null for otherRegistrants.
        /// </summary>
        /// <param name="phFees">The ph fees.</param>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="feeValues">The fee values.</param>
        [RockObsolete("1.8")]
        [Obsolete("Use the override that has otherRegistrants instead.")]
        public void AddFeeControl( PlaceHolder phFees, RegistrationInstance registrationInstance, bool setValues, List<FeeInfo> feeValues )
        {
            AddFeeControl( phFees, registrationInstance, setValues, feeValues, null );
        }

        /// <summary>
        /// Adds the fee control.
        /// </summary>
        /// <param name="phFees">The ph fees.</param>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="feeValues">The fee values.</param>
        /// <param name="otherRegistrants">The other registrants that have been registered so far in this registration. Set to NULL if editing a single registrant.</param>
        public void AddFeeControl( PlaceHolder phFees, RegistrationInstance registrationInstance, bool setValues, List<FeeInfo> feeValues, List<RegistrantInfo> otherRegistrants )
        {
            RegistrationTemplateFee fee = this;
            Control feeControl = null;
            if ( fee.FeeType == RegistrationFeeType.Single )
            {
                var feeItem = fee.FeeItems.FirstOrDefault();
                if ( feeItem != null )
                {
                    int? usageCountRemaining = feeItem.GetUsageCountRemaining( registrationInstance, otherRegistrants );

                    if ( fee.AllowMultiple )
                    {
                        feeControl = GetFeeSingleOptionMultipleQuantityControl( setValues, feeValues, usageCountRemaining );
                    }
                    else
                    {
                        feeControl = GetFeeSingleOptionSingleQuantityControl( setValues, feeValues );
                    }

                    if ( feeControl is IRockControl rockControl )
                    {
                        if ( feeItem.Cost != 0.0M )
                        {
                            rockControl.Label = $"{feeItem.Name} ({feeItem.Cost.FormatAsCurrency()})";
                        }
                        else
                        {
                            rockControl.Label = feeItem.Name;
                        }

                        if ( usageCountRemaining.HasValue )
                        {
                            if ( usageCountRemaining <= 0 )
                            {
                                rockControl.Label += " (none available)";
                                if ( rockControl is WebControl rockWebControl )
                                {
                                    rockWebControl.Enabled = false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if ( fee.AllowMultiple )
                {
                    feeControl = GetFeeMultipleOptionMultipleQuantityControl( setValues, feeValues, registrationInstance, otherRegistrants );
                }
                else
                {
                    feeControl = GetFeeMultipleOptionSingleQuantityControl( setValues, feeValues, registrationInstance, otherRegistrants );
                }
            }

            if ( feeControl != null )
            {
                if ( feeControl is IHasValidationGroup hasValidationGroup )
                {
                    hasValidationGroup.ValidationGroup = phFees.RockBlock()?.BlockValidationGroup;
                }

                phFees.Controls.Add( feeControl );
            }
        }


        /// <summary>
        /// Gets the fee information from controls.
        /// </summary>
        /// <param name="phFees">The ph fees.</param>
        /// <returns></returns>
        public List<FeeInfo> GetFeeInfoFromControls( PlaceHolder phFees )
        {
            RegistrationTemplateFee fee = this;

            string fieldId = string.Format( "fee_{0}", fee.Id );

            if ( fee.FeeType == RegistrationFeeType.Single )
            {
                var singleFeeItem = fee.FeeItems.FirstOrDefault();
                if ( fee.AllowMultiple )
                {
                    // Single Option, Multi Quantity
                    var numUpDown = phFees.FindControl( fieldId ) as NumberUpDown;
                    if ( numUpDown != null && numUpDown.Value > 0 )
                    {
                        return new List<FeeInfo> { new FeeInfo( singleFeeItem, numUpDown.Value, singleFeeItem.Cost ) };
                    }
                }
                else
                {
                    // Single Option, Single Quantity
                    var cb = phFees.FindControl( fieldId ) as RockCheckBox;
                    if ( cb != null && cb.Checked )
                    {
                        return new List<FeeInfo> { new FeeInfo( singleFeeItem, 1, singleFeeItem.Cost ) };
                    }
                }
            }
            else
            {
                if ( fee.AllowMultiple )
                {
                    // Multi Option, Multi Quantity
                    var result = new List<FeeInfo>();

                    foreach ( var feeItem in fee.FeeItems )
                    {
                        string optionFieldId = $"feeItem_{feeItem.Guid.ToString( "N" )}";
                        var numUpDownGroups = phFees.ControlsOfTypeRecursive<NumberUpDownGroup>();

                        foreach ( NumberUpDownGroup numberUpDownGroup in numUpDownGroups )
                        {
                            foreach ( NumberUpDown numberUpDown in numberUpDownGroup.ControlGroup )
                            {
                                if ( numberUpDown.ID == optionFieldId && numberUpDown.Value > 0 )
                                {
                                    result.Add( new FeeInfo( feeItem, numberUpDown.Value, feeItem.Cost ) );
                                }
                            }
                        }
                    }

                    if ( result.Any() )
                    {
                        return result;
                    }
                }
                else
                {
                    // Multi Option, Single Quantity
                    var ddl = phFees.FindControl( fieldId ) as RockDropDownList;
                    if ( ddl != null && ddl.SelectedValue != string.Empty )
                    {

                        var feeItemId = ddl.SelectedValue.AsInteger();
                        var feeItem = fee.FeeItems.FirstOrDefault( a => a.Id == feeItemId );
                        if ( feeItem != null )
                        {
                            return new List<FeeInfo> { new FeeInfo( feeItem, 1, feeItem.Cost ) };
                        }
                    }
                }
            }

            return null;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationTemplateFeeConfiguration : EntityTypeConfiguration<RegistrationTemplateFee>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationTemplateFeeConfiguration"/> class.
        /// </summary>
        public RegistrationTemplateFeeConfiguration()
        {
            this.HasRequired( f => f.RegistrationTemplate ).WithMany( t => t.Fees ).HasForeignKey( f => f.RegistrationTemplateId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Flag for how fee items should be displayed/required by user
    /// </summary>
    public enum RegistrationFeeType
    {
        /// <summary>
        /// There is one only one option for this fee
        /// </summary>
        Single = 0,

        /// <summary>
        /// There are multiple options available for this fee
        /// </summary>
        Multiple = 1,
    }

    #endregion
}
