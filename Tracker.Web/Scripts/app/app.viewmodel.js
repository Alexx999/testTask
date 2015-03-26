function AppViewModel(dataModel) {
    // Private state
    var self = this;

    // Private operations
    function cleanUpLocation() {
        window.location.hash = "";

        if (typeof (history.pushState) !== "undefined") {
            history.pushState("", document.title, location.pathname);
        }
    }
    // Data
    self.Views = {
        Loading: {} // Other views are added dynamically by app.addViewModel(...).
    };
    self.dataModel = dataModel;

    self.expenses = ko.betterObservableArray();

    self.isLoaded = ko.observable(false);

    // UI state
    self.view = ko.observable(self.Views.Loading);

    self.isLoading = ko.pureComputed(function () {
        return self.view() === self.Views.Loading;
    });

    // UI operations

    // Other navigateToX functions are added dynamically by app.addViewModel(...).

    // Other operations
    self.addViewModel = function (options) {
        var viewItem = new options.factory(self, dataModel),
            navigator;

        self["is" + options.name] = ko.pureComputed(function () {
            return self.view() === self.Views[options.name];
        });

        // Add view to AppViewModel.Views enum (for example, app.Views.Home).
        self.Views[options.name] = viewItem;

        // Add binding member to AppViewModel (for example, app.home);
        self[options.bindingMemberName] = ko.computed(function () {
            if (!dataModel.getAccessToken()) {
                // The following code looks for a fragment in the URL to get the access token which will be
                // used to call the protected Web API resource
                var fragment = common.getFragment();

                if (fragment.access_token) {
                    // returning with access token, restore old hash, or at least hide token
                    window.location.hash = fragment.state || '';
                    dataModel.setAccessToken(fragment.access_token);
                } else {
                    // no token - so bounce to Authorize endpoint in AccountController to sign in or register
                    window.location = "/Account/Authorize?client_id=web&response_type=token&state=" + encodeURIComponent(window.location.hash);
                }
            }

            return self.Views[options.name];
        });

        if (typeof (options.navigatorFactory) !== "undefined") {
            navigator = options.navigatorFactory(self, dataModel);
        } else {
            navigator = function () {
                window.location.hash = options.bindingMemberName;
            };
        }

        // Add navigation member to AppViewModel (for example, app.NavigateToHome());
        self["navigateTo" + options.name] = navigator;
    };

    self.initialize = function () {
        Sammy().run();

        self.simpleAjax(self.dataModel.userExpensesUrl, "GET", undefined,
            function (data) {
                self.expenses.setValue(data);
                self.isLoaded(true);
            });
    }

    self.simpleAjax = function (url, method, data, success, error) {
        $.ajax({
            method: method,
            url: url,
            contentType: "application/json; charset=utf-8",
            data: data,
            headers: {
                'Authorization': 'Bearer ' + self.dataModel.getAccessToken()
            },
            success: function (data) {
                if (success != null) {
                    success(data);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                if (error != null) {
                    error(jqXHR.responseJSON);
                }
            }
        });
    }
}

var app = new AppViewModel(new AppDataModel());
