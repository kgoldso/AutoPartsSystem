/* ═══════════════════════════════════════════════
   AutoParts — Frontend Application Logic
   SPA routing, API calls, parallax, animations
   ═══════════════════════════════════════════════ */

// ─── State ───
let currentUser = null;
let currentPage = 'home';

// ─── API Helper ───
async function api(url, options = {}) {
    const res = await fetch(url, {
        headers: { 'Content-Type': 'application/json', ...options.headers },
        ...options
    });
    const data = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(data.message || 'Ошибка сервера');
    return data;
}

// ─── Toast ───
function toast(message, type = 'success') {
    const container = document.getElementById('toastContainer');
    const el = document.createElement('div');
    el.className = `toast toast-${type}`;
    el.textContent = message;
    container.appendChild(el);
    setTimeout(() => el.remove(), 3000);
}

// ─── Modal ───
function openModal(id) {
    document.getElementById(id).classList.add('active');
}
function closeModal(id) {
    document.getElementById(id).classList.remove('active');
}

// ─── Navigation ───
function navigateTo(page) {
    // Hide all pages
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    // Show target page
    const target = document.getElementById(`page-${page}`);
    if (target) {
        target.classList.add('active');
        currentPage = page;
    }

    // Update nav links
    document.querySelectorAll('.nav-links a, [data-page]').forEach(a => {
        a.classList.toggle('active', a.dataset.page === page);
    });

    // Load page data
    switch (page) {
        case 'home': loadHomeProducts(); break;
        case 'catalog': loadCatalog(); loadGroups(); break;
        case 'orders': loadOrders(); break;
        case 'warehouse': loadPending(); break;
        case 'dashboard': loadDashboard(); break;
    }

    // Scroll to top smoothly
    window.scrollTo({ top: 0, behavior: 'smooth' });

    // Re-apply reveal animations
    setTimeout(() => {
        initRevealAnimations();
        initScrollFloat();
    }, 100);
}

// Click handler for navigation links
document.addEventListener('click', (e) => {
    const link = e.target.closest('[data-page]');
    if (link) {
        e.preventDefault();
        navigateTo(link.dataset.page);
    }
});

// ─── Auth ───
async function checkAuth() {
    try {
        currentUser = await api('/api/auth/me');
        updateNavForUser();
    } catch {
        currentUser = null;
        updateNavForGuest();
    }
}

function updateNavForUser() {
    const authDiv = document.getElementById('navAuth');
    const initial = currentUser.login[0].toUpperCase();
    const roleNames = { Admin: 'Администратор', Manager: 'Менеджер', Warehouse: 'Кладовщик', Client: 'Клиент' };

    authDiv.innerHTML = `
        <div class="nav-user">
            <div class="nav-user-avatar">${initial}</div>
            <div>
                <div class="nav-user-info">${currentUser.login}</div>
                <div class="nav-user-role">${roleNames[currentUser.role] || currentUser.role}</div>
            </div>
        </div>
        <button class="btn btn-ghost btn-sm" onclick="doLogout()">Выйти</button>
    `;

    // Show/hide menu items based on role
    document.getElementById('navOrders').style.display =
        ['Manager', 'Admin', 'Client'].includes(currentUser.role) ? '' : 'none';
    document.getElementById('navWarehouse').style.display =
        ['Warehouse', 'Admin'].includes(currentUser.role) ? '' : 'none';
    document.getElementById('navDashboard').style.display =
        currentUser.role === 'Admin' ? '' : 'none';
}

function updateNavForGuest() {
    const authDiv = document.getElementById('navAuth');
    authDiv.innerHTML = `<a href="#" data-page="login" class="btn btn-primary btn-sm">Войти</a>`;
    document.getElementById('navOrders').style.display = 'none';
    document.getElementById('navWarehouse').style.display = 'none';
    document.getElementById('navDashboard').style.display = 'none';
}

async function doLogin(e) {
    e.preventDefault();
    const login = document.getElementById('loginInput').value;
    const password = document.getElementById('passwordInput').value;
    const errorEl = document.getElementById('loginError');

    try {
        currentUser = await api('/api/auth/login', {
            method: 'POST',
            body: JSON.stringify({ login, password })
        });
        errorEl.style.display = 'none';
        updateNavForUser();
        toast(`Добро пожаловать, ${currentUser.login}!`);

        // Redirect based on role
        switch (currentUser.role) {
            case 'Admin': navigateTo('dashboard'); break;
            case 'Manager': navigateTo('orders'); break;
            case 'Warehouse': navigateTo('warehouse'); break;
            default: navigateTo('catalog'); break;
        }
    } catch (err) {
        errorEl.textContent = err.message;
        errorEl.style.display = 'block';
    }
}

async function doLogout() {
    try {
        await api('/api/auth/logout', { method: 'POST' });
    } catch {}
    currentUser = null;
    updateNavForGuest();
    toast('Вы вышли из системы', 'info');
    navigateTo('home');
}

function fillLogin(login, pass) {
    document.getElementById('loginInput').value = login;
    document.getElementById('passwordInput').value = pass;
}

// ─── Product icons by group ───
const groupIcons = {
    'Двигатель': '⚙️', 'Подвеска': '🔩', 'Тормоза': '🛑',
    'Электрика': '⚡', 'Кузов': '🚗'
};

const partImages = {};

function getStockBadge(stock) {
    if (stock === 0) return '<span class="product-badge badge-out">Нет в наличии</span>';
    if (stock < 10) return '<span class="product-badge badge-low">Мало</span>';
    return '<span class="product-badge badge-stock">В наличии</span>';
}

function renderProductCard(part) {
    const icon = groupIcons[part.groupName] || '🔧';
    const imagePath = `/img/catalog/part_${part.id}.png`;

    return `
        <div class="product-card reveal-scale" onclick="openOrderModal(${part.id})">
            <div class="product-img">
                <img src="${imagePath}" alt="${part.name}" onerror="this.style.display='none'; this.nextElementSibling.style.display='block';">
                <div class="product-icon" style="display: none;">${icon}</div>
                ${getStockBadge(part.stock)}
            </div>
            <div class="product-body">
                <div class="product-group">${part.groupName}</div>
                <div class="product-name">${part.name}</div>
                <div class="product-car">${part.carBrand} ${part.carModel} · ${part.manufacturer}</div>
                <div class="product-footer">
                    <div class="product-price">${part.price.toFixed(2)} <small>BYN</small></div>
                    <button class="btn btn-primary btn-sm" onclick="event.stopPropagation();openOrderModal(${part.id})">В корзину</button>
                </div>
            </div>
        </div>
    `;
}

// ─── Home Page ───
let sliderInterval = null;
let sliderIndex = 0;

async function loadHomeProducts() {
    try {
        const parts = await api('/api/catalog');
        const track = document.getElementById('homeProducts');
        // Load up to 10 products for the slider
        track.innerHTML = parts.slice(0, 10).map(renderProductCard).join('');
        
        // Start slider logic
        startProductSlider();
        
        setTimeout(initRevealAnimations, 100);
    } catch {}
}

function startProductSlider() {
    if (sliderInterval) clearInterval(sliderInterval);
    
    const track = document.getElementById('homeProducts');
    const viewport = track.parentElement;
    
    sliderIndex = 0;
    
    sliderInterval = setInterval(() => {
        const cards = track.querySelectorAll('.product-card');
        if (cards.length === 0) return;
        
        // Calculate how many cards fit in view
        const cardWidth = 300 + 24; // width + gap
        const visibleCards = Math.floor(viewport.offsetWidth / cardWidth) || 1;
        const maxIndex = cards.length - visibleCards;
        
        sliderIndex++;
        if (sliderIndex > maxIndex) {
            sliderIndex = 0;
        }
        
        track.style.transform = `translateX(-${sliderIndex * cardWidth}px)`;
    }, 4000); // Change every 4 seconds
}

// ─── Catalog Page ───
async function loadCatalog() {
    const q = document.getElementById('catalogSearch')?.value || '';
    const group = document.getElementById('catalogGroup')?.value || '';
    try {
        const parts = await api(`/api/catalog?q=${encodeURIComponent(q)}&group=${encodeURIComponent(group)}`);
        const grid = document.getElementById('catalogGrid');
        if (parts.length === 0) {
            grid.innerHTML = '<div class="empty-state"><div class="icon">🔍</div><p>Ничего не найдено</p></div>';
        } else {
            grid.innerHTML = parts.map(renderProductCard).join('');
        }
        setTimeout(initRevealAnimations, 100);
    } catch {}
}

async function loadGroups() {
    try {
        const groups = await api('/api/catalog/groups');
        const select = document.getElementById('catalogGroup');
        if (select.options.length <= 1) {
            groups.forEach(g => {
                const opt = document.createElement('option');
                opt.value = g;
                opt.textContent = g;
                select.appendChild(opt);
            });
        }
    } catch {}
}

// ─── Order Modal ───
async function openOrderModal(partId) {
    if (!currentUser) {
        toast('Войдите в систему для заказа', 'info');
        navigateTo('login');
        return;
    }
    try {
        const part = await api(`/api/catalog/${partId}`);
        document.getElementById('orderPartInfo').innerHTML = `
            <div class="card" style="margin-bottom:20px;padding:16px">
                <div class="product-group">${part.groupName}</div>
                <div class="product-name" style="font-size:1.1rem">${part.name}</div>
                <div class="product-car">${part.carBrand} ${part.carModel} · Арт: ${part.article}</div>
                <div style="margin-top:8px;font-size:1.2rem;font-weight:700;color:var(--accent)">${part.price.toFixed(2)} BYN</div>
                <div style="font-size:0.85rem;color:var(--text-muted)">На складе: ${part.stock} шт.</div>
            </div>
        `;
        document.getElementById('orderPartId').value = partId;
        
        // Autofill for test client
        if (currentUser.login === 'client') {
            document.getElementById('orderEmail').value = 'client@autoparts.by';
            document.getElementById('orderName').value = 'Алексей Петров';
            document.getElementById('orderPhone').value = '+375291234567';
        }

        openModal('orderModal');
    } catch (err) {
        toast(err.message, 'error');
    }
}

async function submitOrder(e) {
    e.preventDefault();
    try {
        const result = await api('/api/orders', {
            method: 'POST',
            body: JSON.stringify({
                email: document.getElementById('orderEmail').value,
                fullName: document.getElementById('orderName').value,
                phone: document.getElementById('orderPhone').value,
                partId: parseInt(document.getElementById('orderPartId').value),
                quantity: parseInt(document.getElementById('orderQty').value),
                isUrgent: document.getElementById('orderUrgent').checked
            })
        });
        closeModal('orderModal');
        toast(`✅ ${result.message}! Сумма: ${result.totalPrice.toFixed(2)} BYN`);
        document.getElementById('orderForm').reset();
        
        // Refresh catalog if we are there to update stock display
        if (currentPage === 'catalog') loadCatalog();
        // Just refresh background data, don't navigate
        loadOrders(); 
    } catch (err) {
        toast(err.message, 'error');
    }
    return false;
}

// ─── Orders Page ───
function getStatusClass(status) {
    const map = {
        'Новый': 'status-new', 'В обработке': 'status-processing',
        'Отгружен': 'status-shipped', 'Отменен': 'status-cancelled', 'Отменён': 'status-cancelled'
    };
    return map[status] || 'status-new';
}

async function loadOrders() {
    try {
        const orders = await api('/api/orders');
        const body = document.getElementById('ordersBody');
        if (!body) return;

        const isManager = ['Admin', 'Manager'].includes(currentUser.role);

        body.innerHTML = orders.map(o => `
            <tr>
                <td><strong>#${o.id}</strong></td>
                <td>${o.customerName}<br><small style="color:var(--text-muted)">${o.customerEmail}</small></td>
                <td>${o.partName}<br><small style="color:var(--text-muted)">${o.partArticle}</small></td>
                <td>${o.quantity}</td>
                <td style="font-weight:600;color:var(--accent)">${o.totalPrice.toFixed(2)}</td>
                <td><span class="status ${getStatusClass(o.status)}">${o.status}</span></td>
                <td>${new Date(o.orderDate).toLocaleDateString('ru')}</td>
                <td>
                    ${isManager && o.status === 'Новый' ? `<button class="btn btn-sm btn-secondary" onclick="updateOrderStatus(${o.id},'В обработке')">В работу</button>` : ''}
                    ${isManager && (o.status === 'Новый' || o.status === 'В обработке') ? `<button class="btn btn-sm btn-danger" onclick="updateOrderStatus(${o.id},'Отменен')" style="margin-left:4px">Отмена</button>` : ''}
                </td>
            </tr>
        `).join('');
    } catch (err) {
        toast(err.message, 'error');
    }
}

async function updateOrderStatus(id, status) {
    try {
        await api(`/api/orders/${id}/status`, {
            method: 'PUT',
            body: JSON.stringify({ status })
        });
        toast('Статус обновлён');
        loadOrders();
    } catch (err) {
        toast(err.message, 'error');
    }
}

// ─── Warehouse Page ───
async function loadPending() {
    try {
        const orders = await api('/api/warehouse/pending');
        const body = document.getElementById('pendingBody');
        if (!body) return;
        if (orders.length === 0) {
            body.innerHTML = '<tr><td colspan="8" style="text-align:center;padding:40px;color:var(--text-muted)">📦 Нет заказов на отгрузку</td></tr>';
            return;
        }
        body.innerHTML = orders.map(o => `
            <tr>
                <td><strong>#${o.id}</strong></td>
                <td>${o.customerName}</td>
                <td>${o.partName}<br><small style="color:var(--text-muted)">${o.partArticle}</small></td>
                <td>${o.quantity}</td>
                <td style="font-weight:600;color:var(--accent)">${o.totalPrice.toFixed(2)}</td>
                <td>${o.urgent ? '<span class="status status-processing">⚡ Да</span>' : '—'}</td>
                <td>${new Date(o.orderDate).toLocaleDateString('ru')}</td>
                <td><button class="btn btn-success btn-sm" onclick="shipOrder(${o.id})">✅ Отгрузить</button></td>
            </tr>
        `).join('');
    } catch (err) {
        toast(err.message, 'error');
    }
}

async function shipOrder(id) {
    try {
        await api(`/api/warehouse/shipment/${id}`, { method: 'POST' });
        toast('Отгрузка зарегистрирована! ✅');
        loadPending();
    } catch (err) {
        toast(err.message, 'error');
    }
}

async function submitInventory(e) {
    e.preventDefault();
    try {
        await api('/api/warehouse/inventory', {
            method: 'POST',
            body: JSON.stringify({
                partId: parseInt(document.getElementById('invPartId').value),
                quantity: parseInt(document.getElementById('invQty').value)
            })
        });
        closeModal('inventoryModal');
        toast('Остаток обновлён');
        document.getElementById('inventoryForm').reset();
    } catch (err) {
        toast(err.message, 'error');
    }
}

// ─── Dashboard Page ───
async function loadDashboard() {
    loadDashStats();
    loadDashOrders();
}

async function loadDashStats() {
    try {
        const s = await api('/api/admin/stats');
        document.getElementById('statsGrid').innerHTML = `
            <div class="stat-card revenue">
                <div class="stat-icon">💰</div>
                <div class="stat-value">${s.totalRevenue.toFixed(0)}</div>
                <div class="stat-label">Выручка (BYN)</div>
            </div>
            <div class="stat-card orders">
                <div class="stat-icon">📋</div>
                <div class="stat-value">${s.totalOrders}</div>
                <div class="stat-label">Всего заказов</div>
            </div>
            <div class="stat-card pending">
                <div class="stat-icon">⏳</div>
                <div class="stat-value">${s.pendingOrders}</div>
                <div class="stat-label">Ожидают обработки</div>
            </div>
            <div class="stat-card stock">
                <div class="stat-icon">⚠️</div>
                <div class="stat-value">${s.lowStock}</div>
                <div class="stat-label">Мало на складе</div>
            </div>
        `;
    } catch {}
}

async function loadDashOrders() {
    try {
        const orders = await api('/api/orders');
        const body = document.getElementById('dashOrdersBody');
        body.innerHTML = orders.slice(0, 10).map(o => `
            <tr>
                <td><strong>#${o.id}</strong></td>
                <td>${o.customerName}</td>
                <td>${o.partName}</td>
                <td style="font-weight:600;color:var(--accent)">${o.totalPrice.toFixed(2)}</td>
                <td><span class="status ${getStatusClass(o.status)}">${o.status}</span></td>
                <td>${new Date(o.orderDate).toLocaleDateString('ru')}</td>
            </tr>
        `).join('');
    } catch {}
}

function showDashTab(el, tab) {
    // Update sidebar
    document.querySelectorAll('[data-dash]').forEach(l => l.classList.remove('active'));
    el.classList.add('active');

    // Toggle tabs
    ['overview', 'sales', 'stock', 'users'].forEach(t => {
        const tabEl = document.getElementById(`dash-${t}`);
        if (tabEl) tabEl.style.display = t === tab ? 'block' : 'none';
    });

    // Load data
    if (tab === 'sales') loadSalesReport();
    if (tab === 'stock') loadStockReport();
    if (tab === 'users') loadUsers();
}

async function loadSalesReport() {
    try {
        const data = await api('/api/admin/reports/sales');
        document.getElementById('salesReport').innerHTML = `
            <div class="stat-card revenue" style="margin-bottom:24px">
                <div class="stat-icon">💰</div>
                <div class="stat-value">${data.totalRevenue.toFixed(2)} BYN</div>
                <div class="stat-label">Общая выручка по отгруженным заказам</div>
            </div>
            <div class="table-container">
                <div class="table-header"><h3>Отгруженные заказы</h3></div>
                <div style="overflow-x:auto">
                    <table>
                        <thead><tr><th>ID</th><th>Запчасть</th><th>Сумма</th><th>Дата</th></tr></thead>
                        <tbody>
                            ${data.orders.map(o => `
                                <tr>
                                    <td>#${o.id}</td>
                                    <td>${o.partName}</td>
                                    <td style="font-weight:600;color:var(--accent)">${o.totalPrice.toFixed(2)}</td>
                                    <td>${new Date(o.orderDate).toLocaleDateString('ru')}</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        `;
    } catch (err) {
        toast(err.message, 'error');
    }
}

async function loadStockReport() {
    try {
        const parts = await api('/api/admin/reports/stock');
        document.getElementById('stockReport').innerHTML = parts.length === 0
            ? '<div class="empty-state"><div class="icon">✅</div><p>Все товары в достаточном количестве</p></div>'
            : `<div class="table-container">
                <div class="table-header"><h3>Запчасти с критическим остатком (&lt; 10 шт.)</h3></div>
                <div style="overflow-x:auto">
                    <table>
                        <thead><tr><th>Артикул</th><th>Название</th><th>Группа</th><th>Остаток</th><th>Цена</th></tr></thead>
                        <tbody>
                            ${parts.map(p => `
                                <tr>
                                    <td><code>${p.article}</code></td>
                                    <td>${p.name}</td>
                                    <td>${p.groupName}</td>
                                    <td style="color:var(--danger);font-weight:600">${p.stock}</td>
                                    <td>${p.price.toFixed(2)}</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            </div>`;
    } catch (err) {
        toast(err.message, 'error');
    }
}

async function loadUsers() {
    try {
        const users = await api('/api/admin/users');
        const roleNames = { Admin: 'Администратор', Manager: 'Менеджер', Warehouse: 'Кладовщик', Client: 'Клиент' };
        document.getElementById('usersBody').innerHTML = users.map(u => `
            <tr>
                <td>${u.id}</td>
                <td><strong>${u.login}</strong></td>
                <td><span class="status ${u.role === 'Admin' ? 'status-shipped' : u.role === 'Manager' ? 'status-processing' : 'status-new'}">${roleNames[u.role] || u.role}</span></td>
                <td>${u.login !== 'admin' ? `<button class="btn btn-danger btn-sm" onclick="deleteUser(${u.id})">Удалить</button>` : '—'}</td>
            </tr>
        `).join('');
    } catch (err) {
        toast(err.message, 'error');
    }
}

async function submitUser(e) {
    e.preventDefault();
    try {
        await api('/api/admin/users', {
            method: 'POST',
            body: JSON.stringify({
                login: document.getElementById('newUserLogin').value,
                password: document.getElementById('newUserPass').value,
                role: document.getElementById('newUserRole').value
            })
        });
        closeModal('userModal');
        toast('Пользователь создан');
        document.getElementById('userForm').reset();
        loadUsers();
    } catch (err) {
        toast(err.message, 'error');
    }
}

async function deleteUser(id) {
    if (!confirm('Удалить пользователя?')) return;
    try {
        await api(`/api/admin/users/${id}`, { method: 'DELETE' });
        toast('Пользователь удалён');
        loadUsers();
    } catch (err) {
        toast(err.message, 'error');
    }
}

// ═══════════════════════════════════════════
// ANIMATIONS & EFFECTS
// ═══════════════════════════════════════════

// ─── Parallax ───
function initParallax() {
    window.addEventListener('scroll', () => {
        const scrolled = window.pageYOffset;
        document.querySelectorAll('.parallax-bg').forEach(el => {
            const speed = parseFloat(el.dataset.speed) || 0.5;
            el.style.transform = `translateY(${scrolled * speed}px)`;
        });
    });
}

// ─── Reveal on Scroll (IntersectionObserver) ───
function initRevealAnimations() {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('active');
            }
        });
    }, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });

    document.querySelectorAll('.reveal, .reveal-left, .reveal-right, .reveal-scale').forEach(el => {
        if (!el.classList.contains('active')) {
            observer.observe(el);
        }
    });
}

// ─── Navbar scroll effect ───
function initNavScroll() {
    window.addEventListener('scroll', () => {
        const navbar = document.getElementById('navbar');
        navbar.classList.toggle('scrolled', window.scrollY > 50);
    });
}

// ─── Scroll Float Animation (New) ───
function initScrollFloat() {
    // Target main headings only (Section H2, Dashboard H2)
    // Removed .hero h1 and .login-logo h2 to prevent disappearing text and layout issues
    const targets = document.querySelectorAll('.section-header h2, .main-content h2');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('active');
            }
        });
    }, { threshold: 0.1 });

    targets.forEach(el => {
        if (el.classList.contains('scroll-float-ready')) return;
        
        // Helper to process nodes recursively
        const processNode = (node, charCounter = 0) => {
            let count = charCounter;
            const fragment = document.createDocumentFragment();
            
            [...node.childNodes].forEach(child => {
                if (child.nodeType === Node.TEXT_NODE) {
                    const text = child.textContent;
                    [...text].forEach(char => {
                        const span = document.createElement('span');
                        span.className = 'char';
                        span.textContent = char === ' ' ? '\u00A0' : char;
                        span.style.setProperty('--char-index', count++);
                        fragment.appendChild(span);
                    });
                } else if (child.nodeType === Node.ELEMENT_NODE) {
                    const cloned = child.cloneNode(false); // Shallow clone element
                    const result = processNode(child, count);
                    cloned.appendChild(result.fragment);
                    count = result.count;
                    fragment.appendChild(cloned);
                }
            });
            return { fragment, count };
        };

        const { fragment } = processNode(el);
        el.innerHTML = '';
        el.appendChild(fragment);
        el.classList.add('scroll-float', 'scroll-float-ready');
        observer.observe(el);
    });
}

// ─── Counter animation ───
function animateCounters() {
    const counters = document.querySelectorAll('[data-count]');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting && !entry.target.dataset.counted) {
                entry.target.dataset.counted = 'true';
                const target = parseInt(entry.target.dataset.count);
                const duration = 2000;
                const step = target / (duration / 16);
                let current = 0;

                const timer = setInterval(() => {
                    current += step;
                    if (current >= target) {
                        entry.target.textContent = target + '+';
                        clearInterval(timer);
                    } else {
                        entry.target.textContent = Math.floor(current);
                    }
                }, 16);
            }
        });
    }, { threshold: 0.5 });

    counters.forEach(c => observer.observe(c));
}

// ─── Floating particles ───
function initParticles() {
    const container = document.getElementById('particles');
    if (!container) return;

    for (let i = 0; i < 30; i++) {
        const particle = document.createElement('div');
        particle.className = 'particle';
        particle.style.left = Math.random() * 100 + '%';
        particle.style.animationDuration = (Math.random() * 10 + 8) + 's';
        particle.style.animationDelay = Math.random() * 10 + 's';
        particle.style.width = (Math.random() * 4 + 2) + 'px';
        particle.style.height = particle.style.width;
        container.appendChild(particle);
    }
}

// ═══════════════════════════════════════════
// ═══════════════════════════════════════════
// INITIALIZATION
// ═══════════════════════════════════════════

document.addEventListener('DOMContentLoaded', async () => {
    initParallax();
    initNavScroll();
    initParticles();
    initRevealAnimations();
    initScrollFloat();
    animateCounters();
    
    await checkAuth();
    loadHomeProducts();
});

// ─── Navbar Search Logic ───
document.addEventListener('DOMContentLoaded', () => {
    const navSearch = document.getElementById('navSearch');
    if (navSearch) {
        navSearch.addEventListener('input', (e) => {
            const q = e.target.value;
            if (currentPage !== 'catalog') {
                navigateTo('catalog');
            }
            // Sync with catalog page search input
            const catalogSearch = document.getElementById('catalogSearch');
            if (catalogSearch) {
                catalogSearch.value = q;
                loadCatalog();
            }
        });
    }
});

// ─── Testimonials Typewriter Logic ───
const testimonialsData = [
    { text: 'Отличный сервис! Нашел редкую деталь для своей Audi за считанные минуты. Доставили вовремя.', author: 'Иван Иванов', role: 'Владелец Audi A6' },
    { text: 'Лучший магазин автозапчастей. Цены ниже, чем у конкурентов, а качество на высоте.', author: 'Алексей Соколов', role: 'Владелец VW Passat' },
    { text: 'Покупал тормозные колодки Brembo. Оригинал, все проверки прошли. Рекомендую!', author: 'Дмитрий Волков', role: 'Владелец BMW 3' },
    { text: 'Срочная доставка реально работает. Выручили, когда машина встала в самый неподходящий момент.', author: 'Максим Кузнецов', role: 'Владелец Ford Focus' },
    { text: 'Удобный интерфейс сайта. Поиск по артикулу находит все, что нужно.', author: 'Артем Морозов', role: 'Владелец Mazda 6' },
    { text: 'Профессиональные консультанты. Помогли подобрать масло и фильтры для ТО.', author: 'Игорь Новиков', role: 'Владелец Skoda Octavia' },
    { text: 'Заказывал фары на BMW. Пришли в идеальном состоянии, упакованы на совесть.', author: 'Константин Козлов', role: 'Владелец BMW 5' },
    { text: 'Большой выбор брендов. Можно найти как премиум, так и качественные аналоги.', author: 'Николай Лебедев', role: 'Владелец Mercedes E-class' },
    { text: 'Система лояльности радует. Приятно получать скидки на следующие заказы.', author: 'Виктор Павлов', role: 'Владелец Honda CR-V' },
    { text: 'Работаю с AutoParts уже год. Ни разу не подвели с качеством запчастей.', author: 'Олег Семенов', role: 'Владелец Kia Sportage' },
    { text: 'Самый быстрый подбор запчастей. Не нужно часами ждать ответа.', author: 'Михаил Голубев', role: 'Владелец Hyundai Solaris' },
    { text: 'Качество обслуживания на европейском уровне. Очень доволен покупкой.', author: 'Павел Виноградов', role: 'Владелец Volvo S60' },
    { text: 'Всегда актуальное наличие на складе. Что на сайте, то и в магазине.', author: 'Андрей Богданов', role: 'Владелец Mitsubishi Lancer' },
    { text: 'Удобно отслеживать статус заказа в личном кабинете.', author: 'Роман Воробьев', role: 'Владелец Nissan Qashqai' },
    { text: 'Запчасти для японцев всегда в наличии. Нашел все для своей Toyota.', author: 'Станислав Федоров', role: 'Владелец Toyota RAV4' },
    { text: 'Приятные цены и оперативная работа склада. Всем советую этот сервис.', author: 'Денис Щербаков', role: 'Владелец Opel Astra' },
    { text: 'Нашел оригинальный радиатор, который не мог найти в других местах. Спасибо!', author: 'Евгений Казаков', role: 'Владелец Subaru Forester' },
    { text: 'Прозрачная система оплаты и никаких скрытых платежей.', author: 'Владимир Белов', role: 'Владелец Lexus RX' },
    { text: 'Доставка в регионы работает отлично. Заказ пришел быстрее, чем ожидал.', author: 'Валентин Соловьев', role: 'Владелец Renault Duster' },
    { text: 'Надежный партнер для моего автосервиса. Заказываю запчасти только здесь.', author: 'Григорий Поляков', role: 'Техдиректор АвтоМир' }
];

async function typeText(element, text, speed = 60) {
    element.textContent = '';
    for (let i = 0; i < text.length; i++) {
        element.textContent += text[i];
        await new Promise(r => setTimeout(r, speed));
    }
}

async function deleteText(element, speed = 30) {
    let text = element.textContent;
    while (text.length > 0) {
        text = text.slice(0, -1);
        element.textContent = text;
        await new Promise(r => setTimeout(r, speed));
    }
}


async function updateTestimonials(usedIndices) {
    const cards = [0, 1, 2];
    const currentData = [];
    
    // 1. Pick 3 unique random testimonials
    const localUsed = [];
    for (let i = 0; i < 3; i++) {
        let idx;
        do {
            idx = Math.floor(Math.random() * testimonialsData.length);
        } while (localUsed.includes(idx));
        localUsed.push(idx);
        currentData.push(testimonialsData[idx]);
    }

    // 2. Set authors and roles immediately (before typing)
    cards.forEach(i => {
        document.getElementById('author-' + i).textContent = currentData[i].author;
        document.getElementById('role-' + i).textContent = currentData[i].role;
    });

    // 3. Find max length to normalize typing duration
    const maxLen = Math.max(...currentData.map(d => d.text.length));
    const typeSpeed = 60;

    // 4. Type text in synchronized steps
    for (let charIdx = 0; charIdx < maxLen; charIdx++) {
        cards.forEach(i => {
            const el = document.getElementById('typewriter-' + i);
            const fullText = currentData[i].text;
            if (charIdx < fullText.length) {
                if (charIdx === 0) el.textContent = '';
                el.textContent += fullText[charIdx];
            }
        });
        await new Promise(r => setTimeout(r, typeSpeed));
    }

    // 5. Wait for reading
    await new Promise(r => setTimeout(r, 10000));

    // 6. Delete text in synchronized steps
    for (let charIdx = 0; charIdx < maxLen; charIdx++) {
        cards.forEach(i => {
            const el = document.getElementById('typewriter-' + i);
            let text = el.textContent;
            if (text.length > 0) {
                el.textContent = text.slice(0, -1);
            }
        });
        // Constant step duration for all cards
        await new Promise(r => setTimeout(r, 30)); 
    }

    // 7. Short pause and repeat
    await new Promise(r => setTimeout(r, 1500));
    updateTestimonials(usedIndices);
}

function initTestimonials() {
    const section = document.getElementById('testimonials');
    if (!section) return;

    const observer = new IntersectionObserver((entries) => {
        if (entries[0].isIntersecting) {
            updateTestimonials(new Set());
            observer.disconnect();
        }
    }, { threshold: 0.1 });

    observer.observe(section);
}

// Ensure initTestimonials is called
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initTestimonials);
} else {
    initTestimonials();
}
