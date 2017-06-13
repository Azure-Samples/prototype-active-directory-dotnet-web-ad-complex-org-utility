$(function () {
    var defaultLogDays = 1;
    $("#siteList li").on("click", function () {
        SetSiteClick(this);
    });
    var searchTimer = 0;
    $("#UserSearch").on("change keyup", function () {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(function () {
            var currSiteId = $("#SiteContainer").data("siteId");
            var filter = $("#UserSearch").val();
            if (filter.length == 0) {
                LoadUsers(currSiteId);
            } else {
                LoadFilteredUsers(currSiteId, filter);
            }
        }, 300);
    });

    function SetSiteClick(li) {
        $("#siteList li").removeClass("active");
        $(li).addClass("active");
        LoadSite($(li).data("id"));
    }

    function LoadSite(id) {
        $("#SiteContainer").data("siteId", id);
        var a = $("#pageTabs li.active a").text();
        loadUserTab(a, id);
    }

    $('#pageTabs a[data-toggle="tab"]').on('click', function (e) {
        var id = $("#SiteContainer").data("siteId");
        loadUserTab(e.target.text, id);
    });

    function loadUserTab(tab, id) {
        switch (tab) {
            case "Users":
                clearUserDetail();
                LoadUsers(id);
                break;
            case "Logs":
                LoadLogs(id);
                break;
            case "Setup":
                var sn = $("#siteList li.active span.siteitem").text();
                sn = sn.replace(/\./g, "_");
                var lnkArr = $("#SetupZipLink")[0].href.split("?");
                var url = lnkArr[0] + "?siteId=" + id;
                $("#SetupZipLink")
                    .html(sn + "_setup.zip")
                    .attr("href", url);
                break;
            case "Settings":
                LoadSiteSettings(id);
                break;
        }
    }

    function SetUsers(res) {
        $("#UserList").children().remove();
        if (res.length == 0) {
            $("<li/>")
                .addClass("list-group-item bg-rowEdit")
                .html("No staged users").appendTo("#UserList");

            return;
        }
        for (x = 0; x < res.length; x++) {
            var data = res[x];
            var name = (data.surname == null) ? "" : data.surname + ", ";
            name += data.givenName;
            if (name == "null") {
                name = (data.displayName || data.name);
            }
            $("<li/>")
                .data("data", data)
                .addClass("list-group-item bg-rowEdit")
                .on("click", function () {
                    var d = $(this).data("data");
                    loadUserDetail(d);
                })
                .html(name).appendTo("#UserList");
        }
    }
    function LoadFilteredUsers(id, filter) {
        SiteApi.GetFilteredUsersBySite(id, filter, function (res) {
            SetUsers(res);
        });
    }

    function LoadUsers(id) {
        SiteApi.GetUsersBySite(id, function (res) {
            SetUsers(res);
        });
    }
    function clearUserDetail() {
        var frm = SiteUtil.GetFormObjects("#frmUser");
        $(frm).each(function (i, o) {
            $(o).val("");
        });
    }
    function LoadSiteSettings(id) {
        SiteApi.GetSite(id, function (res) {
            var frm = $("#frmSite");
            frm.data("site", res.Site);
            frm.find("select[name=SiteType]").html("");
            for (var t in res.SiteTypes) {
                var o = $("<option/>").val(t).html(res.SiteTypes[t]);
                $("#SiteType").append(o);
            }
            frm.find("select[name=SiteType]").val(res.Site.siteType);
            frm.find("input[name=ApiKey]").val(res.Site.apiKey);
            frm.find("input[name=B2BRedirectUrl]").val(res.Site.b2bRedirectUrl);
            frm.find("input[name=OnPremDomainName]").val(res.Site.onPremDomainName);
            frm.find("input[name=Id]").val(res.Site.id);
            frm.find("input[name=ApiKey]").val(res.Site.apiKey);

            var dl = "";
            for (var x = 0; x < res.Site.siteDomain.length; x++) {
                dl += res.Site.siteDomain[x] + "\n";
            }
            dl = dl.substring(0, dl.length - 1);
            frm.find("textarea[name=SiteDomainsList]").val(dl);
            checkSiteType();
        });
        refreshScriptVersion(id);
    }

    function refreshScriptVersion(id) {
        SiteApi.GetSiteScriptVersion(id, function (res) {
            $("#SiteScriptVersion").val(res);
            curr = $("#CurrSiteScriptVersion").val();
            var cl = "glyphicon-ok";
            var color = "green";
            if (res != curr) {
                cl = "glyphicon-time";
                color = "brown";
            }
            $("#ssvOK").removeClass("glyphicon-ok glyphicon-time").addClass(cl).css("color", color);
        });
    }

    $("#ssvOK").on("click", function () {
        var frm = $("#frmSite");
        var id = frm.data("site").id;
        refreshScriptVersion(id);
    });

    function LoadLogs(id) {
        SiteApi.GetLogsBySite(id, defaultLogDays, function (res) {
            $("#LogList tr:not(.theader)").remove();
            if (res.length == 0) {
                var tr = $("<tr>");
                $("<td colspan='6'>").html("No logs were found for this site").appendTo(tr);
                tr.appendTo("#LogList");
                return;
            }
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
        });
    }
    function RefreshSiteList() {
        SiteApi.GetAllSites(function (res) {
            $("#siteList li").remove();
            if (res.length == 0) {
                $("<li>").addClass("list-group-item").html("No sites configured").appendTo("#siteList");
                return;
            }
            var currSiteId = $("#SiteContainer").data("siteId");
            for (x = 0; x < res.length; x++) {
                var item = res[x];
                var masterClass = (item.siteType == 0) ? " master" : "";
                var active = (currSiteId == item.id) ? " active" : "";
                $("<li>").addClass("list-group-item bg-rowEdit" + masterClass + active)
                    .data("id", item.id)
                    .attr("title", "Click to edit")
                    .on("click", function () {
                        SetSiteClick(this);
                    })
                    .css("cursor","pointer")
                    .html("<span class='badge'>" + item.siteDomain.length + "</span>" + item.siteDomain[0])
                    .appendTo("#siteList");
            }
        });
    }

    $('#userTabs a[data-toggle="tab"]').on('click', function (e) {
        var userData = $("#frmUser").data("userData");
        var userId = userData.id;

        switch (e.target.text) {
            case "General":
                break;
            case "Contact":
                break;
            case "Advanced":
                break;
            case "Manage":
                LoadUserStatus(userId);
                break;
        }
    });

    $("#SiteType").on("change", function () {
        checkSiteType();
    });

    function loadUserDetail(user) {
        var val = "";
        var frm = $("#frmUser");
        frm.data("userData", user);

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
            frm.find("input[name=" + input + "]").val(val);
        }
        var a = $("#userTabs li.active a").text();
        if (a == "Manage") LoadUserStatus(user.id);
    }

    function SetUserStatus(user) {
        SiteApi.SetUserStatus(userId, user, function (user) {

        });
    }
    $("div.acctexpblock input:radio").on("click", function () {
        var val = SiteUtil.RadioButton.Get("AcctExpSel");
        if (val == "no") {
            SiteUtil.RadioButton.Set("AcctExpSel", "no");
            var d = $("#AccountExpirationDate").val();
            $("#AccountExpirationDate").data("orgval", d).val("").prop("disabled", true);
        } else {
            var d = $("#AccountExpirationDate").data("orgval");
            $("#AccountExpirationDate").val(d).prop("disabled", false);
            SiteUtil.RadioButton.Set("AcctExpSel", "yes");
        }
    });

    function LoadUserStatus(userId) {
        SiteApi.GetUserStatus(userId, function (res) {
            var val = "";
            var frm = $("#userManage");
            var user = res.Data;
            for (col in user) {
                var input = col.toTitleCase();
                switch (input) {
                    case "AccountExpirationDate":
                        val = SiteUtil.UtcToLocal(user[col], null, 'MM/DD/YYYY', false);
                        if (val == "N/A") {
                            val = "";
                            SiteUtil.RadioButton.Set("AcctExpSel", "no");
                            $("#AccountExpirationDate").prop("disabled",true);
                        } else {
                            $("#AccountExpirationDate").prop("disabled", false);
                            SiteUtil.RadioButton.Set("AcctExpSel", "yes");
                        }
                        break;

                    case "AccountLockoutTime":
                    case "LastBadPasswordAttempt":
                    case "LastLogon":
                    case "LastPasswordSet":
                        val = SiteUtil.UtcToLocal(user[col]);
                        break;

                    case "UserCannotChangePassword":
                    case "PasswordNeverExpires":
                    case "SmartcardLogonRequired":
                        frm.find("input[name=" + input + "]").prop("checked", user[col]);
                        continue;

                    case "SamAccountName":
                        val = user[col];
                        $("#SamAccountName").val(val);
                        $("#OrgSamAccountName").val(val);
                        break;

                    case "OrgSamAccountName":
                        continue;

                    case "Enabled":
                        val = (!user[col]);
                        var c = frm.find("input[name=Disabled]");
                        c.prop("checked", val);
                        var l = c.parent().next();
                        l.css("background-color", (val ? "pink" : "white"))
                        continue;

                    default:
                        val = user[col];
                }
                frm.find("input[name=" + input + "]").val(val);
            }
        });
    }
    $("#btnResetPassword").on("click", function () {
        //check passwords
        var p1 = $("#NewPassword1").val();
        var p2 = $("#NewPassword2").val();
        if (p1 != p2) {
            alert("Passwords do not match, please try again.");
            $("#NewPassword1").focus();
            return;
        }

        var frm = $("#frmUser");
        var user = frm.data("userData");

        var data = {
            UserName: user.upn,
            NewPassword: p1,
            Unlock: $("#SetUnlockUserAccount").prop("checked"),
            SetChangePasswordAtNextLogon: $("#SetChangePasswordAtNextLogon").prop("checked")
        }
        SiteApi.ResetPassword(user.id, data, function (res) {
            if (res.Success) {
                LoadUserStatus(user.id);
                $("#PWResetDialog").modal('hide');
                SiteUtil.SiteNotice("Password reset successfully");
            } else {
                SiteUtil.ShowMessage("#btnResetPassword", res.ErrorMessage, 6000);
            }
        });
    });
    $("#btnUserApply").on("click", function () {
        var frm = $("#frmUser");
        var user = frm.data("userData");

        var adUser = {
            Enabled: (!$("#Disabled").prop("checked")),
            AccountExpirationDate: $("#AccountExpirationDate").val(),
            SamAccountName: $("#SamAccountName").val(),
            OrgSamAccountName: $("#OrgSamAccountName").val(),
            SmartcardLogonRequired: $("#SmartcardLogonRequired").prop("checked"),
            UserCannotChangePassword: $("#UserCannotChangePassword").prop("checked"),
            PasswordNeverExpires: $("#PasswordNeverExpires").prop("checked")
        }

        SiteApi.SetUserStatus(user.id, adUser, function (res) {
            if (res.Success) {
                LoadUserStatus(user.id);
                SiteUtil.SiteNotice("Account updated successfully");
            } else {
                SiteUtil.SiteNotice(res.ErrorMessage);
            }
        });
    });

    $("#btnPWResetDialog").on("click", function () {
        var frm = $("#frmUser");
        var user = frm.data("userData");
        $("#pwResetTitleLabel").html("Reset Password - " + user.upn);
        $("#lblSetPwChangeOnLogon").removeClass('disabled');
        $("#lblUnlockUser").addClass('disabled');

        var isLocked = ($("#AccountLockoutTime").val() != "N/A");
        var noChangePw = $("#UserCannotChangePassword").prop("checked");

        $("#SetChangePasswordAtNextLogon").prop("disabled", noChangePw);
        $("#spLockoutStatus").html(isLocked ? "Locked out" : "Unlocked");

        if (noChangePw) {
            $("#lblSetPwChangeOnLogon").addClass('disabled');
        }

        if (isLocked) {
            $("#lblUnlockUser").removeClass('disabled');
            $("#SetUnlockUserAccount").prop("disabled", !isLocked);
        }
        $("#PWResetDialog").modal('show');
    });
    $('#PWResetDialog').on('shown.bs.modal', function () {
        $("#NewPassword1").focus();
    })
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
            RefreshSiteList();
            SiteUtil.SiteNotice("Site updated");
            $("#btnSave").blur();
        });
    });

    $("#btnNew").on("click", function () {
        location.href = "/RemoteSite/Edit";
    });

    $("table tr:gt(0)")
        .on("click", function () {
            location.href = "/RemoteSite/Edit/" + $(this).data("id");
        });

    //init
    $("#siteList li:first").addClass("active");
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
        $("#ErrorBody").html(res.html());
        $("#ErrorDialog").data("id", data.id).modal('show');
    };
});

