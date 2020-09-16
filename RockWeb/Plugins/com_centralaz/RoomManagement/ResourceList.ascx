<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResourceList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.RoomManagement.ResourceList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building-o"></i> Resource List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Campus"/>
                        <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="false" EntityTypeName="com.centralaz.RoomManagement.Model.Resource" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gResources" runat="server" RowItemText="Resource" OnRowSelected="gResources_Edit" TooltipField="Description">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" />
                            <Rock:RockBoundField DataField="Category" HeaderText="Category" />
                            <Rock:RockTemplateField HeaderText="Name" >
                                <ItemTemplate><%# Convert.ToString( Eval( "Location.Name" ) ) == string.Empty ? Convert.ToString( Eval( "Name" ) ) : Convert.ToString( Eval( "Name" ) ) + " <em class='text-muted'>(attached to " + Eval("Location.Name") + ")" %></em></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                            <Rock:RockBoundField DataField="Quantity" HeaderText="Quantity" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
