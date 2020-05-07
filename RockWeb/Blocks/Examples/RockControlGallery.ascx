<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockControlGallery.ascx.cs" Inherits="RockWeb.Blocks.Examples.RockControlGallery" %>
<!-- add after bootstrap.min.css -->

<script type="text/javascript">
    Sys.Application.add_load(function () {
        prettyPrint();

        $(function () {
            var navSelector = '#toc';
            var $myNav = $(navSelector);
            var $exampleHeaders = $('.r-example-nocodepreview,.r-example').find('h1,h2,h3,h4');
            Toc.init({
                $nav: $myNav,
                $scope: $('h1,h2,h3,h4').not($exampleHeaders)
            });
            $('body').scrollspy({
                target: navSelector,
                offset: 80
            });
        });
    });
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
                <h1 class="panel-title" data-toc-skip="1">
                    <i class="fa fa-magic"></i>
                    Control Gallery
                </h1>
            </div>
            <div class="panel-body">
            <div class="row">
            <div class="col-lg-2 col-lg-offset-1 col-md-2" style="position:sticky;top:80px;"><nav id="toc"></nav></div>
            <div id="main-controls" class="col-md-9 col-lg-7">
                <asp:Panel ID="pnlDetails" runat="server">

                    <asp:ValidationSummary ID="valExample" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <h1 runat="server">General Information</h1>

                    <h2 id="input-sizing">Input Sizing Rules</h2>

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

                    <h2 runat="server">Horizontal Forms</h2>
                    <p>While Rock uses a similar approach to Bootstrap, we’ve made horizontal forms a bit easier to help facilitate their use when creating forms in workflows and event
                        registrations. Below is the syntax for declaring a horizontal form.
                    </p>
                    <div runat="server" class="r-example">
                        <div class="form-horizontal label-sm">
                            <div class="form-group">
                                <label for="inputEmail3" class="control-label">Email</label>
                                <div class="control-wrapper">
                                <input type="email" class="form-control" id="inputEmail7" placeholder="Email">
                                </div>
                            </div>
                        </div>
                    </div>

                    <p>When using this in form generators you'll need to complete two steps. The first is adding a wrapping <code>&lt;div class=&quot;form-group &quot;&gt;</code> in your pre/post fields.</p>

                    <p>The second is an additional class on the form-horizontal element that determines how wide the label column should be. Options include:</p>

                    <ul>
                        <li><strong>label-sm: </strong> Label column of 2, field column of 10</li>
                        <li><strong>label-md: </strong> Label column of 4, field column of 8</li>
                        <li><strong>label-lg: </strong> Label column of 6, field column of 6</li>
                        <li><strong>label-xl: </strong> Label column of 8, field column of 4</li>
                        <li><strong>label-auto: </strong> Label and field widths determined by contents</li>
                    </ul>

                    <div runat="server" class="r-example">
                        <div class="form-horizontal label-sm">
                            <div class="form-group">
                                <label for="inputEmail3" class="control-label">Email</label>
                                <div class="control-wrapper">
                                    <input type="email" class="form-control" id="inputEmail4" placeholder="Email">
                                </div>
                            </div>
                        </div>

                        <div class="form-horizontal label-md">
                            <div class="form-group">
                                <label for="inputEmail3" class="control-label">Email</label>
                                <div class="control-wrapper">
                                    <input type="email" class="form-control" id="inputEmail5" placeholder="Email">
                                </div>
                            </div>
                        </div>

                        <div class="form-horizontal label-lg">
                            <div class="form-group">
                                <label for="inputEmail3" class="control-label">Email</label>
                                <div class="control-wrapper">
                                    <input type="email" class="form-control" id="inputEmail6" placeholder="Email">
                                </div>
                            </div>
                        </div>

                        <div class="form-horizontal label-xl">
                            <div class="form-group">
                                <label for="inputEmail3" class="control-label">Email</label>
                                <div class="control-wrapper">
                                    <input type="email" class="form-control" id="inputEmail3" placeholder="Email">
                                </div>
                            </div>
                        </div>

                        <div class="form-horizontal label-auto">
                            <div class="form-group">
                                <label for="inputEmail3" class="control-label">Email Email Email Email Email Email Email Email Email Email Email</label>
                                <div class="control-wrapper">
                                    <input type="email" class="form-control" id="inputEmail4" placeholder="Email">
                                </div>
                            </div>
                        </div>

                    </div>


                    <h2 runat="server">Margins and Padding</h2>

                    <div class="alert alert-warning">
                        <p><strong>Warning!</strong></p>
                        If you think you need to control the margin or padding, you might be 'doing it wrong.'
                        <em>These are for use in those cases when you know what you're doing.</em>
                    </div>

                    <h3>Format</h3>
                    <p>
                        The format is the type (padding or margin) followed by a dash then the position (v=vertical, h=horizontal, t=top, etc.)
                        followed by a dash and then the sizing specifier (none, small, medium, etc).
                    </p>
                    <pre>.padding|margin - v|h|t|b|r|l|all - none|sm|md|lg|xl</pre>

<div runat="server" class="r-example">
<div class="well">
    <Rock:RockTextBox ID="tbMP1" runat="server" CssClass="margin-t-xl" Label=".margin-t-xl" Placeholder="Blah..."/>
</div>

<div class="well">
    <Rock:RockTextBox ID="tbMP2" runat="server" CssClass="padding-h-lg" Label=".padding-h-lg" Placeholder="Blah..." />
</div>

<div class="well">
    <label class="control-label">.padding-all-xl .margin-all-lg</label>
    <Rock:NotificationBox ID="nbMP3" runat="server" CssClass="padding-all-xl margin-all-lg" NotificationBoxType="Info" Title=".padding-all-xl .margin-all-md" Text="For God so loved the world that he gave his one and only Son..." />
</div>
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
                    <h1 runat="server">Drop Downs</h1>

                    <a id="DataDropDownList"></a>
                    <div runat="server" class="r-example">
                        <Rock:DataDropDownList ID="ddlDataExample" runat="server" Label="Rock:DataDropDownList/RockDropDownList" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
                    </div>

                    <a id="StateDropDownList"></a>
                    <h2>State Drop Down List</h2>
                    <div runat="server" class="r-example">
                        <Rock:StateDropDownList ID="statepExample" runat="server" Label="Rock:StateDropDownList" />
                    </div>

                    <a id="ButtonDropDownList"></a>
                    <h2>Button Drop Down List</h2>
                    <div runat="server" class="r-example">
                        <Rock:ButtonDropDownList ID="bddlExample" runat="server" Label="Rock:ButtonDropDownList" />
                    </div>

                    <a id="ButtonDropDownListCheckMark"></a>
                    <h2>Button Drop Down List Checkmark</h2>
                    <div runat="server" class="r-example">
                        <Rock:ButtonDropDownList ID="bddlExampleCheckmark" runat="server" Label="Rock:ButtonDropDownList with Checkmark" SelectionStyle="Checkmark" Title="T-Shirt Size" />
                    </div>

                    <a id="Input"></a>
                    <h1 runat="server">Inputs</h1>


                    <a id="DataTextBox"></a>
                    <h2>Text Box</h2>
                    <div runat="server" class="r-example">
                        <Rock:DataTextBox ID="dtbExample" runat="server" Label="Rock:DataTextBox" LabelTextFromPropertyName="false" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Description" />
                    </div>

                    <a id="EmailBox"></a>
                    <h2>Email Box</h2>
                    <div runat="server" class="r-example">
                        <Rock:EmailBox ID="ebEmail" runat="server" Label="Rock:EmailBox" />
                    </div>

                    <a id="UrlLinkBox"></a>
                    <h2>URL Link Box</h2>
                    <div runat="server" class="r-example">
                        <Rock:UrlLinkBox ID="ulLink" runat="server" Label="Rock:UrlLinkBox" />
                    </div>

                    <a id="NumberBox"></a>
                    <h2>Number Box</h2>
                    <div runat="server" class="r-example">
                        <Rock:NumberBox ID="numbExample" runat="server" Label="Rock:NumberBox" />
                    </div>

                    <a id="AddressControl"></a>
                    <h2>Address Control</h2>
                    <div runat="server" class="r-example">
                        <Rock:AddressControl ID="addrExample" runat="server" Label="Rock:AddressControl" />
                    </div>

                    <a id="NumberUpDown"></a>
                    <h2>Number Up Down (Stepper)</h2>
                    <div runat="server" class="r-example">
                        <Rock:NumberUpDown ID="nudExample" runat="server" Label="Rock:NumberUpDown" Minimum="0" Maximum="5" />
                    </div>

                    <a id="RockCheckBox"></a>
                    <h2>Check Box</h2>
                    <div runat="server" class="r-example">
                        <Rock:RockCheckBox ID="cbExample" runat="server" Label="Rock:RockCheckBox" />
                    </div>

                    <a id="RockCheckBox2"></a>
                    <div runat="server" class="r-example">
                        <Rock:RockCheckBox ID="cbExample2" runat="server" Label="Rock:RockCheckBox" SelectedIconCssClass="fa fa-check-square-o fa-lg" UnSelectedIconCssClass="fa fa-square-o fa-lg" />
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
                    <h2>Radio Button</h2>
                    <div runat="server" class="r-example">
                        <Rock:RockRadioButtonList ID="rblExample" runat="server" Label="Rock:RockRadioButtonList" />
                    </div>

                    <a id="RockRadioButtonListHorizontal"></a>
                    <div runat="server" class="r-example">
                        <Rock:RockRadioButtonList ID="rblExampleHorizontal" runat="server" Label="Rock:RockRadioButtonList (horizontal)" RepeatDirection="Horizontal" />
                    </div>

                    <a id="RockSwitch"></a>
                    <div runat="server" class="r-example">
                        <Rock:Switch ID="swExample" runat="server" Label="Rock:Switch" Text="Rock:Switch" />
                    </div>

                    <a id="RockListItems"></a>
                    <div runat="server" class="r-example">
                        <Rock:ListItems ID="liExample" runat="server" Label="Rock:ListItems"></Rock:ListItems>
                    </div>

                    <a id="NumberRangeEditor"></a>
                    <h2>Number Range</h2>
                    <div runat="server" class="r-example">
                        <Rock:NumberRangeEditor ID="nreExample" runat="server" Label="Rock:NumberRangeEditor" LowerValue="10" UpperValue="25" />
                    </div>

                    <a id="RatingInput"></a>
                    <h2>Rating Input</h2>
                    <div runat="server" class="r-example">
                        <Rock:RockRating ID="rrRating" runat="server" Label="Rock:RatingInput" /><br />
                    </div>

                    <a id="RangeSlider"></a>
                    <h2>Range Slider</h2>
                    <div runat="server" class="r-example">
                        <Rock:RangeSlider ID="rsSlider" runat="server" Label="Rock:RangeSlider" MaxValue="250" MinValue="125" StepValue="5" SelectedValue="200" />
                        <br />
                    </div>

                    <div runat="server" class="r-example">
                        <Rock:RangeSlider ID="rsSlider2" runat="server" Label="Rock:RangeSlider" />
                        <br />
                    </div>

                    <asp:LinkButton ID="lbTestSlider" runat="server" CssClass="btn btn-default" Text="Test" OnClick="lbTestSlider_Click" />

                    <a id="Pickers"></a>
                    <h1 runat="server">Pickers</h1>

                    <h2>Date/Time</h2>

                    <a id="DatePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:DatePicker ID="dpExample" runat="server" Label="Rock:DatePicker" />
                    </div>

                    <a id="DatePartsPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:DatePartsPicker ID="dppExample" runat="server" Label="Rock:DatePartsPicker" OnSelectedDatePartsChanged="dppExample_SelectedDatePartsChanged" />
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

                    <a id="SlidingDateRangePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:SlidingDateRangePicker ID="sdrpExample" runat="server" Label="Rock:SlidingDateRangePicker" />
                    </div>

                    <a id="BirthdayPicker"></a>
                    <div id="Div4" runat="server" class="r-example">
                        <Rock:BirthdayPicker ID="bdaypExample" runat="server" Label="Rock:BirthdayPicker" OnSelectedBirthdayChanged="birthdayPicker_SelectedBirthdayChanged" />
                    </div>

                    <a id="YearPicker"></a>
                    <div id="Div1" runat="server" class="r-example">
                        <Rock:YearPicker ID="ypYearPicker" runat="server" Label="Rock:YearPicker"  />
                    </div>

                    <h2>Campus</h2>

                    <a id="CampusPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:CampusPicker ID="campExample" runat="server" Label="Rock:CampusPicker" ForceVisible="true" />
                    </div>

                    <a id="CampusesPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:CampusesPicker ID="campsExample" runat="server" Label="Rock:CampusesPicker" ForceVisible="true" />
                    </div>

                    <a id="Connections"></a>
                    <h2 runat="server">Connections</h2>

                    <a id="ConnectionRequestPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:ConnectionRequestPicker ID="crpConnectionRequestPicker" runat="server" Label="Rock:ConnectionRequestPicker" />
                    </div>



                    <a id="DefinedValues"></a>
                    <h2 runat="server">DefinedValues</h2>

                    <a id="DefinedValuePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:DefinedValuePicker ID="dvpDefinedValuePicker" runat="server" Label="Rock:DefinedValuePicker for ConnectionStatus defined type" DefinedTypeId="4" />
                    </div>

                    <a id="DefinedValuesPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:DefinedValuesPicker ID="dvpDefinedValuesPicker" runat="server" Label="Rock:DefinedValuesPicker for ConnectionStatus defined type" DefinedTypeId="4" />
                    </div>

                    <a id="DefinedValuesPickerEnhanced"></a>
                    <div runat="server" class="r-example">
                        <Rock:DefinedValuesPickerEnhanced ID="dvpDefinedValuesPickerEnhanced" runat="server" Label="Rock:DefinedValuesPickerEnhanced for ConnectionStatus defined type" DefinedTypeId="4" />
                    </div>

                    <h2>Events</h2>

                    <a id="EventCalendarPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:EventCalendarPicker ID="ecpEventCalendarPicker" runat="server" Label="Rock:EventCalendarPicker" />
                    </div>

                    <a id="EventItemPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:EventItemPicker ID="eipEventItemPicker" runat="server" Label="Rock:EventItemPicker" />
                    </div>

                    <a id="RegistrationTemplatePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:RegistrationTemplatePicker ID="pRegistrationTemplatePicker" runat="server" Label="Rock:RegistrationTemplatePicker" />
                    </div>

                    <h2>Financial</h2>

                     <a id="AccountPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:AccountPicker ID="acctpExample" runat="server" Label="Rock:AccountPicker" />
                    </div>

                    <a id="FinancialGatewayPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:FinancialGatewayPicker ID="fgpFinancialGatewayPicker" runat="server" Label="Rock:FinancialGatewayPicker"/>
                    </div>

                    <h2>Files and Images</h2>

                    <a id="BinaryFileTypePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:BinaryFileTypePicker ID="bftpExample" runat="server" Label="Rock:BinaryFileTypePicker" OnSelectedIndexChanged="binaryFileTypePicker_SelectedIndexChanged" />
                    </div>

                    <a id="BinaryFilePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:BinaryFilePicker ID="bfpExample" runat="server" Label="Rock:BinaryFilePicker" />
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

                    <h2>Groups and Group Types</h2>

                    <a id="GroupPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:GroupPicker ID="gpExample" runat="server" Label="Rock:GroupPicker" />
                    </div>

                    <a id="GroupPicker2"></a>
                    <div runat="server" class="r-example">
                        <Rock:GroupPicker ID="grExampleMultip" runat="server" Label="Rock:GroupPicker (Multiselect)" AllowMultiSelect="true" />
                    </div>

                    <a id="GroupTypePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:GroupTypePicker ID="gpGroupType" runat="server" Label="Rock:GroupTypePicker" />
                    </div>

                    <a id="GroupTypesPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:GroupTypesPicker ID="gpGroupTypes" runat="server" Label="Rock:GroupTypesPicker" />
                    </div>

                    <a id="GroupRolePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:GroupRolePicker ID="grpExample" runat="server" Label="Rock:GroupRolePicker" />
                    </div>

                    <h2 runat="server">Interval</h2>
                    <a id="IntervalPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:IntervalPicker ID="ipExample1" runat="server" Label="Rock:IntervalPicker" />
                    </div>

                    <div runat="server" class="r-example">
                        <Rock:IntervalPicker ID="ipExample2" DefaultValue="2" DefaultInterval="Day" runat="server" Label="Rock:IntervalPicker (Default Value, Default Interval)" />
                    </div>

                    <h2 runat="server">Locations</h2>

                    <a id="LocationPicker"></a>
                    <div id="Div2" runat="server" class="r-example">
                        <Rock:LocationPicker ID="locpExample" runat="server" Label="Rock:LocationPicker" />
                    </div>

                    <a id="LocationPicker2"></a>
                    <div runat="server" class="r-example">
                        <Rock:LocationPicker ID="locpExampleAddressMode" runat="server" Label="Rock:LocationPicker (Address Mode, Mode Selection disabled)" CurrentPickerMode="Address" AllowedPickerModes="Address" />
                    </div>

                    <a id="GeoPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:GeoPicker ID="geopExamplePoint" runat="server" Label="Rock:GeoPicker (Point mode)" DrawingMode="Point" MapStyleValueGuid="BFC46259-FB66-4427-BF05-2B030A582BEA" />
                    </div>

                    <a id="GeoPickerPolygon"></a>
                    <div runat="server" class="r-example">
                        <Rock:GeoPicker ID="geopExamplePolygon" runat="server" Label="Rock:GeoPicker (Polygon mode)" DrawingMode="Polygon" Help="You can set the style of this through the 'Map Style' block attribute." Warning="If you need to stipulate restrictions, use the warning property." />
                    </div>

                    <h2>Pages</h2>

                    <a id="PagePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:PagePicker ID="pagepExample" runat="server" Label="Rock:PagePicker" />
                    </div>

                    <h2>Person Pickers</h2>

                    <a id="PersonPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:PersonPicker ID="ppExample" runat="server" Label="Rock:PersonPicker" />
                    </div>

                    <a id="PersonAndBusinessPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:PersonPicker ID="ppBusinessExample" runat="server" Label="Rock:PersonPicker including businesses" IncludeBusinesses="true" />
                    </div>

                    <a id="PersonPickerEnableSelfSelection"></a>
                    <div runat="server" class="r-example">
                        <Rock:PersonPicker ID="ppSelfSelect" runat="server" Label="Rock:PersonPicker with Self Selection" EnableSelfSelection="true" />
                    </div>

                    <a id="GradePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:GradePicker ID="pGradePicker" runat="server" Label="Rock:GradePicker" />
                    </div>

                    <h2>Lava Commands</h2>

                    <a id="LavaCommandsPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:LavaCommandsPicker ID="pLavaCommandsPicker" runat="server" Label="Rock:LavaCommandsPicker" />
                    </div>

                    <a id="MergeFieldPicker"></a>
                    <h2>Lava Merge Fields</h2>
                    <div runat="server" class="r-example">
                        <Rock:MergeFieldPicker ID="mfpExample" runat="server" Label="Rock:MergeFieldPicker" />
                    </div>

                    <a id="MergeTemplatePicker"></a>
                    <h2>Merge Fields Templates</h2>
                    <div runat="server" class="r-example">
                        <Rock:MergeTemplatePicker ID="pMergeTemplatePicker" runat="server" Label="Rock:MergeTemplatePicker" />
                    </div>

                    <h2>Reporting</h2>

                    <a id="DataViewItemPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:DataViewItemPicker ID="dvpDataViewPicker" runat="server" Label="Rock:DataViewItemPicker for Person Dataviews" EntityTypeId="15" />
                    </div>

                    <a id="DataViewsPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:DataViewsPicker ID="dvpDataViewsPicker" runat="server" Label="Rock:DataViewsPicker for Person Dataviews" EntityTypeId="15"/>
                    </div>

                    <a id="ReportPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:ReportPicker ID="rpReports" runat="server" Label="Rock:ReportPicker for Person Reports" EntityTypeId="15" />
                    </div>

                    <a id="MetricCategoryPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:MetricCategoryPicker ID="pMetricCategoryPicker" runat="server" Label="Rock:MetricCategoryPicker (Pick Metric from Category Tree)" EntityTypeId="15"/>
                    </div>

                    <h2>Workflows</h2>

                    <a id="WorkflowTypePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:WorkflowTypePicker ID="wftpExample" runat="server" Label="Rock:WorkflowTypePicker" />
                    </div>


                    <a id="WorkflowActionTypePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:WorkflowActionTypePicker ID="wfatpExample" runat="server" Label="Rock:WorkflowActionTypePicker" />
                    </div>

                    <a id="WorkflowPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:RockControlWrapper ID="rcwWorkflowPicker" runat="server" Label="Workflow Picker">
                            <Rock:WorkflowPicker ID="pWorkflowPicker" runat="server" FormGroupCssClass="margin-l-md margin-t-sm" Label="Workflow Type" />
                        </Rock:RockControlWrapper>
                    </div>

                    <h2>Other Pickers</h2>

                    <a id="CategoryPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:CategoryPicker ID="catpExample" runat="server" Label="Rock:CategoryPicker" />
                    </div>

                    <a id="ComponentPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:ComponentPicker ID="compExample" runat="server" Label="Rock:ComponentPicker" />
                    </div>

                    <a id="EntityPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:EntityPicker ID="epEntityPicker" runat="server" Label="Rock:EntityPicker" />
                    </div>

                    <a id="FieldTypePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:FieldTypePicker ID="ftlExample" runat="server" Label="Rock:FieldTypePicker" />
                    </div>



                    <a id="RemoteAuthsPicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:RemoteAuthsPicker ID="pRemoteAuthsPicker" runat="server" Label="Rock:RemoteAuthsPicker" />
                    </div>

                    <a id="Schedules"></a>
                    <h1 runat="server">Schedules</h1>

                    <a id="SchedulePicker"></a>
                    <div runat="server" class="r-example">
                        <Rock:SchedulePicker ID="spSchedulePicker" runat="server" Label="Rock:SchedulePicker" />
                    </div>

                    <a id="ScheduleBuilder"></a>
                    <div runat="server" class="r-example">
                        <Rock:ScheduleBuilder ID="schedbExample" runat="server" Label="Rock:ScheduleBuilder" OnSaveSchedule="scheduleBuilder_SaveSchedule" />
                    </div>

                    <a id="Misc"></a>
                    <h1 runat="server">Misc</h1>



                    <a id="Notificationbox"></a>
                    <h2 runat="server">Rock:NotificationBox</h2>
                    <p>
                        This creates a <a href="http://getbootstrap.com/components/#alerts">Bootstrap alert</a>.  We’ve added the ability to have Details that can be shown.
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
                        but we’ve added a few additional custom <code>LabelType</code> options to control the color.
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

                    <a id="ButtonGroup"></a>
                    <h2 runat="server">Rock:ButtonGroup</h2>
                    <div runat="server" class="r-example">
                        <Rock:ButtonGroup ID="bgExample" runat="server" Label="Favorite Fruit">
                            <asp:ListItem Text="Apple" Value="1" />
                            <asp:ListItem Text="Banana" Value="2" />
                            <asp:ListItem Text="Strawberry" Value="3" />
                            <asp:ListItem Text="Chicken" Value="4" />
                        </Rock:ButtonGroup>
                    </div>

                    <a id="BootstrapButton"></a>
                    <h2 runat="server">Rock:BootstrapButton</h2>
                    <div runat="server" class="r-example">
                        <Rock:BootstrapButton ID="lbSave" runat="server" Text="Click Me" CssClass="btn btn-primary"
                            DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving"
                            CompletedText ="Done" CompletedMessage="<div class='margin-t-md alert alert-success'>Changes have been saved.</div>" CompletedDuration="3"/>
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
                    <h2>Rock:HtmlEditor (Full)</h2>
                    <div runat="server" class="r-example">
                        <Rock:HtmlEditor ID="htmlEditorFull" runat="server" Label="HtmlEditor" Toolbar="Full" />
                    </div>

                    <a id="HtmlEditorLight"></a>
                    <h2>Rock:HtmlEditor (Light)</h2>
                    <div runat="server" class="r-example">
                        <Rock:HtmlEditor ID="htmlEditorLight" runat="server" Label="HtmlEditor" Toolbar="Light" />
                    </div>

                    <a id="CodeEditor"></a>
                    <h2 runat="server">Rock:CodeEditor</h2>
                    <div runat="server" class="r-example">
                        <Rock:CodeEditor ID="ceScript" runat="server" EditorTheme="Rock" Label="Script" EditorMode="Html" EditorHeight="300">
    <h3>Hello!!!</h3>
    <p>This is a great way to edit HTML! Reasons:</p>

    <!-- Comment
         We shouldn't have to explain why this is better than just a
         textarea but we will just for you...
    -->

    <ol class="reasons">
        <li>Syntax highlighting</li>
        <li>Tabs work great</li>
        <li>Code folding</li>
    </ol>
                        </Rock:CodeEditor>
                    </div>
                    <p>
                        Alternately, you can provide the contents of the code to edit in the <code>Text</code> property of the control.
                    </p>


                    <a id="MarkdownEditor"></a>
                    <div runat="server" class="r-example-nocodepreview">
                    <h2 runat="server">Rock:MarkdownEditor</h2>
                        <asp:LinkButton ID="btnMarkdownPreview" runat="server" CssClass="pull-right btn btn-xs btn-action" Text="Convert to HTML" OnClick="btnMarkdownPreview_Click" />
                        <Rock:MarkdownEditor ID="mdMarkdownEditor" runat="server" CssClass="margin-t-sm" Label="Rock:MarkdownEditor" EditorHeight="400">
*Italic*
**Bold**


[Link](http://www.rockrms.com)

![Image](/Assets/Images/rock-logo-black.svg)

> Blockquote

* Apples
  * Red
  * Green
  * Blue
* Bananas
* Oranges

1. One
2. Two
3. Three

`Inline code` with backticks
```
# code block
print '3 backticks or'
print 'indent 4 spaces'
```

Horizontal Rule

---

                        </Rock:MarkdownEditor>

                        <asp:Literal ID="lMarkdownHtml" runat="server" />
                    </div>


                    <a id="CampusAccountAmountPicker"></a>
                    <h2 runat="server">Rock:CampusAccountAmountPicker</h2><span>(SingleAccount Mode)</span>
                    <div runat="server" class="r-example">
                        <Rock:CampusAccountAmountPicker ID="caapExampleSingleAccount" runat="server" AmountEntryMode="SingleAccount" AutoPostBack="true" OnAccountChanged="caapExample_Changed" />

                        <hr />
                        <Rock:RockLiteral ID="lCaapExampleSingleAccountResultAccount" runat="server" Label="Resulting Campus Account" Text="-"/>
                    </div>

                    <h2 runat="server">Rock:CampusAccountAmountPicker</h2><span>(MultipleAccounts Mode)</span>
                    <div runat="server" class="r-example">
                        <Rock:CampusAccountAmountPicker ID="caapExampleMultiAccount" runat="server" AmountEntryMode="MultipleAccounts" OnAccountChanged="caapExample_Changed"/>

                         <hr />
                        <Rock:RockLiteral ID="lCaapExampleMultiAccountResultAccount" runat="server" Label="Resulting Campus Accounts" Text="-" />
                    </div>


                    <a id="CssRollovers"></a>
                    <h2 runat="server">CSS Rollovers</h2>
                    You often run across situations where you would like buttons or links to appear when you hover over a selection of code. Instead of using jQuery toggles you can use the
                CSS classes below. These classes can be applied to any tags.  In order to support nested rollovers the actions must be direct descendants of their containers. On touch enabled
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
                    To help promote consistence we have created a standard Rock jQuery UI Library. Below are the current functions with their usage patterns.

                <a id="RockFadeIn"></a>
                    <h3 runat="server">rockFadeIn()</h3>
                    <p>
                        Use this to fade in a selected DOM object in. The function hides the selector and then fades it in. Using this object will help provide
                    consistent fade behavior.
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
</div></div>
            </div>
        </div>


    </ContentTemplate>
</asp:UpdatePanel>

