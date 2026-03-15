import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet, Router, NavigationEnd, Event } from '@angular/router';
import { filter, Subscription } from 'rxjs';

import { Toast } from '../shared/toast/toast';

interface NavItem {
  label: string;
  route: string;
  title: string;
}

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Toast],
  templateUrl: './layout.html',
  styleUrl: './layout.scss',
})
export class Layout implements OnInit, OnDestroy {
  readonly pageTitle = signal<string>('Dashboard');

  readonly navItems: NavItem[] = [
    { label: 'Dashboard',    route: '/dashboard',    title: 'Dashboard'    },
    { label: 'Applications', route: '/applications', title: 'Applications' },
    { label: 'Loans',        route: '/loans',        title: 'Loans'        },
    { label: 'Payments',     route: '/payments',     title: 'Payments'     },
  ];

  private routerSub!: Subscription;

  constructor(private readonly router: Router) {}

  ngOnInit(): void {
    // Set title on initial load
    this.updateTitle(this.router.url);

    this.routerSub = this.router.events
      .pipe(filter((e: Event): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe((e: NavigationEnd) => this.updateTitle(e.urlAfterRedirects));
  }

  ngOnDestroy(): void {
    this.routerSub?.unsubscribe();
  }

  private updateTitle(url: string): void {
    const matched = this.navItems.find(item =>
      url.startsWith(item.route)
    );
    this.pageTitle.set(matched?.title ?? 'Lendora');
  }
}
