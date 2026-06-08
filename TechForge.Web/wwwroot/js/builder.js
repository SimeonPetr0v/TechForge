// PC Builder: loads products per category from the REST API and keeps a live total.
(function () {
    'use strict';

    var fmt = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });

    function loadSlot(select) {
        var categoryId = select.dataset.categoryId;
        return fetch('/api/products/by-category/' + categoryId)
            .then(function (res) { return res.json(); })
            .then(function (products) {
                var html = '<option value="0">— None —</option>';
                products.forEach(function (p) {
                    html += '<option value="' + p.id + '" data-price="' + p.price + '">'
                        + p.name + ' — ' + fmt.format(p.price) + '</option>';
                });
                select.innerHTML = html;
                select.dataset.loading = 'false';
            })
            .catch(function () {
                select.innerHTML = '<option value="0">Failed to load</option>';
            });
    }

    function recompute() {
        var total = 0, count = 0;
        document.querySelectorAll('.tf-build-select').forEach(function (sel) {
            var opt = sel.options[sel.selectedIndex];
            var price = opt ? parseFloat(opt.dataset.price || '0') : 0;
            var slot = sel.closest('[data-slot]');
            var priceEl = slot.querySelector('.tf-slot-price');
            if (price > 0) {
                total += price;
                count++;
                priceEl.textContent = fmt.format(price);
            } else {
                priceEl.textContent = '—';
            }
        });
        document.getElementById('build-total').textContent = fmt.format(total);
        document.getElementById('build-count').textContent = count;
    }

    document.addEventListener('DOMContentLoaded', function () {
        var selects = Array.prototype.slice.call(document.querySelectorAll('.tf-build-select'));
        Promise.all(selects.map(loadSlot)).then(function () {
            selects.forEach(function (sel) { sel.addEventListener('change', recompute); });
            recompute();
        });
    });
})();
