class MainViewModel {

    private expandedFolders: Array<string> = null;

    public images = ko.observableArray<any>();

    public items = ko.observable<any>();

    public selectedItem = ko.observable();

    public treeProviders = ko.observableArray<string>(["Folders"]);

    public activeTreeProvider = ko.observable<string>();

    public treeProviderIcons = { Folders: 'fa fa-folder-o', Tags: 'fa fa-tags', Raiting: 'fa fa-star-o' };

    public singleImageMode = ko.computed(() => this.images().length == 1);

    public title = ko.observable<string>();

    public author = ko.observable<string>();

    public raiting = ko.observable<number>();

    public tags = ko.observableArray<string>();

    public test = ko.observable<string>("test");

    public progress = ko.observable<number>(-1);

    public previewProgress = ko.observable<number>(-1);

    public properties = ko.computed(() => {
        var length = this.images().length;
        if (length > 0) {
            var properties = new Array();

            for (var p in this.images()[0].Properties) {
                properties.push({ title: p, value: this.images()[0].Properties[p] });
            }

            for (var i = 0; i < length; i++) {
                var imageProps = this.images()[i].Properties;
                properties = properties.filter(x => imageProps[x.title] == x.value);
            }
            
            return properties;
        }

        return new Array(0);
    });

    constructor() {
        setTimeout(() => {
            this.load();
            this.treeProviders(LightDesk.resolve("NavigationService").GetTreeProviders());
            this.activeTreeProvider(LightDesk.resolve("NavigationService").GetActiveTreeProvider());
        }, 100);
    }

    public open() {
        var path = LightDesk.resolve("WindowService").SelectFolder();
        if (path) {
            LightDesk.resolve("NavigationService").SetPath(path);
            this.load();
        }
    }

    public setTreeProvider(provider) {
        if (provider) {
            this.activeTreeProvider(provider);
            LightDesk.resolve("NavigationService").SetTreeProvider(provider);
            this.load();
        }
    }

    public load() {
        this.saveExpandedFolders();
        var items = LightDesk.resolve("NavigationService").GetNavigationTree() as Array<any>;
        this.items(ko.mapping.fromJS(items));
        this.restoreExpandedFolders();
    }

    public selectItem(id) {
        var item = LightDesk.resolve("NavigationService").SetSelectedItem(id);
        this.selectedItem(ko.mapping.fromJS(item));
        this.updateImages();
    }

    public setRaiting(rating) {
        var newRaiting = (this.raiting() == rating ? rating - 1 : rating);
        LightDesk.resolve("ImageService").SetRaiting(newRaiting, x => {
            if (x == -1) {
                this.raiting(newRaiting);

                if (LightDesk.resolve("NavigationService").GetActiveTreeProvider() == "Raiting") {
                    LightDesk.resolve("NavigationService").RefreshTree();
                    this.load();
                    var item = LightDesk.resolve("NavigationService").GetSelectedItem();
                    this.selectedItem(ko.mapping.fromJS(item));
                }
            }
            this.progress(x);
        });
    }

    public setTitle(title) {
        LightDesk.resolve("ImageService").SetTitle(title, x => this.progress(x));
    }

    public setAuthor(author) {
        LightDesk.resolve("ImageService").SetAuthor(author, x => this.progress(x));
    }

    public removeTag(tag) {
        LightDesk.resolve("ImageService").RemoveTag(tag, x => {
            this.progress(x)

            if (x == -1 && LightDesk.resolve("NavigationService").GetActiveTreeProvider() == "Tags") {
                LightDesk.resolve("NavigationService").RefreshTree();
                this.load();
                var item = LightDesk.resolve("NavigationService").GetSelectedItem();
                this.selectedItem(ko.mapping.fromJS(item));
            }
        });
    }

    public addTag(tag) {
        LightDesk.resolve("ImageService").AddTag(tag, x=> {
            this.progress(x)

            if (x == -1 && LightDesk.resolve("NavigationService").GetActiveTreeProvider() == "Tags") {
                LightDesk.resolve("NavigationService").RefreshTree();
                this.load();
                var item = LightDesk.resolve("NavigationService").GetSelectedItem();
                this.selectedItem(ko.mapping.fromJS(item));

            }
        });
    }

    public deleteFiles() {
        LightDesk.resolve("ImageService").DeleteFiles(x => {
            this.progress(x)

            if (x == -1) {
                LightDesk.resolve("NavigationService").RefreshTree();
                this.load();
                this.selectedItem(null);
                this.images.removeAll();
            }
        });
    }

    public next() {
        var item = LightDesk.resolve("NavigationService").Forward();
        this.updateImages();
        this.selectedItem(ko.mapping.fromJS(item));
    }

    public previous() {
        var item = LightDesk.resolve("NavigationService").Back();
        this.updateImages();
        this.selectedItem(ko.mapping.fromJS(item));
    }

    public fullScreen() {
        LightDesk.resolve("WindowService").ToggleFullScreen();
    }

    public exit() {
        LightDesk.resolve("WindowService").Close();
    }

    private saveExpandedFolders() {
        if (this.items()) {
            this.expandedFolders = new Array<string>();
            this.forItems(this.items().Children, x => { if (x.Expanded()) this.expandedFolders.push(x.Id()); });
        }
    }

    private restoreExpandedFolders() {
        if (this.expandedFolders && this.items()) {
            this.forItems(this.items().Children, x => x.Expanded(this.expandedFolders.indexOf(x.Id()) != -1));
        }
    }

    private forItems(items, func) {
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
    }

    private updateImages() {
        this.images.removeAll();
        this.title("");
        this.author("");
        this.raiting(0);
        this.tags.removeAll();

        var images = LightDesk.resolve("ImageService").GetSelectedImages(
            x => {
                this.previewProgress(x);
                if (x == -1) {
                    var length = this.images().length;
                    if (length > 0) {
                        var title = this.images()[0].Title;
                        this.title(this.images().every(x=> x.Title == title) ? title : "");

                        var author = this.images()[0].Author;
                        this.author(this.images().every(x=> x.Author == author) ? author : "");

                        var raiting = this.images()[0].Raiting;
                        this.raiting(this.images().every(x=> x.Raiting == raiting) ? raiting : "");

                        var tags = this.images()[0].Tags;
                        for (var i = 0; i < length; i++) {
                            tags = tags.filter(x => this.images()[i].Tags.some(t => t == x));
                        }

                        this.tags(tags);
                    }
                }
            },
            image => {
                this.images.push(image);
            });
    }
}