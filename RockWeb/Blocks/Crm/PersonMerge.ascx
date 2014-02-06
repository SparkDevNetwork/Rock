<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonMerge.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonMerge" %>

<asp:UpdatePanel ID="upnlContent" runat="server">

    <Triggers>
        <%-- Configure nav buttons to do a postback so that the browser back/forward buttons will work--%>
        <asp:PostBackTrigger ControlID="lbSelectPeople" />
        <asp:PostBackTrigger ControlID="lbSelectValuesBack" />
        <asp:PostBackTrigger ControlID="lbSelectValues" />
        <asp:PostBackTrigger ControlID="lbConfirmBack" />
    </Triggers>
    
    <ContentTemplate>

        <asp:Panel ID="pnlSelectPeople" runat="server" CssClass="person-merge-people">

            <Rock:PersonPicker ID="ppAdd" runat="server" Label="Add Another Person" OnSelectPerson="ppAdd_SelectPerson" />

            <Rock:Grid ID="gPeople" runat="server" EmptyDataText="No People Selected" DisplayType="Light" >
                <Columns>
                    <asp:BoundField DataField="FullName"  HeaderText="Person"  SortExpression="LastName,FirstName"  />
                    <asp:BoundField ItemStyle-HorizontalAlign="Right" DataField="Age"  HeaderText="Age"  SortExpression="BirthYear,BirthMonth,BirthDay"  />
                    <Rock:DefinedValueField DataField="ConnectionStatusValueId" HeaderText="Connection Status" SortExpression="ConnectionStatusValue.Name" />
                    <Rock:DefinedValueField DataField="RecordStatusValueId" HeaderText="Record Status" SortExpression="RecordStatusValue.Name"  />
                    <Rock:DateField DataField="CreatedDateTime" HeaderText="Created" FormatAsElapsedTime="true" />
                    <asp:BoundField DataField="CreatedByPersonAlias.Person.FullName" HeaderText="By" />
                    <Rock:DateField DataField="ModifiedDateTime" HeaderText="Last Modified" FormatAsElapsedTime="true" />
                    <asp:BoundField DataField="ModifiedByPersonAlias.Person.FullName" HeaderText="By" />
                    <Rock:DeleteField OnClick="gPeople_Delete" />
                </Columns>
            </Rock:Grid>

            <Rock:NotificationBox ID="nbPeople" runat="server" NotificationBoxType="Warning" Visible="false"
                Text="You need to select at least two people to merge." />

            <div class="actions pull-right">
                <asp:LinkButton ID="lbSelectPeople" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="lbSelectPeople_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlSelectValues" runat="server" Visible="false" CssClass="person-merge-values">

            <Rock:Grid ID="gValues" runat="server" AllowSorting="false" EmptyDataText="No Results" />
            
            <div class="actions pull-right">
                <asp:LinkButton ID="lbSelectValuesBack" runat="server" Text="Back" CssClass="btn btn-secondary" OnClick="lbSelectValuesBack_Click" />
                <asp:LinkButton ID="lbSelectValues" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="lbSelectValues_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlConfirm" runat="server" Visible="false" CssClass="person-merge-confirm" >

            <Rock:Grid ID="gConfirm" runat="server" DisplayType="Light" >
                <Columns>
                    <asp:BoundField DataField="Property"  HeaderText="Property"  />
                    <asp:BoundField DataField="CurrentValue" HeaderText="Current Value" HtmlEncode="false"  />
                    <asp:TemplateField HeaderText="New Value">
                        <ItemTemplate>
                            <p class='<%# (bool)Eval("ValueUpdated") ? "updated" : "" %>'><%# Eval("NewValue") %></p>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </Rock:Grid>

            <div class="actions pull-right">
                <asp:LinkButton ID="lbConfirmBack" runat="server" Text="Back" CssClass="btn btn-secondary" OnClick="lbConfirmBack_Click" />
                <asp:LinkButton ID="lbConfirm" runat="server" Text="Finish" CssClass="btn btn-primary" OnClick="lbConfirm_Click" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
