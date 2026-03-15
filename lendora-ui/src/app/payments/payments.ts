import { Component, OnInit, inject, signal } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';

import { ApiService } from '../core/services/api.service';
import { NotificationService } from '../core/services/notification.service';
import { PaymentResult } from '../core/models/payment.model';

@Component({
  selector: 'app-payments',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe],
  templateUrl: './payments.html',
  styleUrl: './payments.scss',
})
export class Payments implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(ApiService);
  private readonly notify = inject(NotificationService);

  readonly submitting = signal<boolean>(false);
  readonly paymentResult = signal<PaymentResult | null>(null);

  /**
   * Reactive form with four fields.
   * paymentReference is intentionally optional (no Validators.required).
   */
  readonly form: FormGroup = this.fb.group({
    loanId:           ['', [Validators.required, Validators.minLength(1)]],
    amount:           [null, [Validators.required, Validators.min(0.01)]],
    paidAt:           ['', [Validators.required]],
    paymentReference: [''],
  });

  ngOnInit(): void {
    // Pre-fill loanId from ?loanId= query param if present.
    const loanId = this.route.snapshot.queryParamMap.get('loanId');
    if (loanId) {
      this.form.patchValue({ loanId });
    }

    // Default paidAt to today's date in YYYY-MM-DD format for the date input.
    const today = new Date().toISOString().split('T')[0];
    this.form.patchValue({ paidAt: today });
  }

  get loanIdCtrl()   { return this.form.get('loanId')!; }
  get amountCtrl()   { return this.form.get('amount')!; }
  get paidAtCtrl()   { return this.form.get('paidAt')!; }
  get refCtrl()      { return this.form.get('paymentReference')!; }

  /** Returns error classes for an invalid-and-touched control. */
  fieldError(controlName: string): boolean {
    const ctrl = this.form.get(controlName);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.paymentResult.set(null);

    const { loanId, amount, paidAt, paymentReference } = this.form.getRawValue();

    // Build ISO-8601 datetime from the date input value.
    const paidAtIso = new Date(`${paidAt}T00:00:00`).toISOString();

    this.api
      .registerPayment({
        loanId,
        amount: Number(amount),
        paidAt: paidAtIso,
        paymentReference: paymentReference?.trim() || null,
      })
      .subscribe({
        next: (result) => {
          this.paymentResult.set(result);
          this.notify.success(
            `Payment registered. Remaining balance: $${result.remainingBalance.toFixed(2)}.`
          );
          this.submitting.set(false);
          // Reset amounts but keep loanId in case user makes another payment.
          this.form.patchValue({ amount: null, paymentReference: '' });
          this.form.get('amount')?.markAsUntouched();
          this.form.get('paymentReference')?.markAsUntouched();
        },
        error: (err) => {
          const message = err?.message ?? 'Payment registration failed. Please try again.';
          this.notify.error(message);
          this.submitting.set(false);
        },
      });
  }

  navigateToLoan(): void {
    const loanId = this.form.get('loanId')?.value;
    if (loanId) {
      this.router.navigate(['/loans', loanId]);
    }
  }
}
