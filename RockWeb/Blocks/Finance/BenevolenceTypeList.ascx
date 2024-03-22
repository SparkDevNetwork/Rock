<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BenevolenceTypeList.ascx.cs" Inherits="RockWeb.Blocks.Finance.BenevolenceTypeList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plug"></i>Benevolence Types</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gBenevolenceType" runat="server" RowItemText="Benevolence Type" OnRowSelected="gBenevolenceType_Edit" TooltipField="Id" CssClass="js-grid-benevolence-type-list">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" Visible="false" />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name"  />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:BoolField DataField="ShowFinancialResults" HeaderText="Show Financial Results" SortExpression="ShowFinancialResults" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField ID="dfBenevolenceType" OnClick="gBenevolenceType_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div> <!-- panel-body -->
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
