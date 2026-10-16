/* Custom validation for greaterthan */
$.validator.addMethod("greaterthan", function (value, element, param) {
    var target = $(param);
    if (this.settings.onfocusout) {
        target.off(".validate-greaterthan").on("blur.validate-greaterthan", function () {
            $(element).valid();
        });
    }
    var targetValue = target.val();
    if (value === "" || targetValue === "") {
        return true; // let required or other validators handle empty values
    }
    return parseFloat(value) > parseFloat(targetValue);
}, "");

$.validator.unobtrusive.adapters.add("greaterthan", ["other"], function (options) {
    options.rules["greaterthan"] = "#" + options.params.other;
    options.messages["greaterthan"] = options.message;
});