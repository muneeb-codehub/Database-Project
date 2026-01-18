-- ========================================
-- E-COMMERCE DATABASE - STORED PROCEDURES
-- ========================================
-- Date: January 18, 2026
-- Database: Oracle 11g/12c
-- ========================================

-- ========================================
-- 1. STORED PROCEDURE: Add Product
-- ========================================
CREATE OR REPLACE PROCEDURE sp_AddProduct (
    p_product_id IN NUMBER,
    p_name IN VARCHAR2,
    p_description IN VARCHAR2,
    p_price IN NUMBER,
    p_stock_quantity IN NUMBER,
    p_category_name IN VARCHAR2
)
AS
    v_category_id NUMBER;
BEGIN
    -- Check if category exists, if not create it
    BEGIN
        SELECT category_id INTO v_category_id
        FROM categories
        WHERE LOWER(name) = LOWER(p_category_name);
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            -- Insert new category
            INSERT INTO categories (category_id, name)
            VALUES (categories_seq.NEXTVAL, p_category_name)
            RETURNING category_id INTO v_category_id;
    END;

    -- Check if product already exists
    DECLARE
        v_count NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_count
        FROM products
        WHERE product_id = p_product_id;
        
        IF v_count > 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'Product ID already exists');
        END IF;
    END;

    -- Insert product
    INSERT INTO products (product_id, name, description, price, stock_quantity, category_id)
    VALUES (p_product_id, p_name, p_description, p_price, p_stock_quantity, v_category_id);
    
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END sp_AddProduct;
/

-- ========================================
-- 2. STORED PROCEDURE: Update Inventory
-- ========================================
CREATE OR REPLACE PROCEDURE sp_UpdateInventory (
    p_product_id IN NUMBER,
    p_stock_quantity IN NUMBER
)
AS
    v_count NUMBER;
BEGIN
    -- Check if product exists
    SELECT COUNT(*) INTO v_count
    FROM products
    WHERE product_id = p_product_id;
    
    IF v_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20002, 'Product not found');
    END IF;

    -- Validate stock quantity
    IF p_stock_quantity < 0 THEN
        RAISE_APPLICATION_ERROR(-20003, 'Stock quantity cannot be negative');
    END IF;

    -- Update stock
    UPDATE products
    SET stock_quantity = p_stock_quantity
    WHERE product_id = p_product_id;
    
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END sp_UpdateInventory;
/

-- ========================================
-- 3. STORED PROCEDURE: Process Order/Checkout
-- ========================================
CREATE OR REPLACE PROCEDURE sp_ProcessCheckout (
    p_user_id IN NUMBER,
    p_payment_method IN VARCHAR2,
    p_order_id OUT NUMBER
)
AS
    v_cart_id NUMBER;
    v_total_price NUMBER := 0;
    v_cart_item_count NUMBER;
BEGIN
    -- Get cart ID for user
    BEGIN
        SELECT cart_id INTO v_cart_id
        FROM cart
        WHERE user_id = p_user_id;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20004, 'Cart not found for user');
    END;

    -- Check if cart has items
    SELECT COUNT(*) INTO v_cart_item_count
    FROM cart_items
    WHERE cart_id = v_cart_id;
    
    IF v_cart_item_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20005, 'Cart is empty');
    END IF;

    -- Create order
    INSERT INTO orders (order_id, user_id, status, total_price, created_at)
    VALUES (orders_seq.NEXTVAL, p_user_id, 'Processing', 0, SYSDATE)
    RETURNING order_id INTO p_order_id;

    -- Insert order items and calculate total
    FOR cart_item IN (
        SELECT ci.product_id, ci.quantity, p.price
        FROM cart_items ci
        JOIN products p ON ci.product_id = p.product_id
        WHERE ci.cart_id = v_cart_id
    ) LOOP
        INSERT INTO order_items (order_item_id, order_id, product_id, quantity, price)
        VALUES (order_items_seq.NEXTVAL, p_order_id, cart_item.product_id, cart_item.quantity, cart_item.price);
        
        v_total_price := v_total_price + (cart_item.quantity * cart_item.price);
        
        -- Update product stock
        UPDATE products
        SET stock_quantity = stock_quantity - cart_item.quantity
        WHERE product_id = cart_item.product_id;
    END LOOP;

    -- Update order total price
    UPDATE orders
    SET total_price = v_total_price
    WHERE order_id = p_order_id;

    -- Insert payment
    INSERT INTO payments (payment_id, order_id, amount, payment_method, payment_date)
    VALUES (payments_seq.NEXTVAL, p_order_id, v_total_price, p_payment_method, SYSDATE);

    -- Clear cart
    DELETE FROM cart_items WHERE cart_id = v_cart_id;
    
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END sp_ProcessCheckout;
/

-- ========================================
-- 4. STORED PROCEDURE: Assign Delivery
-- ========================================
CREATE OR REPLACE PROCEDURE sp_AssignDelivery (
    p_order_id IN NUMBER,
    p_delivery_user_id IN NUMBER
)
AS
    v_order_status VARCHAR2(50);
    v_user_role VARCHAR2(50);
BEGIN
    -- Validate delivery user
    BEGIN
        SELECT role INTO v_user_role
        FROM users
        WHERE user_id = p_delivery_user_id;
        
        IF UPPER(v_user_role) != 'DELIVERY' THEN
            RAISE_APPLICATION_ERROR(-20006, 'User is not a delivery personnel');
        END IF;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20007, 'Delivery user not found');
    END;

    -- Check order status
    BEGIN
        SELECT status INTO v_order_status
        FROM orders
        WHERE order_id = p_order_id;
        
        IF v_order_status = 'Assigned to Delivery' THEN
            RAISE_APPLICATION_ERROR(-20008, 'Order already assigned to delivery');
        END IF;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20009, 'Order not found');
    END;

    -- Update order status
    UPDATE orders
    SET status = 'Assigned to Delivery'
    WHERE order_id = p_order_id;

    -- Insert delivery record
    INSERT INTO delivery (delivery_id, user_id, order_id, delivery_date)
    VALUES (delivery_seq.NEXTVAL, p_delivery_user_id, p_order_id, SYSDATE);
    
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END sp_AssignDelivery;
/

-- ========================================
-- 5. STORED PROCEDURE: Mark Order as Delivered
-- ========================================
CREATE OR REPLACE PROCEDURE sp_MarkOrderDelivered (
    p_order_id IN NUMBER,
    p_delivery_user_id IN NUMBER
)
AS
    v_delivery_count NUMBER;
BEGIN
    -- Verify delivery assignment
    SELECT COUNT(*) INTO v_delivery_count
    FROM delivery
    WHERE order_id = p_order_id AND user_id = p_delivery_user_id;
    
    IF v_delivery_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20010, 'Order not assigned to this delivery personnel');
    END IF;

    -- Update order status
    UPDATE orders
    SET status = 'Delivered'
    WHERE order_id = p_order_id;
    
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END sp_MarkOrderDelivered;
/

-- ========================================
-- 6. STORED FUNCTION: Calculate Order Total
-- ========================================
CREATE OR REPLACE FUNCTION calculate_order_total (
    p_order_id IN NUMBER
) RETURN NUMBER
AS
    v_total NUMBER := 0;
BEGIN
    SELECT SUM(quantity * price) INTO v_total
    FROM order_items
    WHERE order_id = p_order_id;
    
    RETURN NVL(v_total, 0);
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN 0;
    WHEN OTHERS THEN
        RAISE;
END calculate_order_total;
/

-- ========================================
-- 7. STORED PROCEDURE: Add Review
-- ========================================
CREATE OR REPLACE PROCEDURE sp_AddReview (
    p_product_id IN NUMBER,
    p_user_id IN NUMBER,
    p_rating IN NUMBER,
    p_comment IN VARCHAR2
)
AS
BEGIN
    -- Validate rating
    IF p_rating < 1 OR p_rating > 5 THEN
        RAISE_APPLICATION_ERROR(-20011, 'Rating must be between 1 and 5');
    END IF;

    -- Insert review
    INSERT INTO reviews (review_id, product_id, user_id, rating, review_comment, created_at)
    VALUES (reviews_seq.NEXTVAL, p_product_id, p_user_id, p_rating, p_comment, SYSDATE);
    
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END sp_AddReview;
/

-- ========================================
-- 8. STORED PROCEDURE: Get Low Stock Products
-- ========================================
CREATE OR REPLACE PROCEDURE sp_GetLowStockProducts (
    p_threshold IN NUMBER DEFAULT 10,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT product_id, name, stock_quantity, price
        FROM products
        WHERE stock_quantity < p_threshold
        ORDER BY stock_quantity ASC;
END sp_GetLowStockProducts;
/

COMMIT;

-- End of Stored Procedures Script
