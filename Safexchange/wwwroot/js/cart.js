const Cart = (function () {
    const baseUrl = '/Cart';

    function getViewModal() {
        return bootstrap.Modal.getOrCreateInstance(document.getElementById('cartViewModal'));
    }

    function showToast(message, isError) {
        const toastEl = document.getElementById('cart-toast');
        const bodyEl = document.getElementById('cart-toast-body');
        if (!toastEl || !bodyEl) {
            alert(message);
            return;
        }

        bodyEl.textContent = message;
        toastEl.classList.remove('text-bg-success', 'text-bg-danger');
        toastEl.classList.add(isError ? 'text-bg-danger' : 'text-bg-success');
        bootstrap.Toast.getOrCreateInstance(toastEl, { delay: 2800 }).show();
    }

    function parseJsonResponse(response) {
        const contentType = response.headers.get('content-type') || '';
        if (!response.ok) {
            if (contentType.includes('application/json')) {
                return response.json().then(data => {
                    throw new Error(data.message || `Yêu cầu thất bại (${response.status}).`);
                });
            }
            throw new Error(`Yêu cầu thất bại (${response.status}). Vui lòng đăng nhập và thử lại.`);
        }

        if (contentType.includes('application/json')) {
            return response.json();
        }

        throw new Error('Phản hồi không hợp lệ từ máy chủ.');
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

    function removeItem(productId) {
        if (!confirm('Xóa sản phẩm này khỏi giỏ hàng?')) {
            return;
        }

        const body = new URLSearchParams();
        body.append('productId', productId);

        fetch(`${baseUrl}?handler=Remove`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: body.toString()
        })
            .then(r => parseJsonResponse(r))
            .then(() => {
                openView();
                refreshBadge();
                showToast('Đã xóa sản phẩm khỏi giỏ.', false);
            })
            .catch(err => showToast(err.message || 'Không thể xóa sản phẩm.', true));
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
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: body.toString()
        })
            .then(r => parseJsonResponse(r))
            .then(data => {
                if (!data.success) {
                    if (data.redirectUrl && data.redirectUrl.includes('Login')) {
                        window.location.href = '/Login';
                        return;
                    }
                    showToast(data.message || 'Không thể chuyển sang thanh toán.', true);
                    if (data.message) {
                        openView();
                        refreshBadge();
                    }
                    return;
                }
                getViewModal().hide();
                window.location.href = data.redirectUrl;
            })
            .catch(() => showToast('Không thể chuyển sang thanh toán. Vui lòng đăng nhập.', true));
    }

    function add(productId) {
        const body = new URLSearchParams();
        body.append('productId', productId);

        return fetch(`${baseUrl}?handler=Add`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: body.toString()
        })
            .then(r => parseJsonResponse(r))
            .then(data => {
                if (!data.success) {
                    if (data.redirectUrl) {
                        window.location.href = data.redirectUrl;
                        return data;
                    }
                    showToast(data.message || 'Không thể thêm vào giỏ.', true);
                    return data;
                }

                if (typeof data.cartCount === 'number') {
                    const badge = document.getElementById('cart-badge');
                    if (badge) {
                        badge.textContent = data.cartCount;
                        badge.style.display = data.cartCount > 0 ? 'inline-block' : 'none';
                    }
                } else {
                    refreshBadge();
                }

                showToast(data.message || 'Đã thêm vào giỏ hàng.', false);
                return data;
            })
            .catch(err => {
                showToast(err.message || 'Không thể thêm vào giỏ hàng.', true);
            });
    }

    return {
        openView,
        removeItem,
        goToCheckout,
        toggleSelectAll,
        refreshBadge,
        add
    };
})();
