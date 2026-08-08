(function () {
    "use strict";

    var BASE = "/Admin/Users";
    var HUES = [285, 118, 200, 40, 340, 160];

    var state = {
        query: "",
        role: "All",
        status: "All",
        page: 1,
        pageSize: 6,
        deleteId: null,
        deleteName: ""
    };

    var els = {
        searchInput: document.getElementById("searchInput"),
        clearSearchBtn: document.getElementById("clearSearchBtn"),
        roleSelect: document.getElementById("roleSelect"),
        pageSizeSelect: document.getElementById("pageSizeSelect"),
        inviteBtn: document.getElementById("inviteBtn"),
        resultLabel: document.getElementById("resultLabel"),
        statusTabs: document.getElementById("statusTabs"),
        userRows: document.getElementById("userRows"),
        emptyState: document.getElementById("emptyState"),
        resetFiltersBtn: document.getElementById("resetFiltersBtn"),
        footLabel: document.getElementById("footLabel"),
        prevPageBtn: document.getElementById("prevPageBtn"),
        nextPageBtn: document.getElementById("nextPageBtn"),
        pageNumbers: document.getElementById("pageNumbers"),

        statTotal: document.getElementById("statTotal"),
        statTotalNote: document.getElementById("statTotalNote"),
        statActive: document.getElementById("statActive"),
        statLocked: document.getElementById("statLocked"),
        statLockedNote: document.getElementById("statLockedNote"),
        statAdmins: document.getElementById("statAdmins"),

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

    function initials(username) {
        return (username || "?").split(/[.\-_]/).slice(0, 2).map(function (s) { return s.charAt(0).toUpperCase(); }).join("");
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

    els.inviteBtn.addEventListener("click", function () {
        showToast("Inviting users isn't available yet — they can register from the storefront");
    });

    function buildQuery() {
        var params = new URLSearchParams();
        if (state.query) params.set("search", state.query);
        if (state.role) params.set("role", state.role);
        if (state.status) params.set("status", state.status);
        params.set("page", state.page);
        params.set("pageSize", state.pageSize);
        return params.toString();
    }

    function loadList() {
        fetch(BASE + "/GetList?" + buildQuery())
            .then(function (r) { return r.json(); })
            .then(renderList)
            .catch(function () { showToast("Failed to load users", true); });
    }

    function renderList(data) {
        var stats = data.stats || {};
        els.statTotal.textContent = stats.totalUsers != null ? stats.totalUsers : "0";
        els.statTotalNote.textContent = (stats.adminsCount || 0) + (stats.adminsCount === 1 ? " admin" : " admins");
        els.statActive.textContent = stats.activeCount != null ? stats.activeCount : "0";
        els.statLocked.textContent = stats.lockedCount != null ? stats.lockedCount : "0";
        els.statLockedNote.textContent = (stats.lockedCount || 0) ? "Access suspended" : "No locked accounts";
        els.statAdmins.textContent = stats.adminsCount != null ? stats.adminsCount : "0";

        var items = data.items || [];
        var totalCount = data.totalCount || 0;
        var pageSize = data.pageSize || state.pageSize;
        var page = data.page || state.page;
        var pageCount = Math.max(1, Math.ceil(totalCount / pageSize));

        els.resultLabel.textContent = totalCount + (totalCount === 1 ? " user" : " users") + (state.status === "All" ? "" : " · " + state.status);
        els.emptyState.hidden = items.length !== 0;

        els.userRows.innerHTML = "";
        items.forEach(function (u, i) {
            var hue = HUES[i % HUES.length];
            var row = document.createElement("div");
            row.className = "sh-table-row sh-table-cols-users";

            var rolePillClass = u.role === "Admin" ? "sh-pill-role-admin" : "sh-pill-role-customer";
            var statusLabel = u.isLocked ? "Locked" : "Active";
            var statusPillClass = u.isLocked ? "sh-pill-status-locked" : "sh-pill-status-active";

            var promoteHtml, lockHtml;
            if (u.role === "Admin") {
                promoteHtml = '<button type="button" class="sh-action-btn" data-demote="' + u.id + '"' + (u.isSelf ? " disabled" : "") + '>Demote</button>';
            } else {
                promoteHtml = '<button type="button" class="sh-action-btn" data-promote="' + u.id + '">Promote</button>';
            }
            if (u.isLocked) {
                lockHtml = '<button type="button" class="sh-action-btn" data-unlock="' + u.id + '"' + (u.isSelf ? " disabled" : "") + '>Unlock</button>';
            } else {
                lockHtml = '<button type="button" class="sh-action-btn sh-action-btn-warn" data-lock="' + u.id + '"' + (u.isSelf ? " disabled" : "") + '>Lock</button>';
            }

            row.innerHTML =
                '<div class="sh-user-name-cell">' +
                '<span class="sh-user-avatar" style="background: oklch(0.93 0.05 ' + hue + '); color: oklch(0.42 0.12 ' + hue + ');">' + escapeHtml(initials(u.userName)) + '</span>' +
                '<span style="min-width:0;">' +
                '<span style="display:block; font-size:14.5px; font-weight:600; letter-spacing:-0.01em; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;">' + escapeHtml(u.userName) + (u.isSelf ? ' <span style="font-family:\'IBM Plex Mono\', monospace; font-size:9px; color: oklch(0.55 0.03 280);">(you)</span>' : '') + '</span>' +
                '<span style="display:block; font-family:\'IBM Plex Mono\', monospace; font-size:10px; color: oklch(0.55 0.02 280); margin-top:4px;">' + escapeHtml(u.name) + '</span>' +
                '</span></div>' +
                '<div style="min-width:0; font-size:13.5px; color: oklch(0.42 0.02 280); white-space:nowrap; overflow:hidden; text-overflow:ellipsis;">' + escapeHtml(u.email) + '</div>' +
                '<div class="sh-col-center"><span class="sh-pill ' + rolePillClass + '">' + u.role + '</span></div>' +
                '<div class="sh-col-center"><span class="sh-pill ' + statusPillClass + '">' + statusLabel + '</span></div>' +
                '<div class="sh-cell-actions">' +
                promoteHtml +
                lockHtml +
                '<button type="button" class="sh-action-btn sh-action-btn-danger" data-delete="' + u.id + '" data-name="' + escapeHtml(u.userName) + '" data-email="' + escapeHtml(u.email) + '"' + (u.isSelf ? " disabled" : "") + '>Delete</button>' +
                '</div>';

            els.userRows.appendChild(row);
        });

        els.footLabel.textContent = totalCount
            ? "Showing " + ((page - 1) * pageSize + 1) + "–" + Math.min(page * pageSize, totalCount) + " of " + totalCount + " users"
            : "No users to show";

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

    els.roleSelect.addEventListener("change", function () {
        state.role = els.roleSelect.value;
        state.page = 1;
        loadList();
    });

    els.pageSizeSelect.addEventListener("change", function () {
        state.pageSize = parseInt(els.pageSizeSelect.value, 10);
        state.page = 1;
        loadList();
    });

    els.statusTabs.addEventListener("click", function (e) {
        var tab = e.target.closest("[data-status]");
        if (!tab) return;
        els.statusTabs.querySelectorAll(".sh-chip").forEach(function (c) { c.classList.remove("active"); });
        tab.classList.add("active");
        state.status = tab.getAttribute("data-status");
        state.page = 1;
        loadList();
    });

    els.resetFiltersBtn.addEventListener("click", function () {
        state.query = ""; state.role = "All"; state.status = "All"; state.page = 1;
        els.searchInput.value = "";
        els.clearSearchBtn.hidden = true;
        els.roleSelect.value = "All";
        els.statusTabs.querySelectorAll(".sh-chip").forEach(function (c) { c.classList.remove("active"); });
        els.statusTabs.querySelector('[data-status="All"]').classList.add("active");
        loadList();
    });

    els.prevPageBtn.addEventListener("click", function () {
        if (state.page > 1) { state.page -= 1; loadList(); }
    });
    els.nextPageBtn.addEventListener("click", function () {
        state.page += 1; loadList();
    });

    function postAction(url) {
        return fetch(url, { method: "POST" }).then(function (r) { return r.json(); });
    }

    els.userRows.addEventListener("click", function (e) {
        var promoteBtn = e.target.closest("[data-promote]");
        if (promoteBtn) {
            postAction(BASE + "/Promote?id=" + encodeURIComponent(promoteBtn.getAttribute("data-promote")))
                .then(function (res) { showToast(res.message, !res.success); loadList(); });
            return;
        }
        var demoteBtn = e.target.closest("[data-demote]");
        if (demoteBtn) {
            postAction(BASE + "/Demote?id=" + encodeURIComponent(demoteBtn.getAttribute("data-demote")))
                .then(function (res) { showToast(res.message, !res.success); loadList(); });
            return;
        }
        var lockBtn = e.target.closest("[data-lock]");
        if (lockBtn) {
            postAction(BASE + "/Lock?id=" + encodeURIComponent(lockBtn.getAttribute("data-lock")))
                .then(function (res) { showToast(res.message, !res.success); loadList(); });
            return;
        }
        var unlockBtn = e.target.closest("[data-unlock]");
        if (unlockBtn) {
            postAction(BASE + "/Unlock?id=" + encodeURIComponent(unlockBtn.getAttribute("data-unlock")))
                .then(function (res) { showToast(res.message, !res.success); loadList(); });
            return;
        }
        var delBtn = e.target.closest("[data-delete]");
        if (delBtn) {
            openDeleteModal(delBtn.getAttribute("data-delete"), delBtn.getAttribute("data-name"), delBtn.getAttribute("data-email"));
        }
    });

    function openDeleteModal(id, name, email) {
        state.deleteId = id;
        state.deleteName = name;
        els.deleteSub.textContent = "“" + name + "” (" + email + ") will lose access and their account data will be removed. This can't be undone.";
        els.deleteModalOverlay.hidden = false;
    }
    function closeDeleteModal() { els.deleteModalOverlay.hidden = true; }

    els.cancelDeleteBtn.addEventListener("click", closeDeleteModal);
    els.deleteModalOverlay.addEventListener("click", function (e) {
        if (e.target === els.deleteModalOverlay) closeDeleteModal();
    });

    els.confirmDeleteBtn.addEventListener("click", function () {
        if (!state.deleteId) return;
        fetch(BASE + "/Delete?id=" + encodeURIComponent(state.deleteId), { method: "DELETE" })
            .then(function (r) { return r.json(); })
            .then(function (res) {
                closeDeleteModal();
                showToast(res.success ? ("Deleted — " + state.deleteName) : (res.message || "Could not delete user"), !res.success);
                loadList();
            })
            .catch(function () {
                closeDeleteModal();
                showToast("A network error occurred.", true);
            });
    });

    loadList();
})();
