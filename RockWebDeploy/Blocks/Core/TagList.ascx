<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Core.TagList, RockWeb" %>

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
                            <asp:BoundField DataField="Id" HeaderText="Id" Visible="false" />
                            <asp:BoundField DataField="Name" HeaderText="Name" />
                            <asp:BoundField DataField="EntityTypeName" HeaderText="Entity Type" />
                            <asp:BoundField DataField="EntityTypeQualifierColumn" HeaderText="Qualifier Column" />
                            <asp:BoundField DataField="EntityTypeQualifierValue" HeaderText="Qualifier Value" />
                            <asp:BoundField DataField="Owner" HeaderText="Owner" />

                            <Rock:DeleteField OnClick="rGrid_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
