// ---------------------------------------------------------------------------
// Request / command payloads
// ---------------------------------------------------------------------------

/** Payload sent when recording a loan repayment. */
export interface PaymentRequest {
  loanId: string;

  /** Amount tendered for this payment. */
  amount: number;

  /**
   * ISO-8601 datetime string representing when the payment was received.
   * Example: "2026-03-15T10:30:00Z"
   */
  paidAt: string;

  /**
   * Optional external reference (e.g. bank transaction ID).
   * Pass null when no reference is available.
   */
  paymentReference: string | null;
}

// ---------------------------------------------------------------------------
// Response payloads
// ---------------------------------------------------------------------------

/** Result returned after a payment is successfully registered. */
export interface PaymentResult {
  /** Newly created payment record ID. */
  paymentId: string;

  /** Updated outstanding principal balance on the loan after this payment. */
  remainingBalance: number;
}
