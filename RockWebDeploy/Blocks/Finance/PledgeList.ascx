<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Finance.PledgeList, RockWeb" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server"/>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list"></i> Pledge List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:PersonPicker ID="ppFilterPerson" runat="server" Label="Filter by person"/>
                        <Rock:AccountPicker ID="fpFilterAccount" runat="server" Label="Filter by account" AllowMultiSelect="True"/>
                    </Rock:GridFilter>
                    <Rock:Grid ID="gPledges" runat="server" AutoGenerateColumns="False" AllowSorting="True" AllowPaging="True" OnRowSelected="gPledges_Edit">
                        <Columns>
                            <asp:BoundField DataField="PersonAlias.Person" HeaderText="Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName"/>
                            <asp:BoundField DataField="Account" HeaderText="Account" SortExpression="AccountId"/>
                            <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" SortExpression="TotalAmount" DataFormatString="{0:C}"/>
                            <asp:BoundField DataField="PledgeFrequencyValue" HeaderText="Payment Schedule" SortExpression="PledgeFrequencyValue" />
                            <Rock:DateField DataField="StartDate" HeaderText="Starts" SortExpression="StartDate"/>
                            <Rock:DeleteField OnClick="gPledges_Delete"/>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>