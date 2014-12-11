<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonSearch.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonSearch" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotice" runat="server" NotificationBoxType="Info" Visible="false"></Rock:NotificationBox>
        
        <div class="grid">
            <Rock:Grid ID="gPeople" runat="server" EmptyDataText="No People Found" AllowSorting="true" OnRowSelected="gPeople_RowSelected">
                <Columns>
                    <Rock:SelectField />
                    <Rock:RockBoundField 
                        DataField="FullNameReversed"  
                        HeaderText="Person"  
                        SortExpression="LastName,FirstName" />
                    <Rock:RockBoundField 
                        ItemStyle-HorizontalAlign="Right"
                        HeaderStyle-HorizontalAlign="Right"
                        DataField="Age"  
                        HeaderText="Age"  
                        SortExpression="BirthYear desc,BirthMonth desc,BirthDay desc" />
                    <Rock:DefinedValueField
                        DataField="ConnectionStatusValueId"
                        HeaderText="Connection Status"
                        SortExpression="ConnectionStatusValue.Value" />
                    <Rock:DefinedValueField
                        DataField="RecordStatusValueId"
                        HeaderText="Record Status"
                        SortExpression="RecordStatusValue.Value" />
                </Columns>
            </Rock:Grid>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

