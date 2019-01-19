# Issues

## Connect to Azure Storage Emulator from Docker
Connecting to Azure Storage Emulator from Docker does not work.

The solution is to use https://github.com/azure/azurite.

1. Go to directory `Tools\azurite.2.6.5`
2. Execute `copy __azurite_db_table__.json.init __azurite_db_table__.json` to create the initial tables.
3. Start the table emulator with `table.exe`
4. Now the docker instance can connect to the tables.