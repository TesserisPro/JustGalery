document.activeSplitter = null;

document.addEventListener("mouseup", function (e) {
    document.activeSplitter = null;
}, true);

document.addEventListener("mousemove", function (e) {
    if (document.activeSplitter) {
        var prev = $(document.activeSplitter).prev();
        var next = $(document.activeSplitter).next();
        var splitt = $(document.activeSplitter);
        var shift = document.activeSplitter.shift;

        if ($(document.activeSplitter).hasClass("splitter-left")) {
            prev.css("width", (e.clientX + shift) + "px");
            next.css("left", (e.clientX + shift + splitt.width()) + "px");

            splitt.css("left", prev.width() + "px");
        }
        if ($(document.activeSplitter).hasClass("splitter-right")) {
            next.css("width", (-e.clientX + shift) + "px");
            prev.css("right", (-e.clientX + shift + splitt.width()) + "px");
            
            splitt.css("right", next.width() + "px");
        }
    }
}, true);

document.addEventListener("mousedown", function (e) {
    if ($(e.target).hasClass("splitter")) {
        document.activeSplitter = e.target;
        if ($(document.activeSplitter).hasClass("splitter-left")) {
            document.activeSplitter.shift = $(document.activeSplitter).prev().width() - e.clientX;
        }
        if ($(document.activeSplitter).hasClass("splitter-right")) {
            document.activeSplitter.shift = $(document.activeSplitter).next().width() + e.clientX;
        }
    }
}, true);

document.addEventListener("DOMContentLoaded", function (event) {
    splitters = $(".splitter");
    for (i = 0; i < splitters.length; i++) {
        s = $(splitters[i]);
        if (s.hasClass("splitter-left")) {
            s.css("left", s.prev().width() + "px");
        }
        if (s.hasClass("splitter-right")) {
            s.css("right", s.next().width() + "px");
        }
    }
});


