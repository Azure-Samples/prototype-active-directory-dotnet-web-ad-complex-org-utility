$(function () {
    var searchTimer = 0;

    $("#UserSearch").on("change keyup", function () {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(function () {
            var filter = $("#UserSearch").val();
            if (filter.length == 0) {
                LoadUsers(currSiteId);
            } else {
                LoadFilteredUsers(currSiteId, filter);
            }
        }, 300);
    });
    function LoadFilteredUsers(id, filter) {
        SiteApi.GetFilteredUsersBySite(id, filter, function (res) {
            SetUsers(res);
        });
    }


});