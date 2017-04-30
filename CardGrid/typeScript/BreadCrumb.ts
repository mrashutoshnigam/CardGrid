///<reference path="./library/jquery.d.ts"/>
///<reference path="./OtherPlugin.ts"/>
interface BreadCrumbParams {
    href: string;
    class?: string;
    data?: any;
    text: string;
    callback?: (_a: any) => void;
    icon?: string;
    style?: any;
}

class BreadCrumb {
    public htmlDiv: HTMLDivElement;
    constructor(div: any) {
        this.htmlDiv = div;
    }
    add(key: string, params: BreadCrumbParams): BreadCrumb {
        let options = $.extend({
            data: null,
            href: '#',
            class: 'btn-default',
            text: 'Home',
            callback: null,
            icon: null,
            style: null
        }, params);
        let _span = $('<span/>', {
            href: options.href,
            class: 'btn ' + options.class,
            html: options.icon == null || options.icon == '' ? options.text : '<i class="fa ' + options.icon + '" style="' + options.style + '" />&nbsp;&nbsp;' + options.text,
            'breadcrumb-key': key
        });
        if (options.data)
            _span.data('data', options.data);
        if (options.callback) {
            options.callback(_span);
        } else {
            _span.click(function (event) {
                _span.nextAll().remove();
                _span.blur();
                $('#dynamicForm').loadPage($(this).attr('href'));
            });
        }
        $(this.htmlDiv).append(_span);
        return this;
    }
    remove(key: string): BreadCrumb {
        let anchors = $(this.htmlDiv).children('a');
        $.each(anchors, function (idx, obj) {
            let _span = $(obj);
            if (_span.attr('breadcrumb-key') === key) {
                _span.nextAll().remove();
                _span.remove();
            }
        });
        return this;
    }
    clear(): BreadCrumb {
        $(this.htmlDiv).children().remove();
        return this;
    }
    getLastNode(): JQuery {
        return $(this.htmlDiv).children('span:last-child');
    }
    getNodeByKey(key: string): any {
        return $(this.htmlDiv).children('span[breadcrumb-key="' + key + '"]');
    }
    getData(key?: string): any {
        if (key) {
            return $(this.htmlDiv).children('span[breadcrumb-key="' + key + '"]').data('data');
        } else {
            return $(this.htmlDiv).children('span:last-child').data('data');
        }
    }
    clearNext(a: JQuery): BreadCrumb {
        $(a).nextAll().remove();
        return this;
    }
}