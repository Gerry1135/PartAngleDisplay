/*
 Copyright (c) 2014 Gerry Iles (Padishar)

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PartAngleDisplay
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class EditorWindow : MonoBehaviour
    {
        Int32 WindowID;
        String WindowTitle;
        Rect WindowRect;

        EditorLogic editor;
        GUIStyle headingStyle;
        GUIStyle dataStyle;
        GUIStyle windowStyle;
        GUIStyle buttonStyle;
        GUIStyle areaStyle;
        Vector3 eulerAngles;    // The current part rotation angles
        Vector3 incAngles;      // The current angle increments to apply

        private Boolean _Visible = false;
        public Boolean Visible
        {
            get { return _Visible; }
            set
            {
                if (_Visible != value)
                {
                    if (value)
                        RenderingManager.AddToPostDrawQueue(5, DoPostDraw);
                    else
                        RenderingManager.RemoveFromPostDrawQueue(5, DoPostDraw);
                }
                _Visible = value;
            }
        }


        public EditorWindow()
        {
            Trace("EditorWindow.EditorWindow");

            WindowID = Guid.NewGuid().GetHashCode();
        }

        public void Awake()
        {
            Trace("EditorWindow.Awake");

            editor = EditorLogic.fetch;

            InitStyles();

            WindowTitle = "Part Angle Display";
            WindowRect = new Rect(0, 0, 160, 200);

            Visible = false;
        }

        public void Update()
        {
            editor = EditorLogic.fetch;
            if (editor == null)
                return;

            if (editor.editorScreen != EditorLogic.EditorScreen.Parts)
                return;

            // Update our values
            if (editor.PartSelected)
            {
                //Trace("partRotation = " + editor.partRotation.eulerAngles.ToString());
                eulerAngles = editor.partRotation.eulerAngles;
            }
            else
            {
                eulerAngles = Vector3.zero;
            }

            //check for the various alt/mod etc keypresses
            bool altKeyPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);

            // ALT+P: The only key shortcut
            // When a part is selected we apply the angle increments to the part's current rotation
            // When no part is selected we toggle the visible state of the window
            if (altKeyPressed && Input.GetKeyDown(KeyCode.P))
            {
                if (editor.PartSelected)
                {
                    Trace("Applying part rotation");
                    editor.partRotation = Quaternion.Euler(eulerAngles + incAngles);
                }
                else
                {
                    // Toggle the visibility
                    Visible = !Visible;
                }
            }
        }

        private void DoPostDraw()
        {
            if (Visible)
            {
                WindowRect = GUILayout.Window(WindowID, WindowRect, Window, WindowTitle, windowStyle);
            }
        }

        private void Window(int windowID)
        {
            GUILayout.BeginHorizontal(areaStyle);

            GUILayout.BeginVertical();
            GUILayout.Label("Pitch");
            GUILayout.Label("Roll");
            GUILayout.Label("Yaw");
            GUILayout.Label("Pitch +/-");
            GUILayout.Label("Roll +/-");
            GUILayout.Label("Yaw +/-");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label(eulerAngles.x.ToString("0.00"), dataStyle);
            GUILayout.Label(eulerAngles.y.ToString("0.00"), dataStyle);
            GUILayout.Label(eulerAngles.z.ToString("0.00"), dataStyle);
            incAngles.x = Convert.ToSingle(GUILayout.TextField(incAngles.x.ToString("0.00"), 8));
            incAngles.y = Convert.ToSingle(GUILayout.TextField(incAngles.y.ToString("0.00"), 8));
            incAngles.z = Convert.ToSingle(GUILayout.TextField(incAngles.z.ToString("0.00"), 8));
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }



        private void InitStyles()
        {
            windowStyle = new GUIStyle(HighLogic.Skin.window);

            buttonStyle = new GUIStyle(HighLogic.Skin.button);

            areaStyle = new GUIStyle(HighLogic.Skin.textArea);
            areaStyle.active = areaStyle.hover = areaStyle.normal;

            headingStyle = new GUIStyle(HighLogic.Skin.label);
            headingStyle.normal.textColor = Color.white;
            headingStyle.fontStyle = FontStyle.Normal;
            headingStyle.alignment = TextAnchor.MiddleCenter;
            headingStyle.stretchWidth = true;

            dataStyle = new GUIStyle(HighLogic.Skin.label);
            dataStyle.fontStyle = FontStyle.Normal;
            dataStyle.alignment = TextAnchor.MiddleRight;
            dataStyle.stretchWidth = true;
        }

        private void Trace(String message)
        {
            print(message);
        }

        private float ToDeg(float radians)
        {
            return radians * 180f / (float)Math.PI;
        }

        private float ToRad(float degrees)
        {
            return degrees * (float)Math.PI / 180f;
        }
    }
}
