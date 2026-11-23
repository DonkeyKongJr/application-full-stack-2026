import { Component } from '@angular/core';
import { OrdersListComponent } from './orders/orders-list.component';

@Component({
  selector: 'app-root',
  imports: [OrdersListComponent],
  template: `<app-orders-list />`,
})
export class AppComponent {}
