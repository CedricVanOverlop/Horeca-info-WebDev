import { Component } from '@angular/core';
import { AuthStateService } from '../../services/auth-state.service';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [RouterLink],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  constructor(private authStateService: AuthStateService, private router: Router) {}

  logout(): void {
    this.authStateService.logout();
  }
}
