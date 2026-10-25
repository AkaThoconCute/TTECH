--- Update user's email by username
UPDATE dbo.AspNetUsers
SET 
    Email = 'nguyennhan.success@gmail.com',
    NormalizedEmail = UPPER('NGUYENNHAN.SUCCESS@GMAIL.COM')
WHERE UserName = 'member1';