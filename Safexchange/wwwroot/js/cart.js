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
            });
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

    function checkout() {
        fetch(`${baseUrl}?handler=Checkout`, { method: 'POST' })
            .then(r => r.json())
            .then(data => {
                if (!data.success) {
                    alert(data.message || 'Đặt hàng thất bại.');
                    return;
                }
                getViewModal().hide();
                refreshBadge();
                if (data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                } else {
                    alert(data.message);
                }
            });
    }

    return { openView, openEdit, saveEdit, removeItem, checkout, refreshBadge };
})();
