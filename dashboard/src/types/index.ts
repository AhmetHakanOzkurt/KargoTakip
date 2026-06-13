export interface User {
  token: string;
  username: string;
  fullName: string;
  role: string;
  branchId: number;
  branchName: string;
}

export interface Order {
  id: number;
  trackingCode: string;
  senderName: string;
  receiverName: string;
  receiverAddress: string;
  receiverCity: string;
  weight: number;
  priority: string;
  currentStatus: string;
  branch: string;
  assignedVehicle: string | null;
  createdBy: string;
  createdAt: string;
}

export interface Vehicle {
  id: number;
  plateNumber: string;
  capacity: number;
  currentLoad: number;
  occupancyRate: number;
  isAvailable: boolean;
  vehicleType: string;
  routeType: string;
  branch: string;
  city: string;
}

export interface BranchSummary {
  subeId: number;
  subeAdi: string;
  sehir: string;
  toplamArac: number;
  müsaitArac: number;
  mesgulArac: number;
  toplamKapasite: number;
  toplamYuk: number;
  dolulukOrani: number;
  aktifKargo: number;
}

export interface Notification {
  id: number;
  message: string;
  isRead: boolean;
  createdAt: string;
  shipmentId: number | null;
  transferRequestId: number | null;
}

export interface ConsolidationPlan {
  id: number;
  status: string;
  arac: string;
  cikisSube: string;
  hedefSehir: string;
  totalCapacity: number;
  usedCapacity: number;
  dolulukOrani: number;
  tahminiYakitTasarrufu: number;
  kargoSayisi: number;
  plannedDepartureAt: string;
  createdAt: string;
}

export interface ConsolidationSavings {
  toplamSefer: number;
  toplamKargo: number;
  toplamTahminiTasarruf: number;
  ortalamaDolulukOrani: number;
}