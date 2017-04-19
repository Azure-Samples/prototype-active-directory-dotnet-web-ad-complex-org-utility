var lTimezone, lTimezoneAbb, bSkipSBInit;

//global initializer
$(function () {
    var timezone = jstz.determine();
    lTimezone = timezone.name();
    lTimezoneAbb = moment().tz(lTimezone).zoneName();
    $.fx.speeds._default = 200;
});

$(document).ajaxComplete(function () {
    $(".ui-loader").hide();
});
$(document).ajaxStart(function () {
    $(".ui-loader").show();
});
var localErr = {};

//global events
window.onerror = function (msg, url, line, col, err) {
    $(".ajaxloader").hide();
    localErr.Message = msg + "\n  URL: " + url + "\n  Line: " + line + "\n  Col: " + col + "\n  Stack:\n  " + (((err) && (err.stack)) ? err.stack : "N/A");
    localErr.thrownError = "Global Catch";
    var txt = "A general web application error occured. Please <a href='javascript:popErrorUpdater();'>help us out</a> by supplying additional information.";
    SiteUtil.ShowMessage("body", txt.length > 0 ? txt : 'Unexpected error.');
    // If you return true, then error alerts (like in older versions of Internet Explorer) will be suppressed.
    return true;
};
$(document).ajaxError(function (event, xhr, ajaxOptions, thrownError) {
    if (xhr.status == 401) {
        //re-authenticate
        SiteUtil.ShowMessage("Sorry, your authentication token has expired. Please reload your keyed landing page.", "Authentication Expired", SiteUtil.AlertImages.warning);
        return;
    }
    if (typeof xhr.responseJSON == "object") {
        localErr = xhr.responseJSON;

        if (localErr.ErrorMessage != null && localErr.ErrorMessage.length > 0) {
            SiteUtil.ShowMessage(localErr.ErrorMessage, localErr.ErrorTitle, SiteUtil.AlertImages.warning);
            return;
        }

        localErr.thrownError = thrownError;
    } else {
        localErr.Message = xhr.responseText;
        localErr.thrownError = (thrownError || "Unknown");
    }
    var txt = "There was a problem with your request. Please <a href='javascript:popErrorUpdater();'>help us out</a> by supplying additional information.";
    SiteUtil.ShowMessage(txt.length > 0 ? txt : 'Unexpected error.', 'Error', SiteUtil.AlertImages.error);
});


var SiteUtil = function () {
    function copyToClipboard(text) {
        if (window.clipboardData && window.clipboardData.setData) {
            // IE specific code path to prevent textarea being shown while dialog is visible.
            return clipboardData.setData("Text", text);

        } else if (document.queryCommandSupported && document.queryCommandSupported("copy")) {
            var textarea = document.createElement("textarea");
            textarea.textContent = text;
            textarea.style.position = "fixed";  // Prevent scrolling to bottom of page in MS Edge.
            document.body.appendChild(textarea);
            textarea.select();
            try {
                return document.execCommand("copy");  // Security exception may be thrown by some browsers.
            } catch (ex) {
                console.warn("Copy to clipboard failed.", ex);
                return false;
            } finally {
                document.body.removeChild(textarea);
            }
        }
    }

    var ErrorImages = {
        Warning: '/content/images/warning.png',
        Default: '/content/images/info.png'
    };
    function _showModal(options) {
        options.okHide = (options.okHide == null) ? true : options.okHide;
        options.title = (options.title == null) ? "Message" : options.title;
        options.id = (options.id == null) ? "nAlertDialog" : options.id;
        var dialogId = "#" + options.id;
        var btn = {
            OK: function () {
                $(dialogId).modal("hide");
                return false;
            }
        };
        if (options.callback !== null) {
            if (options.callback.length > 0) {
                options.body += "<br><textarea rows='3' cols='50' style='margin-top:5px' id='nDialogVal' size='20'></textarea>";
            }
            btn.Cancel = function () {
                $(dialogId).modal("hide");
                return false;
            };
            btn.OK = function () {
                var res = $("#nDialogVal").val();
                options.callback(res);
                if (options.okHide) $(dialogId).modal("hide");
            };
        }
        var modal = _getModal({
            title: options.title,
            buttons: btn,
            id: options.id,
            body: options.body,
            modalClass: options.modalClass,
            displayCallback: options.displayCallback
        });
        $("#nDialogVal").focus();
        modal.setContent = function (body) {
            modal.body.html(body);
        };
        return modal;
    }
    function _getModal(opts) {
        var res = $("<div/>").addClass("modal fade").attr("id", opts.id || "nAlertDialog");
        var d = $("<div/>").addClass("modal-dialog").appendTo(res);
        if (opts.modalClass) {
            d.addClass(opts.modalClass);
        }
        var c = $("<div/>").addClass("modal-content").appendTo(d);
        var h = $("<div/>").addClass("modal-header").appendTo(c);
        $("<button/>").attr({ "type": "button", "data-dismiss": "modal", "aria-hidden": "true" }).addClass("close").html("&times;").appendTo(h);
        $("<h4/>").addClass("modal-title").html(opts.title || "Alert").appendTo(h);

        var body = $("<div/>").addClass("modal-body").appendTo(c);
        if (typeof opts.body == "object") {
            body.append(opts.body);
        } else {
            body.html(opts.body);
        }
        //body.append("<img src='/images/ajax-loader-dialog.gif' class='ajaxloader ajaxloader-dialog' />");

        if (opts.buttons) {
            var f = $("<div/>").addClass("modal-footer").appendTo(c);
            for (var button in opts.buttons) {
                $("<button/>").attr({ "type": "button" }).addClass("btn btn-default").on("click", opts.buttons[button]).html(button).appendTo(f);
            }
        }
        res.on("hidden.bs.modal", function () {
            $(this).remove();
        });
        res.body = body;
        return res.modal().on("shown.bs.modal", function () {
            if (opts.displayCallback) opts.displayCallback();
        });
    }
    function _utcToLocal(sDate, sInputFormatMask, sOutputFormatMask, bIncludeTZAbb) {
        bIncludeTZAbb = (bIncludeTZAbb == null) ? true : bIncludeTZAbb;
        sOutputFormatMask = sOutputFormatMask || 'MM/DD/YYYY h:mmA';
        var bIsUTC = (typeof sDate == "string" && sDate.indexOf("T") == 10);
        sInputFormatMask = (bIsUTC) ? null : (sInputFormatMask || 'MM/DD/YYYY HH:mmA');
        var res = moment.utc(sDate, sInputFormatMask).tz(lTimezone).format(sOutputFormatMask);
        if (bIncludeTZAbb) res += " " + lTimezoneAbb;
        return (res.indexOf("Invalid date") > -1) ? "N/A" : res;
    }
    function _getShortDate(sDate, sFormat) {
        sFormat = (sFormat || "MM/DD/YYYY");
        return SiteUtil.UtcToLocal(moment(sDate), null, sFormat, false)
    }

    function _utcToServerAndLocal(sDate, sInputFormatMask) {
        var bIsUTC = (typeof sDate == "string" && sDate.indexOf("T") == 10);
        sInputFormatMask = (bIsUTC) ? null : (sInputFormatMask || 'MM/DD/YYYY HH:mmA');
        var dte = moment.utc(sDate, sInputFormatMask);
        var serverDte = dte.format("MM/DD/YYYY h:mmA") + " UTC";
        var localDte = dte.local().format("MM/DD/YYYY h:mmA") + " " + lTimezoneAbb;
        var res = localDte + " (" + serverDte + ")";

        return (res.indexOf("Invalid date") > -1) ? "N/A" : res;
    }
    function _ajaxCall(url, data, callback, method, successMessage) {
        method = (method == null) ? "GET" : method;
        successMessage = (successMessage == null) ? "" : successMessage;
        if (method !== "GET") {
            data = JSON.stringify(data);
        }
        $.ajax({
            url: url,
            data: data,
            type: method,
            withCredentials: true,
            contentType: "application/json",
            success: function (res, status, xhr) {
                if (successMessage.length > 0) {
                    //SiteUtil.ShowMessage(successMessage, "Success", SiteUtil.AlertImages.success);
                    notifySuccess("Success", successMessage);
                }
                if (callback) callback(res, xhr);
            }
        });
    }


    function showMessage(item, message) {
        var html = '<div class="popover" role="tooltip"><div class="arrow"></div><div class="popover-content"></div></div>';
        $(item)
            .on("shown.bs.popover", function () {
                window.setTimeout(function () {
                    $(item).popover('hide');
                }, 2000);
            })
            .popover({ content: message, template: html }).popover('show');
    }

    function getFormObjects(obj, excludeClasses) {
        obj = $(obj);
        excludeClasses = (excludeClasses == null) ? "" : "," + excludeClasses;
        var oForm = obj
            .find("input,textarea,select")
            .not(":submit, :button, :reset, :image, [disabled]" + excludeClasses);

        var radios = $.grep(oForm, function (el) { return el.type == "radio"; });
        var newForm = $.grep(oForm, function (el) { return el.type == "radio"; }, true);
        var newradios = radios.unique()
        $(newradios).each(function (i, o) {
            var r = $('input[name=' + this + ']:checked');
            if (r.length == 0) r = $('input[name=' + this + ']');   //they're all false, so get all and will return the 1st one by default
            r = r.clone()[0];
            if (r.length == 0) $(r).val("");   //they're all false, so Set to "" to no changes made
            r.id = this;
            newForm.push(r);
        });
        return $(newForm);
    }

    function getDataObject(obj, fnMod) {
        var oForm = getFormObjects(obj);
        var oOut = {};
        oForm.each(function () {
            var o = $(this).clone(true, true)[0];
            o.value = this.value;   //sanity check on the clone method, which appears to not do what I want on SELECT objects
            if ((o.tagName == "INPUT") && (o.type == "checkbox")) {
                oOut[o.name] = o.checked;
                return;
            }
            var s = $(o).val();
            var val = ((s == null) ? "" : s.replace(/"(?:[^"\\]|\\.)*"/g, "\{0}"));
            if (oOut[o.name]) {
                if (oOut[o.name].constructor !== Array) {
                    var t = oOut[o.name];
                    oOut[o.name] = [];
                    oOut[o.name].push(t);
                }
                oOut[o.name].push(val);
            } else {
                oOut[o.name] = val;
            }
        });
        return oOut;
    }
    function getLoadState(state) {
        switch (state) {
            case 0: return "PendingHQAdd";
            case 1 : return "PendingRemoteUpdate";
            case 2 : return "PendingHQUpdate";
            case 3 : return "NothingPending";
            case 4 : return "PendingHQDelete";
            case 5 : return "Deleted";
            case 6: return "NewNothingPending";
            default: return "N/A";
        }
    }
    function _deTc(sTitle) {
        var re = /([a-z])([A-Z])/g;
        var res = sTitle.replace(re, "$1 $2");
    }
    return {
        GetLoadState: getLoadState,
        DeTC: _deTc,
        Copy: copyToClipboard,
        ShowMessage: showMessage,
        ShowModal: _showModal,
        GetModal: _getModal,
        AjaxCall: _ajaxCall,
        UtcToLocal: _utcToLocal,
        UtcToServerAndLocal: _utcToServerAndLocal,
        GetShortDate: _getShortDate,
        GetFormObjects: getFormObjects,
        GetDataObject: getDataObject
    };
}();
Array.prototype.unique = function () {
    var o = {}, i, l = this.length, r = [];
    for (i = 0; i < l; i += 1) o[this[i].name] = this[i].name;
    for (i in o) r.push(o[i]);
    return r;
};
$.fn.serializeObject = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name] !== undefined) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};
String.prototype.toTitleCase = function (n) {
    var s = this;
    //if (1 !== n) s = s.toLowerCase();
    return s.replace(/(^|\s)[a-z]/g, function (f) { return f.toUpperCase() });
}