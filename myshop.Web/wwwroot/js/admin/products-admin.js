(function () {
    "use strict";

    var BASE = "/Admin/Product";
    var ALLOWED_IMAGE_EXT = [".jpg", ".jpeg", ".png", ".webp"];
    var MAX_IMAGE_BYTES = 2 * 1024 * 1024;

    var state = {
        query: "",
        categoryId: "",
        page: 1,
        pageSize: 5,
        editingId: null,
        deleteId: null,
        deleteName: ""
    };

    var els = {
        searchInput: document.getElementById("searchInput"),
        clearSearchBtn: document.getElementById("clearSearchBtn"),
        pageSizeSelect: document.getElementById("pageSizeSelect"),
        openAddBtn: document.getElementById("openAddBtn"),
        emptyAddBtn: document.getElementById("emptyAddBtn"),
        resultLabel: document.getElementById("resultLabel"),
        categoryChips: document.getElementById("categoryChips"),
        productRows: document.getElementById("productRows"),
        emptyState: document.getElementById("emptyState"),
        footLabel: document.getElementById("footLabel"),
        prevPageBtn: document.getElementById("prevPageBtn"),
        nextPageBtn: document.getElementById("nextPageBtn"),
        pageNumbers: document.getElementById("pageNumbers"),

        statTotalProducts: document.getElementById("statTotalProducts"),
        statTotalProductsNote: document.getElementById("statTotalProductsNote"),
        statCatalogValue: document.getElementById("statCatalogValue"),
        statAvgPrice: document.getElementById("statAvgPrice"),
        statCategories: document.getElementById("statCategories"),

        productModalOverlay: document.getElementById("productModalOverlay"),
        productForm: document.getElementById("productForm"),
        productId: document.getElementById("productId"),
        existingImg: document.getElementById("existingImg"),
        formKicker: document.getElementById("formKicker"),
        formTitle: document.getElementById("formTitle"),
        closeFormBtn: document.getElementById("closeFormBtn"),
        cancelFormBtn: document.getElementById("cancelFormBtn"),
        submitFormBtn: document.getElementById("submitFormBtn"),
        formError: document.getElementById("formError"),
        imageUploadBox: document.getElementById("imageUploadBox"),
        imageInput: document.getElementById("imageInput"),
        imagePreview: document.getElementById("imagePreview"),
        imageUploadHint: document.getElementById("imageUploadHint"),
        fieldName: document.getElementById("fieldName"),
        fieldDescription: document.getElementById("fieldDescription"),
        fieldPrice: document.getElementById("fieldPrice"),
        fieldCategory: document.getElementById("fieldCategory"),

        deleteModalOverlay: document.getElementById("deleteModalOverlay"),
        deleteSub: document.getElementById("deleteSub"),
        cancelDeleteBtn: document.getElementById("cancelDeleteBtn"),
        confirmDeleteBtn: document.getElementById("confirmDeleteBtn"),

        toast: document.getElementById("shToast")
    };

    function money(n) {
        return "$" + Number(n).toFixed(2);
    }

    function escapeHtml(str) {
        var div = document.createElement("div");
        div.textContent = str == null ? "" : String(str);
        return div.innerHTML;
    }

    function showToast(message, isError) {
        var toast = els.toast;
        toast.textContent = message;
        toast.classList.toggle("sh-toast-error", !!isError);
        toast.hidden = false;
        requestAnimationFrame(function () { toast.classList.add("show"); });
        clearTimeout(showToast._t);
        showToast._t = setTimeout(function () {
            toast.classList.remove("show");
            setTimeout(function () { toast.hidden = true; }, 240);
        }, 1900);
    }

    function buildQuery() {
        var params = new URLSearchParams();
        if (state.query) params.set("search", state.query);
        if (state.categoryId) params.set("categoryId", state.categoryId);
        params.set("page", state.page);
        params.set("pageSize", state.pageSize);
        return params.toString();
    }

    function loadList() {
        fetch(BASE + "/GetList?" + buildQuery())
            .then(function (r) { return r.json(); })
            .then(renderList)
            .catch(function () { showToast("Failed to load products", true); });
    }

    function renderList(data) {
        var stats = data.stats || {};
        els.statTotalProducts.textContent = stats.totalProducts != null ? stats.totalProducts : "0";
        els.statTotalProductsNote.textContent = (stats.categoriesCount || 0) + " categories";
        els.statCatalogValue.textContent = money(stats.catalogValue || 0);
        els.statAvgPrice.textContent = money(stats.avgPrice || 0);
        els.statCategories.textContent = stats.categoriesCount != null ? stats.categoriesCount : "0";

        var items = data.items || [];
        var totalCount = data.totalCount || 0;
        var pageSize = data.pageSize || state.pageSize;
        var page = data.page || state.page;
        var pageCount = Math.max(1, Math.ceil(totalCount / pageSize));

        var categoryLabel = state.categoryId
            ? (els.categoryChips.querySelector('[data-category-id="' + state.categoryId + '"]') || {}).textContent
            : "";
        els.resultLabel.textContent = totalCount + (totalCount === 1 ? " product" : " products") + (categoryLabel ? " in " + categoryLabel : "");

        els.productRows.innerHTML = "";
        els.emptyState.hidden = items.length !== 0;

        items.forEach(function (p) {
            var row = document.createElement("div");
            row.className = "sh-table-row sh-table-cols";

            var thumbHtml = p.img
                ? '<div class="sh-thumb"><img src="/' + escapeHtml(p.img) + '" alt="" /></div>'
                : '<div class="sh-thumb sh-thumb-empty"></div>';

            row.innerHTML =
                thumbHtml +
                '<div><div class="sh-product-name">' + escapeHtml(p.name) + '</div><div class="sh-product-id">#' + p.id + '</div></div>' +
                '<div class="sh-cell-desc">' + escapeHtml(p.description) + '</div>' +
                '<div class="sh-cell-price">' + money(p.price) + '</div>' +
                '<div class="sh-col-center"><span class="sh-category-badge">' + escapeHtml(p.categoryName) + '</span></div>' +
                '<div class="sh-cell-actions">' +
                '<button type="button" class="sh-action-btn" data-edit="' + p.id + '">Edit</button>' +
                '<button type="button" class="sh-action-btn sh-action-btn-danger" data-delete="' + p.id + '" data-name="' + escapeHtml(p.name) + '">Delete</button>' +
                '</div>';

            els.productRows.appendChild(row);
        });

        els.footLabel.textContent = totalCount
            ? "Showing " + ((page - 1) * pageSize + 1) + "–" + Math.min(page * pageSize, totalCount) + " of " + totalCount + " products"
            : "No products to show";

        els.prevPageBtn.disabled = page <= 1;
        els.nextPageBtn.disabled = page >= pageCount;

        els.pageNumbers.innerHTML = "";
        for (var i = 1; i <= pageCount; i++) {
            var btn = document.createElement("button");
            btn.type = "button";
            btn.className = "sh-page-btn" + (i === page ? " active" : "");
            btn.textContent = i;
            btn.addEventListener("click", (function (n) {
                return function () { state.page = n; loadList(); };
            })(i));
            els.pageNumbers.appendChild(btn);
        }

        state.page = page;
    }

    var searchTimer;
    els.searchInput.addEventListener("input", function () {
        var value = els.searchInput.value;
        els.clearSearchBtn.hidden = value.length === 0;
        clearTimeout(searchTimer);
        searchTimer = setTimeout(function () {
            state.query = value.trim();
            state.page = 1;
            loadList();
        }, 300);
    });

    els.clearSearchBtn.addEventListener("click", function () {
        els.searchInput.value = "";
        els.clearSearchBtn.hidden = true;
        state.query = "";
        state.page = 1;
        loadList();
    });

    els.pageSizeSelect.addEventListener("change", function () {
        state.pageSize = parseInt(els.pageSizeSelect.value, 10);
        state.page = 1;
        loadList();
    });

    els.categoryChips.addEventListener("click", function (e) {
        var chip = e.target.closest(".sh-chip");
        if (!chip) return;
        els.categoryChips.querySelectorAll(".sh-chip").forEach(function (c) { c.classList.remove("active"); });
        chip.classList.add("active");
        state.categoryId = chip.getAttribute("data-category-id") || "";
        state.page = 1;
        loadList();
    });

    els.prevPageBtn.addEventListener("click", function () {
        if (state.page > 1) { state.page -= 1; loadList(); }
    });
    els.nextPageBtn.addEventListener("click", function () {
        state.page += 1; loadList();
    });

    els.productRows.addEventListener("click", function (e) {
        var editBtn = e.target.closest("[data-edit]");
        if (editBtn) { openEditModal(parseInt(editBtn.getAttribute("data-edit"), 10)); return; }

        var delBtn = e.target.closest("[data-delete]");
        if (delBtn) { openDeleteModal(parseInt(delBtn.getAttribute("data-delete"), 10), delBtn.getAttribute("data-name")); }
    });

    function resetForm() {
        els.productForm.reset();
        els.productId.value = "";
        els.existingImg.value = "";
        els.imagePreview.hidden = true;
        els.imagePreview.src = "";
        els.imageUploadHint.hidden = false;
        els.formError.hidden = true;
        els.formError.textContent = "";
    }

    function openAddModal() {
        resetForm();
        state.editingId = null;
        els.formKicker.textContent = "New product";
        els.formTitle.textContent = "Add a product";
        els.submitFormBtn.textContent = "Create product";
        els.productModalOverlay.hidden = false;
    }

    function openEditModal(id) {
        fetch(BASE + "/GetForEdit/" + id)
            .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
            .then(function (p) {
                resetForm();
                state.editingId = id;
                els.formKicker.textContent = "Editing #" + id;
                els.formTitle.textContent = "Edit product";
                els.submitFormBtn.textContent = "Save changes";
                els.productId.value = id;
                els.existingImg.value = p.img || "";
                els.fieldName.value = p.name || "";
                els.fieldDescription.value = p.description || "";
                els.fieldPrice.value = p.price != null ? p.price : "";
                els.fieldCategory.value = p.categoryId != null ? p.categoryId : "";
                if (p.img) {
                    els.imagePreview.src = "/" + p.img;
                    els.imagePreview.hidden = false;
                    els.imageUploadHint.hidden = true;
                }
                els.productModalOverlay.hidden = false;
            })
            .catch(function () { showToast("Could not load product", true); });
    }

    function closeFormModal() { els.productModalOverlay.hidden = true; }

    els.openAddBtn.addEventListener("click", openAddModal);
    els.emptyAddBtn.addEventListener("click", openAddModal);
    els.closeFormBtn.addEventListener("click", closeFormModal);
    els.cancelFormBtn.addEventListener("click", closeFormModal);
    els.productModalOverlay.addEventListener("click", function (e) {
        if (e.target === els.productModalOverlay) closeFormModal();
    });

    els.imageUploadBox.addEventListener("click", function () { els.imageInput.click(); });
    els.imageInput.addEventListener("change", function () {
        var file = els.imageInput.files[0];
        if (!file) return;
        els.imagePreview.src = URL.createObjectURL(file);
        els.imagePreview.hidden = false;
        els.imageUploadHint.hidden = true;
    });

    function validateForm(file) {
        var name = els.fieldName.value.trim();
        var description = els.fieldDescription.value.trim();
        var price = parseFloat(els.fieldPrice.value);
        var categoryId = els.fieldCategory.value;

        if (name.length < 2) return "Enter a product name";
        if (description.length < 10) return "Add a description of at least 10 characters";
        if (!isFinite(price) || price <= 0) return "Enter a valid price";
        if (!categoryId) return "Select a category";

        if (file) {
            var ext = "." + file.name.split(".").pop().toLowerCase();
            if (ALLOWED_IMAGE_EXT.indexOf(ext) === -1) return "Only these image types are allowed: .jpg, .jpeg, .png, .webp";
            if (file.size > MAX_IMAGE_BYTES) return "Image size must not exceed 2 MB.";
        }

        return null;
    }

    els.productForm.addEventListener("submit", function (e) {
        e.preventDefault();
        var file = els.imageInput.files[0] || null;
        var error = validateForm(file);
        if (error) {
            els.formError.textContent = error;
            els.formError.hidden = false;
            return;
        }
        els.formError.hidden = true;

        var formData = new FormData();
        formData.append("Name", els.fieldName.value.trim());
        formData.append("Description", els.fieldDescription.value.trim());
        formData.append("Price", els.fieldPrice.value);
        formData.append("CategoryId", els.fieldCategory.value);
        if (file) formData.append("file", file);

        var url = state.editingId ? BASE + "/Edit/" + state.editingId : BASE + "/Create";

        els.submitFormBtn.disabled = true;
        fetch(url, { method: "POST", body: formData })
            .then(function (r) { return r.json(); })
            .then(function (res) {
                els.submitFormBtn.disabled = false;
                if (res.success) {
                    closeFormModal();
                    showToast(res.message || "Saved");
                    loadList();
                } else {
                    els.formError.textContent = res.message || "Something went wrong.";
                    els.formError.hidden = false;
                }
            })
            .catch(function () {
                els.submitFormBtn.disabled = false;
                els.formError.textContent = "A network error occurred.";
                els.formError.hidden = false;
            });
    });

    function openDeleteModal(id, name) {
        state.deleteId = id;
        state.deleteName = name;
        els.deleteSub.textContent = "“" + name + "” will be removed from the catalog. This can't be undone.";
        els.deleteModalOverlay.hidden = false;
    }
    function closeDeleteModal() { els.deleteModalOverlay.hidden = true; }

    els.cancelDeleteBtn.addEventListener("click", closeDeleteModal);
    els.deleteModalOverlay.addEventListener("click", function (e) {
        if (e.target === els.deleteModalOverlay) closeDeleteModal();
    });

    els.confirmDeleteBtn.addEventListener("click", function () {
        if (!state.deleteId) return;
        fetch(BASE + "/Delete/" + state.deleteId, { method: "DELETE" })
            .then(function (r) { return r.json(); })
            .then(function (res) {
                closeDeleteModal();
                if (res.success) {
                    showToast("Deleted — " + state.deleteName);
                    loadList();
                } else {
                    showToast(res.message || "Could not delete product", true);
                }
            })
            .catch(function () {
                closeDeleteModal();
                showToast("A network error occurred.", true);
            });
    });

    loadList();
})();
