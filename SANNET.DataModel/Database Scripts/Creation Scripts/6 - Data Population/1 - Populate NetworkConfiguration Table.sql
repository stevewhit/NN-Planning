IF NOT EXISTS (SELECT * FROM NetworkConfigurations WHERE NumHiddenLayers = 2 AND NumHiddenLayerNeurons = 20 AND NumTrainingMonths = 2)
BEGIN
	PRINT 'Inserting Network configuration (2, 20, 2) into "NetworkConfigurations" table..'

	INSERT INTO [dbo].[NetworkConfigurations]
           ([NumHiddenLayers]
           ,[NumHiddenLayerNeurons]
           ,[NumTrainingMonths])
     VALUES
           (2, 20, 2)
END
ELSE
	PRINT 'Network configuration (2, 20, 2) already exists in the "NetworkConfigurations" table..'

IF NOT EXISTS (SELECT * FROM NetworkConfigurations WHERE NumHiddenLayers = 3 AND NumHiddenLayerNeurons = 15 AND NumTrainingMonths = 2)
BEGIN
	PRINT 'Inserting Network configuration (3, 15, 2) into "NetworkConfigurations" table..'

	INSERT INTO [dbo].[NetworkConfigurations]
           ([NumHiddenLayers]
           ,[NumHiddenLayerNeurons]
           ,[NumTrainingMonths])
     VALUES
           (3, 15, 2)
END
ELSE
	PRINT 'Network configuration (3, 15, 2) already exists in the "NetworkConfigurations" table..'

IF NOT EXISTS (SELECT * FROM NetworkConfigurations WHERE NumHiddenLayers = 1 AND NumHiddenLayerNeurons = 15 AND NumTrainingMonths = 2)
BEGIN
	PRINT 'Inserting Network configuration (1, 15, 2) into "NetworkConfigurations" table..'

	INSERT INTO [dbo].[NetworkConfigurations]
           ([NumHiddenLayers]
           ,[NumHiddenLayerNeurons]
           ,[NumTrainingMonths])
     VALUES
           (1, 15, 2)
END
ELSE
	PRINT 'Network configuration (1, 15, 2) already exists in the "NetworkConfigurations" table..'