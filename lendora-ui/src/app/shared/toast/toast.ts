import { Component, inject } from '@angular/core';

import { NotificationService, Notification } from '../../core/services/notification.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [],
  templateUrl: './toast.html',
  styleUrl: './toast.scss',
})
export class Toast {
  readonly notificationService = inject(NotificationService);

  dismiss(id: string): void {
    this.notificationService.dismiss(id);
  }

  /** Left-border and icon colour classes keyed by notification type. */
  borderClass(notification: Notification): string {
    return notification.type === 'success'
      ? 'border-l-success-500'
      : 'border-l-danger-500';
  }

  iconColorClass(notification: Notification): string {
    return notification.type === 'success'
      ? 'text-success-500'
      : 'text-danger-500';
  }

  iconBgClass(notification: Notification): string {
    return notification.type === 'success'
      ? 'bg-success-50'
      : 'bg-danger-50';
  }

  /** Track by id for @for to enable enter/leave animations per element. */
  trackById(_index: number, item: Notification): string {
    return item.id;
  }
}
