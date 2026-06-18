import axios from 'axios';

const BASE = process.env.REACT_APP_API_BASE || '';

const AUTH_URL = `${BASE}`;
const ORDER_URL = `${BASE}`;
const VEHICLE_URL = `${BASE}`;
const NOTIFICATION_URL = `${BASE}`;
const REPORT_URL = `${BASE}`;
const CONSOLIDATION_URL = `${BASE}`;

// Token'ı localStorage'dan al
const getToken = () => localStorage.getItem('token');

// Her istekte token header'ı ekle
const authHeader = () => ({
  headers: { Authorization: `Bearer ${getToken()}` }
});

// Auth
export const login = (username: string, password: string) =>
  axios.post(`${AUTH_URL}/api/auth/login`, { username, password });

// Orders
export const getOrders = () =>
  axios.get(`${ORDER_URL}/api/orders`, authHeader());

export const createOrder = (data: any) =>
  axios.post(`${ORDER_URL}/api/orders`, data, authHeader());

export const updateOrderStatus = (id: number, data: any) =>
  axios.put(`${ORDER_URL}/api/orders/${id}/status`, data, authHeader());

// Vehicles
export const getVehicles = () =>
  axios.get(`${VEHICLE_URL}/api/vehicles`, authHeader());

export const getBranchSummary = () =>
  axios.get(`${VEHICLE_URL}/api/vehicles/branch-summary`, authHeader());

// Notifications
export const getNotifications = (branchId: number) =>
  axios.get(`${NOTIFICATION_URL}/api/notifications/${branchId}`, authHeader());

export const getUnreadCount = (branchId: number) =>
  axios.get(`${NOTIFICATION_URL}/api/notifications/${branchId}/unread-count`, authHeader());

export const markAsRead = (id: number) =>
  axios.put(`${NOTIFICATION_URL}/api/notifications/${id}/read`, {}, authHeader());

// Reports
export const getSummary = () =>
  axios.get(`${REPORT_URL}/api/reports/summary`, authHeader());

export const getBranchReport = () =>
  axios.get(`${REPORT_URL}/api/reports/branches`, authHeader());

export const getVehicleReport = () =>
  axios.get(`${REPORT_URL}/api/reports/vehicles`, authHeader());

export const getDailyReport = () =>
  axios.get(`${REPORT_URL}/api/reports/daily`, authHeader());

// Consolidation
export const getConsolidationPlans = () =>
  axios.get(`${CONSOLIDATION_URL}/api/consolidation/plans`, authHeader());

export const getConsolidationSavings = () =>
  axios.get(`${CONSOLIDATION_URL}/api/consolidation/savings`, authHeader());

export const runConsolidation = () =>
  axios.post(`${CONSOLIDATION_URL}/api/consolidation/run`, {}, authHeader());