import { TestBed } from '@angular/core/testing';
import { CartService } from './cart.service';
import { Product } from '../models/models';

const mockProduct: Product = {
  id: 1,
  name: 'Dell XPS 15',
  description: 'A great laptop',
  price: 5999.99,
  stock: 10,
  category: 'Laptops',
  brand: 'Dell',
  imageUrl: ''
};

const mockProduct2: Product = {
  id: 2,
  name: 'Logitech Mouse',
  description: 'A great mouse',
  price: 349.99,
  stock: 20,
  category: 'Peripherals',
  brand: 'Logitech',
  imageUrl: ''
};

describe('CartService', () => {
  let service: CartService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CartService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should start with an empty cart', () => {
    expect(service.items.length).toBe(0);
    expect(service.totalCount).toBe(0);
    expect(service.totalPrice).toBe(0);
  });

  it('should add a product to the cart', () => {
    service.addToCart(mockProduct);
    expect(service.items.length).toBe(1);
    expect(service.items[0].product.id).toBe(1);
    expect(service.items[0].quantity).toBe(1);
  });

  it('should increment quantity when adding the same product twice', () => {
    service.addToCart(mockProduct);
    service.addToCart(mockProduct);
    expect(service.items.length).toBe(1);
    expect(service.items[0].quantity).toBe(2);
  });

  it('should calculate total count correctly', () => {
    service.addToCart(mockProduct, 2);
    service.addToCart(mockProduct2, 3);
    expect(service.totalCount).toBe(5);
  });

  it('should calculate total price correctly', () => {
    service.addToCart(mockProduct, 1);   // 5999.99
    service.addToCart(mockProduct2, 2);  // 349.99 * 2 = 699.98
    expect(service.totalPrice).toBeCloseTo(6699.97, 2);
  });

  it('should remove a product from the cart', () => {
    service.addToCart(mockProduct);
    service.addToCart(mockProduct2);
    service.removeFromCart(1);
    expect(service.items.length).toBe(1);
    expect(service.items[0].product.id).toBe(2);
  });

  it('should update quantity of an existing item', () => {
    service.addToCart(mockProduct);
    service.updateQuantity(1, 5);
    expect(service.items[0].quantity).toBe(5);
  });

  it('should remove item when updating quantity to 0', () => {
    service.addToCart(mockProduct);
    service.updateQuantity(1, 0);
    expect(service.items.length).toBe(0);
  });

  it('should clear the cart', () => {
    service.addToCart(mockProduct);
    service.addToCart(mockProduct2);
    service.clearCart();
    expect(service.items.length).toBe(0);
    expect(service.totalPrice).toBe(0);
  });

  it('should emit updated items via items$ observable', (done) => {
    service.items$.subscribe(items => {
      if (items.length > 0) {
        expect(items[0].product.name).toBe('Dell XPS 15');
        done();
      }
    });
    service.addToCart(mockProduct);
  });
});
