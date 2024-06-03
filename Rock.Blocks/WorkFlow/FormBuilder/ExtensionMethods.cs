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

using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.WorkFlow.FormBuilder;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Various extension methods helpful when working with FormBuilder. Primarily
    /// this contains the methods to convert to/from view models.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Creates a view model representation of a
        /// <see cref="Rock.Workflow.FormBuilder.FormPersonEntrySettings"/> object.
        /// </summary>
        /// <param name="settings">The object to be represented as a view model.</param>
        /// <returns>The view model representation.</returns>
        internal static FormPersonEntryViewModel ToViewModel( this Rock.Workflow.FormBuilder.FormPersonEntrySettings settings )
        {
            return new FormPersonEntryViewModel
            {
                Address = settings.Address.ToFormFieldVisibility(),
                AddressType = Utility.GetDefinedValueGuid( settings.AddressTypeValueId ),
                AutofillCurrentPerson = settings.AutofillCurrentPerson,
                Birthdate = settings.Birthdate.ToFormFieldVisibility(),
                CampusStatus = Utility.GetDefinedValueGuid( settings.CampusStatusValueId ),
                CampusType = Utility.GetDefinedValueGuid( settings.CampusTypeValueId ),
                ConnectionStatus = Utility.GetDefinedValueGuid( settings.ConnectionStatusValueId ),
                Email = settings.Email.ToFormFieldVisibility(),
                Gender = settings.Gender.ToFormFieldVisibility(),
                HideIfCurrentPersonKnown = settings.HideIfCurrentPersonKnown,
                MaritalStatus = settings.MaritalStatus.ToFormFieldVisibility(),
                MobilePhone = settings.MobilePhone.ToFormFieldVisibility(),
                RecordStatus = Utility.GetDefinedValueGuid( settings.RecordStatusValueId ),
                ShowCampus = settings.ShowCampus,
                SpouseEntry = settings.SpouseEntry.ToFormFieldVisibility(),
                SpouseLabel = settings.SpouseLabel,
                RaceEntry = settings.RaceEntry.ToFormFieldVisibility(),
                EthnicityEntry = settings.EthnicityEntry.ToFormFieldVisibility(),
            };
        }

        /// <summary>
        /// Creates a <see cref="Rock.Workflow.FormBuilder.FormPersonEntrySettings"/>
        /// object from its view model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the object.</param>
        /// <returns>The object created from the view model.</returns>
        internal static Rock.Workflow.FormBuilder.FormPersonEntrySettings FromViewModel( this FormPersonEntryViewModel viewModel )
        {
            return new Rock.Workflow.FormBuilder.FormPersonEntrySettings
            {
                Address = viewModel.Address.ToPersonEntryOption(),
                AddressTypeValueId = Utility.GetDefinedValueId( viewModel.AddressType ),
                AutofillCurrentPerson = viewModel.AutofillCurrentPerson,
                Birthdate = viewModel.Birthdate.ToPersonEntryOption(),
                CampusStatusValueId = Utility.GetDefinedValueId( viewModel.CampusStatus ),
                CampusTypeValueId = Utility.GetDefinedValueId( viewModel.CampusType ),
                ConnectionStatusValueId = Utility.GetDefinedValueId( viewModel.ConnectionStatus ),
                Email = viewModel.Email.ToPersonEntryOption(),
                Gender = viewModel.Gender.ToPersonEntryOption(),
                HideIfCurrentPersonKnown = viewModel.HideIfCurrentPersonKnown,
                MaritalStatus = viewModel.MaritalStatus.ToPersonEntryOption(),
                MobilePhone = viewModel.MobilePhone.ToPersonEntryOption(),
                RecordStatusValueId = Utility.GetDefinedValueId( viewModel.RecordStatus ),
                ShowCampus = viewModel.ShowCampus,
                SpouseEntry = viewModel.SpouseEntry.ToPersonEntryOption(),
                SpouseLabel = viewModel.SpouseLabel,
                RaceEntry = viewModel.RaceEntry.ToPersonEntryOption(),
                EthnicityEntry = viewModel.EthnicityEntry.ToPersonEntryOption(),
            };
        }

        /// <summary>
        /// Creates a view model representation of a
        /// <see cref="Rock.Workflow.FormBuilder.FormCompletionActionSettings"/> object.
        /// </summary>
        /// <param name="completionAction">The object to be represented as a view model.</param>
        /// <returns>The view model representation.</returns>
        internal static FormCompletionActionViewModel ToViewModel( this Rock.Workflow.FormBuilder.FormCompletionActionSettings completionAction )
        {
            return new FormCompletionActionViewModel
            {
                Type = completionAction?.Type == Rock.Workflow.FormBuilder.FormCompletionActionType.Redirect
                    ? FormCompletionActionType.Redirect
                    : FormCompletionActionType.DisplayMessage,
                Message = completionAction?.Message,
                RedirectUrl = completionAction?.RedirectUrl
            };
        }

        /// <summary>
        /// Creates a <see cref="Rock.Workflow.FormBuilder.FormCompletionActionSettings"/>
        /// object from its view model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the object.</param>
        /// <returns>The object created from the view model.</returns>
        internal static Rock.Workflow.FormBuilder.FormCompletionActionSettings FromViewModel( this FormCompletionActionViewModel viewModel )
        {
            return new Rock.Workflow.FormBuilder.FormCompletionActionSettings
            {
                Type = viewModel.Type == FormCompletionActionType.Redirect
                    ? Rock.Workflow.FormBuilder.FormCompletionActionType.Redirect
                    : Rock.Workflow.FormBuilder.FormCompletionActionType.DisplayMessage,
                Message = viewModel.Message,
                RedirectUrl = viewModel.RedirectUrl
            };
        }

        /// <summary>
        /// Creates a view model representation of a
        /// <see cref="Rock.Workflow.FormBuilder.FormConfirmationEmailSettings"/> object.
        /// </summary>
        /// <param name="emailSettings">The object to be represented as a view model.</param>
        /// <param name="rockContext">The database context used for data lookups.</param>
        /// <returns>The view model representation.</returns>
        internal static FormConfirmationEmailViewModel ToViewModel( this Rock.Workflow.FormBuilder.FormConfirmationEmailSettings emailSettings, RockContext rockContext )
        {
            if ( emailSettings == null || !emailSettings.Enabled )
            {
                return new FormConfirmationEmailViewModel();
            }

            return new FormConfirmationEmailViewModel
            {
                Enabled = emailSettings.Enabled,
                RecipientAttributeGuid = emailSettings.RecipientAttributeGuid,
                Source = ToViewModel( emailSettings.Source, rockContext )
            };
        }

        /// <summary>
        /// Creates a <see cref="Rock.Workflow.FormBuilder.FormConfirmationEmailSettings"/>
        /// object from its view model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the object.</param>
        /// <returns>The object created from the view model.</returns>
        internal static Rock.Workflow.FormBuilder.FormConfirmationEmailSettings FromViewModel( this FormConfirmationEmailViewModel viewModel, RockContext rockContext )
        {
            if ( viewModel == null || !viewModel.Enabled )
            {
                return new Rock.Workflow.FormBuilder.FormConfirmationEmailSettings();
            }

            return new Rock.Workflow.FormBuilder.FormConfirmationEmailSettings
            {
                Enabled = viewModel.Enabled,
                Destination = Rock.Workflow.FormBuilder.FormConfirmationEmailDestination.Custom,
                RecipientAttributeGuid = viewModel.RecipientAttributeGuid,
                Source = FromViewModel( viewModel.Source, rockContext )
            };
        }

        /// <summary>
        /// Creates a view model representation of a
        /// <see cref="Rock.Workflow.FormBuilder.FormNotificationEmailSettings"/> object.
        /// </summary>
        /// <param name="emailSettings">The object to be represented as a view model.</param>
        /// <param name="rockContext">The database context used for data lookups.</param>
        /// <returns>The view model representation.</returns>
        internal static FormNotificationEmailViewModel ToViewModel( this Rock.Workflow.FormBuilder.FormNotificationEmailSettings emailSettings, RockContext rockContext )
        {
            if ( emailSettings == null || !emailSettings.Enabled )
            {
                return new FormNotificationEmailViewModel();
            }

            ListItemBag recipient = null;

            if ( emailSettings.RecipientAliasId.HasValue )
            {
                var personAlias = new PersonAliasService( rockContext ).Queryable()
                    .Include( pa => pa.Person )
                    .Where( pa => pa.Id == emailSettings.RecipientAliasId.Value )
                    .FirstOrDefault();

                if ( personAlias != null )
                {
                    recipient = new ListItemBag
                    {
                        Value = personAlias.Guid.ToString(),
                        Text = personAlias.Person.FullName
                    };
                }
            }

            return new FormNotificationEmailViewModel
            {
                Enabled = emailSettings.Enabled,
                CampusTopicGuid = Utility.GetDefinedValueGuid( emailSettings.CampusTopicValueId ),
                Destination = ToViewModel( emailSettings.Destination ),
                EmailAddress = emailSettings.EmailAddress,
                Recipient = recipient,
                Source = ToViewModel( emailSettings.Source, rockContext )
            };
        }

        /// <summary>
        /// Creates a <see cref="Rock.Workflow.FormBuilder.FormNotificationEmailSettings"/>
        /// object from its view model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the object.</param>
        /// <returns>The object created from the view model.</returns>
        internal static Rock.Workflow.FormBuilder.FormNotificationEmailSettings FromViewModel( this FormNotificationEmailViewModel viewModel, RockContext rockContext )
        {
            if ( viewModel == null || !viewModel.Enabled )
            {
                return new Rock.Workflow.FormBuilder.FormNotificationEmailSettings();
            }

            int? recipientAliasId = null;

            if ( viewModel.Recipient != null && viewModel.Recipient.Value.AsGuidOrNull().HasValue )
            {
                recipientAliasId = new PersonAliasService( rockContext ).GetId( viewModel.Recipient.Value.AsGuid() );
            }

            return new Rock.Workflow.FormBuilder.FormNotificationEmailSettings
            {
                Enabled = viewModel.Enabled,
                CampusTopicValueId = Utility.GetDefinedValueId( viewModel.CampusTopicGuid ),
                Destination = FromViewModel( viewModel.Destination ),
                EmailAddress = viewModel.EmailAddress,
                RecipientAliasId = recipientAliasId,
                Source = FromViewModel( viewModel.Source, rockContext )
            };
        }

        /// <summary>
        /// Creates a view model representation of a
        /// <see cref="Rock.Workflow.FormBuilder.FormEmailSourceSettings"/> object.
        /// </summary>
        /// <param name="sourceSettings">The object to be represented as a view model.</param>
        /// <param name="rockContext">The database context used for data lookups.</param>
        /// <returns>The view model representation.</returns>
        internal static FormEmailSourceViewModel ToViewModel( this Rock.Workflow.FormBuilder.FormEmailSourceSettings sourceSettings, RockContext rockContext )
        {
            if ( sourceSettings == null )
            {
                return null;
            }

            Guid? templateGuid = null;

            if ( sourceSettings.SystemCommunicationId.HasValue )
            {
                templateGuid = new SystemCommunicationService( rockContext ).GetGuid( sourceSettings.SystemCommunicationId.Value );
            }

            return new FormEmailSourceViewModel
            {
                Type = sourceSettings.Type == Rock.Workflow.FormBuilder.FormEmailSourceType.UseTemplate ? FormEmailSourceType.UseTemplate : FormEmailSourceType.Custom,
                AppendOrgHeaderAndFooter = sourceSettings.AppendOrgHeaderAndFooter,
                Body = sourceSettings.Body,
                ReplyTo = sourceSettings.ReplyTo,
                Subject = sourceSettings.Subject,
                Template = templateGuid
            };
        }

        /// <summary>
        /// Creates a <see cref="Rock.Workflow.FormBuilder.FormEmailSourceSettings"/>
        /// object from its view model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the object.</param>
        /// <returns>The object created from the view model.</returns>
        internal static Rock.Workflow.FormBuilder.FormEmailSourceSettings FromViewModel( this FormEmailSourceViewModel viewModel, RockContext rockContext )
        {
            if ( viewModel == null )
            {
                return null;
            }

            int? templateId = null;

            if ( viewModel.Template.HasValue )
            {
                templateId = new SystemCommunicationService( rockContext ).GetId( viewModel.Template.Value );
            }

            return new Rock.Workflow.FormBuilder.FormEmailSourceSettings
            {
                Type = viewModel.Type == FormEmailSourceType.UseTemplate
                    ? Rock.Workflow.FormBuilder.FormEmailSourceType.UseTemplate
                    : Rock.Workflow.FormBuilder.FormEmailSourceType.Custom,
                AppendOrgHeaderAndFooter = viewModel.AppendOrgHeaderAndFooter,
                Body = viewModel.Body,
                ReplyTo = viewModel.ReplyTo,
                Subject = viewModel.Subject,
                SystemCommunicationId = templateId
            };
        }

        /// <summary>
        /// Creates a view model representation of a
        /// <see cref="Rock.Workflow.FormBuilder.FormNotificationEmailDestination"/> value.
        /// </summary>
        /// <param name="destination">The value to be represented as a view model.</param>
        /// <returns>The view model representation.</returns>
        internal static FormNotificationEmailDestination ToViewModel( this Rock.Workflow.FormBuilder.FormNotificationEmailDestination destination )
        {
            switch ( destination )
            {
                case Rock.Workflow.FormBuilder.FormNotificationEmailDestination.EmailAddress:
                    return FormNotificationEmailDestination.EmailAddress;

                case Rock.Workflow.FormBuilder.FormNotificationEmailDestination.CampusTopic:
                    return FormNotificationEmailDestination.CampusTopic;

                case Rock.Workflow.FormBuilder.FormNotificationEmailDestination.SpecificIndividual:
                default:
                    return FormNotificationEmailDestination.SpecificIndividual;
            }
        }

        /// <summary>
        /// Creates a <see cref="Rock.Workflow.FormBuilder.FormNotificationEmailDestination"/>
        /// value from its view model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the value.</param>
        /// <returns>The value created from the view model.</returns>
        internal static Rock.Workflow.FormBuilder.FormNotificationEmailDestination FromViewModel( this FormNotificationEmailDestination viewModel )
        {
            switch ( viewModel )
            {
                case FormNotificationEmailDestination.EmailAddress:
                    return Rock.Workflow.FormBuilder.FormNotificationEmailDestination.EmailAddress;

                case FormNotificationEmailDestination.CampusTopic:
                    return Rock.Workflow.FormBuilder.FormNotificationEmailDestination.CampusTopic;

                case FormNotificationEmailDestination.SpecificIndividual:
                default:
                    return Rock.Workflow.FormBuilder.FormNotificationEmailDestination.SpecificIndividual;
            }
        }

        /// <summary>
        /// Creates a <see cref="Rock.Workflow.FormBuilder.CampusSetFrom"/>
        /// value from its view model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the value.</param>
        /// <returns>The value created from the view model.</returns>
        internal static Rock.Workflow.FormBuilder.CampusSetFrom FromViewModel( this CampusSetFrom viewModel )
        {
            switch ( viewModel )
            {
                case CampusSetFrom.WorkflowPerson:
                    return Rock.Workflow.FormBuilder.CampusSetFrom.WorkflowPerson;

                case CampusSetFrom.QueryString:
                    return Rock.Workflow.FormBuilder.CampusSetFrom.QueryString;

                case CampusSetFrom.CurrentPerson:
                default:
                    return Rock.Workflow.FormBuilder.CampusSetFrom.CurrentPerson;
            }
        }

        /// <summary>
        /// Creates a view model representation of a
        /// <see cref="Rock.Workflow.FormBuilder.CampusSetFrom"/> value.
        /// </summary>
        /// <param name="value">The value to be represented as a view model.</param>
        /// <returns>The view model representation.</returns>
        internal static CampusSetFrom ToViewModel( this Rock.Workflow.FormBuilder.CampusSetFrom value )
        {
            switch ( value )
            {
                case Rock.Workflow.FormBuilder.CampusSetFrom.WorkflowPerson:
                    return CampusSetFrom.WorkflowPerson;

                case Rock.Workflow.FormBuilder.CampusSetFrom.QueryString:
                    return CampusSetFrom.QueryString;

                case Rock.Workflow.FormBuilder.CampusSetFrom.CurrentPerson:
                default:
                    return CampusSetFrom.CurrentPerson;
            }
        }

        /// <summary>
        /// Creates a view model representation of a
        /// <see cref="WorkflowActionFormPersonEntryOption"/> value.
        /// </summary>
        /// <param name="value">The value to be represented as a view model.</param>
        /// <returns>The view model representation.</returns>
        internal static FormFieldVisibility ToFormFieldVisibility( this WorkflowActionFormPersonEntryOption value )
        {
            switch ( value )
            {
                case WorkflowActionFormPersonEntryOption.Optional:
                    return FormFieldVisibility.Optional;

                case WorkflowActionFormPersonEntryOption.Required:
                    return FormFieldVisibility.Required;

                case WorkflowActionFormPersonEntryOption.Hidden:
                default:
                    return FormFieldVisibility.Hidden;
            }
        }

        /// <summary>
        /// Creates a <see cref="WorkflowActionFormPersonEntryOption"/>
        /// value from its view model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the value.</param>
        /// <returns>The value created from the view model.</returns>
        internal static WorkflowActionFormPersonEntryOption ToPersonEntryOption( this FormFieldVisibility viewModel )
        {
            switch ( viewModel )
            {
                case FormFieldVisibility.Optional:
                    return WorkflowActionFormPersonEntryOption.Optional;

                case FormFieldVisibility.Required:
                    return WorkflowActionFormPersonEntryOption.Required;

                case FormFieldVisibility.Hidden:
                default:
                    return WorkflowActionFormPersonEntryOption.Hidden;
            }
        }

        internal static FormFieldShowHide ToFormFieldShowHide( this WorkflowActionFormShowHideOption value )
        {
            switch ( value )
            {
                case WorkflowActionFormShowHideOption.Show:
                    return FormFieldShowHide.Show;

                case WorkflowActionFormShowHideOption.Hide:
                default:
                    return FormFieldShowHide.Hide;
            }
        }

        internal static WorkflowActionFormShowHideOption ToShowHideOption( this FormFieldShowHide viewModel )
        {
            switch( viewModel )
            {
                case FormFieldShowHide.Show:
                    return WorkflowActionFormShowHideOption.Show;

                case FormFieldShowHide.Hide:
                default:
                    return WorkflowActionFormShowHideOption.Hide;
            }
        }

        /// <summary>
        /// Creates a view model representation of a <see cref="Rock.Field.FieldVisibilityRules"/> object.
        /// </summary>
        /// <param name="rules">The object to be represented as a view model.</param>
        /// <returns>The view model representation.</returns>
        internal static FieldFilterGroupBag ToViewModel( this Rock.Field.FieldVisibilityRules rules )
        {
            return new FieldFilterGroupBag
            {
                Guid = Guid.NewGuid(),
                ExpressionType = rules.FilterExpressionType,
                Rules = rules.RuleList.Select( r => r.ToViewModel() ).ToList()
            };
        }

        /// <summary>
        /// Creates a <see cref="Rock.Field.FieldVisibilityRules"/>
        /// object from its view model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the object.</param>
        /// <returns>The object created from the view model.</returns>
        internal static Rock.Field.FieldVisibilityRules FromViewModel( this FieldFilterGroupBag viewModel, List<FormFieldViewModel> formFields )
        {
            return new Rock.Field.FieldVisibilityRules
            {
                FilterExpressionType = viewModel.ExpressionType,
                RuleList = viewModel.Rules.Select( r => r.FromViewModel( formFields ) ).ToList()
            };
        }

        /// <summary>
        /// Creates a view model representation of a <see cref="Rock.Field.FieldVisibilityRule"/> object.
        /// </summary>
        /// <param name="rule">The object to be represented as a view model.</param>
        /// <returns>The view model representation.</returns>
        internal static FieldFilterRuleBag ToViewModel( this Rock.Field.FieldVisibilityRule rule )
        {
            var viewModel = new FieldFilterRuleBag
            {
                Guid = rule.Guid,
                ComparisonType = rule.ComparisonType,
                SourceType = 0,
                AttributeGuid = rule.ComparedToFormFieldGuid,
                Value = rule.ComparedToValue
            };

            if ( rule.ComparedToFormFieldGuid.HasValue )
            {
                var attribute = AttributeCache.Get( rule.ComparedToFormFieldGuid.Value );

                if ( attribute?.FieldType?.Field != null)
                {
                    var filterValues = new List<string> { rule.ComparisonType.ConvertToInt().ToString(), rule.ComparedToValue };
                    var comparisonValue = attribute.FieldType.Field.GetPublicFilterValue( filterValues.ToJson(), attribute.ConfigurationValues );
                    viewModel.Value = comparisonValue.Value;
                }
            }

            return viewModel;
        }

        /// <summary>
        /// Creates a <see cref="Rock.Field.FieldVisibilityRule"/>
        /// object from its view model representation.
        /// </summary>
        /// <param name="viewModel">The view model that represents the object.</param>
        /// <returns>The object created from the view model.</returns>
        internal static Rock.Field.FieldVisibilityRule FromViewModel( this FieldFilterRuleBag viewModel, List<FormFieldViewModel> formFields )
        {
            var rule = new Rock.Field.FieldVisibilityRule
            {
                Guid = viewModel.Guid,
                ComparisonType = ( ComparisonType ) viewModel.ComparisonType,
                ComparedToFormFieldGuid = viewModel.AttributeGuid,
                ComparedToValue = viewModel.Value
            };

            if ( rule.ComparedToFormFieldGuid.HasValue )
            {
                var comparisonValue = new Rock.Reporting.ComparisonValue
                {
                    ComparisonType = rule.ComparisonType,
                    Value = rule.ComparedToValue
                };
                var field = formFields.Where( f => f.Guid == rule.ComparedToFormFieldGuid.Value ).FirstOrDefault();

                if ( field != null )
                {
                    var fieldType = FieldTypeCache.Get( field.FieldTypeGuid );

                    if ( fieldType?.Field != null )
                    {
                        var privateConfigurationValues = fieldType.Field.GetPrivateConfigurationValues( field.ConfigurationValues );
                        var filterValues = fieldType.Field.GetPrivateFilterValue( comparisonValue, privateConfigurationValues ).FromJsonOrNull<List<string>>();

                        if ( filterValues != null && filterValues.Count == 2 )
                        {
                            rule.ComparedToValue = filterValues[1];
                        }
                        else if ( filterValues != null && filterValues.Count == 1 )
                        {
                            rule.ComparedToValue = filterValues[0];
                        }
                    }
                }
                else
                {
                    var attribute = AttributeCache.Get( rule.ComparedToFormFieldGuid.Value );

                    if ( attribute?.FieldType?.Field != null )
                    {
                        var filterValues = attribute.FieldType.Field.GetPrivateFilterValue( comparisonValue, attribute.ConfigurationValues ).FromJsonOrNull<List<string>>();

                        if ( filterValues != null && filterValues.Count == 2 )
                        {
                            rule.ComparedToValue = filterValues[1];
                        }
                        else if ( filterValues != null && filterValues.Count == 1 )
                        {
                            rule.ComparedToValue = filterValues[0];
                        }
                    }
                }
            }

            return rule;
        }
    }
}
