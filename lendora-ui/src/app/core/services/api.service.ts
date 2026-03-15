import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  ApproveRequest,
  ApproveResult,
  LoanApplication,
  LoanApplicationRequest,
  RejectRequest,
  RiskEvaluationResult,
} from '../models/loan-application.model';
import {
  CustomerLoanSummary,
  Loan,
  RepaymentSchedule,
} from '../models/loan.model';
import { PaymentRequest, PaymentResult } from '../models/payment.model';

/**
 * Base URL for all API calls.
 * Sourced from the environment file so it can be overridden per build target
 * (e.g. point to a staging server in the development environment).
 */
const API_BASE_URL = environment.apiBaseUrl;

/**
 * ApiService
 *
 * Thin wrapper around HttpClient that maps each backend endpoint to a typed
 * Observable. All methods are intentionally side-effect free — callers are
 * responsible for subscribing and handling errors (e.g. via an interceptor or
 * an explicit catchError in the component/effect layer).
 *
 * Endpoint map
 * ────────────
 * POST   /loan-applications                          → submitApplication
 * GET    /loan-applications/:id                      → getApplication
 * POST   /loan-applications/:id/evaluate-risk        → evaluateRisk
 * POST   /loan-applications/:id/approve              → approveApplication
 * POST   /loan-applications/:id/reject               → rejectApplication
 * GET    /loans/:id                                  → getLoan
 * GET    /loans/:id/schedule                         → getLoanSchedule
 * GET    /customers/:customerId/loans                → getCustomerLoans
 * POST   /payments                                   → registerPayment
 */
@Injectable({ providedIn: 'root' })
export class ApiService {
  // Angular 14+ functional injection — no constructor needed.
  private readonly http = inject(HttpClient);

  // -------------------------------------------------------------------------
  // Loan Applications
  // -------------------------------------------------------------------------

  /**
   * Submit a new loan application on behalf of a customer.
   *
   * @returns Observable that emits the server-assigned application ID.
   */
  submitApplication(
    request: LoanApplicationRequest
  ): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(
      `${API_BASE_URL}/loan-applications`,
      request
    );
  }

  /**
   * Fetch a single loan application by its ID.
   */
  getApplication(id: string): Observable<LoanApplication> {
    return this.http.get<LoanApplication>(
      `${API_BASE_URL}/loan-applications/${id}`
    );
  }

  /**
   * Trigger the asynchronous risk-evaluation engine for an application.
   * The backend may process this synchronously or queue it; callers should
   * poll getApplication() if the status remains 'RiskEvaluating'.
   */
  evaluateRisk(applicationId: string): Observable<RiskEvaluationResult> {
    return this.http.post<RiskEvaluationResult>(
      `${API_BASE_URL}/loan-applications/${applicationId}/evaluate-risk`,
      {}
    );
  }

  /**
   * Approve an application. On success the backend creates a Loan record and
   * returns both the application ID and the new loan ID.
   */
  approveApplication(
    id: string,
    request: ApproveRequest
  ): Observable<ApproveResult> {
    return this.http.post<ApproveResult>(
      `${API_BASE_URL}/loan-applications/${id}/approve`,
      request
    );
  }

  /**
   * Reject an application with a mandatory reason.
   * Returns void — the caller can re-fetch the application if the updated
   * state is needed.
   */
  rejectApplication(id: string, request: RejectRequest): Observable<void> {
    return this.http.post<void>(
      `${API_BASE_URL}/loan-applications/${id}/reject`,
      request
    );
  }

  // -------------------------------------------------------------------------
  // Loans
  // -------------------------------------------------------------------------

  /**
   * Fetch a full loan record by ID.
   */
  getLoan(id: string): Observable<Loan> {
    return this.http.get<Loan>(`${API_BASE_URL}/loans/${id}`);
  }

  /**
   * Fetch the complete repayment schedule (all installments) for a loan.
   */
  getLoanSchedule(id: string): Observable<RepaymentSchedule> {
    return this.http.get<RepaymentSchedule>(
      `${API_BASE_URL}/loans/${id}/schedule`
    );
  }

  /**
   * List all loans belonging to a customer.
   * Returns lightweight summary objects — use getLoan() for full details.
   */
  getCustomerLoans(customerId: string): Observable<CustomerLoanSummary[]> {
    return this.http.get<CustomerLoanSummary[]>(
      `${API_BASE_URL}/customers/${customerId}/loans`
    );
  }

  // -------------------------------------------------------------------------
  // Payments
  // -------------------------------------------------------------------------

  /**
   * Register a payment against a loan.
   * The backend applies the payment, updates installment statuses, and returns
   * the new remaining balance.
   */
  registerPayment(request: PaymentRequest): Observable<PaymentResult> {
    return this.http.post<PaymentResult>(
      `${API_BASE_URL}/payments`,
      request
    );
  }
}
