# VISUAL STUDIO

## Build errors

### Hitting a Zip exception

It happens when an Android emulator has been launched, it seems to lock some files. Restarting VS will fix this and the emulator 
can still be kept opened while VS restarts.

---

# ANDROID
## Useful commands in Adb

### Accessing the device (plugged in android phone or emulator)

Apply this command while the device is running
`adb shell`

### Accessing app files

For security purposes all the app files are found within the app own folder behind some permission.

#### Enabling permission

Not doing this will result in a "permission denied" message.

`run-as GSCFieldApp.GSCFieldApp`

#### Accessing/Reading the files

This is how we can see if the geopackage have been written properly within the app folder.

`cd files`
`ls`

#### Copying files into PC

`adb pull /storage/self/primary/download/asd_DSA.gpkg W:\Transit\Gab`

#### Copying files to Android

`adb push "C:\Users\ghuotvez\AppData\Local\Xamarin\Mono for Android\Archives\GSCFieldApp.GSCFieldApp.apk" /storage/self/primary/download`

## Publishing

App needs to be deployed and build first.

In debug mode, app package has around 150Mb.
In release with coding trimming and code shrinker it goes down to 42Mb.
In release mode with ahead of time enabled to goes up to 56Mb, but the code should be faster.

### Pub Android
Password is gscfieldapp for current key

## Coding style

Usually class names should use CamelCasing, but for some reasons the model class in which CamalCasing is used, 
do not work well with the observableProperty inside the viewmodels. EM is a good example, passing the whole 
class to refill the form wasn't working until I removed the capital case M in the class name.

---
