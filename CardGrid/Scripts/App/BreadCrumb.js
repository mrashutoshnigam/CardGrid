///<reference path="./library/jquery.d.ts"/>
///<reference path="./OtherPlugin.ts"/>
var BreadCrumb = (function () {
    function BreadCrumb(div) {
        this.htmlDiv = div;
    }
    BreadCrumb.prototype.add = function (key, params) {
        var options = $.extend({
            data: null,
            href: '#',
            class: 'btn-default',
            text: 'Home',
            callback: null,
            icon: null,
            style: null
        }, params);
        var _span = $('<span/>', {
            href: options.href,
            class: 'btn ' + options.class,
            html: options.icon == null || options.icon == '' ? options.text : '<i class="fa ' + options.icon + '" style="' + options.style + '" />&nbsp;&nbsp;' + options.text,
            'breadcrumb-key': key
        });
        if (options.data)
            _span.data('data', options.data);
        if (options.callback) {
            options.callback(_span);
        }
        else {
            _span.click(function (event) {
                _span.nextAll().remove();
                _span.blur();
                $('#dynamicForm').loadPage($(this).attr('href'));
            });
        }
        $(this.htmlDiv).append(_span);
        return this;
    };
    BreadCrumb.prototype.remove = function (key) {
        var anchors = $(this.htmlDiv).children('a');
        $.each(anchors, function (idx, obj) {
            var _span = $(obj);
            if (_span.attr('breadcrumb-key') === key) {
                _span.nextAll().remove();
                _span.remove();
            }
        });
        return this;
    };
    BreadCrumb.prototype.clear = function () {
        $(this.htmlDiv).children().remove();
        return this;
    };
    BreadCrumb.prototype.getLastNode = function () {
        return $(this.htmlDiv).children('span:last-child');
    };
    BreadCrumb.prototype.getNodeByKey = function (key) {
        return $(this.htmlDiv).children('span[breadcrumb-key="' + key + '"]');
    };
    BreadCrumb.prototype.getData = function (key) {
        if (key) {
            return $(this.htmlDiv).children('span[breadcrumb-key="' + key + '"]').data('data');
        }
        else {
            return $(this.htmlDiv).children('span:last-child').data('data');
        }
    };
    BreadCrumb.prototype.clearNext = function (a) {
        $(a).nextAll().remove();
        return this;
    };
    return BreadCrumb;
}());
//# sourceMappingURL=BreadCrumb.js.map