/**
 * Status values that a loan application can move through during its lifecycle.
 *
 * Submitted       – initial state after the customer submits the form.
 * RiskEvaluating  – the risk-scoring engine is processing the application.
 * Approved        – an underwriter approved the application and a Loan was created.
 * Rejected        – the application was declined (see rejectionReason).
 */
export type LoanApplicationStatus =
  | 'Submitted'
  | 'RiskEvaluating'
  | 'Approved'
  | 'Rejected';

// ---------------------------------------------------------------------------
// Request / command payloads
// ---------------------------------------------------------------------------

/** Payload sent when a customer submits a new loan application. */
export interface LoanApplicationRequest {
  customerId: string;
  requestedAmount: number;
  requestedTermMonths: number;
  annualIncome: number;
  existingMonthlyDebt: number;
}

/**
 * Payload sent by an underwriter to approve an application.
 * The approved figures may differ from what the customer originally requested.
 */
export interface ApproveRequest {
  approvedBy: string;
  interestRate: number;
  approvedAmount: number;
  approvedTermMonths: number;
}

/** Payload sent by an underwriter to reject an application. */
export interface RejectRequest {
  rejectedBy: string;
  reason: string;
}

// ---------------------------------------------------------------------------
// Response / query payloads
// ---------------------------------------------------------------------------

/** Full loan application entity returned from the API. */
export interface LoanApplication {
  id: string;
  customerId: string;
  requestedAmount: number;
  requestedTermMonths: number;
  annualIncome: number;
  existingMonthlyDebt: number;

  /** Populated after an underwriter approves the application. */
  interestRate: number | null;

  /** Populated after the risk-evaluation step completes. */
  riskScore: number | null;

  status: LoanApplicationStatus;

  /** Human-readable reason provided when the application is rejected. */
  rejectionReason: string | null;

  /** Username / ID of the underwriter who approved the application. */
  approvedBy: string | null;

  /** May differ from requestedAmount (underwriter can reduce the amount). */
  approvedAmount: number | null;

  /** May differ from requestedTermMonths. */
  approvedTermMonths: number | null;

  /** ISO-8601 timestamp string. */
  createdAt: string;

  /** ISO-8601 timestamp string; null if the record has never been updated. */
  updatedAt: string | null;
}

/** Result returned by the risk-evaluation endpoint. */
export interface RiskEvaluationResult {
  score: number;
  decision: string;
  status: string;
}

/**
 * Result returned after an application is successfully approved.
 * Contains both the application ID and the newly-created Loan ID.
 */
export interface ApproveResult {
  applicationId: string;
  loanId: string;
}
