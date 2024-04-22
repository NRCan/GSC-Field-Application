# Files

The empty sqlite database, basically the schema itself. Used to create new database for user.

# Geopackage format
https://www.geopackage.org/

## Problems 
Mitigation between ArcGIS geopackage and QGIS.

QGIS creates new features with default geometry field named "geom". Creating new records
with said field for some reasons breaks the rendering in ArcGIS. Creating new features with
ArcGIS adds a geometry field named "SHAPE", which isn't meant for QGIS either.

Solution was to use geometry field named "geometry", that way the features are editable and
viewable in both softwares.

# Code procedure update for new schema

Whenever a new schema version is produced, make sure to update these files

## Model Resources
Keep a version of the previous schema as embeded content for upgradability.
Make sure to set it's properties as "Content" and "Copy always".

## Model
In each upgraded model tables, make sure to update the getFieldList property, 
according to known changes.

## DatabaseLiterals
* Make sure to add the literal strings in the dictionary with a comment regarding the 
added version, example for new fields.
* Make sure to add to the database version string literals DBVersion.

## DataAccess

### Version upgrade
Create a new method that will target new schema version with proper sql queries that will
make sure to parse the input datasets into the new version.

### Vocabulary
Make sure to add to the default creator vocab list defaultCreatorsEditors. This will enable 
the code to get users very own picklist value in the upgraded version schema as well as
keeping all the new values being added from new version.

## Testing
* Make sure to test with previous schema.
* Make sure to test with a database from very first schema so the upgrade tool goes through
all the different versions one by one.




