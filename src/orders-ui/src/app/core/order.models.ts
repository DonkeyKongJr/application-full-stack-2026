export interface OrderListItem {
  id: number;
  orderNumber: string;
  customerName: string;
  createdAt: string;
  totalAmount: number;
  lineCount: number;
}

export interface PagedOrders {
  items: OrderListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface OrderFilter {
  orderNumber?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

export interface CompleteOrderRequest {
  comment?: string;
}
