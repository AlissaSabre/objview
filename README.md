
# objview
A sample C# program to read and render a Wavefront obj file using OpenGL

objview is a program to read a 3D geometory in Wavefront obj file and to redner it using OpenGL.
The project is more like my own practice/refresh of OpenGL knowledges/experiences than a practical utility.

The program is written in C# and relies on [OpenGL.NET](https://github.com/luca-piccioni/OpenGL.Net).

For the moment, the program targets OpenGL 2.2 or so and uses the good old fixed pipeline.
It should run on more recent OpenGL with the Compatibility Profile, though I'm not sure.
(To gain some knowledges on such issues is one of my objectives on experiementing with this program.)

If you are more familiar with C# than C++ and are trying to learn OpenGL, this program may be useful as a sample program.
Otherwise, it would have nothing to do with you.

For the [moment](https://github.com/AlissaSabre/objview/tree/19e0220f699d20f2f5dace71c5ec2bf2a4989b83):
* It runs on Windows with .NET Framework 4.5.
* It can read some obj files and show it on the window using vertex coordinates and normals in the file.  It uses program's own default material (texture/material information in obj is just ignored.)
* You can rotate the model using mouse.
