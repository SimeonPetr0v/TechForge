// TechForge global UI: AJAX add-to-cart, wishlist toggle, live search, toasts.
(function () {
    'use strict';

    // ---------- Toasts ----------
    function showToast(message, type) {
        var container = document.getElementById('toast-container');
        if (!container) { return; }
        var toast = document.createElement('div');
        toast.className = 'tf-toast tf-toast-' + (type || 'success');
        toast.innerHTML = '<i class="bi ' +
            (type === 'error' ? 'bi-exclamation-circle' : 'bi-check-circle') +
            ' me-2"></i>' + message;
        container.appendChild(toast);
        requestAnimationFrame(function () { toast.classList.add('show'); });
        setTimeout(function () {
            toast.classList.remove('show');
            setTimeout(function () { toast.remove(); }, 300);
        }, 2600);
    }

    function updateCartBadge(count) {
        var badge = document.getElementById('cart-count');
        if (!badge) { return; }
        badge.textContent = count;
        badge.classList.toggle('d-none', !count || count <= 0);
    }

    function redirectToLogin() {
        window.location.href = '/Account/Login?ReturnUrl=' +
            encodeURIComponent(window.location.pathname + window.location.search);
    }

    // ---------- AJAX form submit (cart + wishlist) ----------
    document.addEventListener('submit', function (e) {
        var form = e.target;
        var mode = form.getAttribute('data-ajax');
        if (!mode) { return; }

        e.preventDefault();

        fetch(form.action, {
            method: 'POST',
            body: new FormData(form),
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        }).then(function (res) {
            if (res.status === 401) { redirectToLogin(); return null; }
            if (!res.ok) { showToast('Something went wrong.', 'error'); return null; }
            return res.json();
        }).then(function (data) {
            if (!data) { return; }

            if (mode === 'cart-add') {
                updateCartBadge(data.count);
                showToast(data.message, data.success ? 'success' : 'error');
            } else if (mode === 'wishlist-toggle') {
                showToast(data.message, 'success');
                var btn = form.querySelector('[data-wishlist-btn] i, [data-wishlist-btn]');
                var icon = form.querySelector('[data-wishlist-btn] i');
                if (icon) {
                    icon.classList.toggle('bi-heart-fill', data.inWishlist);
                    icon.classList.toggle('bi-heart', !data.inWishlist);
                }
            }
        }).catch(function () {
            showToast('Network error. Please try again.', 'error');
        });
    });

    // ---------- Live product search ----------
    var searchInput = document.getElementById('navSearch');
    var searchResults = document.getElementById('searchResults');
    var debounceTimer = null;

    function hideResults() {
        if (searchResults) {
            searchResults.classList.add('d-none');
            searchResults.innerHTML = '';
        }
    }

    function renderResults(products) {
        if (!searchResults) { return; }
        if (!products || products.length === 0) {
            searchResults.innerHTML = '<div class="tf-search-empty">No products found</div>';
        } else {
            searchResults.innerHTML = products.map(function (p) {
                var img = p.imageUrl
                    ? '<img src="' + p.imageUrl + '" alt="">'
                    : '<span class="tf-search-noimg"><i class="bi bi-cpu"></i></span>';
                return '<a class="tf-search-item" href="/Products/Details/' + p.id + '">' +
                    img +
                    '<span class="tf-search-info"><span class="tf-search-name">' + p.name + '</span>' +
                    '<span class="tf-search-price">$' + Number(p.price).toFixed(2) + '</span></span></a>';
            }).join('');
        }
        searchResults.classList.remove('d-none');
    }

    if (searchInput) {
        searchInput.addEventListener('input', function () {
            var term = searchInput.value.trim();
            clearTimeout(debounceTimer);
            if (term.length < 2) { hideResults(); return; }
            debounceTimer = setTimeout(function () {
                fetch('/api/products/search?term=' + encodeURIComponent(term) + '&limit=6')
                    .then(function (res) { return res.json(); })
                    .then(renderResults)
                    .catch(hideResults);
            }, 250);
        });

        document.addEventListener('click', function (e) {
            if (!searchInput.contains(e.target) && searchResults && !searchResults.contains(e.target)) {
                hideResults();
            }
        });
    }

    window.TechForge = { showToast: showToast, updateCartBadge: updateCartBadge };
})();
