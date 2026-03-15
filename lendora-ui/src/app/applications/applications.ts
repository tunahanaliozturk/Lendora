import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';

import { LoanApplication, LoanApplicationStatus } from '../core/models/loan-application.model';

@Component({
  selector: 'app-applications',
  standalone: true,
  imports: [RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './applications.html',
  styleUrl: './applications.scss',
})
export class Applications {
  /**
   * Mock data seeded for the list view.
   * Replace with an ApiService call when a list endpoint is available.
   */
  readonly applications = signal<LoanApplication[]>([
    {
      id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
      customerId: 'c0ff33-dead-beef-cafe-112233445566',
      requestedAmount: 25000,
      requestedTermMonths: 36,
      annualIncome: 72000,
      existingMonthlyDebt: 450,
      interestRate: null,
      riskScore: null,
      status: 'Submitted',
      rejectionReason: null,
      approvedBy: null,
      approvedAmount: null,
      approvedTermMonths: null,
      createdAt: '2026-03-14T09:21:00Z',
      updatedAt: null,
    },
    {
      id: 'b2c3d4e5-f6a7-8901-bcde-f12345678901',
      customerId: 'deadbe-ef00-1122-3344-556677889900',
      requestedAmount: 50000,
      requestedTermMonths: 60,
      annualIncome: 98000,
      existingMonthlyDebt: 800,
      interestRate: null,
      riskScore: 710,
      status: 'RiskEvaluating',
      rejectionReason: null,
      approvedBy: null,
      approvedAmount: null,
      approvedTermMonths: null,
      createdAt: '2026-03-13T14:05:00Z',
      updatedAt: '2026-03-13T14:12:00Z',
    },
    {
      id: 'c3d4e5f6-a7b8-9012-cdef-123456789012',
      customerId: 'cafe00-1234-5678-9abc-def012345678',
      requestedAmount: 15000,
      requestedTermMonths: 24,
      annualIncome: 55000,
      existingMonthlyDebt: 300,
      interestRate: 7.5,
      riskScore: 780,
      status: 'Approved',
      rejectionReason: null,
      approvedBy: 'underwriter.jane',
      approvedAmount: 15000,
      approvedTermMonths: 24,
      createdAt: '2026-03-12T11:30:00Z',
      updatedAt: '2026-03-12T16:45:00Z',
    },
    {
      id: 'd4e5f6a7-b8c9-0123-defa-234567890123',
      customerId: 'babe00-aaaa-bbbb-cccc-ddddeeee1111',
      requestedAmount: 80000,
      requestedTermMonths: 84,
      annualIncome: 42000,
      existingMonthlyDebt: 1200,
      interestRate: null,
      riskScore: 520,
      status: 'Rejected',
      rejectionReason: 'Debt-to-income ratio exceeds policy limit of 43%.',
      approvedBy: null,
      approvedAmount: null,
      approvedTermMonths: null,
      createdAt: '2026-03-11T08:15:00Z',
      updatedAt: '2026-03-11T10:20:00Z',
    },
  ]);

  /** Truncate a UUID to first 8 chars for display. */
  truncateId(id: string): string {
    return id.substring(0, 8).toUpperCase();
  }

  statusBadgeClasses(status: LoanApplicationStatus): string {
    const map: Record<LoanApplicationStatus, string> = {
      Submitted:       'bg-primary-100 text-primary-700',
      RiskEvaluating:  'bg-warning-50 text-warning-600',
      Approved:        'bg-success-50 text-success-700',
      Rejected:        'bg-danger-50 text-danger-600',
    };
    return map[status];
  }

  statusLabel(status: LoanApplicationStatus): string {
    const map: Record<LoanApplicationStatus, string> = {
      Submitted:       'Submitted',
      RiskEvaluating:  'Risk Eval.',
      Approved:        'Approved',
      Rejected:        'Rejected',
    };
    return map[status];
  }
}
