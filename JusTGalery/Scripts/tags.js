ko.bindingHandlers.tags = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here
        var binding = ko.unwrap(valueAccessor());
        var remove = ko.unwrap(binding.remove);
        var tags = binding.items;
        var add = ko.unwrap(binding.add);
        $(element).css("white-space", "nowrap");
        $(element).html("<div class='tag-container'></div><input class='tag-input' placeholder='...' />");
        $(element).find(".tag-input").keydown(function (e) {
            if (e.keyCode == 13 && e.currentTarget.value) {
                tags.push(e.currentTarget.value);
                if (add) add(e.currentTarget.value);
                e.currentTarget.value = "";
            }
            if (e.keyCode == 8 && !e.currentTarget.value) {
                var t = tags.pop();
                if (remove) remove(t);
            }
        });
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        // This will be called once when the binding is first applied to an element,
        // and again whenever any observables/computeds that are accessed change
        // Update the DOM element based on the supplied values here.
        var html = "";
        var binding = ko.unwrap(valueAccessor());
        var tags = binding.items;
        var remove = ko.unwrap(binding.remove);
        for (var i = 0; i < tags().length; i++) {
            var tag = tags()[i];
            html += "<div class='label label-tag'><span class='fa fa-tag'></span>" + tag + "<span class='fa fa-remove' data-value='" + tag +"'></span></div>";
        }

        $(element).find(".tag-container").html(html);
        $(element).find(".fa-remove").click(function (e) {
            var tag = e.currentTarget.attributes["data-value"].value;
            tags.remove(tag);
            if (remove) remove(tag);
        });
    }
};
