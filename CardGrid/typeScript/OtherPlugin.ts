
///<reference path="./library/jquery.d.ts"/>
///<reference path="./library/lobibox.d.ts"/>
///<reference path="./library/bootbox.d.ts"/>
///<reference path="CardGrid.ts"/>
///<reference path="BreadCrumb.ts"/>

const apiUrl: string = 'http://localhost:5849/api';
const pageUrl: string = 'http://localhost:5831';
interface JQueryStatic {
    populateDropDown(data: Object, text: string, value: string, defaultOption?: string): JQuery;
    ajaxGetRequest(url: string, callback: any): any;
    validateForm(options?: IFormValidationOptions): HTMLElement;
    modalForm(): JQuery;
    openModal(options: IModalFormOption): JQuery;    
    ajaxFormSubmit(options: IFormSubmitOptions): void;
    translateToHindi(): HTMLElement;
    fillData(data: any, callback?: any): HTMLElement;
    applyPascalCase(): HTMLElement;
    confirmBox(title: string, content: string, callback?: any): void;
    hasAttr(name: string): boolean;
    success(text: string): void;
    error(text: string): void;
    warning(text: string): void;
    info(text: string): void;
    default(text: string): void;
    cardGrid(options: Options): CardGrid;
    loadPage(url: string, callback?: any): any;
}
interface IFormValidationOptions {
    framework?: string;
    icon?: any;
    trigger?: string;
}
interface IModalFormOption {
    data: Object,
    width: number;
}
interface IFormSubmitOptions {
    url: string;
    success: any;
    error?: any;
}
interface ISetCase {
    caseValue: string;
    changeonFocusout: boolean;
}

(function ($) {
    $.fn.plugin = function (settings) {
        var config = {
            settingA: "Example",
            settingB: 5
        };

        if (settings) {
            $.extend(config, settings);
        }

        return this.each(function () {
            return this;
        });
    };
    $.fn.populateDropDown = function (data: Object, text: string, value: string, defaultOption?: string): jQuery {

        function fillData(dropDownCtrl: HTMLElement, data: Object, text: string, value: string, defaultOption?: string): HTMLElement {
            $(dropDownCtrl).empty();
            if (defaultOption) {
                $(dropDownCtrl).append('<option value="0">' + defaultOption + '</option>');
            }
            else {
                $(dropDownCtrl).append('<option value="0">Choose Option</option>');
            }
            $.each(data, function (i: number, o: Object) {
                $(dropDownCtrl).append('<option value="' + o[value] + '">' + o[text] + '</option>')
            });
            return $(dropDownCtrl);
        }
        return this.each((i: number, o: HTMLElement) => {
            return fillData(o, data, text, value, defaultOption)
        });
    };
    $.ajaxGetRequest = function (url: string, callback: any): any {
        $.ajax({
            type: "GET",
            url: apiUrl + url,
            dataType: "json",
            success: function (data, status) {
                if (callback && typeof (callback) === "function") {
                    callback(data);
                    return data;
                }
            },
            error: function (response) {
                if (response.status === 500) {
                    $.notify({ text: response.responseText, type: 'error' });
                } else {
                    $.notify({ text: response.responseText, type: 'info' });
                }
            }
        });
        return null;
    };
    $.fn.validateForm = function (options?: IFormValidationOptions): JQuery {
        let form: JQuery = $(this);
        let opt: IFormValidationOptions;
        if (options)
            opt = options;
        else
            opt = {};
        opt.framework = 'bootstrap';
        opt.icon = {
            valid: 'glyphicon glyphicon-ok',
            invalid: 'glyphicon glyphicon-remove',
            validating: 'glyphicon glyphicon-refresh'
        };
        opt.trigger = 'blur';
        $(form).formValidation(opt);
        return form;
    };
    $.fn.modalForm = function (): JQuery {
        let nform = $(this).clone();
        $(this).remove();
        return nform;
    };
    $.fn.openModal = function (options: IModalFormOption, callback?: any): JQuery {
        let option = $.extend({
            width: 500,
            data: null,
            shown: null
        }, options);
        let mycloneForm: JQuery = $(this).clone(true) as JQuery;
        $('body').append(mycloneForm);
        mycloneForm.modal({ backdrop: 'static' });
        mycloneForm.find('.modal-dialog').css('width', option.width);
        mycloneForm.on('hidden.bs.modal', function () {
            $(this).data('bs.modal', null);
            $(this).remove();
        });
        mycloneForm.fillData(option.data);
        mycloneForm.validateForm();
        mycloneForm.translateToHindi();
        mycloneForm.applyPascalCase();
        if (option.shown != null) {
            mycloneForm.on('shown.bs.modal', function (e) {
                option.shown(mycloneForm);
            });
        }
        if (callback)
            callback(mycloneForm);

        return mycloneForm;
    };
   
    $.fn.ajaxFormSubmit = function (options: IFormSubmitOptions): void {
        let form: JQuery = $(this);
        options.url = apiUrl + options.url;
        let $frm: JQuery = undefined;
        form.on('success.form.fv', function (e) {
            // Prevent form submission
            e.preventDefault();
            var $form = $(e.target);
            // Use Ajax to submit form data
            if ($form.is('form') === true)
                $frm = $form;
            else
                $frm = $form.find('form');
            $.ajax({
                method: "POST",
                url: options.url,
                data: $frm.serialize(),
                dataType: 'json',
                success: function (responseText, statusText, xhr) {
                    if (options.success) {
                        options.success(responseText, statusText, xhr, $frm)
                    }
                },
                error: function (responseText, statusText, xhr, $form) {
                    if (responseText.status === 500 && statusText == 'error') {
                        $.error(responseText.responseText);
                    }
                    if (options.error) {
                        options.error(responseText, statusText, xhr, $frm)
                    }
                }
            });
        });
    };
    $.fn.translateToHindi = function (): HTMLElement {
        let $form = $(this);
        var inputElements: Array<HTMLElement> = $form.find('[translate]');
        $.each(inputElements, function (idx: number, obj: HTMLElement) {
            let originalHtmlElement = $form.find('[name="' + $(obj).attr('translate') + '"]');
            $(originalHtmlElement).blur(function () {
                if (originalHtmlElement.value == '') {
                    $form.find(obj).val('');
                }
                else {
                    var xhttp = new XMLHttpRequest();
                    xhttp.onreadystatechange = function () {
                        if (xhttp.readyState == 4 && xhttp.status == 200) {

                            var html = JSON.parse(xhttp.responseText);

                            let str: string = '';
                            for (let i: number = 0; i < html[1].length; i++) {
                                str += html[1][i][1] + ',';
                            }
                            var i = str.lastIndexOf(',');
                            str = str.slice(0, i);
                            $form.find(obj).val(str);
                            $(obj).blur();

                        }
                    };
                    xhttp.open("POST", "https://www.google.com/inputtools/request?text=" + $(originalHtmlElement).val() + "&ime=transliteration_en_hi&num=1&ie=utf-8&oe=utf-8&alt=json", true);
                    xhttp.send();
                }
            });
        });
        return $form;
    };
    $.fn.fillData = function (data: any, callback?: any): HTMLElement {
        let $form = $(this);
        $.each(data, function (i: number, o: any) {
            $.each(o, function (key: number, value: any) {
                $form.find('[name="' + key + '"]').val(value);
            });
        });
        if (callback && typeof (callback) === 'function')
            callback(data);
        return $form;
    };
    $.fn.applyPascalCase = function (): HTMLElement {
        let htmlElement: Array<HTMLElement> = $(this).find('input[type="Text"]');
        $.each(htmlElement, function (index: number, obj: any) {
            $(obj).Setcase({ caseValue: 'pascal', changeonFocusout: true });
        });
        return this;
    };
    $.confirmBox = function (title: string, content: string, callback: any): void {
        let dialogId = 'modal-dialog-' + Math.floor((Math.random() * 100) + 1000).toString();
        let _pDiv: JQuery = $('<div/>', { id: dialogId, class: 'modal fade', tabindex: "-1", role: 'dialog' });
        let _modal_d = $('<div/>', { class: 'modal-dialog', style: 'width:400px' });
        let _modal_x_panel = $('<div/>', { class: 'x_panel' });
        let _modal_x_title = $('<div/>', { class: 'x_title' });
        let _btn_dismiss = $('<button/>', { type: 'button', class: 'close', 'data-dismiss': 'modal' });
        _btn_dismiss.append($('<span/>', { 'area-hidden': true, text: 'x' }));
        let _modal_h4 = $('<h2/>', { html: title });
        _modal_x_title.append(_modal_h4).append(_btn_dismiss).append('<div class="clearfix"></div>');
        _modal_x_panel.append(_modal_x_title);
        _modal_d.append(_modal_x_panel);
        let _modal_x_content = $('<div/>', { class: 'x_content' });
        let _h2 = $('<h2/>', { html: content }).appendTo(_modal_x_content);
        _modal_x_content.append('<div class="ln_solid"></div>');
        let _modal_footer = $('<div/>', { style: 'float:right' });
        let _modal_confirm_btn = $('<button/>', { type: 'button', text: 'Yes', class: 'btn btn-success' });
        _modal_confirm_btn.click(function () {
            $(_pDiv).modal('hide');
            callback();
        });
        let _modal_cancel_btn = $('<button/>', { type: 'button', text: 'No', class: 'btn btn-danger', 'data-dismiss': 'modal' });
        _modal_footer.append(_modal_cancel_btn).append(_modal_confirm_btn);
        _modal_x_content.append(_modal_footer);
        _modal_x_panel.append(_modal_x_content);
        _modal_d.append(_modal_x_panel);
        _pDiv.append(_modal_d);
        $(_pDiv).modal('show');
        $(_pDiv).on('hidden.bs.modal', function () {
            $(this).data('bs.modal', null);
            $(this).remove();
        });
    };
    $.fn.hasAttr = function (name: string): boolean {
        return $(this).attr(name) !== undefined;
    }

    $.fn.cardGrid = function (options: Options): CardGrid {
        let cardGrid = new CardGrid($(this), options);
        return cardGrid;
    }
    $.fn.breadCrumb = function () {
        let breadcrumb = new BreadCrumb(this);
        return breadcrumb;
    }

    $.fn.loadPage = function (url: string, callback?: any): any {
        if (url !== undefined && url != '' && url != '#') {
            let htmlDiv = $(this);
            var xhttp = new XMLHttpRequest();
            xhttp.onreadystatechange = function () {
                if (xhttp.readyState == 4 && xhttp.status == 200) {
                    if (callback) {
                        callback();
                    }
                    $(htmlDiv).html(xhttp.responseText);
                }
            };
            xhttp.open("GET", pageUrl + url, true);
            xhttp.send();

        }
        return null;
    }
    $.delete = function (url: string, idsCollection: any, callback: any): void {
        url = apiUrl + url;
        $.ajax({
            method: "POST",
            url: url,
            data: { Data: idsCollection },
            dataType: 'json',
            success: function (responseText, statusText, xhr) {
                debugger;
                callback(true);
            },
            error: function (responseText, statusText, xhr, $form) {
                $.error(responseText.responseText);
                callback(false);
            }
        });
    }
    $.success = function (text: string): void {
        notifyMe('success', text);
    }
    $.error = function (text: string): void {
        notifyMe('error', text);

    }
    $.warning = function (text: string): void {
        notifyMe('warning', text);

    }
    $.info = function (text: string): void {
        notifyMe('info', text);
    }

    $.default = function (text: string): void {
        notifyMe('default', text);
    }
    $.fillUserInfo = function (responseText: any) {
        var fullname = responseText.FirstName + ' ' + responseText.LastName;
        $('#userName').html(fullname);
        if (responseText.ImageUrl) {
            var img = $('<img/>', { alt: '', id: 'profile-img', class: "profile-img-card", src: responseText.ImageUrl });
            $('#userImageUrl').empty().append(img);
            $('#topNavProfile').empty().append($('<img/>', { src: responseText.ImageUrl, alt: '' })).append(fullname).append('&nbsp;<span class="fa fa-angle-down"></span>');
        } else {
            var img = $('<img/>', { alt: '', id: 'profile-img', class: "profile-img-card", src: '/images/member_login_icon.png' });
            $('#userImageUrl').empty().append(img);
            $('#topNavProfile').empty().append($('<img/>', { src: '/images/member_login_icon.png', alt: '' })).append(fullname).append('&nbsp;<span class="fa fa-angle-down"></span>');
        }
    }
    $.profileText = function (text: string): string {
        if (text == '')
            return '';
        let _textArray: Array<string> = text.split(' ');
        let _str: string = '';
        if (_textArray.length > 1) {
            for (let i = 0; i < 2; i++) {
                _str += _textArray[i][0];
            }
        }
        else {
            _str = text[0] + text[1];
        }
        return _str.toUpperCase();
    }
    $.randomColor = function () {
        var color = '#'; // hexadecimal starting symbol
        var letters = '0123456789ABCDEF'.split(''); //hexadecimal color letters

        //For loop that will create random hexadecimal value.
        for (var i = 0; i < 6; i++) {
            color += letters[Math.round(Math.random() * 15)];
        }
        return color;
        // return '#' + Math.floor(Math.random() * 16777215).toString(16);
    }
})(jQuery);

function notifyMe(type: string, text: string) {
    Lobibox.notify(type,
        {
            title: false, msg: text, pauseDelayOnHover: true, continueDelayOnInactiveTab: false, icon: true, delayIndicator: false, sound: false, showClass: 'slideInRight',
            hideClass: 'slideOutRight', size: 'mini'
        });
}