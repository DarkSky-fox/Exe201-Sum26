const ProductActions = (function () {
    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value
            || document.querySelector('meta[name="RequestVerificationToken"]')?.getAttribute('content')
            || '';
    }

    function postAction(handler, productId) {
        const body = new URLSearchParams();
        body.append('id', productId);
        body.append('quantity', '1');

        const token = getAntiForgeryToken();
        if (token) {
            body.append('__RequestVerificationToken', token);
        }

        const headers = {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        };
        if (token) {
            headers['RequestVerificationToken'] = token;
        }

        return fetch(`/Products/Detail/${productId}?handler=${handler}`, {
            method: 'POST',
            headers,
            body: body.toString()
        }).then(async response => {
            const contentType = response.headers.get('content-type') || '';
            if (!response.ok) {
                const text = contentType.includes('application/json')
                    ? JSON.stringify(await response.json())
                    : await response.text();
                throw new Error(`Yêu cầu thất bại (${response.status}). ${text}`.slice(0, 200));
            }
            if (contentType.includes('application/json')) {
                return response.json();
            }
            throw new Error('Phản hồi không hợp lệ từ máy chủ.');
        });
    }

    function showToast(message, isError) {
        const toastEl = document.getElementById('product-toast');
        const bodyEl = document.getElementById('product-toast-body');
        if (!toastEl || !bodyEl) {
            alert(message);
            return;
        }
        bodyEl.textContent = message;
        toastEl.classList.remove('text-bg-success', 'text-bg-danger');
        toastEl.classList.add(isError ? 'text-bg-danger' : 'text-bg-success');
        bootstrap.Toast.getOrCreateInstance(toastEl).show();
    }

    function handleResponse(data) {
        if (!data.success && data.redirectUrl) {
            window.location.href = data.redirectUrl;
            return;
        }
        if (data.cartCount !== undefined && typeof Cart !== 'undefined') {
            Cart.refreshBadge();
        }
        showToast(data.message || 'Hoàn tất.', !data.success);
        if (data.success && data.redirectUrl) {
            window.location.href = data.redirectUrl;
            return;
        }
        if (data.success && data.openCart && typeof Cart !== 'undefined') {
            setTimeout(() => Cart.openView(), 400);
        }
    }

    function handleError(err) {
        console.error(err);
        showToast(err.message || 'Không thể thực hiện. Vui lòng thử lại.', true);
    }

    function addToCart(productId) {
        postAction('AddToCart', productId)
            .then(handleResponse)
            .catch(handleError);
    }

    function buyNow(productId) {
        postAction('BuyNow', productId)
            .then(handleResponse)
            .catch(handleError);
    }

    return { addToCart, buyNow };
})();
