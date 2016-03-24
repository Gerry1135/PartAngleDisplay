﻿/*
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

        Int32 WindowID;
        String WindowTitle;
        Rect WindowRect;
        ApplicationLauncherButton buttonAppLaunch = null;
        IButton buttonToolbar = null;
        EditorLogic editor;
        GUIStyle windowStyle;
        GUIStyle areaStyle;
        GUIStyle labelStyle;
        GUIStyle dataStyle;
        GUIStyle badDataStyle;
        GUIStyle buttonStyle;
        GUILayoutOption gloWidth20;
        GUILayoutOption gloWidth40;
        GUILayoutOption gloWidth60;

        Vector3 eulerAngles;    // The current part rotation angles

        Part selPartUpdate;
        Quaternion attRotationUpdate;
        float rotVal;
        Vector3 rotAxis;

        String sPitch = "0.0";
        String sRoll = "0.0";
        String sYaw = "0.0";
        String sIncPitch = "0.0";
        String sIncRoll = "0.0";
        String sIncYaw = "0.0";

        String sIncCoarse = "90.0";
        String sIncFine = "5.0";
        bool startVisible = false;
        bool relativeRotate = false;
        bool absoluteAngles = false;
        bool useAppLaunch = true;

        Int32 keyToggleWindow = (Int32)KeyCode.P;
        Int32 keyApplyEuler = (Int32)KeyCode.P;
        Int32 keyCycleRotate = (Int32)KeyCode.B;
        Int32 keyCycleFine = (Int32)KeyCode.G;
        Int32 keyVeryFineMod = (Int32)KeyCode.LeftControl;

        static float[] angleCycle = { 0.01f, 0.1f, 1, 5, 10, 15, 30, 45, 60, 72, 90, 120 };
        static Texture2D texAppLaunch;

        const string configFilename = "settings.cfg";
        const string pathToolbarDisabled = "PartAngleDisplay/toolbaroff";
        const string pathToolbarEnabled = "PartAngleDisplay/toolbaron";
        const String WindowTitle = "Part Angle Display (0.3.1.1)";

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
            //Trace("[PAD] EditorWindow.Awake");
            //Trace("ApplicationLauncher is " + (ApplicationLauncher.Ready ? "" : "not ") + "ready");

            editor = EditorLogic.fetch;

            CreateUIObjects();

            WindowRect = new Rect(300, 200, 200, 50);
            WindowID = Guid.NewGuid().GetHashCode();

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
                string[] lines = File.ReadAllLines<EditorWindow>(configFilename);

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] line = lines[i].Split('=');
                    if (line.Length == 2)
                    {
                        string key = line[0].Trim();
                        string val = line[1].Trim();
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
                            string[] vals = val.Split(',');
                            if (vals.Length == 4)
                            {
                                WindowRect.x = Convert.ToSingle(vals[0].Trim());
                                WindowRect.y = Convert.ToSingle(vals[1].Trim());
                                WindowRect.width = Convert.ToSingle(vals[2].Trim());
                                WindowRect.height = Convert.ToSingle(vals[3].Trim());
                            }
                            else
                                Trace("Ignoring invalid rectangle in settings: '" + lines[i] + "'");
                        }
                        else if (key == "keyToggleWindow")
                            ReadKeyCode(val, ref keyToggleWindow);
                        else if (key == "keyApplyEuler")
                            ReadKeyCode(val, ref keyApplyEuler);
                        else if (key == "keyCycleRotate")
                            ReadKeyCode(val, ref keyCycleRotate);
                        else if (key == "keyCycleFine")
                            ReadKeyCode(val, ref keyCycleFine);
                        else if (key == "keyVeryFineMod")
                            ReadKeyCode(val, ref keyVeryFineMod);
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
            file.WriteLine("windowPos = {0:f},{1:f},{2:f},{3:f}", WindowRect.x, WindowRect.y, WindowRect.width, WindowRect.height);
            file.WriteLine("keyToggleWindow = " + (Int32)keyToggleWindow);
            file.WriteLine("keyApplyEuler = " + (Int32)keyApplyEuler);
            file.WriteLine("keyCycleRotate = " + (Int32)keyCycleRotate);
            file.WriteLine("keyCycleFine = " + (Int32)keyCycleFine);
            file.WriteLine("keyVeryFineMod = " + (Int32)keyVeryFineMod);
            file.WriteLine("useAppLaunch = " + (useAppLaunch ? "true" : "false"));

            file.Close();
        }

        void ReadBool(string val, ref bool variable)
        {
            if (val == "true")
                variable = true;
            else if (val == "false")
                variable = false;
        }

        void ReadKeyCode(string val, ref Int32 variable)
        {
            Int32 keyCode = 0;
            if (Int32.TryParse(val, out keyCode))
                variable = keyCode;
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

            editor = EditorLogic.fetch;
            if (editor == null)
                return;

            if (editor.editorScreen != EditorScreen.Parts)
                return;

            Part part = EditorLogic.SelectedPart;

            // Update our values
            if (part != null)
            {
                selPartUpdate = part;
                attRotationUpdate = part.attRotation;
                eulerAngles = part.attRotation.eulerAngles;
            }
            else
            {
                selPartUpdate = null;
                eulerAngles = Vector3.zero;
            }

            sPitch = eulerAngles.x.ToString("0.00");
            sRoll = eulerAngles.y.ToString("0.00");
            sYaw = eulerAngles.z.ToString("0.00");

            // Key handling
            // Get the state of the shift key and the configured modifier keys
            bool fineTweakKeyPressed = GameSettings.Editor_fineTweak.GetKey();
            bool veryFineTweakKeyPressed = Input.GetKey((KeyCode)keyVeryFineMod);
            bool modKeyPressed = GameSettings.MODIFIER_KEY.GetKey();

            if (editor.EditorConstructionMode == ConstructionMode.Place)
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
                if (modKeyPressed && Input.GetKeyDown((KeyCode)keyToggleWindow))
                {
                    // Toggle the visibility
                    ToggleWindow();
                }
            }
            else if (editor.EditorConstructionMode == ConstructionMode.Place ||
                editor.EditorConstructionMode == ConstructionMode.Rotate)
            {
                if (!Visible)
                    return;

                // Otherwise we apply the relevant angle increments depending on which key was pressed
                // Mod-P: Applies all 3 axes using Euler angles
                if (modKeyPressed && Input.GetKeyDown((KeyCode)keyApplyEuler))
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

        private void HandleCycleKey(Int32 keyCode, bool shiftDown, bool modDown, ref string incValue)
        {
            if (keyCode != (Int32)KeyCode.None && Input.GetKeyDown((KeyCode)keyCode))
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
                WindowRect = GUILayout.Window(WindowID, WindowRect, Window, WindowTitle, windowStyle);
        }

        private void Window(int windowID)
        {
            bool isVAB = HighLogic.LoadedScene == GameScenes.EDITOR;

            GUILayout.BeginVertical(areaStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Pitch", labelStyle);
            GUILayout.Label(eulerAngles.x.ToString("0.00"), dataStyle, gloWidth40);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Roll", labelStyle);
            GUILayout.Label((isVAB ? eulerAngles.y : eulerAngles.z).ToString("0.00"), dataStyle, gloWidth40);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Yaw", labelStyle);
            GUILayout.Label((isVAB ? eulerAngles.z : eulerAngles.y).ToString("0.00"), dataStyle, gloWidth40);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Pitch +/-", labelStyle, gloWidth60);
            if (GUILayout.Button("x", buttonStyle, gloWidth20))
                sIncPitch = "0.0";
            sIncPitch = GUILayout.TextField(sIncPitch, 7, GetDataStyle(sIncPitch));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Roll +/-", labelStyle, gloWidth60);
            if (GUILayout.Button("x", buttonStyle, gloWidth20))
                sIncRoll = "0.0";
            sIncRoll = GUILayout.TextField(sIncRoll, 7, GetDataStyle(sIncRoll));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Yaw +/-", labelStyle, gloWidth60);
            if (GUILayout.Button("x", buttonStyle, gloWidth20))
                sIncYaw = "0.0";
            sIncYaw = GUILayout.TextField(sIncYaw, 7, GetDataStyle(sIncYaw));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Rotation", labelStyle, gloWidth60);
            if (GUILayout.Button("<", buttonStyle, gloWidth20))
                sIncCoarse = IncreaseRotate(sIncCoarse);
            if (GUILayout.Button(">", buttonStyle, gloWidth20))
                sIncCoarse = DecreaseRotate(sIncCoarse);
            sIncCoarse = GUILayout.TextField(sIncCoarse, 7, GetDataStyle(sIncCoarse));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Fine", labelStyle, gloWidth60);
            if (GUILayout.Button("<", buttonStyle, gloWidth20))
                sIncFine = IncreaseRotate(sIncFine);
            if (GUILayout.Button(">", buttonStyle, gloWidth20))
                sIncFine = DecreaseRotate(sIncFine);
            sIncFine = GUILayout.TextField(sIncFine, 7, GetDataStyle(sIncFine));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Part-relative", labelStyle);
            relativeRotate = GUILayout.Toggle(relativeRotate, "", buttonStyle);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

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
            return Single.TryParse(str, out temp) ? dataStyle : badDataStyle;
        }

        private float GetSingleOrZero(String str)
        {
            float temp;
            return Single.TryParse(str, out temp) ? temp : 0f;
        }

        private void CreateUIObjects()
        {
            windowStyle = new GUIStyle(HighLogic.Skin.window);

            areaStyle = new GUIStyle(HighLogic.Skin.textArea);

            labelStyle = new GUIStyle(HighLogic.Skin.label)
            {
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 1, 1)
            };

            dataStyle = new GUIStyle(HighLogic.Skin.label)
            {
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleRight,
                stretchWidth = true,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 1, 1)
            };

            badDataStyle = new GUIStyle(HighLogic.Skin.label)
            {
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleRight,
                stretchWidth = true,
                normal = { textColor = new Color(1.0f, 0.5f, 0.5f) },
                focused = { textColor = new Color(1.0f, 0.5f, 0.5f) },
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 1, 1)
            };

            buttonStyle = new GUIStyle(HighLogic.Skin.button)
            {
                fixedWidth = 20,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(1, 0, 0, 0)
            };

            gloWidth20 = GUILayout.Width(20);
            gloWidth40 = GUILayout.Width(40);
            gloWidth60 = GUILayout.Width(60);
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
