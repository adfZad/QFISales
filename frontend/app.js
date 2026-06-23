/* ==========================================================================
   Sales Customer Order Memo - Core Application Engine (Decoupled API Client)
   ========================================================================== */

// 1. DYNAMIC API CONFIGURATION
const API_BASE_URL = (window.location.protocol === 'file:' || window.location.hostname === '127.0.0.1') ? "http://localhost:5000/api" : "/api";
let isBackendConnected = false;

// Offline Fallback Masters (Used if backend server is unreachable)
let MATERIAL_MASTER = []; // Loaded from API or populated by default fallback
const FALLBACK_MATERIALS = [
    { no: 1, description: "FLOUR NO 1 (BLUE)", code: "5001", packing: "1*50KG", defaultPrice: 140 },
    { no: 2, description: "FLOUR NO 1 (BLACK)", code: "5002", packing: "1*50KG", defaultPrice: 138 },
    { no: 3, description: "FLOUR NO 1 (RED)", code: "5003", packing: "1*50KG", defaultPrice: 140 },
    { no: 4, description: "FLOUR NO 2 (PURPLE)", code: "5004", packing: "1*50KG", defaultPrice: 140 },
    { no: 5, description: "FLOUR NO 2 (GREEN)", code: "5005", packing: "1*50KG", defaultPrice: 138 },
    { no: 6, description: "FLOUR NO 3 (BROWN)", code: "5006", packing: "1*50KG", defaultPrice: 140 },
    { no: 7, description: "FLOUR NO 1- MALABAR PRT", code: "5015", packing: "1*50KG", defaultPrice: 125 },
    { no: 8, description: "FLOUR NO 1 -PIZZA FLR", code: "5008", packing: "1*50KG", defaultPrice: 155 },
    { no: 9, description: "FINE BRAN 1*10kg", code: "312", packing: "1*10kg", defaultPrice: 0 },
    { no: 10, description: "FINE BRAN 1*30kg", code: "335", packing: "1*30kg", defaultPrice: 0 },
    { no: 11, description: "saudi suger", code: "1272", packing: "1*50kg", defaultPrice: 0 },
    { no: 12, description: "QFM SUGAR", code: "1251", packing: "1*50KG", defaultPrice: 0 },
    { no: 13, description: "ZAIN CORN OIL", code: "1306", packing: "6*1.8ltr", defaultPrice: 0 },
    { no: 14, description: "AL RAI SUNFLOWER OIL", code: "1302", packing: "6*1.8ltr", defaultPrice: 0 },
    { no: 15, description: "ZAIN SUNFLOWER OIL", code: "1305", packing: "6*1.8ltr", defaultPrice: 0 },
    { no: 16, description: "ZAIN CORN OIL 5ltr", code: "1356", packing: "4*5ltr", defaultPrice: 0 },
    { no: 17, description: "ZAIN SUNFLOWER OIL 5 ltr", code: "1354", packing: "4*5ltr", defaultPrice: 0 },
    { no: 18, description: "ZAIN PALM OIL", code: "1418", packing: "18LTR", defaultPrice: 0 },
    { no: 19, description: "AL RAI PALM OIL", code: "1385", packing: "18LTR", defaultPrice: 0 },
    { no: 20, description: "YARA GHEE", code: "1373", packing: "16LTR", defaultPrice: 0 },
    { no: 21, description: "ZAIN BAKER FLOUR", code: "5010", packing: "1*50KG", defaultPrice: 110 },
    { no: 22, description: "ZAIN FLOUR NO 1(BLACK)", code: "5011", packing: "1*50KG", defaultPrice: 110 },
    { no: 23, description: "ZAIN FLOUR NO 2(GREEN)", code: "5012", packing: "1*50KG", defaultPrice: 110 },
    { no: 24, description: "CHAKKI ATTA 50kg", code: "1108", packing: "1*50KG", defaultPrice: 130 },
    { no: 25, description: "SEMOLINA ROUGH", code: "303", packing: "1*25KG", defaultPrice: 80 },
    { no: 26, description: "SEMOLINA SOFT", code: "304", packing: "1*25KG", defaultPrice: 80 },
    { no: 27, description: "DAWI SUNFLOWER", code: "1423", packing: "5LTR", defaultPrice: 0 },
    { no: 28, description: "CHAKKI ATTA 10kg", code: "1118", packing: "1*10KG", defaultPrice: 28.5 }
];

const FALLBACK_CUSTOMERS = [
    { code: "C41699", name: "Thamam Trading" },
    { code: "C10042", name: "Jawad & Sons" },
    { code: "C90481", name: "Qatar Cooperative Society" },
    { code: "C30489", name: "Al Meera Consumer Goods" },
    { code: "C49021", name: "Doha Food Distributors" },
    { code: "C12849", name: "Gulf Hypermarket" },
    { code: "C69104", name: "Oasis Logistics" }
];

// 2. APPLICATION STATE
let currentSalesperson = "";
let currentSalespersonCode = "";
let currentPriceType = "REGULAR";
let orders = [];
let currentEditingId = null;
let deleteTargetId = null;
let pendingOrderPayload = null;

// ==========================================
// DOM ELEMENTS REFERENCE
// ==========================================
const viewDashboard = document.getElementById("viewDashboard");
const viewOrderForm = document.getElementById("viewOrderForm");
const viewMemo = document.getElementById("viewMemo");
const viewMappings = document.getElementById("viewMappings");

// Login Elements
const loginModal = document.getElementById("loginModal");
const loginSalespersonSelect = document.getElementById("loginSalespersonSelect");
const btnLogin = document.getElementById("btnLogin");
const userProfile = document.getElementById("userProfile");
const lblLoggedUser = document.getElementById("lblLoggedUser");
const lblLoggedRoute = document.getElementById("lblLoggedRoute");
const btnLogout = document.getElementById("btnLogout");
const btnChangePassword = document.getElementById("btnChangePassword");
const changePasswordModal = document.getElementById("changePasswordModal");
const btnSubmitChangePassword = document.getElementById("btnSubmitChangePassword");

// Mappings Elements
const btnManageMappings = document.getElementById("btnManageMappings");
const mappingCustomerInput = document.getElementById("mappingCustomerInput");
const mappingCustomerCode = document.getElementById("mappingCustomerCode");
const mappingCustomerSuggestions = document.getElementById("mappingCustomerSuggestions");
const mappingSalespersonInput = document.getElementById("mappingSalespersonInput");
const mappingSalesPNCode = document.getElementById("mappingSalesPNCode");
const mappingSalespersonSuggestions = document.getElementById("mappingSalespersonSuggestions");
const btnAddMapping = document.getElementById("btnAddMapping");
const mappingsTableBody = document.getElementById("mappingsTableBody");

const btnAddNewOrder = document.getElementById("btnAddNewOrder");
const btnResetForm = document.getElementById("btnResetForm");
const btnSaveOrder = document.getElementById("btnSaveOrder");

const searchOrdersInput = document.getElementById("searchOrdersInput");
const dateFilterFrom = document.getElementById("dateFilterFrom");
const dateFilterTo = document.getElementById("dateFilterTo");
const btnResetDashboardFilters = document.getElementById("btnResetDashboardFilters");
const ordersTableBody = document.getElementById("ordersTableBody");
const materialsFormBody = document.getElementById("materialsFormBody");
const searchMaterialInput = document.getElementById("searchMaterialInput");
const materialGroupFilter = document.getElementById("materialGroupFilter");
const orderEntryForm = document.getElementById("orderEntryForm");

// Form Inputs
const orderDateInput = document.getElementById("orderDate");
const customerNameInput = document.getElementById("customerName");
const customerCodeInput = document.getElementById("customerCode");
const salesPersonInput = document.getElementById("salesPerson");
const salesPNCodeInput = document.getElementById("salesPNCode");
const salespersonSuggestions = document.getElementById("salespersonSuggestions");

// Dashboard Stats elements
const statTotalOrders = document.getElementById("statTotalOrders");
const statTotalValue = document.getElementById("statTotalValue");
const statCreditOrders = document.getElementById("statCreditOrders");
const statCashOrders = document.getElementById("statCashOrders");

// Form dynamic totals elements
const totalItemsCount = document.getElementById("totalItemsCount");
const totalQtySum = document.getElementById("totalQtySum");
const totalFocSum = document.getElementById("totalFocSum");
const netAmountSum = document.getElementById("netAmountSum");

// View Memo elements
const viewCustomerName = document.getElementById("viewCustomerName");
const viewCustomerCode = document.getElementById("viewCustomerCode");
const viewPaymentMode = document.getElementById("viewPaymentMode");
const viewMemoNumber = document.getElementById("viewMemoNumber");
const viewOrderDate = document.getElementById("viewOrderDate");
const viewSalesPerson = document.getElementById("viewSalesPerson");
const viewSalesPNCode = document.getElementById("viewSalesPNCode");
const viewInvoiceItemsBody = document.getElementById("viewInvoiceItemsBody");
const viewTotalQty = document.getElementById("viewTotalQty");
const viewTotalFoc = document.getElementById("viewTotalFoc");
const viewNetAmount = document.getElementById("viewNetAmount");

// Modal Elements
const deleteConfirmModal = document.getElementById("deleteConfirmModal");
const deleteConfirmCustName = document.getElementById("deleteConfirmCustName");
const btnConfirmDelete = document.getElementById("btnConfirmDelete");

// ==========================================
// INITIALIZATION & DUAL-FETCH CONNECTIVITY
// ==========================================
document.addEventListener("DOMContentLoaded", async () => {
    startLiveClock();
    await checkApiConnectivity();
    await initApp();
    setupEventListeners();
});

// Live clock: updates every second showing date + time
function startLiveClock() {
    const clockEl = document.getElementById("liveClock");
    if (!clockEl) return;
    function tick() {
        const now = new Date();
        const dd   = String(now.getDate()).padStart(2, '0');
        const mm   = String(now.getMonth() + 1).padStart(2, '0');
        const yyyy = now.getFullYear();
        const hh   = String(now.getHours()).padStart(2, '0');
        const min  = String(now.getMinutes()).padStart(2, '0');
        const ss   = String(now.getSeconds()).padStart(2, '0');
        clockEl.textContent = `${dd}/${mm}/${yyyy}  ${hh}:${min}:${ss}`;
    }
    tick();
    setInterval(tick, 1000);
}

// Checks if the C# Web API is running on localhost:5000
async function checkApiConnectivity() {
    try {
        const res = await fetch(`${API_BASE_URL}/materials`, { method: 'GET', signal: AbortSignal.timeout(1500) });
        if (res.ok) {
            isBackendConnected = true;
            console.log("Connected to .NET 10 Web API Backend successfully.");
        } else {
            throw new Error("HTTP error");
        }
    } catch {
        isBackendConnected = false;
        console.warn("Could not connect to Web API backend at localhost:5000. Running in resilient LocalStorage fallback mode.");
    }
}

// Main initial load
async function initApp() {
    // 1. Fetch Dynamic Materials catalog
    if (isBackendConnected) {
        try {
            const res = await fetch(`${API_BASE_URL}/materials`);
            MATERIAL_MASTER = await res.json();
        } catch {
            MATERIAL_MASTER = FALLBACK_MATERIALS;
        }
    } else {
        MATERIAL_MASTER = FALLBACK_MATERIALS;
    }

    // 2. Set default date to today
    const today = new Date().toISOString().split('T')[0];
    orderDateInput.value = today;

    // 3. Render material form rows
    renderMaterialFormRows();

    // 4. Load saved orders and render dashboard
    await loadOrdersFromSource();
    
    // 5. Initialize Login Flow
    await initLoginFlow();
}

async function initLoginFlow() {
    const loggedCode = localStorage.getItem("loggedSalespersonCode");
    const loggedName = localStorage.getItem("loggedSalespersonName");

    const loggedRoute = localStorage.getItem("loggedSalespersonRoute") || "";

    if (loggedCode && loggedName) {
        completeLogin(loggedCode, loggedName, loggedRoute);
    } else {
        // Show login modal
        loginModal.classList.add("active");
        
        // Removed dropdown fetch logic
    }

    btnLogin.addEventListener("click", async () => {
        const empCode = document.getElementById("loginEmpCode").value.trim();
        const password = document.getElementById("loginPassword").value.trim();
        
        if (!empCode || !password) {
            alert("Please enter both ERP EMP CODE and Password");
            return;
        }
        
        btnLogin.disabled = true;
        btnLogin.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Authenticating...';

        try {
            const res = await fetch(`${API_BASE_URL}/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ empCode, password })
            });

            if (res.ok) {
                const profile = await res.json();
                localStorage.setItem("loggedSalespersonCode", profile.code);
                localStorage.setItem("loggedSalespersonName", profile.name);
                localStorage.setItem("loggedUserType", profile.userType);
                localStorage.setItem("loggedSupervisorCode", profile.supervisorCode || "");
                localStorage.setItem("loggedSalespersonRoute", profile.route || "");
                completeLogin(profile.code, profile.name, profile.route || "");
            } else {
                alert("Invalid ERP EMP CODE or Password!");
            }
        } catch (ex) {
            console.error("Login failed", ex);
            alert("Connection error. Could not authenticate.");
        } finally {
            btnLogin.disabled = false;
            btnLogin.innerHTML = '<i class="fa-solid fa-right-to-bracket"></i> Login to System';
        }
    });

    btnLogout.addEventListener("click", () => {
        localStorage.removeItem("loggedSalespersonCode");
        localStorage.removeItem("loggedSalespersonName");
        localStorage.removeItem("loggedUserType");
        localStorage.removeItem("loggedSupervisorCode");
        localStorage.removeItem("loggedSalespersonRoute");
        location.reload();
    });

    if (btnChangePassword) {
        btnChangePassword.addEventListener("click", () => {
            changePasswordModal.classList.add("active");
            document.getElementById("changePasswordForm").reset();
            document.getElementById("changePasswordError").style.display = "none";
            document.getElementById("changePasswordSuccess").style.display = "none";
        });
    }

    if (btnSubmitChangePassword) {
        btnSubmitChangePassword.addEventListener("click", async () => {
            const currentPassword = document.getElementById("currentPassword").value;
            const newPassword = document.getElementById("newPassword").value;
            const confirmNewPassword = document.getElementById("confirmNewPassword").value;
            const errorDiv = document.getElementById("changePasswordError");
            const successDiv = document.getElementById("changePasswordSuccess");
            
            errorDiv.style.display = "none";
            successDiv.style.display = "none";
            
            if (!currentPassword || !newPassword || !confirmNewPassword) {
                errorDiv.textContent = "All fields are required.";
                errorDiv.style.display = "block";
                return;
            }
            
            if (newPassword !== confirmNewPassword) {
                errorDiv.textContent = "New passwords do not match.";
                errorDiv.style.display = "block";
                return;
            }
            
            const empCode = localStorage.getItem("loggedSalespersonCode");
            if (!empCode) {
                errorDiv.textContent = "You must be logged in.";
                errorDiv.style.display = "block";
                return;
            }
            
            btnSubmitChangePassword.disabled = true;
            btnSubmitChangePassword.textContent = "Updating...";
            
            try {
                const res = await fetch(`${API_BASE_URL}/change-password`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        empCode: empCode,
                        currentPassword: currentPassword,
                        newPassword: newPassword
                    })
                });
                
                if (res.ok) {
                    successDiv.style.display = "block";
                    setTimeout(() => {
                        changePasswordModal.classList.remove("active");
                        document.getElementById("changePasswordForm").reset();
                        successDiv.style.display = "none";
                    }, 1500);
                } else if (res.status === 401) {
                    errorDiv.textContent = "Incorrect current password.";
                    errorDiv.style.display = "block";
                } else {
                    errorDiv.textContent = "An error occurred while updating the password.";
                    errorDiv.style.display = "block";
                }
            } catch (ex) {
                errorDiv.textContent = "Connection error. Please try again later.";
                errorDiv.style.display = "block";
            } finally {
                btnSubmitChangePassword.disabled = false;
                btnSubmitChangePassword.textContent = "Update Password";
            }
        });
    }
}

function completeLogin(code, name, route) {
    loginModal.classList.remove("active");
    document.getElementById("appContainer").style.display = "block";
    userProfile.style.display = "flex";
    lblLoggedUser.textContent = `${name} (${code})`;
    if (lblLoggedRoute) {
        lblLoggedRoute.textContent = route ? route : "No Route Assigned";
    }
    
    // Reload orders and customer dropdown now that the user context is established
    loadOrdersFromSource();
    populateCustomerDropdown(code);
}

async function populateCustomerDropdown(spCode) {
    try {
        const response = await fetch(`${API_BASE_URL}/customers?salesPersonCode=${encodeURIComponent(spCode)}`);
        if (response.ok) {
            const customers = await response.json();
            customerNameInput.innerHTML = '<option value="">-- Select Customer --</option>';
            customers.forEach(c => {
                const opt = document.createElement("option");
                opt.value = c.name;
                opt.textContent = `${c.name} (${c.code})`;
                opt.dataset.code = c.code;
                opt.dataset.priceType = c.priceType || "REGULAR";
                customerNameInput.appendChild(opt);
            });
        }
    } catch (e) {
        console.error("Error loading customers for dropdown", e);
    }
}

// Load orders from API or LocalStorage
async function loadOrdersFromSource() {
    if (isBackendConnected) {
        try {
            const loggedSalespersonCode = localStorage.getItem("loggedSalespersonCode") || "";
            const loggedUserType = localStorage.getItem("loggedUserType") || "Salesman";
            const spParam = loggedSalespersonCode ? `?salesPersonCode=${encodeURIComponent(loggedSalespersonCode)}&userType=${encodeURIComponent(loggedUserType)}` : "";
            const res = await fetch(`${API_BASE_URL}/orders${spParam}`);
            orders = await res.json();
        } catch {
            loadOrdersFromLocalStorage();
        }
    } else {
        loadOrdersFromLocalStorage();
    }
    updateDashboard();
}

function loadOrdersFromLocalStorage() {
    const storedOrders = localStorage.getItem("qfi_sales_orders");
    if (storedOrders) {
        orders = JSON.parse(storedOrders);
    } else {
        // Sample order pre-population
        const sampleOrder = {
            id: "ord-100302-0001",
            memoNumber: "100302-0001",
            date: "2026-05-20",
            customerName: "Thamam Trading",
            customerCode: "C41699",
            paymentMode: "CREDIT",
            salesPerson: "Jawed Akthar",
            salesPNCode: "100302",
            totalQty: 10,
            totalFoc: 0,
            totalAmount: 1030.00,
            isCreditVerified: true,
            items: [
                {
                    no: 18,
                    description: "ZAIN PALM OIL",
                    code: "1418",
                    packing: "18LTR",
                    qty: 10,
                    foc: 0,
                    unitPrice: 103.00,
                    totalPrice: 1030.00,
                    remarks: "Handwritten code change: 1425"
                }
            ]
        };
        orders = [sampleOrder];
        localStorage.setItem("qfi_sales_orders", JSON.stringify(orders));
    }
}

// ==========================================
// EVENT LISTENERS SETUP
// ==========================================
function setupEventListeners() {
    // Views Navigation
    btnAddNewOrder.addEventListener("click", () => {
        // Enforce the locked salesperson when creating an order
        const code = localStorage.getItem("loggedSalespersonCode") || "100302";
        const name = localStorage.getItem("loggedSalespersonName") || "Jawed Akthar";
        salesPNCodeInput.value = code;
        salesPersonInput.value = name;
        showView("form");
    });
    
    btnManageMappings.addEventListener("click", () => {
        showView("mappings");
        loadMappingsTable();
    });

    document.querySelectorAll(".btnBackToDashboard").forEach(btn => {
        btn.addEventListener("click", () => showView("dashboard"));
    });

    // Customer dropdown change listener
    customerNameInput.addEventListener("change", (e) => {
        const selectedOption = customerNameInput.options[customerNameInput.selectedIndex];
        if (selectedOption && selectedOption.dataset.code) {
            customerCodeInput.value = selectedOption.dataset.code;
            currentPriceType = selectedOption.dataset.priceType || "REGULAR";
        } else {
            customerCodeInput.value = "";
            currentPriceType = "REGULAR";
        }
        updateCatalogPricing(currentPriceType);
    });

    salesPersonInput.addEventListener("input", debounce(handleSalespersonNameInput, 150));
    document.addEventListener("click", (e) => {
        if (e.target !== salesPersonInput && e.target !== salespersonSuggestions) {
            salespersonSuggestions.style.display = "none";
        }
    });

    // Reset Form button
    btnResetForm.addEventListener("click", resetForm);

    // Form submit
    orderEntryForm.addEventListener("submit", handleFormSubmit);

    // Search dashboard
    searchOrdersInput.addEventListener("input", handleDashboardSearch);
    dateFilterFrom.addEventListener("change", handleDashboardSearch);
    dateFilterTo.addEventListener("change", handleDashboardSearch);
    
    if (btnResetDashboardFilters) {
        btnResetDashboardFilters.addEventListener("click", () => {
            searchOrdersInput.value = "";
            dateFilterFrom.value = "";
            dateFilterTo.value = "";
            handleDashboardSearch();
        });
    }

    // Search and filter materials inside order form
    const filterMaterials = () => {
        const query = searchMaterialInput ? searchMaterialInput.value.trim().toLowerCase() : "";
        const group = materialGroupFilter ? materialGroupFilter.value : "";
        const rows = materialsFormBody.querySelectorAll("tr");
        
        rows.forEach(row => {
            const desc = row.cells[1].textContent.toLowerCase();
            const code = row.cells[2].textContent.toLowerCase();
            const rowGroup = row.dataset.group || "";
            
            const matchesSearch = desc.includes(query) || code.includes(query);
            
            let matchesGroup = true;
            if (group === "_SELECTED_ITEMS_") {
                const qty = parseInt(row.querySelector(".qty-input").value) || 0;
                const foc = parseInt(row.querySelector(".foc-input").value) || 0;
                matchesGroup = (qty > 0 || foc > 0);
            } else {
                matchesGroup = group === "" || rowGroup === group;
            }
            
            if (matchesSearch && matchesGroup) {
                row.style.display = "";
            } else {
                row.style.display = "none";
            }
        });
    };

    if (searchMaterialInput) {
        searchMaterialInput.addEventListener("input", filterMaterials);
    }
    if (materialGroupFilter) {
        materialGroupFilter.addEventListener("change", filterMaterials);
    }


    // Modal Close
    document.querySelectorAll(".btnCloseModal").forEach(btn => {
        btn.addEventListener("click", closeDeleteModal);
    });

    btnConfirmDelete.addEventListener("click", deleteOrderConfirmed);

    // Review Modal Close & Confirm
    document.querySelectorAll(".btnCloseReviewModal").forEach(btn => {
        btn.addEventListener("click", () => document.getElementById("reviewOrderModal").classList.remove("active"));
    });
    
    const btnConfirmReviewSave = document.getElementById("btnConfirmReviewSave");
    if (btnConfirmReviewSave) {
        btnConfirmReviewSave.addEventListener("click", executeOrderSave);
    }
}

// Lightweight debounce utility to optimize API requests on keyboard inputs
function debounce(fn, delay) {
    let timeout;
    return function (...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => fn.apply(this, args), delay);
    };
}

// ==========================================
// RENDER DATA FUNCTIONS
// ==========================================
function updateCatalogPricing(priceType) {
    MATERIAL_MASTER.forEach(material => {
        // Find correct pricing
        let activePrice = material.defaultPrice;
        let activeUOM = material.packing;
        
        if (material.prices && material.prices[priceType]) {
            activePrice = material.prices[priceType].price;
            activeUOM = material.prices[priceType].uom || material.packing;
        } else if (material.prices && material.prices["REGULAR"]) {
            activePrice = material.prices["REGULAR"].price;
            activeUOM = material.prices["REGULAR"].uom || material.packing;
        }
        
        // Update DOM elements if they exist
        const row = document.getElementById(`row-${material.no}`);
        if (row) {
            row.dataset.packing = activeUOM;
            const uomCell = row.querySelector('.col-pack');
            if (uomCell) uomCell.textContent = activeUOM;
            
            const priceCell = row.querySelector('.col-price');
            if (priceCell) priceCell.textContent = activePrice.toFixed(2);
        }
    });
    // Recalculate totals for items already in cart
    calculateTotals();
}
function renderMaterialFormRows() {
    materialsFormBody.innerHTML = "";
    
    if (materialGroupFilter && MATERIAL_MASTER.length > 0) {
        const currentSelection = materialGroupFilter.value;
        materialGroupFilter.innerHTML = '<option value="">All Groups</option><option value="_SELECTED_ITEMS_">Selected Items</option>';
        const groups = [...new Set(MATERIAL_MASTER.map(m => m.group).filter(g => g))].sort();
        groups.forEach(g => {
            const opt = document.createElement("option");
            opt.value = g;
            opt.textContent = g;
            materialGroupFilter.appendChild(opt);
        });
        materialGroupFilter.value = currentSelection;
    }

    MATERIAL_MASTER.forEach(material => {
        let activePrice = material.defaultPrice;
        let activeUOM = material.packing;
        if (material.prices && material.prices[currentPriceType]) {
            activePrice = material.prices[currentPriceType].price;
            activeUOM = material.prices[currentPriceType].uom || material.packing;
        } else if (material.prices && material.prices["REGULAR"]) {
            activePrice = material.prices["REGULAR"].price;
            activeUOM = material.prices["REGULAR"].uom || material.packing;
        }

        const row = document.createElement("tr");
        row.id = `row-${material.no}`;
        row.dataset.no = material.no;
        row.dataset.code = material.code;
        row.dataset.packing = activeUOM;
        row.dataset.desc = material.description;
        row.dataset.group = material.group || "";
        row.dataset.salesGroup = material.salesGroup || "";
        
        row.innerHTML = `
            <td class="col-no text-center">${material.no}</td>
            <td class="col-desc" title="${material.description}"><strong>${material.description}</strong></td>
            <td class="col-code text-center">${material.code}</td>
            <td class="col-pack text-center">${activeUOM}</td>
            <td class="col-qty">
                <input type="number" min="0" class="form-control qty-input" style="text-align: center;" placeholder="0" data-no="${material.no}">
            </td>
            <td class="col-foc">
                <div style="display: flex; align-items: center; justify-content: center; gap: 6px;">
                    <input type="checkbox" class="foc-enable-chk" data-no="${material.no}" title="Enable FOC">
                    <input type="number" min="0" class="form-control foc-input" style="text-align: center;" placeholder="0" data-no="${material.no}" readonly>
                </div>
            </td>
            <td class="col-price text-center">${activePrice.toFixed(2)}</td>
            <td class="col-total" id="total-${material.no}">QAR 0.00</td>
            <td class="col-remarks">
                <input type="text" class="form-control remarks-input" placeholder="Line remarks..." data-no="${material.no}">
            </td>
        `;
        
        const qtyIn = row.querySelector(".qty-input");
        const focIn = row.querySelector(".foc-input");
        const focChk = row.querySelector(".foc-enable-chk");
        
        focChk.addEventListener("change", (e) => {
            if (e.target.checked) {
                focIn.removeAttribute("readonly");
                focIn.focus();
            } else {
                focIn.setAttribute("readonly", "readonly");
                focIn.value = "";
                calculateRowAndFormTotals(material.no);
            }
        });
        
        [qtyIn, focIn].forEach(input => {
            input.addEventListener("input", () => calculateRowAndFormTotals(material.no));
        });
        
        materialsFormBody.appendChild(row);
    });
}

// ==========================================
// DYNAMIC CALCULATIONS & FORM LOGIC
// ==========================================
function calculateTotals() {
    MATERIAL_MASTER.forEach(m => calculateRowAndFormTotals(m.no));
}

function calculateRowAndFormTotals(itemNo) {
    const row = document.getElementById(`row-${itemNo}`);
    const qty = parseInt(row.querySelector(".qty-input").value) || 0;
    const price = parseFloat(row.querySelector(".col-price").textContent) || 0;
    
    const rowTotal = qty * price;
    document.getElementById(`total-${itemNo}`).textContent = rowTotal > 0 ? `QAR ${rowTotal.toFixed(2)}` : "QAR 0.00";
    
    if (qty > 0 || parseInt(row.querySelector(".foc-input").value) > 0) {
        row.classList.add("active-row");
    } else {
        row.classList.remove("active-row");
    }
    
    let grandTotal = 0;
    let totalQty = 0;
    let totalFoc = 0;
    let itemsCount = 0;
    
    MATERIAL_MASTER.forEach(m => {
        const r = document.getElementById(`row-${m.no}`);
        const q = parseInt(r.querySelector(".qty-input").value) || 0;
        const f = parseInt(r.querySelector(".foc-input").value) || 0;
        const p = parseFloat(r.querySelector(".col-price").textContent) || 0;
        
        if (q > 0 || f > 0) {
            itemsCount++;
            totalQty += q;
            totalFoc += f;
            grandTotal += q * p;
        }
    });
    
    totalItemsCount.textContent = itemsCount;
    totalQtySum.textContent = totalQty;
    totalFocSum.textContent = totalFoc;
    netAmountSum.textContent = grandTotal.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

/// Customer Autocomplete removed in favor of Dropdown.

// Handles Salesperson Autocomplete via dynamic API search
async function handleSalespersonNameInput(e, inputElem, codeElem, suggestionsElem) {
    const val = e.target.value.trim().toLowerCase();
    suggestionsElem.innerHTML = "";
    suggestionsElem.style.display = "none";
    codeElem.value = ""; 

    if (val.length < 1) return;

    let matches = [];
    if (isBackendConnected) {
        try {
            const res = await fetch(`${API_BASE_URL}/salespersons?query=${encodeURIComponent(val)}`);
            matches = await res.json();
        } catch {
            matches = [{ code: "100302", name: "Jawed Akthar" }].filter(s => s.name.toLowerCase().includes(val));
        }
    } else {
        matches = [{ code: "100302", name: "Jawed Akthar" }].filter(s => s.name.toLowerCase().includes(val));
    }

    if (matches.length === 0) {
        const suggestion = document.createElement("div");
        suggestion.className = "autocomplete-suggestion";
        suggestion.textContent = "No salesperson found";
        suggestionsElem.appendChild(suggestion);
        suggestionsElem.style.display = "block";
        return;
    }

    matches.forEach(match => {
        const suggestion = document.createElement("div");
        suggestion.className = "autocomplete-suggestion";
        suggestion.innerHTML = `<strong>${match.name}</strong> <span style="float:right; font-size:11px; color:var(--text-muted);">${match.code}</span>`;
        suggestion.addEventListener("click", () => {
            inputElem.value = match.name;
            codeElem.value = match.code;
            suggestionsElem.style.display = "none";
        });
        suggestionsElem.appendChild(suggestion);
    });
    suggestionsElem.style.display = "block";
}

// ==========================================
// MAPPINGS MANAGEMENT LOGIC
// ==========================================
async function loadMappingsTable() {
    mappingsTableBody.innerHTML = `<tr><td colspan="6" style="text-align:center;"><i class="fa-solid fa-spinner fa-spin"></i> Loading mappings...</td></tr>`;
    if (!isBackendConnected) {
        mappingsTableBody.innerHTML = `<tr><td colspan="6" style="text-align:center;">Mappings require backend connection.</td></tr>`;
        return;
    }
    
    try {
        const res = await fetch(`${API_BASE_URL}/mappings`);
        const mappings = await res.json();
        
        mappingsTableBody.innerHTML = "";
        if (mappings.length === 0) {
            mappingsTableBody.innerHTML = `<tr><td colspan="6" style="text-align:center;">No mappings found.</td></tr>`;
            return;
        }

        mappings.forEach(m => {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${m.mappingID}</td>
                <td><span class="badge badge-neutral">${m.customerCode}</span></td>
                <td><strong>${m.customerName}</strong></td>
                <td><span class="badge badge-warning">${m.salesPNCode}</span></td>
                <td>${m.salespersonName}</td>
                <td style="text-align:center;">
                    <button type="button" style="background: transparent; border: none; color: #555; box-shadow: none; cursor: pointer; padding: 4px;" onclick="deleteMapping(${m.mappingID})" title="Delete Mapping">
                        <i class="fa-solid fa-trash" style="font-size: 13px;"></i>
                    </button>
                </td>
            `;
            mappingsTableBody.appendChild(tr);
        });
    } catch (err) {
        mappingsTableBody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:red;">Error loading mappings.</td></tr>`;
    }
}

async function addMapping() {
    const cCode = mappingCustomerCode.value;
    const sCode = mappingSalesPNCode.value;

    if (!cCode || !sCode) {
        alert("Please select both a Customer and a Salesperson from the suggestions.");
        return;
    }

    try {
        const res = await fetch(`${API_BASE_URL}/mappings`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ CustomerCode: cCode, SalesPNCode: sCode })
        });

        if (res.ok) {
            mappingCustomerInput.value = "";
            mappingCustomerCode.value = "";
            mappingSalespersonInput.value = "";
            mappingSalesPNCode.value = "";
            loadMappingsTable();
        } else {
            const err = await res.text();
            alert("Failed to add mapping: " + err);
        }
    } catch (err) {
        alert("Error connecting to server.");
    }
}

async function deleteMapping(id) {
    if (!confirm("Are you sure you want to delete this mapping?")) return;
    
    try {
        const res = await fetch(`${API_BASE_URL}/mappings/${id}`, { method: "DELETE" });
        if (res.ok) {
            loadMappingsTable();
        } else {
            alert("Failed to delete mapping.");
        }
    } catch (err) {
        alert("Error connecting to server.");
    }
}

// Setup Event Listeners for mappings autocompletes
mappingCustomerInput.addEventListener("input", (e) => {
    // For mapping, we want ALL customers, so don't pass salesPersonCode
    const val = e.target.value.trim().toLowerCase();
    mappingCustomerSuggestions.innerHTML = "";
    mappingCustomerSuggestions.style.display = "none";
    mappingCustomerCode.value = ""; 

    if (val.length < 1) return;

    fetch(`${API_BASE_URL}/customers?query=${encodeURIComponent(val)}`)
        .then(res => res.json())
        .then(matches => {
            if (matches.length === 0) return;
            matches.forEach(match => {
                const suggestion = document.createElement("div");
                suggestion.className = "autocomplete-suggestion";
                suggestion.innerHTML = `<strong>${match.name}</strong> <span style="float:right; font-size:11px; color:var(--text-muted);">${match.code}</span>`;
                suggestion.addEventListener("click", () => {
                    mappingCustomerInput.value = match.name;
                    mappingCustomerCode.value = match.code;
                    mappingCustomerSuggestions.style.display = "none";
                });
                mappingCustomerSuggestions.appendChild(suggestion);
            });
            mappingCustomerSuggestions.style.display = "block";
        });
});

mappingSalespersonInput.addEventListener("input", (e) => {
    handleSalespersonNameInput(e, mappingSalespersonInput, mappingSalesPNCode, mappingSalespersonSuggestions);
});

btnAddMapping.addEventListener("click", addMapping);



function resetForm() {
    orderEntryForm.reset();
    
    const today = new Date().toISOString().split('T')[0];
    orderDateInput.value = today;
    document.getElementById("requiredDate").value = today;
    document.getElementById("referenceNumber").value = "";
    customerCodeInput.value = "";
    salesPersonInput.value = localStorage.getItem("loggedSalespersonName") || "";
    salesPNCodeInput.value = localStorage.getItem("loggedSalespersonCode") || "";
    currentPriceType = "REGULAR";
    
    // Reset pricing/uom to default
    updateCatalogPricing("REGULAR");
    
    MATERIAL_MASTER.forEach(m => {
        const r = document.getElementById(`row-${m.no}`);
        r.querySelector(".qty-input").value = "";
        const focIn = r.querySelector(".foc-input");
        focIn.value = "";
        focIn.setAttribute("readonly", "readonly");
        r.querySelector(".foc-enable-chk").checked = false;
        r.querySelector(".remarks-input").value = "";
        r.classList.remove("active-row");
        document.getElementById(`total-${m.no}`).textContent = "QAR 0.00";
    });
    
    totalItemsCount.textContent = "0";
    totalQtySum.textContent = "0";
    totalFocSum.textContent = "0";
    netAmountSum.textContent = "0.00";
}

// ==========================================
// VIEWS & NAVIGATION ENGINE
// ==========================================
function showView(viewName) {
    viewDashboard.classList.remove("active");
    viewOrderForm.classList.remove("active");
    viewMemo.classList.remove("active");
    viewMappings.classList.remove("active");
    
    if (viewName === "dashboard") {
        loadOrdersFromSource();
        viewDashboard.classList.add("active");
        currentEditingId = null;
    } else if (viewName === "form") {
        if (!currentEditingId) {
            resetForm();
            document.getElementById("formTitle").textContent = "Create New Sales Order";
        } else {
            document.getElementById("formTitle").textContent = "Edit Sales Order";
        }
        viewOrderForm.classList.add("active");
        window.scrollTo({ top: 0, behavior: 'smooth' });
    } else if (viewName === "memo") {
        viewMemo.classList.add("active");
        window.scrollTo({ top: 0, behavior: 'smooth' });
    } else if (viewName === "mappings") {
        viewMappings.classList.add("active");
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
}

// ==========================================
// CRUD OPERATIONS
// ==========================================

// 1. RENDER DASHBOARD STATS
function updateDashboard() {
    statTotalOrders.textContent = orders.length;
    
    const sumTotal = orders.reduce((acc, order) => acc + order.totalAmount, 0);
    statTotalValue.textContent = `QAR ${sumTotal.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
    
    const creditCount = orders.filter(o => o.paymentMode === "CREDIT").length;
    statCreditOrders.textContent = creditCount;
    
    const cashCount = orders.filter(o => o.paymentMode === "CASH").length;
    statCashOrders.textContent = cashCount;

    renderOrdersList(orders);
}

function renderOrdersList(orderList) {
    ordersTableBody.innerHTML = "";
    document.getElementById("returnedOrdersTableBody").innerHTML = "";
    document.getElementById("pendingOrdersTableBody").innerHTML = "";
    
    const loggedUserType = (localStorage.getItem("loggedUserType") || "Salesman").trim().toLowerCase();
    const loggedSalespersonCode = (localStorage.getItem("loggedSalespersonCode") || "").trim();
    
    // Only show rejected orders to the salesperson who created them so they can correct it.
    const returnedOrders = orderList.filter(o => o.status && o.status.trim().toLowerCase() === 'rejected' && (o.salesPNCode || "").trim() === loggedSalespersonCode);
    
    let pendingOrders = [];
    let mainOrders = [];
    
    if (loggedUserType === 'supervisor') {
        pendingOrders = orderList.filter(o => o.status && o.status.trim().toLowerCase() === 'pending' && o.approver && o.approver.trim() === loggedSalespersonCode);
    }
    
    mainOrders = orderList.filter(o => o.status && o.status.trim().toLowerCase() !== 'rejected' && o.status.trim().toLowerCase() !== 'pending');
    
    if (returnedOrders.length > 0) {
        document.getElementById("returnedOrdersCard").style.display = "block";
        returnedOrders.forEach(order => renderOrderRow(order, document.getElementById("returnedOrdersTableBody"), true));
    } else {
        document.getElementById("returnedOrdersCard").style.display = "none";
    }
    
    if (pendingOrders.length > 0) {
        document.getElementById("pendingOrdersCard").style.display = "block";
        pendingOrders.forEach(order => renderOrderRow(order, document.getElementById("pendingOrdersTableBody"), false));
    } else {
        document.getElementById("pendingOrdersCard").style.display = "none";
    }
    
    if (mainOrders.length === 0) {
        ordersTableBody.innerHTML = `
            <tr>
                <td colspan="9" style="text-align: center; color: var(--text-muted); padding: 30px;">
                    <i class="fa-solid fa-inbox" style="font-size: 24px; margin-bottom: 8px; display: block;"></i>
                    No orders found. Create one to get started!
                </td>
            </tr>
        `;
    } else {
        mainOrders.forEach(order => renderOrderRow(order, ordersTableBody, false));
    }
}

function renderOrderRow(order, tbody, isReturnedTable) {
    const row = document.createElement("tr");
        
        const dateParts = order.date.split("-");
        const formattedDate = dateParts.length === 3 ? `${dateParts[2]}/${dateParts[1]}/${dateParts[0]}` : order.date;
        
        const loggedUserType = localStorage.getItem("loggedUserType") || "Salesman";
        const loggedSalespersonCode = localStorage.getItem("loggedSalespersonCode") || "";
        
        let editBtn = '';
        if (isReturnedTable) {
            editBtn = `<button class="btn btn-primary btn-sm" onclick="editOrder('${order.id}')" title="Edit Order">
                <i class="fa-solid fa-pen-to-square"></i> Edit
            </button>`;
        }
        
        let approveBtn = '';
        let rejectBtn = '';
        if (loggedUserType === 'supervisor' && order.status && order.status.trim().toLowerCase() === 'pending' && order.approver && order.approver.trim() === loggedSalespersonCode) {
            approveBtn = `<button class="btn btn-success btn-sm" onclick="approveOrder('${order.id}')" title="Approve Order" style="margin-left: 5px;">
                <i class="fa-solid fa-check"></i> Approve
            </button>`;
            rejectBtn = `<button class="btn btn-danger btn-sm" onclick="rejectOrder('${order.id}')" title="Reject Order" style="margin-left: 5px;">
                <i class="fa-solid fa-xmark"></i> Reject
            </button>`;
        }
        
        let statusBadgeColor = 'var(--color-warning)';
        if (order.status && order.status.trim().toLowerCase() === 'approved') statusBadgeColor = 'var(--color-success)';
        if (order.status && order.status.trim().toLowerCase() === 'rejected') statusBadgeColor = 'var(--color-danger)';
        
        row.innerHTML = `
            <td style="font-weight: 600;">${order.memoNumber || '-'}</td>
            <td>${formattedDate}</td>
            <td style="font-weight: 500;">${order.customerCode}</td>
            <td>
                <div style="font-weight: 700; color: var(--text-dark);">${order.customerName}</div>
                <div style="font-size: 11px; color: var(--text-muted); margin-top: 4px;">By: ${order.salesPerson || 'Salesman'}</div>
            </td>
            <td>
                <span style="background: rgba(46, 204, 113, 0.2); color: var(--color-success); padding: 4px 10px; border-radius: 12px; font-size: 11px; font-weight: 700;">
                    ${order.paymentMode}
                </span>
            </td>
            <td style="text-align: right; font-weight: 700;">QAR ${order.totalAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
            <td style="text-align: center; vertical-align: middle;">
                <span class="badge" style="background: ${statusBadgeColor}; color: white;">
                    ${order.status || 'Pending'}
                </span>
            </td>
            <td style="text-align: center;">
                <div class="actions-cell" style="display: flex; justify-content: center; align-items: center; gap: 8px;">
                    <button class="btn btn-secondary btn-sm" onclick="viewOrderDetails('${order.id}')" title="View Printable Order">
                        <i class="fa-solid fa-eye"></i> View
                    </button>
                    ${editBtn}
                    ${approveBtn}
                    ${rejectBtn}
                    <button type="button" style="background: transparent; border: none; color: #555; box-shadow: none; cursor: pointer; padding: 4px;" onclick="confirmDeleteOrder('${order.id}')" title="Delete Order">
                        <i class="fa-solid fa-trash" style="font-size: 13px;"></i>
                    </button>
                </div>
            </td>
        `;
        tbody.appendChild(row);
}

// 2. SAVE ORDER (POST / PUT)
async function handleFormSubmit(e) {
    e.preventDefault();
    
    const custName = customerNameInput.value.trim();
    const custCode = customerCodeInput.value.trim();
    const paymentMode = document.querySelector('input[name="paymentMode"]:checked').value;
    const oDate = orderDateInput.value;
    const requiredDate = document.getElementById("requiredDate").value;
    const referenceNumber = document.getElementById("referenceNumber").value.trim();
    const salesPerson = salesPersonInput.value.trim();
    const salesPNCode = salesPNCodeInput.value.trim();
    
    if (!custName || !custCode) {
        alert("Please select a valid customer.");
        return;
    }
    
    const items = [];
    let grandTotal = 0;
    let totalQty = 0;
    let totalFoc = 0;
    
    MATERIAL_MASTER.forEach(m => {
        const r = document.getElementById(`row-${m.no}`);
        const qty = parseInt(r.querySelector(".qty-input").value) || 0;
        const foc = parseInt(r.querySelector(".foc-input").value) || 0;
        const price = parseFloat(r.querySelector(".col-price").textContent) || 0;
        const remarks = r.querySelector(".remarks-input").value.trim();
        
        if (qty > 0 || foc > 0) {
            const lineTotal = qty * price;
            items.push({
                no: m.no,
                description: m.description,
                code: m.code,
                packing: r.dataset.packing,
                qty: qty,
                foc: foc,
                unitPrice: price,
                totalPrice: lineTotal,
                remarks: remarks
            });
            
            totalQty += qty;
            totalFoc += foc;
            grandTotal += lineTotal;
        }
    });
    
    if (items.length === 0) {
        alert("Please enter a quantity for at least one material.");
        return;
    }
    
    const isCredit = paymentMode === "CREDIT";
    const orderPayload = {
        date: oDate,
        requiredDate: requiredDate,
        referenceNumber: referenceNumber,
        customerName: custName,
        customerCode: custCode,
        paymentMode: paymentMode,
        salesPerson: salesPerson,
        salesPNCode: salesPNCode,
        items: items,
        totalQty: totalQty,
        totalFoc: totalFoc,
        totalAmount: grandTotal,
        isCreditVerified: isCredit
    };

    pendingOrderPayload = orderPayload;
    
    // Populate Review Modal
    document.getElementById("reviewCustomerInfo").innerHTML = `
        <div style="display: flex; justify-content: space-between;">
            <div><strong>Customer:</strong> ${orderPayload.customerName} <code style="margin-left: 6px;">${orderPayload.customerCode}</code></div>
            <div><strong>Date:</strong> ${orderPayload.date}</div>
        </div>
        <div style="margin-top: 6px;">
            <strong>Payment Mode:</strong> <span class="badge ${isCredit ? 'badge-credit' : 'badge-cash'}">${orderPayload.paymentMode}</span>
            <strong style="margin-left: 20px;">Salesperson:</strong> ${orderPayload.salesPerson}
        </div>
    `;
    
    const reviewItemsBody = document.getElementById("reviewItemsBody");
    reviewItemsBody.innerHTML = "";
    
    // Sort items: Bulk first, then Retail
    orderPayload.items = sortItemsByBulk(orderPayload.items);

    orderPayload.items.forEach(item => {
        reviewItemsBody.innerHTML += `
            <tr>
                <td style="padding: 10px 16px;"><strong>${item.description}</strong><br><small style="color:var(--text-muted);">${item.code}</small></td>
                <td style="padding: 10px 16px; text-align:center;">${item.qty > 0 ? item.qty : '-'}</td>
                <td style="padding: 10px 16px; text-align:center; ${item.foc > 0 ? 'color: #e67e22; font-weight: 600;' : ''}">${item.foc > 0 ? item.foc : '-'}</td>
                <td style="padding: 10px 16px; text-align:right;">${item.unitPrice.toFixed(2)}</td>
                <td style="padding: 10px 16px; text-align:right;"><strong>${item.totalPrice.toFixed(2)}</strong></td>
            </tr>
        `;
    });
    
    document.getElementById("reviewGrandTotal").textContent = `Grand Total: QAR ${orderPayload.totalAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
    document.getElementById("reviewOrderModal").classList.add("active");
}

async function executeOrderSave() {
    if (!pendingOrderPayload) return;
    const orderPayload = pendingOrderPayload;
    const btnConfirm = document.getElementById("btnConfirmReviewSave");
    btnConfirm.disabled = true;
    btnConfirm.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Saving...';

    if (isBackendConnected) {
        try {
            let res;
            if (currentEditingId) {
                // PUT update
                res = await fetch(`${API_BASE_URL}/orders/${currentEditingId}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(orderPayload)
                });
            } else {
                // POST create
                res = await fetch(`${API_BASE_URL}/orders`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(orderPayload)
                });
            }

            if (res.ok) {
                currentEditingId = null;
                await loadOrdersFromSource();
                closeReviewAndFinish();
            } else {
                const err = await res.text();
                alert(`Error saving order to Web API backend: ${err}`);
                resetConfirmButton(btnConfirm);
            }
            return;
        } catch (ex) {
            console.error("API call failed, falling back to LocalStorage save", ex);
        }
    }

    // LocalStorage Fallback CRUD
    if (currentEditingId) {
        const index = orders.findIndex(o => o.id === currentEditingId);
        if (index !== -1) {
            orders[index] = { ...orders[index], ...orderPayload, status: 'Pending', rejectReason: null };
        }
    } else {
        const memoSeq = orders.length + 1000 + Math.floor(Math.random()*10);
        orderPayload.id = `ord-${Date.now()}`;
        orderPayload.memoNumber = `${orderPayload.salesPNCode}-${memoSeq}`;
        orders.push(orderPayload);
    }
    
    localStorage.setItem("qfi_sales_orders", JSON.stringify(orders));
    currentEditingId = null;
    updateDashboard();
    closeReviewAndFinish();
}

function resetConfirmButton(btn) {
    btn.disabled = false;
    btn.innerHTML = '<i class="fa-solid fa-floppy-disk"></i> Confirm & Save Order';
}

function closeReviewAndFinish() {
    document.getElementById("reviewOrderModal").classList.remove("active");
    const btnConfirm = document.getElementById("btnConfirmReviewSave");
    resetConfirmButton(btnConfirm);
    pendingOrderPayload = null;
    showView("dashboard");
}

// 3. EDIT ORDER
async function editOrder(orderId) {
    let order = orders.find(o => o.id === orderId);
    
    // Fetch details dynamically if connected to Web API
    if (isBackendConnected) {
        try {
            const res = await fetch(`${API_BASE_URL}/orders/${orderId}`);
            if (res.ok) {
                order = await res.json();
            }
        } catch (ex) {
            console.error("Could not fetch detailed order from API. Using local cache.", ex);
        }
    }

    if (!order) return;
    
    currentEditingId = order.id;
    
    orderDateInput.value = order.date;
    document.getElementById("requiredDate").value = order.requiredDate || order.date;
    document.getElementById("referenceNumber").value = order.referenceNumber || "";
    customerNameInput.value = order.customerName;
    customerCodeInput.value = order.customerCode;
    salesPersonInput.value = order.salesPerson || "Jawed Akthar";
    salesPNCodeInput.value = order.salesPNCode || "100302";
    
    // Update price type and refresh catalog
    const option = Array.from(customerNameInput.options).find(opt => opt.dataset.code === order.customerCode);
    currentPriceType = option ? (option.dataset.priceType || "REGULAR") : "REGULAR";
    updateCatalogPricing(currentPriceType);
    
    if (order.paymentMode === "CREDIT") {
        document.getElementById("payCredit").checked = true;
    } else {
        document.getElementById("payCash").checked = true;
    }
    
    MATERIAL_MASTER.forEach(m => {
        const r = document.getElementById(`row-${m.no}`);
        r.querySelector(".qty-input").value = "";
        const focIn = r.querySelector(".foc-input");
        focIn.value = "";
        focIn.setAttribute("readonly", "readonly");
        r.querySelector(".foc-enable-chk").checked = false;
        r.querySelector(".remarks-input").value = "";
        r.classList.remove("active-row");
        document.getElementById(`total-${m.no}`).textContent = "QAR 0.00";
    });
    
    order.items.forEach(item => {
        const r = document.getElementById(`row-${item.no}`);
        if (r) {
            r.querySelector(".qty-input").value = item.qty || "";
            const focIn = r.querySelector(".foc-input");
            const focChk = r.querySelector(".foc-enable-chk");
            if (item.foc > 0) {
                focChk.checked = true;
                focIn.removeAttribute("readonly");
                focIn.value = item.foc;
            } else {
                focChk.checked = false;
                focIn.setAttribute("readonly", "readonly");
                focIn.value = "";
            }
            r.querySelector(".remarks-input").value = item.remarks || "";
            r.classList.add("active-row");
            
            const total = item.qty * item.unitPrice;
            document.getElementById(`total-${item.no}`).textContent = total > 0 ? `QAR ${total.toFixed(2)}` : "QAR 0.00";
        }
    });
    
    totalItemsCount.textContent = order.items.length;
    totalQtySum.textContent = order.totalQty;
    totalFocSum.textContent = order.totalFoc;
    netAmountSum.textContent = order.totalAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    
    showView("form");
}

// 4. VIEW ORDER DETAILS
async function viewOrderDetails(orderId) {
    let order = orders.find(o => o.id === orderId);
    
    if (isBackendConnected) {
        try {
            const res = await fetch(`${API_BASE_URL}/orders/${orderId}`);
            if (res.ok) {
                order = await res.json();
            }
        } catch (ex) {
            console.error("Could not fetch detailed order for view from API.", ex);
        }
    }

    if (!order) return;
    
    currentEditingId = order.id; 
    
    const dateParts = order.date.split("-");
    const formattedDate = dateParts.length === 3 ? `${dateParts[2]}/${dateParts[1]}/${dateParts[0]}` : order.date;
    
    const reqDateParts = (order.requiredDate || order.date).split("-");
    const formattedReqDate = reqDateParts.length === 3 ? `${reqDateParts[2]}/${reqDateParts[1]}/${reqDateParts[0]}` : (order.requiredDate || order.date);
    
    viewCustomerName.textContent = order.customerName;
    viewCustomerCode.textContent = order.customerCode;
    viewPaymentMode.textContent = order.paymentMode;
    viewMemoNumber.textContent = order.memoNumber;
    viewOrderDate.textContent = formattedDate;
    document.getElementById("viewRequiredDate").textContent = formattedReqDate;
    document.getElementById("viewReferenceNumber").textContent = order.referenceNumber || "-";
    viewSalesPerson.textContent = order.salesPerson || "Jawed Akthar";
    viewSalesPNCode.textContent = order.salesPNCode || "100302";
    
    viewInvoiceItemsBody.innerHTML = "";
    
    // Sort items: Bulk first, then Retail
    order.items = sortItemsByBulk(order.items);

    order.items.forEach(orderedItem => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td class="text-center">${orderedItem.no}</td>
            <td>${orderedItem.description}</td>
            <td class="text-center">${orderedItem.code}</td>
            <td class="text-center">${orderedItem.packing}</td>
            <td class="text-center">${orderedItem.qty > 0 ? orderedItem.qty : '-'}</td>
            <td class="text-center" style="${orderedItem.foc > 0 ? 'color: #e67e22; font-weight: 600;' : ''}">${orderedItem.foc > 0 ? orderedItem.foc : '-'}</td>
            <td class="text-right">${orderedItem.unitPrice > 0 ? orderedItem.unitPrice.toFixed(2) : '-'}</td>
            <td class="text-right">${orderedItem.totalPrice > 0 ? orderedItem.totalPrice.toFixed(2) : '0.00'}</td>
            <td style="font-size: 12px; font-style: italic;">${orderedItem.remarks || '-'}</td>
        `;
        viewInvoiceItemsBody.appendChild(row);
    });
    
    viewTotalQty.textContent = order.totalQty;
    viewTotalFoc.textContent = order.totalFoc;
    viewNetAmount.textContent = order.totalAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    
    // Manage Viewer Buttons
    const viewerActionButtons = document.getElementById("viewerActionButtons");
    const approvalSection = document.getElementById("approvalSection");
    const btnRejectFromViewer = document.getElementById("btnRejectFromViewer");
    const btnApproveFromViewer = document.getElementById("btnApproveFromViewer");
    const approvalComment = document.getElementById("approvalComment");
    
    const loggedUserType = (localStorage.getItem("loggedUserType") || "Salesman").trim().toLowerCase();
    const loggedSalespersonCode = (localStorage.getItem("loggedSalespersonCode") || "").trim();
    
    let buttonsHtml = '';
    
    // Manage bottom approval section
    if (loggedUserType === 'supervisor' && order.status && order.status.trim().toLowerCase() === 'pending' && order.approver && order.approver.trim() === loggedSalespersonCode) {
        approvalSection.style.display = 'block';
        approvalComment.value = ""; // Clear any previous comment
        
        // Remove old listeners by cloning
        const newBtnReject = btnRejectFromViewer.cloneNode(true);
        btnRejectFromViewer.parentNode.replaceChild(newBtnReject, btnRejectFromViewer);
        
        const newBtnApprove = btnApproveFromViewer.cloneNode(true);
        btnApproveFromViewer.parentNode.replaceChild(newBtnApprove, btnApproveFromViewer);
        
        newBtnApprove.addEventListener("click", () => {
            approveOrder(order.id, approvalComment.value.trim());
            showView('dashboard');
        });
        
        newBtnReject.addEventListener("click", () => {
            const reason = approvalComment.value.trim();
            if (!reason) {
                alert("Please enter a reason for rejection in the comment box.");
                approvalComment.focus();
                return;
            }
            rejectOrder(order.id, reason);
            showView('dashboard');
        });
    } else {
        approvalSection.style.display = 'none';
    }
    
    // Add Edit Order if it's Rejected
    if (order.status && order.status.trim().toLowerCase() === 'rejected') {
        buttonsHtml += `
            <button type="button" id="btnEditFromViewerDynamic" class="btn btn-primary">
                <i class="fa-solid fa-pen-to-square"></i> Edit Order
            </button>
        `;
    }
    
    // Add Print Button
    buttonsHtml += `
        <button type="button" id="btnPrintMemoDynamic" class="btn btn-secondary">
            <i class="fa-solid fa-print"></i> Print Order Document
        </button>
    `;
    
    viewerActionButtons.innerHTML = buttonsHtml;
    
    // Attach dynamic listeners
    const btnPrintDynamic = document.getElementById("btnPrintMemoDynamic");
    if (btnPrintDynamic) {
        btnPrintDynamic.addEventListener("click", () => {
            window.print();
        });
    }
    
    const btnEditDynamic = document.getElementById("btnEditFromViewerDynamic");
    if (btnEditDynamic) {
        btnEditDynamic.addEventListener("click", () => {
            editOrder(currentEditingId);
        });
    }
    
    showView("memo");
}

// 5. DELETE ORDER ACTIONS
function confirmDeleteOrder(orderId) {
    const order = orders.find(o => o.id === orderId);
    if (!order) return;
    
    deleteTargetId = order.id;
    deleteConfirmCustName.textContent = `${order.customerName} (${order.memoNumber})`;
    deleteConfirmModal.classList.add("active");
}

function closeDeleteModal() {
    deleteConfirmModal.classList.remove("active");
    deleteTargetId = null;
}

async function deleteOrderConfirmed() {
    if (!deleteTargetId) return;
    
    if (isBackendConnected) {
        try {
            const res = await fetch(`${API_BASE_URL}/orders/${deleteTargetId}`, { method: 'DELETE' });
            if (res.ok) {
                closeDeleteModal();
                await loadOrdersFromSource();
                return;
            } else {
                const err = await res.text();
                alert(`Error deleting order from Web API backend: ${err}`);
            }
        } catch (ex) {
            console.error("API call failed for deletion, falling back to LocalStorage delete", ex);
        }
    }

    // Local Storage delete fallback
    orders = orders.filter(o => o.id !== deleteTargetId);
    localStorage.setItem("qfi_sales_orders", JSON.stringify(orders));
    
    closeDeleteModal();
    updateDashboard();
}

async function approveOrder(orderId, reason = "") {
    if (!confirm("Are you sure you want to approve this order?")) return;
    
    const approverCode = localStorage.getItem("loggedSalespersonCode");
    if (!approverCode) {
        alert("You must be logged in as a supervisor to approve orders.");
        return;
    }

    if (isBackendConnected) {
        try {
            const res = await fetch(`${API_BASE_URL}/orders/${orderId}/approve`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ approverCode, reason })
            });

            if (res.ok) {
                alert("Order approved successfully!");
                loadOrdersFromSource();
                return;
            } else {
                alert("Failed to approve order.");
                return;
            }
        } catch (ex) {
            console.error("API call failed for approval, falling back to local storage", ex);
        }
    }

    // Local Storage fallback
    const index = orders.findIndex(o => o.id === orderId);
    if (index > -1) {
        orders[index].status = "Approved";
        if (reason) orders[index].rejectReason = reason;
        localStorage.setItem("qfi_sales_orders", JSON.stringify(orders));
        updateDashboard();
        alert("Order approved successfully (Local Mode)!");
    }
}

async function rejectOrder(orderId, providedReason = null) {
    let reason = providedReason;
    if (reason === null) {
        reason = prompt("Please enter the reason for rejection:");
        if (reason === null) return; // User cancelled
    }
    
    if (reason.trim() === "") {
        alert("Rejection reason is required.");
        return;
    }
    
    const approverCode = localStorage.getItem("loggedSalespersonCode");
    if (!approverCode) {
        alert("You must be logged in as a supervisor to reject orders.");
        return;
    }

    if (isBackendConnected) {
        try {
            const res = await fetch(`${API_BASE_URL}/orders/${orderId}/reject`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ approverCode, reason })
            });

            if (res.ok) {
                alert("Order rejected successfully.");
                loadOrdersFromSource();
                return;
            } else {
                alert("Failed to reject order.");
                return;
            }
        } catch (ex) {
            console.error("API call failed for rejection, falling back to local storage", ex);
        }
    }

    // Local Storage fallback
    const index = orders.findIndex(o => o.id === orderId);
    if (index > -1) {
        orders[index].status = "Rejected";
        orders[index].rejectReason = reason;
        localStorage.setItem("qfi_sales_orders", JSON.stringify(orders));
        updateDashboard();
        alert("Order rejected successfully (Local Mode)!");
    }
}

// 6. DASHBOARD SEARCH FILTERING
function generateUniqueRef() {
    return 'REQ-' + Math.random().toString(36).substring(2, 8).toUpperCase();
}

// Sorting helper: Bulk items first, then Retail
function sortItemsByBulk(items) {
    return items.sort((a, b) => {
        // Find salesGroup from MATERIAL_MASTER if not on the item directly
        let groupA = a.salesGroup;
        if (!groupA) {
            const matA = MATERIAL_MASTER.find(m => m.code === a.code);
            groupA = matA ? matA.salesGroup : "";
        }
        let groupB = b.salesGroup;
        if (!groupB) {
            const matB = MATERIAL_MASTER.find(m => m.code === b.code);
            groupB = matB ? matB.salesGroup : "";
        }

        const aIsBulk = groupA && groupA.toLowerCase().includes('bulk') ? 1 : 0;
        const bIsBulk = groupB && groupB.toLowerCase().includes('bulk') ? 1 : 0;
        
        if (aIsBulk !== bIsBulk) {
            return bIsBulk - aIsBulk; // 1 (Bulk) comes before 0
        }
        return a.no - b.no; // Fallback to item No
    });
}

function handleDashboardSearch() {
    const query = searchOrdersInput.value.trim().toLowerCase();
    const fromDateStr = dateFilterFrom.value;
    const toDateStr = dateFilterTo.value;
    
    let filtered = orders;
    
    if (query) {
        filtered = filtered.filter(o => 
            o.customerName.toLowerCase().includes(query) || 
            o.customerCode.toLowerCase().includes(query) || 
            o.memoNumber.toLowerCase().includes(query) ||
            o.salesPerson.toLowerCase().includes(query)
        );
    }
    
    if (fromDateStr) {
        const fromDate = new Date(fromDateStr);
        filtered = filtered.filter(o => new Date(o.date) >= fromDate);
    }
    
    if (toDateStr) {
        const toDate = new Date(toDateStr);
        filtered = filtered.filter(o => new Date(o.date) <= toDate);
    }
    
    renderOrdersList(filtered);
}

// Expose functions globally to inline HTML listeners
window.viewOrderDetails = viewOrderDetails;
window.editOrder = editOrder;
window.confirmDeleteOrder = confirmDeleteOrder;
