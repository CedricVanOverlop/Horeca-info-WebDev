import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd, RouterOutlet } from '@angular/router';
import { AuthStateService } from './services/auth-state.service';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from './components/header/header.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule, HeaderComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  title = 'horeca-info';
  showHeader: boolean = false;

  constructor(
    private router: Router,
    private authStateService: AuthStateService
  ) {}

  ngOnInit(): void {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.showHeader = !event.urlAfterRedirects.includes('/login')
          && !event.urlAfterRedirects.includes('/register');

        if (!this.authStateService.isLoggedIn
          && event.urlAfterRedirects !== '/login' && event.urlAfterRedirects !== '/register') {
          this.router.navigate(['/login']);
        }
      }
    });
  }
}
