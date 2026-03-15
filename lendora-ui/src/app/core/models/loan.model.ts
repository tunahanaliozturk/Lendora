/**
 * Lifecycle states for an active loan.
 *
 * Active      – loan is in good standing with on-time payments.
 * Delinquent  – one or more installments are overdue.
 * Closed      – loan has been fully repaid.
 */
export type LoanStatus = 'Active' | 'Delinquent' | 'Closed';

/**
 * Lifecycle states for a single repayment installment.
 *
 * Scheduled      – not yet due.
 * Paid           – paid in full on or before the due date.
 * PartiallyPaid  – a partial payment was made; a balance remains.
 * Late           – the due date has passed without full payment.
 */
export type InstallmentStatus = 'Scheduled' | 'Paid' | 'PartiallyPaid' | 'Late';

// ---------------------------------------------------------------------------
// Loan entity
// ---------------------------------------------------------------------------

/** Full loan entity returned from the API after an application is approved. */
export interface Loan {
  id: string;
  applicationId: string;
  customerId: string;
  approvedAmount: number;
  interestRate: number;
  termMonths: number;
  monthlyPayment: number;

  /** ISO-8601 date string for when repayments begin. */
  startDate: string;

  status: LoanStatus;

  /** Current outstanding principal balance. */
  remainingBalance: number;
}

// ---------------------------------------------------------------------------
// Repayment schedule
// ---------------------------------------------------------------------------

/** A single repayment installment within a loan's schedule. */
export interface Installment {
  installmentNumber: number;

  /** ISO-8601 date string. */
  dueDate: string;

  /** Principal portion of this installment. */
  principalAmount: number;

  /** Interest portion of this installment. */
  interestAmount: number;

  /** principalAmount + interestAmount. */
  totalAmount: number;

  /** Amount already paid against this installment (0 if unpaid). */
  paidAmount: number;

  status: InstallmentStatus;
}

/** Full repayment schedule for a loan, indexed by loan ID. */
export interface RepaymentSchedule {
  loanId: string;
  installments: Installment[];
}

// ---------------------------------------------------------------------------
// Summary / list projection
// ---------------------------------------------------------------------------

/**
 * Lightweight projection used when listing a customer's loans.
 * Avoids fetching full schedule data for every loan in a list view.
 */
export interface CustomerLoanSummary {
  id: string;
  approvedAmount: number;
  status: LoanStatus;

  /** ISO-8601 date string. */
  startDate: string;

  monthlyPayment: number;
}
