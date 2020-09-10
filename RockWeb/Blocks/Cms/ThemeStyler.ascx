<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ThemeStyler.ascx.cs" Inherits="RockWeb.Blocks.Cms.ThemeStyler" %>

<style>
    .imageupload-group {
        float: left;
    }

    .image-uploader.clearable-input i.fa-times {
        margin-top: 113px;
    }

    .imageupload-remove {
        display: none;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-paint-brush"></i> <asp:Literal ID="lThemeName" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessages" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <asp:PlaceHolder ID="phThemeControls" runat="server" />
                    </div>

                    <div class="col-md-6">
                        <div class="clearfix">
                            <div class="pull-right">
                                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click">Save</asp:LinkButton>
                                <asp:LinkButton ID="btnBack" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="btnCancel_Click"  />
                            </div>
                        </div>

                        <asp:Panel ID="pnlFontAwesomeSettings" runat="server">
                            <Rock:RockDropDownList ID="ddlFontAwesomeIconWeight" runat="server" Label="Font Awesome Icon Weight" AutoPostBack="true" OnSelectedIndexChanged="ddlFontAwesomeIconWeight_SelectedIndexChanged" />
                            <Rock:RockCheckBoxList ID="cblFontAwesomeAlternateFonts" runat="server" Label="Font Awesome Alternate Fonts" Help="It allows you to also load those fonts too. So if you want to use 'fas fa-cog' and 'far fa-cog' and 'fal fa-cog' all on the same theme you could." />
                        </asp:Panel>

                        <Rock:CodeEditor ID="ceOverrides" runat="server" Label="CSS Overrides" EditorHeight="600"  />
                    </div>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
    // change approval status to pending if any values are changed
    Sys.Application.add_load( function () {
        $('.js-color-override').each(function() {
            return $(this).siblings('.form-group').children('.control-wrapper').append($(this))
        });

        $('.js-image-override').each(function () {
            return $(this).siblings('.form-group').children('.control-wrapper').append($(this))
        });

        $(".js-color-override").on("click", function () {
            var controlKey = $(this).attr("data-control");
            var originalValue = $(this).attr("data-original-value");

            $("input[id$='" + controlKey + "']").parent().colorpicker('setValue', originalValue);

            $(this).hide();
        });

        $(".js-image-override").on("click", function () {
            var controlKey = $(this).attr("data-control");
            var originalValue = $(this).attr("data-original-value");

            var controlImageId = controlKey + '-thumbnail';
            var controlValueFieldId = controlKey + '_hfContentFileSource';

            console.log(controlImageId);
            console.log(controlValueFieldId);

            $("input[id$='" + controlValueFieldId + "']").val(originalValue);
            $("div[id$='" + controlImageId + "']").css("background-image", "url('" + originalValue.replace( "~", "") + "')");

            $(this).hide();
        });

        $(".js-text-override").on("click", function () {
            var controlKey = $(this).attr("data-control");
            var originalValue = $(this).attr("data-original-value");

            $("input[id$='" + controlKey + "']").val(originalValue);

            $(this).hide();
        });
    });
</script>
