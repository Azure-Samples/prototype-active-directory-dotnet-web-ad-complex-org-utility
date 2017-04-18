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
    return {
        Copy: copyToClipboard,
        ShowMessage: showMessage
    };
}();
