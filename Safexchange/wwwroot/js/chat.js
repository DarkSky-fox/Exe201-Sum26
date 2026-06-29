// Chat real-time functionality using SignalR
const Chat = (function () {
    let connection = null;
    let currentConversationId = null;
    let currentUserId = null;
    let isInitialized = false;

    // Get values from page model (set by Razor)
    function getCurrentConversationId() {
        return window.chatConversationId || null;
    }

    function getCurrentUserId() {
        return window.chatCurrentUserId || null;
    }

    async function init() {
        if (isInitialized) return;
        
        // Get values from global scope
        currentUserId = getCurrentUserId();
        currentConversationId = getCurrentConversationId();
        
        try {
            connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/chat")
                .withAutomaticReconnect([0, 1000, 5000, 10000, 30000])
                .configureLogging(signalR.LogLevel.Information)
                .build();

            setupEventHandlers();
            await connection.start();
            
            isInitialized = true;
            console.log("Chat connected to SignalR hub");

            // Join current conversation if exists
            if (currentConversationId) {
                await joinConversation(currentConversationId);
                scrollToBottom();
            }
            
            // Update unread count
            updateUnreadCount();
        } catch (err) {
            console.error("Chat connection failed:", err);
            // Retry after delay
            setTimeout(init, 5000);
        }
    }

    function setupEventHandlers() {
        // Handle incoming messages
        connection.on("ReceiveMessage", (data) => {
            appendMessage(data);
            scrollToBottom();
            
            // Play notification sound if not from current conversation
            if (data.conversationId !== currentConversationId) {
                playNotificationSound();
                updateConversationPreview(data);
            }
        });

        // Handle new message notifications
        connection.on("NewMessageNotification", (data) => {
            showNewMessageToast(data);
            updateUnreadCount();
            updateConversationBadge(data.conversationId);
        });

        // Handle read receipts
        connection.on("MessagesRead", (data) => {
            markMessagesAsReadInUI(data.conversationId);
        });

        // Handle reconnection
        connection.onreconnected(async () => {
            console.log("Chat reconnected");
            await joinCurrentConversation();
            updateUnreadCount();
        });

        connection.onreconnecting((error) => {
            console.log("Chat reconnecting...", error);
        });

        // Setup message form if on chat page
        setupMessageForm();
        
        // Setup conversation list search
        setupConversationSearch();
    }

    function setupMessageForm() {
        const form = document.getElementById("message-form");
        const input = document.getElementById("message-input");
        
        if (!form || !input) return;

        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            const messageText = input.value.trim();
            
            // Get current conversation ID
            const convId = getCurrentConversationId();
            
            if (!messageText) {
                showError("Vui lòng nhập tin nhắn.");
                return;
            }
            
            if (!convId) {
                showError("Không có cuộc trò chuyện nào được chọn.");
                return;
            }

            try {
                input.value = "";
                input.disabled = true;

                // Send via SignalR
                await connection.invoke("SendMessage", convId, messageText, null);
                
                input.disabled = false;
                input.focus();
            } catch (err) {
                console.error("Failed to send message:", err);
                input.value = messageText; // Restore message
                input.disabled = false;
                showError("Không thể gửi tin nhắn. Vui lòng thử lại.");
            }
        });

        // Also handle Enter key in input (already handled by form submit, but add explicit)
        input.addEventListener("keypress", (e) => {
            if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                form.dispatchEvent(new Event("submit"));
            }
        });
    }

    function setupConversationSearch() {
        const searchInput = document.getElementById("conversation-search");
        if (!searchInput) return;

        searchInput.addEventListener("input", (e) => {
            const searchTerm = e.target.value.toLowerCase();
            document.querySelectorAll(".conversation-item").forEach(item => {
                const name = item.querySelector(".conversation-name")?.textContent?.toLowerCase() || "";
                const preview = item.querySelector(".conversation-preview")?.textContent?.toLowerCase() || "";
                
                if (name.includes(searchTerm) || preview.includes(searchTerm)) {
                    item.style.display = "";
                } else {
                    item.style.display = "none";
                }
            });
        });
    }

    async function joinConversation(conversationId) {
        currentConversationId = conversationId;
        window.chatConversationId = conversationId;
        
        if (currentConversationId) {
            await leaveCurrentConversation();
        }
        
        await connection.invoke("JoinConversation", conversationId);
        await connection.invoke("MarkAsRead", conversationId);
        
        // Update UI
        document.querySelectorAll(".conversation-item").forEach(item => {
            item.classList.toggle("active", parseInt(item.dataset.conversationId) === conversationId);
        });
        
        // Clear unread badge for this conversation
        const badge = document.querySelector(`.conversation-item[data-conversation-id="${conversationId}"] .badge`);
        if (badge) {
            badge.remove();
        }
        
        // On mobile, switch to chat view
        if (window.innerWidth < 768) {
            toggleMobileChat('chat');
        }
    }

    async function leaveCurrentConversation() {
        if (currentConversationId) {
            await connection.invoke("LeaveConversation", currentConversationId);
            currentConversationId = null;
        }
    }

    function appendMessage(data) {
        const container = document.getElementById("chat-messages");
        if (!container) return;

        const isOutgoing = data.senderId === currentUserId;
        const messageHtml = `
            <div class="message ${isOutgoing ? 'message-outgoing' : 'message-incoming'}" data-message-id="${data.messageId}">
                <div class="message-bubble">
                    ${!isOutgoing ? `<small class="sender-name">${data.senderName}</small>` : ''}
                    <p class="mb-0">${escapeHtml(data.messageText)}</p>
                    <div class="message-time">
                        ${formatTime(new Date(data.createdAt))}
                        ${isOutgoing ? `<i class="bi ${data.isRead ? 'bi-check-all text-primary' : 'bi-check'}"></i>` : ''}
                    </div>
                </div>
            </div>
        `;

        container.insertAdjacentHTML("beforeend", messageHtml);
        updateConversationPreview(data);
    }

    function scrollToBottom() {
        const container = document.getElementById("chat-messages");
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }

    function markMessagesAsReadInUI(conversationId) {
        document.querySelectorAll(`.message[data-message-id]`).forEach(msg => {
            const icon = msg.querySelector(".bi-check:not(.bi-check-all)");
            if (icon) {
                icon.classList.remove("bi-check");
                icon.classList.add("bi-check-all", "text-primary");
            }
        });
    }

    function updateConversationPreview(data) {
        const convItem = document.querySelector(`.conversation-item[data-conversation-id="${data.conversationId}"]`);
        if (!convItem) return;

        const preview = convItem.querySelector(".conversation-preview");
        const time = convItem.querySelector(".time-ago");
        
        if (preview) {
            const prefix = data.senderId === currentUserId ? "Bạn: " : "";
            preview.textContent = prefix + data.messageText;
        }
        
        if (time) {
            time.textContent = "Vừa xong";
            time.dataset.time = data.createdAt;
        }
    }

    function updateConversationBadge(conversationId) {
        const convItem = document.querySelector(`.conversation-item[data-conversation-id="${conversationId}"]`);
        if (!convItem || convItem.classList.contains("active")) return;

        let badge = convItem.querySelector(".badge");
        if (!badge) {
            badge = document.createElement("span");
            badge.className = "badge bg-primary rounded-pill";
            convItem.querySelector(".d-flex:last-child")?.appendChild(badge);
        }
        
        const currentCount = parseInt(badge.textContent) || 0;
        badge.textContent = currentCount + 1;
    }

    async function updateUnreadCount() {
        try {
            const count = await connection.invoke("GetUnreadCount");
            updateBadges(count);
        } catch (err) {
            console.error("Failed to get unread count:", err);
        }
    }

    function updateBadges(count) {
        // Update navbar badge
        const navbarBadge = document.getElementById("chat-badge");
        if (navbarBadge) {
            if (count > 0) {
                navbarBadge.textContent = count > 99 ? "99+" : count;
                navbarBadge.style.display = "";
            } else {
                navbarBadge.style.display = "none";
            }
        }

        // Update sidebar badge
        const sidebarBadge = document.getElementById("sidebar-chat-badge");
        if (sidebarBadge) {
            if (count > 0) {
                sidebarBadge.textContent = count > 99 ? "99+" : count;
                sidebarBadge.style.display = "";
            } else {
                sidebarBadge.style.display = "none";
            }
        }
    }

    function showNewMessageToast(data) {
        // Create toast notification
        const toast = document.createElement("div");
        toast.className = "chat-toast";
        toast.innerHTML = `
            <div class="toast-content">
                <i class="bi bi-chat-dots"></i>
                <div>
                    <strong>${data.senderName}</strong>
                    <p class="mb-0">${escapeHtml(data.preview)}</p>
                </div>
            </div>
        `;
        
        document.body.appendChild(toast);
        
        // Show toast
        setTimeout(() => toast.classList.add("show"), 100);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            toast.classList.remove("show");
            setTimeout(() => toast.remove(), 300);
        }, 5000);
    }

    function playNotificationSound() {
        try {
            const audio = new Audio("/sounds/message.mp3");
            audio.volume = 0.5;
            audio.play().catch(() => {}); // Ignore autoplay errors
        } catch (e) {
            // Silently fail if audio not supported
        }
    }

    function showError(message) {
        const errorDiv = document.createElement("div");
        errorDiv.className = "alert alert-danger position-fixed bottom-0 start-50 translate-middle-x mb-3";
        errorDiv.style.zIndex = "9999";
        errorDiv.textContent = message;
        document.body.appendChild(errorDiv);
        
        setTimeout(() => {
            errorDiv.remove();
        }, 3000);
    }

    function escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }

    function formatTime(date) {
        const now = new Date();
        const diff = now - date;
        const minutes = Math.floor(diff / 60000);
        
        if (minutes < 1) return "Vừa xong";
        if (minutes < 60) return `${minutes}p`;
        
        const hours = Math.floor(minutes / 60);
        if (hours < 24) return `${hours} giờ`;
        
        const days = Math.floor(hours / 24);
        if (days < 7) return `${days} ngày`;
        
        return date.toLocaleDateString("vi-VN");
    }

    async function joinCurrentConversation() {
        const conversationItem = document.querySelector(".conversation-item.active");
        if (conversationItem) {
            const convId = parseInt(conversationItem.dataset.conversationId);
            if (convId) {
                await joinConversation(convId);
            }
        }
    }

    // Public API
    return {
        init,
        joinConversation,
        updateUnreadCount,
        connection: () => connection
    };
})();
