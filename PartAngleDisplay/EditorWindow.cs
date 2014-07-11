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
        GUIStyle windowStyle;
        GUIStyle areaStyle;
        GUIStyle dataStyle;
        GUIStyle badDataStyle;
        GUIStyle buttonStyle;
        Vector3 eulerAngles;    // The current part rotation angles
        String sPitch = "0.0";
        String sRoll = "0.0";
        String sYaw = "0.0";
        String sIncPitch = "0.0";
        String sIncRoll = "0.0";
        String sIncYaw = "0.0";

        String sPlainRotate = "90.0";
        String sShiftRotate = "5.0";

        static float[] angleCycle = { 0.01f, 0.1f, 1, 5, 10, 15, 30, 45, 60, 72, 90, 120 };

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
            //Trace("EditorWindow.EditorWindow");

            WindowID = Guid.NewGuid().GetHashCode();
        }

        public void Awake()
        {
            //Trace("EditorWindow.Awake");

            editor = EditorLogic.fetch;

            InitStyles();

            WindowTitle = "Part Angle Display (0.2.0.2)";
            WindowRect = new Rect(300, 200, 200, 50);

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
            sPitch = eulerAngles.x.ToString("0.00");
            sRoll = eulerAngles.y.ToString("0.00");
            sYaw = eulerAngles.z.ToString("0.00");

            //check for the various alt/mod etc keypresses
            //bool altKeyPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
            // Actually read the configured modifier key binding
            bool altKeyPressed = GameSettings.MODIFIER_KEY.GetKeyDown();
            bool shiftKeyPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);

            // When no part is selected:
            // ALT-P            toggle the visible state of the window
            // F/Shift-F/Alt-F  decrease/increase/reset shift-rotation angle
            if (!editor.PartSelected)
            {
                if (altKeyPressed && Input.GetKeyDown(KeyCode.P))
                {
                    // Toggle the visibility
                    Visible = !Visible;
                }
            }
            else
            {
                // Otherwise we apply the relevant angle increments depending on which key was pressed
                // ALT+P: Applies all 3 axes using Euler angles
                // ALT+W: Applies +pitch
                // ALT+S: Applies -pitch
                // ALT+A: Applies +yaw
                // ALT+D: Applies -yaw
                // ALT+Q: Applies +roll
                // ALT+E: Applies -roll
                if (altKeyPressed)
                {
                    if (Input.GetKeyDown(KeyCode.P))
                    {
                        //Trace("Applying part rotation");
                        Vector3 incAngles;
                        incAngles.x = GetSingleOrZero(sIncPitch);
                        incAngles.y = GetSingleOrZero(sIncRoll);
                        incAngles.z = GetSingleOrZero(sIncYaw);
                        editor.partRotation = Quaternion.Euler(eulerAngles + incAngles);
                    }
                    else if (GameSettings.Editor_pitchDown.GetKeyDown())
                    {
                        ApplyIncrements(GetSingleOrZero(sIncPitch) - 90f, 0f, 0f);
                    }
                    else if (GameSettings.Editor_pitchUp.GetKeyDown())
                    {
                        ApplyIncrements(90f - GetSingleOrZero(sIncPitch), 0f, 0f);
                    }
                    else if (GameSettings.Editor_yawLeft.GetKeyDown())
                    {
                        ApplyIncrements(0f, GetSingleOrZero(sIncYaw) - 90f, 0f);
                    }
                    else if (GameSettings.Editor_yawRight.GetKeyDown())
                    {
                        ApplyIncrements(0f, 90f - GetSingleOrZero(sIncYaw), 0f);
                    }
                    else if (GameSettings.Editor_rollLeft.GetKeyDown())
                    {
                        ApplyIncrements(0f, 0f, GetSingleOrZero(sIncRoll) - 90f);
                    }
                    else if (GameSettings.Editor_rollRight.GetKeyDown())
                    {
                        ApplyIncrements(0f, 0f, 90f - GetSingleOrZero(sIncRoll));
                    }
                }
            }
        }

        private void ApplyIncrements(float incPitch, float incYaw, float incRoll)
        {
            bool isVAB = editor.editorType == EditorLogic.EditorMode.VAB;
            if (incPitch != 0f)
            {
                //Trace("Applying pitch of " + incPitch);
                Quaternion qPitch = Quaternion.AngleAxis(incPitch, Vector3.left);
                //Trace("quaternion = " + qPitch.ToString());
                editor.partRotation = qPitch * editor.partRotation;
            }
            if (incYaw != 0f)
            {
                //Trace("Applying yaw of " + incYaw);
                Quaternion qYaw = Quaternion.AngleAxis(incYaw, isVAB ? Vector3.forward : Vector3.down);
                //Trace("quaternion = " + qYaw.ToString());
                editor.partRotation = qYaw * editor.partRotation;
            }
            if (incRoll != 0f)
            {
                //Trace("Applying roll of " + incRoll);
                Quaternion qRoll = Quaternion.AngleAxis(incRoll, isVAB ? Vector3.up : Vector3.forward);
                //Trace("quaternion = " + qRoll.ToString());
                editor.partRotation = qRoll * editor.partRotation;
            }
        }

        private void DoPostDraw()
        {
            if (Visible)
                WindowRect = GUILayout.Window(WindowID, WindowRect, Window, WindowTitle, windowStyle);
        }

        private void Window(int windowID)
        {
            GUILayout.BeginHorizontal(areaStyle);

            GUILayout.BeginVertical();
            GUILayout.Label("Pitch");
            GUILayout.Label("Roll");
            GUILayout.Label("Yaw");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pitch +/-", GUILayout.Width(60));
            if (GUILayout.Button("x", buttonStyle, GUILayout.Width(20)))
                sIncPitch = "0.0";
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Roll +/-", GUILayout.Width(60));
            if (GUILayout.Button("x", buttonStyle, GUILayout.Width(20)))
                sIncRoll = "0.0";
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Yaw +/-", GUILayout.Width(60));
            if (GUILayout.Button("x", buttonStyle, GUILayout.Width(20)))
                sIncYaw = "0.0";
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Rotation", GUILayout.Width(60));
            if (GUILayout.Button("<", buttonStyle, GUILayout.Width(20)))
                sPlainRotate = IncreaseRotate(sPlainRotate);
            if (GUILayout.Button(">", buttonStyle, GUILayout.Width(20)))
                sPlainRotate = DecreaseRotate(sPlainRotate);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Fine Rotation", GUILayout.Width(60));
            if (GUILayout.Button("<", buttonStyle, GUILayout.Width(20)))
                sShiftRotate = IncreaseRotate(sShiftRotate);
            if (GUILayout.Button(">", buttonStyle, GUILayout.Width(20)))
                sShiftRotate = DecreaseRotate(sShiftRotate);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label(eulerAngles.x.ToString("0.00"), dataStyle);
            GUILayout.Label(eulerAngles.y.ToString("0.00"), dataStyle);
            GUILayout.Label(eulerAngles.z.ToString("0.00"), dataStyle);
            sIncPitch = GUILayout.TextField(sIncPitch, 7, GetDataStyle(sIncPitch));
            sIncRoll = GUILayout.TextField(sIncRoll, 7, GetDataStyle(sIncRoll));
            sIncYaw = GUILayout.TextField(sIncYaw, 7, GetDataStyle(sIncYaw));
            sShiftRotate = GUILayout.TextField(sShiftRotate, 7, GetDataStyle(sShiftRotate));
            sPlainRotate = GUILayout.TextField(sPlainRotate, 7, GetDataStyle(sPlainRotate));
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        private String IncreaseRotate(String sAngle)
        {
            // Look for the first item in the cycle larger than the value
            float angle = GetSingleOrZero(sAngle);
            for (int i = 0; i < angleCycle.Length; i++)
            {
                if (angleCycle[i] > angle)
                    return angleCycle[i].ToString("0.00");
            }

            // Nothing larger so go back to the start
            return angleCycle[0].ToString("0.00");
        }

        private String DecreaseRotate(String sAngle)
        {
            // Look for the last item in the cycle smaller than the value
            float angle = GetSingleOrZero(sAngle);
            for (int i = angleCycle.Length - 1; i >= 0; i--)
            {
                if (angleCycle[i] < angle)
                    return angleCycle[i].ToString("0.00");
            }

            // Nothing larger so go back to the start
            return angleCycle[angleCycle.Length - 1].ToString("0.00");
        }

        private GUIStyle GetDataStyle(String str)
        {
            float temp;
            if (Single.TryParse(str, out temp))
            {
                return dataStyle;
            }
            return badDataStyle;
        }

        private float GetSingleOrZero(String str)
        {
            float temp;
            if (Single.TryParse(str, out temp))
            {
                return temp;
            }
            return 0f;
        }

        private void InitStyles()
        {
            windowStyle = new GUIStyle(HighLogic.Skin.window);

            areaStyle = new GUIStyle(HighLogic.Skin.textArea);
            areaStyle.active = areaStyle.hover = areaStyle.normal;

            dataStyle = new GUIStyle(HighLogic.Skin.label);
            dataStyle.fontStyle = FontStyle.Normal;
            dataStyle.alignment = TextAnchor.MiddleRight;
            dataStyle.stretchWidth = true;

            badDataStyle = new GUIStyle(HighLogic.Skin.label);
            badDataStyle.fontStyle = FontStyle.Normal;
            badDataStyle.alignment = TextAnchor.MiddleRight;
            badDataStyle.stretchWidth = true;
            badDataStyle.normal.textColor = new Color(1.0f, 0.5f, 0.5f);
            badDataStyle.focused.textColor = new Color(1.0f, 0.5f, 0.5f);

            buttonStyle = new GUIStyle(HighLogic.Skin.button);
            badDataStyle.fixedWidth = 20;
            buttonStyle.padding = new RectOffset(0, 0, 0, 0);
            buttonStyle.border = new RectOffset(4, 0, 0, 0);
        }

#if false
        private void Trace(String message)
        {
            print(message);
        }
#endif
    }
}
