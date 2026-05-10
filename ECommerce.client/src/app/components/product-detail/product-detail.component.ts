import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Product } from '../../models/models';
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './product-detail.component.html'
})
export class ProductDetailComponent implements OnInit {
  product: Product | null = null;
  loading = true;
  error = '';

  // Cart
  quantity = 1;
  addedToCart = false;

  // Review form
  isLoggedIn = false;
  reviewRating = 0;
  reviewHoverRating = 0;
  reviewTitle = '';
  reviewBody = '';
  reviewLoading = false;
  reviewError = '';
  reviewSuccess = '';
  hasReviewed = false;

  activeTab: 'description' | 'specs' | 'reviews' = 'description';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    public cartService: CartService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.isLoggedIn$.subscribe(v => (this.isLoggedIn = v));

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) { this.router.navigate(['/products']); return; }

    this.productService.getById(id).subscribe({
      next: product => {
        this.product = product;
        this.loading = false;
        this.checkIfReviewed();
      },
      error: () => {
        this.error = 'Product not found.';
        this.loading = false;
      }
    });
  }

  private checkIfReviewed(): void {
    if (!this.product || !this.isLoggedIn) return;
    // If backend returns username matching current user, mark as reviewed
    const username = this.authService.getCurrentUsername();
    this.hasReviewed = this.product.reviews.some(r => r.username === username);
  }

  get cartQty(): number {
    if (!this.product) return 0;
    return this.cartService.items.find(i => i.product.id === this.product!.id)?.quantity ?? 0;
  }

  get maxAddable(): number {
    if (!this.product) return 0;
    return this.product.stock - this.cartQty;
  }

  addToCart(): void {
    if (!this.product) return;
    if (this.quantity > this.maxAddable) return;
    this.cartService.addToCart(this.product, this.quantity);
    this.addedToCart = true;
    this.quantity = 1;
    setTimeout(() => (this.addedToCart = false), 2000);
  }

  setRating(rating: number): void { this.reviewRating = rating; }
  setHoverRating(rating: number): void { this.reviewHoverRating = rating; }
  clearHover(): void { this.reviewHoverRating = 0; }

  get displayRating(): number {
    return this.reviewHoverRating || this.reviewRating;
  }

  submitReview(): void {
    if (!this.product) return;
    if (this.reviewRating === 0) { this.reviewError = 'Please select a rating.'; return; }
    if (!this.reviewTitle.trim()) { this.reviewError = 'Please enter a review title.'; return; }

    this.reviewLoading = true;
    this.reviewError = '';

    this.productService.createReview(this.product.id, {
      rating: this.reviewRating,
      title: this.reviewTitle.trim(),
      body: this.reviewBody.trim()
    }).subscribe({
      next: review => {
        this.product!.reviews.unshift(review);
        this.product!.reviewCount++;
        this.product!.averageRating =
          this.product!.reviews.reduce((s, r) => s + r.rating, 0) / this.product!.reviews.length;
        this.reviewSuccess = 'Your review has been submitted!';
        this.hasReviewed = true;
        this.reviewRating = 0;
        this.reviewTitle = '';
        this.reviewBody = '';
        this.reviewLoading = false;
      },
      error: err => {
        this.reviewError = err.error?.message || 'Failed to submit review.';
        this.reviewLoading = false;
      }
    });
  }

  stars(n: number): number[] { return Array(n).fill(0); }

  getStarClass(star: number, rating: number): string {
    return star <= Math.round(rating) ? 'star-filled' : 'star-empty';
  }
}
