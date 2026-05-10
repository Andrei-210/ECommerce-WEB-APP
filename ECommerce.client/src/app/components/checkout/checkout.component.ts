import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { OrderResponse } from '../../models/models';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './checkout.component.html'
})
export class CheckoutComponent {
  shippingAddress = '';
  city = '';
  postalCode = '';

  loading = false;
  error = '';
  success: OrderResponse | null = null;

  constructor(
    public cartService: CartService,
    private orderService: OrderService,
    private router: Router
  ) {}

  get cartItems() {
    return this.cartService.items;
  }

  get total(): number {
    return this.cartService.totalPrice;
  }

  placeOrder(): void {
    if (!this.shippingAddress.trim() || !this.city.trim() || !this.postalCode.trim()) {
      this.error = 'Please fill in all address fields.';
      return;
    }

    if (this.cartItems.length === 0) {
      this.error = 'Your cart is empty.';
      return;
    }

    this.loading = true;
    this.error = '';

    const request = {
      shippingAddress: this.shippingAddress,
      city: this.city,
      postalCode: this.postalCode,
      items: this.cartItems.map(i => ({
        productId: i.product.id,
        quantity: i.quantity
      }))
    };

    this.orderService.checkout(request).subscribe({
      next: response => {
        this.success = response;
        this.cartService.clearCart();
        this.loading = false;
      },
      error: err => {
        this.error = err.error?.message || 'Checkout failed. Please try again.';
        this.loading = false;
      }
    });
  }
}
