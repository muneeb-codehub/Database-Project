-- ========================================
-- E-COMMERCE DATABASE SCHEMA
-- ========================================
-- Date: January 18, 2026
-- Database: Oracle 11g/12c
-- Description: Complete database schema for E-commerce application
-- ========================================

-- ========================================
-- DROP EXISTING TABLES (for clean installation)
-- ========================================
DROP TABLE reviews CASCADE CONSTRAINTS;
DROP TABLE delivery CASCADE CONSTRAINTS;
DROP TABLE payments CASCADE CONSTRAINTS;
DROP TABLE order_items CASCADE CONSTRAINTS;
DROP TABLE orders CASCADE CONSTRAINTS;
DROP TABLE cart_items CASCADE CONSTRAINTS;
DROP TABLE cart CASCADE CONSTRAINTS;
DROP TABLE products CASCADE CONSTRAINTS;
DROP TABLE categories CASCADE CONSTRAINTS;
DROP TABLE users CASCADE CONSTRAINTS;

-- Drop sequences
DROP SEQUENCE users_seq;
DROP SEQUENCE categories_seq;
DROP SEQUENCE products_seq;
DROP SEQUENCE cart_seq;
DROP SEQUENCE cart_items_seq;
DROP SEQUENCE orders_seq;
DROP SEQUENCE order_items_seq;
DROP SEQUENCE payments_seq;
DROP SEQUENCE delivery_seq;
DROP SEQUENCE reviews_seq;

-- ========================================
-- CREATE SEQUENCES
-- ========================================
CREATE SEQUENCE users_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE categories_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE products_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE cart_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE cart_items_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE orders_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE order_items_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE payments_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE delivery_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE reviews_seq START WITH 1 INCREMENT BY 1;

-- ========================================
-- 1. USERS TABLE
-- ========================================
CREATE TABLE users (
    user_id NUMBER PRIMARY KEY,
    username VARCHAR2(100) NOT NULL UNIQUE,
    password VARCHAR2(255) NOT NULL,
    email VARCHAR2(150) NOT NULL UNIQUE,
    role VARCHAR2(50) NOT NULL CHECK (role IN ('CUSTOMER', 'CASHIER', 'MANAGER', 'ADMIN', 'DELIVERY', 'INVENTORY')),
    created_at DATE DEFAULT SYSDATE
);

-- Indexes for Users
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_role ON users(role);

-- ========================================
-- 2. CATEGORIES TABLE
-- ========================================
CREATE TABLE categories (
    category_id NUMBER PRIMARY KEY,
    name VARCHAR2(100) NOT NULL UNIQUE,
    description VARCHAR2(500)
);

-- Index for Categories
CREATE INDEX idx_categories_name ON categories(name);

-- ========================================
-- 3. PRODUCTS TABLE
-- ========================================
CREATE TABLE products (
    product_id NUMBER PRIMARY KEY,
    name VARCHAR2(200) NOT NULL,
    description VARCHAR2(1000),
    price NUMBER(10, 2) NOT NULL CHECK (price >= 0),
    stock_quantity NUMBER NOT NULL CHECK (stock_quantity >= 0),
    category_id NUMBER NOT NULL,
    created_at DATE DEFAULT SYSDATE,
    CONSTRAINT fk_products_category FOREIGN KEY (category_id) REFERENCES categories(category_id) ON DELETE CASCADE
);

-- Indexes for Products
CREATE INDEX idx_products_name ON products(name);
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_price ON products(price);

-- ========================================
-- 4. CART TABLE
-- ========================================
CREATE TABLE cart (
    cart_id NUMBER PRIMARY KEY,
    user_id NUMBER NOT NULL UNIQUE,
    created_at DATE DEFAULT SYSDATE,
    CONSTRAINT fk_cart_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- Index for Cart
CREATE INDEX idx_cart_user ON cart(user_id);

-- ========================================
-- 5. CART_ITEMS TABLE
-- ========================================
CREATE TABLE cart_items (
    cart_item_id NUMBER PRIMARY KEY,
    cart_id NUMBER NOT NULL,
    product_id NUMBER NOT NULL,
    quantity NUMBER NOT NULL CHECK (quantity > 0),
    added_at DATE DEFAULT SYSDATE,
    CONSTRAINT fk_cart_items_cart FOREIGN KEY (cart_id) REFERENCES cart(cart_id) ON DELETE CASCADE,
    CONSTRAINT fk_cart_items_product FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);

-- Indexes for Cart Items
CREATE INDEX idx_cart_items_cart ON cart_items(cart_id);
CREATE INDEX idx_cart_items_product ON cart_items(product_id);

-- ========================================
-- 6. ORDERS TABLE
-- ========================================
CREATE TABLE orders (
    order_id NUMBER PRIMARY KEY,
    user_id NUMBER NOT NULL,
    total_price NUMBER(10, 2) NOT NULL CHECK (total_price >= 0),
    status VARCHAR2(50) NOT NULL CHECK (status IN ('Processing', 'Pending', 'Assigned to Delivery', 'Delivered', 'Cancelled')),
    created_at DATE DEFAULT SYSDATE,
    CONSTRAINT fk_orders_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- Indexes for Orders
CREATE INDEX idx_orders_user ON orders(user_id);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_date ON orders(created_at);

-- ========================================
-- 7. ORDER_ITEMS TABLE
-- ========================================
CREATE TABLE order_items (
    order_item_id NUMBER PRIMARY KEY,
    order_id NUMBER NOT NULL,
    product_id NUMBER NOT NULL,
    quantity NUMBER NOT NULL CHECK (quantity > 0),
    price NUMBER(10, 2) NOT NULL CHECK (price >= 0),
    CONSTRAINT fk_order_items_order FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE,
    CONSTRAINT fk_order_items_product FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE
);

-- Indexes for Order Items
CREATE INDEX idx_order_items_order ON order_items(order_id);
CREATE INDEX idx_order_items_product ON order_items(product_id);

-- ========================================
-- 8. PAYMENTS TABLE
-- ========================================
CREATE TABLE payments (
    payment_id NUMBER PRIMARY KEY,
    order_id NUMBER NOT NULL UNIQUE,
    amount NUMBER(10, 2) NOT NULL CHECK (amount >= 0),
    payment_method VARCHAR2(50) NOT NULL CHECK (payment_method IN ('Cash', 'Credit Card', 'Debit Card', 'PayPal', 'Bank Transfer')),
    payment_date DATE DEFAULT SYSDATE,
    CONSTRAINT fk_payments_order FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE
);

-- Indexes for Payments
CREATE INDEX idx_payments_order ON payments(order_id);
CREATE INDEX idx_payments_method ON payments(payment_method);

-- ========================================
-- 9. DELIVERY TABLE
-- ========================================
CREATE TABLE delivery (
    delivery_id NUMBER PRIMARY KEY,
    user_id NUMBER NOT NULL,
    order_id NUMBER NOT NULL UNIQUE,
    delivery_date DATE DEFAULT SYSDATE,
    CONSTRAINT fk_delivery_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    CONSTRAINT fk_delivery_order FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE
);

-- Indexes for Delivery
CREATE INDEX idx_delivery_user ON delivery(user_id);
CREATE INDEX idx_delivery_order ON delivery(order_id);

-- ========================================
-- 10. REVIEWS TABLE
-- ========================================
CREATE TABLE reviews (
    review_id NUMBER PRIMARY KEY,
    product_id NUMBER NOT NULL,
    user_id NUMBER NOT NULL,
    rating NUMBER NOT NULL CHECK (rating BETWEEN 1 AND 5),
    review_comment VARCHAR2(1000),
    created_at DATE DEFAULT SYSDATE,
    CONSTRAINT fk_reviews_product FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE CASCADE,
    CONSTRAINT fk_reviews_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- Indexes for Reviews
CREATE INDEX idx_reviews_product ON reviews(product_id);
CREATE INDEX idx_reviews_user ON reviews(user_id);
CREATE INDEX idx_reviews_rating ON reviews(rating);

-- ========================================
-- INSERT SAMPLE DATA
-- ========================================

-- Insert Sample Users
INSERT INTO users (user_id, username, password, email, role) VALUES (1, 'admin', 'admin123', 'admin@ecommerce.com', 'ADMIN');
INSERT INTO users (user_id, username, password, email, role) VALUES (2, 'manager1', 'manager123', 'manager@ecommerce.com', 'MANAGER');
INSERT INTO users (user_id, username, password, email, role) VALUES (3, 'cashier1', 'cashier123', 'cashier@ecommerce.com', 'CASHIER');
INSERT INTO users (user_id, username, password, email, role) VALUES (4, 'delivery1', 'delivery123', 'delivery@ecommerce.com', 'DELIVERY');
INSERT INTO users (user_id, username, password, email, role) VALUES (5, 'inventory1', 'inventory123', 'inventory@ecommerce.com', 'INVENTORY');
INSERT INTO users (user_id, username, password, email, role) VALUES (6, 'customer1', 'customer123', 'customer@example.com', 'CUSTOMER');

-- Insert Sample Categories
INSERT INTO categories (category_id, name, description) VALUES (1, 'Electronics', 'Electronic devices and accessories');
INSERT INTO categories (category_id, name, description) VALUES (2, 'Clothing', 'Apparel and fashion items');
INSERT INTO categories (category_id, name, description) VALUES (3, 'Books', 'Books and educational materials');
INSERT INTO categories (category_id, name, description) VALUES (4, 'Home & Garden', 'Home improvement and garden supplies');
INSERT INTO categories (category_id, name, description) VALUES (5, 'Sports', 'Sports equipment and accessories');

-- Insert Sample Products
INSERT INTO products (product_id, name, description, price, stock_quantity, category_id) VALUES (1, 'Laptop', 'High-performance laptop', 999.99, 50, 1);
INSERT INTO products (product_id, name, description, price, stock_quantity, category_id) VALUES (2, 'Smartphone', '5G enabled smartphone', 699.99, 100, 1);
INSERT INTO products (product_id, name, description, price, stock_quantity, category_id) VALUES (3, 'T-Shirt', 'Cotton t-shirt', 19.99, 200, 2);
INSERT INTO products (product_id, name, description, price, stock_quantity, category_id) VALUES (4, 'Jeans', 'Denim jeans', 49.99, 150, 2);
INSERT INTO products (product_id, name, description, price, stock_quantity, category_id) VALUES (5, 'Programming Book', 'Learn programming', 39.99, 75, 3);

-- Create cart for sample customer
INSERT INTO cart (cart_id, user_id) VALUES (1, 6);

COMMIT;

-- End of Schema Script
