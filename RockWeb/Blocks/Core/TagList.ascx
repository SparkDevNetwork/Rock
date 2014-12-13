<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TagList.ascx.cs" Inherits="RockWeb.Blocks.Core.TagList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-tags"></i> Tag List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" >
                        <Rock:RockDropDownList ID="ddlEntityType" runat="server" Label="Entity Type" />
                        <Rock:RockRadioButtonList ID="rblScope" runat="server" Label="Scope" RepeatDirection="Horizontal" 
                            AutoPostBack="true" OnSelectedIndexChanged="rblScope_SelectedIndexChanged">
                            <asp:ListItem Value="Organization" Text="Organizational" Selected="True" />
                            <asp:ListItem Value="Personal" Text="Personal" />
                        </Rock:RockRadioButtonList>
                        <Rock:PersonPicker ID="ppOwner" runat="server" Label="Owner"  />
                    </Rock:GridFilter>
                    <Rock:Grid ID="rGrid" runat="server" RowItemText="Tag" OnRowSelected="rGrid_Edit" TooltipField="Description">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="EntityTypeName" HeaderText="Entity Type" />
                            <Rock:RockBoundField DataField="EntityTypeQualifierColumn" HeaderText="Qualifier Column" />
                            <Rock:RockBoundField DataField="EntityTypeQualifierValue" HeaderText="Qualifier Value" />
                            <Rock:RockBoundField DataField="Owner" HeaderText="Owner" />

                            <Rock:DeleteField OnClick="rGrid_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
