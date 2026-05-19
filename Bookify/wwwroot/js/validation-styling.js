// Override jQuery Validate defaults to use Bootstrap 5 styling
// Exposes applyBootstrapValidation() for use on dynamically loaded forms

function applyBootstrapValidation(forms) {
    $(forms).each(function () {
        var validator = $.data(this, "validator");
        if (!validator) return;

        // Override highlight: add is-invalid, show validation message
        validator.settings.highlight = function (element) {
            $(element).addClass("is-invalid").removeClass("is-valid");
        };

        // Override unhighlight: remove is-invalid, hide validation message
        validator.settings.unhighlight = function (element) {
            $(element).removeClass("is-invalid").addClass("is-valid");
        };

        // Override errorClass so jQuery Validate doesn't add its own class
        validator.settings.errorClass = "text-danger";

        // Validate on every keyup (not just after first submit)
        validator.settings.onkeyup = function (element) {
            $(element).valid();
        };

        // Validate when the field loses focus
        validator.settings.onfocusout = function (element) {
            $(element).valid();
        };
    });
}

// Apply to any forms already on the page at load time
$(function () {
    applyBootstrapValidation("form");
});