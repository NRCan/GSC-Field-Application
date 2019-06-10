Services folder is mainly used for scripts that will make some general services for other scripts.

DatabaseServices.DataAccess.cs is used to work with the database itself, read-write-update-create-delete-etc.
DatabaseServices.DataIDCalculation is used to create unique and alias ids for database tables. This is where, for example, STATIONIDNAME and STATIONID is created.
DatabaseServices.DataLocalSettings is used to keep in-memory some users options, like which table of on/off in field note page, whic field book is the activated and working one, etc. This is mainly something used to keep options when user shuts down the app and reopen it again.

FileServices.cs is used to delete, create or move files inside the device.

SettingsServices.cs was a default script for Template10.