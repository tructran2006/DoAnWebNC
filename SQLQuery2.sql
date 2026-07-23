SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /* =========================================================
       1. USERS
    ========================================================= */

    INSERT INTO Users
    (
        Username,
        Email,
        PasswordHash,
        FullName,
        AvatarUrl,
        CoverUrl,
        Bio,
        CreatedAt,
        LastLoginAt,
        Role,
        Status
    )
    VALUES
    (
        N'admin',
        N'admin@gmail.com',
        N'123456',
        N'Quản trị viên',
        NULL,
        NULL,
        N'Quản trị hệ thống mạng xã hội.',
        DATEADD(DAY, -120, GETUTCDATE()),
        NULL,
        1,
        0
    ),
    (
        N'truc',
        N'truc@gmail.com',
        N'123456',
        N'Trúc Trần',
        N'/uploads/avatars/truc.jpg',
        NULL,
        N'Yêu công nghệ và lập trình.',
        DATEADD(DAY, -90, GETUTCDATE()),
        NULL,
        0,
        0
    ),
    (
        N'nguyen',
        N'nguyen@gmail.com',
        N'123456',
        N'Nguyễn Văn An',
        N'/uploads/avatars/nguyen.jpg',
        NULL,
        N'Lập trình viên ASP.NET.',
        DATEADD(DAY, -80, GETUTCDATE()),
        NULL,
        0,
        0
    ),
    (
        N'hoa',
        N'hoa@gmail.com',
        N'123456',
        N'Nguyễn Thị Hoa',
        N'/uploads/avatars/hoa.jpg',
        NULL,
        N'Thích du lịch và chụp ảnh.',
        DATEADD(DAY, -75, GETUTCDATE()),
        NULL,
        0,
        0
    ),
    (
        N'minh',
        N'minh@gmail.com',
        N'123456',
        N'Trần Quốc Minh',
        NULL,
        NULL,
        N'Đam mê bóng đá và âm nhạc.',
        DATEADD(DAY, -70, GETUTCDATE()),
        NULL,
        0,
        0
    ),
    (
        N'lan',
        N'lan@gmail.com',
        N'123456',
        N'Phạm Ngọc Lan',
        NULL,
        NULL,
        N'Sinh viên công nghệ thông tin.',
        DATEADD(DAY, -65, GETUTCDATE()),
        NULL,
        0,
        0
    ),
    (
        N'tuan',
        N'tuan@gmail.com',
        N'123456',
        N'Lê Anh Tuấn',
        NULL,
        NULL,
        N'Thích lập trình Flutter.',
        DATEADD(DAY, -60, GETUTCDATE()),
        NULL,
        0,
        0
    ),
    (
        N'mai',
        N'mai@gmail.com',
        N'123456',
        N'Đặng Thanh Mai',
        NULL,
        NULL,
        N'Yêu thiết kế và sáng tạo.',
        DATEADD(DAY, -55, GETUTCDATE()),
        NULL,
        0,
        0
    ),
    (
        N'khanh',
        N'khanh@gmail.com',
        N'123456',
        N'Võ Minh Khánh',
        NULL,
        NULL,
        N'Backend developer.',
        DATEADD(DAY, -50, GETUTCDATE()),
        NULL,
        0,
        0
    ),
    (
        N'linh',
        N'linh@gmail.com',
        N'123456',
        N'Bùi Thùy Linh',
        NULL,
        NULL,
        N'Thích đọc sách và cà phê.',
        DATEADD(DAY, -45, GETUTCDATE()),
        NULL,
        0,
        0
    ),
    (
        N'huy',
        N'huy@gmail.com',
        N'123456',
        N'Phan Gia Huy',
        NULL,
        NULL,
        N'Đang học SQL Server.',
        DATEADD(DAY, -40, GETUTCDATE()),
        NULL,
        0,
        0
    ),
    (
        N'vy',
        N'vy@gmail.com',
        N'123456',
        N'Ngô Thảo Vy',
        NULL,
        NULL,
        N'Chia sẻ những điều tích cực.',
        DATEADD(DAY, -35, GETUTCDATE()),
        NULL,
        0,
        0
    );

    /* Lấy ID theo Username để script không phụ thuộc ID cố định */

    DECLARE @AdminId INT =
        (SELECT Id FROM Users WHERE Username = N'admin');

    DECLARE @TrucId INT =
        (SELECT Id FROM Users WHERE Username = N'truc');

    DECLARE @NguyenId INT =
        (SELECT Id FROM Users WHERE Username = N'nguyen');

    DECLARE @HoaId INT =
        (SELECT Id FROM Users WHERE Username = N'hoa');

    DECLARE @MinhId INT =
        (SELECT Id FROM Users WHERE Username = N'minh');

    DECLARE @LanId INT =
        (SELECT Id FROM Users WHERE Username = N'lan');

    DECLARE @TuanId INT =
        (SELECT Id FROM Users WHERE Username = N'tuan');

    DECLARE @MaiId INT =
        (SELECT Id FROM Users WHERE Username = N'mai');

    DECLARE @KhanhId INT =
        (SELECT Id FROM Users WHERE Username = N'khanh');

    DECLARE @LinhId INT =
        (SELECT Id FROM Users WHERE Username = N'linh');

    DECLARE @HuyId INT =
        (SELECT Id FROM Users WHERE Username = N'huy');

    DECLARE @VyId INT =
        (SELECT Id FROM Users WHERE Username = N'vy');

    /* =========================================================
       2. POSTS
       Visibility:
       0 = Public
       1 = Friends
       2 = Private
    ========================================================= */

    INSERT INTO Posts
    (
        UserId,
        Content,
        ImageUrl,
        MediaUrl,
        MediaType,
        CreatedAt,
        UpdatedAt,
        IsDeleted,
        IsHidden,
        Visibility
    )
    VALUES
    (
        @TrucId,
        N'Hôm nay bắt đầu hoàn thiện đồ án mạng xã hội ASP.NET MVC.',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -18, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @NguyenId,
        N'Vừa học xong phần Entity Framework Core. Migration khá thú vị!',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -17, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @HoaId,
        N'Một buổi chiều thật đẹp bên bờ biển.',
        NULL,
        N'/uploads/posts/bien-01.jpg',
        N'image',
        DATEADD(DAY, -16, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @MinhId,
        N'Tối nay có ai xem bóng đá không?',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -15, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @LanId,
        N'Bài tập hôm nay hơi nhiều nhưng vẫn cố gắng hoàn thành.',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -14, GETUTCDATE()),
        NULL,
        0,
        0,
        1
    ),
    (
        @TuanId,
        N'Flutter và ASP.NET API kết hợp khá hay.',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -13, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @MaiId,
        N'Mình vừa hoàn thành thiết kế giao diện mới.',
        NULL,
        N'/uploads/posts/design-01.jpg',
        N'image',
        DATEADD(DAY, -12, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @KhanhId,
        N'Tối ưu truy vấn SQL giúp hệ thống nhanh hơn rất nhiều.',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -11, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @LinhId,
        N'Cuối tuần này mình đang đọc một cuốn sách rất hay.',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -10, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @HuyId,
        N'Đang tìm hiểu cách tạo Stored Procedure trong SQL Server.',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -9, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @VyId,
        N'Mỗi ngày cố gắng một chút, rồi bạn sẽ tiến rất xa.',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -8, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @TrucId,
        N'Đã hoàn thành chức năng đăng nhiều ảnh và video.',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -7, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @NguyenId,
        N'Có ai đang gặp lỗi PendingModelChangesWarning không?',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -6, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @HoaId,
        N'Một vài khoảnh khắc trong chuyến đi vừa rồi.',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -5, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @MinhId,
        N'Âm nhạc giúp mình tập trung làm việc tốt hơn.',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -4, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @LanId,
        N'Hôm nay học được thêm rất nhiều điều mới.',
        NULL,
        NULL,
        NULL,
        DATEADD(DAY, -3, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @TuanId,
        N'Đã chạy thành công ứng dụng Flutter trên Windows.',
        NULL,
        N'/uploads/posts/flutter-demo.mp4',
        N'video',
        DATEADD(DAY, -2, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @MaiId,
        N'Màu sắc và khoảng trắng quyết định rất nhiều đến trải nghiệm người dùng.',
        NULL,
        NULL,
        NULL,
        DATEADD(HOUR, -30, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @KhanhId,
        N'Code sạch giúp dự án dễ bảo trì hơn.',
        NULL,
        NULL,
        NULL,
        DATEADD(HOUR, -20, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    ),
    (
        @LinhId,
        N'Chúc mọi người một ngày làm việc hiệu quả.',
        NULL,
        NULL,
        NULL,
        DATEADD(HOUR, -5, GETUTCDATE()),
        NULL,
        0,
        0,
        0
    );

    /* Lấy Post ID */

    DECLARE @Post1 INT =
    (
        SELECT TOP 1 Id
        FROM Posts
        WHERE UserId = @TrucId
        ORDER BY CreatedAt
    );

    DECLARE @Post2 INT =
    (
        SELECT TOP 1 Id
        FROM Posts
        WHERE UserId = @NguyenId
        ORDER BY CreatedAt
    );

    DECLARE @Post3 INT =
    (
        SELECT TOP 1 Id
        FROM Posts
        WHERE UserId = @HoaId
        ORDER BY CreatedAt
    );

    DECLARE @Post4 INT =
    (
        SELECT TOP 1 Id
        FROM Posts
        WHERE UserId = @MinhId
        ORDER BY CreatedAt
    );

    DECLARE @Post5 INT =
    (
        SELECT TOP 1 Id
        FROM Posts
        WHERE UserId = @LanId
        ORDER BY CreatedAt
    );

    DECLARE @Post6 INT =
    (
        SELECT TOP 1 Id
        FROM Posts
        WHERE UserId = @TuanId
        ORDER BY CreatedAt
    );

    DECLARE @Post7 INT =
    (
        SELECT TOP 1 Id
        FROM Posts
        WHERE UserId = @MaiId
        ORDER BY CreatedAt
    );

    DECLARE @Post8 INT =
    (
        SELECT TOP 1 Id
        FROM Posts
        WHERE UserId = @KhanhId
        ORDER BY CreatedAt
    );

    DECLARE @Post9 INT =
    (
        SELECT TOP 1 Id
        FROM Posts
        WHERE UserId = @LinhId
        ORDER BY CreatedAt
    );

    DECLARE @Post10 INT =
    (
        SELECT TOP 1 Id
        FROM Posts
        WHERE UserId = @HuyId
        ORDER BY CreatedAt
    );

    /* =========================================================
       3. POST MEDIA
       Chỉ chạy khi bảng PostMedia đã tồn tại
    ========================================================= */

    IF OBJECT_ID(N'dbo.PostMedia', N'U') IS NOT NULL
    BEGIN
        INSERT INTO PostMedia
        (
            PostId,
            MediaUrl,
            MediaType,
            DisplayOrder,
            CreatedAt
        )
        VALUES
        (
            @Post3,
            N'/uploads/posts/bien-01.jpg',
            N'image',
            1,
            GETUTCDATE()
        ),
        (
            @Post3,
            N'/uploads/posts/bien-02.jpg',
            N'image',
            2,
            GETUTCDATE()
        ),
        (
            @Post3,
            N'/uploads/posts/bien-03.jpg',
            N'image',
            3,
            GETUTCDATE()
        ),
        (
            @Post7,
            N'/uploads/posts/design-01.jpg',
            N'image',
            1,
            GETUTCDATE()
        ),
        (
            @Post7,
            N'/uploads/posts/design-02.jpg',
            N'image',
            2,
            GETUTCDATE()
        ),
        (
            @Post7,
            N'/uploads/posts/design-demo.mp4',
            N'video',
            3,
            GETUTCDATE()
        );
    END;

    /* =========================================================
       4. COMMENTS
    ========================================================= */

    INSERT INTO Comments
    (
        PostId,
        UserId,
        Content,
        CreatedAt,
        IsDeleted
    )
    VALUES
    (@Post1, @NguyenId, N'Chúc bạn hoàn thành đồ án thật tốt!', DATEADD(HOUR, -6, GETUTCDATE()), 0),
    (@Post1, @HoaId, N'Giao diện nhìn rất đẹp.', DATEADD(HOUR, -5, GETUTCDATE()), 0),
    (@Post1, @MinhId, N'Cố lên nhé!', DATEADD(HOUR, -4, GETUTCDATE()), 0),
    (@Post2, @TrucId, N'EF Core rất tiện khi làm MVC.', DATEADD(HOUR, -8, GETUTCDATE()), 0),
    (@Post2, @LanId, N'Mình cũng đang học phần này.', DATEADD(HOUR, -7, GETUTCDATE()), 0),
    (@Post3, @TrucId, N'Ảnh đẹp quá!', DATEADD(HOUR, -6, GETUTCDATE()), 0),
    (@Post3, @MaiId, N'Khung cảnh thật yên bình.', DATEADD(HOUR, -5, GETUTCDATE()), 0),
    (@Post4, @TuanId, N'Tối nay mình cũng xem.', DATEADD(HOUR, -7, GETUTCDATE()), 0),
    (@Post5, @VyId, N'Cố gắng lên bạn nhé.', DATEADD(HOUR, -6, GETUTCDATE()), 0),
    (@Post6, @KhanhId, N'Kết hợp này làm app rất ổn.', DATEADD(HOUR, -5, GETUTCDATE()), 0),
    (@Post7, @HoaId, N'Màu sắc rất hài hòa.', DATEADD(HOUR, -4, GETUTCDATE()), 0),
    (@Post7, @TrucId, N'Thiết kế đẹp và hiện đại.', DATEADD(HOUR, -3, GETUTCDATE()), 0),
    (@Post8, @HuyId, N'Đúng rồi, nên dùng AsNoTracking cho truy vấn đọc.', DATEADD(HOUR, -6, GETUTCDATE()), 0),
    (@Post9, @LanId, N'Bạn đang đọc sách gì vậy?', DATEADD(HOUR, -5, GETUTCDATE()), 0),
    (@Post10, @NguyenId, N'Stored Procedure rất hữu ích.', DATEADD(HOUR, -4, GETUTCDATE()), 0);

    /* =========================================================
       5. LIKES
    ========================================================= */

    INSERT INTO Likes
    (
        PostId,
        UserId,
        CreatedAt
    )
    VALUES
    (@Post1, @NguyenId, DATEADD(HOUR, -12, GETUTCDATE())),
    (@Post1, @HoaId, DATEADD(HOUR, -11, GETUTCDATE())),
    (@Post1, @MinhId, DATEADD(HOUR, -10, GETUTCDATE())),
    (@Post1, @LanId, DATEADD(HOUR, -9, GETUTCDATE())),
    (@Post1, @TuanId, DATEADD(HOUR, -8, GETUTCDATE())),
    (@Post2, @TrucId, DATEADD(HOUR, -10, GETUTCDATE())),
    (@Post2, @HoaId, DATEADD(HOUR, -9, GETUTCDATE())),
    (@Post2, @MaiId, DATEADD(HOUR, -8, GETUTCDATE())),
    (@Post3, @TrucId, DATEADD(HOUR, -10, GETUTCDATE())),
    (@Post3, @NguyenId, DATEADD(HOUR, -9, GETUTCDATE())),
    (@Post3, @MaiId, DATEADD(HOUR, -8, GETUTCDATE())),
    (@Post3, @LinhId, DATEADD(HOUR, -7, GETUTCDATE())),
    (@Post4, @TrucId, DATEADD(HOUR, -8, GETUTCDATE())),
    (@Post4, @TuanId, DATEADD(HOUR, -7, GETUTCDATE())),
    (@Post5, @VyId, DATEADD(HOUR, -6, GETUTCDATE())),
    (@Post6, @KhanhId, DATEADD(HOUR, -6, GETUTCDATE())),
    (@Post6, @NguyenId, DATEADD(HOUR, -5, GETUTCDATE())),
    (@Post7, @HoaId, DATEADD(HOUR, -5, GETUTCDATE())),
    (@Post7, @TrucId, DATEADD(HOUR, -4, GETUTCDATE())),
    (@Post7, @LanId, DATEADD(HOUR, -3, GETUTCDATE())),
    (@Post8, @HuyId, DATEADD(HOUR, -5, GETUTCDATE())),
    (@Post8, @NguyenId, DATEADD(HOUR, -4, GETUTCDATE())),
    (@Post9, @LanId, DATEADD(HOUR, -4, GETUTCDATE())),
    (@Post9, @VyId, DATEADD(HOUR, -3, GETUTCDATE())),
    (@Post10, @NguyenId, DATEADD(HOUR, -3, GETUTCDATE()));

    /* =========================================================
       6. FRIENDS
       Lưu hai chiều để GetFriendsAsync hoạt động dễ dàng
    ========================================================= */

    INSERT INTO Friends
    (
        UserId,
        FriendId,
        CreatedAt
    )
    VALUES
    (@TrucId, @NguyenId, DATEADD(DAY, -30, GETUTCDATE())),
    (@NguyenId, @TrucId, DATEADD(DAY, -30, GETUTCDATE())),

    (@TrucId, @HoaId, DATEADD(DAY, -28, GETUTCDATE())),
    (@HoaId, @TrucId, DATEADD(DAY, -28, GETUTCDATE())),

    (@TrucId, @MinhId, DATEADD(DAY, -25, GETUTCDATE())),
    (@MinhId, @TrucId, DATEADD(DAY, -25, GETUTCDATE())),

    (@NguyenId, @LanId, DATEADD(DAY, -22, GETUTCDATE())),
    (@LanId, @NguyenId, DATEADD(DAY, -22, GETUTCDATE())),

    (@HoaId, @MaiId, DATEADD(DAY, -20, GETUTCDATE())),
    (@MaiId, @HoaId, DATEADD(DAY, -20, GETUTCDATE())),

    (@TuanId, @KhanhId, DATEADD(DAY, -18, GETUTCDATE())),
    (@KhanhId, @TuanId, DATEADD(DAY, -18, GETUTCDATE())),

    (@LinhId, @VyId, DATEADD(DAY, -15, GETUTCDATE())),
    (@VyId, @LinhId, DATEADD(DAY, -15, GETUTCDATE()));

    /* =========================================================
       7. FRIEND REQUESTS
    ========================================================= */

    INSERT INTO FriendRequests
    (
        SenderId,
        ReceiverId,
        CreatedAt,
        IsAccepted,
        IsRejected
    )
    VALUES
    (@LanId, @TrucId, DATEADD(DAY, -2, GETUTCDATE()), 0, 0),
    (@HuyId, @TrucId, DATEADD(DAY, -1, GETUTCDATE()), 0, 0),
    (@MaiId, @NguyenId, DATEADD(HOUR, -12, GETUTCDATE()), 0, 0),
    (@VyId, @HoaId, DATEADD(HOUR, -10, GETUTCDATE()), 0, 0);

    /* =========================================================
       8. NOTIFICATIONS
    ========================================================= */

    INSERT INTO Notifications
    (
        UserId,
        ActorUserId,
        Type,
        ReferenceId,
        Message,
        Url,
        IsRead,
        CreatedAt
    )
    VALUES
    (
        @TrucId,
        @NguyenId,
        N'Like',
        @Post1,
        N'Nguyễn Văn An đã thích bài viết của bạn.',
        N'/Post/Details/' + CAST(@Post1 AS NVARCHAR(20)),
        0,
        DATEADD(HOUR, -4, GETUTCDATE())
    ),
    (
        @TrucId,
        @HoaId,
        N'Comment',
        @Post1,
        N'Nguyễn Thị Hoa đã bình luận bài viết của bạn.',
        N'/Post/Details/' + CAST(@Post1 AS NVARCHAR(20)),
        0,
        DATEADD(HOUR, -3, GETUTCDATE())
    ),
    (
        @TrucId,
        @LanId,
        N'FriendRequest',
        @LanId,
        N'Phạm Ngọc Lan đã gửi lời mời kết bạn.',
        N'/Friend/Requests',
        0,
        DATEADD(HOUR, -2, GETUTCDATE())
    ),
    (
        @TrucId,
        @HuyId,
        N'FriendRequest',
        @HuyId,
        N'Phan Gia Huy đã gửi lời mời kết bạn.',
        N'/Friend/Requests',
        0,
        DATEADD(HOUR, -1, GETUTCDATE())
    ),
    (
        @NguyenId,
        @TrucId,
        N'Like',
        @Post2,
        N'Trúc Trần đã thích bài viết của bạn.',
        N'/Post/Details/' + CAST(@Post2 AS NVARCHAR(20)),
        1,
        DATEADD(DAY, -1, GETUTCDATE())
    ),
    (
        @HoaId,
        @TrucId,
        N'Comment',
        @Post3,
        N'Trúc Trần đã bình luận bài viết của bạn.',
        N'/Post/Details/' + CAST(@Post3 AS NVARCHAR(20)),
        0,
        DATEADD(HOUR, -6, GETUTCDATE())
    );

    COMMIT TRANSACTION;

    PRINT N'Đã thêm dữ liệu mẫu thành công.';
    PRINT N'Tài khoản Admin: admin / 123456';
    PRINT N'Tài khoản User: truc / 123456';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;