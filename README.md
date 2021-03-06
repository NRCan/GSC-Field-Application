# Geological Survey Canada Field Application

([Français](#Application-de-notes-terrain-de-la-Commission-géologique-du-Canada))

This project is a Geological Survey Canada on-site data collection application for geologists. The application automatically stores locational data for each location created and allows data entry of complex geological information. 

The application mainly presents a GIS map page with access to specialized forms. Map layers and forms data are presented as notebooks that can be transfered between application/handhelds and save with some metadata like current working project name, geologist and activity name.

Target users are geologist that would like an easy to use, validation, standardize and consult field geological information for later office and analysis use. This application is intented for regional geology for data gathering or cartography.

The GSC Field Application only records bedrock geology data at this time, but we will be extending it to include surficial data in the near future.

## Requirements

Windows 10 Minimum build 17763, version 1809. 

## Installation

The application is side-loaded onto any Windows 10 PC that meets the build requirement. A developer configuration is needed on W10 in order to side-load this application.

## Data Format

The current application uses a SQLite data format to store all information gathered by users. In addition, a spatial extension called SpatialLite can be added to it and used in any GIS software like QGIS, ESRI ArcGIS or ESRI Desktop Pro.

### Development environment

Universal Windows Platform (UWP), Visual Studio Pro, 2019

## Contacts

Gabriel Huot-Vézina: gabriel.huot-vezina@canada.ca

Étienne Girard: etienne.girard@canada.ca


### How to Contribute

See [CONTRIBUTING.md](CONTRIBUTING.md)

### License
Unless otherwise noted, the source code of this project is covered under Crown Copyright, Government of Canada, and is distributed under the [MIT Licence](LICENSE.txt)

The Canada wordmark and related graphics associated with this distribution are protected under trademark law and copyright law. No permission is granted to use them outside the parameters of the Government of Canada's corporate identity program. For more information, see [Federal identity requirements](https://www.canada.ca/en/treasury-board-secretariat/topics/government-communications/federal-identity-requirements.html).

______________________

# Application de notes terrain de la Commission géologique du Canada

([English](#Geological-Survey-Canada-Field-Application))

Ce projet est une application de collecte de données sur le terrain de la Commission géologique du Canada pour les géologues. L'application stocke automatiquement les données de localisation pour chaque emplacement créé et permet la saisie de données d'informations géologiques complexes.

L'application présente principalement une carte SIG avec accès à des formulaires spécialisés. Les couches de carte et les données de formulaires sont présentées sous forme de cahiers qui peuvent être transférés entre les applications/ordinateurs de poche et enregistrés avec certaines métadonnées telles que le nom du projet de travail actuel, le géologue et le nom de l'activité.

Le public cible est principalement les géologues et intervenants souhaitant récolter des données géolgiques de type régionales de manière standard. De manière à obtenir des jeux de données validate et bien organisées.

Cette application permet de récolter les données concernant la géologique du socle rocheux et sommairement des dépôts de surface. Pour cette dernière de nouveau formulaire seront disponible dans un proche avenir.

## Pré-requis

Windows 10, version compilée minimale 17763, version 1809. 

## Installation

L'application doit être installé manuellement sur toute plateforme Windows 10 qui atteint le pré-requis minimal. Une configuration en mode développeur est aussi nécessaire pour l'installation manuelle.

## Format de donnée

L'application actuelle utilise un format de données SQLite pour stocker toutes les informations recueillies par les utilisateurs. De plus, une extension spatiale appelée SpatialLite peut y être ajoutée et utilisée dans n'importe quel logiciel SIG comme QGIS, ESRI ArcGIS ou ESRI Desktop Pro.

### Environnement de développement
Plateforme Windows universelle (PWU), Visual Studio Pro, 2019

### Contacts

Gabriel Huot-Vézina: gabriel.huot-vezina@canada.ca

Étienne Girard: etienne.girard@canada.ca

### Comment contribuer

Voir [CONTRIBUTING.md](CONTRIBUTING.md)

### Licence

Sauf indication contraire, le code source de ce projet est protégé par le droit d'auteur de la Couronne du gouvernement du Canada et distribué sous la [licence MIT](LICENSE.txt).

Le mot-symbole « Canada » et les éléments graphiques connexes liés à cette distribution sont protégés en vertu des lois portant sur les marques de commerce et le droit d'auteur. Aucune autorisation n'est accordée pour leur utilisation à l'extérieur des paramètres du programme de coordination de l'image de marque du gouvernement du Canada. Pour obtenir davantage de renseignements à ce sujet, veuillez consulter les [Exigences pour l'image de marque](https://www.canada.ca/fr/secretariat-conseil-tresor/sujets/communications-gouvernementales/exigences-image-marque.html).

