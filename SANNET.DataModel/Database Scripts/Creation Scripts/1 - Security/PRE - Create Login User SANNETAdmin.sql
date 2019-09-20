IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE [name] = 'SANNETAdmin')
BEGIN
	PRINT 'Creating "SANNETAdmin" login..'

	CREATE LOGIN [SANNETAdmin] WITH PASSWORD='@WSX#EDC2wsx3edc', DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
	ALTER SERVER ROLE [sysadmin] ADD MEMBER [SANNETAdmin]
END
ELSE
	PRINT 'The user "SANNETAdmin" login already exists..'
