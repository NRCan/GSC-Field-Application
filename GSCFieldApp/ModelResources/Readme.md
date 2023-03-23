## Files

The empty sqlite database, basically the schema itself. Used to create new database for user.

## Geopackage format
https://www.geopackage.org/

### Problems 
Mitigation between ArcGIS geopackage and QGIS.

QGIS creates new features with default geometry field named "geom". Creating new records
with said field for some reasons breaks the rendering in ArcGIS. Creating new features with
ArcGIS adds a geometry field named "SHAPE", which isn't meant for QGIS either.

Solution was to use geometry field named "geometry", that way the features are editable and
viewable in both softwares.

