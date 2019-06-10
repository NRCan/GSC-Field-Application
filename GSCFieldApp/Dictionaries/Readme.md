Dictionaries contains classes in which all constant strings used throughout the app are found. By having all string stored at the same place we only have one place to change a string if it needs to be updated.

ApplicationLiterals.cs contains keyword string used in the views for inherit functionalities.
DatabaseLiterals.cs contains all names of field and tables found in the field database model (sqlite). Those names are use throughout the whole application.
ScienceLiterals.cs keeps science project type strings, like bedrock or surficial. This is used to default which table are on or off in field note page.