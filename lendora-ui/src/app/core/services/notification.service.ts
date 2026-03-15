import { Injectable, signal, computed } from '@angular/core';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export type NotificationType = 'success' | 'error';

export interface Notification {
  /** Unique identifier used to target removal. */
  id: string;
  type: NotificationType;
  message: string;
}

/** How long (ms) a notification stays visible before being auto-removed. */
const AUTO_DISMISS_MS = 5_000;

// ---------------------------------------------------------------------------
// Service
// ---------------------------------------------------------------------------

/**
 * NotificationService
 *
 * Manages a reactive list of toast notifications using Angular signals.
 * Components bind to the `notifications` signal (or the typed helpers
 * `successNotifications` / `errorNotifications`) and render whatever is
 * currently in the array.
 *
 * Usage
 * ─────
 *   // In a component or effect:
 *   private readonly notify = inject(NotificationService);
 *
 *   this.notify.success('Application submitted successfully.');
 *   this.notify.error('Failed to load loan data. Please try again.');
 *
 *   // In a template:
 *   @for (n of notificationService.notifications(); track n.id) { … }
 */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  /**
   * Internal writable signal — mutated only within this service.
   * Exposed read-only via the public `notifications` computed below.
   */
  private readonly _notifications = signal<Notification[]>([]);

  /** Tracks pending auto-dismiss timers so they can be cancelled on demand. */
  private readonly _timers = new Map<string, ReturnType<typeof setTimeout>>();

  // -------------------------------------------------------------------------
  // Public signals
  // -------------------------------------------------------------------------

  /** Read-only view of all currently visible notifications. */
  readonly notifications = computed(() => this._notifications());

  /** Filtered view — success toasts only. Useful for separate render zones. */
  readonly successNotifications = computed(() =>
    this._notifications().filter((n) => n.type === 'success')
  );

  /** Filtered view — error toasts only. */
  readonly errorNotifications = computed(() =>
    this._notifications().filter((n) => n.type === 'error')
  );

  // -------------------------------------------------------------------------
  // Public API
  // -------------------------------------------------------------------------

  /**
   * Display a success toast. Auto-dismissed after {@link AUTO_DISMISS_MS} ms.
   */
  success(message: string): void {
    this.add('success', message);
  }

  /**
   * Display an error toast. Auto-dismissed after {@link AUTO_DISMISS_MS} ms.
   *
   * Prefer passing a concise, user-facing message rather than a raw error
   * object string so the UI stays readable.
   */
  error(message: string): void {
    this.add('error', message);
  }

  /**
   * Immediately remove a notification by ID.
   * Called automatically by the auto-dismiss timer; can also be called
   * from a dismiss button in the toast UI component.
   */
  dismiss(id: string): void {
    // Cancel the pending timer if the user dismisses manually before it fires.
    const timer = this._timers.get(id);
    if (timer !== undefined) {
      clearTimeout(timer);
      this._timers.delete(id);
    }

    this._notifications.update((current) =>
      current.filter((n) => n.id !== id)
    );
  }

  /**
   * Remove all notifications at once (e.g. on route navigation).
   */
  dismissAll(): void {
    // Cancel every pending timer.
    this._timers.forEach((timer) => clearTimeout(timer));
    this._timers.clear();

    this._notifications.set([]);
  }

  // -------------------------------------------------------------------------
  // Private helpers
  // -------------------------------------------------------------------------

  private add(type: NotificationType, message: string): void {
    const id = this.generateId();
    const notification: Notification = { id, type, message };

    this._notifications.update((current) => [...current, notification]);

    // Schedule auto-removal and store the handle for potential cancellation.
    const timer = setTimeout(() => this.dismiss(id), AUTO_DISMISS_MS);
    this._timers.set(id, timer);
  }

  /**
   * Generates a short, collision-resistant ID.
   * Uses crypto.randomUUID() when available (all modern browsers and Node 18+),
   * with a Date.now() + Math.random() fallback for older environments.
   */
  private generateId(): string {
    if (typeof crypto !== 'undefined' && crypto.randomUUID) {
      return crypto.randomUUID();
    }
    // Fallback — good enough for a notification list, not a security token.
    return `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 9)}`;
  }
}
