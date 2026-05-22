const ShipperDeliveries = (function () {
    const baseUrl = '/Shipper/Deliveries';

    function showToast(message, isError) {
        const toastEl = document.getElementById('shipper-toast');
        const bodyEl = document.getElementById('shipper-toast-body');
        if (!toastEl || !bodyEl) {
            alert(message);
            return;
        }
        bodyEl.textContent = message;
        toastEl.classList.remove('text-bg-success', 'text-bg-danger');
        toastEl.classList.add(isError ? 'text-bg-danger' : 'text-bg-success');
        bootstrap.Toast.getOrCreateInstance(toastEl).show();
    }

    function post(handler, shipmentId) {
        const body = new URLSearchParams();
        body.append('shipmentId', shipmentId);

        return fetch(`${baseUrl}?handler=${handler}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: body.toString()
        })
            .then(r => r.json())
            .then(data => {
                if (!data.success) {
                    showToast(data.message || 'Thao tác thất bại.', true);
                    return;
                }
                showToast(data.message || 'Thành công.', false);
                setTimeout(() => window.location.reload(), 700);
            })
            .catch(() => showToast('Không thể kết nối máy chủ.', true));
    }

    function accept(shipmentId) {
        if (!confirm('Nhận đơn giao hàng này?')) return;
        post('Accept', shipmentId);
    }

    function confirmCod(shipmentId) {
        if (!confirm('Xác nhận đã giao hàng và thu tiền COD từ khách? Trạng thái đơn sẽ chuyển sang Đã thanh toán / Hoàn thành.')) return;
        post('ConfirmCod', shipmentId);
    }

    return { accept, confirmCod };
})();
