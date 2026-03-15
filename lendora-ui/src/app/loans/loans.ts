import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';

import { Loan, LoanStatus } from '../core/models/loan.model';

/** Truncate a string to maxLen chars, appending '...' when shortened. */
function truncate(value: string, maxLen: number): string {
  return value.length > maxLen ? value.slice(0, maxLen) + '...' : value;
}

@Component({
  selector: 'app-loans',
  standalone: true,
  imports: [RouterLink, CurrencyPipe, DatePipe, DecimalPipe],
  templateUrl: './loans.html',
  styleUrl: './loans.scss',
})
export class Loans {
  /** Raw mock dataset — in a real app this would come from ApiService. */
  private readonly allLoans = signal<Loan[]>([
    {
      id: 'LN-00000001-ABCD',
      applicationId: 'APP-00000001-ABCD',
      customerId: 'CUST-1001',
      approvedAmount: 25000,
      interestRate: 7.5,
      termMonths: 36,
      monthlyPayment: 777.28,
      startDate: '2025-09-01',
      status: 'Active',
      remainingBalance: 21500,
    },
    {
      id: 'LN-00000002-EFGH',
      applicationId: 'APP-00000002-EFGH',
      customerId: 'CUST-1042',
      approvedAmount: 10000,
      interestRate: 9.25,
      termMonths: 24,
      monthlyPayment: 457.33,
      startDate: '2025-06-15',
      status: 'Delinquent',
      remainingBalance: 7200,
    },
    {
      id: 'LN-00000003-IJKL',
      applicationId: 'APP-00000003-IJKL',
      customerId: 'CUST-0875',
      approvedAmount: 5000,
      interestRate: 6.0,
      termMonths: 12,
      monthlyPayment: 430.33,
      startDate: '2024-03-01',
      status: 'Closed',
      remainingBalance: 0,
    },
  ]);

  /** Current value of the customer-ID search input. */
  readonly searchQuery = signal<string>('');

  /** Loans filtered by the search query (case-insensitive customer ID match). */
  readonly filteredLoans = computed<Loan[]>(() => {
    const query = this.searchQuery().trim().toLowerCase();
    if (!query) return this.allLoans();
    return this.allLoans().filter((loan) =>
      loan.customerId.toLowerCase().includes(query)
    );
  });

  onSearch(event: Event): void {
    this.searchQuery.set((event.target as HTMLInputElement).value);
  }

  truncateId(value: string): string {
    return truncate(value, 14);
  }

  /** CSS classes for the status badge. */
  statusClasses(status: LoanStatus): string {
    const map: Record<LoanStatus, string> = {
      Active:     'bg-success-50 text-success-700 ring-1 ring-success-500/30',
      Delinquent: 'bg-danger-50 text-danger-700 ring-1 ring-danger-500/30',
      Closed:     'bg-slate-100 text-slate-500 ring-1 ring-slate-300/50',
    };
    return map[status];
  }
}
