# Bitbendaz Atari STe palette and raster toolkit
Calculate Atari Ste palettes (12-bit colors), tweak palettes, export palettes to Motorola 68000 assembler code, create fades and generate gradients/rasters.
This is the ultimate tool to tweak palette data from Degas PI1 files.

Inspired by the fade calculator and the rasterbar generator from Simon Sunnyboy/Paradize.

Warning, this is an early alpha version, most likely containing issues and ugly UI :)

**Work in progress!**

## Requirements
.NET Core 3.

VisualStudio 2019 is recommended for compiling the WPF client.

## Documentation

![](screen1.png)

### Pictures and Palettes tab
Click **Browse** to load a Degas PI1 picture. When loaded, the picture and the palette will be shown.
Click a color in the palette to select a new color. When a new color is selected, the picture and palette will be updated.
Modify a hex-value in the palette textbox and click **Update** to apply the new color.
Click **Reset** to restore the original palette.
#### Adjusting HSL 
Adjust Hue, Saturation and Lightness with the sliders.
The generated palette are shown below the sliders.

### Fades
Data type allows you to control which datatype the generated assembler code will use.
Click **Fade to black** to generate a fade from the loaded picture's palette which fades to black in 16 steps.
Click **Fade to white** to generate a fade from the loaded picture's palette which fades to white in 16 steps.
Click **Hue Fade** to generate a fade from the loaded picture's palette to the adjusted HSL palette in 16 steps.

When a fade is generated, the generated assembler code will be shown as well as the generated palette data as a matrix of 16 * 16 items.

![](screen2.png)

### Rasters
This tab allows you to create, preview and export raster-data.
The tab is divided into four main columns:

#### Picture preview:
* Click **Load** to load a picture used for the previewing
* Click **Refresh** to refresh the preview image with the defined raster data
* Change **Mask color** to control which color in the picture the raster data will be merged into

#### Color picker
* Use the color picker to change color for the selected item in the Raster list

#### Raster list
Displays all 200 items with color and 12-bit hexcode for the color in the list.
* **Selec**t a color to change it
* Copy the selected color by pressing **Ctrl+C** or click the Copy button
* Paste a copied color to one or more selected items with **Ctrl+V** or click the Paste button
* Create a gradient between the first selected color and the last selected color by clicking the **Gradient** button
* Click the **Paste Asm** button to paste raster data from the clipboard in dc.w assembler syntax. The data will automatically be parsed and shown.
* Click **Load** to load saved raster data
* Click **Save** to save the current raster data
* Click **Generate** to generate assembler raster data. The generated data is shown in the *Output tab*

#### Preview
Displays a preview of the definied raster data. The selected index is shown to the left of the preview in red.

![](screen3.png)


### Gradients
Generate a gradient between the start color and the end color (16 steps). Change the colors either by entering the 12-bit hexcode for the color (RGB) or click **Pick** to select a color.
Click **Generate** to generate the gradient and a preview.

### Output
Generated assembler data from the *Raster list* page is shown here.

![](screen4.png)


## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[Beerware](https://en.wikipedia.org/wiki/Beerware)


Created by [Mikael Stalvik (Stalvik / Bitbendaz)](https://demozoo.org/sceners/27448/)

