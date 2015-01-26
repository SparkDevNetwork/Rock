<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockControlGallery.ascx.cs" Inherits="RockWeb.Blocks.Examples.RockControlGallery" %>
<script type="text/javascript">
    function pageLoad() {
        prettyPrint();
    }
</script>
<style>
    .rlink {
        font-size: 16px;
        margin-left: -16px;
        outline: none;
    }

    .anchor {
        outline: none;
    }
</style>
<asp:UpdatePanel ID="upnlExample" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-magic"></i> Control Gallery</h1>
            </div>
            <div class="panel-body">
                <asp:Panel ID="pnlDetails" runat="server">

                    <asp:ValidationSummary ID="valExample" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <h1 runat="server">General Information</h1>

                    <h2 runat="server">Input Sizing Rules</h2>

                    <div class="alert alert-warning">
                        <p><strong>Warning!</strong></p>
                        In Bootstrap 3 inputs are meant to fill the width of their parent container (<a href="http://getbootstrap.com/css/#forms-control-sizes" class="alert-link">link</a>).  If a small input is desired they should
                    be wrapped in a table grid.  This provides the best responsive solution.  In some rare cases it's beneficial to be able to fix the width of
                    certain inputs to provide better context of what the input is for.  For instance a credit card CVV field makes more sense visually being
                    fixed width to 3 characters.  To provide this capability we have added the following CSS classes to fix width inputs.  <em>Please use them
                    sparingly.</em>
                    </div>

                    <div class="alert alert-danger">
                        <p><strong>Alert</strong></p>
                        Rock framework developers should get approval from the Core Team before using these styles.

                    </div>

                    <div runat="server" class="r-example">
                        <Rock:RockTextBox ID="tbInput1" runat="server" CssClass="input-width-xs" Label=".input-width-xs" />

                        <Rock:RockTextBox ID="tbInput2" runat="server" CssClass="input-width-sm" Label=".input-width-sm" />

                        <Rock:RockTextBox ID="tbInput3" runat="server" CssClass="input-width-md" Label=".input-width-md" />

                        <Rock:RockTextBox ID="tbInput4" runat="server" CssClass="input-width-lg" Label=".input-width-lg" />

                        <Rock:RockTextBox ID="tbInput5" runat="server" CssClass="input-width-xl" Label=".input-width-xl" />

                        <Rock:RockTextBox ID="tbInput6" runat="server" CssClass="input-width-xxl" Label=".input-width-xxl" />
                    </div>

                    <div class="alert alert-info">
                        <p><strong>Note</strong></p>
                        In Bootstrap 3 inputs are <em>display:block;</em>. If you need these sized controls to align horizontally, consider wrapping them with the <em>form-control-group</em> class.
                    </div>


                    <a id="Grid"></a>
                    <h2 runat="server">Rock:Grid</h2>
                    <div runat="server" class="r-example">
                        <div class="grid">
                            <Rock:Grid ID="gExample" runat="server" AllowSorting="true">
                                <Columns>
                                    <Rock:ColorField DataField="DefinedValueColor" ToolTipDataField="DefinedValueTypeName" HeaderText="" />
                                    <Rock:RockBoundField DataField="DefinedValueTypeName" HeaderText="Name" SortExpression="DefinedValueTypeName" />
                                    <Rock:DateTimeField DataField="SomeDateTime" HeaderText="DateTime" SortExpression="SomeDateTime" />
                                    <Rock:BoolField DataField="SomeBoolean" HeaderText="Some Boolean" SortExpression="SomeBoolean" />
                                    <Rock:EditField />
                                    <Rock:SecurityField />
                                    <Rock:DeleteField />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                    <div class="alert alert-info"><strong>Note</strong> An extra div is required around the grid to help wrap the grid and an optional grid filter for styling.</div>


                    <a id="DropDowns"></a>
                    <h1 runat="server">DropDowns</h1>

                    <a id="DataDropDownList"></a>
                    <div runat="server" class="r-example">
                        <Rock:DataDropDownList ID="ddlDataExample" runat="server" Label="Rock:DataDropDownList/RockDropDownList" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
                    </div>

                    <a id="StateDropDownList"></a>
                    <div runat="server" class="r-example">
                        <Rock:StateDropDownList ID="statepExample" runat="server" Label="Rock:StateDropDownList" />
                    </div>

                    <a id="ButtonDropDownList"></a>
                    <div runat="server" class="r-example">
                        <Rock:ButtonDropDownList ID="bddlExample" runat="server" Label="Rock:ButtonDropDownList" />
                    </div>

                    <a id="Input"></a>
                    <h1 runat="server">Input</h1>


                    <a id="DataTextBox"></a>
                    <div runat="server" class="r-example">
                        <Rock:DataTextBox ID="dtbExample" runat="server" Label="Rock:DataTextBox" LabelTextFromPropertyName="false" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Description" />
                    </div>

                    <a id="EmailBox"></a>
                    <div runat="server" class="r-example">
                        <Rock:EmailBox ID="ebEmail" runat="server" Label="Rock:EmailBox" />
                    </div>

                    <a id="UrlLinkBox"></a>
                    <div runat="server" class="r-example">
                        <Rock:UrlLinkBox ID="ulLink" runat="server" Label="Rock:UrlLinkBox" />
                    </div>

                    <a id="NumberBox"></a>
                    <div runat="server" class="r-example">
                        <Rock:NumberBox ID="numbExample" runat="server" Label="Rock:NumberBox" />
                    </div>

                    <a id="RockCheckBox"></a>
                    <div runat="server" class="r-example">
                        <Rock:RockCheckBox ID="cbExample" runat="server" Label="Rock:RockCheckBox" />
                    </div>

                    <a id="RockCheckBoxList"></a>
                    <div runat="server" class="r-example">
                        <Rock:RockCheckBoxList ID="cblExample" runat="server" Label="Rock:RockCheckBoxList" />
                    </div>

                    <a id="RockCheckBoxListHorizontal"></a>
                    <div runat="server" class="r-example">
                        <Rock:RockCheckBoxList ID="cblExampleHorizontal" runat="server" Label="Rock:RockCheckBoxList (horizontal)" RepeatDirection="Horizontal" />
                    </div>

                    <a id="RockRadioButtonList"></a>
                    <div runat="server" class="r-example">
                        <Rock:RockRadioButtonList ID="rblExample" runat="server" Label="Rock:RockRadioButtonList" />
                    </div>

                    <a id="RockRadioButtonListHorizontal"></a>
                    <div runat="server" class="r-example">
                        <Rock:RockRadioButtonList ID="rblExampleHorizontal" runat="server" Label="Rock:RockRadioButtonList (horizontal)" RepeatDirection="Horizontal" />
                    </div>

                    <a id="NumberRangeEditor"></a>
                    <div runat="server" class="r-example">
                        <Rock:NumberRangeEditor ID="nreExample" runat="server" Label="Rock:NumberRangeEditor" LowerValue="10" UpperValue="25" />
                    </div>

                    <a id="RatingInput"></a>
                    <div runat="server" class="r-example">
                        <Rock:RockRating ID="rrRating" runat="server" Label="Rock:RatingInput" /><br />
                    </div>

                    <a id="Pickers"></a>
                    <h1 runat="server">Pickers</h1>

                    <a id="DatePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:DatePicker ID="dpExample" runat="server" Label="Rock:DatePicker" />
                    </div>

                    <a id="DateTimePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:DateTimePicker ID="dtpExample" runat="server" Label="Rock:DateTimePicker" />
                    </div>

                    <a id="DaysOfWeekPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:DaysOfWeekPicker ID="daysOfWeekPicker" RepeatDirection="Horizontal" runat="server" Label="Rock:DaysOfWeekPicker" />
                    </div>

                    <a id="DayOfWeekPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:DayOfWeekPicker ID="dayOfWeekPicker" runat="server" Label="Rock:DayOfWeekPicker" />
                    </div>


                    <a id="TimePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:TimePicker ID="timepExample" runat="server" Label="Rock:TimePicker" />
                    </div>

                    <a id="MonthYearPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:MonthYearPicker ID="mypExample" runat="server" Label="Rock:MonthYearPicker" OnSelectedMonthYearChanged="monthYearPicker_SelectedMonthYearChanged" />
                    </div>

                    <a id="MonthDayPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:MonthDayPicker ID="mdpExample" runat="server" Label="Rock:MonthDayPicker" OnSelectedMonthDayChanged="monthDayPicker_SelectedMonthDayChanged" />
                    </div>

                    <a id="DateRangePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:DateRangePicker ID="drpExample" runat="server" Label="Rock:DateRangePicker" LowerValue="1/1/2012" UpperValue="12/31/2014" />
                    </div>

                    <a id="BirthdayPicker"></a>
                    <div id="Div4" runat="server" class="r-example">
                        <Rock:BirthdayPicker ID="bdaypExample" runat="server" Label="Rock:BirthdayPicker" OnSelectedBirthdayChanged="birthdayPicker_SelectedBirthdayChanged" />
                    </div>

                    <a id="GroupPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:GroupPicker ID="gpExample" runat="server" Label="Rock:GroupPicker" />
                    </div>

                    <a id="CampusPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:CampusPicker ID="campExample" runat="server" Label="Rock:CampusPicker" />
                    </div>

                    <a id="PagePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:PagePicker ID="pagepExample" runat="server" Label="Rock:PagePicker" />
                    </div>

                    <a id="PersonPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:PersonPicker ID="ppExample" runat="server" Label="Rock:PersonPicker" />
                    </div>

                    <a id="AccountPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:AccountPicker ID="acctpExample" runat="server" Label="Rock:AccountPicker" />
                    </div>

                    <a id="CategoryPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:CategoryPicker ID="catpExample" runat="server" Label="Rock:CategoryPicker" />
                    </div>

                    <a id="ComponentPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:ComponentPicker ID="compExample" runat="server" Label="Rock:ComponentPicker" />
                    </div>

                    <a id="LocationPicker"></a>
                    <div id="Div2" runat="server" class="r-example">
                        <Rock:LocationPicker ID="locpExample" runat="server" Label="Rock:LocationPicker" />
                    </div>

                    <a id="LocationPicker2"></a>
                    <div runat="server" class="r-example">
                        <Rock:LocationPicker ID="locpExampleAddressMode" runat="server" Label="Rock:LocationPicker (Address Mode, Mode Selection disabled)" CurrentPickerMode="Address" AllowedPickerModes="Address" />
                    </div>

                    <a id="GroupRolePicker"></a>
                    <div id="Div1" runat="server" class="r-example">
                        <Rock:GroupRolePicker ID="grpExample" runat="server" Label="Rock:GroupRolePicker" />
                    </div>

                    <a id="Other"></a>
                    <h1 runat="server">Other</h1>

                    <a id="ScheduleBuilder"></a>
                    <div runat="server" class="r-example">
                        <Rock:ScheduleBuilder ID="schedbExample" runat="server" Label="Rock:ScheduleBuilder" OnSaveSchedule="scheduleBuilder_SaveSchedule" />
                    </div>

                    <a id="GeoPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:GeoPicker ID="geopExamplePoint" runat="server" Label="Rock:GeoPicker (Point mode)" DrawingMode="Point" MapStyleValueGuid="BFC46259-FB66-4427-BF05-2B030A582BEA" />
                    </div>

                    <a id="GeoPickerPolygon"></a>
                    <div runat="server" class="r-example">
                        <Rock:GeoPicker ID="geopExamplePolygon" runat="server" Label="Rock:GeoPicker (Polygon mode)" DrawingMode="Polygon" Help="You can set the style of this through the 'Map Style' block attribute." />
                    </div>

                    <a id="MergeFieldPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:MergeFieldPicker ID="mfpExample" runat="server" Label="Rock:MergeFieldPicker" />
                    </div>

                    <a id="MetricEntityPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:MetricEntityPicker ID="mepExample" runat="server" Label="Rock:MetricEntityPicker" />
                    </div>

                    <h2 runat="server">BinaryFilePicker, BinaryFileTypePicker</h2>

                    <a id="BinaryFileTypePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:BinaryFileTypePicker ID="bftpExample" runat="server" Label="Rock:BinaryFileTypePicker" OnSelectedIndexChanged="binaryFileTypePicker_SelectedIndexChanged" />
                    </div>

                    <a id="BinaryFilePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:BinaryFilePicker ID="bfpExample" runat="server" Label="Rock:BinaryFilePicker" />
                    </div>

                    <a id="Misc"></a>
                    <h1 runat="server">Misc</h1>

                    <a id="FieldTypeList"></a>
                    <div runat="server" class="r-example">
                        <Rock:FieldTypeList ID="ftlExample" runat="server" Label="Rock:FieldTypeList" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
                    </div>

                    <a id="FileUploader"></a>
                    <div runat="server" class="r-example">
                        <Rock:FileUploader ID="fuprExampleBinaryFile" runat="server" Label="Rock:FileUploader (BinaryFile mode)" BinaryFileTypeGuid="C1142570-8CD6-4A20-83B1-ACB47C1CD377" />
                    </div>

                    <a id="FileUploaderContentFileMode"></a>
                    <div runat="server" class="r-example">
                        <Rock:FileUploader ID="fuprExampleContentFile" runat="server" Label="Rock:FileUploader (Content file mode)" IsBinaryFile="false" RootFolder="~/App_Data/TemporaryFiles" OnFileUploaded="fupContentFile_FileUploaded" />
                        <asp:Label ID="lblPhysicalFileName" runat="server" Text="Uploaded File: -" />
                    </div>

                    <a id="ImageUploader"></a>
                    <div runat="server" class="r-example">
                        <Rock:ImageUploader ID="imgupExample" runat="server" Label="Rock:ImageUploader" />
                    </div>

                    <div runat="server" class="r-example">
                        <Rock:ImageUploader ID="ImageUploader1" runat="server" Label="Rock:ImageUploader" ThumbnailHeight="200" ThumbnailWidth="200" />
                    </div>

                    <a id="ImageEditor"></a>
                    <div runat="server" class="r-example">
                        <Rock:ImageEditor ID="imageEditor" runat="server" Label="Rock:ImageEditor" MaxImageWidth="1600" MaxImageHeight="1200" />
                    </div>

                    <a id="Notificationbox"></a>
                    <h2 runat="server">Rock:Notificationbox</h2>
                    <p>
                        This creates a <a href="http://getbootstrap.com/components/#alerts">Bootstrap alert</a>.  We've added the ability to have Details that can be shown. 
                    </p>

                    <div runat="server" class="r-example">
                        <Rock:NotificationBox ID="nbExampleSuccess" runat="server" Title="Success" Text="This is a success message." NotificationBoxType="Success" />
                        <Rock:NotificationBox ID="nbExampleInfo" runat="server" Title="Info" Text="This is an informational message." NotificationBoxType="Info" />
                        <Rock:NotificationBox ID="nbExampleWarning" runat="server" Title="Warning" Text="This is a warning." NotificationBoxType="Warning" />
                        <Rock:NotificationBox ID="nbExampleDanger" runat="server" Title="Danger" Text="Something really went wrong." NotificationBoxType="Danger" />
                        <Rock:NotificationBox ID="nbExampleDismissable" runat="server" Title="Hey" Text="You can close this when you are done reading it if you want." NotificationBoxType="Warning" Dismissable="true" />
                        <Rock:NotificationBox ID="nbExampleDetails" runat="server" Title="Some Info" Text="This is a message with extra stuff." Details="Here are the extra details" />
                    </div>

                    <a id="Badge"></a>
                    <h2 runat="server">Rock:Badge</h2>
                    <p>
                        This is a mostly standard Bootstrap badge. We say "mostly" because we added the ability to
                    control the color of the badge via the BadgeType property (danger, warning, success, info) similar
                    to the old <a href="http://getbootstrap.com/2.3.2/components.html#labels-badges">Bootstrap 2.3 Labels and Badges</a>.
                    </p>
                    <div runat="server" class="r-example">
                        <Rock:Badge ID="badge" runat="server" ToolTip="you have new messages waiting" Text="1" />
                        <Rock:Badge ID="badge1" runat="server" BadgeType="success" Text="0" />
                        <Rock:Badge ID="badge2" runat="server" BadgeType="info" Text="5" />
                        <Rock:Badge ID="badge3" runat="server" BadgeType="warning" Text="15" />
                        <Rock:Badge ID="badge4" runat="server" BadgeType="danger" Text="99" />

                    </div>

                    <a id="HighlightLabel"></a>
                    <h2 runat="server">Rock:HighlightLabel</h2>
                    <p>
                        This creates a <a href="http://getbootstrap.com/components/#labels">Bootstrap Label</a>
                        but we've added a few additional custom <code>LabelType</code> options to control the color.
                    </p>
                    <div runat="server" class="r-example">
                        <Rock:HighlightLabel ID="hlblExample1" runat="server" LabelType="Default" Text="Default" ToolTip="More information is here." />
                        <Rock:HighlightLabel ID="hlblExample2" runat="server" LabelType="Primary" Text="Primary" />
                        <Rock:HighlightLabel ID="hlblExample3" runat="server" LabelType="Success" Text="Success" />
                        <Rock:HighlightLabel ID="hlblExample4" runat="server" LabelType="Info" Text="Info" />
                        <Rock:HighlightLabel ID="hlblExample5" runat="server" LabelType="Warning" Text="Warning" />
                        <Rock:HighlightLabel ID="hlblExample6" runat="server" LabelType="Danger" Text="Danger" />
                        <Rock:HighlightLabel ID="hlblExample7" runat="server" LabelType="Campus" Text="Campus" />
                        <Rock:HighlightLabel ID="hlblExample8" runat="server" LabelType="Type" Text="Type" />
                        <Rock:HighlightLabel ID="hlblExample9" runat="server" LabelType="Custom" CustomClass="danger" Text="Custom" />
                    </div>

                    <p>
                        While you can set the <code>Text</code> to include HTML (such as font icons), you can also do this 
                    a little easier just by setting the <code>IconCssClass</code> property.
                    </p>

                    <a id="HighlightLabelErrors"></a>
                    <div runat="server" class="r-example">
                        <Rock:HighlightLabel ID="hlblExample" runat="server" LabelType="Danger" IconCssClass="fa fa-flag" Text="errors" />
                    </div>

                    <a id="Toggle"></a>
                    <h2 runat="server">Rock:Toggle</h2>
                    <p>A toggle switch for those cases when a simple checkbox just won't do.</p>
                    <div runat="server" class="r-example">
                        <Rock:Toggle ID="toggleShowPreview" runat="server"
                            Label="Show Preview?" OnText="Yes" OffText="No" Checked="true"
                            Help="If set to yes, a preview will be shown immediately as you update your criteria."
                            OnCheckedChanged="toggleShowPreview_CheckedChanged" />
                    </div>

                    <a id="ToggleActiveButton"></a>
                    <p>Need special color indicators on the buttons? Set ActiveButtonCssClass to <code>.btn-info</code>, <code>.btn-success</code>, <code>.btn-danger</code> or <code>.btn-warning</code></p>
                    <div runat="server" class="r-example">
                        <Rock:Toggle ID="tglExample1" runat="server" />
                        <Rock:Toggle ID="tglExample2" runat="server" ActiveButtonCssClass="btn-info" />
                        <Rock:Toggle ID="tglExample3" runat="server" ActiveButtonCssClass="btn-success" />
                        <Rock:Toggle ID="tglExample4" runat="server" ActiveButtonCssClass="btn-danger" />
                        <Rock:Toggle ID="tglExample5" runat="server" ActiveButtonCssClass="btn-warning" />
                        <Rock:Toggle ID="tglExample6" runat="server" ActiveButtonCssClass="btn-primary" />
                    </div>

                    <a id="ToggleOnOffStyles"></a>
                    <p>Want different colors for the on/off states?</p>
                    <div runat="server" class="r-example">
                        <Rock:Toggle ID="tglExampleOnOff" OnCssClass="btn-success" OffCssClass="btn-danger" runat="server" />
                    </div>

                    <a id="ToggleSizes"></a>
                    <p>Need larger or smaller toggle buttons? Set ButtonSizeCssClass to <code>.btn-lg</code>, <code>.btn-sm</code> or <code>.btn-xs</code></p>
                    <div runat="server" class="r-example">
                        <Rock:Toggle ID="tglExample7" runat="server" ButtonSizeCssClass="btn-lg" />
                        <Rock:Toggle ID="tglExample8" runat="server" />
                        <Rock:Toggle ID="tglExample9" runat="server" ButtonSizeCssClass="btn-sm" />
                        <Rock:Toggle ID="tglExample10" runat="server" ButtonSizeCssClass="btn-xs" />
                    </div>

                    <a id="BootstrapButton"></a>
                    <h2 runat="server">Rock:BootstrapButton</h2>
                    <div runat="server" class="r-example">
                        <Rock:BootstrapButton ID="lbSave" runat="server" Text="Click Me" CssClass="btn btn-primary"
                            DataLoadingText="&lt;i class='fa fa-refresh fa-spin fa-2x'&gt;&lt;/i&gt; Saving" />
                    </div>

                    <a id="NoteControl"></a>
                    <h2 runat="server">Rock:NoteControl</h2>
                    <div id="Div3" runat="server" class="r-example">
                        <section class="panel-note">
                            <Rock:NoteControl ID="noteExample" runat="server" IsAlert="false" IsPrivate="false" Text="Here is some example note text." CanEdit="true" />
                        </section>
                    </div>

                    <a id="AttributeEditor"></a>
                    <h2 runat="server">Rock:AttributeEditor</h2>
                    <div runat="server" class="r-example">
                        <asp:LinkButton ID="lbExample" runat="server" CssClass="btn btn-link" Text="Attribute Editor..." OnClick="btnShowAttributeEditor_Click" CausesValidation="false" />
                        <asp:Panel ID="pnlAttributeEditor" runat="server" Visible="false" CssClass="well">
                            <Rock:AttributeEditor ID="edtExample" runat="server" OnCancelClick="aeExample_CancelClick" OnSaveClick="aeExample_SaveClick" ValidationGroup="Attribute" />
                        </asp:Panel>
                    </div>

                    <a id="HtmlEditor"></a>
                    <h2 runat="server">Rock:HtmlEditor</h2>
                    <div runat="server" class="r-example">
                        <Rock:HtmlEditor ID="htmlEditorFull" runat="server" Label="HtmlEditor" Toolbar="Full" />
                    </div>

                    <a id="HtmlEditorLight"></a>
                    <h2 runat="server">Rock:HtmlEditor</h2>
                    <div runat="server" class="r-example">
                        <Rock:HtmlEditor ID="htmlEditorLight" runat="server" Label="HtmlEditor" Toolbar="Light" />
                    </div>

                    <a id="CodeEditor"></a>
                    <h2 runat="server">Rock:CodeEditor</h2>
                    <div runat="server" class="r-example">
                        <Rock:CodeEditor ID="ceScript" runat="server" EditorTheme="Rock" Label="Script" EditorMode="Html" EditorHeight="300">
    <h1>Hello!!!</h1> 
    <p>This is a great way to edit HTML! Reasons:</p>

    <!-- Comment 
         We shouldn't have to explain why this is better than just a 
         textarea but we will just for you...
    -->

    <ol class="reasons">
        <li>Stynax highlighting</li>
        <li>Tabs work great</li>
        <li>Code folding</li>
    </ol>             
                        </Rock:CodeEditor>
                    </div>
                    <p>
                        Alternately, you can provide the contents of the code to edit in the <code>Text</code> property of the control.
                    </p>


                    <a id="CssRollovers"></a>
                    <h2 runat="server">CSS Rollovers</h2>
                    You often run across situiations where you would like buttons or links to apprear when you hover over a selection of code. Instead of using jQuery toggles you can use the
                CSS classes below. These classes can be applied to any tags.  In order to support nested rollovers the actions must be direct decendents of their containers.  On touch enabled
                devices the rollover-items will always be displayed.
                <div runat="server" class="r-example">
                    <div class="alert alert-info rollover-container">
                        <em>(roll over the box to see effect)</em>
                        <div class="rollover-item pull-right">
                            <a class="btn btn-default btn-xs" href="#">Delete</a>
                            <a class="btn btn-default btn-xs" href="#">Export</a>
                        </div>
                    </div>
                </div>

                    <h2 runat="server">Rock jQuery UI Library</h2>
                    To help promote consistance we have created a standard Rock jQuery UI Library.  Below are the current functions with their usage patters.
            
                <a id="RockFadeIn"></a>
                    <h3 runat="server">rockFadeIn()</h3>
                    <p>
                        Use this to fade in a selected DOM object in. The function hides the selector and then fades it in. Using this object will help provide
                    consistant fade behavior.
                    </p>
                    <strong>Usage Examples</strong>
                    <ul>
                        <li>PrayerSession.ascx - Used when moving from one prayer request to another.</li>
                        <li>Check-in Layouts - Used to fade in the `&lt;body&gt;`</li>
                    </ul>
                    <div runat="server" class="r-example">
                        <div class="js-fadepanel alert alert-info">
                            I Fade In
                        </div>

                        <a href="#" class="js-fadebutton btn btn-sm btn-action">Press To Fade</a>

                        <script>
                            $('.js-fadebutton').on("click", function () {
                                $('.js-fadepanel').rockFadeIn();
                                return false;
                            });
                        </script>
                    </div>

                    <p>Tip: When used within an UpdatePanel, you'll want to add your fade-in handler to the <code>endRequest</code> event of the PageRequestManager similar to this:</p>
                    <div runat="server" class="r-example">
                        <script>
                            function FadePanelIn() {
                                $("[id$='upYourPanel']").rockFadeIn();
                            }

                            $(document).ready(function () { FadePanelIn(); });
                            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(FadePanelIn);
                        </script>
                    </div>

                </asp:Panel>

            </div>
        </div>


    </ContentTemplate>
</asp:UpdatePanel>

