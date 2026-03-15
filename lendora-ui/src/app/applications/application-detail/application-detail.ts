import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CurrencyPipe, DatePipe } from '@angular/common';

import { ApiService } from '../../core/services/api.service';
import { NotificationService } from '../../core/services/notification.service';
import {
  LoanApplication,
  LoanApplicationStatus,
} from '../../core/models/loan-application.model';

@Component({
  selector: 'app-application-detail',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, CurrencyPipe, DatePipe],
  templateUrl: './application-detail.html',
  styleUrl: './application-detail.scss',
})
export class ApplicationDetail implements OnInit {
  private readonly api = inject(ApiService);
  private readonly notify = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  // -------------------------------------------------------------------------
  // State
  // -------------------------------------------------------------------------

  readonly application = signal<LoanApplication | null>(null);
  readonly isLoading = signal(true);
  readonly loadError = signal<string | null>(null);

  /** ID of the newly created loan after an approval (from ApproveResult). */
  readonly createdLoanId = signal<string | null>(null);

  // Action busy states
  readonly isEvaluating = signal(false);
  readonly isApproving = signal(false);
  readonly isRejecting = signal(false);

  // Inline form visibility
  readonly showApproveForm = signal(false);
  readonly showRejectForm = signal(false);

  // -------------------------------------------------------------------------
  // Forms
  // -------------------------------------------------------------------------

  readonly approveForm: FormGroup = this.fb.group({
    approvedBy:          ['', [Validators.required, Validators.minLength(2)]],
    interestRate:        [null, [Validators.required, Validators.min(0.1), Validators.max(100)]],
    approvedAmount:      [null, [Validators.required, Validators.min(1)]],
    approvedTermMonths:  [null, [Validators.required, Validators.min(1), Validators.max(120)]],
  });

  readonly rejectForm: FormGroup = this.fb.group({
    rejectedBy: ['', [Validators.required, Validators.minLength(2)]],
    reason:     ['', [Validators.required, Validators.minLength(10)]],
  });

  // -------------------------------------------------------------------------
  // Lifecycle
  // -------------------------------------------------------------------------

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loadError.set('No application ID found in the route.');
      this.isLoading.set(false);
      return;
    }
    this.loadApplication(id);
  }

  // -------------------------------------------------------------------------
  // Data loading
  // -------------------------------------------------------------------------

  private loadApplication(id: string): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.api.getApplication(id).subscribe({
      next: (app) => {
        this.application.set(app);
        // Pre-fill approve form with requested values as a starting point
        this.approveForm.patchValue({
          approvedAmount:     app.requestedAmount,
          approvedTermMonths: app.requestedTermMonths,
        });
        this.isLoading.set(false);
      },
      error: () => {
        this.loadError.set('Failed to load application. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  // -------------------------------------------------------------------------
  // Actions
  // -------------------------------------------------------------------------

  evaluateRisk(): void {
    const app = this.application();
    if (!app) return;

    this.isEvaluating.set(true);

    this.api.evaluateRisk(app.id).subscribe({
      next: (result) => {
        this.notify.success(`Risk evaluation complete — score: ${result.score} (${result.decision})`);
        this.loadApplication(app.id);
        this.isEvaluating.set(false);
      },
      error: () => {
        this.notify.error('Risk evaluation failed. Please try again.');
        this.isEvaluating.set(false);
      },
    });
  }

  openApproveForm(): void {
    this.showRejectForm.set(false);
    this.showApproveForm.set(true);
  }

  openRejectForm(): void {
    this.showApproveForm.set(false);
    this.showRejectForm.set(true);
  }

  cancelForms(): void {
    this.showApproveForm.set(false);
    this.showRejectForm.set(false);
    this.approveForm.markAsUntouched();
    this.rejectForm.markAsUntouched();
  }

  submitApprove(): void {
    if (this.approveForm.invalid) {
      this.approveForm.markAllAsTouched();
      return;
    }

    const app = this.application();
    if (!app) return;

    this.isApproving.set(true);

    this.api.approveApplication(app.id, this.approveForm.getRawValue()).subscribe({
      next: (result) => {
        this.createdLoanId.set(result.loanId);
        this.notify.success('Application approved and loan created successfully.');
        this.showApproveForm.set(false);
        this.loadApplication(app.id);
        this.isApproving.set(false);
      },
      error: () => {
        this.notify.error('Failed to approve application. Please try again.');
        this.isApproving.set(false);
      },
    });
  }

  submitReject(): void {
    if (this.rejectForm.invalid) {
      this.rejectForm.markAllAsTouched();
      return;
    }

    const app = this.application();
    if (!app) return;

    this.isRejecting.set(true);

    this.api.rejectApplication(app.id, this.rejectForm.getRawValue()).subscribe({
      next: () => {
        this.notify.success('Application rejected.');
        this.showRejectForm.set(false);
        this.loadApplication(app.id);
        this.isRejecting.set(false);
      },
      error: () => {
        this.notify.error('Failed to reject application. Please try again.');
        this.isRejecting.set(false);
      },
    });
  }

  // -------------------------------------------------------------------------
  // View helpers
  // -------------------------------------------------------------------------

  statusBadgeClasses(status: LoanApplicationStatus): string {
    const map: Record<LoanApplicationStatus, string> = {
      Submitted:       'bg-primary-100 text-primary-700',
      RiskEvaluating:  'bg-warning-50 text-warning-600',
      Approved:        'bg-success-50 text-success-700',
      Rejected:        'bg-danger-50 text-danger-600',
    };
    return map[status];
  }

  riskScoreColor(score: number): string {
    if (score >= 700) return 'text-success-600';
    if (score >= 600) return 'text-warning-600';
    return 'text-danger-600';
  }

  riskBarWidth(score: number): string {
    // Scores range roughly 300–850; normalise to a percentage
    const pct = Math.min(100, Math.max(0, ((score - 300) / 550) * 100));
    return `${pct.toFixed(1)}%`;
  }

  riskBarColor(score: number): string {
    if (score >= 700) return 'bg-success-500';
    if (score >= 600) return 'bg-warning-500';
    return 'bg-danger-500';
  }

  isInvalid(form: FormGroup, field: string): boolean {
    const ctrl = form.get(field);
    return !!(ctrl?.invalid && ctrl.touched);
  }

  fieldError(form: FormGroup, field: string): string {
    const ctrl = form.get(field);
    if (!ctrl?.errors) return '';
    if (ctrl.errors['required'])   return 'This field is required.';
    if (ctrl.errors['min'])        return `Minimum value is ${ctrl.errors['min'].min}.`;
    if (ctrl.errors['max'])        return `Maximum value is ${ctrl.errors['max'].max}.`;
    if (ctrl.errors['minlength'])  return `Minimum ${ctrl.errors['minlength'].requiredLength} characters.`;
    return 'Invalid value.';
  }
}
