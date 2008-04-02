namespace SvnBridge.Infrastructure
{
	public static class Queries
	{
		public static string SelectItemOneLevel =
			@"
SELECT [Id]
      ,[ItemId]
      ,[Name]
      ,[Parent]
      ,[IsFolder]
      ,[ItemRevision]
      ,[EffectiveRevision]
      ,[DownloadUrl]
      ,[LastModifiedDate]
FROM  [ItemMetaData]
WHERE EffectiveRevision = @Revision 
AND	  (Name = @Path OR Parent = @Parent)
AND   ServerUrl = @ServerUrl
AND	  UserName = @UserName
";

		public static string SelectItemFullRecursion =
						@"
SELECT [Id]
      ,[ItemId]
      ,[Name]
      ,[Parent]
      ,[IsFolder]
      ,[ItemRevision]
      ,[EffectiveRevision]
      ,[DownloadUrl]
      ,[LastModifiedDate]
FROM  [ItemMetaData]
WHERE EffectiveRevision = @Revision 
AND   ServerUrl = @ServerUrl
AND	  Name LIKE @Path
AND	  UserName = @UserName
";

		public static string CreateLoggingDatabase =
			@"
CREATE TABLE Logs
(
	[Id] INT IDENTITY PRIMARY KEY,
	[Date] DATETIME DEFAULT(getdate()) NOT NULL,
	[Level] NVARCHAR(15) NOT NULL,
	[Message] NTEXT NOT NULL,
	[Exception] NTEXT NULL
);
";

		public static string InsertLog = 
			@"
INSERT INTO Logs ([Level], [Message], [Exception])
VALUES ( @Level, @Message, @Exception );
";

		public const string DeleteCache =
			@"
DELETE FROM ItemMetaData;
DELETE FROM CachedRevisions;
";

		public const string SelectItem =
			@"
SELECT [Id]
      ,[ItemId]
      ,[Name]
      ,[Parent]
      ,[IsFolder]
      ,[ItemRevision]
      ,[EffectiveRevision]
      ,[DownloadUrl]
      ,[LastModifiedDate]
FROM  [ItemMetaData]
WHERE EffectiveRevision = @Revision 
AND   ServerUrl = @ServerUrl
AND	  Name = @Path
AND	  UserName = @UserName
";

		public const string InsertCachedRevision =
			@"
INSERT INTO [CachedRevisions] (Revision, ServerUrl, RootPath, UserName)
VALUES (@Revision, @ServerUrl, @RootPath, @UserName)
";

		public const string InsertItemMetaData =
			@"
INSERT INTO [ItemMetaData]
           ([Id]
		   ,[IsFolder]
           ,[ItemId]
           ,[ServerUrl]
           ,[Name]
		   ,[UserName]
           ,[Parent]
           ,[ItemRevision]
           ,[EffectiveRevision]
           ,[DownloadUrl]
           ,[LastModifiedDate])
     VALUES
           (@Id
           ,@IsFolder
           ,@ItemId
           ,@ServerUrl
           ,@Name
           ,@UserName
           ,@Parent
           ,@ItemRevision
           ,@EffectiveRevision
           ,@DownloadUrl
           ,@LastModifiedDate)
";

		public const string SelectCachedRevision =
			@"
SELECT 1
FROM CachedRevisions
WHERE Revision  = @Revision
AND   RootPath  = @RootPath
AND	  ServerUrl = @ServerUrl
AND	  UserName = @UserName
";

		public const string CreateDatabase =
			@"
CREATE TABLE CachedRevisions
(
	Revision INT NOT NULL,
	ServerUrl NVARCHAR(256) NOT NULL,
	RootPath NVARCHAR(256) NOT NULL,
	UserName NVARCHAR(256) NOT NULL,
	CONSTRAINT CachedRevisions_PK PRIMARY KEY ( Revision, ServerUrl, RootPath, UserName )
);

CREATE TABLE ItemMetaData
(
	Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT(NEWID()) NOT NULL,
	ItemId INT NOT NULL,
	UserName NVARCHAR(256) NOT NULL,
	ServerUrl NVARCHAR(100) NOT NULL,
	Name NVARCHAR(256) NOT NULL,
	Parent NVARCHAR(256) NOT NULL,
	IsFolder BIT NOT NULL,
	ItemRevision INT NOT NULL,
	EffectiveRevision INT NOT NULL,
	DownloadUrl NVARCHAR(512) NOT NULL,
	LastModifiedDate DATETIME NOT NULL,
	CONSTRAINT ItemMetaData_ServerNameRevisionItemName UNIQUE ( ServerUrl, UserName, Name, EffectiveRevision),
	CONSTRAINT ItemMetaData_ServerNameRevisionItemId UNIQUE ( ServerUrl, UserName, ItemId, EffectiveRevision) 
);
";
	}
}