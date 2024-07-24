using Newtonsoft.Json;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.ElectronicSignature;
using Rock.Model;
using Rock.Pdf;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Wordprocessing;
using Rock.Security;

// ID 7534 GUID 37AC82B9-901D-4F1D-BF70-E5C3508AB8A6
// ID 7535 GUID 9A81AB9A-4D4E-49A3-AE7E-D8D960193E03
namespace RockWeb.Plugins.org_lakepointe.VolunteerApplication
{
    [DisplayName( "Volunteer Application" )]
    [Category( "LPC > Connections" )]
    [Description( "Form that will allow a person to apply for a volunteer opportunity." )]
    [FileField( Rock.SystemGuid.BinaryFiletype.DEFAULT,
        Name = "Safety Policy - English",
        Description = "PDF of Safety Policy in English",
        IsRequired = true,
        Key = "SafetyPolicyEnglish",
        Order = 1 )]
    [FileField( Rock.SystemGuid.BinaryFiletype.DEFAULT,
        Name = "Safety Policy - Spanish",
        Description = "PDF of Safety Policy in Spanish",
        IsRequired = true,
        Key = "SafetyPolicySpanish",
        Order = 2 )]

    public partial class VolunteerApplication : RockBlock
    {
        #region Constants

        const int VOLUNTEER_ONBOARDING_CONNECTION_STATUS_LINK_SENT = 112;
        const int VOLUNTEER_ONBOARDING_CONNECTION_STATUS_SUBMITTED = 114;
        const int VOLUNTEER_ONBOARDING_CONNECTION_STATUS_SIGNED = 123;
        const string WISTIA_EMBED = @"<div style=""--aspect-ratio: 16/9;""><iframe src=""//fast.wistia.net/embed/iframe/{0}?playerColor=f04b28"" allowtransparency=""true"" frameborder=""0"" scrolling=""no"" class=""wistia_embed"" name=""wistia_embed"" allowfullscreen mozallowfullscreen webkitallowfullscreen oallowfullscreen msallowfullscreen width=""1600"" height=""900""></iframe><script src=""//fast.wistia.net/assets/external/E-v1.js"" async></script></div>";
        const int CAMPUS_TYPE_PHYSICAL_ENGLISH = 4549;
        const int CAMPUS_TYPE_PHYSICAL_SPANISH = 5339;

        protected enum PageNames
        {
            LANGUAGE,
            INTRO,        // no longer exists but retained for backward compatibility
            PERSONAL_INFORMATION,
            MINISTRY_INFORMATION,
            BACKGROUND,
            PERSONAL_HISTORY,
            CORE_BELIEFS,
            FAITH_STORY,  // no longer exists but retained for backward compatibility
            SAFETY_POLICY_1,
            SAFETY_POLICY_2,  // no longer exists but retained for backward compatibility
            SAFETY_POLICY_3,  // no longer exists but retained for backward compatibility
            SAFETY_POLICY_4,  // no longer exists but retained for backward compatibility
            SAFETY_POLICY_5,  // no longer exists but retained for backward compatibility
            SAFETY_POLICY_7,
            SIGNATURE,
            CONFIRMATION,
            NUMBER_OF_PAGES,
            LOCK
        }

        private static readonly CircularList<PageNames> AdultSequence = new CircularList<PageNames> {
            PageNames.LANGUAGE,
            PageNames.PERSONAL_INFORMATION,
            PageNames.MINISTRY_INFORMATION,
            PageNames.BACKGROUND,
            PageNames.PERSONAL_HISTORY,
            PageNames.CORE_BELIEFS,
            PageNames.SAFETY_POLICY_1,
            PageNames.SAFETY_POLICY_7,
            PageNames.SIGNATURE,
            PageNames.CONFIRMATION
        };

        private static readonly CircularList<PageNames> StaffSequence = new CircularList<PageNames> {
            PageNames.LANGUAGE,
            PageNames.PERSONAL_INFORMATION,
            PageNames.MINISTRY_INFORMATION,
            PageNames.BACKGROUND,
            PageNames.CORE_BELIEFS,
            PageNames.SAFETY_POLICY_1,
            PageNames.SAFETY_POLICY_7,
            PageNames.SIGNATURE,
            PageNames.CONFIRMATION
        };

        private static readonly CircularList<PageNames> ExistingVolunteerSequence = new CircularList<PageNames> {
            PageNames.LANGUAGE,
            PageNames.PERSONAL_INFORMATION,
            PageNames.CORE_BELIEFS,
            PageNames.SAFETY_POLICY_1,
            PageNames.SAFETY_POLICY_7,
            PageNames.SIGNATURE,
            PageNames.CONFIRMATION
        };

        private static readonly CircularList<PageNames> TeenSequence = new CircularList<PageNames> {
            PageNames.LANGUAGE,
            PageNames.PERSONAL_INFORMATION,
            PageNames.MINISTRY_INFORMATION,
            PageNames.BACKGROUND,
            PageNames.PERSONAL_HISTORY,
            PageNames.CORE_BELIEFS,
            PageNames.SAFETY_POLICY_1,
            PageNames.SAFETY_POLICY_7,
            PageNames.SIGNATURE,
            PageNames.CONFIRMATION
        };

        // Teens who have been approved in the last two years don't need references
        private static readonly CircularList<PageNames> RecentTeenSequence = new CircularList<PageNames> {
            PageNames.LANGUAGE,
            PageNames.PERSONAL_INFORMATION,
            PageNames.MINISTRY_INFORMATION,
            PageNames.BACKGROUND,
            PageNames.CORE_BELIEFS,
            PageNames.SAFETY_POLICY_1,
            PageNames.SAFETY_POLICY_7,
            PageNames.SIGNATURE,
            PageNames.CONFIRMATION
        };

        private static readonly CircularList<PageNames> ChildSequence = new CircularList<PageNames> {
            PageNames.LANGUAGE,
            PageNames.PERSONAL_INFORMATION,
            PageNames.MINISTRY_INFORMATION,
            PageNames.BACKGROUND,
            PageNames.PERSONAL_HISTORY,
            PageNames.CORE_BELIEFS,
            PageNames.SAFETY_POLICY_1,
            PageNames.SAFETY_POLICY_7,
            PageNames.SIGNATURE,
            PageNames.CONFIRMATION
        };

        private static readonly CircularList<PageNames> Level2Sequence = new CircularList<PageNames>
        {
            PageNames.LANGUAGE,
            PageNames.PERSONAL_INFORMATION,
            PageNames.MINISTRY_INFORMATION,
            PageNames.PERSONAL_HISTORY,
            PageNames.CORE_BELIEFS,
            PageNames.SAFETY_POLICY_7,
            PageNames.SIGNATURE,
            PageNames.CONFIRMATION
        };

        private static readonly CircularList<PageNames> Level0Sequence = new CircularList<PageNames>
        {
            PageNames.LANGUAGE,
            PageNames.PERSONAL_INFORMATION,
            PageNames.MINISTRY_INFORMATION,
            PageNames.CORE_BELIEFS,
            PageNames.SAFETY_POLICY_7,
            PageNames.SIGNATURE,
            PageNames.CONFIRMATION
        };

        protected enum ApplicationTypes
        {
            STAFF,
            EXISTING_VOLUNTEER,
            ADULT,
            TEEN,
            RECENT_TEEN,
            CHILD,
            LEVEL2,
            LEVEL0
        }

        protected enum Direction
        {
            FORWARD,
            BACKWARD
        }

        #endregion

        #region Fields

        private RockContext _context;
        private Dictionary<string, string> _translation;

        #endregion

        #region Properties

        private PageNames CurrentPage { get; set; }
        //private Dictionary<PageNames, string> Guesses { get; set; }
        private ApplicationTypes ApplicationType { get; set; }
        private CircularList<PageNames> PageSequence { get; set; }
        private Dictionary<string, string> SavedValues { get; set; }
        private Dictionary<string, string> Translation
        {
            get
            {
                if ( _translation == null )
                {
                    _translation = new Dictionary<string, string>();
                    using ( StreamReader r = new StreamReader( MapPath( ResolveRockUrl( string.Format( "~/Plugins/org_lakepointe/VolunteerApplication/VolunteerApplication.{0}.json", SavedValues["PreferredLanguage"].Equals( "Spanish" ) ? "es" : "en" ) ) ) ) )
                    {
                        string json = r.ReadToEnd();
                        _translation = JsonConvert.DeserializeObject<Dictionary<string, string>>( json );
                    }
                }
                return _translation;
            }
            set
            {
                _translation = value;
            }
        }
        private ConnectionRequest ConnectionRequest { get; set; }

        /// <summary>
        /// Gets or sets the signature document HTML, not including the SignatureData.
        /// </summary>
        /// <value>The signature document HTML.</value>
        public string SignatureDocumentHtml
        {
            get { return ViewState["SignatureDocumentHtml"] as string; }
            set { ViewState["SignatureDocumentHtml"] = value; }
        }


        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            lbDownload.Click += LbDownload_Click;
            lbRequestReopen.Click += LbRequestReopen_Click;
            lbNext.Click += LbNext_Click;
            lbPrevious.Click += LbPrevious_Click;
            ddlMinor.SelectedIndexChanged += DdlMinor_SelectedIndexChanged;
            ddlLanguage.SelectedIndexChanged += DdlLanguage_SelectedIndexChanged;

            BlockUpdated += VolunteerApplication_BlockUpdated;
            AddConfigurationUpdateTrigger( upVolunteerApplication );

            if ( _context == null )
            {
                _context = new RockContext();
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( LoadDataset() && !Page.IsPostBack )
            {
                InitializeForm();
            }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            CurrentPage = ( PageNames ) ViewState["CurrentPage"];
            ApplicationType = ( ApplicationTypes ) ViewState["ApplicationType"];
            PageSequence = PageSequenceFor( ApplicationType );
        }

        protected override object SaveViewState()
        {
            ViewState["CurrentPage"] = CurrentPage;
            ViewState["ApplicationType"] = ApplicationType;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        private void LbRequestReopen_Click( object sender, EventArgs e )
        {
            Person applicant = GetApplicant();

            if ( applicant == null )
            {
                return;
            }

            var body = string.Format( @"{0} {1} has requested that their SWiM application be re-opened so they can update it. Please click <a href=""https://rock.lakepointe.church/page/2119?ConnectionRequestId={2}"" target=""_blank"">this link</a> and send them a fresh link to the application.",
                applicant.NickName, applicant.LastName, ConnectionRequest.Id );

            Send( new List<RockEmailMessageRecipient>() { RockEmailMessageRecipient.CreateAnonymous( "serveteams@lakepointe.church", null ) }, "do-not-reply@lakepointe.church", "Rock", "Request to Reopen SWiM Application", body );

            lCantUpdate.Text = Translation["ReopenRequested"];
        }

        private void LbDownload_Click( object sender, EventArgs e )
        {
            // This has been disabled by making the button not visible. The file is a Digitally Signed Document, and logged in users
            // don't have permission to download those. The alternative would be to make a local copy of the file and let them download
            // that, but we've already emailed them a copy so they really shouldn't need this feature. They can ask staff to send them
            // a copy if necessary.
            Person applicant = GetApplicant();

            if ( applicant == null )
            {
                return;
            }

            var pdfGuid = GetApplicationPdfGuid( applicant );

            if ( pdfGuid == null )
            {
                return;
            }

            var file = new BinaryFileService( _context ).Get( pdfGuid.AsGuid() );
            if ( file != null )
            {
                var url = string.Format( "{0}GetFile.ashx?id={1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), file.Id );
                Response.Redirect( url );
            }
        }

        protected void LbPrevious_Click( object sender, EventArgs e )
        {
            Navigate( Direction.BACKWARD );
        }

        protected void LbNext_Click( object sender, EventArgs e )
        {
            if ( !AnswersAreCorrect() )
            {
                return;
            }

            Navigate( Direction.FORWARD );
        }

        private void SetPolicyHyperlink()
        {
            var pdfGuid = GetAttributeValue( SavedValues["PreferredLanguage"].Equals( "Spanish" ) ? "SafetyPolicySpanish" : "SafetyPolicyEnglish" ).AsGuid();

            var file = new BinaryFileService( _context ).Get( pdfGuid );
            if ( file != null )
            {
                hlPolicyPDF.NavigateUrl = string.Format( "{0}GetFile.ashx?id={1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), file.Id );
            }
        }

        protected void VolunteerApplication_BlockUpdated( object sender, EventArgs e )
        {
            if ( LoadDataset() )
            {
                InitializeForm();
            }
        }

        private void DdlMinor_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateKidInfo( ddlMinor.SelectedValue.AsInteger() );
        }

        private void DdlLanguage_SelectedIndexChanged( object sender, EventArgs e )
        {
            SavedValues["PreferredLanguage"] = ddlLanguage.SelectedValue;
            SetUILanguage();
            upVolunteerApplication.Triggers.Add( new PostBackTrigger() { ControlID = "lbNext" } );
        }

        protected void rblInternationalStudent_SelectedIndexChanged( object sender, EventArgs e )
        {
            ConnectionRequest.AttributeValues["IsInternationalStudent"].Value = MapYesNoToTrueFalse( rblInternationalStudent.SelectedValue );

            if (rblInternationalStudent.SelectedValue == "Yes" )
            {
                // if they tell us they're an international student, make sure we force SSN to be false. We'll hide the control but if had
                // previously been set to true that might mislead the people following up on the application. (Sometimes international students
                // have a SSN for reporting scholarship income. That doesn't mean we can use it for a background check.)
                SavedValues["HasSSN"] = "False";
                rblSSN.SelectedValue = "No";
            }

            SelectPage( CurrentPage ); // Reconfigure current page based on new selection
        }

        #endregion

        #region Methods

        private void InitializeForm()
        {
            SetUILanguage();

            if ( ConnectionRequest.ConnectionStatusId == VOLUNTEER_ONBOARDING_CONNECTION_STATUS_LINK_SENT ||
                ConnectionRequest.ConnectionStatusId == VOLUNTEER_ONBOARDING_CONNECTION_STATUS_SUBMITTED ||
                ConnectionRequest.ConnectionStatusId == VOLUNTEER_ONBOARDING_CONNECTION_STATUS_SIGNED )
            {
                PageNames lastPageViewed;
                if ( !Enum.TryParse( SavedValues["LastPageViewed"], out lastPageViewed ) )
                {
                    lastPageViewed = PageNames.LANGUAGE;
                }
                CurrentPage = lastPageViewed;

                switch ( ApplicationType )
                {
                    case ApplicationTypes.STAFF:
                    case ApplicationTypes.ADULT:
                        InitializeAdultApplication();
                        break;
                    case ApplicationTypes.EXISTING_VOLUNTEER:
                        InitializeExistingVolunteerApplication();
                        break;
                    case ApplicationTypes.RECENT_TEEN:
                    case ApplicationTypes.TEEN:
                        InitializeTeenApplication();
                        break;
                    case ApplicationTypes.CHILD:
                        InitializeChildApplication();
                        break;
                    case ApplicationTypes.LEVEL2:
                        InitializeLevel2Application();
                        break;
                    case ApplicationTypes.LEVEL0:
                        InitializeLevel0Application();
                        break;
                }

                SelectPage( CurrentPage );
            }
            else
            {
                SelectPage( PageNames.LOCK );
            }
        }

        private bool LoadDataset()
        {
            ConnectionRequest = null;
            var connectionRequestGuid = ( ( string ) ViewState["CR"] ).AsGuid();
            if ( connectionRequestGuid == Guid.Empty )
            {
                connectionRequestGuid = Request["CR"].AsGuid();
            }
            if ( connectionRequestGuid != Guid.Empty )
            {
                ViewState["CR"] = connectionRequestGuid.ToString();
                ConnectionRequest = new ConnectionRequestService( _context ).Get( connectionRequestGuid );
            }
            else
            {
                nbWarning.Heading = "Error ...";
                nbWarning.Text = "Required page routing data missing. Please use the link from the email you received.";
                ScrollToTop();
                return false;
            }

            if ( ConnectionRequest.PersonAlias.PersonId != CurrentPersonId )
            {
                nbWarning.Heading = "Error ...";
                nbWarning.Text = string.Format( "You must be logged in as {0} to complete this form.", ConnectionRequest.PersonAlias.Person.FullName );
                ScrollToTop();
                return false;
            }

            if ( CurrentPerson.AgeClassification != AgeClassification.Adult )
            {
                nbWarning.Heading = "Error ...";
                nbWarning.Text = "You must be an adult to complete this form.";
                ScrollToTop();
                return false;
            }

            SavedValues = null;
            var connectionRequestAttributes = ConnectionRequest.AssignedGroupMemberAttributeValues;
            if ( connectionRequestAttributes.IsNotNullOrWhiteSpace() )
            {
                SavedValues = JsonConvert.DeserializeObject<Dictionary<string, string>>( connectionRequestAttributes );
            }

            if ( SavedValues == null )
            {
                SavedValues = InitGroupMemberAttributeValues( ConnectionRequest.AssignedGroupId );
                if ( SavedValues != null )
                {
                    ConnectionRequest.AssignedGroupMemberAttributeValues = JsonConvert.SerializeObject( SavedValues );
                }
                else
                {
                    nbWarning.Heading = "Error ...";
                    nbWarning.Text = "Could not initialize group member attributes.";
                    ScrollToTop();
                    return false;
                }
            }
            else
            {
                // transitional patch ... for in-process applications started but not submitted before we added the requirement to provide four references,
                // we need to add the spot for the fourth reference so they don't have to figure out how to do that manually using
                // the attribute matrix widget.
                // SNS - 20240306 - This entire else clause can be removed after 20240601. That should be more than enough time for all "in-process" applications to be finished.
                if ( SavedValues.ContainsKey("References") ) // Level 0 apps don't require references
                {
                    var attributeMatrixService = new AttributeMatrixService( _context );
                    var references = attributeMatrixService.Get( SavedValues["References"].AsGuid() );
                    references.LoadAttributes();
                    if ( references.AttributeMatrixItems.Count < 4 )
                    {
                        // add a fourth reference slot
                        AddAttributeMatrixItem( _context, references );
                        _context.SaveChanges();
                    }
                }
            }

            switch ( ConnectionRequest.AssignedGroupId )
            {
                case 841673:
                    ApplicationType = ApplicationTypes.EXISTING_VOLUNTEER;
                    break;
                case 795253:
                    ApplicationType = ApplicationTypes.ADULT;
                    break;
                case 842622:
                    ApplicationType = TeenHasRecentApproval() ? ApplicationTypes.RECENT_TEEN : ApplicationTypes.TEEN;
                    break;
                case 842637:
                    ApplicationType = ApplicationTypes.CHILD;

                    // Patch child applications created before the latest round of updates
                    if ( !SavedValues.ContainsKey( "Hinderence" ) )
                    {
                        SavedValues.Add( "Hinderence", null );
                    }
                    if ( !SavedValues.ContainsKey( "HinderenceInfo" ) )
                    {
                        SavedValues.Add( "HinderenceInfo", null );
                    }
                    break;
                case 852568:
                    ApplicationType = ApplicationTypes.STAFF;
                    break;
                case 887750:
                    ApplicationType = ApplicationTypes.LEVEL2;
                    break;
                case 1014494:
                    ApplicationType = ApplicationTypes.LEVEL0;
                    break;
                default:
                    nbWarning.Heading = "Error ...";
                    nbWarning.Text = "No application type target group set.";
                    ScrollToTop();
                    return false;
            }

            ConnectionRequest.LoadAttributes();

            PageSequence = PageSequenceFor( ApplicationType );

            return true;
        }

        private bool TeenHasRecentApproval()
        {
            if ( ConnectionRequest == null || ConnectionRequest.AssignedGroupId != 842622 || SavedValues == null ) // shouldn't happen
            {
                return false;
            }

            var studentGuid = SavedValues["Student"].AsGuid();
            if ( studentGuid == Guid.Empty ) // happens if we haven't picked a student yet
            {
                return false;
            }

            var student = new PersonAliasService( _context ).Queryable().AsNoTracking()
                .Where( pa => pa.Guid == studentGuid ).FirstOrDefault().Person;
            student.LoadAttributes( _context );
            if ( !student.AttributeValues.ContainsKey( "Arena-29-275" ) )
            {
                return false;
            }

            var cmStudentApprovalDateString = student.AttributeValues["Arena-29-275"].Value;
            if ( cmStudentApprovalDateString.IsNullOrWhiteSpace() )
            {
                return false;
            }

            if ( DateTime.Parse( cmStudentApprovalDateString ) < DateTime.Now.AddYears( -2 ) )  // application approval date is before two years ago
            {
                return false;
            }

            return true;
        }

        private Dictionary<string, string> InitGroupMemberAttributeValues( int? groupId )
        {
            var values = new Dictionary<string, string>();

            if ( groupId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    var groupType = new GroupTypeService( rockContext ).Get( group.GroupTypeId );
                    var role = new GroupTypeRoleService( rockContext ).Get( groupType.DefaultGroupRoleId.Value );
                    if ( group != null && role != null )
                    {
                        var groupMember = new GroupMember();
                        groupMember.Group = group;
                        groupMember.GroupId = group.Id;
                        groupMember.GroupRole = role;
                        groupMember.GroupRoleId = role.Id;
                        groupMember.GroupMemberStatus = GroupMemberStatus.Active;

                        groupMember.LoadAttributes();

                        foreach ( var attrValue in groupMember.AttributeValues )
                        {
                            values.Add( attrValue.Key, attrValue.Value.Value );
                        }

                        // Create the AttributeMatrixes now and save it even though they haven't hit save yet. We'll need the AttributeMatrix record to exist so that we can add AttributeMatrixItems to it
                        // If this ends up creating orphans, they'll be cleaned up later by the database maintenance task
                        var attributeMatrixService = new AttributeMatrixService(rockContext);

                        if ( ApplicationType != ApplicationTypes.LEVEL2 && ApplicationType != ApplicationTypes.LEVEL0 )
                        {
                            var experienceWithMinors = new AttributeMatrix { Guid = Guid.NewGuid() };
                            experienceWithMinors.AttributeMatrixTemplateId = 9; // Experience with Minors
                            experienceWithMinors.AttributeMatrixItems = new List<AttributeMatrixItem>();
                            attributeMatrixService.Add( experienceWithMinors );
                            values["ExperienceWithMinors"] = experienceWithMinors.Guid.ToString();
                        }

                        if ( ApplicationType != ApplicationTypes.LEVEL0 )
                        {
                            var references = new AttributeMatrix { Guid = Guid.NewGuid() };
                            references.AttributeMatrixTemplateId = 10; // Personal References
                            references.AttributeMatrixItems = new List<AttributeMatrixItem>();
                            attributeMatrixService.Add(references);
                            values["References"] = references.Guid.ToString();

                            // People are having difficulty understanding how to use the attribute matrix control
                            // on the form, so we want to pre-populate the matrix with three empty references so they
                            // don't have to figure out how to click the "+" button.
                            rockContext.SaveChanges(); // necessary before we start adding items to the matrix we just created
                            AddAttributeMatrixItem(rockContext, references);
                            AddAttributeMatrixItem(rockContext, references);
                            AddAttributeMatrixItem(rockContext, references);
                            AddAttributeMatrixItem( rockContext, references );

                            var referenceResponses = new AttributeMatrix { Guid = Guid.NewGuid() };
                            referenceResponses.AttributeMatrixTemplateId = ApplicationType == ApplicationTypes.LEVEL2 ? 20 : 12; // Reference Response
                            referenceResponses.AttributeMatrixItems = new List<AttributeMatrixItem>();
                            attributeMatrixService.Add(referenceResponses);
                            values["ReferenceResponses"] = referenceResponses.Guid.ToString();
                        }

                        rockContext.SaveChanges();

                        return values;
                    }
                }
            }

            return null;
        }

        private void AddAttributeMatrixItem(RockContext rockContext, AttributeMatrix attributeMatrix)
        {
            var attributeMatrixItem = new AttributeMatrixItem
            {
                AttributeMatrix = attributeMatrix
            };

            //attributeMatrixItem.AttributeMatrix.AttributeMatrixTemplate = new AttributeMatrixTemplateService(rockContext).Get(attributeMatrixItem.AttributeMatrix.AttributeMatrixTemplateId);

            attributeMatrixItem.LoadAttributes();
            attributeMatrix.AttributeMatrixItems.Add(attributeMatrixItem);
        }

        private void SetUILanguage()
        {
            bool isEnglish = !SavedValues["PreferredLanguage"].Equals( "Spanish" ); // do it this way so "" is "English"

            // modify UI accordingly

            // Personal Information
            PageTitlePersonalInformation.Text = Translation["PageTitlePersonalInformation"];
            cbParentNotification.Text = Translation["ParentNotification"];
            PersonalInformationInstructions.Text = Translation["PersonalInformationInstructions"];
            lSectionTitleParentInfo.Text = Translation["SectionTitleParentInfo"];
            tbFirstName.Label = Translation["LabelFirstName"];
            tbFirstName.RequiredErrorMessage = Translation["ErrorMessageFirstName"];
            tbLastName.Label = Translation["LabelLastName"];
            tbLastName.RequiredErrorMessage = Translation["ErrorMessageLastName"];
            ddlGender.Label = Translation["LabelGender"];
            ddlGender.RequiredErrorMessage = Translation["ErrorMessageGender"];
            tbPhone.Label = Translation["LabelPhone"];
            tbPhone.RequiredErrorMessage = Translation["ErrorMessagePhone"];
            tbEmail.Label = Translation["LabelEmail"];
            tbEmail.RequiredErrorMessage = Translation["ErrorMessageEmail"];
            dpBirthDate.Label = Translation["LabelBirthDate"];
            dpBirthDate.RequiredErrorMessage = Translation["ErrorMessageBirthDate"];
            dpBirthDate.FutureDatesErrorMessage = Translation["FutureBirthDate"];
            lPreferredContactInstructions.Text = Translation["PreferredContactInstructions"];
            rblContactMethod.Label = Translation["LabelPreferredContactMethod"];
            rblContactMethod.Items[0].Text = Translation["OptionCall"];
            rblContactMethod.Items[1].Text = Translation["OptionEmail"];
            rblContactMethod.Items[2].Text = Translation["OptionText"];
            lSectionTitleMinorInfo.Text = Translation["SectionTitleMinorInfo"];
            ddlMinor.Label = Translation["LabelMinorSelector"];
            tbKidFirstName.Label = Translation["LabelFirstName"];
            tbKidFirstName.RequiredErrorMessage = Translation["ErrorMessageFirstName"];
            tbKidLastName.Label = Translation["LabelLastName"];
            tbKidLastName.RequiredErrorMessage = Translation["ErrorMessageLastName"];
            ddlKidGender.Label = Translation["LabelGender"];
            ddlKidGender.RequiredErrorMessage = Translation["ErrorMessageGender"];
            tbKidPhone.Label = Translation["LabelPhone"];
            tbKidPhone.RequiredErrorMessage = Translation["ErrorMessagePhone"];
            tbKidEmail.Label = Translation["LabelEmail"];
            tbKidEmail.Help = Translation["HelpKidEmail"];
            tbKidEmail.RequiredErrorMessage = Translation["ErrorMessageEmail"];
            dpKidBirthDate.Label = Translation["LabelBirthDate"];
            dpKidBirthDate.RequiredErrorMessage = Translation["ErrorMessageBirthDate"];
            dpKidBirthDate.FutureDatesErrorMessage = Translation["FutureBirthDate"];

            // Campus & Ministry Selection
            lSectionTitleVolunteerOpportunitiesForMinors.Text = Translation["SectionTitleVolunteerOpportunitiesForMinors"];
            pnlOpportunitiesEnglish.Visible = isEnglish;
            pnlOpportunitiesSpanish.Visible = !isEnglish;
            lPageTitleCampusAndMinistry.Text = Translation["PageTitleCampusAndMinistry"];
            cpCampus.Label = Translation["LabelCampus"];
            cblMinistries.Label = Translation["LabelMinistries"];
            tbOther.Label = Translation["OtherLabel"];

            // Background Information
            lPageTitleBackground.Text = Translation["PageTitleBackgroundInformation"];
            rblChurchHome.Label = Translation["LabelChurchHome"];
            rblChurchHome.Items[0].Text = Translation["Yes"];
            rblChurchHome.Items[1].Text = Translation["No"];
            tbChurchYears.Label = Translation["LabelChurchYears"];
            tbChurchHome.Label = Translation["LabelOtherChurch"];
            lGridDescription.Text = Translation["LabelPreviousExperience"];
            tbMotivation.Label = Translation["LabelMotivation"];
            rblAgeGroupOrGender.Label = Translation["LabelAgeGroupOrGenderPreference"];
            rblAgeGroupOrGender.Items[0].Text = Translation["Yes"];
            rblAgeGroupOrGender.Items[1].Text = Translation["No"];
            rblAgeGroupOrGender.Items[2].Text = Translation["NotApplicable"];
            rtbAgeGroupOrGender.Label = Translation["LabelAgeGroupOrGenderExplain"];

            lConfidentialDisclaimer.Text = Translation["DisclaimerSensitiveInformation"];
            rblHasAbused.Label = Translation["LabelHasAbused"];
            rblHasAbused.Items[0].Text = Translation["Yes"];
            rblHasAbused.Items[1].Text = Translation["No"];
            rtbHasAbused.Label = Translation["LabelHasAbusedExplain"];
            rblHasBeenAccused.Label = Translation["LabelHasBeenAccused"];
            rblHasBeenAccused.Items[0].Text = Translation["Yes"];
            rblHasBeenAccused.Items[1].Text = Translation["No"];
            rtbHasBeenAccused.Label = Translation["LabelHasBeenAccusedExplain"];
            rblPsycho.Label = Translation["LabelMentalHealth"];
            rblPsycho.Items[0].Text = Translation["Yes"];
            rblPsycho.Items[1].Text = Translation["No"];
            rtbPsycho.Label = Translation["LabelMentalHealthExplain"];
            rblHindrance.Label = Translation["LabelHindrance"];
            rblHindrance.Items[0].Text = Translation["Yes"];
            rblHindrance.Items[1].Text = Translation["No"];
            rtbHindrance.Label = Translation["LabelHindranceExplain"];

            // References
            lPageTitleReferences.Text = Translation["PageTitleReferences"];
            lReferenceDescription.Text = Translation["LabelReferenceDescription"];
            lReferenceFooter.Text = Translation["LabelReferenceFooter"];

            // Core Beliefs
            lPageTitleBelief.Text = Translation["PageTitleBelief"];
            pnlBeliefEnglish.Visible = isEnglish;
            pnlBeliefSpanish.Visible = !isEnglish;
            rblBelief.Items[0].Text = Translation["OptionBelief1"];
            rblBelief.Items[1].Text = Translation["OptionBelief2"];

            // Safety Policy Pages
            lSafetyPolicyInstruction.Text = Translation["SafetyPolicyInstruction"];
            lPageTitleSafetyPolicy1.Text = Translation["PageTitleSafetyPolicy"];
            pnlSafety1English.Visible = isEnglish;
            pnlSafety1Spanish.Visible = !isEnglish;
            hlPolicyPDF.Text = Translation["DownloadPolicy"];
            SetPolicyHyperlink();

            lPageTitleSafetyPolicy7.Text = Translation["PageTitleSafetyPolicy"];
            pnlSafety7English.Visible = isEnglish;
            pnlSafety7Spanish.Visible = !isEnglish;
            lStudentVolunteerAgreement.Text = Translation["SectionTitleStudentVolunteerAgreement"];
            lSignatureNote.Text = Translation["SectionSubtitleStudentVolunteerAgreement"];
            rcblStudentAgreement.Items[0].Text = Translation["StudentAgree1"];
            rcblStudentAgreement.Items[1].Text = Translation["StudentAgree2"];
            rcblStudentAgreement.Items[2].Text = Translation["StudentAgree3"];
            rcblStudentAgreement.Items[3].Text = Translation["StudentAgree4"];
            rcblStudentAgreement.Items[4].Text = Translation["StudentAgree5"];
            rcblStudentAgreement.Items[5].Text = Translation["StudentAgree6"];

            lSectionTitleNoticeOfRequirements.Text = Translation["SectionTitleNoticeOfRequirements"];
            lNoticeOfRequirements.Text = Translation["NoticeOfRequirements"];
            rcblNoticeOfRequirements.Items[0].Text = Translation["Notice1"];
            lParentGuardianSignature.Text = string.Format( @"<b>{0}: </b>{{t:s;r:y;o:""Volunteer"";}}", Translation["ParentGuardianSignature"] );
            lParentGuardianDate.Text = string.Format( @"<b>{0}: </b>{{t:t;r:y;o:""Volunteer"";}}", Translation["Date"] );
            lSectionTitleParentGuardianApproval.Text = Translation["SectionTitleParentGuardianApproval"];
            rcblParentGuardianApproval.Items[0].Text = Translation["Approval1"];
            rcblParentGuardianApproval.Items[1].Text = Translation["Approval2"];
            rcblParentGuardianApproval.Items[2].Text = Translation["Approval3"];
            rcblParentGuardianApproval.Items[3].Text = Translation["Approval4"];
            rcblParentGuardianApproval.Items[4].Text = Translation["Approval5"];

            lSectionTitleAckAndAg.Text = Translation["SectionTitleAckAndAg"];
            lSectionTitleAckAndAgLevel2.Text = Translation["SectionTitleAckAndAg"];
            rcblAgree.Items[0].Text = Translation["Agree1"];
            rcblAgree.Items[1].Text = Translation["Agree2"];
            rcblAgree.Items[2].Text = Translation["Agree3"];
            rcblAgreeL2.Items[0].Text = Translation["Level2Agree1"];
            rcblAgreeL2.Items[1].Text = Translation["Level2Agree3"];
            rcblAgreeL0.Items[0].Text = Translation["Level0Agree1"];
            rcblAgreeL0.Items[1].Text = Translation["Level0Agree3"];
            rcblAgreeL0.Items[2].Text = Translation["Level0Agree2"];
            rblSSN.Label = Translation["LabelHaveSSN"];
            rblSSN.Help = Translation["HelpHaveSSN"];
            rblSSN.Items[0].Text = Translation["Yes"];
            rblSSN.Items[1].Text = Translation["No"];

            rblInternationalStudent.Label = Translation["LabelInternationalStudent"];
            rblInternationalStudent.Help = Translation["HelpInternationalStudent"];
            rblInternationalStudent.Items[0].Text = Translation["Yes"];
            rblInternationalStudent.Items[1].Text = Translation["No"];
            rtbSurname.Label = Translation["Surname"];
            rtbGivenName.Label = Translation["GivenName"];
            rtbMothersMaidenName.Label = Translation["MothersMaidenName"];
            rtbOtherNames.Label = Translation["OtherNames"];
            rtbPlaceOfBirth.Label = Translation["PlaceOfBirth"];
            dppDateOfBirth.Label = Translation["LabelBirthDate"];
            rtbCurrentAddress.Label = Translation["CurrentAddress"];
            rtbYearsInTX.Label = Translation["YearsInTexas"];
            rtbMonthsAtLakepointe.Label = Translation["MonthsAtLakepointe"];
            rtbPriorAddress.Label = Translation["PriorAddress"];
            rtbSocialSecurityNumber.Label = Translation["SocialSecurityNumber"];
            rfuIDFront.Label = Translation["CopyOfIdFront"];

            lSectionTitleAdultRelease.Text = Translation["SectionTitleRelease"];
            rcblAdultRelease.Items[0].Text = Translation["AdultRelease1"];
            rcblAdultRelease.Items[1].Text = Translation["AdultRelease2"];
            rcblAdultRelease.Items[2].Text = Translation["AdultRelease3"];
            rcblAdultRelease.Items[3].Text = Translation["AdultRelease4"];
            lSectionTitleMinorRelease.Text = Translation["SectionTitleRelease"];
            rcblMinorRelease.Items[0].Text = Translation["MinorRelease1"];
            rcblMinorRelease.Items[1].Text = Translation["MinorRelease3"];
            rcblMinorRelease.Items[2].Text = Translation["MinorRelease4"];
            rtbMinorSignature.Label = Translation["MinorSignature"];

            lPageTitleSignNow.Text = Translation["PageTitleSignNow"];
            lSignatureInstructions.Text = Translation["GuardianSignatureInstructions"];
            lSignatureInstructionsPDF.Text = Translation["GuardianSignatureInstructions"];

            // Confirmation Page
            lPageTitleConfirmation.Text = Translation["PageTitleConfirmation"];

            // Lock Page
            lPageTitleLocked.Text = Translation["PageTitleLocked"];
            lCantUpdate.Text = Translation["CantUpdate"];
            lbDownload.Text = Translation["Download"];
            lbRequestReopen.Text = Translation["RequestReopen"];

            // Footer
            lClickDoneFirst.Text = Translation["ClickDoneFirst"];
            lbNext.Text = Translation["NavigationNext"];
            lbPrevious.Text = Translation["NavigationPrevious"];
            lFooterMessage.Text = Translation["FooterMessage"];
        }

        private CircularList<PageNames> PageSequenceFor( ApplicationTypes applicationType )
        {
            switch ( applicationType )
            {
                case ApplicationTypes.EXISTING_VOLUNTEER:
                    return ExistingVolunteerSequence;
                case ApplicationTypes.STAFF:
                    return StaffSequence;
                case ApplicationTypes.ADULT:
                    return AdultSequence;
                case ApplicationTypes.TEEN:
                    return TeenSequence;
                case ApplicationTypes.RECENT_TEEN:
                    return RecentTeenSequence;
                case ApplicationTypes.CHILD:
                    return ChildSequence;
                case ApplicationTypes.LEVEL2:
                    return Level2Sequence;
                case ApplicationTypes.LEVEL0:
                    return Level0Sequence;
                default:
                    return null;
            }
        }

        private void InitializeLevel2Application()
        {
            // Language Selection Page
            ddlLanguage.SelectedValue = SavedValues["PreferredLanguage"];

            // Personal Information Page
            ddlGender.BindToEnum<Gender>(); // false, new Gender[] { Gender.Unknown });

            tbFirstName.Text = CurrentPerson.NickName;
            tbLastName.Text = CurrentPerson.LastName;
            ddlGender.SelectedValue = ( ( int ) CurrentPerson.Gender ).ToString();
            var phone = CurrentPerson.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            tbPhone.Text = phone == null ? null : phone.NumberFormatted;
            tbEmail.Text = CurrentPerson.Email;
            dpBirthDate.SelectedDate = CurrentPerson.BirthDate;
            rblContactMethod.SelectedValue = SavedValues.ContainsKey( "PreferredContactMethod" ) ? SavedValues["PreferredContactMethod"] : null;

            // Campus and Ministries Page
            cpCampus.Campuses = CampusCache.All().Where( c => ( c.CampusTypeValueId == CAMPUS_TYPE_PHYSICAL_ENGLISH || c.CampusTypeValueId == CAMPUS_TYPE_PHYSICAL_SPANISH ) && c.IsActive.Value ).ToList();
            cpCampus.SelectedCampusId = CurrentPerson.GetCampus()?.Id ?? 3;   // preset to home campus in case nothing has been set in the CR
            var campusGuid = SavedValues["Campus"].AsGuid();
            if ( campusGuid != Guid.Empty )                               // override with CR campus
            {
                cpCampus.SelectedCampusId = new CampusService( _context ).Get( campusGuid ).Id;
            }

            cblMinistries.DataSource = Level2MinistriesOfInterest().ToList();
            cblMinistries.DataTextField = "Value";
            cblMinistries.DataValueField = "Key";
            cblMinistries.DataBind();
            cblMinistries.SetValues( SavedValues["MinistriesofInterest"].Split( ',' ) );

            tbOther.Text = SavedValues["OtherMinistry"];

            //// Background Information Page
            //lPageSubtitleBackground.Visible = false;
            //rblChurchHome.SelectedValue = MapTrueFalseToYesNo( SavedValues["LPisChurchHome"] );
            //tbChurchHome.Text = SavedValues["ChurchHome"];
            //tbChurchYears.Text = SavedValues["HowLong"];

            // References Page
            ameReferences.AttributeMatrixGuid = SavedValues["References"].AsGuid();

            // What We Believe Page
            rblBelief.SelectedValue = SavedValues["CoreBeliefs"];

            // Personal Conduct Code
            lPageTitleSafetyPolicy7.Visible = false;
            rcblAgreeL2.SetValues( SavedValues["Agree"].Split( ',' ) );
            rblSSN.SelectedValue = MapTrueFalseToYesNo( SavedValues["HasSSN"] );

            rblInternationalStudent.SelectedValue = MapTrueFalseToYesNo( ConnectionRequest.AttributeValues.ContainsKey( "IsInternationalStudent" ) ? ConnectionRequest.AttributeValues["IsInternationalStudent"].Value : "No" );
            rtbSurname.Text = ConnectionRequest.AttributeValues.ContainsKey( "Surname" ) ? ConnectionRequest.AttributeValues["Surname"].Value : string.Empty;
            rtbGivenName.Text = ConnectionRequest.AttributeValues.ContainsKey( "GivenName" ) ? ConnectionRequest.AttributeValues["GivenName"].Value : string.Empty;
            rtbMothersMaidenName.Text = ConnectionRequest.AttributeValues.ContainsKey( "MothersFullMaidenName" ) ? ConnectionRequest.AttributeValues["MothersFullMaidenName"].Value : string.Empty;
            rtbOtherNames.Text = ConnectionRequest.AttributeValues.ContainsKey( "OtherNamesUsed" ) ? ConnectionRequest.AttributeValues["OtherNamesUsed"].Value : string.Empty;
            rtbPlaceOfBirth.Text = ConnectionRequest.AttributeValues.ContainsKey( "PlaceOfBirth" ) ? ConnectionRequest.AttributeValues["PlaceOfBirth"].Value : string.Empty;
            rtbCurrentAddress.Text = ConnectionRequest.AttributeValues.ContainsKey( "CurrentUSAddress" ) ? ConnectionRequest.AttributeValues["CurrentUSAddress"].Value : string.Empty;
            rtbYearsInTX.Text = ConnectionRequest.AttributeValues.ContainsKey( "NumberOfYearsInTexas" ) ? ConnectionRequest.AttributeValues["NumberOfYearsInTexas"].Value : string.Empty;
            rtbMonthsAtLakepointe.Text = ConnectionRequest.AttributeValues.ContainsKey( "MonthsAttendingLakepointeChurch" ) ? ConnectionRequest.AttributeValues["MonthsAttendingLakepointeChurch"].Value : string.Empty;
            rtbPriorAddress.Text = ConnectionRequest.AttributeValues.ContainsKey( "PriorAddressOut-of-USAddress" ) ? ConnectionRequest.AttributeValues["PriorAddressOut-of-USAddress"].Value : string.Empty;
            rtbSocialSecurityNumber.Text = ConnectionRequest.AttributeValues.ContainsKey( "NationalIDNumber" ) ? Encryption.DecryptString( ConnectionRequest.AttributeValues["NationalIDNumber"].Value ) : string.Empty;

            var frontId = GetImageId( "CopyOfPassport" );
            if ( frontId != null )
            {
                rfuIDFront.BinaryFileId = frontId.Value;
            }

            var dateString = ConnectionRequest.AttributeValues.ContainsKey( "DateOfBirth" ) ? ConnectionRequest.AttributeValues["DateOfBirth"].Value : string.Empty;
            if ( dateString != string.Empty )
            {
                dppDateOfBirth.SelectedDate = DateTime.Parse( dateString, null, System.Globalization.DateTimeStyles.RoundtripKind );
            }

            rcblAdultRelease.SetValues( SavedValues["Release"].Split( ',' ) );
        }

        private void InitializeLevel0Application()
        {
            // Language Selection Page
            ddlLanguage.SelectedValue = SavedValues["PreferredLanguage"];

            // Personal Information Page
            ddlGender.BindToEnum<Gender>(); // false, new Gender[] { Gender.Unknown });

            tbFirstName.Text = CurrentPerson.NickName;
            tbLastName.Text = CurrentPerson.LastName;
            ddlGender.SelectedValue = ((int)CurrentPerson.Gender).ToString();
            var phone = CurrentPerson.GetPhoneNumber(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid());
            tbPhone.Text = phone == null ? null : phone.NumberFormatted;
            tbEmail.Text = CurrentPerson.Email;
            dpBirthDate.SelectedDate = CurrentPerson.BirthDate;
            rblContactMethod.SelectedValue = SavedValues.ContainsKey("PreferredContactMethod") ? SavedValues["PreferredContactMethod"] : null;

            // Campus and Ministries Page
            cpCampus.Campuses = CampusCache.All().Where(c => (c.CampusTypeValueId == CAMPUS_TYPE_PHYSICAL_ENGLISH || c.CampusTypeValueId == CAMPUS_TYPE_PHYSICAL_SPANISH) && c.IsActive.Value).ToList();
            cpCampus.SelectedCampusId = CurrentPerson.GetCampus()?.Id ?? 3;   // preset to home campus in case nothing has been set in the CR
            var campusGuid = SavedValues["Campus"].AsGuid();
            if (campusGuid != Guid.Empty)                               // override with CR campus
            {
                cpCampus.SelectedCampusId = new CampusService(_context).Get(campusGuid).Id;
            }

            cblMinistries.DataSource = Level0MinistriesOfInterest().ToList();
            cblMinistries.DataTextField = "Value";
            cblMinistries.DataValueField = "Key";
            cblMinistries.DataBind();
            cblMinistries.SetValues(SavedValues["MinistriesofInterest"].Split(','));

            tbOther.Text = SavedValues["OtherMinistry"];

            // What We Believe Page
            rblBelief.SelectedValue = SavedValues["CoreBeliefs"];

            // Personal Conduct Code
            lPageTitleSafetyPolicy7.Visible = false;
            rcblAgreeL0.SetValues(SavedValues["Agree"].Split(','));
        }

        private void InitializeAdultApplication()
        {
            // Language Selection Page
            ddlLanguage.SelectedValue = SavedValues["PreferredLanguage"];

            // Personal Information Page
            ddlGender.BindToEnum<Gender>(); // false, new Gender[] { Gender.Unknown });

            // Beware. Except for PreferredContactMethod, everything on this page is derived from CurrentPerson
            // and any changes to that data are NOT persisted.
            tbFirstName.Text = CurrentPerson.NickName;
            tbLastName.Text = CurrentPerson.LastName;
            ddlGender.SelectedValue = ( ( int ) CurrentPerson.Gender ).ToString();
            var phone = CurrentPerson.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            tbPhone.Text = phone == null ? null : phone.NumberFormatted;
            tbEmail.Text = CurrentPerson.Email;
            dpBirthDate.SelectedDate = CurrentPerson.BirthDate;
            rblContactMethod.SelectedValue = SavedValues.ContainsKey( "PreferredContactMethod" ) ? SavedValues["PreferredContactMethod"] : null;

            // Campus and Ministries Page
            cpCampus.Campuses = CampusCache.All().Where( c => ( c.CampusTypeValueId == CAMPUS_TYPE_PHYSICAL_ENGLISH || c.CampusTypeValueId == CAMPUS_TYPE_PHYSICAL_SPANISH ) && c.IsActive.Value ).ToList();
            cpCampus.SelectedCampusId = CurrentPerson.GetCampus()?.Id ?? 3;   // preset to home campus in case nothing has been set in the CR
            var campusGuid = SavedValues["Campus"].AsGuid();
            if ( campusGuid != Guid.Empty )                               // override with CR campus
            {
                cpCampus.SelectedCampusId = new CampusService( _context ).Get( campusGuid ).Id;
            }

            cblMinistries.DataSource = MinistriesOfInterest().ToList();
            cblMinistries.DataTextField = "Value";
            cblMinistries.DataValueField = "Key";
            cblMinistries.DataBind();
            cblMinistries.SetValues( SavedValues["MinistriesofInterest"].Split( ',' ) );

            tbOther.Text = SavedValues["OtherMinistry"];

            // Background Information Page
            lPageSubtitleBackground.Visible = false;
            rblChurchHome.SelectedValue = MapTrueFalseToYesNo( SavedValues["LPisChurchHome"] );
            tbChurchHome.Text = SavedValues["ChurchHome"];
            tbChurchYears.Text = SavedValues["HowLong"];
            ameHistory.AttributeMatrixGuid = SavedValues["ExperienceWithMinors"].AsGuid();
            tbMotivation.Text = SavedValues["MotivationforServing"];
            rblAgeGroupOrGender.SelectedValue = SavedValues["AgeOrGender"];
            rtbAgeGroupOrGender.Text = SavedValues["AgeOrGenderInfo"];

            rblHasAbused.SelectedValue = MapTrueFalseToYesNo( SavedValues["HasAbusedChild"] );
            rtbHasAbused.Text = SavedValues["HasAbusedChildInfo"];
            rblHasBeenAccused.SelectedValue = MapTrueFalseToYesNo( SavedValues["HasBeenAccused"] );
            rtbHasBeenAccused.Text = SavedValues["HasBeenAccusedInfo"];
            rblPsycho.SelectedValue = MapTrueFalseToYesNo( SavedValues["Psycho"] );
            rtbPsycho.Text = SavedValues["PsychoInfo"];
            rblHindrance.SelectedValue = MapTrueFalseToYesNo( SavedValues["Hinderence"] );
            rtbHindrance.Text = SavedValues["HinderenceInfo"];

            // References Page
            ameReferences.AttributeMatrixGuid = SavedValues["References"].AsGuid();

            // What We Believe Page
            rblBelief.SelectedValue = SavedValues["CoreBeliefs"];

            // Safety Policy for Minors Pages
            rcblAgree.SetValues( SavedValues["Agree"].Split( ',' ) );
            rblSSN.SelectedValue = MapTrueFalseToYesNo( SavedValues["HasSSN"] );

            rblInternationalStudent.SelectedValue = MapTrueFalseToYesNo( ConnectionRequest.AttributeValues.ContainsKey( "IsInternationalStudent" ) ? ConnectionRequest.AttributeValues["IsInternationalStudent"].Value : "No" );
            rtbSurname.Text = ConnectionRequest.AttributeValues.ContainsKey( "Surname" ) ? ConnectionRequest.AttributeValues["Surname"].Value : string.Empty;
            rtbGivenName.Text = ConnectionRequest.AttributeValues.ContainsKey( "GivenName" ) ? ConnectionRequest.AttributeValues["GivenName"].Value : string.Empty;
            rtbMothersMaidenName.Text = ConnectionRequest.AttributeValues.ContainsKey( "MothersFullMaidenName" ) ? ConnectionRequest.AttributeValues["MothersFullMaidenName"].Value : string.Empty;
            rtbOtherNames.Text = ConnectionRequest.AttributeValues.ContainsKey( "OtherNamesUsed" ) ? ConnectionRequest.AttributeValues["OtherNamesUsed"].Value : string.Empty;
            rtbPlaceOfBirth.Text = ConnectionRequest.AttributeValues.ContainsKey( "PlaceOfBirth" ) ? ConnectionRequest.AttributeValues["PlaceOfBirth"].Value : string.Empty;
            rtbCurrentAddress.Text = ConnectionRequest.AttributeValues.ContainsKey( "CurrentUSAddress" ) ? ConnectionRequest.AttributeValues["CurrentUSAddress"].Value : string.Empty;
            rtbYearsInTX.Text = ConnectionRequest.AttributeValues.ContainsKey( "NumberOfYearsInTexas" ) ? ConnectionRequest.AttributeValues["NumberOfYearsInTexas"].Value : string.Empty;
            rtbMonthsAtLakepointe.Text = ConnectionRequest.AttributeValues.ContainsKey( "MonthsAttendingLakepointeChurch" ) ? ConnectionRequest.AttributeValues["MonthsAttendingLakepointeChurch"].Value : string.Empty;
            rtbPriorAddress.Text = ConnectionRequest.AttributeValues.ContainsKey( "PriorAddressOut-of-USAddress" ) ? ConnectionRequest.AttributeValues["PriorAddressOut-of-USAddress"].Value : string.Empty;
            rtbSocialSecurityNumber.Text = ConnectionRequest.AttributeValues.ContainsKey( "NationalIDNumber" ) ? Encryption.DecryptString( ConnectionRequest.AttributeValues["NationalIDNumber"].Value ) : string.Empty;

            var frontId = GetImageId( "CopyOfPassport" );
            if ( frontId != null )
            {
                rfuIDFront.BinaryFileId = frontId.Value;
            }

            var dateString = ConnectionRequest.AttributeValues.ContainsKey( "DateOfBirth" ) ? ConnectionRequest.AttributeValues["DateOfBirth"].Value : string.Empty;
            if ( dateString != string.Empty )
            {
                dppDateOfBirth.SelectedDate = DateTime.Parse( dateString, null, System.Globalization.DateTimeStyles.RoundtripKind );
            }

            rcblAdultRelease.SetValues( SavedValues["Release"].Split( ',' ) );
        }

        private int? GetImageId( string key )
        {
            if ( !ConnectionRequest.AttributeValues.ContainsKey( key ) )
            {
                return null;
            }

            var guid = ConnectionRequest.AttributeValues[key].Value.AsGuidOrNull();
            if ( guid == null )
            {
                return null;
            }

            return new BinaryFileService( _context ).Get( guid.Value ).Id;
        }

        private void InitializeChildApplication()
        {
            // Language Selection Page
            ddlLanguage.SelectedValue = SavedValues["PreferredLanguage"];

            // Personal Information Page
            pnlParentNotification.Visible = true;
            pnlParentNotification.Enabled = true;
            cbParentNotification.Checked = SavedValues["ParentUnderstands"].Equals( "1" );
            ddlGender.BindToEnum<Gender>(); // false, new Gender[] { Gender.Unknown });
            tbFirstName.Text = CurrentPerson.NickName;
            tbLastName.Text = CurrentPerson.LastName;
            ddlGender.SelectedValue = ( ( int ) CurrentPerson.Gender ).ToString();
            var phone = CurrentPerson.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            tbPhone.Text = phone == null ? null : phone.NumberFormatted;
            tbEmail.Text = CurrentPerson.Email;
            dpBirthDate.SelectedDate = CurrentPerson.BirthDate;
            rblContactMethod.SelectedValue = SavedValues.ContainsKey( "PreferredContactMethod" ) ? SavedValues["PreferredContactMethod"] : null;

            BindMinors();
            ddlKidGender.BindToEnum<Gender>();
            if ( SavedValues["Student"].IsNullOrWhiteSpace() )
            {
                UpdateKidInfo( 0 );
            }
            else
            {
                var studentGuid = SavedValues["Student"].AsGuid();
                var studentId = new PersonAliasService( _context ).Queryable().AsNoTracking()
                    .Where( pa => pa.Guid == studentGuid ).FirstOrDefault().PersonId;
                UpdateKidInfo( studentId );
            }

            // Campus and Ministries Page
            cpCampus.Campuses = CampusCache.All().Where( c => ( c.CampusTypeValueId == CAMPUS_TYPE_PHYSICAL_ENGLISH || c.CampusTypeValueId == CAMPUS_TYPE_PHYSICAL_SPANISH ) && c.IsActive.Value ).ToList();
            cpCampus.SelectedCampusId = CurrentPerson.GetCampus()?.Id ?? 3;   // preset to home campus in case nothing has been set in the CR
            var campusGuid = SavedValues["Campus"].AsGuid();
            if ( campusGuid != Guid.Empty )                               // override with CR campus
            {
                cpCampus.SelectedCampusId = new CampusService( _context ).Get( campusGuid ).Id;
            }

            cblMinistries.DataSource = StudentMinistriesOfInterest( "KeysForMinistriesChildren" ).ToList();
            cblMinistries.DataTextField = "Value";
            cblMinistries.DataValueField = "Key";
            cblMinistries.DataBind();
            cblMinistries.SetValues( SavedValues["MinistriesofInterest"].Split( ',' ) );

            tbOther.Text = SavedValues["OtherMinistry"];

            // Background Information Page
            lPageSubtitleBackground.Text = Translation["SubtitleBackgroundChild"];
            rblChurchHome.SelectedValue = MapTrueFalseToYesNo( SavedValues["LPisChurchHome"] );
            tbChurchHome.Text = SavedValues["ChurchHome"];
            tbChurchYears.Visible = false;
            lGridDescription.Visible = false;
            ameHistory.Visible = false;
            tbMotivation.Text = SavedValues["MotivationforServing"];
            rblAgeGroupOrGender.Visible = false;
            rtbAgeGroupOrGender.Visible = false;

            rblHasAbused.Visible = false;
            rtbHasAbused.Visible = false;
            rblHasBeenAccused.Visible = false;
            rtbHasBeenAccused.Visible = false;
            rblPsycho.Visible = false;
            rtbPsycho.Visible = false;
            rblHindrance.SelectedValue = MapTrueFalseToYesNo( SavedValues["Hinderence"] );
            rtbHindrance.Text = SavedValues["HinderenceInfo"];

            // References Page
            ameReferences.AttributeMatrixGuid = SavedValues["References"].AsGuid();

            // What We Believe Page
            rblBelief.SelectedValue = SavedValues["CoreBeliefs"];

            // Safety Policy for Minors Pages
            rcblStudentAgreement.SetValues( SavedValues["StudentAgree"].Split( ',' ) );
            rcblNoticeOfRequirements.SetValues( SavedValues["NoticeOfRequirements"].Split( ',' ) );
            rcblParentGuardianApproval.SetValues( SavedValues["ParentApproval"].Split( ',' ) );
            rblSSN.SelectedValue = MapTrueFalseToYesNo( SavedValues["HasSSN"] );
            rblInternationalStudent.SelectedValue = MapTrueFalseToYesNo( ConnectionRequest.AttributeValues["IsInternationalStudent"].Value ) ?? "No";
            rcblMinorRelease.SetValues( SavedValues["Release"].Split( ',' ) );
            rtbMinorSignature.Text = SavedValues["MinorSignature"];
        }

        private void InitializeTeenApplication()
        {
            // Language Selection Page
            ddlLanguage.SelectedValue = SavedValues["PreferredLanguage"];

            // Personal Information Page
            pnlParentNotification.Visible = true;
            pnlParentNotification.Enabled = true;
            cbParentNotification.Checked = SavedValues["ParentUnderstands"].Equals( "1" );
            ddlGender.BindToEnum<Gender>(); // false, new Gender[] { Gender.Unknown });
            tbFirstName.Text = CurrentPerson.NickName;
            tbLastName.Text = CurrentPerson.LastName;
            ddlGender.SelectedValue = ( ( int ) CurrentPerson.Gender ).ToString();
            var phone = CurrentPerson.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            tbPhone.Text = phone == null ? null : phone.NumberFormatted;
            tbEmail.Text = CurrentPerson.Email;
            dpBirthDate.SelectedDate = CurrentPerson.BirthDate;
            rblContactMethod.SelectedValue = SavedValues.ContainsKey( "PreferredContactMethod" ) ? SavedValues["PreferredContactMethod"] : null;

            BindMinors();
            ddlKidGender.BindToEnum<Gender>();
            if ( SavedValues["Student"].IsNullOrWhiteSpace() )
            {
                UpdateKidInfo( 0 );
            }
            else
            {
                var studentGuid = SavedValues["Student"].AsGuid();
                var studentId = new PersonAliasService( _context ).Queryable().AsNoTracking()
                    .Where( pa => pa.Guid == studentGuid ).FirstOrDefault().PersonId;
                UpdateKidInfo( studentId );
            }

            // Campus and Ministries Page
            cpCampus.Campuses = CampusCache.All().Where( c => ( c.CampusTypeValueId == CAMPUS_TYPE_PHYSICAL_ENGLISH || c.CampusTypeValueId == CAMPUS_TYPE_PHYSICAL_SPANISH ) && c.IsActive.Value ).ToList();
            cpCampus.SelectedCampusId = CurrentPerson.GetCampus()?.Id ?? 3;   // preset to home campus in case nothing has been set in the CR
            var campusGuid = SavedValues["Campus"].AsGuid();
            if ( campusGuid != Guid.Empty )                               // override with CR campus
            {
                cpCampus.SelectedCampusId = new CampusService( _context ).Get( campusGuid ).Id;
            }

            cblMinistries.DataSource = StudentMinistriesOfInterest( "KeysForMinistriesStudents" ).ToList();
            cblMinistries.DataTextField = "Value";
            cblMinistries.DataValueField = "Key";
            cblMinistries.DataBind();
            cblMinistries.SetValues( SavedValues["MinistriesofInterest"].Split( ',' ) );

            tbOther.Text = SavedValues["OtherMinistry"];

            // Background Information Page
            lPageSubtitleBackground.Text = Translation["SubtitleBackgroundStudent"];
            rblChurchHome.SelectedValue = MapTrueFalseToYesNo( SavedValues["LPisChurchHome"] );
            tbChurchHome.Text = SavedValues["ChurchHome"];
            tbChurchYears.Text = SavedValues["HowLong"];
            lGridDescription.Visible = false;
            ameHistory.Visible = false;
            tbMotivation.Text = SavedValues["MotivationforServing"];
            rblAgeGroupOrGender.SelectedValue = SavedValues["AgeOrGender"];
            rtbAgeGroupOrGender.Text = SavedValues["AgeOrGenderInfo"];

            rblHasAbused.SelectedValue = MapTrueFalseToYesNo( SavedValues["HasAbusedChild"] );
            rtbHasAbused.Text = SavedValues["HasAbusedChildInfo"];
            rblHasBeenAccused.SelectedValue = MapTrueFalseToYesNo( SavedValues["HasBeenAccused"] );
            rtbHasBeenAccused.Text = SavedValues["HasBeenAccusedInfo"];
            rblPsycho.SelectedValue = MapTrueFalseToYesNo( SavedValues["Psycho"] );
            rtbPsycho.Text = SavedValues["PsychoInfo"];
            rblHindrance.SelectedValue = MapTrueFalseToYesNo( SavedValues["Hinderence"] );
            rtbHindrance.Text = SavedValues["HinderenceInfo"];

            // References Page
            ameReferences.AttributeMatrixGuid = SavedValues["References"].AsGuid();

            // What We Believe Page
            rblBelief.SelectedValue = SavedValues["CoreBeliefs"];

            // Safety Policy for Minors Pages
            rcblStudentAgreement.SetValues( SavedValues["StudentAgree"].Split( ',' ) );
            rcblNoticeOfRequirements.SetValues( SavedValues["NoticeOfRequirements"].Split( ',' ) );
            rcblParentGuardianApproval.SetValues( SavedValues["ParentApproval"].Split( ',' ) );
            rblSSN.SelectedValue = MapTrueFalseToYesNo( SavedValues["HasSSN"] );
            rblInternationalStudent.SelectedValue = MapTrueFalseToYesNo( ConnectionRequest.AttributeValues["IsInternationalStudent"].Value ) ?? "No";
            rcblMinorRelease.SetValues( SavedValues["Release"].Split( ',' ) );
            rtbMinorSignature.Text = SavedValues["MinorSignature"];
        }

        private void UpdateKidInfo( int studentId )
        {
            if ( studentId == 0 )
            {
                ddlMinor.SelectedValue = null;
                tbKidFirstName.Text = string.Empty;
                tbKidLastName.Text = string.Empty;
                ddlKidGender.SelectedValue = "0";
                tbKidPhone.Text = string.Empty;
                tbKidEmail.Text = string.Empty;
                dpKidBirthDate.SelectedDate = null;
            }
            else
            {
                Person student = new PersonService( _context ).Queryable().AsNoTracking()
                    .Where( p => p.Id == studentId )
                    .FirstOrDefault();
                ddlMinor.SelectedValue = student.Id.ToString();
                tbKidFirstName.Text = student.NickName;
                tbKidLastName.Text = student.LastName;
                ddlKidGender.SelectedValue = ( ( int ) student.Gender ).ToString();
                ddlKidGender.Enabled = student.Gender == Gender.Unknown ? true : false;
                var number = student.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                tbKidPhone.Text = number == null ? null : number.NumberFormatted;
                tbKidEmail.Text = student.Email;
                dpKidBirthDate.SelectedDate = student.BirthDate;
            }
        }

        private void InitializeExistingVolunteerApplication()
        {
            // Language Selection Page
            ddlLanguage.SelectedValue = SavedValues["PreferredLanguage"];

            // Personal Information Page
            ddlGender.BindToEnum<Gender>(); // false, new Gender[] { Gender.Unknown });

            tbFirstName.Text = CurrentPerson.NickName;
            tbLastName.Text = CurrentPerson.LastName;
            ddlGender.SelectedValue = ( ( int ) CurrentPerson.Gender ).ToString();
            var phone = CurrentPerson.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            if ( phone != null )
            {
                tbPhone.Text = phone.NumberFormatted;
            }
            tbEmail.Text = CurrentPerson.Email;
            dpBirthDate.SelectedDate = CurrentPerson.BirthDate;
            rblContactMethod.SelectedValue = SavedValues.ContainsKey( "PreferredContactMethod" ) ? SavedValues["PreferredContactMethod"] : null;

            // What We Believe Page
            rblBelief.SelectedValue = SavedValues["CoreBeliefs"];

            // Safety Policy for Minors Pages
            rcblAgree.SetValues( SavedValues["Agree"].Split( ',' ) );
            if ( ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.STAFF )
            {
                rblSSN.SelectedValue = MapTrueFalseToYesNo( SavedValues["HasSSN"] );
            }
            if ( ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.LEVEL2 )
            {
                rblInternationalStudent.SelectedValue = MapTrueFalseToYesNo( ConnectionRequest.AttributeValues["IsInternationalStudent"].Value ) ?? "No";
            }
            rcblAdultRelease.SetValues( SavedValues["Release"].Split( ',' ) );
        }

        private void Navigate( Direction direction )
        {
            var connectionRequestAttributes = ConnectionRequest.AssignedGroupMemberAttributeValues;
            if ( connectionRequestAttributes.IsNotNullOrWhiteSpace() )
            {
                SavedValues = JsonConvert.DeserializeObject<Dictionary<string, string>>( connectionRequestAttributes );

                PageNames nextPage = PageSequence.Next( CurrentPage ); // until proven otherwise
                switch ( CurrentPage )
                {
                    case PageNames.LANGUAGE:
                        SavedValues["PreferredLanguage"] = ddlLanguage.SelectedValue;
                        break;
                    case PageNames.PERSONAL_INFORMATION:
                        SavedValues["PreferredContactMethod"] = rblContactMethod.SelectedValue;
                        if ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD )
                        {
                            SavedValues["ParentUnderstands"] = cbParentNotification.Checked ? "1" : "";

                            var studentId = ddlMinor.SelectedValue.AsInteger();
                            UpdateStudentRecord( studentId );
                            SavedValues["Student"] = studentId == 0 ? null : new PersonAliasService( _context ).Queryable().AsNoTracking().Where( pa => pa.PersonId == studentId ).FirstOrDefault().Guid.ToString();
                        }
                        break;
                    case PageNames.MINISTRY_INFORMATION:
                        SavedValues["Campus"] = new CampusService( _context ).Get( cpCampus.SelectedValueAsId().Value ).Guid.ToString();
                        SavedValues["MinistriesofInterest"] = cblMinistries.SelectedValues.JoinStrings( "," );
                        SavedValues["OtherMinistry"] = tbOther.Text;
                        break;
                    case PageNames.BACKGROUND:
                        SavedValues["LPisChurchHome"] = MapYesNoToTrueFalse( rblChurchHome.SelectedValue );
                        SavedValues["ChurchHome"] = tbChurchHome.Text;
                        if ( ApplicationType != ApplicationTypes.CHILD )
                        {
                            SavedValues["HowLong"] = tbChurchYears.Text;
                        }
                        if ( ApplicationType != ApplicationTypes.CHILD && ApplicationType != ApplicationTypes.TEEN && ApplicationType != ApplicationTypes.RECENT_TEEN )
                        {
                            SavedValues["ExperienceWithMinors"] = ameHistory.AttributeMatrixGuid.Value.ToString();
                        }
                        SavedValues["MotivationforServing"] = tbMotivation.Text;
                        if ( ApplicationType != ApplicationTypes.CHILD )
                        {
                            SavedValues["AgeOrGender"] = rblAgeGroupOrGender.SelectedValue;
                            SavedValues["AgeOrGenderInfo"] = rtbAgeGroupOrGender.Text;
                        }
                        SavedValues["HasAbusedChild"] = MapYesNoToTrueFalse( rblHasAbused.SelectedValue );
                        SavedValues["HasAbusedChildInfo"] = rtbHasAbused.Text;
                        SavedValues["HasBeenAccused"] = MapYesNoToTrueFalse( rblHasBeenAccused.SelectedValue );
                        SavedValues["HasBeenAccusedInfo"] = rtbHasBeenAccused.Text;
                        SavedValues["Psycho"] = MapYesNoToTrueFalse( rblPsycho.SelectedValue );
                        SavedValues["PsychoInfo"] = rtbPsycho.Text;
                        SavedValues["Hinderence"] = MapYesNoToTrueFalse( rblHindrance.SelectedValue );
                        SavedValues["HinderenceInfo"] = rtbHindrance.Text;

                        if ( rblHasAbused.SelectedValue.Equals( "Yes" )
                            || rblHasBeenAccused.SelectedValue.Equals( "Yes" )
                            || rblPsycho.SelectedValue.Equals( "Yes" )
                            || rblHindrance.SelectedValue.Equals( "Yes" ))
                        {
                            SavedValues["Sensitive"] = "True";
                        }
                        else
                        {
                            SavedValues["Sensitive"] = "False";
                        }
                        break;
                    case PageNames.PERSONAL_HISTORY:
                        SavedValues["References"] = ameReferences.AttributeMatrixGuid.Value.ToString();
                        break;
                    case PageNames.CORE_BELIEFS:
                        SavedValues["CoreBeliefs"] = rblBelief.SelectedValue;
                        break;
                    case PageNames.SAFETY_POLICY_7:
                        var flagged = false;
                        switch ( ApplicationType )
                        {
                            case ApplicationTypes.LEVEL0:
                                SavedValues["Agree"] = rcblAgreeL0.SelectedValues.JoinStrings(",");
                                break;
                            case ApplicationTypes.LEVEL2:
                                SavedValues["Agree"] = rcblAgreeL2.SelectedValues.JoinStrings( "," );
                                SavedValues["HasSSN"] = MapYesNoToTrueFalse( rblSSN.SelectedValue );
                                ConnectionRequest.AttributeValues["IsInternationalStudent"].Value = MapYesNoToTrueFalse( rblInternationalStudent.SelectedValue );
                                ConnectionRequest.AttributeValues["Surname"].Value = rtbSurname.Text;
                                ConnectionRequest.AttributeValues["GivenName"].Value = rtbGivenName.Text;
                                ConnectionRequest.AttributeValues["MothersFullMaidenName"].Value = rtbMothersMaidenName.Text;
                                ConnectionRequest.AttributeValues["OtherNamesUsed"].Value = rtbOtherNames.Text;
                                ConnectionRequest.AttributeValues["PlaceOfBirth"].Value = rtbPlaceOfBirth.Text;
                                ConnectionRequest.AttributeValues["DateOfBirth"].Value = dppDateOfBirth.SelectedDate.ToISO8601DateString();
                                ConnectionRequest.AttributeValues["CurrentUSAddress"].Value = rtbCurrentAddress.Text;
                                ConnectionRequest.AttributeValues["NumberOfYearsInTexas"].Value = rtbYearsInTX.Text;
                                ConnectionRequest.AttributeValues["MonthsAttendingLakepointeChurch"].Value = rtbMonthsAtLakepointe.Text;
                                ConnectionRequest.AttributeValues["PriorAddressOut-of-USAddress"].Value = rtbPriorAddress.Text;
                                ConnectionRequest.AttributeValues["NationalIDNumber"].Value = Encryption.EncryptString( rtbSocialSecurityNumber.Text );
                                ConnectionRequest.AttributeValues["CopyOfPassport"].Value = GetGuidOfBinaryFile( rfuIDFront.BinaryFileId );
                                ConnectionRequest.SaveAttributeValues();
                                SavedValues["Release"] = rcblAdultRelease.SelectedValues.JoinStrings( "," );
                                break;
                            case ApplicationTypes.ADULT:
                            case ApplicationTypes.STAFF:
                                SavedValues["Agree"] = rcblAgree.SelectedValues.JoinStrings( "," );
                                SavedValues["HasSSN"] = MapYesNoToTrueFalse( rblSSN.SelectedValue );
                                ConnectionRequest.AttributeValues["IsInternationalStudent"].Value = MapYesNoToTrueFalse( rblInternationalStudent.SelectedValue );
                                ConnectionRequest.AttributeValues["Surname"].Value = rtbSurname.Text;
                                ConnectionRequest.AttributeValues["GivenName"].Value = rtbGivenName.Text;
                                ConnectionRequest.AttributeValues["MothersFullMaidenName"].Value = rtbMothersMaidenName.Text;
                                ConnectionRequest.AttributeValues["OtherNamesUsed"].Value = rtbOtherNames.Text;
                                ConnectionRequest.AttributeValues["PlaceOfBirth"].Value = rtbPlaceOfBirth.Text;
                                ConnectionRequest.AttributeValues["DateOfBirth"].Value = dppDateOfBirth.SelectedDate.ToISO8601DateString();
                                ConnectionRequest.AttributeValues["CurrentUSAddress"].Value = rtbCurrentAddress.Text;
                                ConnectionRequest.AttributeValues["NumberOfYearsInTexas"].Value = rtbYearsInTX.Text;
                                ConnectionRequest.AttributeValues["MonthsAttendingLakepointeChurch"].Value = rtbMonthsAtLakepointe.Text;
                                ConnectionRequest.AttributeValues["PriorAddressOut-of-USAddress"].Value = rtbPriorAddress.Text;
                                ConnectionRequest.AttributeValues["NationalIDNumber"].Value = Encryption.EncryptString( rtbSocialSecurityNumber.Text );
                                ConnectionRequest.AttributeValues["CopyOfPassport"].Value = GetGuidOfBinaryFile( rfuIDFront.BinaryFileId );
                                ConnectionRequest.SaveAttributeValues();
                                SavedValues["Release"] = rcblAdultRelease.SelectedValues.JoinStrings( "," );
                                flagged = ( SavedValues["FollowRules"] == "False" ) ||
                                    ( SavedValues["HasAbusedChild"] == "True" ) ||
                                    ( SavedValues["CoreBeliefs"] == "2" ) ||
                                    ( SavedValues["Agree"] != "1,2,3" ) ||
                                    ( SavedValues["Release"] != "1,2,3,4" );
                                break;
                            case ApplicationTypes.EXISTING_VOLUNTEER:
                                SavedValues["Agree"] = rcblAgree.SelectedValues.JoinStrings( "," );
                                SavedValues["HasSSN"] = MapYesNoToTrueFalse( rblSSN.SelectedValue );
                                SavedValues["Release"] = rcblAdultRelease.SelectedValues.JoinStrings( "," );
                                flagged = ( SavedValues["CoreBeliefs"] == "2" ) ||
                                    ( SavedValues["Agree"] != "1,2,3" ) ||
                                    ( SavedValues["Release"] != "1,2,3,4" );
                                break;
                            case ApplicationTypes.TEEN:
                            case ApplicationTypes.RECENT_TEEN:
                                SavedValues["StudentAgree"] = rcblStudentAgreement.SelectedValues.JoinStrings( "," );
                                SavedValues["NoticeOfRequirements"] = rcblNoticeOfRequirements.SelectedValues.JoinStrings( "," );
                                SavedValues["ParentApproval"] = rcblParentGuardianApproval.SelectedValues.JoinStrings( "," );
                                SavedValues["Release"] = rcblMinorRelease.SelectedValues.JoinStrings( "," );
                                SavedValues["MinorSignature"] = rtbMinorSignature.Text;
                                flagged = ( SavedValues["FollowRules"] == "False" ) ||
                                    ( SavedValues["HasAbusedChild"] == "True" ) ||
                                    ( SavedValues["CoreBeliefs"] == "2" ) ||
                                    ( SavedValues["ParentApproval"] != "1,2,3,4,5" ) ||
                                    ( SavedValues["Release"] != "1,3,4" );
                                break;
                            case ApplicationTypes.CHILD:
                                SavedValues["StudentAgree"] = rcblStudentAgreement.SelectedValues.JoinStrings( "," );
                                SavedValues["NoticeOfRequirements"] = rcblNoticeOfRequirements.SelectedValues.JoinStrings( "," );
                                SavedValues["ParentApproval"] = rcblParentGuardianApproval.SelectedValues.JoinStrings( "," );
                                SavedValues["Release"] = rcblMinorRelease.SelectedValues.JoinStrings( "," );
                                SavedValues["MinorSignature"] = rtbMinorSignature.Text;
                                flagged = ( SavedValues["FollowRules"] == "False" ) ||
                                    ( SavedValues["CoreBeliefs"] == "2" ) ||
                                    ( SavedValues["ParentApproval"] != "1,2,3,4,5" ) ||
                                    ( SavedValues["Release"] != "1,3,4" );
                                break;
                        }

                        if ( direction == Direction.FORWARD )
                        {
                            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson,
                            new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );

                            mergeFields.Add( "requiresBackgroundCheck", IsRequiresBackgroundCheck( ConnectionRequest ) ? "yes" : "no" );
                            if ( ApplicationType != ApplicationTypes.LEVEL0 )
                            {
                                var hasSSN = MapTrueFalseToYesNo( SavedValues["HasSSN"] ) ?? "no";
                                mergeFields.Add( "hasSSN", hasSSN.ToLower() );
                            }
                            ConnectionRequest.ConnectionStatusId = VOLUNTEER_ONBOARDING_CONNECTION_STATUS_SUBMITTED;
                            if ( flagged )
                            {
                                lConfirmationMessage.Text = Translation["WillBeContactedMessage"].ResolveMergeFields( mergeFields );
                            }
                            else // Otherwise, check to see if it needs to be submitted for signature
                            {
                                switch ( ApplicationType )
                                {
                                    case ApplicationTypes.LEVEL0:
                                        lConfirmationMessage.Text = Translation["ConfirmationMessageLevel0"].ResolveMergeFields( mergeFields );
                                        break;
                                    case ApplicationTypes.LEVEL2:
                                        lConfirmationMessage.Text = Translation["ConfirmationMessageLevel2"].ResolveMergeFields( mergeFields );
                                        break;
                                    case ApplicationTypes.STAFF:
                                    case ApplicationTypes.ADULT:
                                        lConfirmationMessage.Text = Translation["ConfirmationMessageAdult"].ResolveMergeFields( mergeFields );
                                        break;
                                    case ApplicationTypes.EXISTING_VOLUNTEER:
                                        lConfirmationMessage.Text = Translation["ConfirmationMessageExistingVolunteer"].ResolveMergeFields( mergeFields );
                                        break;
                                    case ApplicationTypes.TEEN:
                                    case ApplicationTypes.RECENT_TEEN:
                                        lConfirmationMessage.Text = Translation["ConfirmationMessageMinor"].ResolveMergeFields( mergeFields );
                                        break;
                                    case ApplicationTypes.CHILD:
                                        lConfirmationMessage.Text = Translation["ConfirmationMessageChild"].ResolveMergeFields( mergeFields );
                                        break;
                                }
                            }
                        }
                        break;
                    case PageNames.CONFIRMATION:
                        nextPage = CurrentPage; // don't advance beyond this page ... it's the last page (shouldn't get here anyway)
                        break;
                    default:
                        // Nothing to see here
                        break;
                }

                nextPage = ( direction == Direction.BACKWARD ) ? PageSequence.Previous( CurrentPage ) : nextPage;
                SavedValues["LastPageViewed"] = nextPage.ToString();

                if ( SavedValues.ContainsKey( "ApplicationStartTimestamp" ) )
                {
                    if ( SavedValues["ApplicationStartTimestamp"].IsNullOrWhiteSpace() )
                    {
                        SavedValues["ApplicationStartTimestamp"] = RockDateTime.Now.ToISO8601DateString();
                    }
                }
                else
                {
                    SavedValues.Add( "ApplicationStartTimestamp", RockDateTime.Now.ToISO8601DateString() );
                }

                ConnectionRequest.AssignedGroupMemberAttributeValues = JsonConvert.SerializeObject( SavedValues );

                if ( ApplicationType != ApplicationTypes.LEVEL2 && ApplicationType != ApplicationTypes.LEVEL0 )
                {
                    // The following moves the data in and out of the Sensitive opportunity depending on answers to questions.
                    // Note this doesn't apply to "staff" applications that have restricted visibility regardless of answers to questions.
                    if ( SavedValues["Sensitive"].Equals( "True" ) )
                    {
                        if ( ConnectionRequest.ConnectionOpportunityId == 227 ) // sensitive but not yet moved to Sensitive or staff opportunity type
                        {
                            ConnectionRequest.ConnectionOpportunityId = 280; // move to Sensitive opportunity
                        }
                    }
                    else
                    {
                        if ( ConnectionRequest.ConnectionOpportunityId == 280 ) // marked sensitive, but is no longer sensitive
                        {
                            ConnectionRequest.ConnectionOpportunityId = 227; // move to normal volunteer opportunity
                        }
                    }
                }

                _context.SaveChanges();

                SelectPage( nextPage );
            }
        }

        private string GetGuidOfBinaryFile( int? id )
        {
            if ( id == null )
            {
                return null;
            }

            var bfs = new BinaryFileService( _context );
            return bfs.Get( id.Value ).Guid.ToString();
        }

        private void SaveApplication( Guid? binaryFileGuid, Person applicant )
        {
            int id = 0;
            string key = null;

            switch ( ApplicationType )
            {
                case ApplicationTypes.LEVEL0:
                    id = 1014494;
                    key = "Level0VolunteerApplicationDocument";
                    break;
                case ApplicationTypes.LEVEL2:
                    id = 84794;
                    key = "Level2VolunteerApplicationDocument";
                    break;
                case ApplicationTypes.EXISTING_VOLUNTEER:
                case ApplicationTypes.ADULT:
                    id = 74001;
                    key = "VolunteerApplicationDocument";
                    break;
                case ApplicationTypes.CHILD:
                    id = 76484;
                    key = "ChildVolunteerApplicationDocument";
                    break;
                case ApplicationTypes.TEEN:
                case ApplicationTypes.RECENT_TEEN:
                    id = 76483;
                    key = "TeenVolunteerApplicationDocument";
                    break;
                case ApplicationTypes.STAFF:
                    id = 76485;
                    key = "StaffVolunteerApplicationDocument";
                    break;
            }

            if ( applicant.AttributeValues.ContainsKey( key ) )
            {
                if ( binaryFileGuid.HasValue)
                {
                    // If there is an existing attribute, there may be an existing signature document.
                    var binaryFileId = new BinaryFileService(_context).Get(binaryFileGuid.Value).Id;
                    var signatureDocumentService = new SignatureDocumentService(_context);
                    var oldSignatureDocument = signatureDocumentService.Queryable().AsNoTracking().Where(sd => sd.BinaryFileId == binaryFileId).FirstOrDefault();
                    if (oldSignatureDocument != null)
                    {
                        // This will need to be deleted first before we can free up the old binary file.
                        signatureDocumentService.Attach(oldSignatureDocument);
                        signatureDocumentService.Delete(oldSignatureDocument);
                    }
                }

                applicant.AttributeValues[key].Value = binaryFileGuid.ToStringSafe();
                applicant.SaveAttributeValue( key, _context );
            }
            else
            {
                var volunteerApplicationDocument = new AttributeValue
                {
                    AttributeId = id,
                    EntityId = applicant.Id,
                    Value = binaryFileGuid.ToStringSafe()
                };
                new AttributeValueService( _context ).Add( volunteerApplicationDocument );
            }
        }

        private string GetApplicationPdfGuid( Person applicant )
        {
            string key = null;

            switch ( ApplicationType )
            {
                case ApplicationTypes.LEVEL0:
                    key = "Level0VolunteerApplicationDocument";
                    break;
                case ApplicationTypes.LEVEL2:
                    key = "Level2VolunteerApplicationDocument";
                    break;
                case ApplicationTypes.EXISTING_VOLUNTEER:
                case ApplicationTypes.ADULT:
                    key = "VolunteerApplicationDocument";
                    break;
                case ApplicationTypes.CHILD:
                    key = "ChildVolunteerApplicationDocument";
                    break;
                case ApplicationTypes.TEEN:
                case ApplicationTypes.RECENT_TEEN:
                    key = "TeenVolunteerApplicationDocument";
                    break;
                case ApplicationTypes.STAFF:
                    key = "StaffVolunteerApplicationDocument";
                    break;
            }

            if ( applicant.AttributeValues == null )
            {
                applicant.LoadAttributes();
            }

            if ( applicant.AttributeValues.ContainsKey( key ) )
            {
                return applicant.AttributeValues[key].Value;
            }
            else
            {
                return null;
            }
        }

        private void SetApplicationDate( Person applicant )
        {
            string key = "VolunteerApplicationDate";
            int id = 75985;

            if ( applicant.AttributeValues.ContainsKey( key ) )
            {
                applicant.AttributeValues[key].Value = DateTime.Now.ToISO8601DateString();
                applicant.SaveAttributeValue( key, _context );
            }
            else
            {
                var volunteerApplicationDocument = new AttributeValue
                {
                    AttributeId = id,
                    EntityId = applicant.Id,
                    Value = DateTime.Now.ToISO8601DateString()
                };
                new AttributeValueService( _context ).Add( volunteerApplicationDocument );
            }
        }

        private void UpdateStudentRecord( int studentId )
        {
            Person student = new PersonService( _context ).Queryable()
                .Where( p => p.Id == studentId )
                .FirstOrDefault();

            bool isDirty = false;

            if ( !ddlKidGender.SelectedValue.Equals( ( ( int ) student.Gender ).ToString() ) )
            {
                student.Gender = ( Gender ) ddlKidGender.SelectedValue.AsInteger();
                isDirty = true;
            }

            var studentNumber = student.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            var studentNumberText = studentNumber == null ? null : studentNumber.NumberFormatted;
            if ( !tbKidPhone.Text.Equals( studentNumberText ) )
            {
                var numberTypeValueMobile = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                student.UpdatePhoneNumber( numberTypeValueMobile.Id, null, tbKidPhone.Text, true, false, _context );
            }

            if ( !tbKidEmail.Text.Equals( student.Email ) )
            {
                student.Email = tbKidEmail.Text;
                isDirty = true;
            }

            if ( dpKidBirthDate.SelectedDate != student.BirthDate )
            {
                student.SetBirthDate( dpKidBirthDate.SelectedDate );
                isDirty = true;
            }

            if ( isDirty )
            {
                _context.SaveChanges();
            }
        }

        private bool IsRequiresBackgroundCheck( ConnectionRequest connectionRequest )
        {
            if ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD )
            {
                return false; // can't run background check on minors
            }

            if ( ApplicationType == ApplicationTypes.LEVEL0 )
            {
                return false; // not required for L0
            }

            if ( connectionRequest == null )
            {
                return true; // shouldn't ever hit this
            }

            var applicant = connectionRequest.PersonAlias.Person;
            applicant.LoadAttributes();
            string bc = null;
            if ( applicant.AttributeValues.ContainsKey( "BackgroundCheckExpireDate" ) )
            {
                bc = applicant.AttributeValues["BackgroundCheckExpireDate"].Value;
            }

            if ( bc == null && applicant.AttributeValues.ContainsKey( "Arena-29-279" ) )
            {
                bc = applicant.AttributeValues["Arena-29-279"].Value;
            }

            if ( bc == null )
            {
                return true;
            }

            var expiration = bc.AsDateTime();
            if ( !expiration.HasValue )
            {
                return true;
            }
            return expiration < RockDateTime.Today.AddDays( 60 ); // If it's going to expire in the next 60 days, require one anyway
        }

        // For a future refactor: Make ApplicationType a virtual class with derivations for each type that has properties for each of the visibility questions below.
        private void SelectPage( PageNames pageIndex )
        {
            pnlParentNotification.Visible = ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD;
            lSectionTitleParentInfo.Visible = ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD;
            rblSSN.Visible = ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.STAFF || ApplicationType == ApplicationTypes.LEVEL2;
            rblSSN.Required = ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.STAFF || ApplicationType == ApplicationTypes.LEVEL2;
            rblInternationalStudent.Visible = ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.LEVEL2;

            bool isInternationalStudent = ConnectionRequest.AttributeValues.ContainsKey( "IsInternationalStudent" ) ? ConnectionRequest.AttributeValues["IsInternationalStudent"].Value == "True" : false;

            if ( Request.Params.AllKeys.Contains( "PDF" ) )
            {
                // Make everything we want in the PDF visible.
                // But also disable all the controls so they can't change anything in the document they're about to sign.

                lProgress.Visible = false;

                pnlLanguageChoice.Visible = false;

                pnlPersonalInfo.Visible = true;
                cbParentNotification.Enabled = false;
                PersonalInformationInstructions.Visible = false;
                rblContactMethod.Enabled = false;

                pnlMinorInfo.Visible = ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD;
                ddlMinor.Enabled = false;
                ddlKidGender.Enabled = false;
                tbKidPhone.Enabled = false;
                tbKidEmail.Enabled = false;
                dpKidBirthDate.Enabled = false;

                pnlVolunteerOpportunitiesForMinors.Visible = false;

                pnlMinistry.Visible = ApplicationType != ApplicationTypes.EXISTING_VOLUNTEER;
                cpCampus.Enabled= false;
                cblMinistries.Enabled= false;
                tbOther.Enabled = false;

                pnlBackground.Visible = ApplicationType != ApplicationTypes.EXISTING_VOLUNTEER && ApplicationType != ApplicationTypes.LEVEL2 && ApplicationType != ApplicationTypes.LEVEL0;
                rblChurchHome.Enabled = false;
                tbChurchYears.Enabled = false;
                tbChurchHome.Enabled = false;
                ameHistory.Enabled = false;
                tbMotivation.Enabled = false;
                rblAgeGroupOrGender.Enabled = false;
                rtbAgeGroupOrGender.Enabled = false;
                rblHasAbused.Enabled = false;
                rtbHasAbused.Enabled = false;
                rblHasBeenAccused.Enabled = false;
                rtbHasBeenAccused.Enabled = false;
                rblPsycho.Enabled = false;
                rtbPsycho.Enabled = false;
                rblHindrance.Enabled = false;
                rtbHindrance.Enabled = false;

                pnlPersonalHistory.Visible = ApplicationType != ApplicationTypes.EXISTING_VOLUNTEER && ApplicationType != ApplicationTypes.RECENT_TEEN && ApplicationType != ApplicationTypes.STAFF && ApplicationType != ApplicationTypes.LEVEL0;
                ameReferences.Enabled = false;

                pnlCoreBeliefs.Visible = true;
                rblBelief.Enabled = false;

                pnlSafetyPolicy1.Visible = ApplicationType != ApplicationTypes.LEVEL2 && ApplicationType != ApplicationTypes.LEVEL0;

                pnlSafetyPolicy7.Visible = true;
                pnlStudentVolunteerAgreement.Visible = ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD;
                rcblStudentAgreement.Enabled = false;
                rcblNoticeOfRequirements.Enabled = false;
                rcblParentGuardianApproval.Enabled = false;

                pnlAckAndAg.Visible = ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.EXISTING_VOLUNTEER || ApplicationType == ApplicationTypes.STAFF;
                rcblAgree.Enabled = false;

                pnlAckAndAgLevel2.Visible = ApplicationType == ApplicationTypes.LEVEL2;
                rcblAgreeL2.Enabled = false;

                pnlAckAndAgLevel0.Visible = ApplicationType == ApplicationTypes.LEVEL0;
                rcblAgreeL0.Enabled = false;

                pnlAdultRelease.Visible = ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.EXISTING_VOLUNTEER || ApplicationType == ApplicationTypes.STAFF || ApplicationType == ApplicationTypes.LEVEL2;
                pnlSSN.Visible = ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.EXISTING_VOLUNTEER || ApplicationType == ApplicationTypes.STAFF || ApplicationType == ApplicationTypes.LEVEL2;
                rblSSN.Enabled = false;
                pnlInternationalStudent.Visible = ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.LEVEL2;
                rblInternationalStudent.Enabled = false;
                pnlInternationalStudentDetails.Visible = false;
                if ( isInternationalStudent )
                {
                    pnlInternationalStudentDetails.Visible = ( ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.LEVEL2 );
                    rtbSurname.Enabled = false;
                    rtbGivenName.Enabled = false;
                    rtbMothersMaidenName.Enabled = false;
                    rtbOtherNames.Enabled = false;
                    rtbPlaceOfBirth.Enabled = false;
                    dppDateOfBirth.Enabled = false;
                    rtbCurrentAddress.Enabled = false;
                    rtbYearsInTX.Enabled = false;
                    rtbMonthsAtLakepointe.Enabled = false;
                    rtbPriorAddress.Enabled = false;
                    rfuIDFront.Visible = false;     // don't include images of ID in the PDF
                    rtbSocialSecurityNumber.Visible = false; // don't include the SSN in the PDF
                }

                rcblAdultRelease.Enabled = false;

                pnlMinorRelease.Visible = ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD;
                rcblMinorRelease.Enabled = false;
                rtbMinorSignature.Enabled = false;

                pnlSignature.Visible = false;
                pnlConfirmation.Visible = false;
                pnlNavigation.Visible = false;

                if ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD )
                {
                    var studentGuid = SavedValues["Student"].AsGuid();
                    if ( studentGuid == Guid.Empty ) // happens if we haven't picked a student yet
                    {
                        return;
                    }

                    var student = new PersonAliasService( _context ).Queryable().AsNoTracking()
                        .Where( pa => pa.Guid == studentGuid ).FirstOrDefault().Person;

                    lApplicantFullName.Text = Translation["ApplicantFullName"] + student.FullName;
                    lParentGuardianFullName.Text = Translation["ParentGuardianFullName"] + ConnectionRequest.PersonAlias.Person.FullName;
                    lSignatureInstructionsPDF.Visible = true;
                }
            }
            else
            {
                int pageNumber = PageSequence.IndexOf( pageIndex ) + 1;  // zero-based
                int numberOfPages = PageSequence.Count;
                int percentComplete = pageNumber * 100 / numberOfPages;
                lProgress.Text = string.Format( @"<div class=""progress""><div class=""progress-bar"" role=""progressbar"" style=""width:{0}% "">Page {1} of {2}</div></div>",
                    percentComplete, pageNumber, numberOfPages );

                CurrentPage = pageIndex;
                pnlLanguageChoice.Visible = CurrentPage == PageNames.LANGUAGE;
                pnlPersonalInfo.Visible = CurrentPage == PageNames.PERSONAL_INFORMATION;
                pnlMinorInfo.Visible = CurrentPage == PageNames.PERSONAL_INFORMATION && ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD );
                pnlVolunteerOpportunitiesForMinors.Visible = CurrentPage == PageNames.MINISTRY_INFORMATION && ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD );
                pnlMinistry.Visible = CurrentPage == PageNames.MINISTRY_INFORMATION;
                pnlBackground.Visible = CurrentPage == PageNames.BACKGROUND;
                pnlPersonalHistory.Visible = CurrentPage == PageNames.PERSONAL_HISTORY;
                pnlCoreBeliefs.Visible = CurrentPage == PageNames.CORE_BELIEFS;
                pnlSafetyPolicy1.Visible = CurrentPage == PageNames.SAFETY_POLICY_1;
                pnlSafetyPolicy7.Visible = CurrentPage == PageNames.SAFETY_POLICY_7;
                pnlStudentVolunteerAgreement.Visible = CurrentPage == PageNames.SAFETY_POLICY_7 && ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD );
                pnlAckAndAg.Visible = CurrentPage == PageNames.SAFETY_POLICY_7 && ( ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.EXISTING_VOLUNTEER || ApplicationType == ApplicationTypes.STAFF );
                pnlAckAndAgLevel2.Visible = CurrentPage == PageNames.SAFETY_POLICY_7 && ApplicationType == ApplicationTypes.LEVEL2;
                pnlAckAndAgLevel0.Visible = CurrentPage == PageNames.SAFETY_POLICY_7 && ApplicationType == ApplicationTypes.LEVEL0;
                pnlSSN.Visible = CurrentPage == PageNames.SAFETY_POLICY_7 && ( ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.EXISTING_VOLUNTEER || ApplicationType == ApplicationTypes.STAFF || ApplicationType == ApplicationTypes.LEVEL2 );

                pnlInternationalStudent.Visible = CurrentPage == PageNames.SAFETY_POLICY_7 && ( ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.LEVEL2 );
                pnlInternationalStudentDetails.Visible = false;
                if ( isInternationalStudent && CurrentPage == PageNames.SAFETY_POLICY_7 && ( ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.LEVEL2 ) )
                {
                    pnlSSN.Visible = false;
                    pnlInternationalStudentDetails.Visible = true;
                    rtbSurname.Enabled = true;
                    rtbGivenName.Enabled = true;
                    rtbMothersMaidenName.Enabled = true;
                    rtbOtherNames.Enabled = true;
                    rtbPlaceOfBirth.Enabled = true;
                    dppDateOfBirth.Enabled = true;
                    rtbCurrentAddress.Enabled = true;
                    rtbYearsInTX.Enabled = true;
                    rtbMonthsAtLakepointe.Enabled = true;
                    rtbPriorAddress.Enabled = true;
                    rfuIDFront.Enabled = true;
                }

                pnlAdultRelease.Visible = CurrentPage == PageNames.SAFETY_POLICY_7 && ( ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.EXISTING_VOLUNTEER || ApplicationType == ApplicationTypes.STAFF || ApplicationType == ApplicationTypes.LEVEL2 );
                pnlMinorRelease.Visible = CurrentPage == PageNames.SAFETY_POLICY_7 && ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD );

                pnlSignature.Visible = CurrentPage == PageNames.SIGNATURE;
                lSignatureInstructions.Visible = CurrentPage == PageNames.SIGNATURE && (ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD);
                pnlConfirmation.Visible = CurrentPage == PageNames.CONFIRMATION;
                if (CurrentPage == PageNames.SIGNATURE && SavedValues["PreferredLanguage"].Equals("Spanish"))
                {
                    TranslateSignatureBlock();
                }

                lbPrevious.Enabled = CurrentPage > 0 && CurrentPage != PageNames.LOCK;
                lbNext.Text = ( CurrentPage == PageNames.SAFETY_POLICY_7 ) ? Translation["NavigationSubmit"] : Translation["NavigationNext"];
                lbNext.Visible = ( CurrentPage < PageNames.SIGNATURE );
                lClickDoneFirst.Visible = ( CurrentPage == PageNames.SIGNATURE );

                pnlLockedApplication.Visible = CurrentPage == PageNames.LOCK;

                if ( CurrentPage != PageNames.LOCK )
                {
                    var nextPage = PageSequence.Next( CurrentPage );
                    SetTriggerIfPageHasVideo( nextPage, "lbNext" );

                    var previousPage = PageSequence.Previous( CurrentPage );
                    SetTriggerIfPageHasVideo( previousPage, "lbPrevious" );

                    switch ( CurrentPage )
                    {
                        case PageNames.SIGNATURE:
                            escElectronicSignatureControl.SignatureType = SignatureType.Typed;
                            escElectronicSignatureControl.DocumentTerm = "Volunteer Application";
                            escElectronicSignatureControl.SignedByEmail = CurrentPerson?.Email;
                            escElectronicSignatureControl.LegalName = CurrentPerson?.FullName;
                            escElectronicSignatureControl.ShowNameOnCompletionStepWhenInTypedSignatureMode = true;
                            escElectronicSignatureControl.EmailAddressPrompt = Rock.Web.UI.Controls.ElectronicSignatureControl.EmailAddressPromptType.CompletionEmail;

                            SignatureDocumentHtml = GetSignableHTML();
                            iframeSignatureDocumentHTML.Attributes["srcdoc"] = SignatureDocumentHtml;
                            iframeSignatureDocumentHTML.Attributes.Add( "onload", "resizeIframe(this)" );
                            iframeSignatureDocumentHTML.Attributes.Add( "onresize", "resizeIframe(this)" );
                            break;
                    }
                }
            }
            ScrollToTop();
        }

        /// <summary>
        /// Handles the Click event of the <see cref="ElectronicSignatureControl" />
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSignSignature_Click(object sender, EventArgs e)
        {
            // Glue stuff into the signature document
            var signatureDocument = new SignatureDocument();

            var signatureDocumentName = string.Format("{0}_VolunteerApplication", CurrentPerson.FullName.RemoveSpaces());

            signatureDocument.SignatureDocumentTemplateId = 113;
            signatureDocument.Status = SignatureDocumentStatus.Signed;
            signatureDocument.Name = signatureDocumentName;
            signatureDocument.EntityTypeId = EntityTypeCache.GetId<ConnectionRequest>();
            signatureDocument.EntityId = ConnectionRequest.Id;
            signatureDocument.SignedByPersonAliasId = CurrentPerson.PrimaryAliasId;
            signatureDocument.AssignedToPersonAliasId = CurrentPerson.PrimaryAliasId;

            if (ApplicationType == ApplicationTypes.CHILD || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.RECENT_TEEN)
            {
                var studentGuid = SavedValues["Student"].AsGuid();
                signatureDocument.AppliesToPersonAliasId = new PersonAliasService(_context).Queryable().AsNoTracking()
                    .Where(pa => pa.Guid == studentGuid).FirstOrDefault().Id;
            }
            else
            {
                signatureDocument.AppliesToPersonAliasId = CurrentPerson.PrimaryAliasId;
            }

            signatureDocument.SignedDocumentText = SignatureDocumentHtml;
            signatureDocument.LastStatusDate = RockDateTime.Now;
            signatureDocument.SignedDateTime = RockDateTime.Now;

            // From ElectronicSignatureControl
            signatureDocument.SignatureData = escElectronicSignatureControl.DrawnSignatureImageDataUrl;
            signatureDocument.SignedName = escElectronicSignatureControl.SignedName;
            signatureDocument.SignedByEmail = escElectronicSignatureControl.SignedByEmail;

            // From System.Web
            signatureDocument.SignedClientIp = this.GetClientIpAddress();
            signatureDocument.SignedClientUserAgent = Request.UserAgent;

            // Needed before determing SignatureInformation (Signed Name, metadata)
            signatureDocument.SignatureVerificationHash = SignatureDocumentService.CalculateSignatureVerificationHash(signatureDocument);

            var signatureInformationHtmlArgs = new GetSignatureInformationHtmlOptions
            {
                SignatureType = escElectronicSignatureControl.SignatureType,
                SignedName = escElectronicSignatureControl.SignedName,
                DrawnSignatureDataUrl = escElectronicSignatureControl.DrawnSignatureImageDataUrl,
                SignedByPerson = CurrentPerson,
                SignedDateTime = signatureDocument.SignedDateTime,
                SignedClientIp = signatureDocument.SignedClientIp,
                SignatureVerificationHash = signatureDocument.SignatureVerificationHash
            };

            // Helper takes care of generating HTML and combining SignatureDocumentHTML and signedSignatureDocumentHtml into the final Signed Document
            var signatureInformationHtml = ElectronicSignatureHelper.GetSignatureInformationHtml(signatureInformationHtmlArgs);
            var signedSignatureDocumentHtml = ElectronicSignatureHelper.GetSignedDocumentHtml( SignatureDocumentHtml, signatureInformationHtml);

            // PDF Generator to BinaryFile
            BinaryFile pdfFile;
            try
            {
                using (var pdfGenerator = new PdfGenerator())
                {
                    var binaryFileTypeId = BinaryFileTypeCache.GetId(Rock.SystemGuid.BinaryFiletype.DIGITALLY_SIGNED_DOCUMENTS.AsGuid());
                    pdfGenerator.PDFMediaType = MediaType.Print; // Doesn't seem to work but it was worth a try.

                    pdfFile = pdfGenerator.GetAsBinaryFileFromHtml(binaryFileTypeId ?? 0, signatureDocumentName, signedSignatureDocumentHtml);
                    pdfFile.IsTemporary = false;
                }
            }
            catch (PdfGeneratorException pdfGeneratorException)
            {
                LogException(pdfGeneratorException);
                nbWarning.Text = "Document Error: " + pdfGeneratorException.Message;
                ScrollToTop();
                return;
            }
            catch (Exception ex)
            {
                LogException( ex );
                nbWarning.Text = "PDF Generator Error: " + ex.Messages().JoinStrings( "; " );
                ScrollToTop();
                return;
            }

            pdfFile.IsTemporary = false;
            using (var rockContext = new RockContext())
            {
                new BinaryFileService(rockContext).Add(pdfFile);
                rockContext.SaveChanges();
                signatureDocument.BinaryFileId = pdfFile.Id;

                // Save Signature Documen to database
                var signatureDocumentService = new SignatureDocumentService(rockContext);
                signatureDocumentService.Add(signatureDocument);
                rockContext.SaveChanges();
            }

            // reload with new context to get navigation properties to load. This will be needed to save values back to Workflow Attributes
            signatureDocument = new SignatureDocumentService(new RockContext()).Get(signatureDocument.Id);

            using (var rockContext = new RockContext())
            {
                // Save to application Attributes
                SaveSignatureDocumentValuesToAttributes( signatureDocument );
            }

            // lbNext.Enabled = true;
            Navigate(Direction.FORWARD); // Press Next for them
        }

        /// <summary>
        /// Saves the signature signature document values to the person attribute.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="signatureDocument">The signature document.</param>
        public void SaveSignatureDocumentValuesToAttributes(SignatureDocument signatureDocument)
        {
            if (signatureDocument.BinaryFileId.HasValue)
            {
                var binaryFileGuid = new BinaryFileService(_context).GetGuid(signatureDocument.BinaryFileId.Value);
                if (binaryFileGuid != null)
                {
                    var applicant = GetApplicant();

                    // attach to person
                    applicant.LoadAttributes(_context);
                    SaveApplication(binaryFileGuid, applicant);
                    SetApplicationDate(applicant);

                    // This will trigger a workflow that will send a copy of the document to the volunteer
                    ConnectionRequest.ConnectionStatusId = VOLUNTEER_ONBOARDING_CONNECTION_STATUS_SIGNED;
                }
                else
                {
                    nbWarning.Text = "Error saving document";
                    //nextPage = CurrentPage;
                    ScrollToTop();
                }
            }
        }

        private void SetTriggerIfPageHasVideo( PageNames page, string controlId )
        {
            //switch ( page )
            //{
            //    case PageNames.CORE_BELIEFS:
            //    case PageNames.SAFETY_POLICY_1:
            //    case PageNames.CONFIRMATION:
            //        upVolunteerApplication.Triggers.Add( new PostBackTrigger() { ControlID = controlId } );
            //        break;
            //}
        }

        private bool AnswersAreCorrect()
        {
            nbWarning.Text = "";

            switch ( CurrentPage )
            {
                case PageNames.LANGUAGE:
                    if ( ddlLanguage.SelectedValue.IsNullOrWhiteSpace() )
                    {
                        nbWarning.Text = "Please select a language option.";
                        ScrollToTop();
                        return false;
                    }
                    break;
                case PageNames.PERSONAL_INFORMATION:
                    if ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD )
                    {
                        if ( !cbParentNotification.Checked )
                        {
                            nbWarning.Text = "You must acknowledge the Parent Notification at the top of the form.";
                            ScrollToTop();
                            return false;
                        }
                    }
                    break;
                case PageNames.PERSONAL_HISTORY:
                    int unrelated = 0;
                    int related = 0;
                    if ( ameReferences.AttributeMatrixGuid.HasValue )
                    {
                        var items = new AttributeMatrixService( _context ).GetByGuids( new List<Guid> { ameReferences.AttributeMatrixGuid.Value } ).FirstOrDefault().AttributeMatrixItems;
                        if ( items != null )
                        {
                            foreach ( var item in items )
                            {
                                item.LoadAttributes();
                                switch (item.AttributeValues["Related"].Value)
                                {
                                    case "1":
                                        unrelated++;
                                        break;
                                    case "2":
                                        related++;
                                        break;
                                    default: // no answer, no action
                                        break;
                                }
                            }
                        }
                    }
                    if ( related < 1 )
                    {
                        nbWarning.Text = "You must provide at least one reference who is related to you.";
                        ScrollToTop();
                        return false;
                    }
                    if ( unrelated < 3 )
                    {
                        nbWarning.Text = "You must provide at least three references who are not related to you.";
                        ScrollToTop();
                        return false;
                    }
                    break;
                case PageNames.CORE_BELIEFS:
                    if ( rblBelief.SelectedValue != "1" && rblBelief.SelectedValue != "2" )
                    {
                        nbWarning.Text = "You must select one option or the other before proceeding.";
                        ScrollToTop();
                        return false;
                    }
                    break;
                case PageNames.SAFETY_POLICY_7:
                    if ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD )
                    {
                        if ( rcblStudentAgreement.SelectedValues.Count < 6 )
                        {
                            nbWarning.Text = "You must agree to all terms of the Volunteer Agreement section to continue.";
                            ScrollToTop();
                            return false;
                        }

                        if ( rcblNoticeOfRequirements.SelectedValues.Count < 1 )
                        {
                            nbWarning.Text = "You must acknowledge the Notice of Requirements to continue.";
                            ScrollToTop();
                            return false;
                        }

                        if ( rcblParentGuardianApproval.SelectedValues.Count < 5 )
                        {
                            nbWarning.Text = "You must agree to all terms in the Parent/Guardian Approval section to continue.";
                            ScrollToTop();
                            return false;
                        }

                        if ( rcblMinorRelease.SelectedValues.Count < 3 )
                        {
                            nbWarning.Text = "You must agree to all terms in the Release section to continue.";
                            ScrollToTop();
                            return false;
                        }

                        if (rtbMinorSignature.Text.IsNullOrWhiteSpace() )
                        {
                            nbWarning.Text = "Student/child must type their name in the signature box to continue.";
                            ScrollToTop();
                            return false;
                        }
                    }

                    if ( ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.EXISTING_VOLUNTEER || ApplicationType == ApplicationTypes.STAFF )
                    {
                        if ( rcblAgree.SelectedValues.Count < 3 )
                        {
                            nbWarning.Text = "You must agree to all terms in the Statement of Acknowledgement and Agreement section to continue.";
                            ScrollToTop();
                            return false;
                        }

                        if ( rcblAdultRelease.SelectedValues.Count < 4 )
                        {
                            nbWarning.Text = "You must agree to all terms in the Release section to continue.";
                            ScrollToTop();
                            return false;
                        }
                    }

                    if ( ApplicationType == ApplicationTypes.LEVEL2 )
                    {
                        if ( rcblAgreeL2.SelectedValues.Count < 2 )
                        {
                            nbWarning.Text = "You must agree to all terms in the Statement of Acknowledgement and Agreement section to continue.";
                            ScrollToTop();
                            return false;
                        }

                        if ( rcblAdultRelease.SelectedValues.Count < 4 )
                        {
                            nbWarning.Text = "You must agree to all terms in the Release section to continue.";
                            ScrollToTop();
                            return false;
                        }
                    }

                    if ( ApplicationType == ApplicationTypes.LEVEL0 )
                    {
                        if ( !( rcblAgreeL0.SelectedValues.Contains( "1" ) ^ rcblAgreeL0.SelectedValues.Contains( "3" ) ) )
                        {
                            nbWarning.Text = "You must select exactly one of the first two options to continue.";
                            ScrollToTop();
                            return false;
                        }

                        if ( !rcblAgreeL0.SelectedValues.Contains( "2" ) )
                        {
                            nbWarning.Text = "You must select the third option to continue.";
                            ScrollToTop();
                            return false;
                        }
                    }

                    if ( ( ApplicationType == ApplicationTypes.ADULT || ApplicationType == ApplicationTypes.STAFF ) && rblSSN.SelectedValue != "Yes" && rblSSN.SelectedValue != "No" )
                    {
                        nbWarning.Text = "You must select one of the options for Social Security Number. If you have questions, please refer to the contact information below.";
                        ScrollToTop();
                        return false;
                    }
                    break;
            }

            return true;
        }

        private string GetSignableHTML()
        {
            var applicationUrl = ConstructPdfSourceLink((string)ViewState["CR"]);
            System.Diagnostics.Debug.WriteLine($"VolunteerApplication: targeting {applicationUrl} for signature document.");
            var webRequest = (HttpWebRequest)HttpWebRequest.Create(applicationUrl);
            webRequest.CookieContainer = new CookieContainer(); // Adding a cookie container so automatic redirects work correctly
            webRequest.Method = "GET";
            var response = webRequest.GetResponse();
            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        private void BindMinors()
        {
            var candidates = new List<Person>();
            candidates.Add( new Person() { Id = 0, FirstName = "", LastName = "" } ); // Insert a blank entry at the top

            var groupMemberService = new GroupMemberService( _context );
            var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
            var knownRelationshipGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid();
            var wardRelationshipGuid = "F6D48DB4-435A-4CA8-B8D9-DC5831CCC1EE".AsGuid();
            //var guardianRelationshipGuid = "CCEDF37C-860A-4C81-BBAE-D4C6A361E77B".AsGuid();

            // build list of age-elligible minors in the household
            var family = groupMemberService.Queryable()
                .Where( gm => gm.Group.GroupType.Guid == familyGroupTypeGuid )
                .Where( gm => gm.PersonId == CurrentPersonId )
                .FirstOrDefault().Group;
            var minors = family.Members;
            if ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN )
            {
                minors = minors.Where( p => p.Person.Age < 18 && p.Person.Age >= 13 ).ToList(); 
            }
            else
            {
                minors = minors.Where( p => p.Person.Age <= 12 && p.Person.Age >= 9 ).ToList();
            }
            candidates.AddRange( minors.Select( gm => gm.Person ) );

            // add age-elligible minors with whom the logged-in user has a Can Check In relationship
            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
            var ownerRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) );
            int? ownerRoleId = null;
            if ( ownerRole != null )
            {
                ownerRoleId = ownerRole.Id;
            }

            var relationshipIds = groupMemberService.Queryable().AsNoTracking()
                .Where( gm => gm.Group.GroupType.Guid == knownRelationshipGroupTypeGuid )
                .Where( gm => gm.GroupRoleId == ownerRoleId )
                .Where( gm => gm.PersonId == CurrentPersonId )
                .Select( gm => gm.GroupId )
                .ToList();
            var wards = groupMemberService.Queryable().AsNoTracking()
                .Where( gm => relationshipIds.Contains( gm.GroupId ) )
                .Where( gm => gm.GroupRole.Guid == wardRelationshipGuid ).ToList();

            if ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN )
            {
                // allow adults who are tagged as wards to fill out Teen applications
                wards = wards.Where( p => /* p.Person.Age < 18 && */ p.Person.Age >= 13 ).ToList(); 
            }
            else
            {
                wards = wards.Where( p => p.Person.Age <= 12 && p.Person.Age >= 9 ).ToList();
            }
            candidates.AddRange( wards.Select( gm => gm.Person ).ToList().Except( candidates ) );

            ddlMinor.DataSource = candidates;
            ddlMinor.DataTextField = "FullName";
            ddlMinor.DataValueField = "Id";
            ddlMinor.DataBind();
        }

        private Person GetApplicant()
        {
            // applicant is not necessarily CurrentPerson--make sure we attach the document to the person applying
            Person applicant = ConnectionRequest.PersonAlias.Person;
            if ( ApplicationType == ApplicationTypes.RECENT_TEEN || ApplicationType == ApplicationTypes.TEEN || ApplicationType == ApplicationTypes.CHILD )
            {
                var studentGuid = SavedValues["Student"].AsGuid();
                var studentId = new PersonAliasService( _context ).Queryable().AsNoTracking()
                    .Where( pa => pa.Guid == studentGuid ).FirstOrDefault().PersonId;
                applicant = new PersonService( _context ).Queryable().AsNoTracking()
                    .Where( p => p.Id == studentId )
                    .FirstOrDefault();
            }

            return applicant;
        }

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="fromEmail">From email.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        private void Send( List<RockEmailMessageRecipient> recipients, string fromEmail, string fromName, string subject, string body )
        {
            var emailMessage = new RockEmailMessage();
            emailMessage.SetRecipients( recipients );
            emailMessage.FromEmail = fromEmail;
            emailMessage.FromName = fromName.IsNullOrWhiteSpace() ? fromEmail : fromName;
            emailMessage.Subject = subject;
            emailMessage.Message = body;
            emailMessage.CreateCommunicationRecord = false;
            emailMessage.AppRoot = Rock.Web.Cache.GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" ) ?? string.Empty;

            emailMessage.Send();
        }

        private Dictionary<string, string> Level2MinistriesOfInterest()
        {
            var result = new Dictionary<string, string>();
            foreach (var option in Translation["KeysForMinistriesLevel2"].Split(','))
            {
                var keyAndValue = option.Split('^');
                result.Add(keyAndValue[0], keyAndValue[1]);
            }

            return result;
        }

        private Dictionary<string, string> Level0MinistriesOfInterest()
        {
            var result = new Dictionary<string, string>();
            foreach ( var option in Translation["KeysForMinistriesLevel0"].Split( ',' ) )
            {
                var keyAndValue = option.Split( '^' );
                result.Add( keyAndValue[0], keyAndValue[1] );
            }

            return result;
        }

        private Dictionary<string, string> MinistriesOfInterest()
        {
            var result = new Dictionary<string, string>();
            foreach ( var option in Translation["KeysForMinistries"].Split( ',' ) )
            {
                var keyAndValue = option.Split( '^' );
                result.Add( keyAndValue[0], keyAndValue[1] );
            }

            return result;
        }

        private Dictionary<string, string> StudentMinistriesOfInterest(string key)
        {
            var result = new Dictionary<string, string>();
            foreach ( var option in Translation[key].Split( ',' ) )
            {
                var keyAndValue = option.Split( '^' );
                result.Add( keyAndValue[0], keyAndValue[1] );
            }

            return result;
        }

        private string MapTrueFalseToYesNo( string trueFalse )
        {
            switch ( trueFalse )
            {
                case "True":
                    return "Yes";
                case "False":
                    return "No";
            }
            return null;
        }

        private string MapYesNoToTrueFalse( string yesNo )
        {
            switch ( yesNo )
            {
                case "Yes":
                    return "True";
                case "No":
                    return "False";
            }
            return null;
        }

        private void ScrollToTop( int intPosY = 0 )
        {
            string strScript = @"var manager = Sys.WebForms.PageRequestManager.getInstance(); 
            manager.add_beginRequest(beginRequest); 
            function beginRequest() 
            { 
                manager._scrollPosition = null; 
            }
            window.scroll(0, " + intPosY.ToString() + ");";

            ScriptManager.RegisterStartupScript( upVolunteerApplication, upVolunteerApplication.GetType(), "Error_" + RockDateTime.Now.Ticks.ToString(), strScript, true );

            return;
        }

        private void TranslateSignatureBlock()
        {
            string strScript = @"Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageLoaded);
                function pageLoaded(sender, args) 
                {
                    $('[id$=tbSignatureTyped]').attr('placeholder', '" + Translation["SignaturePlaceholder"] + @"');
                    if ($('.signature-entry-agreement').length > 0)
                        $('.signature-entry-agreement').get(0).lastChild.nodeValue = '" + Translation["SignatureAgreement"] + @"';
                    if ($('[id$=btnSignSignature]').length > 0)
                        $('[id$=btnSignSignature]').get(0).lastChild.nodeValue = '" + Translation["SignatureSign"] + @"';
                    if ($('[id$=btnCompleteSignature]').length > 0)
                        $('[id$=btnCompleteSignature]').get(0).lastChild.nodeValue = '" + Translation["SignatureComplete"] + @"';
                    if ($('[for$=lCompletionSignedName]').length > 0)
                        $('[for$=lCompletionSignedName]').get(0).lastChild.nodeValue = '" + Translation["LegalName"] + @"';
                    if ($('[for$=ebEmailAddress]').length > 0)
                        $('[for$=ebEmailAddress]').get(0).lastChild.nodeValue = '" + Translation["ConfirmationEmailPrompt"] + @"';
                }
            ";

            ScriptManager.RegisterStartupScript(upVolunteerApplication, upVolunteerApplication.GetType(), "Translate_" + RockDateTime.Now.Ticks.ToString(), strScript, true);

            return;
        }

        private string ConstructPdfSourceLink(string connectionRequestGuid)
        {
            // SNS 20230320 this really should be public application root but RockIIS is not able to route traffic to my.lakepointe.church for reasons unknown.
            //var publicApplicationRoot = "https://localhost:44353/";
            var publicApplicationRoot = GlobalAttributesCache.Value( "InternalApplicationRoot" ).EnsureTrailingForwardslash() ?? "https://rock.lakepointe.church/";
            return string.Format("{2}page/2124?PDF=true&CR={0}&rckipid={1}",
                connectionRequestGuid,
                CurrentPerson.GetImpersonationToken(RockDateTime.Now.AddMinutes(5), 1, 2124),
                publicApplicationRoot); // expires in five minutes after a single use, good for page 2124 only
        }

        #endregion

        #region Private Classes

        private class CircularList<T> : List<T>
        {
            public T Next( T current )
            {
                return this[( IndexOf( current ) + 1 ) % Count];
            }

            public T Previous( T current )
            {
                var index = IndexOf( current ) - 1;
                if ( index < 0 )
                {
                    index = Count - 1;
                }
                return this[index];
            }
        }

        #endregion
    }
}