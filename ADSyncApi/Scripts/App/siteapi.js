
var SiteApi = function () {
    var getSite = function (siteId, callback) {
        var url = "/api/RemoteSiteApi/GetSite";
        SiteUtil.AjaxCall(url, "id=" + siteId, function (res) {
            callback(res);
        });
    }
    var getSiteScriptVersion = function (siteId, callback) {
        var url = "/api/RemoteSiteApi/GetSiteScriptVersion";
        SiteUtil.AjaxCall(url, "id=" + siteId, function (res) {
            callback(res);
        });
    }
    var getAllSites = function (callback) {
        var url = "/api/RemoteSiteApi/GetAllSites";
        SiteUtil.AjaxCall(url, null, function (res) {
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

    var getFilteredUsersBySite = function (siteId, filterString, callback) {
        var url = "/api/UserApi/GetFilteredUsersBySite";
        SiteUtil.AjaxCall(url, "siteId=" + siteId + "&filter=" + filterString, function (res) {
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

    var getUserStatus = function (userId, callback) {
        var url = "/api/UserApi/GetUserStatus";
        SiteUtil.AjaxCall(url, "userId=" + userId, function (res) {
            testRelayResponse(res, callback);
        });
    }
    var setUserStatus = function (userId, user, callback) {
        user.userId = userId;
        var url = "/api/UserApi/SetUserStatus";
        SiteUtil.AjaxCall(url, user, function (res) {
            testRelayResponse(res, callback);
        }, "POST");
    }
    var resetPassword = function (userId, reset, callback) {
        reset.userId = userId;
        var url = "/api/UserApi/ResetPw";
        SiteUtil.AjaxCall(url, reset, function (res) {
            testRelayResponse(res, callback);
        }, "POST");
    }

    function testRelayResponse(res, callback) {
        if (res == null) {
            SiteUtil.SiteNotice("An error occured retrieving this information. The remote site may be temporarily offline. Please try again in a few minutes.");
        }
        else if (!res.Success) {
            SiteUtil.SiteNotice(res.ErrorMessage);
        } else {
            callback(res);
        }
    }

    return {
        GetUsersBySite: getUsersBySite,
        GetUserStatus: getUserStatus,
        SetUserStatus: setUserStatus,
        ResetPassword: resetPassword,
        GetFilteredUsersBySite: getFilteredUsersBySite,
        GetUsersByDomain: getUsersByDomain,
        GetLogsBySite: getLogsBySite,
        GetLogsByUser: getLogsByUser,
        GetSite: getSite,
        GetSiteScriptVersion: getSiteScriptVersion,
        GetAllSites: getAllSites,
        UpdateSite: updateSite
    };
}();