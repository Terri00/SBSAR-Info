# SBSAR-Info
Gets some information from Substance Archive files in C#, an alternative to the Allegorithmic python/cmd interface. 

It is getting the info by itself, by decompressing and extracting the .xml files inside the archive. Meaning you don't need the python library to write tools in C#.

## Example:
Included in Program.cs is a small example program to output some information about the archive file on args[0]
![Basic info](http://harrygodden.com/rs/?i=5c5fd60ef2a59.png)

## Example 2 (Auto convert SBSAR to source):
The solution 'SBSARSourceEngine' is a rudimentary source integration that automatically converts any sbsar files dropped into a source engine game materials folder, exporting diffuse/normal and creating a vmt by itself. The material is then ready to use in hammer.

It takes the first argument on the command line as the path to the materials folder or defaults to csgo's material folder.

Demonstration:
https://www.youtube.com/watch?v=sqCZgbNo5Fs
