<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ViewEditor.ascx.cs" Inherits="RockWeb.Blocks.ViewEditor" %>

<section class="widget widget-dark">
    <header class="clearfix">
        <div class="filter-toogle pull-left">Show if xxx of these are true</div>
        <div class="btn-group pull-right">
            <button class="btn btn-inverse"><i class="icon-list-alt"></i> Add Filter Group</button>
            <button class="btn btn-inverse"><i class="icon-filter"></i> Add Filter</button>
        </div>
    </header>

    <div class="widget-content">
        <article class="widget filter-item">
            <header class="clearfix clickable">
                <div class="pull-left">
                    Gender Is Male
                </div>
                <div class="pull-right">
                    <a class="btn btn-mini"><i class="filter-view-state icon-chevron-down"></i></a>
                    <a class="btn btn-mini btn-danger"><i class="icon-remove"></i></a>
                </div>
            </header>
            <div class="widget-content" style="display: none;">
                Ok just put all your filter fields in here.
                <p>
                    Got it?
                </p>
            </div>
        </article>

        <article class="widget filter-item">
            <header class="clearfix clickable">
                <div class="pull-left">
                    Age is greater than 8
                </div>
                <div class="pull-right">
                    <a class="btn btn-mini"><i class="filter-view-state icon-chevron-down"></i></a>
                    <a class="btn btn-mini btn-danger"><i class="icon-remove"></i></a>
                </div>
            </header>
            <div class="widget-content" style="display: none;">
                Ok just put all your filter fields in here.
                <p>
                    Got it?
                </p>
            </div>
        </article>



        <section class="widget widget-dark">
            <header class="clearfix">
                <div class="filter-toogle pull-left">Show if xxx of these are true</div>
                <div class="btn-group pull-right">
                    <button class="btn btn-inverse"><i class="icon-list-alt"></i> Add Filter Group</button>
                    <button class="btn btn-inverse"><i class="icon-filter"></i> Add Filter</button>
                </div>
            </header>

            <div class="widget-content">
                <article class="widget filter-item">
                    <header class="clearfix clickable">
                        <div class="pull-left">
                            T-shirt size is medium
                        </div>
                        <div class="pull-right">
                            <a class="btn btn-mini"><i class="filter-view-state icon-chevron-down"></i></a>
                            <a class="btn btn-mini btn-danger"><i class="icon-remove"></i></a>
                        </div>
                    </header>
                    <div class="widget-content" style="display: none;">
                        Ok just put all your filter fields in here.
                        <p>
                            Got it?
                        </p>
                    </div>
                </article>

                <article class="widget filter-item">
                    <header class="clearfix clickable">
                        <div class="pull-left">
                            T-shirt size is Large
                        </div>
                        <div class="pull-right">
                            <a class="btn btn-mini"><i class="filter-view-state icon-chevron-down"></i></a>
                            <a class="btn btn-mini btn-danger"><i class="icon-remove"></i></a>
                        </div>
                    </header>
                    <div class="widget-content" style="display: none;">
                        Ok just put all your filter fields in here.
                        <p>
                            Got it?
                        </p>
                    </div>
                </article>
            </div>
        </section>



    </div>
</section>

<script>
    $('.filter-item header').click(function () {
        $(this).siblings('.widget-content').slideToggle();
        
        $('i.filter-view-state', this).toggleClass('icon-chevron-down');
        $('i.filter-view-state', this).toggleClass('icon-chevron-up');

        
    });
</script>