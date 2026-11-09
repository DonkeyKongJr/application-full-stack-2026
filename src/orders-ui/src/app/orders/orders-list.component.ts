import { Component, OnInit } from '@angular/core';
import { OrderService } from '../services/order.service';

@Component({
  selector: 'app-orders-list',
  templateUrl: './orders-list.component.html',
  styleUrls: ['./orders-list.component.css'],
})
export class OrdersListComponent implements OnInit {
  orders: any[] = [];
  filterOrderNumber = '';
  loading = false;

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.orderService.getOpenOrders({ orderNumber: this.filterOrderNumber }).subscribe((data) => {
      this.orders = data;
      this.loading = false;
    });
  }

  markCompleted(order: any): void {
    this.orderService.completeOrder(order.id, { comment: 'Erledigt durch Kunde' }).subscribe(() => {
      this.orderService.getOpenOrders({ orderNumber: this.filterOrderNumber }).subscribe((data) => {
        this.orders = data;
      });
    });
  }

  applyFilter(): void {
    this.loadOrders();
  }
}
