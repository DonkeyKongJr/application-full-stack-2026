import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { debounceTime } from 'rxjs/operators';
import { OrderApiService } from '../services/order-api.service';
import { OrderListItem } from '../core/order.models';

@Component({
  selector: 'app-orders-list',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, CurrencyPipe],
  templateUrl: './orders-list.component.html',
  styleUrl: './orders-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrdersListComponent implements OnInit {
  private readonly api = inject(OrderApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly orderNumberControl = new FormControl('', { nonNullable: true });
  readonly fromControl = new FormControl('', { nonNullable: true });
  readonly toControl = new FormControl('', { nonNullable: true });

  readonly orders = signal<OrderListItem[]>([]);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly totalCount = signal(0);

  readonly commentDrafts = signal<Record<number, string>>({});

  readonly hasOrders = computed(() => this.orders().length > 0);

  ngOnInit(): void {
    this.orderNumberControl.valueChanges
      .pipe(debounceTime(300), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.reload());

    this.fromControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.reload());

    this.toControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.reload());

    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    const fromRaw = this.fromControl.value;
    const toRaw = this.toControl.value;

    const from = fromRaw ? new Date(fromRaw).toISOString() : undefined;
    const to = toRaw ? new Date(toRaw).toISOString() : undefined;

    this.api
      .getOpenOrders({
        orderNumber: this.orderNumberControl.value || undefined,
        from,
        to,
        page: 1,
        pageSize: 20,
      })
      .subscribe({
        next: (page) => {
          this.orders.set(page.items);
          this.totalCount.set(page.totalCount);
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('Aufträge konnten nicht geladen werden.');
          this.loading.set(false);
        },
      });
  }

  setComment(index: number, value: string): void {
    this.commentDrafts.update((drafts) => ({ ...drafts, [index]: value }));
  }

  markCompleted(order: OrderListItem, index: number): void {
    const comment = this.commentDrafts()[index] ?? '';

    this.orders.update((list) => {
      list.splice(index, 1);
      return list;
    });
    this.totalCount.update((c) => Math.max(0, c - 1));

    this.api.completeOrder(order.id, { comment: comment || undefined }).subscribe({
      next: () => undefined,
      error: () => {
        this.errorMessage.set(
          `Auftrag ${order.orderNumber} konnte nicht abgeschlossen werden.`
        );
      },
    });
  }
}
