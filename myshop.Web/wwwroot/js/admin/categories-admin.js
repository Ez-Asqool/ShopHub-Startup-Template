(function () {
    "use strict";

    var BASE = "/Admin/Category";
    var HUES = [118, 285, 200, 40, 340, 160];
    var MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    var state = {
        query: "",
        sort: "newest",
        editingId: null,
        deleteId: null,
        deleteName: ""
    };

    var els = {
        searchInput: document.getElementById("searchInput"),
        clearSearchBtn: document.getElementById("clearSearchBtn"),
        sortSelect: document.getElementById("sortSelect"),
        openAddBtn: document.getElementById("openAddBtn"),
        emptyAddBtn: document.getElementById("emptyAddBtn"),
        resultLabel: document.getElementById("resultLabel"),
        categoryRows: document.getElementById("categoryRows"),
        emptyState: document.getElementById("emptyState"),
        footLabel: document.getElementById("footLabel"),

        statTotal: document.getElementById("statTotal"),
        statTotalNote: document.getElementById("statTotalNote"),
        statNewest: document.getElementById("statNewest"),
        statNewestNote: document.getElementById("statNewestNote"),
        statAvg: document.getElementById("statAvg"),
        statEmpty: document.getElementById("statEmpty"),
        statEmptyNote: document.getElementById("statEmptyNote"),

        categoryModalOverlay: document.getElementById("categoryModalOverlay"),
        categoryForm: document.getElementById("categoryForm"),
        categoryId: document.getElementById("categoryId"),
        formKicker: document.getElementById("formKicker"),
        formTitle: document.getElementById("formTitle"),
        closeFormBtn: document.getElementById("closeFormBtn"),
        cancelFormBtn: document.getElementById("cancelFormBtn"),
        submitFormBtn: document.getElementById("submitFormBtn"),
        formError: document.getElementById("formError"),
        fieldName: document.getElementById("fieldName"),
        fieldDescription: document.getElementById("fieldDescription"),

        deleteModalOverlay: document.getElementById("deleteModalOverlay"),
        deleteSub: document.getElementById("deleteSub"),
        cancelDeleteBtn: document.getElementById("cancelDeleteBtn"),
        confirmDeleteBtn: document.getElementById("confirmDeleteBtn"),

        toast: document.getElementById("shToast")
    };

    function escapeHtml(str) {
        var div = document.createElement("div");
        div.textContent = str == null ? "" : String(str);
        return div.innerHTML;
    }

    function slugify(s) {
        return s.trim().toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
    }

    function fmtDate(iso) {
        var d = new Date(iso);
        if (isNaN(d)) return iso;
        return MONTHS[d.getMonth()] + " " + d.getDate() + ", " + d.getFullYear();
    }

    function ago(iso) {
        var d = new Date(iso);
        if (isNaN(d)) return "";
        var days = Math.round((Date.now() - d.getTime()) / 86400000);
        if (days < 1) return "today";
        if (days < 30) return days + "d ago";
        var months = Math.round(days / 30);
        if (months < 12) return months + "mo ago";
        return (months / 12).toFixed(1) + "y ago";
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
        if (state.sort) params.set("sort", state.sort);
        return params.toString();
    }

    function loadList() {
        fetch(BASE + "/GetList?" + buildQuery())
            .then(function (r) { return r.json(); })
            .then(renderList)
            .catch(function () { showToast("Failed to load categories", true); });
    }

    function renderList(data) {
        var stats = data.stats || {};
        els.statTotal.textContent = stats.totalCategories != null ? stats.totalCategories : "0";
        els.statTotalNote.textContent = (stats.totalCategories || 0) === 1 ? "1 category" : (stats.totalCategories || 0) + " categories";

        els.statNewest.textContent = stats.newestName || "—";
        els.statNewestNote.textContent = stats.newestCreatedTime ? "Created " + fmtDate(stats.newestCreatedTime) : "No categories yet";

        els.statAvg.textContent = (stats.avgProductsPerCategory != null ? stats.avgProductsPerCategory : 0).toFixed(1);

        els.statEmpty.textContent = stats.emptyCategoriesCount != null ? stats.emptyCategoriesCount : "0";
        els.statEmptyNote.textContent = (stats.emptyCategoriesCount || 0) ? "Needs products" : "All categories in use";

        var items = data.items || [];
        els.resultLabel.textContent = items.length + (items.length === 1 ? " category" : " categories");
        els.footLabel.textContent = "Showing " + items.length + " of " + (stats.totalCategories || 0) + " categories";
        els.emptyState.hidden = items.length !== 0;

        els.categoryRows.innerHTML = "";
        items.forEach(function (c, i) {
            var hue = HUES[i % HUES.length];
            var row = document.createElement("div");
            row.className = "sh-table-row sh-table-cols-cat";

            var slug = slugify(c.name);
            var meta = "/" + slug + " · " + c.productCount + (c.productCount === 1 ? " product" : " products");

            row.innerHTML =
                '<div class="sh-cat-name-cell">' +
                '<span class="sh-cat-dot" style="background: oklch(0.93 0.05 ' + hue + '); color: oklch(0.42 0.12 ' + hue + ');">' + escapeHtml((c.name || "?").charAt(0).toUpperCase()) + '</span>' +
                '<span style="min-width:0;">' +
                '<span style="display:block; font-size:14.5px; font-weight:600; letter-spacing:-0.01em; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;">' + escapeHtml(c.name) + '</span>' +
                '<span style="display:block; font-family:\'IBM Plex Mono\', monospace; font-size:10px; color: oklch(0.55 0.02 280); margin-top:4px;">' + escapeHtml(meta) + '</span>' +
                '</span></div>' +
                '<div class="sh-cell-desc">' + escapeHtml(c.description) + '</div>' +
                '<div class="sh-cat-date"><div class="sh-cat-date-label">' + fmtDate(c.createdTime) + '</div><div class="sh-cat-date-ago">' + ago(c.createdTime) + '</div></div>' +
                '<div class="sh-cell-actions">' +
                '<button type="button" class="sh-action-btn" data-edit="' + c.id + '">Edit</button>' +
                '<button type="button" class="sh-action-btn sh-action-btn-danger" data-delete="' + c.id + '" data-name="' + escapeHtml(c.name) + '" data-count="' + c.productCount + '">Delete</button>' +
                '</div>';

            els.categoryRows.appendChild(row);
        });
    }

    var searchTimer;
    els.searchInput.addEventListener("input", function () {
        var value = els.searchInput.value;
        els.clearSearchBtn.hidden = value.length === 0;
        clearTimeout(searchTimer);
        searchTimer = setTimeout(function () {
            state.query = value.trim();
            loadList();
        }, 300);
    });

    els.clearSearchBtn.addEventListener("click", function () {
        els.searchInput.value = "";
        els.clearSearchBtn.hidden = true;
        state.query = "";
        loadList();
    });

    els.sortSelect.addEventListener("change", function () {
        state.sort = els.sortSelect.value;
        loadList();
    });

    els.categoryRows.addEventListener("click", function (e) {
        var editBtn = e.target.closest("[data-edit]");
        if (editBtn) { openEditModal(parseInt(editBtn.getAttribute("data-edit"), 10)); return; }

        var delBtn = e.target.closest("[data-delete]");
        if (delBtn) {
            openDeleteModal(
                parseInt(delBtn.getAttribute("data-delete"), 10),
                delBtn.getAttribute("data-name"),
                parseInt(delBtn.getAttribute("data-count"), 10)
            );
        }
    });

    function resetForm() {
        els.categoryForm.reset();
        els.categoryId.value = "";
        els.formError.hidden = true;
        els.formError.textContent = "";
    }

    function openAddModal() {
        resetForm();
        state.editingId = null;
        els.formKicker.textContent = "New category";
        els.formTitle.textContent = "Add a category";
        els.submitFormBtn.textContent = "Create category";
        els.categoryModalOverlay.hidden = false;
    }

    function openEditModal(id) {
        fetch(BASE + "/GetForEdit/" + id)
            .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
            .then(function (c) {
                resetForm();
                state.editingId = id;
                els.formKicker.textContent = "Editing /" + slugify(c.name || "");
                els.formTitle.textContent = "Edit category";
                els.submitFormBtn.textContent = "Save changes";
                els.categoryId.value = id;
                els.fieldName.value = c.name || "";
                els.fieldDescription.value = c.description || "";
                els.categoryModalOverlay.hidden = false;
            })
            .catch(function () { showToast("Could not load category", true); });
    }

    function closeFormModal() { els.categoryModalOverlay.hidden = true; }

    els.openAddBtn.addEventListener("click", openAddModal);
    els.emptyAddBtn.addEventListener("click", openAddModal);
    els.closeFormBtn.addEventListener("click", closeFormModal);
    els.cancelFormBtn.addEventListener("click", closeFormModal);
    els.categoryModalOverlay.addEventListener("click", function (e) {
        if (e.target === els.categoryModalOverlay) closeFormModal();
    });

    function validateForm() {
        var name = els.fieldName.value.trim();
        var description = els.fieldDescription.value.trim();
        if (name.length < 2) return "Enter a category name";
        if (description.length < 10) return "Add a description of at least 10 characters";
        return null;
    }

    els.categoryForm.addEventListener("submit", function (e) {
        e.preventDefault();
        var error = validateForm();
        if (error) {
            els.formError.textContent = error;
            els.formError.hidden = false;
            return;
        }
        els.formError.hidden = true;

        var formData = new FormData();
        formData.append("Name", els.fieldName.value.trim());
        formData.append("Description", els.fieldDescription.value.trim());

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

    function openDeleteModal(id, name, productCount) {
        state.deleteId = id;
        state.deleteName = name;
        var sub = "“" + name + "” will be permanently deleted.";
        if (productCount > 0) {
            sub = "“" + name + "” and its " + productCount + (productCount === 1 ? " product" : " products") +
                " will be permanently deleted. This can't be undone.";
        } else {
            sub += " This can't be undone.";
        }
        els.deleteSub.textContent = sub;
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
                    showToast(res.message || "Could not delete category", true);
                }
            })
            .catch(function () {
                closeDeleteModal();
                showToast("A network error occurred.", true);
            });
    });

    loadList();
})();
