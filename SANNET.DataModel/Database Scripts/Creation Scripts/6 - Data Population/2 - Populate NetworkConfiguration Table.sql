IF NOT EXISTS (SELECT * FROM NetworkConfigurations WHERE DatasetRetrievalMethodId = 1 AND NumHiddenLayers = 2 AND NumHiddenLayerNeurons = 20 AND NumTrainingMonths = 2)
BEGIN
	PRINT 'Inserting Network configuration (1, 2, 20, 2) into "NetworkConfigurations" table..'

	INSERT INTO [dbo].[NetworkConfigurations]
           ([DatasetRetrievalMethodId]
		   ,[NumHiddenLayers]
           ,[NumHiddenLayerNeurons]
           ,[NumTrainingMonths])
     VALUES
           (1, 2, 20, 2)
END
ELSE
	PRINT 'Network configuration (1, 2, 20, 2) already exists in the "NetworkConfigurations" table..'

IF NOT EXISTS (SELECT * FROM NetworkConfigurations WHERE DatasetRetrievalMethodId = 1 AND NumHiddenLayers = 3 AND NumHiddenLayerNeurons = 15 AND NumTrainingMonths = 2)
BEGIN
	PRINT 'Inserting Network configuration (1, 3, 15, 2) into "NetworkConfigurations" table..'

	INSERT INTO [dbo].[NetworkConfigurations]
           ([DatasetRetrievalMethodId]
		   ,[NumHiddenLayers]
           ,[NumHiddenLayerNeurons]
           ,[NumTrainingMonths])
     VALUES
           (1, 3, 15, 2)
END
ELSE
	PRINT 'Network configuration (1, 3, 15, 2) already exists in the "NetworkConfigurations" table..'

IF NOT EXISTS (SELECT * FROM NetworkConfigurations WHERE DatasetRetrievalMethodId = 1 AND NumHiddenLayers = 1 AND NumHiddenLayerNeurons = 15 AND NumTrainingMonths = 2)
BEGIN
	PRINT 'Inserting Network configuration (1, 1, 15, 2) into "NetworkConfigurations" table..'

	INSERT INTO [dbo].[NetworkConfigurations]
           ([DatasetRetrievalMethodId]
		   ,[NumHiddenLayers]
           ,[NumHiddenLayerNeurons]
           ,[NumTrainingMonths])
     VALUES
           (1, 1, 15, 2)
END
ELSE
	PRINT 'Network configuration (1, 1, 15, 2) already exists in the "NetworkConfigurations" table..'