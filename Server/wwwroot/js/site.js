// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    $('form').submit(function () {
        $(':submit', this).prop('disabled', true);
    });

    $('form :input').on('invalid', function () {
        $(':submit', $(this).parents('form')).prop('disabled', false);
    });
});