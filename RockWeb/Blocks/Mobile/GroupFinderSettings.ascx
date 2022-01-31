<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupFinderSettings.ascx.cs" Inherits="RockWeb.Blocks.Mobile.GroupFinderSettings" %>

<Rock:RockListBox
    ID="rlbGroupTypes"
    runat="server"
    Label="Group Types"
    Help="Specifies which group types are included in search results."
    Required="true"
    DisplayDropAsAbsolute="true"
    AutoPostBack="true"
    OnSelectedIndexChanged="GroupTypes_SelectedIndexChanged" />

<Rock:RockControlWrapper
    ID="rcwGroupTypesLocationType"
    runat="server"
    Label="Group Types Location Type"
    Help="The type of location each group type can use for distance calculations.">
    <Rock:Grid
        ID="gGroupTypesLocationType"
        runat="server"
        DisplayType="Light"
        ShowHeader="false"
        OnRowDataBound="GroupTypesLocationType_RowDataBound">
        <Columns>
            <Rock:RockBoundField DataField="Name" />
            <asp:TemplateField>
                <ItemTemplate>
                    <Rock:RockDropDownList
                        ID="ddlLocationList"
                        runat="server"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="LocationList_SelectedIndexChanged" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </Rock:Grid>
</Rock:RockControlWrapper>

<Rock:RockCheckBoxList
    ID="cblAttributeFilters"
    runat="server"
    Label="Attribute Filters"
    Help="Attributes to make available for the user to filter groups by."
    RepeatDirection="Horizontal" />
