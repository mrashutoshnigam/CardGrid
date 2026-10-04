///<reference path="./types/jquery.d.ts"/>
///<reference path="./types/bootpag.d.ts"/>
/*
    Author  : Ashutosh Nigam
    Website : http://www.ashutoshnigam.com
    Email   : ashutosh.nigam@live.com

    Bootstrap 5 port for the ASP.NET Core host (CardGrid.Core). Requires jQuery 3.x,
    Bootstrap 5 (bundle, for dropdowns), jquery.bootpag and Font Awesome 4.
*/
var SortDirection;
(function (SortDirection) {
    SortDirection[SortDirection["asc"] = 0] = "asc";
    SortDirection[SortDirection["desc"] = 1] = "desc";
})(SortDirection || (SortDirection = {}));
var CardGrid = /** @class */ (function () {
    function CardGrid(_cardGrid, options) {
        this._allowToSwitchView = false;
        if (!_cardGrid.hasClass('card-grid')) {
            alert('Please Specify Card-grid class');
            return;
        }
        this.options = $.extend({
            url: '#',
            data: null,
            type: 'GET',
            pagination: true,
            searchBox: true,
            success: null,
            recordsPerPage: [12],
            ItemsPerRow: 4,
            template: null,
            cols: null,
            currentView: 'card',
            sortCols: [{ text: 'Id', value: 'Id' }],
            multiSelect: true,
            queryParams: null,
            beforeSend: null,
            beforeGridLoad: null
        }, options);
        if (window.localStorage.getItem('cardgrid-view')) {
            this.options.currentView = window.localStorage.getItem('cardgrid-view');
        }
        else {
            window.localStorage.setItem('cardgrid-view', this.options.currentView);
        }
        if (this.options.template && this.options.cols)
            this._allowToSwitchView = true;
        else if (this.options.template == null && this.options.cols != null) {
            this.options.currentView = 'table';
            this._allowToSwitchView = false;
        }
        else if (this.options.template != null && this.options.cols == null) {
            this.options.currentView = 'card';
            this._allowToSwitchView = false;
        }
        else {
            alert('Please Define Template for Grid');
            return;
        }
        var Id = '';
        if (_cardGrid.attr('id')) {
            Id = _cardGrid.attr('id');
        }
        else {
            Id = 'cardgrid' + Math.floor((Math.random() * 100) + 1000).toString();
            _cardGrid.attr('id', Id);
        }
        this.cardGridId = Id;
        this.cardRowId = Id + '-card-row';
        this.searchBoxId = Id + '-search-textbox';
        this.sortbyId = Id + '-sort-by';
        this.paginationControlId = Id + '-page-selection';
        this.goToPageControlId = Id + '-go-to-page';
        this.viewControlId = Id + '-view';
        this.switchViewControlId = Id + '-switchView';
        this.pageSizeSelect = Id + '-pageSizeSelect';
        this.selectedDataIds = new Array();
        this.buildGrid();
        this.load();
    }
    CardGrid.prototype.getOptions = function () {
        return this.options;
    };
    CardGrid.prototype.getData = function () {
        return this.options.data;
    };
    CardGrid.prototype.load = function () {
        var option = this.options;
        var $this = this;
        if (option.beforeSend != null)
            option.beforeSend();
        $.ajax({
            type: option.type,
            url: this.options.url + (this.options.queryParams != null ? '?' + $.param(this.options.queryParams) : ''),
            method: this.options.type,
            data: this.getRequestParams(),
            dataType: "json",
            success: function (responseText, statusText, xhr) {
                if (option.beforeGridLoad != null) {
                    responseText = option.beforeGridLoad(responseText);
                }
                option.data = responseText;
                $this.processData(responseText);
                if (option.success && option.success != undefined && typeof (option.success) === 'function') {
                    option.success();
                }
                if (option.afterGridRender && option.afterGridRender != undefined && typeof (option.afterGridRender) === 'function') {
                    option.afterGridRender();
                }
            },
            error: function (responseText, statusText, xhr) {
                if (responseText.status === 500 && statusText == 'error') {
                    $.error(responseText.responseText);
                }
            }
        });
        // Request Parameters for Sending Request
    };
    CardGrid.prototype.reload = function () {
        this.load();
    };
    // Getting Request Parameters
    CardGrid.prototype.getRequestParams = function () {
        var rows = window.localStorage.getItem('cardgrid-stored-per-page') != null ? parseInt(window.localStorage.getItem('cardgrid-stored-per-page')) : this.options.recordsPerPage[0];
        if (!this.Pagination) {
            this.Pagination = {
                total: 0,
                noOfPages: 1,
                rows: rows,
                page: 1,
                searchParam: '',
                sord: 'asc',
                sidx: this.options.sortCols[0].value
            };
        }
        if (this.options.searchBox) {
            this.Pagination.searchParam = document.getElementById(this.searchBoxId).value;
        }
        return this.Pagination;
    };
    // Process Data
    CardGrid.prototype.processData = function (responseData) {
        var $this = this;
        if (responseData == null || responseData == undefined) {
            alert('No Data');
        }
        else {
            this.Pagination.noOfPages = parseInt((Math.ceil((parseFloat(responseData.total.toString()) / parseFloat(this.Pagination.rows.toString())))).toString());
            this.Pagination.total = responseData.total;
            var ctrl = document.getElementById($this.goToPageControlId);
            if (ctrl)
                ctrl.value = this.Pagination.page.toString();
            // this.Pagination.rows = responseData.rows;
            //Create Pagination
            if (typeof $.fn.bootpag === 'function') {
                var _pager = $('#' + this.paginationControlId);
                _pager.unbind("page").show().bootpag({
                    total: this.Pagination.noOfPages,
                    wrapClass: 'pagination mb-0',
                    page: this.Pagination.page,
                    maxVisible: 5,
                    leaps: true,
                    next: 'Next',
                    prev: 'Prev',
                    first: 'First',
                    last: 'Last',
                    firstLastUse: true
                }).on("page", function (event, num) {
                    $this.Pagination.page = num;
                    var goToPage = document.getElementById($this.goToPageControlId);
                    goToPage.value = num.toString();
                    $this.load();
                });
                // bootpag emits Bootstrap 3 pagination markup; add the Bootstrap 5 item/link classes.
                _pager.find('li').addClass('page-item').children('a').addClass('page-link');
            }
            else {
                alert('Please download Bootpag from http://botmonster.com/jquery-bootpag/#.V--5BSh96hc');
                window.open('http://botmonster.com/jquery-bootpag/#.V--5BSh96hc', '_blank');
                return;
            }
            // View Control Building Block
            {
                var currentPage = $this.Pagination.page;
                var prevPage = currentPage - 1;
                var pageSize = $this.Pagination.rows;
                var from = currentPage == 1 ? 1 : prevPage * pageSize + 1;
                var to = currentPage == $this.Pagination.noOfPages ? $this.Pagination.total : currentPage * pageSize;
                var viewtext = "View " + from.toString() + " - " + to.toString() + " of " + $this.Pagination.total.toString();
                var lbl = $('<div/>', { text: viewtext, style: 'height: 34px;padding: 6px 12px;' });
                $('#' + this.viewControlId).empty().append(lbl);
            }
            //Card Grid Building Block
            // building Cards
            this.buildCardTable($this, responseData.Data);
        }
    };
    CardGrid.prototype.buildCardTable = function ($this, data) {
        var _cardRow = document.getElementById(this.cardRowId);
        _cardRow.innerHTML = '';
        if ($this.options.currentView == 'card') {
            $.each(data, function (index, _singleData) {
                var _template = $this.options.template(_singleData);
                var _divContainer = $('<div/>', { 'data-id': _singleData.Id, id: $this.cardGridId + '-data-' + _singleData.Id });
                switch ($this.options.itemsPerRow) {
                    case 2:
                        _divContainer.addClass('col-12 col-sm-6 col-lg-6');
                        break;
                    case 3:
                        _divContainer.addClass('col-12 col-sm-4 col-lg-4');
                        break;
                    case 4:
                        _divContainer.addClass('col-12 col-sm-4 col-lg-3');
                        break;
                    case 6:
                        _divContainer.addClass('col-12 col-sm-3 col-lg-2');
                        break;
                    case 12:
                        _divContainer.addClass('col-12 col-sm-3 col-lg-1');
                        break;
                }
                var _thumbnailDiv = $('<div/>', { class: 'card thumbnail mb-4' });
                // Selected if Previously Selected
                if ($.inArray(_singleData.Id.toString(), $this.selectedDataIds) !== -1) {
                    _thumbnailDiv.attr('selected', 'selected');
                }
                //Selection Block
                _divContainer.unbind('click');
                _divContainer.on('click', function () {
                    var dataId = $(this).attr('data-id');
                    if ($(_thumbnailDiv).attr('selected') == 'selected') {
                        $(_thumbnailDiv).removeAttr('selected');
                        // $(_thumbnailDiv).removeClass('animated fadeOut');
                        var index_1 = $this.selectedDataIds.indexOf(dataId);
                        if (index_1 > -1)
                            $this.selectedDataIds.splice(index_1, 1);
                    }
                    else {
                        if (!$this.options.multiSelect) {
                            $(_cardRow).find('.thumbnail').removeAttr('selected');
                            $this.selectedDataIds.splice(0, $this.selectedDataIds.length);
                        }
                        $(_thumbnailDiv).attr('selected', 'selected');
                        // $(_thumbnailDiv).addClass('animated fadeIn');
                        $this.selectedDataIds.push(dataId);
                    }
                });
                _thumbnailDiv.append(_template).appendTo(_divContainer);
                $(_cardRow).append(_divContainer);
            });
        }
        else {
            var _table_1 = $('<table/>', { class: 'table table-bordered table-hover', 'role': 'grid' });
            var _thead = $('<thead/>');
            var _thead_tr_1 = $('<tr/>', { role: 'row' });
            var _width_total_1 = 0;
            var _no_of_items_1 = 0;
            $.each($this.options.cols, function (i, o) {
                if (o.width != undefined) {
                    _width_total_1 += o.width;
                    _no_of_items_1 += 1;
                }
            });
            var _width_1 = (100 - _width_total_1) / ($this.options.cols.length - _no_of_items_1);
            $.each($this.options.cols, function (idx, col) {
                col = $.extend({
                    sortable: true,
                    hidden: false,
                    template: null,
                    width: _width_1
                }, col);
                var _th = $('<th/>', { text: col.title, style: 'width:' + col.width + '%', 'colspan': 1, 'rowspan': 1 });
                if (col.hidden)
                    _th.attr('hidden', 'hidden');
                else
                    _th.removeAttr('hidden');
                if (col.sortable)
                    _th.addClass('sorting');
                else
                    _th.removeClass('sorting');
                _thead_tr_1.append(_th);
            });
            _thead.append(_thead_tr_1);
            _table_1.append(_thead);
            _table_1.append(_thead);
            $.each(data, function (idx, obj) {
                var _tr = $('<tr/>', { 'data-id': obj.Id });
                $.each($this.options.cols, function (i, col) {
                    var _td = $('<td/>');
                    // Raw values are inserted as text so data can never inject markup; templates own their output.
                    if (col.template != null)
                        _td.append(col.template(obj));
                    else
                        _td.text(obj[col.id] == null ? '' : obj[col.id]);
                    if (col.hidden)
                        _td.attr('hidden', 'hidden');
                    else
                        _td.removeAttr('hidden');
                    _tr.append(_td);
                });
                _tr.unbind('click');
                if ($.inArray(obj.Id.toString(), $this.selectedDataIds) !== -1) {
                    _tr.attr('selected', 'selected');
                }
                _tr.bind('click', function (event) {
                    var dataId = _tr.attr('data-id');
                    if (_tr.attr('selected') == 'selected') {
                        _tr.removeAttr('selected');
                        var index = $this.selectedDataIds.indexOf(dataId);
                        if (index > -1)
                            $this.selectedDataIds.splice(index, 1);
                    }
                    else {
                        if (!$this.options.multiSelect) {
                            _table_1.find('tr').removeAttr('selected');
                            $this.selectedDataIds.splice(0, $this.selectedDataIds.length);
                        }
                        _tr.attr('selected', 'selected');
                        $this.selectedDataIds.push(dataId);
                    }
                });
                _table_1.append(_tr);
            });
            var _div = $('<div/>', { class: 'col-12' });
            _div.append(_table_1);
            $(_cardRow).append(_div);
        }
    };
    CardGrid.prototype.buildGrid = function () {
        var _cardgrid = document.getElementById(this.cardGridId);
        $(_cardgrid).append(this.createHeader())
            .append('<div class="clearfix" style="margin-top:20px"></div>')
            .append($('<div/>', { class: 'card-row row', id: this.cardRowId }))
            .append('<div class="clearfix"></div>')
            .append(this.createFooter());
    };
    // Header
    CardGrid.prototype.createHeader = function () {
        var $this = this;
        var _divHeader = $('<div/>', { class: 'header' });
        var _divRow = $('<div/>', { class: 'row' });
        var _divLeft = $('<div/>', { class: 'col-12 col-md-4 col-lg-3' });
        var _divMiddle = $('<div/>', { class: 'col-12 col-md-4 col-lg-5', style: 'text-align:center' });
        var _divRight = $('<div/>', { class: 'col-12 col-md-4 col-lg-4' });
        // Search Box on Left
        if (this.options.searchBox) {
            var _divSbox = $('<div/>', { class: 'input-group' });
            var _span = $('<span/>', { class: 'input-group-text' }).append('<i class="fa fa-search" aria-hidden="true"></i>');
            var _tbox = $('<input/>', { type: 'search', class: 'form-control', id: this.searchBoxId, placeholder: 'Search', 'aria-label': 'Search' });
            _divSbox.append(_span).append(_tbox);
            _divSbox.appendTo(_divLeft);
            $(_tbox).on("keypress", function (e) {
                if (e.keyCode == 13) {
                    $this.load();
                    return false; // prevent the button click from happening
                }
            });
        }
        // Sort in Middle
        if ($this.options.sortCols && $this.options.sortCols.length > 0) {
            var _select_1 = $('<select/>', { class: 'form-select d-inline-block', style: 'width:70%;', id: this.sortbyId });
            $.each($this.options.sortCols, function (i, o) {
                _select_1.append($('<option/>', { value: o.value + '-asc', text: o.text + ' - Asc', 'data-val': o.value, 'sort-direction': 'asc' }));
                _select_1.append($('<option/>', { value: o.value + '-desc', text: o.text + ' - Desc', 'data-val': o.value, 'sort-direction': 'desc' }));
            });
            _select_1.change(function () {
                $this.Pagination.sidx = $('option:selected', this).attr('data-val');
                $this.Pagination.sord = $('option:selected', this).attr('sort-direction');
                ;
                $this.reload();
            });
            var _divbox1 = $('<div/>', { style: 'float:left;width:60%;text-align:left;' });
            _divbox1.append('Sort by : ').append(_select_1);
            var _divbox2 = $('<div/>', { style: 'float:right;width:40%' });
            // View Changer
            var _button1_1 = $('<button/>', { type: 'button', class: 'btn', 'data-type': 'table', title: 'Table view' }).append('<i class="fa fa-table" aria-hidden="true"></i>');
            var _button2_1 = $('<button/>', { type: 'button', class: 'btn', 'data-type': 'card', title: 'Card view' }).append('<i class="fa fa-credit-card" aria-hidden="true"></i>');
            var _btnGroup = $('<div/>', { class: 'btn-group btn-toggle', role: 'group' });
            var setActiveView_1 = function (view) {
                _button1_1.toggleClass('btn-primary active', view === 'table').toggleClass('btn-outline-secondary', view !== 'table');
                _button2_1.toggleClass('btn-primary active', view === 'card').toggleClass('btn-outline-secondary', view !== 'card');
            };
            setActiveView_1(this.options.currentView);
            _btnGroup.append(_button2_1).append(_button1_1);
            if (this._allowToSwitchView) {
                _divbox2.append('View : ');
                _divbox2.append(_btnGroup);
            }
            _divMiddle.append(_divbox1).append(_divbox2);
            _btnGroup.on('click', '.btn', function () {
                var view = $(this).attr('data-type');
                if (view === $this.options.currentView)
                    return;
                $this.options.currentView = view;
                window.localStorage.setItem('cardgrid-view', view);
                setActiveView_1(view);
                $this.buildCardTable($this, $this.getData().Data);
            });
        }
        // Buttton on Right
        if (this.options.buttons && this.options.buttons.length > 0) {
            var _buttonsDiv = this.createButtons(this.options.buttons);
            _divRight.append(_buttonsDiv);
        }
        _divRow.append(_divLeft).append(_divMiddle).append(_divRight);
        _divHeader.append(_divRow);
        return _divHeader[0];
    };
    // Footer
    CardGrid.prototype.createFooter = function () {
        var $this = this;
        var _footrow = $('<div/>', { class: 'footrow row', style: 'padding-top: 20px;' });
        var _left_cols = $('<div/>', { class: 'col-12 col-lg-5 row' });
        var _middle_cols = $('<div/>', { class: 'col-12 col-lg-2' });
        var _right_cols = $('<div/>', { class: 'col-12 col-md-6 col-lg-5' });
        // No of Records selector
        if ($this.options.recordsPerPage.length > 1) {
            var _pagesize_selector_1 = $('<select/>', { id: $this.pageSizeSelect, style: 'width: 80px;', class: 'form-select d-inline-block ms-1' });
            var storedPerpage_1 = window.localStorage.getItem('cardgrid-stored-per-page') ? window.localStorage.getItem('cardgrid-stored-per-page') : $this.options.recordsPerPage[0];
            $.each($this.options.recordsPerPage, function (i, o) {
                var _opt = $('<option/>', { value: o, text: o });
                if (o == storedPerpage_1)
                    _opt.attr("selected", "selected");
                _pagesize_selector_1.append(_opt);
            });
            _pagesize_selector_1.change(function () {
                $this.Pagination.rows = $(this).val();
                $this.Pagination.page = 1;
                window.localStorage.setItem('cardgrid-stored-per-page', $(this).val());
                $this.reload();
            });
            _left_cols.append($('<div/>', { class: 'col-6' }).append('Row Count :').append(_pagesize_selector_1));
        }
        // Go TO page
        var _page_input = $('<input/>', { id: this.goToPageControlId, type: 'number', min: 1, style: 'width: 80px;', class: 'form-control d-inline-block ms-1' });
        $(_page_input).on("keypress", function (e) {
            if (e.keyCode == 13) {
                var requested = parseInt(this.value);
                if (isNaN(requested) || requested < 1 || requested > $this.Pagination.noOfPages) {
                    alert('Out of Range! ' + $this.Pagination.noOfPages + ' Pages Available');
                }
                else {
                    $this.Pagination.page = requested;
                    $this.reload();
                }
                return false; // prevent the button click from happening
            }
        });
        // From 1-12 records Like that
        _middle_cols.append($('<div/>', { class: 'col-center', style: 'text-align:center', id: this.viewControlId }));
        _left_cols.append($('<div/>', { class: 'col-6' }).append('Go To Page :').append(_page_input));
        // Pagination
        var _pagination_div = $('<div/>', { id: this.paginationControlId });
        _right_cols.append($('<div/>', { class: 'card-paginaton' }).append(_pagination_div));
        $(_footrow).append(_left_cols).append(_middle_cols).append(_right_cols);
        return $(_footrow)[0];
    };
    // Generate Buttons
    CardGrid.prototype.createButtons = function (buttons) {
        var $this = this;
        var options = $this.options;
        var _divToolbox = $('<div/>', { class: 'btn-group float-end panel_toolbox', role: 'group' });
        $.each(buttons, function (index, element) {
            element = $.extend({
                id: '-btn-' + (index + 1),
                enable: true,
                hidden: false,
                buttons: null,
                onClick: null
            }, element);
            if (element.buttons === null) {
                var _btn = $('<button/>', { type: 'button', text: element.text, class: CardGrid.buttonClass(element.className), id: $this.cardGridId + element.id });
                if (element.enable)
                    _btn.removeClass('disabled');
                else
                    _btn.addClass('disabled');
                if (element.onClick) {
                    $(_btn).click(function () {
                        element.onClick(options.data);
                    });
                }
                if (element.hidden)
                    _btn.hide();
                else
                    _btn.show();
                _divToolbox.append(_btn);
            }
            else {
                if (element.buttons && element.buttons.length > 0) {
                    var _divbtnGroup = $('<div/>', { class: 'btn-group', role: 'group' });
                    var _btn = $('<button/>', { type: 'button', text: element.text, class: CardGrid.buttonClass(element.className), id: $this.cardGridId + element.id });
                    if (element.onClick) {
                        $(_btn).click(function () {
                            element.onClick(options.data);
                        });
                    }
                    if (element.hidden)
                        _btn.hide();
                    else
                        _btn.show();
                    _divbtnGroup.append(_btn);
                    var _btnGroup = $('<button/>', { 'data-bs-toggle': 'dropdown', class: CardGrid.buttonClass(element.className) + ' dropdown-toggle dropdown-toggle-split', type: 'button', 'aria-expanded': 'false' });
                    _btnGroup.append('<span class="visually-hidden">Toggle Dropdown</span>');
                    var _ul_1 = $('<ul/>', { class: 'dropdown-menu dropdown-menu-end' });
                    $.each(element.buttons, function (i, el) {
                        el = $.extend({
                            id: '-btn-' + (index + 1) + '-' + (i + 1),
                            enable: true,
                            hidden: false,
                            onClick: null
                        }, el);
                        var _a = $('<a/>', { href: '#', html: el.text, class: 'dropdown-item', id: $this.cardGridId + el.id });
                        if (el.onClick) {
                            $(_a).click(function (e) {
                                e.preventDefault(); //this will prevent the link trying to navigate to another page
                                el.onClick(options.data);
                            });
                        }
                        var _li = $('<li/>').append(_a).appendTo(_ul_1);
                        if (el.hidden)
                            _li.hide();
                        else
                            _li.show();
                    });
                    _btnGroup.appendTo(_divbtnGroup);
                    _ul_1.appendTo(_divbtnGroup);
                    _divbtnGroup.appendTo(_divToolbox);
                }
            }
        });
        return _divToolbox[0];
    };
    // Bootstrap 5 classes for a toolbar button: keeps a caller-supplied colour variant (btn-success, ...)
    // and falls back to the outline style that replaced Bootstrap 3's btn-default.
    CardGrid.buttonClass = function (className) {
        var classes = className ? ' ' + className : '';
        var hasVariant = /(^|\s)btn-(?!group|sm|lg|toggle)[a-z-]+/.test(classes);
        return 'btn' + (hasVariant ? '' : ' btn-outline-secondary') + classes;
    };
    // Get all Selected Data
    CardGrid.prototype.getSelectedData = function () {
        var $this = this;
        var arr = new Array();
        var _carddiv = document.getElementById($this.cardRowId);
        if (_carddiv) {
            if ($this.options.currentView === 'card') {
                var _divs = $(_carddiv).find('[selected]').parent();
                //should be modified to this.options.data.data;
                $.each(_divs, function (ind, ctrl) {
                    $.each($this.getData().Data, function (index, data) {
                        var num = parseInt($(ctrl).attr('data-id'));
                        if (data.Id === num) {
                            arr.push(data);
                        }
                    });
                });
            }
            else {
                var _trs = $(_carddiv).find('table').find('tr[selected="selected"]');
                $.each(_trs, function (idx, obj) {
                    $.each($this.getData().Data, function (index, data) {
                        var num = parseInt($(obj).attr('data-id'));
                        if (data.Id === num) {
                            arr.push(data);
                        }
                    });
                });
            }
        }
        return arr;
    };
    CardGrid.prototype.getSelectedIds = function () {
        var selectedData = this.getSelectedData();
        var arr = new Array();
        $.each(selectedData, function (idx, obj) {
            arr.push(obj.Id);
        });
        return arr;
    };
    return CardGrid;
}());
(function ($) {
    $.fn.cardGrid = function (options) {
        var cardGrid = new CardGrid($(this), options);
        return cardGrid;
    };
})(jQuery);
