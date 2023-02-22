$(function () {

    /* Потверждение удаления страницы */

    $("a.delete").click(function () {
        if (!confirm("Вы действительно хотите удалить страницу")) return false;
    });

    $("table#pages tbody").sortable({
        items: "tr:not(.home)",
        placeholder: "ui-state-highlight",
        update: function () {
            var ids = $("table#pages tbody").sortable("serialize");
            var url = "/Admin/Pages/ReorderPages";

            $.post(url, ids, function (data) {

            });
        }
    });

});