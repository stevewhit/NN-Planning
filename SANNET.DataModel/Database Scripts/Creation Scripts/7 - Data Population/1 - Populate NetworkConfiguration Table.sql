IF NOT EXISTS (SELECT * FROM NetworkConfigurations WHERE NumHiddenLayers = 1 AND NumHiddenLayerNeurons = 15)
BEGIN
	PRINT 'Inserting Network configuration (1, 15) into "NetworkConfigurations" table..'

	INSERT INTO [dbo].[NetworkConfigurations]
           ([NumHiddenLayers]
           ,[NumHiddenLayerNeurons])
     VALUES
           (1, 15)
END
ELSE
	PRINT 'Network configuration (1, 15) already exists in the "NetworkConfigurations" table..'