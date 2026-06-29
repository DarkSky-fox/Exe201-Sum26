var Favourite = (function () {
    var badgeEl = null;
    var sidebarBadgeEl = null;
    var initialized = false;

    function init() {
        if (initialized) return;
        initialized = true;
        badgeEl = document.getElementById('favourite-badge');
        sidebarBadgeEl = document.getElementById('sidebar-fav-badge');
        refreshBadge();
    }

    function refreshBadge() {
        fetch('/Favourites?handler=Count')
            .then(r => r.json())
            .then(data => {
                if (badgeEl) {
                    if (data.count > 0) {
                        badgeEl.textContent = data.count;
                        badgeEl.style.display = '';
                    } else {
                        badgeEl.style.display = 'none';
                    }
                }
                if (sidebarBadgeEl) {
                    if (data.count > 0) {
                        sidebarBadgeEl.textContent = data.count;
                        sidebarBadgeEl.style.display = '';
                    } else {
                        sidebarBadgeEl.style.display = 'none';
                    }
                }
            })
            .catch(() => { });
    }

    function toggle(productId, btnEl) {
        fetch(`/Favourites?handler=Toggle&productId=${productId}`)
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    if (data.isFavourited) {
                        btnEl.classList.add('favourited');
                        btnEl.querySelector('i').className = 'bi bi-heart-fill';
                        btnEl.title = 'Bỏ khỏi yêu thích';
                    } else {
                        btnEl.classList.remove('favourited');
                        btnEl.querySelector('i').className = 'bi bi-heart';
                        btnEl.title = 'Thêm vào yêu thích';
                    }
                    refreshBadge();
                } else {
                    alert(data.message || 'Có lỗi xảy ra');
                }
            })
            .catch(err => {
                console.error(err);
                alert('Có lỗi xảy ra');
            });
    }

    function check(productId, callback) {
        fetch(`/Favourites?handler=Check&productId=${productId}`)
            .then(r => r.json())
            .then(data => callback(data.isFavourited))
            .catch(() => callback(false));
    }

    return {
        init: init,
        toggle: toggle,
        check: check,
        refreshBadge: refreshBadge
    };
})();

document.addEventListener('DOMContentLoaded', function () {
    Favourite.init();
});
