import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterRequest } from '../../../models/auth.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class RegisterComponent {
  registerData: RegisterRequest = { fullName: '', email: '', password: '' };
  isLoading = signal(false);
  errorMessages = signal<string[]>([]);

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    this.isLoading.set(true);
    this.errorMessages.set([]);

    this.authService.register(this.registerData).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/projects']);
      },
      error: (err) => {
        this.isLoading.set(false);
        const errors = err.error?.errors as string[] | undefined;
        this.errorMessages.set(
          errors ?? [err.error?.message ?? 'حدث خطأ أثناء إنشاء الحساب']
        );
      }
    });
  }
}
