import {
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe, DecimalPipe, PercentPipe } from '@angular/common';
import { forkJoin } from 'rxjs';

import { ApiService } from '../../core/services/api.service';
import { NotificationService } from '../../core/services/notification.service';
import { Loan, InstallmentStatus, RepaymentSchedule } from '../../core/models/loan.model';
import { LoanStatus } from '../../core/models/loan.model';

@Component({
  selector: 'app-loan-detail',
  standalone: true,
  imports: [RouterLink, CurrencyPipe, DatePipe, DecimalPipe],
  templateUrl: './loan-detail.html',
  styleUrl: './loan-detail.scss',
})
export class LoanDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(ApiService);
  private readonly notify = inject(NotificationService);

  readonly loan = signal<Loan | null>(null);
  readonly schedule = signal<RepaymentSchedule | null>(null);
  readonly loading = signal<boolean>(true);
  readonly error = signal<string | null>(null);

  /**
   * Percentage of the loan that has been repaid.
   * Formula: (approvedAmount - remainingBalance) / approvedAmount * 100
   */
  readonly paidPercent = computed<number>(() => {
    const l = this.loan();
    if (!l || l.approvedAmount === 0) return 0;
    const paid = l.approvedAmount - l.remainingBalance;
    return Math.min(100, Math.max(0, (paid / l.approvedAmount) * 100));
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('No loan ID provided.');
      this.loading.set(false);
      return;
    }

    forkJoin({
      loan: this.api.getLoan(id),
      schedule: this.api.getLoanSchedule(id),
    }).subscribe({
      next: ({ loan, schedule }) => {
        this.loan.set(loan);
        this.schedule.set(schedule);
        this.loading.set(false);
      },
      error: (err) => {
        const message = err?.message ?? 'Failed to load loan details.';
        this.error.set(message);
        this.notify.error(message);
        this.loading.set(false);
      },
    });
  }

  goToPayment(): void {
    const loanId = this.loan()?.id;
    if (loanId) {
      this.router.navigate(['/payments'], { queryParams: { loanId } });
    }
  }

  /** CSS classes for the loan-level status badge. */
  loanStatusClasses(status: LoanStatus): string {
    const map: Record<LoanStatus, string> = {
      Active:     'bg-success-50 text-success-700 ring-1 ring-success-500/30',
      Delinquent: 'bg-danger-50 text-danger-700 ring-1 ring-danger-500/30',
      Closed:     'bg-slate-100 text-slate-500 ring-1 ring-slate-300/50',
    };
    return map[status];
  }

  /** CSS classes for installment status badges. */
  installmentStatusClasses(status: InstallmentStatus): string {
    const map: Record<InstallmentStatus, string> = {
      Scheduled:     'bg-primary-50 text-primary-700 ring-1 ring-primary-500/30',
      Paid:          'bg-success-50 text-success-700 ring-1 ring-success-500/30',
      PartiallyPaid: 'bg-warning-50 text-warning-600 ring-1 ring-warning-500/30',
      Late:          'bg-danger-50 text-danger-700 ring-1 ring-danger-500/30',
    };
    return map[status];
  }

  /** Background highlight for late installment rows. */
  rowClasses(status: InstallmentStatus, odd: boolean): string {
    if (status === 'Late') return 'bg-danger-50';
    return odd ? 'bg-slate-50' : '';
  }
}
