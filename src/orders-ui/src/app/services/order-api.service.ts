import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CompleteOrderRequest,
  OrderFilter,
  PagedOrders,
} from '../core/order.models';

@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/orders';

  getOpenOrders(filter: OrderFilter): Observable<PagedOrders> {
    let params = new HttpParams();
    if (filter.orderNumber) {
      params = params.set('orderNumber', filter.orderNumber);
    }
    if (filter.from) {
      params = params.set('from', filter.from);
    }
    if (filter.to) {
      params = params.set('to', filter.to);
    }
    if (filter.page) {
      params = params.set('page', filter.page);
    }
    if (filter.pageSize) {
      params = params.set('pageSize', filter.pageSize);
    }
    return this.http.get<PagedOrders>(this.baseUrl, { params });
  }

  completeOrder(id: number, request: CompleteOrderRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/complete`, request);
  }
}
