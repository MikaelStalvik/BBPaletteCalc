# Bitbendaz Atari STe palette and raster toolkit
Calculate AtariSte palettes, tweak palettes, export palettes to Motorola 68000 assembler code, create fades and generate gradients/rasters.
This is the ultimate tool to tweak palette data from Degas PI1 files.

Warning, this is an early alpha version, most likely containing issues and ugly UI :)

**Work in progress!**

![](screen1.png)
![](screen2.png)
![](screen3.png)

## Requirements
.NET Core 3.

VisualStudio 2019 is recommended for compiling the WPF client.

## Documentation
### Pictures and Palettes tab
Click Browse to load a Degas PI1 picture. When loaded, the picture and the palette will be shown.
Click a color in the palette to select a new color. When a new color is selected, the picture and palette will be updated.
Modify a hex-value in the palette textbox and click Update to apply the new color.
Click Reset to restore the original palette.
#### Adjusting HSL 
Adjust Hue, Saturation and Lightness with the sliders.
The generated palette are shown below the sliders.


## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[Beerware](https://en.wikipedia.org/wiki/Beerware)


Created by [Mikael Stalvik (Stalvik / Bitbendaz)](https://demozoo.org/sceners/27448/)

