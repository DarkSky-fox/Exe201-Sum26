const Cart = (function () {
    const baseUrl = '/Cart';

    function getViewModal() {
        return bootstrap.Modal.getOrCreateInstance(document.getElementById('cartViewModal'));
    }

    function getEditModal() {
        return bootstrap.Modal.getOrCreateInstance(document.getElementById('cartEditModal'));
    }

    function loadPartial(url, targetId) {
        return fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(r => {
                if (!r.ok) throw new Error('Không tải được nội dung.');
                return r.text();
            })
            .then(html => {
                document.getElementById(targetId).innerHTML = html;
                bindCartSelection();
            });
    }

    function getSelectedCheckboxes() {
        return Array.from(document.querySelectorAll('.cart-item-select:checked'));
    }

    function bindCartSelection() {
        const boxes = document.querySelectorAll('.cart-item-select');
        boxes.forEach(box => {
            box.addEventListener('change', updateSelectedSummary);
        });
        const selectAll = document.getElementById('cart-select-all');
        if (selectAll) {
            selectAll.addEventListener('change', () => toggleSelectAll(selectAll.checked));
        }
        updateSelectedSummary();
    }

    function toggleSelectAll(checked) {
        document.querySelectorAll('.cart-item-select').forEach(box => {
            box.checked = checked;
        });
        const selectAll = document.getElementById('cart-select-all');
        if (selectAll) selectAll.checked = checked;
        updateSelectedSummary();
    }

    function updateSelectedSummary() {
        const selected = getSelectedCheckboxes();
        const countEl = document.getElementById('cart-selected-count');
        const totalEl = document.getElementById('cart-selected-total');
        let total = 0;
        selected.forEach(box => {
            total += parseFloat(box.dataset.price) || 0;
        });
        if (countEl) countEl.textContent = selected.length;
        if (totalEl) totalEl.textContent = total.toLocaleString('vi-VN');
    }

    function refreshBadge() {
        fetch(`${baseUrl}?handler=Count`)
            .then(r => r.json())
            .then(data => {
                const badge = document.getElementById('cart-badge');
                if (!badge) return;
                if (data.count > 0) {
                    badge.textContent = data.count;
                    badge.style.display = 'inline-block';
                } else {
                    badge.style.display = 'none';
                }
            })
            .catch(() => { });
    }

    function openView() {
        loadPartial(`${baseUrl}?handler=View`, 'cart-view-body')
            .then(() => getViewModal().show());
    }

    function openEdit() {
        const viewEl = document.getElementById('cartViewModal');
        const viewInstance = bootstrap.Modal.getInstance(viewEl);
        if (viewInstance) viewInstance.hide();

        loadPartial(`${baseUrl}?handler=Edit`, 'cart-edit-body')
            .then(() => getEditModal().show());
    }

    function saveEdit(event) {
        event.preventDefault();
        const inputs = document.querySelectorAll('.cart-qty-input');
        const updates = Array.from(inputs).map(input => {
            const productId = parseInt(input.dataset.productId, 10);
            const quantity = parseInt(input.value, 10) || 0;
            return updateItem(productId, quantity);
        });

        Promise.all(updates).then(() => {
            getEditModal().hide();
            openView();
            refreshBadge();
        });
    }

    function updateItem(productId, quantity) {
        const body = new URLSearchParams();
        body.append('productId', productId);
        body.append('quantity', quantity);

        return fetch(`${baseUrl}?handler=Update`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: body.toString()
        }).then(r => r.json());
    }

    function removeItem(productId) {
        const body = new URLSearchParams();
        body.append('productId', productId);

        fetch(`${baseUrl}?handler=Remove`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: body.toString()
        })
            .then(r => r.json())
            .then(() => {
                openEdit();
                refreshBadge();
            });
    }

    function goToCheckout() {
        const selected = getSelectedCheckboxes();
        if (selected.length === 0) {
            alert('Chọn ít nhất một sản phẩm để thanh toán.');
            return;
        }

        const body = new URLSearchParams();
        selected.forEach(box => body.append('productIds', box.value));

        fetch(`${baseUrl}?handler=PrepareCheckout`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: body.toString()
        })
            .then(r => r.json())
            .then(data => {
                if (!data.success) {
                    if (data.redirectUrl && data.redirectUrl.includes('Login')) {
                        window.location.href = '/Login';
                        return;
                    }
                    alert(data.message || 'Không thể chuyển sang thanh toán.');
                    return;
                }
                getViewModal().hide();
                window.location.href = data.redirectUrl;
            })
            .catch(() => alert('Không thể chuyển sang thanh toán. Vui lòng đăng nhập.'));
    }

    function prepareCheckout(productIds) {
        const body = new URLSearchParams();
        productIds.forEach(id => body.append('productIds', id));

        return fetch(`${baseUrl}?handler=PrepareCheckout`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: body.toString()
        }).then(r => r.json());
    }

    return {
        openView,
        openEdit,
        saveEdit,
        removeItem,
        goToCheckout,
        prepareCheckout,
        toggleSelectAll,
        refreshBadge
    };
})();
