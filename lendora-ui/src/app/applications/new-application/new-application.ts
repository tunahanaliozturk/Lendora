import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { ApiService } from '../../core/services/api.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-new-application',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './new-application.html',
  styleUrl: './new-application.scss',
})
export class NewApplication {
  private readonly api = inject(ApiService);
  private readonly notify = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly isSubmitting = signal(false);

  readonly form: FormGroup = this.fb.group({
    customerId: ['', [Validators.required, Validators.minLength(3)]],
    requestedAmount: [null, [Validators.required, Validators.min(1000), Validators.max(500000)]],
    requestedTermMonths: [null, [Validators.required, Validators.min(6), Validators.max(120)]],
    annualIncome: [null, [Validators.required, Validators.min(1)]],
    existingMonthlyDebt: [null, [Validators.required, Validators.min(0)]],
  });

  get customerId() { return this.form.get('customerId')!; }
  get requestedAmount() { return this.form.get('requestedAmount')!; }
  get requestedTermMonths() { return this.form.get('requestedTermMonths')!; }
  get annualIncome() { return this.form.get('annualIncome')!; }
  get existingMonthlyDebt() { return this.form.get('existingMonthlyDebt')!; }

  /** Returns true when a field is invalid AND has been touched. */
  isInvalid(field: ReturnType<FormGroup['get']>): boolean {
    return !!(field?.invalid && field.touched);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    this.api.submitApplication(this.form.getRawValue()).subscribe({
      next: (result) => {
        this.notify.success(`Application submitted successfully. ID: ${result.id.substring(0, 8).toUpperCase()}`);
        this.router.navigate(['/applications']);
      },
      error: () => {
        this.notify.error('Failed to submit application. Please try again.');
        this.isSubmitting.set(false);
      },
    });
  }
}
