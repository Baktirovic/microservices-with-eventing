class AuditLogsDashboard {
    constructor() {
        this.logs = [];
        this.filteredLogs = [];
        this.apiBaseUrl = '/api/logs';
        this.uniqueActions = new Set();
        this.uniqueUsers = new Set();
        
        this.initializeElements();
        this.attachEventListeners();
        this.loadLogs();
    }

    initializeElements() {
        this.elements = {
            refreshBtn: document.getElementById('refreshBtn'),
            clearFiltersBtn: document.getElementById('clearFiltersBtn'),
            actionFilter: document.getElementById('actionFilter'),
            userFilter: document.getElementById('userFilter'),
            searchInput: document.getElementById('searchInput'),
            dateFrom: document.getElementById('dateFrom'),
            dateTo: document.getElementById('dateTo'),
            sortBy: document.getElementById('sortBy'),
            sortOrder: document.getElementById('sortOrder'),
            loadingIndicator: document.getElementById('loadingIndicator'),
            errorMessage: document.getElementById('errorMessage'),
            errorText: document.getElementById('errorText'),
            logsList: document.getElementById('logsList'),
            noLogsMessage: document.getElementById('noLogsMessage'),
            totalLogs: document.getElementById('totalLogs'),
            filteredLogs: document.getElementById('filteredLogs'),
            uniqueActions: document.getElementById('uniqueActions')
        };
    }

    attachEventListeners() {
        this.elements.refreshBtn.addEventListener('click', () => this.loadLogs());
        this.elements.clearFiltersBtn.addEventListener('click', () => this.clearFilters());
        this.elements.actionFilter.addEventListener('change', () => this.applyFilters());
        this.elements.userFilter.addEventListener('change', () => this.applyFilters());
        this.elements.searchInput.addEventListener('input', this.debounce(() => this.applyFilters(), 300));
        this.elements.dateFrom.addEventListener('change', () => this.applyFilters());
        this.elements.dateTo.addEventListener('change', () => this.applyFilters());
        this.elements.sortBy.addEventListener('change', () => this.applyFilters());
        this.elements.sortOrder.addEventListener('change', () => this.applyFilters());
    }

    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    async loadLogs() {
        this.showLoading();
        this.hideError();

        try {
            const response = await fetch(this.apiBaseUrl);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            this.logs = await response.json();
            this.updateUniqueActions();
            this.updateUniqueUsers();
            this.updateStats();
            this.applyFilters();
        } catch (error) {
            console.error('Error loading logs:', error);
            this.showError(`Failed to load logs: ${error.message}`);
        } finally {
            this.hideLoading();
        }
    }

    updateUniqueActions() {
        this.uniqueActions.clear();
        this.logs.forEach(log => this.uniqueActions.add(log.action));
        
        // Update action filter dropdown
        const actionFilter = this.elements.actionFilter;
        const currentValue = actionFilter.value;
        
        // Clear existing options except "All Actions"
        actionFilter.innerHTML = '<option value="">All Actions</option>';
        
        // Add unique actions
        Array.from(this.uniqueActions).sort().forEach(action => {
            const option = document.createElement('option');
            option.value = action;
            option.textContent = action;
            actionFilter.appendChild(option);
        });
        
        // Restore previous selection if it still exists
        if (this.uniqueActions.has(currentValue)) {
            actionFilter.value = currentValue;
        }
    }

    updateUniqueUsers() {
        this.uniqueUsers.clear();
        this.logs.forEach(log => {
            if (log.user && log.user.name) {
                this.uniqueUsers.add(log.user.name);
            }
        });
        
        // Update user filter dropdown
        const userFilter = this.elements.userFilter;
        const currentValue = userFilter.value;
        
        // Clear existing options except "All Users"
        userFilter.innerHTML = '<option value="">All Users</option>';
        
        // Add unique users
        Array.from(this.uniqueUsers).sort().forEach(userName => {
            const option = document.createElement('option');
            option.value = userName;
            option.textContent = userName;
            userFilter.appendChild(option);
        });
        
        // Restore previous selection if it still exists
        if (this.uniqueUsers.has(currentValue)) {
            userFilter.value = currentValue;
        }
    }

    parseUserName(fullName) {
        if (!fullName) return { firstName: 'Unknown', lastName: 'User' };
        
        const nameParts = fullName.trim().split(' ');
        if (nameParts.length === 1) {
            return { firstName: nameParts[0], lastName: '' };
        } else if (nameParts.length === 2) {
            return { firstName: nameParts[0], lastName: nameParts[1] };
        } else {
            // Handle multiple middle names or complex names
            const firstName = nameParts[0];
            const lastName = nameParts.slice(1).join(' ');
            return { firstName, lastName };
        }
    }

    applyFilters() {
        let filtered = [...this.logs];

        // Filter by action
        const actionFilter = this.elements.actionFilter.value;
        if (actionFilter) {
            filtered = filtered.filter(log => log.action === actionFilter);
        }

        // Filter by user
        const userFilter = this.elements.userFilter.value;
        if (userFilter) {
            filtered = filtered.filter(log => 
                log.user && log.user.name && log.user.name === userFilter
            );
        }

        // Filter by search text
        const searchText = this.elements.searchInput.value.toLowerCase();
        if (searchText) {
            filtered = filtered.filter(log => {
                const messageMatch = log.message.toLowerCase().includes(searchText);
                const actionMatch = log.action.toLowerCase().includes(searchText);
                
                let userMatch = false;
                if (log.user && log.user.name) {
                    const { firstName, lastName } = this.parseUserName(log.user.name);
                    userMatch = log.user.name.toLowerCase().includes(searchText) ||
                               firstName.toLowerCase().includes(searchText) ||
                               lastName.toLowerCase().includes(searchText);
                }
                
                let externalIdMatch = false;
                if (log.user && log.user.externalId) {
                    externalIdMatch = log.user.externalId.toLowerCase().includes(searchText);
                }
                
                return messageMatch || actionMatch || userMatch || externalIdMatch;
            });
        }

        // Filter by date range
        const dateFrom = this.elements.dateFrom.value;
        const dateTo = this.elements.dateTo.value;
        
        if (dateFrom) {
            const fromDate = new Date(dateFrom);
            filtered = filtered.filter(log => new Date(log.createdAt) >= fromDate);
        }
        
        if (dateTo) {
            const toDate = new Date(dateTo);
            toDate.setHours(23, 59, 59, 999); // Include the entire day
            filtered = filtered.filter(log => new Date(log.createdAt) <= toDate);
        }

        // Sort logs
        const sortBy = this.elements.sortBy.value;
        const sortOrder = this.elements.sortOrder.value;
        
        filtered.sort((a, b) => {
            let aValue, bValue;
            
            switch (sortBy) {
                case 'action':
                    aValue = a.action;
                    bValue = b.action;
                    break;
                case 'user':
                    const aName = a.user ? a.user.name : '';
                    const bName = b.user ? b.user.name : '';
                    const aParsed = this.parseUserName(aName);
                    const bParsed = this.parseUserName(bName);
                    aValue = `${aParsed.lastName}, ${aParsed.firstName}`;
                    bValue = `${bParsed.lastName}, ${bParsed.firstName}`;
                    break;
                case 'createdAt':
                default:
                    aValue = new Date(a.createdAt);
                    bValue = new Date(b.createdAt);
                    break;
            }
            
            if (sortOrder === 'asc') {
                return aValue > bValue ? 1 : -1;
            } else {
                return aValue < bValue ? 1 : -1;
            }
        });

        this.filteredLogs = filtered;
        this.updateStats();
        this.renderLogs();
    }

    renderLogs() {
        const logsList = this.elements.logsList;
        const noLogsMessage = this.elements.noLogsMessage;

        if (this.filteredLogs.length === 0) {
            logsList.innerHTML = '';
            noLogsMessage.style.display = 'block';
            return;
        }

        noLogsMessage.style.display = 'none';
        
        logsList.innerHTML = this.filteredLogs.map(log => this.createLogElement(log)).join('');
    }

    createLogElement(log) {
        const createdAt = new Date(log.createdAt);
        const formattedDate = createdAt.toLocaleDateString();
        const formattedTime = createdAt.toLocaleTimeString();
        const fullUserName = log.user ? log.user.name : 'Unknown User';
        const { firstName, lastName } = this.parseUserName(fullUserName);
        const externalId = log.user ? log.user.externalId : 'N/A';
        
        return `
            <div class="log-item" data-action="${log.action}">
                <div class="log-id">#${log.id}</div>
                <div class="log-header">
                    <span class="log-action">${log.action}</span>
                    <span class="log-timestamp">
                        <i class="fas fa-clock"></i>
                        ${formattedDate} at ${formattedTime}
                    </span>
                </div>
                <div class="log-user">
                    <i class="fas fa-user"></i>
                    <div class="user-info">
                        <div class="user-name">
                            <span class="first-name">${firstName}</span>
                            <span class="last-name">${lastName}</span>
                        </div>
                        <div class="user-external-id">
                            <i class="fas fa-id-card"></i>
                            <span class="external-id-label">External ID:</span>
                            <span class="external-id-value">${externalId}</span>
                        </div>
                    </div>
                </div>
                <div class="log-message">${this.escapeHtml(log.message)}</div>
            </div>
        `;
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    updateStats() {
        this.elements.totalLogs.textContent = this.logs.length;
        this.elements.filteredLogs.textContent = this.filteredLogs.length;
        this.elements.uniqueActions.textContent = this.uniqueActions.size;
    }

    clearFilters() {
        this.elements.actionFilter.value = '';
        this.elements.userFilter.value = '';
        this.elements.searchInput.value = '';
        this.elements.dateFrom.value = '';
        this.elements.dateTo.value = '';
        this.elements.sortBy.value = 'createdAt';
        this.elements.sortOrder.value = 'desc';
        this.applyFilters();
    }

    showLoading() {
        this.elements.loadingIndicator.style.display = 'block';
        this.elements.logsList.style.display = 'none';
        this.elements.noLogsMessage.style.display = 'none';
    }

    hideLoading() {
        this.elements.loadingIndicator.style.display = 'none';
        this.elements.logsList.style.display = 'block';
    }

    showError(message) {
        this.elements.errorText.textContent = message;
        this.elements.errorMessage.style.display = 'flex';
    }

    hideError() {
        this.elements.errorMessage.style.display = 'none';
    }
}

// Initialize the dashboard when the DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new AuditLogsDashboard();
});

// Add some utility functions for better UX
document.addEventListener('DOMContentLoaded', () => {
    // Auto-refresh every 30 seconds
    setInterval(() => {
        if (document.visibilityState === 'visible') {
            const refreshBtn = document.getElementById('refreshBtn');
            if (refreshBtn) {
                refreshBtn.click();
            }
        }
    }, 30000);

    // Add keyboard shortcuts
    document.addEventListener('keydown', (e) => {
        // Ctrl/Cmd + R to refresh
        if ((e.ctrlKey || e.metaKey) && e.key === 'r') {
            e.preventDefault();
            const refreshBtn = document.getElementById('refreshBtn');
            if (refreshBtn) {
                refreshBtn.click();
            }
        }
        
        // Ctrl/Cmd + F to focus search
        if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
            e.preventDefault();
            const searchInput = document.getElementById('searchInput');
            if (searchInput) {
                searchInput.focus();
            }
        }
    });
});

