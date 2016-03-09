PartAngleDisplay
================

This is a simple KSP plugin that allows you to surface attach parts at accurate angles.  To install, simply copy the PartAngleDisplay folder from the zip file into the GameData folder in your KSP installation.

Simply put, the plugin allows you to display a small window in the VAB/SPH that displays the orientation of the currently selected part as pitch, roll and yaw angles and it also allows you to enter increment values for the pitch, roll and yaw and to apply these to the selected part.

It originally used a single hotkey (Mod-P as it didn't clash with any of the mods I use). If you do not have a part selected then it will show or hide the window. If you do have a part selected then it will apply the angle increments to the part by simply adding the increment values to the displayed Euler angles and re-setting the rotation of the selected part.  This has strange effects caused by the way that Euler angles work.

Version 0.2 introduced a new way use the plugin.  The original Mod-P hotkey still works the same way but it also now overrides the handling of Mod-WASDQE (the standard part rotation controls with Mod held down) in the stock game (they do the same as the unmodified key, rotate by 90 degrees) to instead rotate by whatever angle increment is entered into the respective field in the dialog.  This allows you to set increment values to 1 (or 0.1 or even 0.01) and then have seamless, accurate rotation of parts.

Version 0.2.1 fixed the handling of the standard pitch keys and makes the angle increments they use configurable.  W/S and Shift-W/S no longer go in opposite directions.  The < and > buttons can be used to cycle the angle setting through 120, 90, 72, 60, 45, 30, 15, 10, 5, 1, 0.1 and 0.01 degrees.  The F key also adjusts the "Fine" angle control (F to cycle down, Shift-F to cycle up and Mod-F to reset to 5).

Version 0.2.2 introduced "Part-relative" mode.  This changes the rotation keys to act around the axes of the currently selected part rather than the usual fixed axes.  E.g. if you rotate a Mk 1 plane cockpit to an odd angle and then switch to "Part-relative" and roll using Q and E the part will roll around its own axis.

Version 0.2.4.2 added the saving and loading of settings (window position, visibility and all the control settings).

Version 0.2.4.3 made the keyboard shortcuts configurable in the settings file.  To change the shortcuts you will need to run the game and enter and exit the VAB/SPH once for the default settings file to be written out (in GameData\PartAngleDisplay\PluginData\PartAngleDisplay\settings.cfg).  Then simply edit this file (you shouldn't even need to quit KSP) and change the three lines starting "key" to be the keycodes you desire.  Note that the "toggle window" and "apply Euler" operations always use the configured modifier key and the "cycle fine" operation uses plain, shifted and modified keypresses.

Version 0.2.4.4 added support for both the stock and Blizzy's toolbars.  The use of the stock toolbar can be disabled in the settings file (useAppLaunch).

Version 0.3.0.0 for KSP 0.90 has had to change quite a few things.  The key to cycle the fine angle increment now defaults to G because the stock editor uses F.  The normal angle increment can be cycled using B.  The modifier key for the WASDQE keys to use the separate axis increment values now defaults to Ctrl as Mod is used to disable surface attachment which would make it impossible to adjust surface attached parts in place.

Version 0.3.0.1 for KSP 1.0.2 is simply a recompile for the new version and a minor bug fix.

Version 0.3.1.0 for KSP 1.1 has been significantly refactored to simplify it and fix the part relative rotation that has been broken since the editor changes in KSP 0.90.

Change Log
==========
	09/03/2016 16:32 GMT   Updated for KSP 1.1
                           Refactored rotation application to fix part relative rotation

	02/05/2015 18:05 GMT   Updated for KSP 1.0.2
                           Fixed rotation in rotate mode to match stock behaviour
						   Updated to version 0.3.0.1

	16/12/2014 14:38 GMT   Updated for KSP 0.90
                           Changed fine increment cycling key to G (configurable)
						   Added normal increment cycling key of B (configurable)
						   Changed Mod-WASDQE to Ctrl-WASDQE (configurable)
						   Updated to version 0.3.0.0

	12/08/2014 19:50 GMT   Fixed yaw and roll controls when editor mode is changed using Editor Extensions
                           Updated to version 0.2.4.5

	04/08/2014 19:50 GMT   Now supports both the stock and Blizzy's toolbars
                           Use of stock toolbar can be disabled in settings (useAppLaunch)
                           Updated to version 0.2.4.4

	01/08/2014 11:57 GMT   Now allows the keyboard shortcuts to be configured in the settings file
                           Updated to version 0.2.4.3

	26/07/2014 17:33 GMT   Compiled against KSP 0.24.2
	                       Added loading and saving of settings (window position, visibility and all the control settings)
	                       Updated to version 0.2.4.2

	25/07/2014 08:43 GMT   Compiled against KSP 0.24.1
	                       Updated to version 0.2.4.1

	23/07/2014 22:50 GMT   Fixed build to target correct .NET runtime
	                       Updated to version 0.2.4.0

    17/07/2014 22:27 GMT   Swapped roll and yaw displays in SPH
                           Compiled against KSP 0.24
                           Updated version to 0.2.3.0

    13/07/2014 16:37 GMT   Implemented "Part-relative" mode that changes all part rotation axes to be relative to the selected part
                           Updated version to 0.2.2.0

    13/07/2014 12:29 GMT   Now totally overrides part rotation hotkeys
                           Allows changing of the default and fine rotation increments
                           Updated version to 0.2.1.0

    10/07/2014 21:52 GMT   Now uses configured key bindings for part rotation rather than hardwired WASDQE
                           Updated version to 0.2.0.2

    10/07/2014 20:53 GMT   Fixed roll and yaw rotation axes in SPH
                           Updated version to 0.2.0.1
                           Fixed version in title bar

    10/07/2014 12:12 GMT   Added handling of Mod-WASDQE to rotate by the entered amounts in the respective axes
                           Updated version to 0.2.0.0

    28/05/2014 09:48 GMT   Added buttons to zero the increment fields
                           Rearranged window to avoid things moving when entering values
                           First release version 0.1.0.1
    
    23/05/2014 14:06 GMT   Fixed editable fields to work better
    
    20/05/2014 23:17 GMT   Removed some unused code and logging
    
    20/05/2014 15:52 GMT   First release

The code is released under the MIT license (see PartAngleDisplay/EditorWindow.cs).
