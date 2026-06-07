import { Component } from '@angular/core';
import { AuthStateService } from '../../services/auth-state.service';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [CommonModule, RouterLink],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  constructor(private authStateService: AuthStateService, private router: Router) {}

  logout(): void {
    this.authStateService.logout();
  }
}
