import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Product } from '../../models/models';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './product-list.component.html'
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  filteredProducts: Product[] = [];

  // Filter state
  categories: string[] = [];
  brands: string[] = [];
  selectedCategory = 'All';
  selectedBrands: Set<string> = new Set();
  minPrice = 0;
  maxPrice = 10000;
  priceRangeMax = 10000;
  priceRangeMin = 0;
  currentMinPrice = 0;
  currentMaxPrice = 10000;

  loading = true;
  error = '';
  addedMap: Record<number, boolean> = {};

  constructor(
    private productService: ProductService,
    private cartService: CartService
  ) {}

  ngOnInit(): void {
    this.productService.getAll().subscribe({
      next: products => {
        this.products = products;
        this.categories = ['All', ...new Set(products.map(p => p.category))];
        this.brands = [...new Set(products.map(p => p.brand))].sort();
        const prices = products.map(p => p.price);
        this.priceRangeMin = Math.floor(Math.min(...prices));
        this.priceRangeMax = Math.ceil(Math.max(...prices));
        this.currentMinPrice = this.priceRangeMin;
        this.currentMaxPrice = this.priceRangeMax;
        this.loading = false;
        this.applyFilters();
      },
      error: () => {
        this.error = 'Failed to load products. Make sure the API is running.';
        this.loading = false;
      }
    });
  }

  filterByCategory(category: string): void {
    this.selectedCategory = category;
    this.applyFilters();
  }

  toggleBrand(brand: string): void {
    if (this.selectedBrands.has(brand)) {
      this.selectedBrands.delete(brand);
    } else {
      this.selectedBrands.add(brand);
    }
    this.applyFilters();
  }

  onPriceChange(): void {
    if (this.currentMinPrice > this.currentMaxPrice) {
      this.currentMaxPrice = this.currentMinPrice;
    }
    this.applyFilters();
  }

  applyFilters(): void {
    this.filteredProducts = this.products.filter(p => {
      const catMatch = this.selectedCategory === 'All' || p.category === this.selectedCategory;
      const brandMatch = this.selectedBrands.size === 0 || this.selectedBrands.has(p.brand);
      const priceMatch = p.price >= this.currentMinPrice && p.price <= this.currentMaxPrice;
      return catMatch && brandMatch && priceMatch;
    });
  }

  clearFilters(): void {
    this.selectedCategory = 'All';
    this.selectedBrands.clear();
    this.currentMinPrice = this.priceRangeMin;
    this.currentMaxPrice = this.priceRangeMax;
    this.applyFilters();
  }

  get activeFilterCount(): number {
    let count = 0;
    if (this.selectedCategory !== 'All') count++;
    count += this.selectedBrands.size;
    if (this.currentMinPrice > this.priceRangeMin || this.currentMaxPrice < this.priceRangeMax) count++;
    return count;
  }

  addToCart(product: Product): void {
    if (product.stock === 0) return;
    const inCart = this.cartService.items.find(i => i.product.id === product.id)?.quantity ?? 0;
    if (inCart >= product.stock) return;
    this.cartService.addToCart(product);
    this.addedMap[product.id] = true;
    setTimeout(() => { this.addedMap[product.id] = false; }, 1500);
  }

  getCartQty(productId: number): number {
    return this.cartService.items.find(i => i.product.id === productId)?.quantity ?? 0;
  }

  stars(rating: number): boolean[] {
    return [1,2,3,4,5].map(s => s <= Math.round(rating));
  }
}
