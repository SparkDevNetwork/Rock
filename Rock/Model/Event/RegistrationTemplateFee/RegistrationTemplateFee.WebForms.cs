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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Web.UI.Controls;

namespace Rock.Model
{
    public partial class RegistrationTemplateFee
    {
        /// <summary>
        /// Builds the single option single quantity checkbox
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="controlLabel">The control label.</param>
        /// <param name="feeValues">The fee values.</param>
        /// <param name="usageCountRemaining">The usage count remaining.</param>
        /// <returns></returns>
        private Control GetFeeSingleOptionSingleQuantityControl( bool setValues, string controlLabel, List<FeeInfo> feeValues, int? usageCountRemaining )
        {
            var fee = this;
            var cb = new RockCheckBox
            {
                ID = "fee_" + fee.Id.ToString(),
                SelectedIconCssClass = "fa fa-check-square-o fa-lg",
                UnSelectedIconCssClass = "fa fa-square-o fa-lg",
                Required = fee.IsRequired,
                Label = controlLabel
            };

            var currentValue = feeValues?.FirstOrDefault()?.Quantity ?? 0;

            if ( fee.IsRequired && ( !usageCountRemaining.HasValue || usageCountRemaining > 0 ) )
            {
                cb.Checked = true;
                cb.Enabled = false;
            }
            else
            {
                if ( usageCountRemaining <= 0 )
                {
                    cb.Label += " (none remaining)";

                    // if there aren't any remaining, and the currentValue isn't counted in the used counts, disable the option
                    if ( currentValue == 0 )
                    {
                        // Unless this should be hidden, then set to null so it isn't added.
                        if ( HideWhenNoneRemaining == true )
                        {
                            cb = null;
                        }
                        else
                        {
                            cb.Enabled = false;
                            cb.FormGroupCssClass = "none-remaining text-muted disabled";
                        }
                    }
                }

                if ( cb != null && setValues )
                {
                    cb.Checked = currentValue > 0;
                }
            }

            return cb;
        }

        /// <summary>
        /// Gets the single option multiple quantity.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="controlLabel">The control label.</param>
        /// <param name="feeValues">The fee values.</param>
        /// <param name="usageCountRemaining">The usage count remaining.</param>
        /// <returns></returns>
        private Control GetFeeSingleOptionMultipleQuantityControl( bool setValues, string controlLabel, List<FeeInfo> feeValues, int? usageCountRemaining )
        {
            var fee = this;
            var numUpDown = new NumberUpDown
            {
                ID = "fee_" + fee.Id.ToString(),
                Minimum = fee.IsRequired == true ? 1 : 0,
                Required = fee.IsRequired,
                Label = controlLabel
            };

            var currentValue = feeValues?.FirstOrDefault()?.Quantity ?? 0;

            if ( usageCountRemaining.HasValue )
            {
                if ( usageCountRemaining <= 0 )
                {
                    numUpDown.Label += " (none remaining)";
                    numUpDown.Maximum = currentValue;

                    // if there aren't any remaining, and the currentValue isn't counted in the used counts, disable the option
                    if ( currentValue == 0 )
                    {
                        // Unless this should be hidden, then set to null so it isn't added.
                        if ( HideWhenNoneRemaining == true )
                        {
                            numUpDown = null;
                        }
                        else
                        {
                            numUpDown.Enabled = false;
                        }
                    }
                }
                else
                {
                    numUpDown.Label += $" ({usageCountRemaining} remaining)";
                    numUpDown.Maximum = usageCountRemaining.Value;
                }
            }

            if ( numUpDown != null && setValues && feeValues != null && feeValues.Any() )
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
            var ddl = new RockDropDownList
            {
                ID = "fee_" + fee.Id.ToString(),
                Label = fee.Name,
                DataValueField = "Key",
                DataTextField = "Value",
                Required = fee.IsRequired
            };

            ddl.AddCssClass( "input-width-md" );
            ddl.Items.Clear();
            ddl.Items.Add( new ListItem() );

            foreach ( var feeItem in fee.FeeItems )
            {
                var feeInfo = feeValues?.FirstOrDefault( a => a.RegistrationTemplateFeeItemId == feeItem.Id );
                int currentValue = feeInfo?.Quantity ?? 0;

                string listItemText = feeItem.Cost == 0.0M ? feeItem.Name : $"{feeItem.Name} ({feeItem.Cost.FormatAsCurrency()})";
                var listItem = new ListItem( listItemText, feeItem.Id.ToString() );

                int? usageCountRemaining = feeItem.GetUsageCountRemaining( registrationInstance, otherRegistrants );
                if ( usageCountRemaining.HasValue )
                {
                    if ( usageCountRemaining <= 0 )
                    {
                        listItem.Text += " (none remaining)";

                        // if there aren't any remaining, and the currentValue isn't counted in the used counts, disable the option
                        if ( currentValue == 0 )
                        {
                            // Unless this should be hidden, then set to null so it isn't added.
                            if ( HideWhenNoneRemaining == true )
                            {
                                listItem = null;
                            }
                            else
                            {
                                listItem.Enabled = false;

                            }
                        }
                    }
                    else
                    {
                        listItem.Text += $" ({usageCountRemaining} remaining)";
                    }
                }

                if ( listItem != null )
                {
                    ddl.Items.Add( listItem );
                }
            }

            // The first item is blank. If there are no other items then return null, this will prevent the control from showing and won't count as a control when deciding to show the fee div.
            if ( ddl.Items.Count == 1 )
            {
                return null;
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
            var numberUpDownGroup = new NumberUpDownGroup
            {
                ID = "fee_" + fee.Id.ToString(),
                Label = fee.Name,
                Required = fee.IsRequired,
                NumberUpDownControls = new List<NumberUpDown>()
            };

            foreach ( var feeItem in fee.FeeItems )
            {
                var feeInfo = feeValues?.FirstOrDefault( a => a.RegistrationTemplateFeeItemId == feeItem.Id );
                int currentValue = feeInfo?.Quantity ?? 0;

                string controlLabel = feeItem.Cost == 0.0M ? feeItem.Name : $"{feeItem.Name} ({feeItem.Cost.FormatAsCurrency()})";

                var numUpDown = new NumberUpDown
                {
                    ID = $"feeItem_{feeItem.Guid.ToString( "N" )}",
                    Label = controlLabel,
                    Minimum = 0
                };

                int? usageCountRemaining = feeItem.GetUsageCountRemaining( registrationInstance, otherRegistrants );

                if ( usageCountRemaining.HasValue )
                {
                    if ( usageCountRemaining <= 0 )
                    {
                        numUpDown.Label += " (none remaining)";
                        numUpDown.Maximum = currentValue;

                        // if there aren't any remaining, and the currentValue isn't counted in the used counts, disable the option
                        if ( currentValue == 0 )
                        {
                            // Unless this should be hidden, then set to null so it isn't added.
                            if ( HideWhenNoneRemaining == true )
                            {
                                numUpDown = null;
                            }
                            else
                            {
                                numUpDown.Enabled = false;
                            }
                        }
                    }
                    else
                    {
                        numUpDown.Label += $" ({usageCountRemaining} remaining)";
                        numUpDown.Maximum = usageCountRemaining.Value;
                    }
                }

                if ( numUpDown != null )
                {
                    numberUpDownGroup.NumberUpDownControls.Add( numUpDown );

                    if ( setValues && feeValues != null && feeValues.Any() )
                    {
                        numUpDown.Value = feeValues
                            .Where( f => f.RegistrationTemplateFeeItemId == feeItem.Id )
                            .Select( f => f.Quantity )
                            .FirstOrDefault();
                    }
                }
            }

            // If there are no items then return null, this will prevent the control from showing and won't count as a control when deciding to show the fee div.
            if ( !numberUpDownGroup.NumberUpDownControls.Any() )
            {
                return null;
            }

            return numberUpDownGroup;
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

                    string controlLabel = feeItem.Cost == 0.0M ? feeItem.Name : $"{feeItem.Name} ({feeItem.Cost.FormatAsCurrency()})";

                    if ( fee.AllowMultiple )
                    {
                        feeControl = GetFeeSingleOptionMultipleQuantityControl( setValues, controlLabel, feeValues, usageCountRemaining );
                    }
                    else
                    {
                        feeControl = GetFeeSingleOptionSingleQuantityControl( setValues, controlLabel, feeValues, usageCountRemaining );
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
                            foreach ( NumberUpDown numberUpDown in numberUpDownGroup.NumberUpDownControls )
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
    }
}
