import { Routes } from '@angular/router';

export const routes: Routes = [
  // Redirect root to dashboard
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },

  // Shell layout wraps all feature pages
  {
    path: '',
    loadComponent: () =>
      import('./layout/layout').then((m) => m.Layout),
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'applications',
        loadComponent: () =>
          import('./applications/applications').then((m) => m.Applications),
      },
      {
        path: 'applications/new',
        loadComponent: () =>
          import('./applications/new-application/new-application').then(
            (m) => m.NewApplication
          ),
      },
      {
        path: 'applications/:id',
        loadComponent: () =>
          import('./applications/application-detail/application-detail').then(
            (m) => m.ApplicationDetail
          ),
      },
      {
        path: 'loans',
        loadComponent: () =>
          import('./loans/loans').then((m) => m.Loans),
      },
      {
        path: 'loans/:id',
        loadComponent: () =>
          import('./loans/loan-detail/loan-detail').then((m) => m.LoanDetail),
      },
      {
        path: 'payments',
        loadComponent: () =>
          import('./payments/payments').then((m) => m.Payments),
      },
    ],
  },
];
