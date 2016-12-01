<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupRegionUtility.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Groups.GroupRegionUtility" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>


        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Group Region Utility</h1>

                <div class="panel-labels">
                </div>
            </div>

            <div class="panel-body">

                <div class="row">
                    <div class="col-md-6">
                        <Rock:GroupPicker ID="gpCurrentParentArea" runat="server" Label="Current Parent Area" />

                        <h1>Organize Groups</h1>
                        <Rock:GroupPicker ID="gpNewParentArea" runat="server" Label="New Parent Area" />
                        <pre>
Select 'Neighborhood Area'
                        </pre>
                        <Rock:GroupTypePicker ID="gtpGeofencingGroupType" runat="server" Label="Geofence Group Type" />
                        <pre>
a) Parent: Neighborhood Group Sub Section
b) Group Type: Neighborhood Group
c) Run/Apply        
                        </pre>
                        <Rock:GroupTypePicker ID="gtpParentGroupType" runat="server" Label="Parent Group Type" />
                        <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Label="Group Type" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnRun" runat="server" Text="Run" CssClass="btn btn-primary" OnClick="btnRun_Click" />
                </div>

                <Rock:Grid ID="grdPreview" runat="server" DataKeyNames="Id" AllowPaging="false">
                    <Columns>
                        <asp:BoundField DataField="GroupId" HeaderText="GroupId" />
                        <asp:BoundField DataField="GroupName" HeaderText="GroupName" />
                        <asp:BoundField DataField="CurrentParentRegion.Id" HeaderText="CurrentParentRegionId" Visible="false" />
                        <asp:BoundField DataField="CurrentParentRegion.Name" HeaderText="CurrentParentRegionName" />
                        <asp:BoundField DataField="CurrentParentGroup.Id" HeaderText="CurrentParentGroupId" Visible="false"/>
                        <asp:BoundField DataField="CurrentParentGroup.Name" HeaderText="CurrentParentGroupName" />
                        <asp:BoundField DataField="NewParentRegion.Id" HeaderText="NewParentRegionId" Visible="false"/>
                        <asp:BoundField DataField="NewParentRegion.Name" HeaderText="NewParentRegionName" />
                        <asp:BoundField DataField="NewParentGroup.Id" HeaderText="NewParentGroupId" Visible="false"/>
                        <asp:BoundField DataField="NewParentGroup.Name" HeaderText="NewParentGroupName" />
                        <Rock:BoolField DataField="RegionChanged" HeaderText="Region Changed" />
                    </Columns>
                </Rock:Grid>

                <div class="actions">
                    <asp:LinkButton ID="btnApply" runat="server" Text="Apply" CssClass="btn btn-primary" OnClick="btnApply_Click" />
                </div>

                <h1>Delete Groups</h1>
                <pre>
HINT:
    1) Next Step Group Section
    2) Neighborhood Group Section
    3) Neighborhood Area
                </pre>
                <Rock:GroupTypePicker ID="gtpParentForDelete" runat="server" Label="Parent Group Type" />
                <div class="actions">

                    <asp:LinkButton ID="btnDeleteIfNoChildGroups" runat="server" Text="Delete If No Child Groups" CssClass="btn btn-primary" OnClick="btnDeleteIfNoChildGroups_Click" />
                </div>

            </div>

        </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
