<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedTypes.ascx.cs" Inherits="RockWeb.Blocks.Administration.DefinedTypes" %>

<asp:UpdatePanel ID="upSettings" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-massage error"/>
    
    <asp:Panel ID="pnlTypes" runat="server">

        <Rock:GridFilter ID="tFilter" runat="server">
            <Rock:LabeledDropDownList ID="ddlCategoryFilter" runat="server" LabelText="Category" AutoPostBack="true" />            
        </Rock:GridFilter>

        <Rock:Grid ID="rGridType" runat="server" AllowSorting="true" RowItemText="setting" OnRowSelected="rGridType_Edit" EmptyDataText="No Defined Types Found">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id"/>
                <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category" />
                <asp:TemplateField HeaderText="Name" ShowHeader="true" SortExpression="Name"><ItemTemplate>
                    <asp:LinkButton ID="btnEditValue" runat="server" Text='<%#Eval("Name") %>' 
                        CommandName="ID" CommandArgument='<%#Eval("ID") %>' OnCommand="rGridType_EditValue" />  </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <asp:BoundField DataField="FieldType.Name" HeaderText="Field Type" SortExpression="FieldType.Name"/>
                <Rock:EditField OnClick="rGridType_Edit" />
                <Rock:EditValueField OnClick="rGridType_EditAttribute"  />
                <Rock:DeleteField OnClick="rGridType_Delete" />
            </Columns>
        </Rock:Grid>          
    </asp:Panel>

    <asp:Panel ID="pnlTypeDetails" runat="server" Visible="false">
        
        <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
                     
        <fieldset>
            <legend><asp:Literal ID="lType" runat="server" Text=""></asp:Literal></legend>
            <Rock:DataTextBox ID="tbTypeName" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="Name" />
            <Rock:DataTextBox ID="tbTypeCategory" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="Category" />
            <Rock:DataTextBox ID="tbTypeDescription" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
            <Rock:FieldTypeList ID="ddlTypeFieldType" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="FieldType" />
        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSaveType" runat="server" Text="Save" CssClass="btn primary" onclick="btnSaveType_Click" />
            <asp:LinkButton id="btnCancelType" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancelType_Click" />
        </div>

    </asp:Panel>
    
    <asp:Panel ID="pnlAttributes" runat="server" Visible="false">
  
        <Rock:Grid ID="rGridAttribute" runat="server" AllowSorting="true" RowItemText="setting" OnRowSelected="rGridAttribute_Edit" ShowHeader="true" EmptyDataText="No Attributes Found">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id"/>
                <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category" />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name"/>
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description"/>
                <Rock:BoolField DataField="IsGridColumn" HeaderText="Grid Column" SortExpression="IsGridColumn"/>
                <Rock:BoolField DataField="IsRequired" HeaderText="Required" SortExpression="IsRequired"/>
                <Rock:EditField OnClick="rGridAttribute_Edit" />
                <Rock:DeleteField OnClick="rGridAttribute_Delete" />               
            </Columns>
        </Rock:Grid>

        <asp:LinkButton id="btnCloseAttribute" runat="server" Text="Done" CssClass="btn close" CausesValidation="false" OnClick="btnCloseAttribute_Click" />
    </asp:Panel>


    <asp:Panel ID="pnlValues" runat="server" Visible="false">
    
        <label class="control-label">
            <asp:Literal ID="valueCaption" runat="server" Text="Defined Values"></asp:Literal>
        </label>
                
        <Rock:Grid ID="rGridValue" runat="server" ShowHeader="true" RowItemText="setting" OnRowSelected="rGridValue_Edit" AllowSorting="true" EmptyDataText="No Defined Values Found" >
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="ID" SortExpression="Id"/>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name"/>
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                <Rock:EditField Onclick="rGridValue_Edit" />
                <Rock:DeleteField OnClick="rGridValue_Delete" />      
            </Columns>
        </Rock:Grid>
        
        <asp:LinkButton id="btnCloseValue" runat="server" Text="Done" CssClass="btn close" CausesValidation="false" OnClick="btnCloseValue_Click" />
    </asp:Panel>
       

    <Rock:ModalDialog ID="modalAttributes" runat="server" Title="Attribute Values">
    <Content>
        <asp:HiddenField ID="hfIdAttribute" runat="server" />
        <asp:ValidationSummary ID="valSummaryAttribute" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
        <fieldset>
            <Rock:DataTextBox ID="tbAttributeKey" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Key" />
            <Rock:DataTextBox ID="tbAttributeName" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Name" />
            <Rock:DataTextBox ID="tbAttributeCategory" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Category" />
            <Rock:DataTextBox ID="tbAttributeDescription" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
            <Rock:FieldTypeList ID="ddlAttributeFieldType" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="FieldTypeId" LabelText="Field Type" />
            <Rock:DataTextBox ID="tbAttributeDefaultValue" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="DefaultValue" />
            <Rock:LabeledCheckBox ID="cbAttributeGridColumn" runat="server" LabelText="Grid Column" />
            <Rock:LabeledCheckBox ID="cbAttributeRequired" runat="server" LabelText="Required" />
        </fieldset>                
    </Content>
    </Rock:ModalDialog>
    
    <Rock:ModalDialog ID="modalValues" runat="server" Title="Type Values">
    <Content>
        <asp:HiddenField ID="hfIdValue" runat="server" />
        <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
        
        <fieldset>
            <Rock:DataTextBox ID="tbValueName" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Name" />
            <Rock:DataTextBox ID="tbValueDescription" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
            <div class="control-group">
                <label class="control-label">
                    <asp:Literal ID="attrCaption" runat="server"></asp:Literal></label>
                <div class="controls">
                    <asp:PlaceHolder ID="phAttrControl" runat="server"></asp:PlaceHolder>
                </div>
            </div>
        </fieldset>
    </Content>
    </Rock:ModalDialog>

    <asp:HiddenField ID="hfIdType" runat="server" />
    <asp:Button ID="btnRefresh" runat="server" Text="Save" style="display:none" onclick="btnRefresh_Click" />
    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>