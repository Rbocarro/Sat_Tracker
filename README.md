# Sat_Tracker
![](./ReadmeAssets/Holgraphic_AR.png)
This project is an Implementation of SGP4 in Unity that takes in a list of TLE element and uses an implmentation of SGP4 called SGP to propogate the orbits of satellites that have been launched in the past 30 days based on an 

# Demo Download

[Android APK](https://github.com/Rbocarro/Holgraphic_AR/releases)


# Tools and Techniques Used

## TLE
![](./ReadmeAssets/tlebreakdown.png)<br/>
TLE is used to encode orbital data of satellites at a set time with parameters such as the satellite's Norad No.,Apogee, Perigee, Eccentricity, Inclination etc thus enabling predictions of their past or future positions using models such as SGP. There are several sites that provied TLE data of satellites and I found out that celestrak.org provides a good list of easy to use TLE elements such as all currently active satellites, NOAA satellites etc. For this project, I primarily used the list of satellites that have been launched in the last 30 days. Also SO to Dr. T.S. Kelso for running Celestrak. its a cool site.

## SGP
SGP(Simplified General Pertuberation) Models are mathamatical models that are used to calculate the orbital postion of satellites based on 

## Utility Class
![](./ReadmeAssets/Nadir.svg)<br/>
the Utility class is a wrapper for the SGP library for unity that allowd me to make some easliy useable founctions that help convert stuff like geodesic cooridinates into Unity Vector 3 Postions. I also added additional methods such as the abilty to caluclate the Nadir point of a satellite on the surface of the earth.

## Holographic Shader
![](./ReadmeAssets/HolofoilEffect.gif)<br/>
todo

##Stylised Specular
![](./ReadmeAssets/specular.gif)<br/>
Specular reflections were faked using a premade texture that was offset based off the view angle.

## Card Overlay
Card Overlay's visual elements were created using [Pokecardmaker](https://pokecardmaker.net/) .

## Improvements
+ The main performance hurdle is the calculation and rendering of several satellites. since each satellite has its own update loop which calculates its respective position, rotation and scale with respect to the camera and is all done on the main CPU thread. The code could be modified to use Unity's ECS and GPU instancing to help parallse some of the computation on either the GPU or CPU threads

## References
+ [Wikipedia - Two-line element set](https://en.wikipedia.org/wiki/Two-line_element_set)
+ [Wikipedia - Simplified perturbations models](https://en.wikipedia.org/wiki/Simplified_perturbations_models)
+ [Wikipedia - Earth-centered inertial](https://en.wikipedia.org/wiki/Earth-centered_inertial
+ [Celestrak.org](https://celestrak.org/NORAD/elements/)


