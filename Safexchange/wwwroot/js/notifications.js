// Notifications real-time functionality using SignalR
const Notifications = (function () {
    let connection = null;
    let isInitialized = false;

    async function init() {
        if (isInitialized) return;
        
        try {
            connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/notification")
                .withAutomaticReconnect([0, 1000, 5000, 10000, 30000])
                .configureLogging(signalR.LogLevel.Information)
                .build();

            setupEventHandlers();
            await connection.start();
            
            isInitialized = true;
            console.log("Notifications connected to SignalR hub");

            // Update unread count
            await updateBadgeCount();
        } catch (err) {
            console.error("Notifications connection failed:", err);
            // Retry after delay
            setTimeout(init, 5000);
        }
    }

    function setupEventHandlers() {
        // Handle new notification
        connection.on("ReceiveNotification", (data) => {
            addNewNotificationToDropdown(data);
            incrementBadge();
            showToast(data);
        });

        // Handle notification read
        connection.on("NotificationRead", (data) => {
            updateNotificationUI(data.notificationId, true);
            decrementBadge();
        });

        // Handle all notifications read
        connection.on("AllNotificationsRead", () => {
            resetBadges();
            markAllAsReadInUI();
        });

        // Handle reconnection
        connection.onreconnected(async () => {
            console.log("Notifications reconnected");
            await updateBadgeCount();
        });

        connection.onreconnecting((error) => {
            console.log("Notifications reconnecting...", error);
        });
    }

    async function updateBadgeCount() {
        try {
            const count = await connection.invoke("GetUnreadCount");
            updateBadges(count);
        } catch (err) {
            console.error("Failed to get unread count:", err);
        }
    }

    function updateBadges(count) {
        // Update navbar badge
        const navbarBadge = document.getElementById("notification-badge");
        if (navbarBadge) {
            if (count > 0) {
                navbarBadge.textContent = count > 99 ? "99+" : count;
                navbarBadge.style.display = "";
            } else {
                navbarBadge.style.display = "none";
            }
        }

        // Update sidebar badge
        const sidebarBadge = document.getElementById("sidebar-notif-badge");
        if (sidebarBadge) {
            if (count > 0) {
                sidebarBadge.textContent = count > 99 ? "99+" : count;
                sidebarBadge.style.display = "";
            } else {
                sidebarBadge.style.display = "none";
            }
        }
    }

    function incrementBadge() {
        const navbarBadge = document.getElementById("notification-badge");
        const sidebarBadge = document.getElementById("sidebar-notif-badge");
        
        const updateBadge = (badge) => {
            if (badge) {
                const current = parseInt(badge.textContent) || 0;
                const newCount = current + 1;
                badge.textContent = newCount > 99 ? "99+" : newCount;
                badge.style.display = "";
            }
        };
        
        updateBadge(navbarBadge);
        updateBadge(sidebarBadge);
    }

    function decrementBadge() {
        const navbarBadge = document.getElementById("notification-badge");
        const sidebarBadge = document.getElementById("sidebar-notif-badge");
        
        const updateBadge = (badge) => {
            if (badge) {
                const current = parseInt(badge.textContent) || 0;
                const newCount = Math.max(0, current - 1);
                if (newCount > 0) {
                    badge.textContent = newCount > 99 ? "99+" : newCount;
                    badge.style.display = "";
                } else {
                    badge.style.display = "none";
                }
            }
        };
        
        updateBadge(navbarBadge);
        updateBadge(sidebarBadge);
    }

    function resetBadges() {
        const navbarBadge = document.getElementById("notification-badge");
        const sidebarBadge = document.getElementById("sidebar-notif-badge");
        
        if (navbarBadge) navbarBadge.style.display = "none";
        if (sidebarBadge) sidebarBadge.style.display = "none";
    }

    function addNewNotificationToDropdown(data) {
        const list = document.getElementById("notification-list");
        if (!list) return;

        // Remove empty state if present
        const emptyState = list.querySelector(".text-center");
        if (emptyState) {
            emptyState.remove();
        }

        // Create notification item
        const item = document.createElement("a");
        item.href = data.linkUrl || "/Notifications/Index";
        item.className = "notification-dropdown-item unread";
        item.dataset.notificationId = data.notificationId;
        
        item.innerHTML = `
            <div class="notification-dropdown-icon ${data.iconClass || 'bg-primary'}">
                <i class="${data.icon || 'bi-bell'}"></i>
            </div>
            <div class="notification-dropdown-content">
                <p class="mb-0 fw-medium">${escapeHtml(data.title)}</p>
                <p class="mb-0 text-muted small">${escapeHtml(data.content)}</p>
                <small class="text-muted">Vừa xong</small>
            </div>
        `;

        // Add click handler to mark as read
        item.addEventListener("click", (e) => {
            e.preventDefault();
            markAsRead(data.notificationId);
            window.location.href = item.href;
        });

        // Insert at top
        list.insertBefore(item, list.firstChild);

        // Limit to 10 items in dropdown
        const items = list.querySelectorAll(".notification-dropdown-item");
        if (items.length > 10) {
            items[items.length - 1].remove();
        }
    }

    function updateNotificationUI(notificationId, isRead) {
        const item = document.querySelector(`[data-notification-id="${notificationId}"]`);
        if (item) {
            item.classList.toggle("unread", !isRead);
        }
    }

    function markAllAsReadInUI() {
        document.querySelectorAll(".notification-dropdown-item.unread").forEach(item => {
            item.classList.remove("unread");
        });
    }

    function showToast(data) {
        // Create toast notification
        const toast = document.createElement("div");
        toast.className = "notification-toast";
        toast.innerHTML = `
            <div class="toast-icon ${data.iconClass || 'bg-primary'}">
                <i class="${data.icon || 'bi-bell'}"></i>
            </div>
            <div class="toast-content">
                <strong>${escapeHtml(data.title)}</strong>
                <p class="mb-0">${escapeHtml(data.content)}</p>
            </div>
            <button class="toast-close" onclick="this.parentElement.remove()">
                <i class="bi bi-x"></i>
            </button>
        `;
        
        document.body.appendChild(toast);
        
        // Show toast
        setTimeout(() => toast.classList.add("show"), 100);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            toast.classList.remove("show");
            setTimeout(() => toast.remove(), 300);
        }, 5000);

        // Play sound
        playNotificationSound();
    }

    function playNotificationSound() {
        try {
            const audio = new Audio("/sounds/notification.mp3");
            audio.volume = 0.5;
            audio.play().catch(() => {}); // Ignore autoplay errors
        } catch (e) {
            // Silently fail if audio not supported
        }
    }

    async function markAsRead(notificationId) {
        try {
            await connection.invoke("MarkAsRead", notificationId);
        } catch (err) {
            console.error("Failed to mark notification as read:", err);
            // Fallback to server endpoint
            try {
                await fetch(`/Notifications/Index?handler=MarkAsRead&id=${notificationId}`, {
                    method: "POST"
                });
            } catch (e) {
                console.error("Fallback also failed:", e);
            }
        }
    }

    async function markAllAsRead() {
        try {
            await connection.invoke("MarkAllAsRead");
        } catch (err) {
            console.error("Failed to mark all notifications as read:", err);
            // Fallback to server endpoint
            try {
                await fetch("/Notifications/Index?handler=MarkAllAsRead", {
                    method: "POST"
                });
            } catch (e) {
                console.error("Fallback also failed:", e);
            }
        }
    }

    function loadDropdown() {
        // This is called when dropdown opens
        fetch("/Notifications/Index?handler=List")
            .then(res => res.json())
            .then(data => {
                updateDropdown(data);
            })
            .catch(err => {
                console.error("Failed to load notifications:", err);
            });
    }

    function updateDropdown(notifications) {
        const list = document.getElementById("notification-list");
        if (!list) return;

        if (!notifications || notifications.length === 0) {
            list.innerHTML = `
                <div class="text-center py-4 text-muted">
                    <i class="bi bi-bell-slash fs-4"></i>
                    <p class="mb-0 small">Không có thông báo nào</p>
                </div>
            `;
            return;
        }

        list.innerHTML = notifications.map(n => `
            <a href="${n.linkUrl || '/Notifications/Index'}" 
               class="notification-dropdown-item ${n.isRead ? '' : 'unread'}"
               data-notification-id="${n.notificationId}"
               onclick="Notifications.markAsRead(${n.notificationId})">
                <div class="notification-dropdown-icon ${n.iconClass || 'bg-primary'}">
                    <i class="${n.icon || 'bi-bell'}"></i>
                </div>
                <div class="notification-dropdown-content">
                    <p class="mb-0 fw-medium">${escapeHtml(n.title)}</p>
                    <p class="mb-0 text-muted small">${escapeHtml(n.content)}</p>
                    <small class="text-muted">${n.timeAgo}</small>
                </div>
            </a>
        `).join("");
    }

    function escapeHtml(text) {
        if (!text) return "";
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }

    // Public API
    return {
        init,
        markAsRead,
        markAllAsRead,
        loadDropdown,
        updateBadgeCount
    };
})();
