<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Confirm.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Confirm" %>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row-fluid checkin-header">
        <div class="span3">
            <asp:LinkButton ID="lbBack" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" runat="server" OnClick="lbBack_Click" Text="Back"/>
        </div>

        <div class="span6">
            <legend>Confirm</legend>
        </div>

        <div class="span3">
            <asp:LinkButton ID="lbDone" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" runat="server" OnClick="lbDone_Click" Text="Done"/>
        </div>
    </div>

    <div class="row-fluid checkin-body">

        <div class="span9">
            <div class="checkin-body-container">
                <Rock:Grid ID="gPersonList" runat="server" AllowSorting="true" OnRowSelected="gPerson_Edit">
                    <Columns>
                        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Text" />
                        <asp:BoundField DataField="AssignedTo" HeaderText="Assigned To" SortExpression="AssignedTo" />
                        <asp:BoundField DataField="Room" HeaderText="Room" SortExpression="Room" />
                        <Rock:EditValueField OnClick="gPerson_Edit" />
                        <Rock:DeleteField OnClick="gPerson_Delete" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:Button ID="btnPrint" runat="server" CommandName="Print" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>" CssClass="icon-print" />
                            </ItemTemplate> 
                        </asp:TemplateField>
                    </Columns>
                </Rock:Grid>
            </div>
        </div>

        <div class="span3">
            <div class="checkin-body-container">
                <asp:LinkButton ID="lbPrintAll" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" runat="server" OnClick="lbPrintAll_Click" Text="Print All"/>
            </div>
        </div>

    </div>

</ContentTemplate>
</asp:UpdatePanel>