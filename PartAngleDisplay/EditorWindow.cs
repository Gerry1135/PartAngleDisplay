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
using System.Reflection;
using UnityEngine;
using KSP.IO;
using KSP.UI;
using KSP.UI.Screens;

namespace PartAngleDisplay
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class EditorWindow : MonoBehaviour
    {
        LogMsg Log = new LogMsg();

        Rect rArea;
        Rect rLabAngleP;
        Rect rValAngleP;
        Rect rLabAngleR;
        Rect rValAngleR;
        Rect rLabAngleY;
        Rect rValAngleY;
        Rect rLabIncP;
        Rect rResetIncP;
        Rect rValIncP;
        Rect rLabIncR;
        Rect rResetIncR;
        Rect rValIncR;
        Rect rLabIncY;
        Rect rResetIncY;
        Rect rValIncY;
        Rect rLabRot;
        Rect rIncRot;
        Rect rDecRot;
        Rect rValRot;
        Rect rLabFine;
        Rect rIncFine;
        Rect rDecFine;
        Rect rValFine;
        Rect rLabPartRel;
        Rect rButPartRel;

        GUI.WindowFunction windowFunc = null;
        Int32 windowID;
        Rect windowPos;
        Rect windowDragRect;
        ApplicationLauncherButton buttonAppLaunch = null;
        IButton buttonToolbar = null;
        EditorLogic editor;
        GUIStyle windowStyle;
        GUIStyle areaStyle;
        GUIStyle labelStyle;
        GUIStyle dataStyle;
        GUIStyle badDataStyle;
        GUIStyle buttonStyle;

        Vector3 eulerAngles;    // The current part rotation angles

        Part selPartUpdate;
        Quaternion attRotationUpdate;
        float rotVal;
        Vector3 rotAxis;

        String sPitch = "0.00";
        String sRoll = "0.00";
        String sYaw = "0.00";
        String sIncPitch = "0.0";
        String sIncRoll = "0.0";
        String sIncYaw = "0.0";

        String sIncCoarse = "90.0";
        String sIncFine = "5.0";
        bool startVisible = false;
        bool relativeRotate = false;
        bool absoluteAngles = false;
        bool useAppLaunch = true;

        KeyCode keyToggleWindow = KeyCode.P;
        KeyCode keyApplyEuler = KeyCode.P;
        KeyCode keyCycleRotate = KeyCode.B;
        KeyCode keyCycleFine = KeyCode.G;
        KeyCode keyVeryFineMod = KeyCode.LeftControl;

        static float[] angleCycle = { 0.01f, 0.1f, 1, 5, 10, 15, 30, 45, 60, 72, 90, 120 };
        static String[] angleCycleStr = { "0.01", "0.10", "1.00", "5.00", "10.00", "15.00", "30.00", "45.00", "60.00", "72.00", "90.00", "120.00" };
        static Texture2D texAppLaunch;

        const String windowTitle = "Part Angle Display (0.3.2.4)";
        const String configFilename = "settings.cfg";
        const String pathToolbarDisabled = "PartAngleDisplay/toolbaroff";
        const String pathToolbarEnabled = "PartAngleDisplay/toolbaron";

        private Boolean _Visible = false;

        public EditorWindow()
        {
            //Trace("EditorWindow.EditorWindow");
            //Trace("ApplicationLauncher is " + (ApplicationLauncher.Ready ? "" : "not ") + "ready");
            //Log.Flush();
        }

        public Boolean Visible
        {
            get { return _Visible; }
            set { _Visible = value; }
        }

        private void ToggleWindow()
        {
            Visible = !Visible;
        }

        public void Awake()
        {
            Trace("[PAD] EditorWindow.Awake");
            //Trace("ApplicationLauncher is " + (ApplicationLauncher.Ready ? "" : "not ") + "ready");

            editor = EditorLogic.fetch;

            CreateUIObjects();

            LayoutWindow();
            
            windowID = Guid.NewGuid().GetHashCode();

            LoadConfig();
            Log.Flush();
        }

        public void Start()
        {
            //Trace("[PAD] EditorWindow.Start");
            //Trace("ApplicationLauncher is " + (ApplicationLauncher.Ready ? "" : "not ") + "ready");

            if (ToolbarManager.ToolbarAvailable)
            {
                buttonToolbar = ToolbarManager.Instance.add("PAD", "button");
                buttonToolbar.ToolTip = "Part Angle Display";
                SetToolbarState();
                buttonToolbar.OnClick += e => ToggleWindow();
                buttonToolbar.Visible = true;
            }

            Visible = startVisible;
            Log.Flush();
        }

        void OnDestroy()
        {
            //Trace("[PAD]EditorWindow.OnDestroy");
            //Trace("ApplicationLauncher is " + (ApplicationLauncher.Ready ? "" : "not ") + "ready");

            SaveConfig();

            if (buttonAppLaunch != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(buttonAppLaunch);
                buttonAppLaunch = null;
            }

            if (buttonToolbar != null)
            {
                buttonToolbar.Destroy();
                buttonToolbar = null;
            }
            Log.Flush();
        }

        // Simple, hardwired config
        public void LoadConfig()
        {
            if (File.Exists<EditorWindow>(configFilename))
            {
                String[] lines = File.ReadAllLines<EditorWindow>(configFilename);

                for (int i = 0; i < lines.Length; i++)
                {
                    String[] line = lines[i].Split('=');
                    if (line.Length == 2)
                    {
                        String key = line[0].Trim();
                        String val = line[1].Trim();
                        if (key == "visible")
                            ReadBool(val, ref startVisible);
                        else if (key == "incPitch")
                            sIncPitch = val;
                        else if (key == "incRoll")
                            sIncRoll = val;
                        else if (key == "incYaw")
                            sIncYaw = val;
                        else if (key == "incCoarse")
                            sIncCoarse = val;
                        else if (key == "incFine")
                            sIncFine = val;
                        else if (key == "relRotate")
                            ReadBool(val, ref relativeRotate);
                        else if (key == "absAngles")
                            ReadBool(val, ref absoluteAngles);
                        else if (key == "windowPos")
                        {
                            String[] vals = val.Split(',');
                            if (vals.Length == 4)
                            {
                                windowPos.x = Convert.ToSingle(vals[0].Trim());
                                windowPos.y = Convert.ToSingle(vals[1].Trim());
                                windowPos.width = Convert.ToSingle(vals[2].Trim());
                                windowPos.height = Convert.ToSingle(vals[3].Trim());
                            }
                            else
                                Trace("Ignoring invalid rectangle in settings: '" + lines[i] + "'");
                        }
                        else if (key == "keyToggleWindow")
                            ReadKeyCode(val, ref keyToggleWindow, KeyCode.P);
                        else if (key == "keyApplyEuler")
                            ReadKeyCode(val, ref keyApplyEuler, KeyCode.P);
                        else if (key == "keyCycleRotate")
                            ReadKeyCode(val, ref keyCycleRotate, KeyCode.B);
                        else if (key == "keyCycleFine")
                            ReadKeyCode(val, ref keyCycleFine, KeyCode.G);
                        else if (key == "keyVeryFineMod")
                            ReadKeyCode(val, ref keyVeryFineMod, KeyCode.LeftControl);
                        else if (key == "useAppLaunch")
                            ReadBool(val, ref useAppLaunch);
                        else
                            Trace("Ignoring invalid key in settings: '" + lines[i] + "'");
                    }
                    else
                        Trace("Ignoring invalid line in settings: '" + lines[i] + "'");
                }
            }
        }

        public void SaveConfig()
        {
            TextWriter file = File.CreateText<EditorWindow>(configFilename);

            file.WriteLine("visible = " + (_Visible ? "true" : "false"));
            file.WriteLine("incPitch = " + sIncPitch);
            file.WriteLine("incRoll = " + sIncRoll);
            file.WriteLine("incYaw = " + sIncYaw);
            file.WriteLine("incCoarse = " + sIncCoarse);
            file.WriteLine("incFine = " + sIncFine);
            file.WriteLine("relRotate = " + (relativeRotate ? "true" : "false"));
            file.WriteLine("absAngles = " + (absoluteAngles ? "true" : "false"));
            file.WriteLine("windowPos = {0:f},{1:f},{2:f},{3:f}", windowPos.x, windowPos.y, windowPos.width, windowPos.height);
            file.WriteLine("keyToggleWindow = " + keyToggleWindow);
            file.WriteLine("keyApplyEuler = " + keyApplyEuler);
            file.WriteLine("keyCycleRotate = " + keyCycleRotate);
            file.WriteLine("keyCycleFine = " + keyCycleFine);
            file.WriteLine("keyVeryFineMod = " + keyVeryFineMod);
            file.WriteLine("useAppLaunch = " + (useAppLaunch ? "true" : "false"));

            file.Close();
        }

        void ReadBool(String val, ref bool variable)
        {
            if (val == "true")
                variable = true;
            else if (val == "false")
                variable = false;
        }

        void ReadKeyCode(String str, ref KeyCode variable, KeyCode defValue)
        {
            try
            {
                variable = (KeyCode)Enum.Parse(typeof(KeyCode), str, false);
                Log.buf.Append("Read value of:");
                Log.buf.AppendLine("" + variable);
            }
            catch (Exception exp)
            {
                Log.buf.Append("Unrecognised KeyCode: ");
                Log.buf.AppendLine(str);
                Log.buf.AppendLine(exp.ToString());
                variable = defValue;
            }
        }

        public void LateUpdate()
        {
            if (selPartUpdate != null && rotVal != 0f)
            {
                //Log.buf.AppendLine("Applying rotation of " + rotVal + " around " + rotAxis.ToString());
                selPartUpdate.attRotation = Quaternion.AngleAxis(rotVal, rotAxis) * attRotationUpdate;
                //Log.buf.AppendLine("rot after  = " + selPartUpdate.attRotation.ToString());
                GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartRotated, selPartUpdate);
                //Log.buf.AppendLine("rot event  = " + selPartUpdate.attRotation.ToString());
                //Log.Flush();
            }
        }

        public void Update()
        {
            if (useAppLaunch && buttonAppLaunch == null)
            {
                if (ApplicationLauncher.Ready)
                {
                    if (texAppLaunch == null)
                    {
                        texAppLaunch = new Texture2D(38, 38, TextureFormat.RGBA32, false);
                        texAppLaunch.LoadImage(System.IO.File.ReadAllBytes(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "applaunch.png")));
                    }

                    buttonAppLaunch = ApplicationLauncher.Instance.AddModApplication(
                        ToggleWindow,
                        ToggleWindow,
                        null,
                        null,
                        null,
                        null,
                        ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
                        texAppLaunch
                        );
                }
                else
                {
                    Trace("ApplicationLauncher is not ready in Update");
                }
            }

            SetAppLaunchState();
            SetToolbarState();

            // Main Update code starts here

            editor = EditorLogic.fetch;
            if (editor == null)
                return;

            if (editor.editorScreen != EditorScreen.Parts)
                return;

            Part part = EditorLogic.SelectedPart;

            // Remember the previous values
            Vector3 oldAngles = eulerAngles;

            // Get the current values and remember the part and current rotation for use in LateUpdate
            if (part != null)
            {
                selPartUpdate = part;
                attRotationUpdate = part.attRotation;

                // TODO: Get this from a different place in rotate mode and allow it to 
                // show building relative angles in place mode
                eulerAngles = part.attRotation.eulerAngles;
            }
            else
            {
                // No part selected so show zeros
                selPartUpdate = null;
                eulerAngles = Vector3.zero;
            }

            // Only update the angle strings if the values have changed
            if (eulerAngles.x != oldAngles.x)
                sPitch = eulerAngles.x.ToString("0.00");
            if (eulerAngles.y != oldAngles.y)
                sRoll = eulerAngles.y.ToString("0.00");
            if (eulerAngles.z != oldAngles.z)
                sYaw = eulerAngles.z.ToString("0.00");

            // Key handling
            // Get the state of the shift key and the configured modifier keys
            bool fineTweakKeyPressed = GameSettings.Editor_fineTweak.GetKey();
            bool veryFineTweakKeyPressed = Input.GetKey(keyVeryFineMod);
            bool modKeyPressed = GameSettings.MODIFIER_KEY.GetKey();

            if (ShouldRotateKeysWork())
            {
                // G/Shift-G/Mod-G  decrease/increase/reset rotation angle
                HandleCycleKey(keyCycleRotate, fineTweakKeyPressed, modKeyPressed, ref sIncCoarse);

                // F/Shift-F/Mod-F  decrease/increase/reset shift-rotation angle
                HandleCycleKey(keyCycleFine, fineTweakKeyPressed, modKeyPressed, ref sIncFine);
            }

            // When no part is selected:
            // Mod-P            toggle the visible state of the window
            if (part == null)
            {
                if (modKeyPressed && Input.GetKeyDown(keyToggleWindow))
                {
                    // Toggle the visibility
                    ToggleWindow();
                }
            }
            else if (ShouldRotateKeysWork())
            {
                // Otherwise we apply the relevant angle increments depending on which key was pressed
                // Mod-P: Applies all 3 axes using Euler angles
                if (modKeyPressed && Input.GetKeyDown(keyApplyEuler))
                {
                    //Trace("Applying part rotation");
                    Vector3 incAngles;
                    incAngles.x = GetSingleOrZero(sIncPitch);
                    incAngles.y = GetSingleOrZero(sIncRoll);
                    incAngles.z = GetSingleOrZero(sIncYaw);
                    part.attRotation = Quaternion.Euler(eulerAngles + incAngles);
                }

                // Work out what rotation we want and store it for application in LateUpdate
                // WASDQE           Apply our rotation of sPlainRotate
                // Shift-WASDQE     Apply our rotation of sShiftRotate
                // Mod-WASDQE       Apply our rotation of sIncPitch/Yaw/Roll
                rotVal = 0f;
                rotAxis = Vector3.zero;
                if (GameSettings.Editor_yawLeft.GetKeyDown())
                {
                    rotVal = fineTweakKeyPressed ? GetSingleOrZero(sIncFine) : (veryFineTweakKeyPressed ? GetSingleOrZero(sIncYaw) : GetSingleOrZero(sIncCoarse));
                    rotAxis = relativeRotate ? part.transform.forward : Vector3.forward;
                }
                else if (GameSettings.Editor_yawRight.GetKeyDown())
                {
                    rotVal = fineTweakKeyPressed ? -GetSingleOrZero(sIncFine) : -(veryFineTweakKeyPressed ? GetSingleOrZero(sIncYaw) : GetSingleOrZero(sIncCoarse));
                    rotAxis = relativeRotate ? part.transform.forward : Vector3.forward;
                }
                else if (GameSettings.Editor_rollLeft.GetKeyDown())
                {
                    rotVal = fineTweakKeyPressed ? GetSingleOrZero(sIncFine) : (veryFineTweakKeyPressed ? GetSingleOrZero(sIncRoll) : GetSingleOrZero(sIncCoarse));
                    rotAxis = relativeRotate ? part.transform.up : Vector3.up;
                }
                else if (GameSettings.Editor_rollRight.GetKeyDown())
                {
                    rotVal = fineTweakKeyPressed ? -GetSingleOrZero(sIncFine) : -(veryFineTweakKeyPressed ? GetSingleOrZero(sIncRoll) : GetSingleOrZero(sIncCoarse));
                    rotAxis = relativeRotate ? part.transform.up : Vector3.up;
                }
                else if (GameSettings.Editor_pitchUp.GetKeyDown())
                {
                    rotVal = fineTweakKeyPressed ? -GetSingleOrZero(sIncFine) : -(veryFineTweakKeyPressed ? GetSingleOrZero(sIncPitch) : GetSingleOrZero(sIncCoarse));
                    rotAxis = relativeRotate ? part.transform.right : Vector3.right;
                }
                else if (GameSettings.Editor_pitchDown.GetKeyDown())
                {
                    rotVal = fineTweakKeyPressed ? GetSingleOrZero(sIncFine) : (veryFineTweakKeyPressed ? GetSingleOrZero(sIncPitch) : GetSingleOrZero(sIncCoarse));
                    rotAxis = relativeRotate ? part.transform.right : Vector3.right;
                }
            }
            Log.Flush();
        }

        private bool ShouldRotateKeysWork()
        {
            return (editor.EditorConstructionMode == ConstructionMode.Place);
        }

        private void HandleCycleKey(KeyCode keyCode, bool shiftDown, bool modDown, ref String incValue)
        {
            if (keyCode != KeyCode.None && Input.GetKeyDown(keyCode))
            {
                if (modDown)
                    incValue = "5.0";
                else if (shiftDown)
                    incValue = IncreaseRotate(incValue);
                else
                    incValue = DecreaseRotate(incValue);
            }
        }

        private void OnGUI()
        {
            if (Visible)
                windowPos = GUI.Window(windowID, windowPos, windowFunc, windowTitle, windowStyle);
        }

        private void WindowGUI(int windowID)
        {
            GUI.Box(rArea, GUIContent.none, areaStyle);
            
            GUI.Label(rLabAngleP, "Pitch", labelStyle);
            GUI.Label(rValAngleP, sPitch, dataStyle);
            GUI.Label(rLabAngleR, "Roll", labelStyle);
            GUI.Label(rValAngleR, sRoll, dataStyle);
            GUI.Label(rLabAngleY, "Yaw", labelStyle);
            GUI.Label(rValAngleY, sYaw, dataStyle);

            GUI.Label(rLabIncP, "Pitch +/-", labelStyle);
            if (GUI.Button(rResetIncP, "x", buttonStyle))
                sIncPitch = "0.0";
            sIncPitch = GUI.TextField(rValIncP, sIncPitch, 7, GetDataStyle(sIncPitch));

            GUI.Label(rLabIncR, "Roll +/-", labelStyle);
            if (GUI.Button(rResetIncR, "x", buttonStyle))
                sIncRoll = "0.0";
            sIncRoll = GUI.TextField(rValIncR, sIncRoll, 7, GetDataStyle(sIncRoll));

            GUI.Label(rLabIncY, "Yaw +/-", labelStyle);
            if (GUI.Button(rResetIncY, "x", buttonStyle))
                sIncYaw = "0.0";
            sIncYaw = GUI.TextField(rValIncY, sIncYaw, 7, GetDataStyle(sIncYaw));

            GUI.Label(rLabRot, "Rotation", labelStyle);
            if (GUI.Button(rIncRot, "<", buttonStyle))
                sIncCoarse = IncreaseRotate(sIncCoarse);
            if (GUI.Button(rDecRot, ">", buttonStyle))
                sIncCoarse = DecreaseRotate(sIncCoarse);
            sIncCoarse = GUI.TextField(rValRot, sIncCoarse, 7, GetDataStyle(sIncCoarse));

            GUI.Label(rLabFine, "Fine", labelStyle);
            if (GUI.Button(rIncFine, "<", buttonStyle))
                sIncFine = IncreaseRotate(sIncFine);
            if (GUI.Button(rDecFine, ">", buttonStyle))
                sIncFine = DecreaseRotate(sIncFine);
            sIncFine = GUI.TextField(rValFine, sIncFine, 7, GetDataStyle(sIncFine));

            GUI.Label(rLabPartRel, "Part-relative", labelStyle);
            relativeRotate = GUI.Toggle(rButPartRel, relativeRotate, "", buttonStyle);

            GUI.DragWindow(windowDragRect);
        }

        private String IncreaseRotate(String sAngle)
        {
            // Look for the first item in the cycle larger than the value
            float angle = GetSingleOrZero(sAngle);
            for (int i = 0; i < angleCycle.Length; i++)
            {
                if (angleCycle[i] > angle)
                    return angleCycleStr[i];
            }

            // Nothing larger so go back to the start
            return angleCycleStr[0];
        }

        private String DecreaseRotate(String sAngle)
        {
            // Look for the last item in the cycle smaller than the value
            float angle = GetSingleOrZero(sAngle);
            for (int i = angleCycle.Length - 1; i >= 0; i--)
            {
                if (angleCycle[i] < angle)
                    return angleCycleStr[i];
            }

            // Nothing larger so go back to the start
            return angleCycleStr[angleCycle.Length - 1];
        }

        private GUIStyle GetDataStyle(String str)
        {
            float temp;
            return Single.TryParse(str, out temp) ? dataStyle : badDataStyle;
        }

        private float GetSingleOrZero(String str)
        {
            float temp;
            return Single.TryParse(str, out temp) ? temp : 0f;
        }

        private void CreateUIObjects()
        {
            windowFunc = new GUI.WindowFunction(WindowGUI);

            windowStyle = new GUIStyle(HighLogic.Skin.window)
            {
                fontSize = HighLogic.Skin.label.fontSize
            };

            areaStyle = new GUIStyle(HighLogic.Skin.textArea);

            labelStyle = new GUIStyle(HighLogic.Skin.label)
            {
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 1, 1)
            };

            dataStyle = new GUIStyle(HighLogic.Skin.label)
            {
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 1, 1)
            };

            badDataStyle = new GUIStyle(HighLogic.Skin.label)
            {
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(1.0f, 0.5f, 0.5f) },
                focused = { textColor = new Color(1.0f, 0.5f, 0.5f) },
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 1, 1)
            };

            buttonStyle = new GUIStyle(HighLogic.Skin.button)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(1, 0, 0, 0)
            };
        }

        private void LayoutWindow()
        {
            const int TopMargin = 28;
            const int LabelX = 12;
            const int LabelWidth = 60;
            const int ValueX = 128;
            const int ValueWidth = 60;
            const int Button1X = 70;
            const int Button2X = 90;
            const int LabelPartRelWidth = 100;
            const int ButPartRelX = 168;
            const int LabelHeight = 20;
            const int ButtonWidth = 20;
            const int ButtonHeight = 16;
            const int ButtonYOff = 3;
            const int RowHeight = 18;
            const int WndWidth = 200;
            const int WndHeight = 200;

            windowPos.Set(300, 200, WndWidth, WndHeight);
            windowDragRect.Set(0, 0, WndWidth, WndHeight);
            rArea.Set(5, 27, 190, 168);

            int yRow = TopMargin;
            rLabAngleP.Set(LabelX, yRow, LabelWidth, LabelHeight);
            rValAngleP.Set(ValueX, yRow, ValueWidth, LabelHeight);

            yRow += RowHeight;
            rLabAngleR.Set(LabelX, yRow, LabelWidth, LabelHeight);
            rValAngleR.Set(ValueX, yRow, ValueWidth, LabelHeight);

            yRow += RowHeight;
            rLabAngleY.Set(LabelX, yRow, LabelWidth, LabelHeight);
            rValAngleY.Set(ValueX, yRow, ValueWidth, LabelHeight);

            yRow += RowHeight;
            rLabIncP.Set(LabelX, yRow, LabelWidth, LabelHeight);
            rResetIncP.Set(Button1X, yRow + ButtonYOff, ButtonWidth, ButtonHeight);
            rValIncP.Set(ValueX, yRow, ValueWidth, LabelHeight);

            yRow += RowHeight;
            rLabIncR.Set(LabelX, yRow, LabelWidth, LabelHeight);
            rResetIncR.Set(Button1X, yRow + ButtonYOff, ButtonWidth, ButtonHeight);
            rValIncR.Set(ValueX, yRow, ValueWidth, LabelHeight);

            yRow += RowHeight;
            rLabIncY.Set(LabelX, yRow, LabelWidth, LabelHeight);
            rResetIncY.Set(Button1X, yRow + ButtonYOff, ButtonWidth, ButtonHeight);
            rValIncY.Set(ValueX, yRow, ValueWidth, LabelHeight);

            yRow += RowHeight;
            rLabRot.Set(LabelX, yRow, LabelWidth, LabelHeight);
            rIncRot.Set(Button1X, yRow + ButtonYOff, ButtonWidth, ButtonHeight);
            rDecRot.Set(Button2X, yRow + ButtonYOff, ButtonWidth, ButtonHeight);
            rValRot.Set(ValueX, yRow, ValueWidth, LabelHeight);

            yRow += RowHeight;
            rLabFine.Set(LabelX, yRow, LabelWidth, LabelHeight);
            rIncFine.Set(Button1X, yRow + ButtonYOff, ButtonWidth, ButtonHeight);
            rDecFine.Set(Button2X, yRow + ButtonYOff, ButtonWidth, ButtonHeight);
            rValFine.Set(ValueX, yRow, ValueWidth, LabelHeight);

            yRow += RowHeight;
            rLabPartRel.Set(LabelX, yRow, LabelPartRelWidth, LabelHeight);
            rButPartRel.Set(ButPartRelX, yRow + ButtonYOff, ButtonWidth, ButtonHeight);
        }

        private void SetAppLaunchState()
        {
            if (buttonAppLaunch != null)
            {
                if (_Visible && buttonAppLaunch.toggleButton.CurrentState == UIRadioButton.State.False)
                    buttonAppLaunch.SetTrue(false);
                else if (!_Visible && buttonAppLaunch.toggleButton.CurrentState == UIRadioButton.State.True)
                    buttonAppLaunch.SetFalse(false);
            }
        }

        private void SetToolbarState()
        {
            if (buttonToolbar != null)
                buttonToolbar.TexturePath = _Visible ? pathToolbarEnabled : pathToolbarDisabled;
        }

        private void Trace(String message)
        {
            Log.buf.AppendLine(message);
        }
    }
}
