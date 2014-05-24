PartAngleDisplay
================

This is a simple KSP plugin that allows you to surface attach parts at accurate angles.

Simply put, the plugin allows you to display a small window in the VAB/SPH that displays the orientation of the currently selected part as pitch, roll and yaw angles and it also allows you to enter increment values for the pitch, roll and yaw and to apply these to the selected part.

It currently uses a single hotkey (Alt-P as it doesn't clash with any of the mods I use). If you do not have a part selected then it will show or hide the window. If you do have a part selected then it will apply the angle increments to the part.

It isn't "nice" or particularly intuitive but I am releasing it as is just in case anyone else finds it useful or wants to develop it further. There are various issues with it (e.g. the editable fields behave quite strange) but it works for what I designed it for (setting the pitch angle of radially attached girders to 2 decimal places).

Change Log
==========
23/05/2014 14:06 GMT   Fixed editable fields to work better

20/05/2014 23:17 GMT   Removed some unused code and logging

20/05/2014 15:52 GMT   First release

The code is released under the MIT license (see PartAngleDisplay/EditorWindow.cs).
