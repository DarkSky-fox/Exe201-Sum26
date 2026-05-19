const OrderEdit = (function () {
    function getModal() {
        return bootstrap.Modal.getOrCreateInstance(document.getElementById('orderEditModal'));
    }

    function open(orderId) {
        fetch(`/Orders/Edit/${orderId}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(r => {
                if (!r.ok) throw new Error('Không tải được form chỉnh sửa.');
                return r.text();
            })
            .then(html => {
                document.getElementById('order-edit-body').innerHTML = html;
                getModal().show();
            })
            .catch(err => alert(err.message));
    }

    function save(event, orderId) {
        event.preventDefault();
        const form = document.getElementById('order-edit-form');
        const shippingFee = form.querySelector('#shippingFee').value;
        const voucherCode = form.querySelector('#voucherCode').value;

        const body = new URLSearchParams();
        body.append('shippingFee', shippingFee);
        body.append('voucherCode', voucherCode);

        fetch(`/Orders/Edit/${orderId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: body.toString()
        })
            .then(r => r.json())
            .then(data => {
                alert(data.message);
                if (data.success) {
                    getModal().hide();
                    window.location.reload();
                }
            });
    }

    return { open, save };
})();
