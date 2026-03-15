import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';


interface StatCard {
  label: string;
  value: string | number;
  icon: string;
  accentBg: string;
  accentText: string;
  trend: string;
  trendPositive: boolean;
}

interface RecentActivity {
  id: string;
  description: string;
  time: string;
  type: 'application' | 'approval' | 'payment' | 'rejection';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  readonly stats: StatCard[] = [
    {
      label: 'Total Applications',
      value: 248,
      icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
      accentBg: 'bg-primary-50',
      accentText: 'text-primary-600',
      trend: '+12% this month',
      trendPositive: true,
    },
    {
      label: 'Approved Loans',
      value: 186,
      icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
      accentBg: 'bg-success-50',
      accentText: 'text-success-600',
      trend: '+8% this month',
      trendPositive: true,
    },
    {
      label: 'Active Loans',
      value: 142,
      icon: 'M13 10V3L4 14h7v7l9-11h-7z',
      accentBg: 'bg-warning-50',
      accentText: 'text-warning-600',
      trend: '3 new today',
      trendPositive: true,
    },
    {
      label: 'Pending Review',
      value: 21,
      icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z',
      accentBg: 'bg-purple-50',
      accentText: 'text-purple-600',
      trend: '5 require action',
      trendPositive: false,
    },
  ];

  readonly recentActivity: RecentActivity[] = [
    {
      id: 'app-001',
      description: 'New application submitted by customer CUST-1042',
      time: '2 minutes ago',
      type: 'application',
    },
    {
      id: 'app-002',
      description: 'Application APP-0089 approved — Loan LN-0186 created',
      time: '18 minutes ago',
      type: 'approval',
    },
    {
      id: 'app-003',
      description: 'Payment of $1,250.00 registered for Loan LN-0174',
      time: '1 hour ago',
      type: 'payment',
    },
    {
      id: 'app-004',
      description: 'Application APP-0087 rejected — insufficient income',
      time: '3 hours ago',
      type: 'rejection',
    },
    {
      id: 'app-005',
      description: 'Risk evaluation completed for APP-0091 — score 720',
      time: '5 hours ago',
      type: 'application',
    },
  ];

  activityIcon(type: RecentActivity['type']): string {
    const icons: Record<RecentActivity['type'], string> = {
      application: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
      approval:    'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
      payment:     'M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z',
      rejection:   'M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z',
    };
    return icons[type];
  }

  activityIconBg(type: RecentActivity['type']): string {
    const colors: Record<RecentActivity['type'], string> = {
      application: 'bg-primary-100 text-primary-600',
      approval:    'bg-success-50 text-success-600',
      payment:     'bg-warning-50 text-warning-600',
      rejection:   'bg-danger-50 text-danger-600',
    };
    return colors[type];
  }
}
