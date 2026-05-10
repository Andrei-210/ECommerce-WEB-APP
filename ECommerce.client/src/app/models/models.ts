export interface ProductSpecification {
  key: string;
  value: string;
}

export interface Review {
  id: number;
  userId: number;
  username: string;
  rating: number;
  title: string;
  body: string;
  createdAt: string;
}

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stock: number;
  category: string;
  brand: string;
  imageUrl: string;
  specifications: ProductSpecification[];
  reviews: Review[];
  averageRating: number;
  reviewCount: number;
}

export interface CartItem {
  product: Product;
  quantity: number;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  email: string;
}

export interface CheckoutRequest {
  shippingAddress: string;
  city: string;
  postalCode: string;
  items: { productId: number; quantity: number }[];
}

export interface OrderResponse {
  orderId: number;
  totalPrice: number;
  status: string;
  createdAt: string;
}

export interface CreateReviewRequest {
  rating: number;
  title: string;
  body: string;
}
