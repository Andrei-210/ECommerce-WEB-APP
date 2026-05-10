import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { CartItem, Product } from '../models/models';

@Injectable({ providedIn: 'root' })
export class CartService {
  private _items = new BehaviorSubject<CartItem[]>([]);
  items$ = this._items.asObservable();

  get items(): CartItem[] {
    return this._items.getValue();
  }

  get totalCount(): number {
    return this._items.getValue().reduce((sum, i) => sum + i.quantity, 0);
  }

  get totalPrice(): number {
    return this._items.getValue().reduce((sum, i) => sum + i.product.price * i.quantity, 0);
  }

  addToCart(product: Product, quantity: number = 1): void {
    const current = this._items.getValue();
    const existing = current.find(i => i.product.id === product.id);
    const currentQty = existing ? existing.quantity : 0;

    // Stock validation — cannot exceed available stock
    const newQty = currentQty + quantity;
    if (newQty > product.stock) return;

    if (existing) {
      this._items.next(current.map(i =>
        i.product.id === product.id ? { ...i, quantity: newQty } : i
      ));
    } else {
      this._items.next([...current, { product, quantity }]);
    }
  }

  removeFromCart(productId: number): void {
    this._items.next(this._items.getValue().filter(i => i.product.id !== productId));
  }

  updateQuantity(productId: number, quantity: number): void {
    if (quantity <= 0) {
      this.removeFromCart(productId);
      return;
    }
    const item = this._items.getValue().find(i => i.product.id === productId);
    if (!item) return;

    // Clamp to stock
    const clamped = Math.min(quantity, item.product.stock);
    this._items.next(
      this._items.getValue().map(i =>
        i.product.id === productId ? { ...i, quantity: clamped } : i
      )
    );
  }

  clearCart(): void {
    this._items.next([]);
  }
}
