<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockControlGallery.ascx.cs" Inherits="RockWeb.Blocks.Examples.RockControlGallery" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:LinkButton runat="server" ID="btnToggleLabels" Text="Toggle Labels" CausesValidation="false" OnClick="btnToggleLabels_Click"
                OnClientClick="javascript: $(':header').toggle()" />
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <h4 id="H1" runat="server">Rock:Grid</h4>
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
            
            <h4 runat="server">DropDowns</h4>
            <Rock:DataDropDownList ID="ddlDataExample" runat="server" LabelText="Rock:DataDropDownList/LabeledDropDownList" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
            <Rock:StateDropDownList ID="NotificationBox3" runat="server" LabelText="Rock:StateDropDownList" />
            <Rock:ButtonDropDownList ID="bddlExample" runat="server" LabelText="Rock:ButtonDropDownList" />

            <h4 runat="server">Input</h4>
            <Rock:DataTextBox ID="dt" runat="server" LabelText="Rock:DataTextBox" LabelTextFromPropertyName="false" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
            <Rock:LabeledCheckBox ID="labeledCheckBox" runat="server" LabelText="Rock:LabeledCheckBox" />
            <Rock:LabeledCheckBoxList ID="labeledCheckBoxList" runat="server" LabelText="Rock:LabeledCheckBoxList"  />
            <Rock:LabeledRadioButtonList ID="labeledRadioButtonList" runat="server" LabelText="Rock:LabeledRadioButtonList"  />
            <Rock:NumberRangeEditor ID="numberRangeEditor" runat="server" LabelText="Rock:NumberRangeEditor" LowerValue="10" UpperValue="25" />

            <h4 runat="server">Pickers</h4>
            <Rock:DatePicker ID="datePicker" runat="server" LabelText="Rock:DatePicker" />
            <Rock:DateTimePicker ID="dateTimePicker" runat="server" LabelText="Rock:DateTimePicker" />
            <Rock:TimePicker ID="timePicker" runat="server" LabelText="Rock:TimePicker" />
            <Rock:MonthYearPicker ID="monthYearPicker" runat="server" LabelText="Rock:MonthYearPicker" OnTextChanged="monthYearPicker_TextChanged" AutoPostBack="true" />
            <Rock:MonthDayPicker ID="monthDayPicker" runat="server" LabelText="Rock:MonthDayPicker" OnTextChanged="monthDayPicker_TextChanged" AutoPostBack="true" />
            <Rock:DateRangePicker ID="dateRangerPicker" runat="server" LabelText="Rock:DateRangePicker" LowerValue="1/1/2012" UpperValue="12/31/2014" />

            <Rock:GroupPicker ID="groupPicker" runat="server" LabelText="Rock:GroupPicker" />
            <Rock:CampusPicker ID="campusPicker" runat="server" LabelText="Rock:CampusPicker" />
            <Rock:PagePicker ID="pagePicker" runat="server" LabelText="Rock:PagePicker" />
            <Rock:PersonPicker ID="personPicker" runat="server" LabelText="Rock:PersonPicker" />

            <Rock:ScheduleBuilder ID="scheduleBuilder" runat="server" LabelText="Rock:ScheduleBuilder" />

            <Rock:AccountPicker ID="accountPicker" runat="server" LabelText="Rock:AccountPicker" />
            <Rock:CategoryPicker ID="categoryPicker" runat="server" LabelText="Rock:CategoryPicker" />
            <Rock:ComponentPicker ID="componentPicker" runat="server" LabelText="Rock:ComponentPicker" />


            <%-- 
                <Rock:MergeFieldPicker ID="LabeledCheckBox4" runat="server" LabelText="Rock:MergeFieldPicker" />
            --%>

            <h4 runat="server">BinaryFilePicker, BinaryFileTypePicker</h4>
            <Rock:BinaryFileTypePicker ID="binaryFileTypePicker" runat="server" OnSelectedIndexChanged="binaryFileTypePicker_SelectedIndexChanged" />
            <Rock:BinaryFilePicker ID="binaryFilePicker" runat="server" />

            <h4 runat="server">Misc</h4>
            <Rock:FieldTypeList ID="FieldTypeList" runat="server" LabelText="Rock:FieldTypeList" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
            <Rock:FileUploader ID="fup" runat="server" LabelText="Rock:FileUploader" />
            <Rock:ImageUploader ID="imageUploader" runat="server" LabelText="Rock:ImageUploader" />

            <Rock:NotificationBox ID="notificationBox" runat="server" Title="Rock:NotificationBox" Text="Box Text" />
            <Rock:Badge ID="badge" runat="server" BadgeType="Important" ToolTip="Some Tooltip">Badge</Rock:Badge>

            <h4>Rock:BootstrapButton</h4>
            <Rock:BootstrapButton ID="lbSave" runat="server" Text="Click Me" DataLoadingText="Disabled and Loading..." CssClass="btn btn-primary" />

            <h4 runat="server">Rock:AttributeEditor</h4>
            <asp:LinkButton ID="btnShowAttributeEditor" runat="server" CssClass="btn" Text="Attribute Editor..." OnClick="btnShowAttributeEditor_Click" CausesValidation="false" />
            <asp:Panel ID="aeExampleDiv" runat="server" Visible="false" CssClass="well">
                <Rock:AttributeEditor ID="aeExample" runat="server" OnCancelClick="aeExample_CancelClick" OnSaveClick="aeExample_SaveClick" />
            </asp:Panel>

            <h4 runat="server">Rock:HtmlEditor</h4>
            <Rock:HtmlEditor ID="htmlEdit" runat="server" LabelText="HtmlEditor" />

            

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

