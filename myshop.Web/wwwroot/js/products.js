$(document).ready(function () {

    $("#mytable").DataTable({
        ajax: {
            url: "/Product/GetData",
            type: "GET",
            dataSrc: "data"
        },
        columns: [
            { data: "name" },
            { data: "description" },
            { data: "price" },
            { data: "categoryName" },
            {
                data: "id",
                render: function (id) {
                    return `
                        <a href="/Product/Edit/${id}" class="btn btn-success btn-sm">
                            <i class="fa-solid fa-pen"></i>
                        </a>

                        <button class="btn btn-danger btn-sm delete-btn" data-id="${id}">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    `;
                }
            }
        ],
        autoWidth: false,
        scrollX: true
    });

});



$(document).on("click", ".delete-btn", function () {
    var productId = $(this).data("id");

    if (confirm("Are you sure you want to delete this product?")) {
        $.ajax({
            url: "/Product/Delete/" + productId,
            type: "DELETE",
            success: function (response) {
                if (response.success) {
                    $("#mytable").DataTable().ajax.reload();
                    toastr.error(response.message);
                } else {
                    toastr.error(response.message);
                }
            },
            error: function () {
                toastr.error("An error occurred while deleting.");
            }
        });
    }
});