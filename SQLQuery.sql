CREATE DATABASE SafexchangeDB;
GO

USE SafexchangeDB;
GO

/* =========================
   1. DROP TABLES IF EXISTS
========================= */

DROP TABLE IF EXISTS dbo.Return_Image;
DROP TABLE IF EXISTS dbo.[Return];
DROP TABLE IF EXISTS dbo.Feedback_Image;
DROP TABLE IF EXISTS dbo.Feedback;
DROP TABLE IF EXISTS dbo.Notification;
DROP TABLE IF EXISTS dbo.Report;
DROP TABLE IF EXISTS dbo.Promotion_List;
DROP TABLE IF EXISTS dbo.Payment;
DROP TABLE IF EXISTS dbo.Promotion_Order;
DROP TABLE IF EXISTS dbo.Promotion;
DROP TABLE IF EXISTS dbo.Shipment;
DROP TABLE IF EXISTS dbo.Ship_Status;
DROP TABLE IF EXISTS dbo.Ship_Method;
DROP TABLE IF EXISTS dbo.Rating;
DROP TABLE IF EXISTS dbo.[Order];
DROP TABLE IF EXISTS dbo.Voucher;
DROP TABLE IF EXISTS dbo.Fee_Rules;
DROP TABLE IF EXISTS dbo.Message;
DROP TABLE IF EXISTS dbo.Conversation;
DROP TABLE IF EXISTS dbo.Comment;
DROP TABLE IF EXISTS dbo.Favourite;
DROP TABLE IF EXISTS dbo.Combo;
DROP TABLE IF EXISTS dbo.Product_Image;
DROP TABLE IF EXISTS dbo.Product;
DROP TABLE IF EXISTS dbo.Category;
DROP TABLE IF EXISTS dbo.Product_Status;
DROP TABLE IF EXISTS dbo.[Rule];
DROP TABLE IF EXISTS dbo.Shipper_Profile;
DROP TABLE IF EXISTS dbo.User_Address;
DROP TABLE IF EXISTS dbo.User_Verification;
DROP TABLE IF EXISTS dbo.[User];
DROP TABLE IF EXISTS dbo.Area;
GO

/* =========================
   2. USER / AREA TABLES
========================= */

CREATE TABLE dbo.Area (
    area_id INT IDENTITY(1,1) PRIMARY KEY,
    area_name NVARCHAR(100) NOT NULL,
    city NVARCHAR(100) NOT NULL,
    district NVARCHAR(100) NULL,
    ward NVARCHAR(100) NULL,
    area_type NVARCHAR(30) NOT NULL CHECK (area_type IN ('campus', 'district', 'dormitory', 'other')),
    is_active BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.[User] (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    full_name NVARCHAR(100) NOT NULL,
    email NVARCHAR(150) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    phone NVARCHAR(20) NULL,
    role NVARCHAR(30) NOT NULL CHECK (role IN ('student', 'admin', 'shipper')),
    year_level NVARCHAR(30) NULL CHECK (year_level IN ('year1', 'year2', 'year3', 'year4', 'graduated')),
    account_status NVARCHAR(30) NOT NULL DEFAULT 'active'
        CHECK (account_status IN ('active', 'suspended', 'deleted')),
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE dbo.User_Verification (
    verification_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    verification_type NVARCHAR(50) NOT NULL CHECK (verification_type IN ('student_email', 'student_card')),
    verification_value NVARCHAR(150) NULL,
    document_url NVARCHAR(500) NULL,
    status NVARCHAR(30) NOT NULL DEFAULT 'pending'
        CHECK (status IN ('pending', 'approved', 'rejected')),
    reviewed_by INT NULL,
    rejection_reason NVARCHAR(500) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    verified_at DATETIME2 NULL,

    CONSTRAINT FK_UserVerification_User FOREIGN KEY (user_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_UserVerification_Reviewer FOREIGN KEY (reviewed_by) REFERENCES dbo.[User](user_id)
);

CREATE TABLE dbo.User_Address (
    address_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    area_id INT NULL,
    receiver_name NVARCHAR(100) NOT NULL,
    phone NVARCHAR(20) NOT NULL,
    address_line NVARCHAR(255) NOT NULL,
    address_type NVARCHAR(30) NOT NULL CHECK (address_type IN ('home', 'pickup', 'delivery')),
    is_default BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_UserAddress_User FOREIGN KEY (user_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_UserAddress_Area FOREIGN KEY (area_id) REFERENCES dbo.Area(area_id)
);

CREATE TABLE dbo.Shipper_Profile (
    shipper_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL UNIQUE,
    area_id INT NULL,
    vehicle_type NVARCHAR(50) NOT NULL,
    license_plate NVARCHAR(30) NULL,
    shipper_status NVARCHAR(30) NOT NULL DEFAULT 'available'
        CHECK (shipper_status IN ('available', 'busy', 'inactive')),
    rating_avg DECIMAL(3,2) NOT NULL DEFAULT 0,

    CONSTRAINT FK_ShipperProfile_User FOREIGN KEY (user_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_ShipperProfile_Area FOREIGN KEY (area_id) REFERENCES dbo.Area(area_id)
);

CREATE TABLE dbo.[Rule] (
    rule_id INT IDENTITY(1,1) PRIMARY KEY,
    rule_name NVARCHAR(150) NOT NULL,
    rule_type NVARCHAR(50) NOT NULL,
    description NVARCHAR(1000) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

/* =========================
   3. PRODUCT TABLES
========================= */

CREATE TABLE dbo.Product_Status (
    status_id INT IDENTITY(1,1) PRIMARY KEY,
    status_code NVARCHAR(50) NOT NULL UNIQUE,
    status_name NVARCHAR(100) NOT NULL
);

CREATE TABLE dbo.Category (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    parent_category_id INT NULL,
    category_name NVARCHAR(100) NOT NULL,
    description NVARCHAR(500) NULL,
    is_active BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Category_Parent FOREIGN KEY (parent_category_id) REFERENCES dbo.Category(category_id)
);

CREATE TABLE dbo.Product (
    product_id INT IDENTITY(1,1) PRIMARY KEY,
    seller_id INT NOT NULL,
    category_id INT NOT NULL,
    status_id INT NOT NULL,
    area_id INT NULL,
    title NVARCHAR(200) NOT NULL,
    description NVARCHAR(2000) NULL,
    price DECIMAL(18,2) NOT NULL,
    original_price DECIMAL(18,2) NULL,
    condition_status NVARCHAR(30) NOT NULL
        CHECK (condition_status IN ('like_new', 'good', 'fair', 'need_repair')),
    product_type NVARCHAR(30) NOT NULL DEFAULT 'single'
        CHECK (product_type IN ('single', 'combo')),
    is_negotiable BIT NOT NULL DEFAULT 1,
    view_count INT NOT NULL DEFAULT 0,
    published_at DATETIME2 NULL,
    expires_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Product_Seller FOREIGN KEY (seller_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_Product_Category FOREIGN KEY (category_id) REFERENCES dbo.Category(category_id),
    CONSTRAINT FK_Product_Status FOREIGN KEY (status_id) REFERENCES dbo.Product_Status(status_id),
    CONSTRAINT FK_Product_Area FOREIGN KEY (area_id) REFERENCES dbo.Area(area_id)
);

CREATE TABLE dbo.Product_Image (
    image_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    image_url NVARCHAR(500) NOT NULL,
    is_cover BIT NOT NULL DEFAULT 0,
    sort_order INT NOT NULL DEFAULT 1,

    CONSTRAINT FK_ProductImage_Product FOREIGN KEY (product_id) REFERENCES dbo.Product(product_id)
);

CREATE TABLE dbo.Combo (
    combo_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    item_name NVARCHAR(150) NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    item_condition NVARCHAR(100) NULL,
    estimated_value DECIMAL(18,2) NULL,

    CONSTRAINT FK_Combo_Product FOREIGN KEY (product_id) REFERENCES dbo.Product(product_id)
);

CREATE TABLE dbo.Favourite (
    favourite_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    product_id INT NOT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Favourite_User FOREIGN KEY (user_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_Favourite_Product FOREIGN KEY (product_id) REFERENCES dbo.Product(product_id),
    CONSTRAINT UQ_Favourite UNIQUE (user_id, product_id)
);

CREATE TABLE dbo.Comment (
    comment_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    user_id INT NOT NULL,
    parent_comment_id INT NULL,
    comment_text NVARCHAR(1000) NOT NULL,
    status NVARCHAR(30) NOT NULL DEFAULT 'visible'
        CHECK (status IN ('visible', 'hidden', 'deleted')),
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Comment_Product FOREIGN KEY (product_id) REFERENCES dbo.Product(product_id),
    CONSTRAINT FK_Comment_User FOREIGN KEY (user_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_Comment_Parent FOREIGN KEY (parent_comment_id) REFERENCES dbo.Comment(comment_id)
);

/* =========================
   4. CHAT TABLES
========================= */

CREATE TABLE dbo.Conversation (
    conversation_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    buyer_id INT NOT NULL,
    seller_id INT NOT NULL,
    status NVARCHAR(30) NOT NULL DEFAULT 'active'
        CHECK (status IN ('active', 'closed', 'blocked')),
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Conversation_Product FOREIGN KEY (product_id) REFERENCES dbo.Product(product_id),
    CONSTRAINT FK_Conversation_Buyer FOREIGN KEY (buyer_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_Conversation_Seller FOREIGN KEY (seller_id) REFERENCES dbo.[User](user_id)
);

CREATE TABLE dbo.Message (
    message_id INT IDENTITY(1,1) PRIMARY KEY,
    conversation_id INT NOT NULL,
    sender_id INT NOT NULL,
    message_text NVARCHAR(2000) NOT NULL,
    attachment_url NVARCHAR(500) NULL,
    is_read BIT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Message_Conversation FOREIGN KEY (conversation_id) REFERENCES dbo.Conversation(conversation_id),
    CONSTRAINT FK_Message_Sender FOREIGN KEY (sender_id) REFERENCES dbo.[User](user_id)
);

/* =========================
   5. ORDER / PAYMENT TABLES
========================= */

CREATE TABLE dbo.Voucher (
    voucher_id INT IDENTITY(1,1) PRIMARY KEY,
    voucher_code NVARCHAR(50) NOT NULL UNIQUE,
    voucher_name NVARCHAR(150) NOT NULL,
    discount_type NVARCHAR(30) NOT NULL CHECK (discount_type IN ('fixed', 'percent')),
    discount_value DECIMAL(18,2) NOT NULL,
    min_order_value DECIMAL(18,2) NOT NULL DEFAULT 0,
    max_discount_amount DECIMAL(18,2) NULL,
    start_at DATETIME2 NOT NULL,
    end_at DATETIME2 NOT NULL,
    usage_limit INT NULL,
    is_active BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.Fee_Rules (
    fee_rule_id INT IDENTITY(1,1) PRIMARY KEY,
    rule_name NVARCHAR(150) NOT NULL,
    min_order_value DECIMAL(18,2) NOT NULL,
    max_order_value DECIMAL(18,2) NULL,
    fee_type NVARCHAR(30) NOT NULL CHECK (fee_type IN ('fixed', 'percent')),
    fee_value DECIMAL(18,2) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.[Order] (
    order_id INT IDENTITY(1,1) PRIMARY KEY,
    buyer_id INT NOT NULL,
    seller_id INT NOT NULL,
    product_id INT NOT NULL,
    voucher_id INT NULL,
    item_price DECIMAL(18,2) NOT NULL,
    platform_fee DECIMAL(18,2) NOT NULL DEFAULT 0,
    discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    shipping_fee DECIMAL(18,2) NOT NULL DEFAULT 0,
    total_amount DECIMAL(18,2) NOT NULL,
    order_status NVARCHAR(30) NOT NULL DEFAULT 'pending'
        CHECK (order_status IN ('pending', 'confirmed', 'completed', 'cancelled', 'returned')),
    payment_status NVARCHAR(30) NOT NULL DEFAULT 'unpaid'
        CHECK (payment_status IN ('unpaid', 'paid', 'refunded')),
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    completed_at DATETIME2 NULL,

    CONSTRAINT FK_Order_Buyer FOREIGN KEY (buyer_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_Order_Seller FOREIGN KEY (seller_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_Order_Product FOREIGN KEY (product_id) REFERENCES dbo.Product(product_id),
    CONSTRAINT FK_Order_Voucher FOREIGN KEY (voucher_id) REFERENCES dbo.Voucher(voucher_id)
);

CREATE TABLE dbo.Rating (
    rating_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NULL,
    reviewer_id INT NOT NULL,
    reviewee_id INT NOT NULL,
    rating_score INT NOT NULL CHECK (rating_score BETWEEN 1 AND 5),
    comment NVARCHAR(1000) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Rating_Order FOREIGN KEY (order_id) REFERENCES dbo.[Order](order_id),
    CONSTRAINT FK_Rating_Reviewer FOREIGN KEY (reviewer_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_Rating_Reviewee FOREIGN KEY (reviewee_id) REFERENCES dbo.[User](user_id)
);

/* =========================
   6. SHIPPING TABLES
========================= */

CREATE TABLE dbo.Ship_Method (
    ship_method_id INT IDENTITY(1,1) PRIMARY KEY,
    method_name NVARCHAR(100) NOT NULL,
    description NVARCHAR(500) NULL,
    base_fee DECIMAL(18,2) NOT NULL DEFAULT 0,
    estimated_time NVARCHAR(100) NULL,
    is_active BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.Ship_Status (
    ship_status_id INT IDENTITY(1,1) PRIMARY KEY,
    status_code NVARCHAR(50) NOT NULL UNIQUE,
    status_name NVARCHAR(100) NOT NULL
);

CREATE TABLE dbo.Shipment (
    shipment_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL UNIQUE,
    shipper_id INT NULL,
    ship_method_id INT NOT NULL,
    ship_status_id INT NOT NULL,
    pickup_address_id INT NOT NULL,
    delivery_address_id INT NOT NULL,
    shipping_fee DECIMAL(18,2) NOT NULL DEFAULT 0,
    payer NVARCHAR(30) NOT NULL CHECK (payer IN ('buyer', 'seller', 'split')),
    scheduled_pickup_at DATETIME2 NULL,
    picked_up_at DATETIME2 NULL,
    delivered_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Shipment_Order FOREIGN KEY (order_id) REFERENCES dbo.[Order](order_id),
    CONSTRAINT FK_Shipment_Shipper FOREIGN KEY (shipper_id) REFERENCES dbo.Shipper_Profile(shipper_id),
    CONSTRAINT FK_Shipment_Method FOREIGN KEY (ship_method_id) REFERENCES dbo.Ship_Method(ship_method_id),
    CONSTRAINT FK_Shipment_Status FOREIGN KEY (ship_status_id) REFERENCES dbo.Ship_Status(ship_status_id),
    CONSTRAINT FK_Shipment_PickupAddress FOREIGN KEY (pickup_address_id) REFERENCES dbo.User_Address(address_id),
    CONSTRAINT FK_Shipment_DeliveryAddress FOREIGN KEY (delivery_address_id) REFERENCES dbo.User_Address(address_id)
);

/* =========================
   7. PROMOTION TABLES
========================= */

CREATE TABLE dbo.Promotion (
    promotion_id INT IDENTITY(1,1) PRIMARY KEY,
    promotion_name NVARCHAR(150) NOT NULL,
    promotion_type NVARCHAR(50) NOT NULL
        CHECK (promotion_type IN ('featured', 'combo_featured', 'boost')),
    duration_days INT NOT NULL DEFAULT 0,
    max_products INT NOT NULL DEFAULT 1,
    max_boosts INT NOT NULL DEFAULT 0,
    price DECIMAL(18,2) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.Promotion_Order (
    promotion_order_id INT IDENTITY(1,1) PRIMARY KEY,
    seller_id INT NOT NULL,
    promotion_id INT NOT NULL,
    total_amount DECIMAL(18,2) NOT NULL,
    payment_status NVARCHAR(30) NOT NULL DEFAULT 'pending'
        CHECK (payment_status IN ('pending', 'paid', 'failed', 'refunded')),
    starts_at DATETIME2 NULL,
    ends_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_PromotionOrder_Seller FOREIGN KEY (seller_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_PromotionOrder_Promotion FOREIGN KEY (promotion_id) REFERENCES dbo.Promotion(promotion_id)
);

CREATE TABLE dbo.Promotion_List (
    promotion_list_id INT IDENTITY(1,1) PRIMARY KEY,
    promotion_order_id INT NOT NULL,
    product_id INT NOT NULL,
    promotion_type NVARCHAR(50) NOT NULL
        CHECK (promotion_type IN ('featured', 'combo_featured', 'boost')),
    priority_score INT NOT NULL DEFAULT 0,
    start_at DATETIME2 NOT NULL,
    end_at DATETIME2 NULL,
    status NVARCHAR(30) NOT NULL DEFAULT 'active'
        CHECK (status IN ('active', 'expired', 'cancelled')),

    CONSTRAINT FK_PromotionList_Order FOREIGN KEY (promotion_order_id) REFERENCES dbo.Promotion_Order(promotion_order_id),
    CONSTRAINT FK_PromotionList_Product FOREIGN KEY (product_id) REFERENCES dbo.Product(product_id)
);

CREATE TABLE dbo.Payment (
    payment_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    order_id INT NULL,
    promotion_order_id INT NULL,
    amount DECIMAL(18,2) NOT NULL,
    payment_method NVARCHAR(30) NOT NULL
        CHECK (payment_method IN ('cash', 'bank_transfer', 'momo', 'zalopay')),
    payment_status NVARCHAR(30) NOT NULL DEFAULT 'pending'
        CHECK (payment_status IN ('pending', 'success', 'failed', 'refunded')),
    transaction_code NVARCHAR(100) NULL,
    paid_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Payment_User FOREIGN KEY (user_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_Payment_Order FOREIGN KEY (order_id) REFERENCES dbo.[Order](order_id),
    CONSTRAINT FK_Payment_PromotionOrder FOREIGN KEY (promotion_order_id) REFERENCES dbo.Promotion_Order(promotion_order_id),
    CONSTRAINT CK_Payment_Target CHECK (order_id IS NOT NULL OR promotion_order_id IS NOT NULL)
);

/* =========================
   8. FEEDBACK / REPORT / RETURN
========================= */

CREATE TABLE dbo.Feedback (
    feedback_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    order_id INT NULL,
    feedback_type NVARCHAR(50) NOT NULL
        CHECK (feedback_type IN ('website', 'service', 'delivery', 'product')),
    content NVARCHAR(2000) NOT NULL,
    rating_score INT NULL CHECK (rating_score BETWEEN 1 AND 5),
    status NVARCHAR(30) NOT NULL DEFAULT 'new'
        CHECK (status IN ('new', 'reviewed', 'resolved')),
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Feedback_User FOREIGN KEY (user_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_Feedback_Order FOREIGN KEY (order_id) REFERENCES dbo.[Order](order_id)
);

CREATE TABLE dbo.Feedback_Image (
    feedback_image_id INT IDENTITY(1,1) PRIMARY KEY,
    feedback_id INT NOT NULL,
    image_url NVARCHAR(500) NOT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_FeedbackImage_Feedback FOREIGN KEY (feedback_id) REFERENCES dbo.Feedback(feedback_id)
);

CREATE TABLE dbo.Report (
    report_id INT IDENTITY(1,1) PRIMARY KEY,
    reporter_id INT NOT NULL,
    target_type NVARCHAR(50) NOT NULL
        CHECK (target_type IN ('user', 'product', 'comment', 'message', 'order')),
    target_id INT NOT NULL,
    reason NVARCHAR(200) NOT NULL,
    description NVARCHAR(1000) NULL,
    status NVARCHAR(30) NOT NULL DEFAULT 'pending'
        CHECK (status IN ('pending', 'reviewing', 'resolved', 'rejected')),
    handled_by INT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Report_Reporter FOREIGN KEY (reporter_id) REFERENCES dbo.[User](user_id),
    CONSTRAINT FK_Report_Handler FOREIGN KEY (handled_by) REFERENCES dbo.[User](user_id)
);

CREATE TABLE dbo.Report_Image (
    report_image_id INT IDENTITY(1,1) PRIMARY KEY,
    report_id INT NOT NULL,
    image_url NVARCHAR(500) NOT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_ReportImage_Report 
        FOREIGN KEY (report_id) REFERENCES dbo.Report(report_id)
);


CREATE TABLE dbo.Notification (
    notification_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    title NVARCHAR(200) NOT NULL,
    content NVARCHAR(1000) NOT NULL,
    notification_type NVARCHAR(50) NOT NULL
        CHECK (notification_type IN ('order', 'message', 'promotion', 'system', 'report', 'return')),
    is_read BIT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Notification_User FOREIGN KEY (user_id) REFERENCES dbo.[User](user_id)
);

CREATE TABLE dbo.[Return] (
    return_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL,
    requester_id INT NOT NULL,
    reason NVARCHAR(200) NOT NULL,
    description NVARCHAR(1000) NULL,
    return_status NVARCHAR(30) NOT NULL DEFAULT 'pending'
        CHECK (return_status IN ('pending', 'approved', 'rejected', 'refunded')),
    refund_amount DECIMAL(18,2) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    resolved_at DATETIME2 NULL,

    CONSTRAINT FK_Return_Order FOREIGN KEY (order_id) REFERENCES dbo.[Order](order_id),
    CONSTRAINT FK_Return_Requester FOREIGN KEY (requester_id) REFERENCES dbo.[User](user_id)
);

CREATE TABLE dbo.Return_Image (
    return_image_id INT IDENTITY(1,1) PRIMARY KEY,
    return_id INT NOT NULL,
    image_url NVARCHAR(500) NOT NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_ReturnImage_Return FOREIGN KEY (return_id) REFERENCES dbo.[Return](return_id)
);
GO

