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

namespace PartAngleDisplay
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class EditorWindow : MonoBehaviour
    {
        Int32 WindowID;
        String WindowTitle;
        Rect WindowRect;
        ApplicationLauncherButton buttonAppLaunch;
        IButton buttonToolbar;
        EditorLogic editor;
        GUIStyle windowStyle;
        GUIStyle areaStyle;
        GUIStyle labelStyle;
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

        String sIncCoarse = "90.0";
        String sIncFine = "5.0";
        bool startVisible = false;
        bool relativeRotate = false;
        bool absoluteAngles = false;
        bool useAppLaunch = true;

        Int32 keyToggleWindow = (Int32)KeyCode.P;
        Int32 keyApplyEuler = (Int32)KeyCode.P;
        Int32 keyCycleRotate = (Int32)KeyCode.G;
        Int32 keyCycleFine = (Int32)KeyCode.F;

        static float[] angleCycle = { 0.01f, 0.1f, 1, 5, 10, 15, 30, 45, 60, 72, 90, 120 };
        static Texture2D texAppLaunch;

        const string configFilename = "settings.cfg";
        const string pathToolbarDisabled = "PartAngleDisplay/toolbaroff";
        const string pathToolbarEnabled = "PartAngleDisplay/toolbaron";

        private Boolean _Visible = false;

        public EditorWindow()
        {
            //Trace("EditorWindow.EditorWindow");
        }

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

        private void ToggleWindow()
        {
            Visible = !Visible;
        }

        public void Awake()
        {
            //Trace("EditorWindow.Awake");

            editor = EditorLogic.fetch;

            InitStyles();

            WindowTitle = "Part Angle Display (0.2.4.5)";
            WindowRect = new Rect(300, 200, 200, 50);
            WindowID = Guid.NewGuid().GetHashCode();

            LoadConfig();

            if (useAppLaunch)
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

            if (ToolbarManager.ToolbarAvailable)
            {
                buttonToolbar = ToolbarManager.Instance.add("PAD", "button");
                buttonToolbar.ToolTip = "Part Angle Display";
                SetToolbarState();
                buttonToolbar.OnClick += e => ToggleWindow();
                buttonToolbar.Visible = true;
            }
        }

        public void Start()
        {
            //Trace("EditorWindow.Start");

            Visible = startVisible;
        }

        void OnDestroy()
        {
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

        public void Update()
        {
            SetAppLaunchState();
            SetToolbarState();
            
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

            // Get the state of the shift key and the configured modifier key
            bool shiftKeyPressed = Input.GetKey(KeyCode.LeftShift);
            bool altKeyPressed = GameSettings.MODIFIER_KEY.GetKey();
            //Trace("shift = " + shiftKeyPressed + "  alt = " + altKeyPressed);

            // Key handling
            // G/Shift-G/Alt-G  decrease/increase/reset rotation angle
            HandleCycleKey(keyCycleRotate, shiftKeyPressed, altKeyPressed, ref sIncCoarse);
            
            // F/Shift-F/Alt-F  decrease/increase/reset shift-rotation angle
            HandleCycleKey(keyCycleFine, shiftKeyPressed, altKeyPressed, ref sIncFine);

            // When no part is selected:
            // ALT-P            toggle the visible state of the window
            if (!editor.PartSelected)
            {
                if (altKeyPressed && Input.GetKeyDown((KeyCode)keyToggleWindow))
                {
                    // Toggle the visibility
                    ToggleWindow();
                }
            }
            else
            {
                // Otherwise we apply the relevant angle increments depending on which key was pressed
                // ALT-P: Applies all 3 axes using Euler angles
                if (altKeyPressed && Input.GetKeyDown((KeyCode)keyApplyEuler))
                {
                    //Trace("Applying part rotation");
                    Vector3 incAngles;
                    incAngles.x = GetSingleOrZero(sIncPitch);
                    incAngles.y = GetSingleOrZero(sIncRoll);
                    incAngles.z = GetSingleOrZero(sIncYaw);
                    editor.partRotation = Quaternion.Euler(eulerAngles + incAngles);
                }

                // WASDQE           Undo core rotation of 90 and apply our rotation of sPlainRotate
                // Shift-WASDQE     Undo core rotation of 5 and apply our rotation of sShiftRotate
                // Mod-WASDQE       Undo core rotation of 90 and apply our rotation of sIncPitch/Yaw/Roll
                float incPitch = 0f;
                float incYaw = 0f;
                float incRoll = 0f;
                float relPitch = 0f;
                float relYaw = 0f;
                float relRoll = 0f;
                if (GameSettings.Editor_pitchDown.GetKeyDown())
                {
                    if (relativeRotate)
                    {
                        incPitch = shiftKeyPressed ? 5f : -90f;
                        relPitch = shiftKeyPressed ? -GetSingleOrZero(sIncFine) : -(altKeyPressed ? GetSingleOrZero(sIncPitch) : GetSingleOrZero(sIncCoarse));
                    }
                    else
                    {
                        incPitch = shiftKeyPressed ? 5f - GetSingleOrZero(sIncFine) : -90f - (altKeyPressed ? GetSingleOrZero(sIncPitch) : GetSingleOrZero(sIncCoarse));
                    }
                }
                else if (GameSettings.Editor_pitchUp.GetKeyDown())
                {
                    if (relativeRotate)
                    {
                        incPitch = shiftKeyPressed ? -5f : 90f;
                        relPitch = shiftKeyPressed ? GetSingleOrZero(sIncFine) : (altKeyPressed ? GetSingleOrZero(sIncPitch) : GetSingleOrZero(sIncCoarse));
                    }
                    else
                    {
                        incPitch = shiftKeyPressed ? GetSingleOrZero(sIncFine) - 5f : 90f + (altKeyPressed ? GetSingleOrZero(sIncPitch) : GetSingleOrZero(sIncCoarse));
                    }
                }
                else if (GameSettings.Editor_yawLeft.GetKeyDown())
                {
                    if (relativeRotate)
                    {
                        incYaw = shiftKeyPressed ? -5f : -90f;
                        relYaw = shiftKeyPressed ? GetSingleOrZero(sIncFine) : (altKeyPressed ? GetSingleOrZero(sIncYaw) : GetSingleOrZero(sIncCoarse));
                    }
                    else
                    {
                        incYaw = shiftKeyPressed ? GetSingleOrZero(sIncFine) - 5f : -90f + (altKeyPressed ? GetSingleOrZero(sIncYaw) : GetSingleOrZero(sIncCoarse));
                    }
                }
                else if (GameSettings.Editor_yawRight.GetKeyDown())
                {
                    if (relativeRotate)
                    {
                        incYaw = shiftKeyPressed ? 5f : 90f;
                        relYaw = shiftKeyPressed ? -GetSingleOrZero(sIncFine) : -(altKeyPressed ? GetSingleOrZero(sIncYaw) : GetSingleOrZero(sIncCoarse));
                    }
                    else
                    {
                        incYaw = shiftKeyPressed ? 5f - GetSingleOrZero(sIncFine) : 90f - (altKeyPressed ? GetSingleOrZero(sIncYaw) : GetSingleOrZero(sIncCoarse));
                    }
                }
                else if (GameSettings.Editor_rollLeft.GetKeyDown())
                {
                    if (relativeRotate)
                    {
                        incRoll = shiftKeyPressed ? -5f : -90f;
                        relRoll = shiftKeyPressed ? GetSingleOrZero(sIncFine) : (altKeyPressed ? GetSingleOrZero(sIncRoll) : GetSingleOrZero(sIncCoarse));
                    }
                    else
                    {
                        incRoll = shiftKeyPressed ? GetSingleOrZero(sIncFine) - 5f : -90f + (altKeyPressed ? GetSingleOrZero(sIncRoll) : GetSingleOrZero(sIncCoarse));
                    }
                }
                else if (GameSettings.Editor_rollRight.GetKeyDown())
                {
                    if (relativeRotate)
                    {
                        incRoll = shiftKeyPressed ? 5f : 90f;
                        relRoll = shiftKeyPressed ? -GetSingleOrZero(sIncFine) : -(altKeyPressed ? GetSingleOrZero(sIncRoll) : GetSingleOrZero(sIncCoarse));
                    }
                    else
                    {
                        incRoll = shiftKeyPressed ? 5f - GetSingleOrZero(sIncFine) : 90f - (altKeyPressed ? GetSingleOrZero(sIncRoll) : GetSingleOrZero(sIncCoarse));
                    }
                }

                ApplyIncrements(incPitch, incYaw, incRoll);
                ApplyRelativeIncrements(relPitch, relYaw, relRoll);
            }
        }

        private void ApplyIncrements(float incPitch, float incYaw, float incRoll)
        {
            bool isVAB = HighLogic.LoadedScene == GameScenes.EDITOR;
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

        private void ApplyRelativeIncrements(float relPitch, float relYaw, float relRoll)
        {
            Part part = EditorLogic.SelectedPart;
            if (part == null)
                return;

            if (relPitch != 0f)
            {
                //Trace("Applying rel pitch of " + relPitch);
                Quaternion qPitch = Quaternion.AngleAxis(-relPitch, part.transform.right);
                //Trace("quaternion = " + qPitch.ToString());
                editor.partRotation = qPitch * editor.partRotation;
            }
            if (relYaw != 0f)
            {
                //Trace("Applying rel yaw of " + relYaw);
                Quaternion qYaw = Quaternion.AngleAxis(relYaw, part.transform.forward);
                //Trace("quaternion = " + qYaw.ToString());
                editor.partRotation = qYaw * editor.partRotation;
            }
            if (relRoll != 0f)
            {
                //Trace("Applying roll of " + relRoll);
                Quaternion qRoll = Quaternion.AngleAxis(relRoll, part.transform.up);
                //Trace("quaternion = " + qRoll.ToString());
                editor.partRotation = qRoll * editor.partRotation;
            }
        }

        private void HandleCycleKey(Int32 keyCode, bool shiftDown, bool altDown, ref string incValue)
        {
            if (keyCode != (Int32)KeyCode.None && Input.GetKeyDown((KeyCode)keyCode))
            {
                if (altDown)
                    incValue = "5.0";
                else if (shiftDown)
                    incValue = IncreaseRotate(incValue);
                else
                    incValue = DecreaseRotate(incValue);
            }
        }

        private void DoPostDraw()
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
            GUILayout.Label(eulerAngles.x.ToString("0.00"), dataStyle, GUILayout.Width(40));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Roll", labelStyle);
            GUILayout.Label((isVAB ? eulerAngles.y : eulerAngles.z).ToString("0.00"), dataStyle, GUILayout.Width(40));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Yaw", labelStyle);
            GUILayout.Label((isVAB ? eulerAngles.z : eulerAngles.y).ToString("0.00"), dataStyle, GUILayout.Width(40));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pitch +/-", labelStyle, GUILayout.Width(60));
            if (GUILayout.Button("x", buttonStyle, GUILayout.Width(20)))
                sIncPitch = "0.0";
            sIncPitch = GUILayout.TextField(sIncPitch, 7, GetDataStyle(sIncPitch));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Roll +/-", labelStyle, GUILayout.Width(60));
            if (GUILayout.Button("x", buttonStyle, GUILayout.Width(20)))
                sIncRoll = "0.0";
            sIncRoll = GUILayout.TextField(sIncRoll, 7, GetDataStyle(sIncRoll));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Yaw +/-", labelStyle, GUILayout.Width(60));
            if (GUILayout.Button("x", buttonStyle, GUILayout.Width(20)))
                sIncYaw = "0.0";
            sIncYaw = GUILayout.TextField(sIncYaw, 7, GetDataStyle(sIncYaw));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Rotation", labelStyle, GUILayout.Width(60));
            if (GUILayout.Button("<", buttonStyle, GUILayout.Width(20)))
                sIncCoarse = IncreaseRotate(sIncCoarse);
            if (GUILayout.Button(">", buttonStyle, GUILayout.Width(20)))
                sIncCoarse = DecreaseRotate(sIncCoarse);
            sIncCoarse = GUILayout.TextField(sIncCoarse, 7, GetDataStyle(sIncCoarse));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Fine", labelStyle, GUILayout.Width(60));
            if (GUILayout.Button("<", buttonStyle, GUILayout.Width(20)))
                sIncFine = IncreaseRotate(sIncFine);
            if (GUILayout.Button(">", buttonStyle, GUILayout.Width(20)))
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

        private void InitStyles()
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
        }

        private void SetAppLaunchState()
        {
            if (buttonAppLaunch != null)
            {
                if (_Visible && buttonAppLaunch.State == RUIToggleButton.ButtonState.FALSE)
                    buttonAppLaunch.SetTrue(false);
                else if (!_Visible && buttonAppLaunch.State == RUIToggleButton.ButtonState.TRUE)
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
            print("[PAD] " + message);
        }
    }
}
