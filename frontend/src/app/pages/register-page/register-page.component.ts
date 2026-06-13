import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthStateService } from '../../services/auth-state.service';
import { RegisterRequest } from '../../services/api/models/register-request.model';
import { NavbarComponent } from '../../components/navbar/navbar.component';

@Component({
  selector: 'app-register-page',
  imports: [ReactiveFormsModule, RouterLink, NavbarComponent],
  templateUrl: './register-page.component.html',
  styleUrls: ['./register-page.component.css']
})
export class RegisterPageComponent implements OnInit {
  registerForm!: FormGroup;
  serverError: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authStateService: AuthStateService
  ) { }

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      nom: ['', Validators.required],
      prenom: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      motDePasse: ['', Validators.required],
      telephone: ['']
    });
  }

  onSubmit(): void {
    this.serverError = null;
    if (this.registerForm.valid) {
      const { nom, prenom, email, motDePasse, telephone } = this.registerForm.value;
      const credentials: RegisterRequest = { nom, prenom, email, motDePasse, telephone: telephone || undefined };
      this.authStateService.register(credentials).subscribe({
        error: err => {
          this.serverError = err?.status === 409
            ? 'Cet email est déjà utilisé.'
            : 'Une erreur est survenue. Veuillez réessayer.';
        }
      });
    } else {
      this.registerForm.markAllAsTouched();
    }
  }

  get nom() { return this.registerForm.get('nom'); }
  get prenom() { return this.registerForm.get('prenom'); }
  get email() { return this.registerForm.get('email'); }
  get motDePasse() { return this.registerForm.get('motDePasse'); }
  get telephone() { return this.registerForm.get('telephone'); }
}
