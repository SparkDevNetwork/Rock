using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_newpointe.SHAPE
{
    /// <summary>
    /// Block for the homepage of the New Family Check-in System
    /// </summary>
    [DisplayName( "Assessment" )]
    [Category( "NewPointe -> SHAPE" )]
    [Description( "Shows the SHAPE Assessment." )]

    [LinkedPage( "DISC Assessment Page", "The page for the DISC Assessment.", true, "", "", 0, ATTRIBUTE_KEY__DISC_ASSESSMENT_PAGE )]
    [LinkedPage( "SHAPE Results Page", "The page for the SHAPE Results.", true, "", "", 1, ATTRIBUTE_KEY__SHAPE_RESULTS_PAGE )]

    [DefinedTypeField( "SHAPE Spiritual Gifts", "The defined type that holds the SHAPE Spiritual Gifts.", true, "", "", 2, ATTRIBUTE_KEY__SHAPE_SPIRITUAL_GIFTS )]
    [DefinedTypeField( "SHAPE Spiritual Gifts Questions", "The defined type that holds the SHAPE Spiritual Gifts Questions.", true, "", "", 3, ATTRIBUTE_KEY__SHAPE_SPIRITUAL_GIFTS_QUESTIONS )]
    [DefinedTypeField( "SHAPE Abilities", "The defined type that holds the SHAPE Abilities.", true, "", "", 4, ATTRIBUTE_KEY__SHAPE_ABILITIES )]
    [DefinedTypeField( "SHAPE Abilities Questions", "The defined type that holds the SHAPE Abilities Questions.", true, "", "", 5, ATTRIBUTE_KEY__SHAPE_ABILITIES_QUESTIONS )]
    [DefinedTypeField( "SHAPE Heart Categories", "The defined type that holds the SHAPE Heart Categories.", true, "", "", 6, ATTRIBUTE_KEY__SHAPE_HEART_CATEGORIES )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Default Record Status", "The record status to use when creating a new person", false, false, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING, "", 7, ATTRIBUTE_KEY__DEFAULT_RECORD_STATUS )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status", "The connection status to use when creating a new person", false, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT, "", 8, ATTRIBUTE_KEY__DEFAULT_CONNECTION_STATUS )]
    public partial class Assessment : Rock.Web.UI.RockBlock
    {

        // Block Attribute Keys

        private const string ATTRIBUTE_KEY__DISC_ASSESSMENT_PAGE = "DISCAssessmentPage";
        private const string ATTRIBUTE_KEY__SHAPE_RESULTS_PAGE = "SHAPEResultsPage";

        private const string ATTRIBUTE_KEY__SHAPE_SPIRITUAL_GIFTS = "SHAPESpiritualGifts";
        private const string ATTRIBUTE_KEY__SHAPE_SPIRITUAL_GIFTS_QUESTIONS = "SHAPESpiritualGiftsQuestions";
        private const string ATTRIBUTE_KEY__SHAPE_ABILITIES = "SHAPEAbilities";
        private const string ATTRIBUTE_KEY__SHAPE_ABILITIES_QUESTIONS = "SHAPEAbilitiesQuestions";
        private const string ATTRIBUTE_KEY__SHAPE_HEART_CATEGORIES = "SHAPEHeartCategories";

        private const string ATTRIBUTE_KEY__DEFAULT_RECORD_STATUS = "DefaultRecordStatus";
        private const string ATTRIBUTE_KEY__DEFAULT_CONNECTION_STATUS = "DefaultConnectionStatus";

        // Defined Type Attribute Keys

        private const string ATTRIBUTE_KEY__SPIRITUAL_GIFT = "SpiritualGift";
        private const string ATTRIBUTE_KEY__ABILITY = "Ability";

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if (!Page.IsPostBack)
            {
                BindControls();
            }
        }

        protected void BindControls()
        {

            if (CurrentPerson != null)
            {
                dtbFirstName.Text = CurrentPerson.FirstName;
                dtbLastName.Text = CurrentPerson.LastName;
                ebEmailAddress.Text = CurrentPerson.Email;
            }

            var spiritualGiftsQuestions = DefinedTypeCache.Get( GetAttributeValue( ATTRIBUTE_KEY__SHAPE_SPIRITUAL_GIFTS_QUESTIONS ).AsGuid() );
            if (spiritualGiftsQuestions != null)
            {
                pShapeGifts.Visible = true;
                rSpiritualGiftsQuestions.DataSource = Rock.Lava.RockFilters.Shuffle( spiritualGiftsQuestions.DefinedValues );
                rSpiritualGiftsQuestions.DataBind();
            }
            else
            {
                pShapeGifts.Visible = false;
            }

            var abilitiesQuestions = DefinedTypeCache.Get( GetAttributeValue( ATTRIBUTE_KEY__SHAPE_ABILITIES_QUESTIONS ).AsGuid() );
            if (abilitiesQuestions != null)
            {
                pShapeAbilities.Visible = true;
                rAbilitiesQuestions.DataSource = abilitiesQuestions.DefinedValues;
                rAbilitiesQuestions.DataBind();
            }
            else
            {
                pShapeAbilities.Visible = false;
            }

            var heartCategories = DefinedTypeCache.Get( GetAttributeValue( ATTRIBUTE_KEY__SHAPE_HEART_CATEGORIES ).AsGuid() );
            if (heartCategories != null)
            {
                pShapeHeart.Visible = true;
                rcblHeartOptions.DataSource = heartCategories.DefinedValues;
                rcblHeartOptions.DataTextField = "Value";
                rcblHeartOptions.DataValueField = "Guid";
                rcblHeartOptions.DataBind();
            }
            else
            {
                pShapeHeart.Visible = false;
            }

        }

        protected void bbSubmit_Click( object sender, EventArgs e )
        {

            if (Page.IsValid)
            {

                RockContext rockContext = new RockContext();
                PersonService personService = new PersonService( rockContext );
                Person person = null;

                PersonService.PersonMatchQuery personQuery = new PersonService.PersonMatchQuery( dtbFirstName.Text, dtbLastName.Text, ebEmailAddress.Text, null );
                person = personService.FindPerson( personQuery, true );

                if (person == null)
                {
                    person = new Person();
                    person.FirstName = dtbFirstName.Text;
                    person.LastName = dtbLastName.Text;
                    person.IsEmailActive = true;
                    person.Email = ebEmailAddress.Text;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                    person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                    var defaultConnectionStatus = DefinedValueCache.Get( GetAttributeValue( ATTRIBUTE_KEY__DEFAULT_CONNECTION_STATUS ).AsGuid() );
                    if (defaultConnectionStatus != null)
                    {
                        person.ConnectionStatusValueId = defaultConnectionStatus.Id;
                    }

                    var defaultRecordStatus = DefinedValueCache.Get( GetAttributeValue( ATTRIBUTE_KEY__DEFAULT_RECORD_STATUS ).AsGuid() );
                    if (defaultRecordStatus != null)
                    {
                        person.RecordStatusValueId = defaultRecordStatus.Id;
                    }

                    PersonService.SaveNewPerson( person, rockContext );

                }


                var spiritualGiftsTally = new Dictionary<string, int>();

                var spiritualGiftsQuestions = DefinedTypeCache.Get( GetAttributeValue( ATTRIBUTE_KEY__SHAPE_SPIRITUAL_GIFTS_QUESTIONS ).AsGuid() );
                if (spiritualGiftsQuestions != null)
                {
                    foreach (var item in rSpiritualGiftsQuestions.Items)
                    {
                        var repeaterItem = item as RepeaterItem;
                        if (repeaterItem != null)
                        {
                            var questionIdField = repeaterItem.FindControl( "hfSpiritualGiftsQuestionId" ) as HiddenField;
                            if (questionIdField != null)
                            {
                                var questionId = questionIdField.Value.AsInteger();

                                var optionList = repeaterItem.FindControl( "rrblSpiritualGiftsQuestion" ) as RockRadioButtonList;
                                if (optionList != null)
                                {
                                    var answer = optionList.SelectedValue.AsInteger();

                                    var question = DefinedValueCache.Get( questionId );
                                    if (question.DefinedType.Guid == spiritualGiftsQuestions.Guid)
                                    {
                                        var spiritualGiftGuid = question.GetAttributeValue( ATTRIBUTE_KEY__SPIRITUAL_GIFT );
                                        if (!string.IsNullOrWhiteSpace( spiritualGiftGuid ))
                                        {
                                            var currentScore = spiritualGiftsTally.GetValueOrNull( spiritualGiftGuid ) ?? 0;
                                            currentScore = currentScore + answer;
                                            spiritualGiftsTally.AddOrReplace( spiritualGiftGuid, currentScore );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var sortedGifts = spiritualGiftsTally.OrderBy( g => g.Value ).Select( g => g.Key ).ToArray();


                var abilitiesTally = new Dictionary<string, int>();

                var abilitiesQuestions = DefinedTypeCache.Get( GetAttributeValue( ATTRIBUTE_KEY__SHAPE_ABILITIES_QUESTIONS ).AsGuid() );
                if (abilitiesQuestions != null)
                {
                    foreach (var item in rAbilitiesQuestions.Items)
                    {
                        var repeaterItem = item as RepeaterItem;
                        if (repeaterItem != null)
                        {
                            var questionIdField = repeaterItem.FindControl( "hfAbilitiesQuestionId" ) as HiddenField;
                            if (questionIdField != null)
                            {
                                var questionId = questionIdField.Value.AsInteger();

                                var optionList = repeaterItem.FindControl( "rrblAbilitiesQuestion" ) as RockRadioButtonList;
                                if (optionList != null)
                                {
                                    var answer = optionList.SelectedValue.AsInteger();

                                    var question = DefinedValueCache.Get( questionId );
                                    if (question.DefinedType.Guid == abilitiesQuestions.Guid)
                                    {
                                        var spiritualGiftGuid = question.GetAttributeValue( ATTRIBUTE_KEY__ABILITY );
                                        if (!string.IsNullOrWhiteSpace( spiritualGiftGuid ))
                                        {
                                            var currentScore = abilitiesTally.GetValueOrNull( spiritualGiftGuid ) ?? 0;
                                            currentScore = currentScore + answer;
                                            abilitiesTally.AddOrReplace( spiritualGiftGuid, currentScore );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var sortedAbilities = abilitiesTally.OrderBy( g => g.Value ).Select( g => g.Key ).ToArray();


                var heartCategories = DefinedTypeCache.Get( GetAttributeValue( ATTRIBUTE_KEY__SHAPE_HEART_CATEGORIES ).AsGuid() );

                var hearts = rcblHeartOptions
                    .SelectedValues
                    .Where( v =>
                    {
                        var guid = v.AsGuidOrNull();
                        if (guid.HasValue)
                        {
                            var definedValue = DefinedValueCache.Get( guid.Value );
                            if (definedValue != null)
                            {
                                return definedValue.DefinedType.Guid == heartCategories.Guid;
                            }
                        }
                        return false;
                    } );


                person.LoadAttributes();

                if (sortedGifts.Length > 0) person.SetAttributeValue( "SpiritualGift1", sortedGifts[0] );
                if (sortedGifts.Length > 1) person.SetAttributeValue( "SpiritualGift2", sortedGifts[1] );
                if (sortedGifts.Length > 2) person.SetAttributeValue( "SpiritualGift3", sortedGifts[2] );
                if (sortedGifts.Length > 3) person.SetAttributeValue( "SpiritualGift4", sortedGifts[3] );

                if (sortedAbilities.Length > 0) person.SetAttributeValue( "Ability1", sortedAbilities[0] );
                if (sortedAbilities.Length > 1) person.SetAttributeValue( "Ability2", sortedAbilities[1] );

                person.SetAttributeValue( "HeartCategories", string.Join( ",", hearts ) );
                person.SetAttributeValue( "HeartCauses", rtbHeartPast.Text );
                person.SetAttributeValue( "HeartPassion", rtbHeartFuture.Text );

                person.SetAttributeValue( "SHAPEPeople", rtbExperiences_People.Text );
                person.SetAttributeValue( "SHAPEPlaces", rtbExperiences_Places.Text );
                person.SetAttributeValue( "SHAPEEvents", rtbExperiences_Events.Text );

                person.SaveAttributeValues( rockContext );


                var _DISCLastSaveDate = person.GetAttributeValue( "LastSaveDate" );

                if (string.IsNullOrWhiteSpace( _DISCLastSaveDate ))
                {
                    NavigateToLinkedPage( ATTRIBUTE_KEY__DISC_ASSESSMENT_PAGE );
                }
                else
                {
                    NavigateToLinkedPage( ATTRIBUTE_KEY__SHAPE_RESULTS_PAGE );
                }

            }


        }
    }
}