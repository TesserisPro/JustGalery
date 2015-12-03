var MainViewModel = (function () {
    function MainViewModel() {
        var _this = this;
        this.expandedFolders = null;
        this.images = ko.observableArray();
        this.items = ko.observable();
        this.selectedItem = ko.observable();
        this.treeProviders = ko.observableArray(["Folders"]);
        this.activeTreeProvider = ko.observable();
        this.treeProviderIcons = { Folders: 'fa fa-folder-o', Tags: 'fa fa-tags', Raiting: 'fa fa-star-o' };
        this.singleImageMode = ko.computed(function () { return _this.images().length == 1; });
        this.title = ko.observable();
        this.author = ko.observable();
        this.raiting = ko.observable();
        this.tags = ko.observableArray();
        this.test = ko.observable("test");
        this.progress = ko.observable(-1);
        this.previewProgress = ko.observable(-1);
        this.properties = ko.computed(function () {
            var length = _this.images().length;
            if (length > 0) {
                var properties = new Array();
                for (var p in _this.images()[0].Properties) {
                    properties.push({ title: p, value: _this.images()[0].Properties[p] });
                }
                for (var i = 0; i < length; i++) {
                    var imageProps = _this.images()[i].Properties;
                    properties = properties.filter(function (x) { return imageProps[x.title] == x.value; });
                }
                return properties;
            }
            return new Array(0);
        });
        setTimeout(function () {
            _this.load();
            _this.treeProviders(LightDesk.resolve("NavigationService").GetTreeProviders());
            _this.activeTreeProvider(LightDesk.resolve("NavigationService").GetActiveTreeProvider());
        }, 100);
    }
    MainViewModel.prototype.open = function () {
        var path = LightDesk.resolve("WindowService").SelectFolder();
        if (path) {
            LightDesk.resolve("NavigationService").SetPath(path);
            this.load();
        }
    };
    MainViewModel.prototype.setTreeProvider = function (provider) {
        if (provider) {
            this.activeTreeProvider(provider);
            LightDesk.resolve("NavigationService").SetTreeProvider(provider);
            this.load();
        }
    };
    MainViewModel.prototype.load = function () {
        this.saveExpandedFolders();
        var items = LightDesk.resolve("NavigationService").GetNavigationTree();
        this.items(ko.mapping.fromJS(items));
        this.restoreExpandedFolders();
    };
    MainViewModel.prototype.selectItem = function (id) {
        var item = LightDesk.resolve("NavigationService").SetSelectedItem(id);
        this.selectedItem(ko.mapping.fromJS(item));
        this.updateImages();
    };
    MainViewModel.prototype.setRaiting = function (rating) {
        var _this = this;
        var newRaiting = (this.raiting() == rating ? rating - 1 : rating);
        LightDesk.resolve("ImageService").SetRaiting(newRaiting, function (x) {
            if (x == -1) {
                _this.raiting(newRaiting);
                if (LightDesk.resolve("NavigationService").GetActiveTreeProvider() == "Raiting") {
                    LightDesk.resolve("NavigationService").RefreshTree();
                    _this.load();
                    var item = LightDesk.resolve("NavigationService").GetSelectedItem();
                    _this.selectedItem(ko.mapping.fromJS(item));
                }
            }
            _this.progress(x);
        });
    };
    MainViewModel.prototype.setTitle = function (title) {
        var _this = this;
        LightDesk.resolve("ImageService").SetTitle(title, function (x) { return _this.progress(x); });
    };
    MainViewModel.prototype.setAuthor = function (author) {
        var _this = this;
        LightDesk.resolve("ImageService").SetAuthor(author, function (x) { return _this.progress(x); });
    };
    MainViewModel.prototype.removeTag = function (tag) {
        var _this = this;
        LightDesk.resolve("ImageService").RemoveTag(tag, function (x) {
            _this.progress(x);
            if (x == -1 && LightDesk.resolve("NavigationService").GetActiveTreeProvider() == "Tags") {
                LightDesk.resolve("NavigationService").RefreshTree();
                _this.load();
                var item = LightDesk.resolve("NavigationService").GetSelectedItem();
                _this.selectedItem(ko.mapping.fromJS(item));
            }
        });
    };
    MainViewModel.prototype.addTag = function (tag) {
        var _this = this;
        LightDesk.resolve("ImageService").AddTag(tag, function (x) {
            _this.progress(x);
            if (x == -1 && LightDesk.resolve("NavigationService").GetActiveTreeProvider() == "Tags") {
                LightDesk.resolve("NavigationService").RefreshTree();
                _this.load();
                var item = LightDesk.resolve("NavigationService").GetSelectedItem();
                _this.selectedItem(ko.mapping.fromJS(item));
            }
        });
    };
    MainViewModel.prototype.deleteFiles = function () {
        var _this = this;
        LightDesk.resolve("ImageService").DeleteFiles(function (x) {
            _this.progress(x);
            if (x == -1) {
                LightDesk.resolve("NavigationService").RefreshTree();
                _this.load();
                _this.selectedItem(null);
                _this.images.removeAll();
            }
        });
    };
    MainViewModel.prototype.next = function () {
        var item = LightDesk.resolve("NavigationService").Forward();
        this.updateImages();
        this.selectedItem(ko.mapping.fromJS(item));
    };
    MainViewModel.prototype.previous = function () {
        var item = LightDesk.resolve("NavigationService").Back();
        this.updateImages();
        this.selectedItem(ko.mapping.fromJS(item));
    };
    MainViewModel.prototype.fullScreen = function () {
        LightDesk.resolve("WindowService").ToggleFullScreen();
    };
    MainViewModel.prototype.exit = function () {
        LightDesk.resolve("WindowService").Close();
    };
    MainViewModel.prototype.saveExpandedFolders = function () {
        var _this = this;
        if (this.items()) {
            this.expandedFolders = new Array();
            this.forItems(this.items().Children, function (x) { if (x.Expanded())
                _this.expandedFolders.push(x.Id()); });
        }
    };
    MainViewModel.prototype.restoreExpandedFolders = function () {
        var _this = this;
        if (this.expandedFolders && this.items()) {
            this.forItems(this.items().Children, function (x) { return x.Expanded(_this.expandedFolders.indexOf(x.Id()) != -1); });
        }
    };
    MainViewModel.prototype.forItems = function (items, func) {
        if (items()) {
            var l = items().length;
            for (var i = 0; i < l; i++) {
                var item = items()[i];
                func(item);
                if (item.Children && item.Children()) {
                    this.forItems(item.Children, func);
                }
            }
        }
    };
    MainViewModel.prototype.updateImages = function () {
        var _this = this;
        this.images.removeAll();
        this.title("");
        this.author("");
        this.raiting(0);
        this.tags.removeAll();
        var images = LightDesk.resolve("ImageService").GetSelectedImages(function (x) {
            _this.previewProgress(x);
            if (x == -1) {
                var length = _this.images().length;
                if (length > 0) {
                    var title = _this.images()[0].Title;
                    _this.title(_this.images().every(function (x) { return x.Title == title; }) ? title : "");
                    var author = _this.images()[0].Author;
                    _this.author(_this.images().every(function (x) { return x.Author == author; }) ? author : "");
                    var raiting = _this.images()[0].Raiting;
                    _this.raiting(_this.images().every(function (x) { return x.Raiting == raiting; }) ? raiting : "");
                    var tags = _this.images()[0].Tags;
                    for (var i = 0; i < length; i++) {
                        tags = tags.filter(function (x) { return _this.images()[i].Tags.some(function (t) { return t == x; }); });
                    }
                    _this.tags(tags);
                }
            }
        }, function (image) {
            _this.images.push(image);
        });
    };
    return MainViewModel;
})();
//# sourceMappingURL=MainViewModel.js.map