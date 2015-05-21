<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonSearch.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonSearch" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotice" runat="server" NotificationBoxType="Info" Visible="false"></Rock:NotificationBox>
        
        <div class="grid">
            <Rock:Grid ID="gPeople" runat="server" EmptyDataText="No People Found" AllowSorting="true" OnRowSelected="gPeople_RowSelected">
                <Columns>
                    <Rock:SelectField />
                    <Rock:RockTemplateField HeaderText="Person" SortExpression="LastName,FirstName">
                        <ItemTemplate>
                            <asp:Literal ID="lPerson" runat="server" />
                        </ItemTemplate>
                    </Rock:RockTemplateField>
                    <Rock:RockBoundField 
                        ItemStyle-HorizontalAlign="Right"
                        HeaderStyle-HorizontalAlign="Right"
                        DataField="Age"  
                        HeaderText="Age"  
                        SortExpression="BirthYear desc,BirthMonth desc,BirthDay desc" ColumnPriority="Desktop" />
                    <Rock:DefinedValueField
                        DataField="ConnectionStatusValue.Id"
                        HeaderText="Connection Status"
                        SortExpression="ConnectionStatusValue.Value" ColumnPriority="Tablet" />
                    <Rock:DefinedValueField
                        DataField="RecordStatusValue.Id"
                        HeaderText="Record Status"
                        SortExpression="RecordStatusValue.Value" ColumnPriority="Desktop" />
                    <Rock:RockTemplateField HeaderText="Campus" ColumnPriority="Tablet" >
                        <ItemTemplate>
                            <asp:Literal ID="lCampus" runat="server" />
                        </ItemTemplate>
                    </Rock:RockTemplateField>
                </Columns>
            </Rock:Grid>
        </div>

        <asp:Literal ID="lPerformance" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>

