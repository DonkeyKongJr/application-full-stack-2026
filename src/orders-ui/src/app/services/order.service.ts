import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

const API_URL = 'https://api.mrsd-customer.local/api/orders';

export interface OrderFilter {
  orderNumber?: string;
  from?: string;
  to?: string;
}

export interface CompleteOrderRequest {
  comment?: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  constructor(private http: HttpClient) {}

  getOpenOrders(filter: OrderFilter): Observable<any> {
    return this.http.get<any>(API_URL, { params: { ...filter } as any });
  }

  completeOrder(id: number, request: CompleteOrderRequest): Observable<any> {
    return this.http.post<any>(`${API_URL}/${id}/complete`, request);
  }
}
