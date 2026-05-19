using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Safexchange.Models;

public partial class SafexchangeDbContext : DbContext
{
    public SafexchangeDbContext()
    {
    }

    public SafexchangeDbContext(DbContextOptions<SafexchangeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Combo> Combos { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Favourite> Favourites { get; set; }

    public virtual DbSet<FeeRule> FeeRules { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<FeedbackImage> FeedbackImages { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductStatus> ProductStatuses { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PromotionList> PromotionLists { get; set; }

    public virtual DbSet<PromotionOrder> PromotionOrders { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<ReportImage> ReportImages { get; set; }

    public virtual DbSet<Return> Returns { get; set; }

    public virtual DbSet<ReturnImage> ReturnImages { get; set; }

    public virtual DbSet<Rule> Rules { get; set; }

    public virtual DbSet<ShipMethod> ShipMethods { get; set; }

    public virtual DbSet<ShipStatus> ShipStatuses { get; set; }

    public virtual DbSet<Shipment> Shipments { get; set; }

    public virtual DbSet<ShipperProfile> ShipperProfiles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAddress> UserAddresses { get; set; }

    public virtual DbSet<UserVerification> UserVerifications { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=DESKTOP-Q602A7J;database=SafexchangeDB;uid=sa;pwd=123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.AreaId).HasName("PK__Area__985D6D6B63F7A05B");

            entity.ToTable("Area");

            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.AreaName)
                .HasMaxLength(100)
                .HasColumnName("area_name");
            entity.Property(e => e.AreaType)
                .HasMaxLength(30)
                .HasColumnName("area_type");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasColumnName("district");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Ward)
                .HasMaxLength(100)
                .HasColumnName("ward");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__D54EE9B46CF83991");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .HasColumnName("category_name");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_Category_Parent");
        });

        modelBuilder.Entity<Combo>(entity =>
        {
            entity.HasKey(e => e.ComboId).HasName("PK__Combo__18F74AA320864093");

            entity.ToTable("Combo");

            entity.Property(e => e.ComboId).HasColumnName("combo_id");
            entity.Property(e => e.EstimatedValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("estimated_value");
            entity.Property(e => e.ItemCondition)
                .HasMaxLength(100)
                .HasColumnName("item_condition");
            entity.Property(e => e.ItemName)
                .HasMaxLength(150)
                .HasColumnName("item_name");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");

            entity.HasOne(d => d.Product).WithMany(p => p.Combos)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Combo_Product");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__E7957687C72C2D6B");

            entity.ToTable("Comment");

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.CommentText)
                .HasMaxLength(1000)
                .HasColumnName("comment_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("visible")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
                .HasForeignKey(d => d.ParentCommentId)
                .HasConstraintName("FK_Comment_Parent");

            entity.HasOne(d => d.Product).WithMany(p => p.Comments)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comment_Product");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comment_User");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__Conversa__311E7E9A7AC4F2D4");

            entity.ToTable("Conversation");

            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("active")
                .HasColumnName("status");

            entity.HasOne(d => d.Buyer).WithMany(p => p.ConversationBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversation_Buyer");

            entity.HasOne(d => d.Product).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversation_Product");

            entity.HasOne(d => d.Seller).WithMany(p => p.ConversationSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversation_Seller");
        });

        modelBuilder.Entity<Favourite>(entity =>
        {
            entity.HasKey(e => e.FavouriteId).HasName("PK__Favourit__B3E742CEF5670B7A");

            entity.ToTable("Favourite");

            entity.HasIndex(e => new { e.UserId, e.ProductId }, "UQ_Favourite").IsUnique();

            entity.Property(e => e.FavouriteId).HasColumnName("favourite_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Product).WithMany(p => p.Favourites)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favourite_Product");

            entity.HasOne(d => d.User).WithMany(p => p.Favourites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favourite_User");
        });

        modelBuilder.Entity<FeeRule>(entity =>
        {
            entity.HasKey(e => e.FeeRuleId).HasName("PK__Fee_Rule__7BB5B6F40C9FD9F2");

            entity.ToTable("Fee_Rules");

            entity.Property(e => e.FeeRuleId).HasColumnName("fee_rule_id");
            entity.Property(e => e.FeeType)
                .HasMaxLength(30)
                .HasColumnName("fee_type");
            entity.Property(e => e.FeeValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("fee_value");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MaxOrderValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("max_order_value");
            entity.Property(e => e.MinOrderValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("min_order_value");
            entity.Property(e => e.RuleName)
                .HasMaxLength(150)
                .HasColumnName("rule_name");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__7A6B2B8C6703C015");

            entity.ToTable("Feedback");

            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.Content)
                .HasMaxLength(2000)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.FeedbackType)
                .HasMaxLength(50)
                .HasColumnName("feedback_type");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.RatingScore).HasColumnName("rating_score");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("new")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Order).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Feedback_Order");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Feedback_User");
        });

        modelBuilder.Entity<FeedbackImage>(entity =>
        {
            entity.HasKey(e => e.FeedbackImageId).HasName("PK__Feedback__C050D7ABB0538B20");

            entity.ToTable("Feedback_Image");

            entity.Property(e => e.FeedbackImageId).HasColumnName("feedback_image_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");

            entity.HasOne(d => d.Feedback).WithMany(p => p.FeedbackImages)
                .HasForeignKey(d => d.FeedbackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FeedbackImage_Feedback");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Message__0BBF6EE68D1EA96D");

            entity.ToTable("Message");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.AttachmentUrl)
                .HasMaxLength(500)
                .HasColumnName("attachment_url");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.MessageText)
                .HasMaxLength(2000)
                .HasColumnName("message_text");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Message_Conversation");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Message_Sender");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F79484878");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .HasColumnName("notification_type");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notification_User");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__4659622928DCA059");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("discount_amount");
            entity.Property(e => e.ItemPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("item_price");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(30)
                .HasDefaultValue("pending")
                .HasColumnName("order_status");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(30)
                .HasDefaultValue("unpaid")
                .HasColumnName("payment_status");
            entity.Property(e => e.PlatformFee)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("platform_fee");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.ShippingFee)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("shipping_fee");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");

            entity.HasOne(d => d.Buyer).WithMany(p => p.OrderBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Buyer");

            entity.HasOne(d => d.Product).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Product");

            entity.HasOne(d => d.Seller).WithMany(p => p.OrderSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Seller");

            entity.HasOne(d => d.Voucher).WithMany(p => p.Orders)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK_Order_Voucher");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__ED1FC9EA8C8FE598");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(30)
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(30)
                .HasDefaultValue("pending")
                .HasColumnName("payment_status");
            entity.Property(e => e.PromotionOrderId).HasColumnName("promotion_order_id");
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(100)
                .HasColumnName("transaction_code");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Payment_Order");

            entity.HasOne(d => d.PromotionOrder).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PromotionOrderId)
                .HasConstraintName("FK_Payment_PromotionOrder");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_User");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Product__47027DF573690477");

            entity.ToTable("Product");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ConditionStatus)
                .HasMaxLength(30)
                .HasColumnName("condition_status");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IsNegotiable)
                .HasDefaultValue(true)
                .HasColumnName("is_negotiable");
            entity.Property(e => e.OriginalPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("original_price");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductType)
                .HasMaxLength(30)
                .HasDefaultValue("single")
                .HasColumnName("product_type");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.ViewCount).HasColumnName("view_count");

            entity.HasOne(d => d.Area).WithMany(p => p.Products)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("FK_Product_Area");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Category");

            entity.HasOne(d => d.Seller).WithMany(p => p.Products)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Seller");

            entity.HasOne(d => d.Status).WithMany(p => p.Products)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Status");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__Product___DC9AC955936DFD36");

            entity.ToTable("Product_Image");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsCover).HasColumnName("is_cover");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(1)
                .HasColumnName("sort_order");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductImage_Product");
        });

        modelBuilder.Entity<ProductStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__Product___3683B53164565C0C");

            entity.ToTable("Product_Status");

            entity.HasIndex(e => e.StatusCode, "UQ__Product___4157B021D5DA4FCD").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(50)
                .HasColumnName("status_code");
            entity.Property(e => e.StatusName)
                .HasMaxLength(100)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__2CB9556BDA7B588C");

            entity.ToTable("Promotion");

            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MaxBoosts).HasColumnName("max_boosts");
            entity.Property(e => e.MaxProducts)
                .HasDefaultValue(1)
                .HasColumnName("max_products");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.PromotionName)
                .HasMaxLength(150)
                .HasColumnName("promotion_name");
            entity.Property(e => e.PromotionType)
                .HasMaxLength(50)
                .HasColumnName("promotion_type");
        });

        modelBuilder.Entity<PromotionList>(entity =>
        {
            entity.HasKey(e => e.PromotionListId).HasName("PK__Promotio__97F7A4F3E55649FC");

            entity.ToTable("Promotion_List");

            entity.Property(e => e.PromotionListId).HasColumnName("promotion_list_id");
            entity.Property(e => e.EndAt).HasColumnName("end_at");
            entity.Property(e => e.PriorityScore).HasColumnName("priority_score");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.PromotionOrderId).HasColumnName("promotion_order_id");
            entity.Property(e => e.PromotionType)
                .HasMaxLength(50)
                .HasColumnName("promotion_type");
            entity.Property(e => e.StartAt).HasColumnName("start_at");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("active")
                .HasColumnName("status");

            entity.HasOne(d => d.Product).WithMany(p => p.PromotionLists)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionList_Product");

            entity.HasOne(d => d.PromotionOrder).WithMany(p => p.PromotionLists)
                .HasForeignKey(d => d.PromotionOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionList_Order");
        });

        modelBuilder.Entity<PromotionOrder>(entity =>
        {
            entity.HasKey(e => e.PromotionOrderId).HasName("PK__Promotio__9021ACF8AD4C4143");

            entity.ToTable("Promotion_Order");

            entity.Property(e => e.PromotionOrderId).HasColumnName("promotion_order_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EndsAt).HasColumnName("ends_at");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(30)
                .HasDefaultValue("pending")
                .HasColumnName("payment_status");
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.StartsAt).HasColumnName("starts_at");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_amount");

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionOrders)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionOrder_Promotion");

            entity.HasOne(d => d.Seller).WithMany(p => p.PromotionOrders)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionOrder_Seller");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__Rating__D35B278B4F9D6A92");

            entity.ToTable("Rating");

            entity.Property(e => e.RatingId).HasColumnName("rating_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.RatingScore).HasColumnName("rating_score");
            entity.Property(e => e.RevieweeId).HasColumnName("reviewee_id");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");

            entity.HasOne(d => d.Order).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Rating_Order");

            entity.HasOne(d => d.Reviewee).WithMany(p => p.RatingReviewees)
                .HasForeignKey(d => d.RevieweeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rating_Reviewee");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.RatingReviewers)
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rating_Reviewer");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Report__779B7C581D7309D9");

            entity.ToTable("Report");

            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.HandledBy).HasColumnName("handled_by");
            entity.Property(e => e.Reason)
                .HasMaxLength(200)
                .HasColumnName("reason");
            entity.Property(e => e.ReporterId).HasColumnName("reporter_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(50)
                .HasColumnName("target_type");

            entity.HasOne(d => d.HandledByNavigation).WithMany(p => p.ReportHandledByNavigations)
                .HasForeignKey(d => d.HandledBy)
                .HasConstraintName("FK_Report_Handler");

            entity.HasOne(d => d.Reporter).WithMany(p => p.ReportReporters)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Report_Reporter");
        });

        modelBuilder.Entity<ReportImage>(entity =>
        {
            entity.HasKey(e => e.ReportImageId).HasName("PK__Report_I__19F0AB7FFE3B1D37");

            entity.ToTable("Report_Image");

            entity.Property(e => e.ReportImageId).HasColumnName("report_image_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.ReportId).HasColumnName("report_id");

            entity.HasOne(d => d.Report).WithMany(p => p.ReportImages)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReportImage_Report");
        });

        modelBuilder.Entity<Return>(entity =>
        {
            entity.HasKey(e => e.ReturnId).HasName("PK__Return__35C23473E210CF7C");

            entity.ToTable("Return");

            entity.Property(e => e.ReturnId).HasColumnName("return_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(200)
                .HasColumnName("reason");
            entity.Property(e => e.RefundAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("refund_amount");
            entity.Property(e => e.RequesterId).HasColumnName("requester_id");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.ReturnStatus)
                .HasMaxLength(30)
                .HasDefaultValue("pending")
                .HasColumnName("return_status");

            entity.HasOne(d => d.Order).WithMany(p => p.Returns)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Return_Order");

            entity.HasOne(d => d.Requester).WithMany(p => p.Returns)
                .HasForeignKey(d => d.RequesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Return_Requester");
        });

        modelBuilder.Entity<ReturnImage>(entity =>
        {
            entity.HasKey(e => e.ReturnImageId).HasName("PK__Return_I__88E2D43EB4E9C204");

            entity.ToTable("Return_Image");

            entity.Property(e => e.ReturnImageId).HasColumnName("return_image_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.ReturnId).HasColumnName("return_id");

            entity.HasOne(d => d.Return).WithMany(p => p.ReturnImages)
                .HasForeignKey(d => d.ReturnId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnImage_Return");
        });

        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("PK__Rule__E92A92963F62D9A4");

            entity.ToTable("Rule");

            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.RuleName)
                .HasMaxLength(150)
                .HasColumnName("rule_name");
            entity.Property(e => e.RuleType)
                .HasMaxLength(50)
                .HasColumnName("rule_type");
        });

        modelBuilder.Entity<ShipMethod>(entity =>
        {
            entity.HasKey(e => e.ShipMethodId).HasName("PK__Ship_Met__ADA291E12F2D5995");

            entity.ToTable("Ship_Method");

            entity.Property(e => e.ShipMethodId).HasColumnName("ship_method_id");
            entity.Property(e => e.BaseFee)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("base_fee");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.EstimatedTime)
                .HasMaxLength(100)
                .HasColumnName("estimated_time");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MethodName)
                .HasMaxLength(100)
                .HasColumnName("method_name");
        });

        modelBuilder.Entity<ShipStatus>(entity =>
        {
            entity.HasKey(e => e.ShipStatusId).HasName("PK__Ship_Sta__043124DFD990D4DC");

            entity.ToTable("Ship_Status");

            entity.HasIndex(e => e.StatusCode, "UQ__Ship_Sta__4157B0210B67B0EA").IsUnique();

            entity.Property(e => e.ShipStatusId).HasColumnName("ship_status_id");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(50)
                .HasColumnName("status_code");
            entity.Property(e => e.StatusName)
                .HasMaxLength(100)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(e => e.ShipmentId).HasName("PK__Shipment__41466E592D183DB6");

            entity.ToTable("Shipment");

            entity.HasIndex(e => e.OrderId, "UQ__Shipment__46596228B0CE199E").IsUnique();

            entity.Property(e => e.ShipmentId).HasColumnName("shipment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeliveredAt).HasColumnName("delivered_at");
            entity.Property(e => e.DeliveryAddressId).HasColumnName("delivery_address_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Payer)
                .HasMaxLength(30)
                .HasColumnName("payer");
            entity.Property(e => e.PickedUpAt).HasColumnName("picked_up_at");
            entity.Property(e => e.PickupAddressId).HasColumnName("pickup_address_id");
            entity.Property(e => e.ScheduledPickupAt).HasColumnName("scheduled_pickup_at");
            entity.Property(e => e.ShipMethodId).HasColumnName("ship_method_id");
            entity.Property(e => e.ShipStatusId).HasColumnName("ship_status_id");
            entity.Property(e => e.ShipperId).HasColumnName("shipper_id");
            entity.Property(e => e.ShippingFee)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("shipping_fee");

            entity.HasOne(d => d.DeliveryAddress).WithMany(p => p.ShipmentDeliveryAddresses)
                .HasForeignKey(d => d.DeliveryAddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Shipment_DeliveryAddress");

            entity.HasOne(d => d.Order).WithOne(p => p.Shipment)
                .HasForeignKey<Shipment>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Shipment_Order");

            entity.HasOne(d => d.PickupAddress).WithMany(p => p.ShipmentPickupAddresses)
                .HasForeignKey(d => d.PickupAddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Shipment_PickupAddress");

            entity.HasOne(d => d.ShipMethod).WithMany(p => p.Shipments)
                .HasForeignKey(d => d.ShipMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Shipment_Method");

            entity.HasOne(d => d.ShipStatus).WithMany(p => p.Shipments)
                .HasForeignKey(d => d.ShipStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Shipment_Status");

            entity.HasOne(d => d.Shipper).WithMany(p => p.Shipments)
                .HasForeignKey(d => d.ShipperId)
                .HasConstraintName("FK_Shipment_Shipper");
        });

        modelBuilder.Entity<ShipperProfile>(entity =>
        {
            entity.HasKey(e => e.ShipperId).HasName("PK__Shipper___0F95F1DD02194BD0");

            entity.ToTable("Shipper_Profile");

            entity.HasIndex(e => e.UserId, "UQ__Shipper___B9BE370EB06F4B1A").IsUnique();

            entity.Property(e => e.ShipperId).HasColumnName("shipper_id");
            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(30)
                .HasColumnName("license_plate");
            entity.Property(e => e.RatingAvg)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("rating_avg");
            entity.Property(e => e.ShipperStatus)
                .HasMaxLength(30)
                .HasDefaultValue("available")
                .HasColumnName("shipper_status");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VehicleType)
                .HasMaxLength(50)
                .HasColumnName("vehicle_type");

            entity.HasOne(d => d.Area).WithMany(p => p.ShipperProfiles)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("FK_ShipperProfile_Area");

            entity.HasOne(d => d.User).WithOne(p => p.ShipperProfile)
                .HasForeignKey<ShipperProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShipperProfile_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370F873931DC");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__AB6E6164F0687768").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AccountStatus)
                .HasMaxLength(30)
                .HasDefaultValue("active")
                .HasColumnName("account_status");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(30)
                .HasColumnName("role");
            entity.Property(e => e.YearLevel)
                .HasMaxLength(30)
                .HasColumnName("year_level");
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__User_Add__CAA247C8ED9FFFB0");

            entity.ToTable("User_Address");

            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.AddressLine)
                .HasMaxLength(255)
                .HasColumnName("address_line");
            entity.Property(e => e.AddressType)
                .HasMaxLength(30)
                .HasColumnName("address_type");
            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.ReceiverName)
                .HasMaxLength(100)
                .HasColumnName("receiver_name");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Area).WithMany(p => p.UserAddresses)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("FK_UserAddress_Area");

            entity.HasOne(d => d.User).WithMany(p => p.UserAddresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAddress_User");
        });

        modelBuilder.Entity<UserVerification>(entity =>
        {
            entity.HasKey(e => e.VerificationId).HasName("PK__User_Ver__24F1796987F32FAA");

            entity.ToTable("User_Verification");

            entity.Property(e => e.VerificationId).HasColumnName("verification_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DocumentUrl)
                .HasMaxLength(500)
                .HasColumnName("document_url");
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500)
                .HasColumnName("rejection_reason");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VerificationType)
                .HasMaxLength(50)
                .HasColumnName("verification_type");
            entity.Property(e => e.VerificationValue)
                .HasMaxLength(150)
                .HasColumnName("verification_value");
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.UserVerificationReviewedByNavigations)
                .HasForeignKey(d => d.ReviewedBy)
                .HasConstraintName("FK_UserVerification_Reviewer");

            entity.HasOne(d => d.User).WithMany(p => p.UserVerificationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserVerification_User");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("PK__Voucher__80B6FFA81A17E49B");

            entity.ToTable("Voucher");

            entity.HasIndex(e => e.VoucherCode, "UQ__Voucher__21731069918968D8").IsUnique();

            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(30)
                .HasColumnName("discount_type");
            entity.Property(e => e.DiscountValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("discount_value");
            entity.Property(e => e.EndAt).HasColumnName("end_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MaxDiscountAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("max_discount_amount");
            entity.Property(e => e.MinOrderValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("min_order_value");
            entity.Property(e => e.StartAt).HasColumnName("start_at");
            entity.Property(e => e.UsageLimit).HasColumnName("usage_limit");
            entity.Property(e => e.VoucherCode)
                .HasMaxLength(50)
                .HasColumnName("voucher_code");
            entity.Property(e => e.VoucherName)
                .HasMaxLength(150)
                .HasColumnName("voucher_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
