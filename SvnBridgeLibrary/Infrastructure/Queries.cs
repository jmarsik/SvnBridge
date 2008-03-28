namespace SvnBridge.Infrastructure
{
	public static class Queries
	{
		public static string SelectItemOneLevel =
			@"
SELECT [Id]
      ,[ItemId]
      ,[ServerUrl]
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
";

		public static string SelectItemFullRecursion =
						@"
SELECT [Id]
      ,[ItemId]
      ,[ServerUrl]
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
";

		public const string SelectItemById =
			@"
SELECT [Id]
      ,[ItemId]
      ,[ServerUrl]
      ,[Name]
      ,[Parent]
      ,[IsFolder]
      ,[ItemRevision]
      ,[EffectiveRevision]
      ,[DownloadUrl]
      ,[LastModifiedDate]
FROM  [ItemMetaData]
WHERE EffectiveRevision = @Revision 
AND   ItemID = @ItemId
";

		public const string DeleteCache =
			@"
DELETE FROM ItemProperties;
DELETE FROM ItemMetaData;
DELETE FROM CachedRevisions;
";

		public const string SelectItem =
			@"
SELECT [Id]
      ,[ItemId]
      ,[ServerUrl]
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
";

		public const string InsertCachedRevision =
			@"
INSERT INTO [CachedRevisions] (Revision, ServerUrl, RootPath)
VALUES (@Revision, @ServerUrl, @RootPath)
";

		public const string InsertItemMetaData =
			@"
INSERT INTO [ItemMetaData]
           ([Id]
		   ,[IsFolder]
           ,[ItemId]
           ,[ServerUrl]
           ,[Name]
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
";

		public const string CreateDatabase =
			@"
CREATE TABLE CachedRevisions
(
	Revision INT NOT NULL,
	ServerUrl NVARCHAR(256) NOT NULL,
	RootPath NVARCHAR(256) NOT NULL,
	CONSTRAINT CachedRevisions_PK PRIMARY KEY ( Revision, ServerUrl, RootPath )
);

CREATE TABLE ItemMetaData
(
	Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT(NEWID()) NOT NULL,
	ItemId INT NOT NULL,
	ServerUrl NVARCHAR(100) NOT NULL,
	Name NVARCHAR(256) NOT NULL,
	Parent NVARCHAR(256) NOT NULL,
	IsFolder BIT NOT NULL,
	ItemRevision INT NOT NULL,
	EffectiveRevision INT NOT NULL,
	DownloadUrl NVARCHAR(512) NOT NULL,
	LastModifiedDate DATETIME NOT NULL,
	CONSTRAINT ItemMetaData_ServerNameRevisionItemName UNIQUE ( ServerUrl, Name, EffectiveRevision),
	CONSTRAINT ItemMetaData_ServerNameRevisionItemId UNIQUE ( ServerUrl, ItemId, EffectiveRevision) 
);


CREATE TABLE ItemProperties
(
	Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT(NEWID()) NOT NULL,
	ItemId UNIQUEIDENTIFIER REFERENCES ItemMetaData(Id) NOT NULL,
	PropertyKey NVARCHAR(255) NOT NULL,
	PropertyValue NTEXT NOT NULL
);
";
	}
}