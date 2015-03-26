function AppDataModel() {
    var self = this;
    // Routes
    self.userExpensesUrl = "/api/Expenses";
    self.siteUrl = "/";

    // Route operations

    // Other private operations

    // Operations

    // Data
    self.returnUrl = self.siteUrl;
    self.dateFormat = "YYYY-MM-DD";
    self.timeFormat = "HH:mm";

    // Data access operations
    self.setAccessToken = function (accessToken) {
        sessionStorage.setItem("accessToken", accessToken);
    };

    self.getAccessToken = function () {
        return sessionStorage.getItem("accessToken");
    };
}
