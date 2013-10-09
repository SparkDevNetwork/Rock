<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockControlGallery.ascx.cs" Inherits="RockWeb.Blocks.Examples.RockControlGallery" %>
<script type="text/javascript">
    function pageLoad() {
        prettyPrint();
    }
</script>
<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:LinkButton runat="server" ID="btnToggleLabels" Text="Toggle Labels" CausesValidation="false" OnClick="btnToggleLabels_Click"
                OnClientClick="javascript: $(':header').toggle()" />
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" />

            <h1>General Information</h1>

            <h2>Input Sizing Rules</h2>

            <div class="alert alert-warning">
                <p><strong>Warning!</strong></p> In Bootstrap 3 inputs are meant to fill the width of their parent container (<a href="http://getbootstrap.com/css/#forms-control-sizes" class="alert-link">link</a>).  If a small input is desired they should
                be wrapped in a table grid.  This provides the best responsive solution.  In some rare cases it's beneficial to be able to fix the width of
                certain inputs to provide better context of what the input is for.  For instance a credit card CVV field makes more sense visually being
                fixed width to 3 characters.  To provide this capability we have added the following CSS classes to fix width inputs.  <em>Please use them
                sparingly.</em>
            </div>

            <div class="alert alert-danger">
                <p><strong>Alert</strong></p> Rock framework developers should get approval from the Core Team before using these styles.

            </div>

            <div runat="server" class="r-example">
                <Rock:DataTextBox ID="DataTextBoxXs" runat="server" CssClass="input-width-xs" Label=".input-width-xs" LabelTextFromPropertyName="false" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />

                <Rock:DataTextBox ID="DataTextBox1" runat="server" CssClass="input-width-sm" Label=".input-width-sm" LabelTextFromPropertyName="false" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />

                <Rock:DataTextBox ID="DataTextBox2" runat="server" CssClass="input-width-md" Label=".input-width-md" LabelTextFromPropertyName="false" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />

                <Rock:DataTextBox ID="DataTextBox3" runat="server" CssClass="input-width-lg" Label=".input-width-lg" LabelTextFromPropertyName="false" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />

                <Rock:DataTextBox ID="DataTextBox4" runat="server" CssClass="input-width-xl" Label=".input-width-xl" LabelTextFromPropertyName="false" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />

                <Rock:DataTextBox ID="DataTextBox5" runat="server" CssClass="input-width-xxl" Label=".input-width-xxl" LabelTextFromPropertyName="false" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
            </div>

            <div class="alert alert-info">
                <p><strong>Note</strong></p> In Bootstrap 3 inputs are <em>display:block;</em>. If you need these sized controls to align horizontally, consider wrapping them with the <em>form-control-group</em> class.
            </div>



            <h2>Rock:Grid</h2>
            <div runat="server" class="r-example">
                <Rock:Grid ID="gExample" runat="server" AllowSorting="true">
                    <Columns>
                        <Rock:ColorField DataField="DefinedValueColor" ToolTipDataField="DefinedValueTypeName" HeaderText="" />
                        <asp:BoundField DataField="DefinedValueTypeName" HeaderText="Name" SortExpression="DefinedValueTypeName" />
                        <Rock:DateTimeField DataField="SomeDateTime" HeaderText="DateTime" SortExpression="SomeDateTime" />
                        <Rock:BoolField DataField="SomeBoolean" HeaderText="Some Boolean" SortExpression="SomeBoolean" />
                        <Rock:EditValueField />
                        <Rock:EditField />
                        <Rock:SecurityField />
                        <Rock:DeleteField />
                    </Columns>
                </Rock:Grid>
            </div>

            <a id="DropDowns"></a>
            <h1>DropDowns</h1>

            <div runat="server" class="r-example">
                <Rock:DataDropDownList ID="ddlDataExample" runat="server" Label="Rock:DataDropDownList/RockDropDownList" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
            </div>

            <div runat="server" class="r-example">
                <Rock:StateDropDownList ID="NotificationBox3" runat="server" Label="Rock:StateDropDownList" />
            </div>

            <div runat="server" class="r-example">
                <Rock:ButtonDropDownList ID="bddlExample" runat="server" Label="Rock:ButtonDropDownList" />
            </div>

            <a id="Input"></a>
            <h1>Input</h1>

            <div runat="server" class="r-example">
                <Rock:DataTextBox ID="dt" runat="server" Label="Rock:DataTextBox" LabelTextFromPropertyName="false" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
            </div>

            <div runat="server" class="r-example">
                <Rock:NumberBox ID="numberBox" runat="server" Label="Rock:NumberBox" />
            </div>

            <div runat="server" class="r-example">
                <Rock:RockCheckBox ID="RockCheckBox" runat="server" Label="Rock:RockCheckBox" />
            </div>

            <div runat="server" class="r-example">
                <Rock:RockCheckBoxList ID="RockCheckBoxList" runat="server" Label="Rock:RockCheckBoxList" />
            </div>

            <div runat="server" class="r-example">
                <Rock:RockCheckBoxList ID="RockCheckBoxList1" runat="server" Label="Rock:RockCheckBoxList (horizontal)" RepeatDirection="Horizontal" />
            </div>

            <div runat="server" class="r-example">
                <Rock:RockRadioButtonList ID="RockRadioButtonList" runat="server" Label="Rock:RockRadioButtonList" />
            </div>

            <div runat="server" class="r-example">
                <Rock:RockRadioButtonList ID="RockRadioButtonList1" runat="server" Label="Rock:RockRadioButtonList (horizontal)" RepeatDirection="Horizontal" />
            </div>

            <div runat="server" class="r-example">
                <Rock:NumberRangeEditor ID="NumberRangeEditor" runat="server" Label="Rock:NumberRangeEditor" LowerValue="10" UpperValue="25" />
            </div>

            <a id="Pickers"></a>
            <h1>Pickers</h1>

            <div runat="server" class="r-example">
                <Rock:DatePicker ID="datePicker" runat="server" Label="Rock:DatePicker" />
            </div>

            <div runat="server" class="r-example">
                <Rock:DateTimePicker ID="dateTimePicker" runat="server" Label="Rock:DateTimePicker" />
            </div>

            <div runat="server" class="r-example">
                <Rock:TimePicker ID="timePicker" runat="server" Label="Rock:TimePicker" />
            </div>

            <div runat="server" class="r-example">
                <Rock:MonthYearPicker ID="monthYearPicker" runat="server" Label="Rock:MonthYearPicker" OnSelectedMonthYearChanged="monthYearPicker_SelectedMonthYearChanged" />
            </div>

            <div runat="server" class="r-example">
                <Rock:MonthDayPicker ID="monthDayPicker" runat="server" Label="Rock:MonthDayPicker" OnSelectedMonthDayChanged="monthDayPicker_SelectedMonthDayChanged" />
            </div>

            <div runat="server" class="r-example">
                <Rock:DateRangePicker ID="dateRangerPicker" runat="server" Label="Rock:DateRangePicker" LowerValue="1/1/2012" UpperValue="12/31/2014" />
            </div>

            <div runat="server" class="r-example">
                <Rock:GroupPicker ID="groupPicker" runat="server" Label="Rock:GroupPicker" />
            </div>

            <div runat="server" class="r-example">
                <Rock:CampusPicker ID="campusPicker" runat="server" Label="Rock:CampusPicker" />
            </div>

            <div runat="server" class="r-example">
                <Rock:PagePicker ID="pagePicker" runat="server" Label="Rock:PagePicker" />
            </div>

            <div runat="server" class="r-example">
                <Rock:PersonPicker ID="personPicker" runat="server" Label="Rock:PersonPicker" />
            </div>

            <div runat="server" class="r-example">
                <Rock:AccountPicker ID="accountPicker" runat="server" Label="Rock:AccountPicker" />
            </div>

            <div runat="server" class="r-example">
                <Rock:CategoryPicker ID="categoryPicker" runat="server" Label="Rock:CategoryPicker" />
            </div>

            <div runat="server" class="r-example">
                <Rock:ComponentPicker ID="componentPicker" runat="server" Label="Rock:ComponentPicker" />
            </div>

            <a id="LocationPicker"></a>
            <div runat="server" class="r-example">
                <Rock:LocationPicker ID="locationPicker" runat="server" Label="Rock:LocationPicker (Named Location Mode)" LimitToNamedLocations="false" />
            </div>

            <div id="Div1" runat="server" class="r-example">
                <Rock:GroupRolePicker ID="groupRolePicker" runat="server" Label="Rock:GroupRolePicker" />
            </div>

            <a id="Other"></a>
            <h1>Other</h1>

            <div runat="server" class="r-example">
                <Rock:ScheduleBuilder ID="scheduleBuilder" runat="server" Label="Rock:ScheduleBuilder" OnSaveSchedule="scheduleBuilder_SaveSchedule" />
            </div>
            <%--
            <div runat="server" class="r-example">
                <Rock:GeoPicker ID="geoPicker" runat="server" Label="Rock:GeoPicker (Point mode)" DrawingMode="Point" /> 
            </div>

            <div runat="server" class="r-example">
                <Rock:GeoPicker ID="geoPicker1" runat="server" Label="Rock:GeoPicker (Polygon mode)" DrawingMode="Polygon" /> 
            </div>

            --%>

            <%-- 
                <Rock:MergeFieldPicker ID="LabeledCheckBox4" runat="server" Label="Rock:MergeFieldPicker" />
            --%>

            <h2>BinaryFilePicker, BinaryFileTypePicker</h2>

            <div runat="server" class="r-example">
                <Rock:BinaryFileTypePicker ID="binaryFileTypePicker" runat="server" Label="Rock:BinaryFileTypePicker" OnSelectedIndexChanged="binaryFileTypePicker_SelectedIndexChanged" />
            </div>

            <div runat="server" class="r-example">
                <Rock:BinaryFilePicker ID="binaryFilePicker" runat="server" Label="Rock:BinaryFilePicker" />
            </div>

            <a id="Misc"></a>
            <h1>Misc</h1>

            <div runat="server" class="r-example">
                <Rock:FieldTypeList ID="FieldTypeList" runat="server" Label="Rock:FieldTypeList" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
            </div>

            <div runat="server" class="r-example">
                <Rock:FileUploader ID="fup" runat="server" Label="Rock:FileUploader" />
            </div>

            <div runat="server" class="r-example">
                <Rock:ImageUploader ID="imageUploader" runat="server" Label="Rock:ImageUploader" />
            </div>

            <div runat="server" class="r-example">
                <Rock:NotificationBox ID="notificationBox" runat="server" Title="Rock:NotificationBox" Text="Box Text" />
            </div>

            <h2>Rock:Badge</h2>
            <div runat="server" class="r-example">
                <Rock:Badge ID="badge" runat="server" ToolTip="you have new messages waiting">99</Rock:Badge>
            </div>

            <h2>Rock:HighlightLabel</h2>
            <div runat="server" class="r-example">
                <Rock:HighlightLabel ID="hlDefault" runat="server" LabelType="Default" Text="Default" />
                <Rock:HighlightLabel ID="hlPrimary" runat="server" LabelType="Primary" Text="Primary" />
                <Rock:HighlightLabel ID="hlSuccess" runat="server" LabelType="Success" Text="Success" />
                <Rock:HighlightLabel ID="hlInfo" runat="server" LabelType="Info" Text="Info" />
                <Rock:HighlightLabel ID="hlWarning" runat="server" LabelType="Warning" Text="Warning" />
                <Rock:HighlightLabel ID="hlDanger" runat="server" LabelType="Danger" Text="Danger" />
                <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" Text="Campus" />
                <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" Text="Type" />
                <Rock:HighlightLabel ID="hlCustom" runat="server" LabelType="Custom" CustomClass="danger" Text="Custom" />
            </div>

            <h2>Rock:BootstrapButton</h2>
            <div runat="server" class="r-example">
                <Rock:BootstrapButton ID="lbSave" runat="server" Text="Click Me"
                    DataLoadingText="<i class='icon-spinner icon-spin icon-large'></i> Saving" CssClass="btn btn-primary" />
            </div>

            <h2>Rock:AttributeEditor</h2>
            <div runat="server" class="r-example">
                <asp:LinkButton ID="btnShowAttributeEditor" runat="server" CssClass="btn" Text="Attribute Editor..." OnClick="btnShowAttributeEditor_Click" CausesValidation="false" />
                <asp:Panel ID="aeExampleDiv" runat="server" Visible="false" CssClass="well">
                    <Rock:AttributeEditor ID="aeExample" runat="server" OnCancelClick="aeExample_CancelClick" OnSaveClick="aeExample_SaveClick" />
                </asp:Panel>
            </div>

            <h2>Rock:HtmlEditor</h2>
            <div runat="server" class="r-example">
                <Rock:HtmlEditor ID="htmlEdit" runat="server" Label="HtmlEditor" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

