import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthStateService } from '../../services/auth-state.service';
import { AuthenticationRequest } from '../../services/api/models/authentication-request.model';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-login-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.css']
})
export class LoginPageComponent implements OnInit {
  loginForm!: FormGroup;
  serverError: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authStateService: AuthStateService
  ) { }

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      motDePasse: ['', Validators.required]
    });
  }

  onSubmit(): void {
    this.serverError = null;
    if (this.loginForm.valid) {
      const credentials: AuthenticationRequest = this.loginForm.value;
      this.authStateService.login(credentials).subscribe({
        error: err => {
          this.serverError = err?.status === 401
            ? 'Email ou mot de passe incorrect.'
            : 'Une erreur est survenue. Veuillez réessayer.';
        }
      });
    } else {
      this.loginForm.markAllAsTouched();
    }
  }

  get email() { return this.loginForm.get('email'); }
  get motDePasse() { return this.loginForm.get('motDePasse'); }
}
