import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private _isLoggedIn = new BehaviorSubject<boolean>(this.hasToken());
  private _username = new BehaviorSubject<string>(this.getStoredUsername());

  isLoggedIn$ = this._isLoggedIn.asObservable();
  username$ = this._username.asObservable();

  constructor(private http: HttpClient) {}

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data).pipe(
      tap(response => this.storeAuth(response))
    );
  }

  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data).pipe(
      tap(response => this.storeAuth(response))
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
    this._isLoggedIn.next(false);
    this._username.next('');
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getCurrentUsername(): string {
    return localStorage.getItem('username') ?? '';
  }

  private storeAuth(response: AuthResponse): void {
    localStorage.setItem('token', response.token);
    localStorage.setItem('username', response.username);
    this._isLoggedIn.next(true);
    this._username.next(response.username);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem('token');
  }

  private getStoredUsername(): string {
    return localStorage.getItem('username') ?? '';
  }
}
