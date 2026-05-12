// ===== Material Admin Portal - Site JavaScript =====

// --- Sidebar Toggle ---
function toggleSidebar() {
    document.body.classList.toggle('sidebar-collapsed');
    const sidebar = document.querySelector('.sidebar');
    if (sidebar) sidebar.classList.toggle('collapsed');
    localStorage.setItem('sidebarCollapsed', document.body.classList.contains('sidebar-collapsed'));
}

// Restore sidebar state
document.addEventListener('DOMContentLoaded', function () {
    if (localStorage.getItem('sidebarCollapsed') === 'true') {
        document.body.classList.add('sidebar-collapsed');
        const sidebar = document.querySelector('.sidebar');
        if (sidebar) sidebar.classList.add('collapsed');
    }
});

// --- Delete Confirmation Modal ---
let deleteForm = null;

function showDeleteModal(formId, itemName) {
    const overlay = document.getElementById('deleteModal');
    const nameEl = document.getElementById('deleteItemName');
    deleteForm = document.getElementById(formId);
    if (nameEl) nameEl.textContent = itemName || 'this item';
    if (overlay) overlay.classList.add('show');
}

function hideDeleteModal() {
    const overlay = document.getElementById('deleteModal');
    if (overlay) overlay.classList.remove('show');
    deleteForm = null;
}

function confirmDelete() {
    if (deleteForm) deleteForm.submit();
    hideDeleteModal();
}

// Close modal on overlay click
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('md-modal-overlay')) {
        hideDeleteModal();
    }
});

// --- Toast Notifications ---
function showToast(message, type) {
    type = type || 'info';
    const container = document.querySelector('.toast-container');
    if (!container) return;
    const toast = document.createElement('div');
    toast.className = 'md-toast ' + type;
    toast.innerHTML = '<span class="material-icons">' +
        (type === 'success' ? 'check_circle' : type === 'error' ? 'error' : 'info') +
        '</span><span>' + message + '</span>';
    container.appendChild(toast);
    setTimeout(function () {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(60px)';
        toast.style.transition = 'all .3s ease';
        setTimeout(function () { toast.remove(); }, 300);
    }, 4000);
}

// Auto-show toast from TempData
document.addEventListener('DOMContentLoaded', function () {
    var msgEl = document.getElementById('toastMessage');
    var typeEl = document.getElementById('toastType');
    if (msgEl && msgEl.value) {
        showToast(msgEl.value, typeEl ? typeEl.value : 'info');
    }
});

// --- Search Debounce ---
document.addEventListener('DOMContentLoaded', function () {
    var searchInput = document.getElementById('tableSearch');
    var searchForm = document.getElementById('searchForm');
    if (searchInput && searchForm) {
        var timer;
        searchInput.addEventListener('input', function () {
            clearTimeout(timer);
            timer = setTimeout(function () { searchForm.submit(); }, 500);
        });
    }
});

// --- Ripple Effect ---
document.addEventListener('click', function (e) {
    var btn = e.target.closest('.md-btn');
    if (!btn) return;
    var ripple = document.createElement('span');
    var rect = btn.getBoundingClientRect();
    var size = Math.max(rect.width, rect.height);
    ripple.style.width = ripple.style.height = size + 'px';
    ripple.style.left = (e.clientX - rect.left - size / 2) + 'px';
    ripple.style.top = (e.clientY - rect.top - size / 2) + 'px';
    ripple.style.position = 'absolute';
    ripple.style.borderRadius = '50%';
    ripple.style.background = 'rgba(255,255,255,.3)';
    ripple.style.transform = 'scale(0)';
    ripple.style.animation = 'rippleAnim .5s ease-out';
    btn.appendChild(ripple);
    setTimeout(function () { ripple.remove(); }, 500);
});

// Ripple keyframe injected once
(function () {
    var style = document.createElement('style');
    style.textContent = '@keyframes rippleAnim { to { transform: scale(2.5); opacity: 0; } }';
    document.head.appendChild(style);
})();

// --- Dashboard Chart (subtle line) ---
function initDashboardChart(labels, data) {
    var ctx = document.getElementById('loanChart');
    if (!ctx) return;
    var teal = 'rgba(20, 184, 166, 0.35)';
    var tealLine = '#0d9488';
    new Chart(ctx.getContext('2d'), {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Volume',
                data: data,
                fill: true,
                tension: 0.35,
                pointRadius: 3,
                pointBackgroundColor: '#fff',
                pointBorderColor: tealLine,
                pointBorderWidth: 2,
                backgroundColor: teal,
                borderColor: tealLine,
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: { color: 'rgba(13, 148, 136, 0.08)' },
                    ticks: { font: { family: 'system-ui, sans-serif' }, color: '#64748b' }
                },
                x: {
                    grid: { display: false },
                    ticks: { font: { family: 'system-ui, sans-serif' }, color: '#64748b' }
                }
            }
        }
    });
}
