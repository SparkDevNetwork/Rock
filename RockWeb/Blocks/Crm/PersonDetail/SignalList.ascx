<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SignalList.ascx.cs" Inherits="RockWeb.Blocks.CRM.PersonDetail.SignalList" %>

<asp:UpdatePanel ID="upSignalList" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <asp:Panel ID="pnlList" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-flag"></i> Signals</h1>
            </div>

            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gSignal" runat="server" AllowSorting="true" OnRowSelected="gSignal_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:PersonField DataField="OwnerPersonAlias" HeaderText="Owner" SortExpression="OwnerPersonAlias" />
                            <Rock:RockBoundField DataField="Note" HeaderText="Note" SortExpression="Note" />
                            <Rock:DateField DataField="ExpirationDate" HeaderText="Expiration Date" SortExpression="ExpirationDate" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdEditSignal" runat="server" Title="Edit Signal" OnSaveClick="mdEditSignal_SaveClick" ValidationGroup="EditSignal">
            <Content>
                <asp:HiddenField ID="hfEditSignalId" runat="server" />
                <asp:ValidationSummary ID="vsEditSignal" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="EditSignal" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlSignalType" runat="server" Label="Signal Type" Required="true" ValidationGroup="EditSignal" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppSignalOwner" runat="server" Label="Owner" Help="The person who should be contacted for futher information about this signal." ValidationGroup="EditSignal" Required="true" />
                    </div>

                    <div class="col-md-6">
                        <Rock:DatePicker ID="dpExpirationDate" runat="server" Label="Expiration Date" Help="Once this date has been reached the signal will automatically be removed." ValidationGroup="EditSignal" />
                    </div>
                </div>

                <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" Rows="3" ValidationGroup="EditSignal" />

                <asp:PlaceHolder ID="phAttributes" runat="server" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
