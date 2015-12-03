ko.bindingHandlers["local"] = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        var str = ko.unwrap(valueAccessor());
        element.textContent = str;
    }
};
