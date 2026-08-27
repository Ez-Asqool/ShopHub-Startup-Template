(function () {
    "use strict";

    var BASE = "/Admin/Order";

    var state = {
        query: "",
        status: "",
        page: 1,
        pageSize: 6
    };

    var els = {
        searchInput: document.getElementById("searchInput"),
        clearSearchBtn: document.getElementById("clearSearchBtn"),
        pageSizeSelect: document.getElementById("pageSizeSelect"),
        resultLabel: document.getElementById("resultLabel"),
        statusChips: document.getElementById("statusChips"),
        orderRows: document.getElementById("orderRows"),
        emptyState: document.getElementById("emptyState"),
        footLabel: document.getElementById("footLabel"),
        prevPageBtn: document.getElementById("prevPageBtn"),
        nextPageBtn: document.getElementById("nextPageBtn"),
        pageNumbers: document.getElementById("pageNumbers"),

        statTotalOrders: document.getElementById("statTotalOrders"),
        statRevenue: document.getElementById("statRevenue"),
        statProcessing: document.getElementById("statProcessing"),
        statDelivered: document.getElementById("statDelivered"),

        detailsModalOverlay: document.getElementById("orderDetailsModalOverlay"),
        detailsKicker: document.getElementById("detailsKicker"),
        detailsTitle: document.getElementById("detailsTitle"),
        detailsBody: document.getElementById("detailsBody"),
        closeDetailsBtn: document.getElementById("closeDetailsBtn"),

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

    function statusPillClass(status) {
        return "sh-pill-order-" + status.toLowerCase();
    }

    function paymentPillClass(status) {
        return "sh-pill-payment-" + status.toLowerCase();
    }

    function buildQuery() {
        var params = new URLSearchParams();
        if (state.query) params.set("search", state.query);
        if (state.status) params.set("status", state.status);
        params.set("page", state.page);
        params.set("pageSize", state.pageSize);
        return params.toString();
    }

    function loadList() {
        fetch(BASE + "/GetList?" + buildQuery())
            .then(function (r) { return r.json(); })
            .then(renderList)
            .catch(function () { showToast("Failed to load orders", true); });
    }

    function renderList(data) {
        var stats = data.stats || {};
        els.statTotalOrders.textContent = stats.totalOrders != null ? stats.totalOrders : "0";
        els.statRevenue.textContent = money(stats.totalRevenue || 0);
        els.statProcessing.textContent = stats.processingCount != null ? stats.processingCount : "0";
        els.statDelivered.textContent = stats.deliveredCount != null ? stats.deliveredCount : "0";

        var items = data.items || [];
        var totalCount = data.totalCount || 0;
        var pageSize = data.pageSize || state.pageSize;
        var page = data.page || state.page;
        var pageCount = Math.max(1, Math.ceil(totalCount / pageSize));

        var statusLabel = state.status ? " · " + state.status : "";
        els.resultLabel.textContent = totalCount + (totalCount === 1 ? " order" : " orders") + statusLabel;

        els.orderRows.innerHTML = "";
        els.emptyState.hidden = items.length !== 0;

        items.forEach(function (o) {
            var row = document.createElement("div");
            row.className = "sh-table-row sh-table-cols-orders";

            row.innerHTML =
                '<div class="sh-product-id">SH-' + String(o.id).padStart(5, "0") + '</div>' +
                '<div style="min-width:0; font-size:13.5px; font-weight:600; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">' + escapeHtml(o.recipientName) + '</div>' +
                '<div style="min-width:0; font-size:13px; color: oklch(0.5 0.02 280); overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">' + escapeHtml(o.city) + '</div>' +
                '<div class="sh-col-center">' + o.itemCount + '</div>' +
                '<div class="sh-col-center sh-cell-price">' + money(o.totalPrice) + '</div>' +
                '<div class="sh-col-center"><span class="sh-pill ' + statusPillClass(o.orderStatus) + '">' + o.orderStatus + '</span></div>' +
                '<div class="sh-col-center"><span class="sh-pill ' + paymentPillClass(o.paymentStatus) + '">' + o.paymentStatus + '</span></div>' +
                '<div class="sh-cell-actions"><button type="button" class="sh-action-btn" data-view="' + o.id + '">View</button></div>';

            els.orderRows.appendChild(row);
        });

        els.footLabel.textContent = totalCount
            ? "Showing " + ((page - 1) * pageSize + 1) + "–" + Math.min(page * pageSize, totalCount) + " of " + totalCount + " orders"
            : "No orders to show";

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

    els.statusChips.addEventListener("click", function (e) {
        var chip = e.target.closest(".sh-chip");
        if (!chip) return;
        els.statusChips.querySelectorAll(".sh-chip").forEach(function (c) { c.classList.remove("active"); });
        chip.classList.add("active");
        state.status = chip.getAttribute("data-status") || "";
        state.page = 1;
        loadList();
    });

    els.prevPageBtn.addEventListener("click", function () {
        if (state.page > 1) { state.page -= 1; loadList(); }
    });
    els.nextPageBtn.addEventListener("click", function () {
        state.page += 1; loadList();
    });

    function openDetailsModal(id) {
        fetch(BASE + "/GetDetails/" + id)
            .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
            .then(function (o) {
                els.detailsKicker.textContent = "Order SH-" + String(o.id).padStart(5, "0");
                els.detailsTitle.textContent = escapeHtml(o.recipientName) + "'s order";

                var itemsHtml = (o.items || []).map(function (item) {
                    var thumb = item.productImg
                        ? '<img class="sh-order-item-thumb" src="/' + escapeHtml(item.productImg) + '" alt="" />'
                        : '<div class="sh-order-item-thumb"></div>';
                    return '<div class="sh-order-item-row">' +
                        thumb +
                        '<div class="sh-order-item-name">' + escapeHtml(item.productName) + '</div>' +
                        '<div class="sh-order-item-qty">&times;' + item.quantity + '</div>' +
                        '<div class="sh-order-item-total">' + money(item.unitPrice * item.quantity) + '</div>' +
                        '</div>';
                }).join("");

                els.detailsBody.innerHTML =
                    '<div class="sh-order-detail-grid">' +
                    '<div class="sh-order-detail-item"><span class="sh-order-detail-label">Date</span><span class="sh-order-detail-value">' + new Date(o.orderDate).toLocaleString() + '</span></div>' +
                    '<div class="sh-order-detail-item"><span class="sh-order-detail-label">Status</span><span><span class="sh-pill ' + statusPillClass(o.orderStatus) + '">' + o.orderStatus + '</span></span></div>' +
                    '<div class="sh-order-detail-item"><span class="sh-order-detail-label">Recipient</span><span class="sh-order-detail-value">' + escapeHtml(o.recipientName) + '</span></div>' +
                    '<div class="sh-order-detail-item"><span class="sh-order-detail-label">Payment</span><span><span class="sh-pill ' + paymentPillClass(o.paymentStatus) + '">' + o.paymentStatus + '</span></span></div>' +
                    '<div class="sh-order-detail-item"><span class="sh-order-detail-label">Address</span><span class="sh-order-detail-value">' + escapeHtml(o.address) + '</span></div>' +
                    '<div class="sh-order-detail-item"><span class="sh-order-detail-label">City</span><span class="sh-order-detail-value">' + escapeHtml(o.city) + '</span></div>' +
                    (o.phoneNumber ? '<div class="sh-order-detail-item"><span class="sh-order-detail-label">Phone</span><span class="sh-order-detail-value">' + escapeHtml(o.phoneNumber) + '</span></div>' : '') +
                    '</div>' +
                    '<div class="sh-order-items-wrap">' + itemsHtml + '</div>' +
                    '<div class="sh-order-total-row"><span class="sh-order-total-label">Order total</span><span class="sh-order-total-amount">' + money(o.totalPrice) + '</span></div>';

                els.detailsModalOverlay.hidden = false;
            })
            .catch(function () { showToast("Could not load order details", true); });
    }

    els.orderRows.addEventListener("click", function (e) {
        var viewBtn = e.target.closest("[data-view]");
        if (viewBtn) openDetailsModal(parseInt(viewBtn.getAttribute("data-view"), 10));
    });

    function closeDetailsModal() { els.detailsModalOverlay.hidden = true; }
    els.closeDetailsBtn.addEventListener("click", closeDetailsModal);
    els.detailsModalOverlay.addEventListener("click", function (e) {
        if (e.target === els.detailsModalOverlay) closeDetailsModal();
    });

    loadList();
})();
