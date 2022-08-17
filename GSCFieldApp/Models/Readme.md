## Files

This folder contains the definition of all tables from the field database. Those are used to create, read and save from the database easier. 

Some other files are also used to create different class model used to define some abstract entity with properties used throughout the application code like FieldBooks.cs can keep track of all the informations used inside field book page. 

## Schema versions

Every schema version changes are stored within each of the table models. Added fields, or removed one are being tracked within a property of those classes (getFieldList). 
Make sure to update the property when the schema changes for upgradability sake.
Make sure to add or remove and replace field at the same place as they are within the database, else sqlite queries might fail because of integrity.