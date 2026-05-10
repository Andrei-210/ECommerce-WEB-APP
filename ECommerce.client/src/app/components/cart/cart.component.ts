import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Observable } from 'rxjs';
import { CartItem } from '../../models/models';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './cart.component.html'
})
export class CartComponent {
  items$: Observable<CartItem[]>;

  constructor(public cartService: CartService) {
    this.items$ = cartService.items$;
  }

  updateQuantity(productId: number, event: Event): void {
    const value = +(event.target as HTMLInputElement).value;
    this.cartService.updateQuantity(productId, value);
  }

  remove(productId: number): void {
    this.cartService.removeFromCart(productId);
  }

  get total(): number {
    return this.cartService.totalPrice;
  }
}
