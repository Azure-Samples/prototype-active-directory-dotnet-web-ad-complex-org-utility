$(function () {
    var defaultLogDays = 1;
    $("#siteList li").on("click", function () {
        LoadSite($(this).data("id"));
    });

    function LoadSite(id) {
        $("#SiteContainer").data("siteId", id);
        SiteApi.GetUsersBySite(id, function (res) {
            $("#UserList").children().remove();
            for (x = 0; x < res.length; x++) {
                var data = res[x];
                $("<li/>")
                    .data("data", data)
                    .addClass("list-group-item bg-rowEdit")
                    .on("click", function () {
                        var d = $(this).data("data");
                        loadUserDetail(d);
                    })
                    .html(data.surname + ", " + data.givenName).appendTo("#UserList");
            }
        });
    }
    function loadUserDetail(user) {
        var val = "";
        for (col in user) {
            var input = col.toTitleCase();
            switch (input) {
                case "CreateDate":
                    val = SiteUtil.UtcToLocal(user[col]);
                    break;
                case "LoadState":
                    val = SiteUtil.GetLoadState(user[col]);
                    break;
                default:
                    val = user[col];
            }
            $("#" + input).val(val);
        }
    }

    function loadLogs(res) {
        $("#LogList").children("tr:gt(0)").remove();
        for (x = 0; x < res.length; x++) {
            var data = res[x];
            var tr = $("<tr>");
            tr.data("data", data).on("click", function () {
                var d = $(this).data("data");
                showDetailDialog(d);
            });
            var d = SiteUtil.UtcToLocal(data.logDate);
            $("<td>").html(d).appendTo(tr);
            $("<td>").html(data.errorType).appendTo(tr);
            $("<td>").html(data.site).appendTo(tr);
            $("<td>").html(data.user).appendTo(tr);
            $("<td>").html(data.source).appendTo(tr);
            var detail = (data.detail.length > 200) ? data.detail.substring(0, 200) + "..." : data.detail;
            $("<td>").html(detail).appendTo(tr);
            switch (data.errorType) {
                case "Info":
                    bg = "default";
                    break;
                case "Warning":
                    bg = "#feffb4";
                    break;
                case "Error":
                    bg = "#ffbdbd";
                    break;
                case "Success":
                    bg = "#c3fbc7";
                    break;
            }
            tr.css("backgroundColor", bg);
            tr.appendTo("#LogList");
        }
    }
    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        var id = $("#SiteContainer").data("siteId");

        switch (e.target.text) {
            case "Users":
                break;
            case "Logs":
                SiteApi.GetLogsBySite(id, defaultLogDays, function (res) {
                    loadLogs(res);
                });
                break;
            case "Settings":
                SiteApi.GetSite(id, function (res) {
                    $("#SiteType").html("");
                    for (var t in res.SiteTypes) {
                        var o = $("<option/>").val(t).html(res.SiteTypes[t]);
                        $("#SiteType").append(o);
                    }
                    $("#SiteType").val(res.Site.siteType);
                    $("#ApiKey").val(res.Site.apiKey);
                    $("#B2BRedirectUrl").val(res.Site.b2bRedirectUrl);
                    $("#Id").val(res.Site.id);
                    var dl = "";
                    for (var x = 0; x < res.Site.siteDomain.length; x++) {
                        dl += res.Site.siteDomain[x] + "\n";
                    }
                    $("#SiteDomainsList").val(dl);
                    checkSiteType();
                });
                break;
        }
    })

    $("#SiteType").on("change", function () {
        checkSiteType();
    });
    function checkSiteType() {
        var val = $("#SiteType").val();
        //master for default, b2b for per-site
        var show = (val == "0" || val == "1") ? "block" : "none";
        var fg = $("#B2BRedirectUrl").closest("div.form-group");
        var lbl = (val == "0") ? "B2B Default Redirect URL" : "B2B Redirect Url";
        fg.children("label").html(lbl);
        fg.css("display", show);
        $("#B2BRedirectUrl").focus();
    }
    $("#ApiKey").on("focus", function () {
        setTimeout(function () {
            $("#ApiKey").select();
        }, 100);
    });

    $("#btnCopyApiKey").on("click", function () {
        if (SiteUtil.Copy($("#ApiKey").val())) {
            SiteUtil.ShowMessage("#btnCopyApiKey", "Key copied to clipboard");
        } else {
            SiteUtil.ShowMessage("#btnCopyApiKey", "Key not copied to clipboard, your browser may not support this operation. Please Ctrl-C to copy.");
            setTimeout(function () {
                $("#ApiKey").select();
            }, 100);
        }
    });

    $("#btnCancel").on("click", function () {
        location.href = "/RemoteSite";
    });

    $("#btnDelete").on("click", function () {
        if (!confirm("Are you sure you want to delete this site?")) return;

        document.forms[1].action = "/RemoteSite/Delete";
        document.forms[1].submit();
    });

    $("#btnSave").on("click", function () {
        var site = SiteUtil.GetDataObject("#frmSite");
        SiteApi.UpdateSite(site, function (res) {
            debugger;
        });
    });

    $("#btnNew").on("click", function () {
        location.href = "/RemoteSite/Edit";
    });

    $("table tr:gt(0)")
        .on("click", function () {
            location.href = "/RemoteSite/Edit/" + $(this).data("id");
        });

    LoadSite($("#siteList li:first").data("id"));

    var showDetailDialog = function (data) {
        var res = $("<div/>");
        var i = 0;
        for (col in data) {
            i++;
            var ds = "";
            var bg = (i % 2 == 0) ? "#fafafa;" : "";
            var d = $("<div/>").css("backgroundColor", bg).appendTo(res);
            if (data[col] != null) {
                if (col == 'logDate') {
                    ds = SiteUtil.UtcToServerAndLocal(data[col]);
                } else if (typeof data[col] == "object") {
                    var data2 = data[col];
                    for (var col2 in data2) {
                        ds += col2 + ": " + data2[col2] + "<br>";
                    }
                } else {
                    ds = data[col].toString().replace(/\r\n/g, "<br/>");
                }
                d.html("<span class='label'>" + SiteUtil.DeTC(col) + "</span><span class='data'>" + ds + "</span>");
            }
        }
        $(".modal-body").html(res.html());
        $("#ErrorDialog").data("id", data.id).modal('show');
    };
});


var SiteApi = function () {
    var getSite = function (siteId, callback) {
        var url = "/api/RemoteSiteApi/GetSite";
        SiteUtil.AjaxCall(url, "id=" + siteId, function (res) {
            callback(res);
        });
    }

    var updateSite = function (site, callback) {
        var url = "/api/RemoteSiteApi/UpdateSite";
        SiteUtil.AjaxCall(url, site, function (res) {
            callback(res);
        },"POST");
    }

    var getUsersBySite = function (siteId, callback) {
        var url = "/api/UserApi/GetUsersBySite";
        SiteUtil.AjaxCall(url, "siteId=" + siteId, function (res) {
            callback(res);
        });
    }

    var getUsersByDomain = function(domain, callback) {
        var url = "/api/UserApi/getUsersByDomain";
        SiteUtil.AjaxCall(url, "domain=" + domain, function (res) {
            callback(res);
        });
    }

    var getLogsBySite = function (siteId, days, callback) {
        var url = "/api/LogApi/getLogsBySite";
        var q = "siteId=" + siteId + "&days=" + days;
        SiteUtil.AjaxCall(url, q, function (res) {
            callback(res);
        });
    }

    var getLogsByUser = function (userId, days, callback) {
        var url = "/api/LogApi/getLogsByUser";
        var q = "userId=" + userId + "&days=" + days;
        SiteUtil.AjaxCall(url, q, function (res) {
            callback(res);
        });
    }

    return {
        GetUsersBySite: getUsersBySite,
        GetUsersByDomain: getUsersByDomain,
        GetLogsBySite: getLogsBySite,
        GetLogsByUser: getLogsByUser,
        GetSite: getSite,
        UpdateSite: updateSite
    };
}();