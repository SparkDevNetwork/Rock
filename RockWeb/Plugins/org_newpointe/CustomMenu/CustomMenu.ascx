<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomMenu.ascx.cs" Inherits="Plugins_org_newpointe_CustomMenu_CustomMenu" %>
<%@ Import Namespace="Rock" %>

<style>
    /*  Google Custom Search */
    input.gsc-input, .gsc-input-box, .gsc-input-box-hover, .gsc-input-box-focus {
        height: 30px;
    }

    gsc-results-wrapper-overlay gsc-results-wrapper-visible {
        height: 400px !important;
        background-color: #404040 !important;
    }



    #searchDiv {
        position: absolute;
        top: 17px;
        right: 0;
        border-left: 1px solid #808080;
        padding-left: 10px;
    }

    #NPSearch {
        height: 75px;
        background-color: #404040;
    }

        /*#NPSearch table.gsc-search-box td {
            padding-top: 13px;
            vertical-align: top;
        }*/

        #NPSearch .gsc-search-box {
            height: 70px !important;
        }

    .gsc-search-button {
        display: none;
    }


    #searchDiv a {
        font-size: 24px;
    }

    /**** Results Page ****/

    div.gs-title {
        height: 3.5em !important;
        line-height: 1.7em !important;
        margin-bottom: 5px !important;
    }

    div.gs-visibleUrl {
        font-size: 32px !important;
        height: 2em !important;
        line-height: 1em !important;
        margin-bottom: 0 !important;
        padding-top: 5px !important;
    }

    .gsc-expansionArea .gsc-cursor {
        font-size: 22px !important;
    }

    .gs-bidi-start-align.gs-snippet {
        font-size: 18px !important;
        margin-bottom: 20px !important;
    }

    div.gsc-result {
        box-shadow: 0 0 0 #c0c0c0 !important;
        margin-bottom: 20px !important;
        padding: 20px !important;
        border-bottom: 1px solid #c0c0c0;
    }

    div.gs-snippet {
        font-size: 32px !important;
        height: 2.5em !important;
        line-height: 1.4em !important;
        margin-bottom: 10px !important;
        width: 100%;
        margin-bottom: 20px;
    }

    .gsc-tabHeader {
        font-size: 25px !important;
    }

    div.gs-title * {
        font-size: 32px !important;
    }

    .gs-title a {
        font-size: 24px !important;
    }

    .gs-bidi-start-align.gs-visibleUrl.gs-visibleUrl-long {
        font-size: 18px !important;
    }

    .gs-bidi-start-align.gs-snippet {
        font-size: 18px !important;
    }

    .gsc-webResult.gsc-result {
        overflow: hidden;
    }
</style>
<div id="FloatingMenu" class="desktop-only">
    <div style="height: 70px;margin-bottom: 10px;"></div>
    <nav class="container navbar-fixed-top affix-top" data-spy="affix" data-offset-top="72">
        <div id="NPSearch" style="display: none;" class="col-xs-12">
            <script>
                if (!window.gcseInjected)
                {
                    window.gcseInjected = true;
                    (function ()
                    {
                        var cx = '001784362343258229631:fdnsp4tdlfa';
                        var gcse = document.createElement('script');
                        gcse.type = 'text/javascript';
                        gcse.async = true;
                        gcse.src = (document.location.protocol == 'https:' ? 'https:' : 'http:') +
                            '//cse.google.com/cse.js?cx=' + cx;
                        var s = document.getElementsByTagName('script')[0];
                        s.parentNode.insertBefore(gcse, s);
                    })();
                }
            </script>
            <gcse:searchbox-only resultsurl="https://newpointe.org/SearchResults"></gcse:searchbox-only>
        </div>
        <div id="CustomMenuMenu">
            <div id="CustomMenuLogo" class="col-md-1">
                <div class="brand-logo" style="margin: 5px auto;">
                    <asp:Image runat="server" ImageUrl="~/Assets/Images/NPCC_Logo_Full_White.svg" />
                </div>
            </div>
            <div class="col-md-11 pull-right">
                <ul class="npCustomMenu">
                    <asp:Repeater ID="rptMenuLinks" runat="server">
                        <ItemTemplate>
                            <li><a href="#" id="<%#DataBinder.Eval(Container.DataItem, "ID") %>" onclick="ShowPopup(<%#DataBinder.Eval(Container.DataItem, "ID") %>);"><%# DataBinder.Eval(Container.DataItem, "PageTitle") %></a></li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
                <div id="searchDiv">
                    <a id="searchIcon" href="#"><i class="fa fa-search"></i></a>
                </div>
            </div>
            <asp:Repeater ID="rptMenuDivs" runat="server">
                <ItemTemplate>
                    <div id="popup-<%#DataBinder.Eval(Container.DataItem, "ID") %>" style="display: none;" class="pop-over">
                        <%#(string)DataBinder.Eval(Container.DataItem, "HtmlContent") %>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </nav>
</div>

<script>

    $(window).scroll(function ()
    {
        var scroll = $(window).scrollTop();

        if (scroll >= 72)
        {
            $('#GreenDiv').show();
        }
        else
        {
            $("#GreenDiv").hide();
        }
    });



    $(document).ready(function ()
    {
        $('.aCustomMenuLink').click(function (e)
        {
            e.preventDefault();
        })

        $('.bordered a').hover(function ()
        {
            $('.bordered').css('border-color', '#8BC540');
        }, function ()
        {
            $('.bordered').css('border-color', '#fff');
        });

        //setTimeout(showSearchText, 1000);

        function showSearchText()
        {
            $("#gsc-i-id2").attr("placeholder", "Enter to search");

        }

    });

    $("#searchIcon").click(function (e)
    {
        e.preventDefault();

        $('#NPSearch').height('75px');
        $('input.gsc-input').css("cssText", "height: 38px !important; font-size: 20px !important;");
        $('td.gsib_a').css("cssText", "font-size: 20px !important;");

        $("#NPSearch").toggle();
        $("#NPMenu").toggle();
    });

    //$("#NPSearch").focusout(function () {
    //    $("#NPSearch").fadeOut();
    //    $("#NPMenu").fadeIn();
    //});

    $(document).mouseup(function (e)
    {
        var container = $("#NPSearch");
        if (!container.is(e.target) && container.has(e.target).length === 0)
        {
            container.hide();
            $("#NPMenu").show();
        }
    })

    function ShowPopup(id)
    {
        $(".npCustomMenu li a").removeClass("active");

        if ($("#popup-" + id).is(":visible"))
        {
            $(".pop-over").hide();
        }
        else
        {
            $(".pop-over").hide();
            $("#" + id).addClass('active');
            $("#popup-" + id).show();
        }

    }
    $(".npCustomMenu li a").click(function (e)
    {
        e.preventDefault();
    });





</script>
