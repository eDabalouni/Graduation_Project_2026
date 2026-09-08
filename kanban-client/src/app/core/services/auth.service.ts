import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { RegisterRequest, LoginRequest, AuthResponse } from '../../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly tokenKey = 'kanban_token';

  currentUserName = signal<string | null>(null);

  constructor(private http: HttpClient) {
    const savedName = localStorage.getItem('kanban_user_name');
    if (savedName) {
      this.currentUserName.set(savedName);
    }
  }

  register(dto: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, dto).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  login(dto: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, dto).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem('kanban_user_name');
    this.currentUserName.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  private handleAuthSuccess(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem('kanban_user_name', response.fullName);
    this.currentUserName.set(response.fullName);
  }
}