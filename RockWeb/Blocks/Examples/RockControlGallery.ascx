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
            <Rock:DataDropDownList ID="ddlDataExample" runat="server" Label="Rock:DataDropDownList/LabeledDropDownList" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
            <Rock:StateDropDownList ID="NotificationBox3" runat="server" Label="Rock:StateDropDownList" />
            <Rock:ButtonDropDownList ID="bddlExample" runat="server" Label="Rock:ButtonDropDownList" />

            <h4 runat="server">Input</h4>
            <Rock:DataTextBox ID="dt" runat="server" Label="Rock:DataTextBox" LabelTextFromPropertyName="false" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
            <Rock:RockCheckBox ID="labeledCheckBox" runat="server" Label="Rock:RockCheckBox" />
            <Rock:RockCheckBoxList ID="labeledCheckBoxList" runat="server" Label="Rock:RockCheckBoxList"  />
            <Rock:RockRadioButtonList ID="labeledRadioButtonList" runat="server" Label="Rock:RockRadioButtonList"  />
            <Rock:NumberRangeEditor ID="numberRangeEditor" runat="server" Label="Rock:NumberRangeEditor" LowerValue="10" UpperValue="25" />

            <h4 runat="server">Pickers</h4>
            <Rock:DatePicker ID="datePicker" runat="server" Label="Rock:DatePicker" />
            <Rock:DateTimePicker ID="dateTimePicker" runat="server" Label="Rock:DateTimePicker" />
            <Rock:TimePicker ID="timePicker" runat="server" Label="Rock:TimePicker" />
            <Rock:MonthYearPicker ID="monthYearPicker" runat="server" Label="Rock:MonthYearPicker" OnTextChanged="monthYearPicker_TextChanged" AutoPostBack="true" />
            <Rock:MonthDayPicker ID="monthDayPicker" runat="server" Label="Rock:MonthDayPicker" OnTextChanged="monthDayPicker_TextChanged" AutoPostBack="true" />
            <Rock:DateRangePicker ID="dateRangerPicker" runat="server" Label="Rock:DateRangePicker" LowerValue="1/1/2012" UpperValue="12/31/2014" />

            <Rock:GroupPicker ID="groupPicker" runat="server" Label="Rock:GroupPicker" />
            <Rock:CampusPicker ID="campusPicker" runat="server" Label="Rock:CampusPicker" />
            <Rock:PagePicker ID="pagePicker" runat="server" Label="Rock:PagePicker" />
            <Rock:PersonPicker ID="personPicker" runat="server" Label="Rock:PersonPicker" />

            <Rock:ScheduleBuilder ID="scheduleBuilder" runat="server" Label="Rock:ScheduleBuilder" />

            <Rock:AccountPicker ID="accountPicker" runat="server" Label="Rock:AccountPicker" />
            <Rock:CategoryPicker ID="categoryPicker" runat="server" Label="Rock:CategoryPicker" />
            <Rock:ComponentPicker ID="componentPicker" runat="server" Label="Rock:ComponentPicker" />

            <Rock:GeoPicker ID="geoPicker" runat="server" Label="Rock:GeoPicker" DrawingMode="Point" />
            
            


            <%-- 
                <Rock:MergeFieldPicker ID="LabeledCheckBox4" runat="server" Label="Rock:MergeFieldPicker" />
            --%>

            <h4 runat="server">BinaryFilePicker, BinaryFileTypePicker</h4>
            <Rock:BinaryFileTypePicker ID="binaryFileTypePicker" runat="server" OnSelectedIndexChanged="binaryFileTypePicker_SelectedIndexChanged" />
            <Rock:BinaryFilePicker ID="binaryFilePicker" runat="server" />

            <h4 runat="server">Misc</h4>
            <Rock:FieldTypeList ID="FieldTypeList" runat="server" Label="Rock:FieldTypeList" SourceTypeName="Rock.Model.GroupType, Rock" PropertyName="Name" />
            <Rock:FileUploader ID="fup" runat="server" Label="Rock:FileUploader" />
            <Rock:ImageUploader ID="imageUploader" runat="server" Label="Rock:ImageUploader" />

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
            <Rock:HtmlEditor ID="htmlEdit" runat="server" Label="HtmlEditor" />

            

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

