// Sidebar toggle function
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebar-overlay');
    
    if (sidebar && overlay) {
        sidebar.classList.toggle('show');
        overlay.classList.toggle('show');
    }
}

// Close sidebar when clicking outside on mobile
document.addEventListener('click', function(e) {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebar-overlay');
    const toggleBtn = document.querySelector('.sidebar-toggle-btn');
    
    if (sidebar && overlay && sidebar.classList.contains('show')) {
        if (!sidebar.contains(e.target) && !toggleBtn?.contains(e.target)) {
            sidebar.classList.remove('show');
            overlay.classList.remove('show');
        }
    }
});

// Time ago formatting
function timeAgo(dateString) {
    if (!dateString) return '';
    
    const date = new Date(dateString);
    const now = new Date();
    const diff = now - date;
    
    const minutes = Math.floor(diff / 60000);
    if (minutes < 1) return 'Vừa xong';
    if (minutes < 60) return `${minutes}p`;
    
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours} giờ`;
    
    const days = Math.floor(hours / 24);
    if (days < 7) return `${days} ngày`;
    
    return date.toLocaleDateString('vi-VN');
}

// Update all time-ago elements
function updateTimeAgo() {
    document.querySelectorAll('.time-ago[data-time]').forEach(el => {
        el.textContent = timeAgo(el.dataset.time);
    });
}

// Update time ago every minute
setInterval(updateTimeAgo, 60000);
