/*
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;

namespace See1.See1Shot
{
    public class EditorCoroutine
    {
        //---------------------------------------------------------------------
        // Static
        //---------------------------------------------------------------------

        public static EditorCoroutine Start(IEnumerator _routine)
        {
            EditorCoroutine coroutine = new EditorCoroutine(_routine);
            coroutine.Start();
            return coroutine;
        }

        public static EditorCoroutine StartManual(IEnumerator _routine)
        {
            var coroutine = new EditorCoroutine(_routine);
            return coroutine;
        }

        //---------------------------------------------------------------------
        // Instance
        //---------------------------------------------------------------------

        private readonly IEnumerator _routine;

        private EditorCoroutine(IEnumerator routine)
        {
            _routine = routine;
        }

        protected void Start()
        {
            EditorApplication.update += Update;
        }
        protected void Stop()
        {
            EditorApplication.update -= Update;
        }

        protected void Update()
        {
            if (!_routine.MoveNext()) Stop();
        }
    }

    [Serializable]
    public class CameraConfig
    {
        public Camera camera;
        public string name;
        public float depth;
        public bool enabled = true;

        public CameraConfig(Camera camera)
        {
            this.camera = camera;
            this.name = camera.name;
            this.depth = camera.depth;
        }
    }

    [Serializable]
    public class ImageConfig
    {
        public enum Format
        {
            PNG,
            JPG,
            EXR
        }

        public string name = String.Empty;
        public int width = 0;
        public int height = 0;
        public Format type = Format.PNG;
        public bool enabled = true;

        public ImageConfig(string name, int width, int height, Format type)
        {
            this.name = name;
            this.width = width;
            this.height = height;
            this.type = type;
        }
    }

    [Serializable]
    public class Settings
    {
        private static Settings _instance;

        public static Settings instance
        {
            get
            {
                if (_instance == null) Load();
                return _instance;
            }
            set { _instance = value; }
        }

        //Data to serialize
        public List<CameraConfig> cameraList = new List<CameraConfig>();
        public List<ImageConfig> imageConfigList = new List<ImageConfig>();
        public string saveFolder = "Screenshots";
        public string prefix = string.Empty;
        public string suffix = string.Empty;
        public bool collectToFolder = false;
        public bool preserveAlpha = false;
        public bool showPreview = true;

        public static readonly string DATA_PATH = "Assets/Editor/See1ShotSettings.json";
        public static readonly string KEY = string.Format("{0}.{1}", "com.see1.see1shot.settings", GetProjectName().ToLower());
        public static bool _isDirty;

        public static void Load()
        {
            var setting = new Settings();
            var json = EditorPrefs.GetString(KEY);
            //Debug.Log(json);
            JsonUtility.FromJsonOverwrite(json, setting);
            _instance = setting;
        }

        public static void LoadFromFile()
        {
            var setting = new Settings();

            var dataAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(DATA_PATH);
            string data = string.Empty;
            if (dataAsset)
            {
                data = dataAsset.text;
                JsonUtility.FromJsonOverwrite(data, setting);
                _isDirty = false;
            }
            else
            {
                SetDirty();
            }
            instance = setting;
        }

        public static void Save()
        {
            var json = JsonUtility.ToJson(_instance, false);
            //Debug.Log(json);
            EditorPrefs.SetString(KEY,json);
        }

        public static void SaveToFile()
        {
            var json = JsonUtility.ToJson(_instance, true);
            DirectoryInfo di = new DirectoryInfo(Application.dataPath.Replace("Assets", "") + Path.GetDirectoryName(DATA_PATH));
            if (!di.Exists) di.Create();
            File.WriteAllText(DATA_PATH, json);
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(DATA_PATH));
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        public static void Delete()
        {
            if (EditorPrefs.HasKey(KEY))
            {
                var title = "Removing " + KEY + "?";
                var message = "Are you sure you want to " + "delete the editor key " + KEY + "?, This action cant be undone";
                if (EditorUtility.DisplayDialog(title, message, "Yes", "No")) EditorPrefs.DeleteKey(KEY);
            }
            else
            {
                var title = "Could not find " + KEY;
                var message = "Seems that " + KEY + " does not exists or it has been deleted already, " + "check that you have typed correctly the name of the key.";
                EditorUtility.DisplayDialog(title, message, "Ok");
            }
        }

        public static string GetProjectName()
        {
            string[] s = Application.dataPath.Split('/');
            string projectName = s[s.Length - 2];
            return projectName;
        }

        public static void SetDirty()
        {
            _isDirty = true;
        }

        public static void ConfirmSave()
        {
            if (_isDirty)
            {
                if (EditorUtility.DisplayDialog("", "", "", ""))
                {
                    SaveToFile();
                }
            }
        }
    }

    public class EditorHelper
    {
        public static void IconLabel(Type type, string text, int size = 18)
        {
            GUIContent title = new GUIContent(text, EditorGUIUtility.ObjectContent(null, type).image, text);
            var style = new GUIStyle(EditorStyles.label);
            style.fontSize = size;
            style.normal.textColor = Color.gray * 0.75f;
            style.fontStyle = FontStyle.BoldAndItalic;
            style.alignment = TextAnchor.MiddleLeft;
            style.stretchWidth = true;
            style.stretchHeight = true;
            GUILayout.Label(title, style, GUILayout.Height(size * 2));
        }
        public static void IconLabel(string iconName, string text, int size = 18)
        {
            GUIContent title = new GUIContent(text, EditorGUIUtility.IconContent(iconName).image, text);
            var style = new GUIStyle(EditorStyles.label);
            style.fontSize = size;
            style.normal.textColor = Color.gray * 0.75f;
            style.fontStyle = FontStyle.BoldAndItalic;
            style.alignment = TextAnchor.MiddleLeft;
            style.stretchWidth = true;
            style.stretchHeight = true;
            GUILayout.Label(title, style, GUILayout.Height(size * 2));
        }
    }

    public static class GameViewUtil
    {
        private static readonly object _gameViewSizesInstance;
        private static readonly MethodInfo _getGroup;

        //---------------------------------------------------------------------
        // Constructor
        //---------------------------------------------------------------------

        static GameViewUtil()
        {
            var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instanceProp = singleType.GetProperty("instance");
            _getGroup = sizesType.GetMethod("GetGroup");
            _gameViewSizesInstance = instanceProp.GetValue(null, null);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static void SetSizeByIndex(int index)
        {
            var gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            var gameViewWindow = EditorWindow.GetWindow(gameViewType);
            gameViewType.GetMethod("SizeSelectionCallback", BindingFlags.Public | BindingFlags.Instance).Invoke(gameViewWindow, new object[] { index, null });
        }

        public static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
        {
            var group = GetGroup(sizeGroupType);
            var addCustomSize = _getGroup.ReturnType.GetMethod("AddCustomSize");
            var gameViewSize = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            var gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
            var ctor = gameViewSize.GetConstructor(new Type[] { gameViewSizeType, typeof(int), typeof(int), typeof(string) });
            var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
            addCustomSize.Invoke(group, new object[] { newSize });
        }

        public static void RemoveCustomSize(GameViewSizeGroupType sizeGroupType, int index)
        {
            var group = GetGroup(sizeGroupType);
            var addCustomSize = _getGroup.ReturnType.GetMethod("RemoveCustomSize");
            addCustomSize.Invoke(group, new object[] { index });
        }

        public static int FindSizeIndex(GameViewSizeGroupType sizeGroupType, string text)
        {
            var group = GetGroup(sizeGroupType);
            var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
            var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
            for (var i = 0; i < displayTexts.Length; i++)
            {
                var display = displayTexts[i];
                // the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
                // so if we're querying a custom size text we substring to only get the name
                // You could see the outputs by just logging
                // Debug.Log(display);
                var pren = display.IndexOf('(');
                // -1 to remove the space that's before the prens. This is very implementation-depdenent
                if (pren != -1) display = display.Substring(0, pren - 1);
                if (display == text)
                    return i;
            }
            return -1;
        }

        public static int FindSizeIndex(GameViewSizeGroupType sizeGroupType, int width, int height)
        {
            var group = GetGroup(sizeGroupType);
            var groupType = group.GetType();
            var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
            var getCustomCount = groupType.GetMethod("GetCustomCount");
            var sizesCount = (int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);
            var getGameViewSize = groupType.GetMethod("GetGameViewSize");
            var gvsType = getGameViewSize.ReturnType;
            var widthProp = gvsType.GetProperty("width");
            var heightProp = gvsType.GetProperty("height");
            var indexValue = new object[1];
            for (var i = 0; i < sizesCount; i++)
            {
                indexValue[0] = i;
                var size = getGameViewSize.Invoke(group, indexValue);
                var sizeWidth = (int)widthProp.GetValue(size, null);
                var sizeHeight = (int)heightProp.GetValue(size, null);
                if (sizeWidth == width && sizeHeight == height)
                    return i;
            }
            return -1;
        }

        public static bool IsSizeExist(GameViewSizeGroupType sizeGroupType, string text)
        {
            return FindSizeIndex(sizeGroupType, text) != -1;
        }

        public static bool IsSizeExist(GameViewSizeGroupType sizeGroupType, int width, int height)
        {
            return FindSizeIndex(sizeGroupType, width, height) != -1;
        }

        public static object GetGroup(GameViewSizeGroupType type)
        {
            return _getGroup.Invoke(_gameViewSizesInstance, new object[] { (int)type });
        }

        public static GameViewSizeGroupType GetCurrentGroupType()
        {
            var getCurrentGroupTypeProp = _gameViewSizesInstance.GetType().GetProperty("currentGroupType");
            return (GameViewSizeGroupType)(int)getCurrentGroupTypeProp.GetValue(_gameViewSizesInstance, null);
        }

        public static int GetCurrentSizeIndex()
        {
            var gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            var selectedSizeIndexProp = gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var gameViewWindow = EditorWindow.GetWindow(gameViewType);
            return (int)selectedSizeIndexProp.GetValue(gameViewWindow, null);
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum GameViewSizeType
        {
            AspectRatio,
            FixedResolution
        }
    }

    public static class ScreenshotUtil
    {
        public class CameraSetupScope : IDisposable
        {
            public Camera cam;
            public CameraClearFlags cleafFlags;
            public Color backgroundColor;
            public RenderTexture targetTexture;
            public bool allowMSAA;

            public CameraSetupScope(Camera cam, bool preserveAlpha)
            {
                this.cam = cam;
                this.cleafFlags = cam.clearFlags;
                this.allowMSAA = cam.allowMSAA;
                this.targetTexture = cam.targetTexture;
                cam.clearFlags = (cleafFlags == CameraClearFlags.Skybox && preserveAlpha) ? CameraClearFlags.Color : cam.clearFlags;
                this.backgroundColor = cam.backgroundColor;
                cam.backgroundColor = Color.clear;
                cam.allowMSAA = false;
            }

            public void Dispose()
            {
                cam.targetTexture = targetTexture;
                cam.allowMSAA = allowMSAA;
                cam.clearFlags = cleafFlags;
                cam.backgroundColor = backgroundColor;
            }
        }

        public static Texture2D TakeShot(List<CameraConfig> cameraConfigs, ImageConfig imageConfig, bool preserveAlpha, float multiplier=1)
        {
            var scrTexture = new Texture2D((int)(imageConfig.width * multiplier), (int)(imageConfig.height * multiplier), preserveAlpha ?  TextureFormat.RGBA32 : TextureFormat.RGB24, false);
            var scrRenderTexture = RenderTexture.GetTemporary(scrTexture.width, scrTexture.height, 24, RenderTextureFormat.ARGB32);
            var cameras = cameraConfigs.Where(x => x.enabled).Select(y => y.camera).ToArray();
            for (var i = 0; i < cameras.Length; i++)
            {
                Camera cam = cameras[i];
                if (cam)
                {
                    using (new CameraSetupScope(cam, preserveAlpha))
                    {
                        cam.targetTexture = scrRenderTexture;
                        cam.Render();
                    }
                }
            }

            RenderTexture.active = scrRenderTexture;
            scrTexture.ReadPixels(new Rect(0, 0, scrTexture.width, scrTexture.height), 0, 0);
            scrTexture.Apply();
            return scrTexture;
        }

        //public static void TakeShot(See1ShotSettings settings, ScreenshotConfig config, bool preserveAlpha, float multiplier = 1)
        //{
        //    //var suffix = settings.AppendTimestamp ? "." + DateTime.Now.ToString("yyyyMMddHHmmssfff") : "";
        //    var suffix = settings.AppendTimestamp ? DateTime.Now.ToString("yyyyMMddHHmm") : "";
        //    TakeShot(settings.cameraList, settings.SaveFolder, settings.prefix, suffix, config, preserveAlpha, multiplier);
        //}

        public static void TakeShot(List<CameraConfig> cameraConfigs, ImageConfig imageConfig, string dir, string prefix, string suffix, bool preserveAlpha, float multiplier = 1)
        {
            var shot = TakeShot(cameraConfigs, imageConfig, preserveAlpha, multiplier);
            SaveAsFile(shot, dir, prefix, suffix, imageConfig);
        }

        public static void SaveAsFile(Texture2D texture, string dir, string prefix, string suffix, ImageConfig imageConfig)
        {
            byte[] bytes;
            string extension;

            switch (imageConfig.type)
            {
                case ImageConfig.Format.PNG:
                    bytes = texture.EncodeToPNG();
                    extension = ".png";
                    break;
                case ImageConfig.Format.JPG:
                    bytes = texture.EncodeToJPG();
                    extension = ".jpg";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //var fileName = prefix + screenshotConfig.Name + "." + screenshotConfig.Width + "x" + screenshotConfig.Height + suffix;
            var fileName = prefix + imageConfig.name + "-" + imageConfig.width + "x" + imageConfig.height + suffix;
            //var imageFilePath = folder + "/" + MakeValidFileName(fileName + extension);
            //dir = dir + "/" + suffix;
            var imageFilePath = dir + "/" + MakeValidFileName(fileName + extension);
            imageFilePath = System.IO.File.Exists(imageFilePath) ? dir  + "/" + MakeValidFileName(fileName + "_1" + extension) : imageFilePath;
            // ReSharper disable once PossibleNullReferenceException
            (new FileInfo(imageFilePath)).Directory.Create();
            File.WriteAllBytes(imageFilePath, bytes);
            Debug.Log("Image saved to: " + imageFilePath);
        }

        private static string MakeValidFileName(string name)
        {
            var invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }

    public static class ReorderableHelper
    {
        public class GUIContents
        {
            public static GUIContent add = new GUIContent("Add");
            public static GUIContent refresh = new GUIContent("Refresh");
            public static GUIContent remove = new GUIContent("Remove");
            public static GUIContent sort = new GUIContent("Sort");
        }
        private static readonly string[] _fileTypes = { "PNG", "JPG" };

        public static UnityEditorInternal.ReorderableList CreateCameraList(List<CameraConfig> cameraList, UnityAction onChanged )
        {
            var reorderableList = new UnityEditorInternal.ReorderableList(cameraList, typeof(ImageConfig), true, true, false, false);

            reorderableList.showDefaultBackground = false;
            reorderableList.headerHeight = 20;
            reorderableList.footerHeight = 00;
            reorderableList.elementHeight = EditorGUIUtility.singleLineHeight+4;
            reorderableList.onReorderCallback = (x) =>onChanged();

            reorderableList.drawHeaderCallback = (position) =>
            {
                var btn = position.width * 0.3333333f;
                position.width = btn;

                if (GUI.Button(position, GUIContents.refresh, EditorStyles.miniButtonLeft))
                {
                    //reorderableList.onAddDropdownCallback.Invoke(position, reorderableList);
                    cameraList.Clear();
                    var sceneCameras = UnityEngine.Object.FindObjectsOfType<Camera>().OrderBy((x) => x.depth);
                    foreach (var sceneCamera in sceneCameras)
                    {
                        cameraList.Add(new CameraConfig(sceneCamera));
                    }
                }


                position.x += position.width;
                position.width = btn;
                using (new EditorGUI.DisabledScope(reorderableList.index < 0))
                {
                    if (GUI.Button(position, GUIContents.remove, EditorStyles.miniButtonMid))
                    {
                        reorderableList.onRemoveCallback(reorderableList);
                    }
                }
                position.x += position.width;
                position.width = btn;
                if (GUI.Button(position, GUIContents.sort, EditorStyles.miniButtonRight))
                {
                    cameraList = cameraList.OrderBy((x) => x.depth).ToList();
                }
            };

            reorderableList.drawElementCallback = (position, index, isActive, isFocused) =>
            {
                var enabled = cameraList[index].enabled;
                const float toggleWidth = 16f;
                const float depthWidth = 60f;
                var objectWidth = position.width - toggleWidth - depthWidth;
                var camera = cameraList[index].camera;
                position.width = toggleWidth;
                cameraList[index].enabled = EditorGUI.Toggle(position, cameraList[index].enabled);
                position.x += position.width;
                position.width = objectWidth;
                var objectRect = new RectOffset(2,2,2,2).Remove(position);
                EditorGUI.ObjectField(objectRect, camera, typeof(Camera), false);
                position.x += position.width;
                position.width = depthWidth;
                EditorGUI.LabelField(position,string.Format("Depth : {0}", cameraList[index].depth.ToString()),EditorStyles.miniLabel);
                if(enabled!=cameraList[index].enabled) onChanged.Invoke();

            };

            reorderableList.onRemoveCallback = (list) =>
            {
                reorderableList.index = Mathf.Clamp(reorderableList.index, 0, cameraList.Count - 1);
                if (cameraList.Count > 0)
                {
                    var cam = cameraList[reorderableList.index];
                    cameraList.Remove(cam);
                }

                reorderableList.index = Mathf.Clamp(reorderableList.index, 0, cameraList.Count - 1);
            };

            return reorderableList;
        }
        public static UnityEditorInternal.ReorderableList CreateConfigList(List<ImageConfig> configsList, GenericMenu.MenuFunction2 menuItemHandler)
        {
            var reorderableList = new UnityEditorInternal.ReorderableList(configsList, typeof(ImageConfig), true, true, false, false);

            reorderableList.showDefaultBackground = false;
            reorderableList.headerHeight = 20;
            reorderableList.footerHeight = 00;
            reorderableList.elementHeight = EditorGUIUtility.singleLineHeight+4;

            reorderableList.drawHeaderCallback = (position) =>
            {
                var btn = position.width * 0.5f;
                position.width = btn;

                if (GUI.Button(position, GUIContents.add, EditorStyles.miniButtonLeft))
                {
                    reorderableList.onAddDropdownCallback.Invoke(position, reorderableList);
                }


                position.x += position.width;
                position.width = btn;
                using (new EditorGUI.DisabledScope(reorderableList.index < 0))
                {
                    if (GUI.Button(position, GUIContents.remove, EditorStyles.miniButtonRight))
                    {
                        reorderableList.onRemoveCallback(reorderableList);
                    }
                }
            };

            reorderableList.drawElementCallback = (position, index, isActive, isFocused) =>
            {
                const float toggleWidth = 16f;
                const float textWidth = 12f;
                const float dimensionWidth = 35f;
                const float typeWidth = 40f;
                const float space = 5f;

                var config = configsList[index];
                var nameWidth = position.width - space - textWidth - 2 * dimensionWidth - space - typeWidth - toggleWidth;

                position.width = toggleWidth;
                configsList[index].enabled = EditorGUI.Toggle(position, configsList[index].enabled);

                position.x += position.width;
                position.y += 2;
                position.width = nameWidth;
                position.height -= 4;
                config.name = EditorGUI.TextField(position, config.name);

                position.x += position.width + space;
                position.width = dimensionWidth;
                config.width = EditorGUI.IntField(position, config.width);

                position.x += position.width;
                position.width = textWidth;
                EditorGUI.LabelField(position, "x");

                position.x += position.width;
                position.width = dimensionWidth;
                config.height = EditorGUI.IntField(position, config.height);

                position.x += position.width + space;
                position.width = typeWidth;
                config.type = (ImageConfig.Format)EditorGUI.Popup(position, (int)config.type, _fileTypes);
            };

            reorderableList.onAddDropdownCallback = (buttonRect, list) =>
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Custom"), false, menuItemHandler, new ImageConfig("Custom", 1024, 1024, ImageConfig.Format.PNG));
                menu.AddSeparator("");

                foreach (var config in PredefinedConfigs.Android)
                {
                    var label = "Android/" + config.name + " (" + config.width + "x" + config.height + ")";
                    menu.AddItem(new GUIContent(label), false, menuItemHandler, config);
                }

                foreach (var config in PredefinedConfigs.iOS)
                {
                    var label = "iOS/" + config.name + " (" + config.width + "x" + config.height + ")";
                    menu.AddItem(new GUIContent(label), false, menuItemHandler, config);
                }

                foreach (var config in PredefinedConfigs.Standalone)
                {
                    var label = "Standalone/" + config.name + " (" + config.width + "x" + config.height + ")";
                    menu.AddItem(new GUIContent(label), false, menuItemHandler, config);
                }

                foreach (var config in PredefinedConfigs.POT)
                {
                    var label = "POT/" + config.name + " (" + config.width + "x" + config.height + ")";
                    menu.AddItem(new GUIContent(label), false, menuItemHandler, config);
                }

                menu.ShowAsContext();
            };

            reorderableList.onRemoveCallback = (list) =>
            {
                reorderableList.index = Mathf.Clamp(reorderableList.index, 0, configsList.Count - 1);
                if (configsList.Count > 0)
                {
                    var config = configsList[reorderableList.index];
                    configsList.Remove(config);
                }

               See1Shot.previewIdx = reorderableList.index = Mathf.Clamp(reorderableList.index, 0, configsList.Count - 1);
            };

            return reorderableList;
        }
    }

    public class PredefinedConfigs
    {
        public static ImageConfig[] Android =
        {
            new ImageConfig("Nexus 4 Portrait", 768, 1280, ImageConfig.Format.PNG),
            new ImageConfig("Nexus 4 Landscape", 1280, 768, ImageConfig.Format.PNG),

            new ImageConfig("Nexus 5 Portrait", 1080, 1920, ImageConfig.Format.PNG),
            new ImageConfig("Nexus 5 Landscape", 1920, 1080, ImageConfig.Format.PNG),

            new ImageConfig("Nexus 6 Portrait", 1440, 2560, ImageConfig.Format.PNG),
            new ImageConfig("Nexus 6 Landscape", 2560, 1440, ImageConfig.Format.PNG),

            new ImageConfig("Nexus 7 Portrait", 800, 1280, ImageConfig.Format.PNG),
            new ImageConfig("Nexus 7 Landscape", 1280, 800, ImageConfig.Format.PNG),

            new ImageConfig("Nexus 7 (2013) Portrait", 1200, 1920, ImageConfig.Format.PNG),
            new ImageConfig("Nexus 7 (2013) Landscape", 1920, 1200, ImageConfig.Format.PNG),

            new ImageConfig("Nexus 10 Portrait", 1600, 2560, ImageConfig.Format.PNG),
            new ImageConfig("Nexus 10 Landscape", 2560, 1600, ImageConfig.Format.PNG),
        };

        public static ImageConfig[] iOS =
        {
            new ImageConfig("iPhone 3.5-Inch Portrait", 640, 960, ImageConfig.Format.PNG),
            new ImageConfig("iPhone 3.5-Inch Landscape", 960, 640, ImageConfig.Format.PNG),

            new ImageConfig("iPhone 4-Inch Portrait", 640, 1136, ImageConfig.Format.PNG),
            new ImageConfig("iPhone 4-Inch Landscape", 1136, 640, ImageConfig.Format.PNG),

            new ImageConfig("iPhone 4.7-Inch Portrait", 750, 1334, ImageConfig.Format.PNG),
            new ImageConfig("iPhone 4.7-Inch Landscape", 1334, 750, ImageConfig.Format.PNG),

            new ImageConfig("iPhone 5.5-Inch Portrait", 1242, 2208, ImageConfig.Format.PNG),
            new ImageConfig("iPhone 5.5-Inch Landscape", 2208, 1242, ImageConfig.Format.PNG),

            new ImageConfig("iPad Portrait", 768, 1024, ImageConfig.Format.PNG),
            new ImageConfig("iPad Landscape", 1024, 768, ImageConfig.Format.PNG),

            new ImageConfig("iPad Hi-Res Portrait", 1536, 2048, ImageConfig.Format.PNG),
            new ImageConfig("iPad Hi-Res Landscape", 2048, 1536, ImageConfig.Format.PNG),

            new ImageConfig("iPad Pro Portrait", 2048, 2732, ImageConfig.Format.PNG),
            new ImageConfig("iPad Pro Landscape", 2732, 2048, ImageConfig.Format.PNG)
        };

        public static ImageConfig[] Standalone =
        {
            new ImageConfig("XGA", 1024, 768, ImageConfig.Format.PNG),
            new ImageConfig("SXGA", 1280, 1024, ImageConfig.Format.PNG),
            new ImageConfig("WXGA", 1280, 800, ImageConfig.Format.PNG),
            new ImageConfig("WXGA+", 1440, 900, ImageConfig.Format.PNG),
            new ImageConfig("WSXGA+", 1680, 1050, ImageConfig.Format.PNG),
            new ImageConfig("HD", 1366, 768, ImageConfig.Format.PNG),
            new ImageConfig("HD+", 1600, 900, ImageConfig.Format.PNG),
            new ImageConfig("Full HD", 1920, 1080, ImageConfig.Format.PNG),
            new ImageConfig("Quad HD", 2560, 1440, ImageConfig.Format.PNG),
            new ImageConfig("4K UHD", 3840, 2160, ImageConfig.Format.PNG)
        };

        public static ImageConfig[] POT =
        {
            new ImageConfig("128", 128, 128, ImageConfig.Format.PNG),
            new ImageConfig("256", 256, 256, ImageConfig.Format.PNG),
            new ImageConfig("512", 512, 512, ImageConfig.Format.PNG),
            new ImageConfig("1024", 1024, 1024, ImageConfig.Format.PNG),
            new ImageConfig("2048", 2048, 2048, ImageConfig.Format.PNG),
            new ImageConfig("4096", 4096, 4096, ImageConfig.Format.PNG),
            new ImageConfig("8192", 8192, 8192, ImageConfig.Format.PNG),
        };
    }

    public class See1Shot : EditorWindow
    {
        public class GUIContents
        {
            public static GUIContent load = new GUIContent("Load from file", string.Format("Load from {0}", Settings.DATA_PATH));
            public static GUIContent save = new GUIContent("Save to file", string.Format("Save to {0}", Settings.DATA_PATH));
            public static string configError = "하나 이상의 카메라와 이미지 설정이 필요합니다.";
            public static string pathError = "폴더 경로가 비어있거나 잘못된 문자가 포함되어 있습니다.";
            public static GUIContent preview = new GUIContent("Preview", "프리뷰 보기");
            public static GUIContent showSettings = new GUIContent("Settings","기타 설정");
            public static GUIContent collect = new GUIContent("Collect to time stamp folder", "해당 시간으로 이름지어진 폴더 안에 만들어진 스크린샷을 저장합니다");
            public static GUIContent alpha = new GUIContent("Alpha", "활성화하면 빈 픽셀이나 스카이박스를 강제로 채우지 않고 알파채널이 살아있는 이미지를 만듭니다.");
        }

        private static Settings settings { get { return  Settings.instance;} }
        private UnityEditorInternal.ReorderableList _cameraList;
        private UnityEditorInternal.ReorderableList _imageConfigList;
        private static Texture2D preview;
        private static bool _isProcessing;
        private static bool _canProcess;
        private static AnimBool _showPreview;
        private static AnimBool _showSettings;
        private static string tempStr = string.Empty;
        private static GUIStyle _buttonStyle;
        public static int previewIdx = 0;

        protected void OnEnable()
        {
            _cameraList = _cameraList ?? ReorderableHelper.CreateCameraList(settings.cameraList, () => RefreshPreview());
            _imageConfigList = _imageConfigList ?? ReorderableHelper.CreateConfigList(settings.imageConfigList, MenuItemHandler);
            _showPreview = new AnimBool(() => { Repaint(); });
            _showPreview.target = settings.showPreview;
            _showSettings = new AnimBool(Repaint);
            EditorApplication.hierarchyChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= Refresh;
            _showSettings.target = false;
            DestroyImmediate(preview);
            Settings.Save();
        }

        private void OnFocus()
        {
            Refresh();
        }

        void RefreshCamera()
        {
            if (IsCameraChanged())
            {
                settings.cameraList.Clear();
                var sceneCameras = FindObjectsOfType<Camera>().OrderBy((x) => x.depth);
                foreach (var sceneCamera in sceneCameras)
                {
                    settings.cameraList.Add(new CameraConfig(sceneCamera));
                }
            }
        }

        void RefreshPreview()
        {
            if (settings.imageConfigList.Count < 1) return;
            previewIdx = Mathf.Clamp(previewIdx, 0, settings.imageConfigList.Count - 1);
            var config = settings.imageConfigList[previewIdx];
            var scale = position.width / config.width;
            preview = ScreenshotUtil.TakeShot(settings.cameraList, config, settings.preserveAlpha, scale);
        }

        void Refresh()
        {
            RefreshCamera();
            RefreshPreview();
        }

        bool IsCameraChanged()
        {
            var sceneCameras = FindObjectsOfType<Camera>().OrderBy((x) => x.depth);
            var exceptA = settings.cameraList.Select(x=>x.camera).Except(sceneCameras).ToList();
            var exceptB = sceneCameras.Except(settings.cameraList.Select(x => x.camera)).ToList();
            return exceptA.Count>0 || exceptB.Count>0;
        }

        protected void OnGUI()
        {
            using (new EditorGUI.DisabledScope(_isProcessing))
            {
                using (new EditorGUILayout.HorizontalScope("Toolbar"))
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        _showPreview.target = settings.showPreview = GUILayout.Toggle(settings.showPreview, GUIContents.preview, EditorStyles.toolbarButton);
                        if (check.changed)
                        {
                            if (!IsDocked() && preview)
                            {
                                RefreshPreview();
                            }
                        }
                        //GUILayout.Label(_targetSize.ToString());
                        GUILayout.FlexibleSpace();
                        _showSettings.target = GUILayout.Toggle(_showSettings.target, GUIContents.showSettings, EditorStyles.toolbarButton);
                    }
                }

                using (var faded = new EditorGUILayout.FadeGroupScope(_showSettings.faded))
                {
                    if (faded.visible)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button(GUIContents.load, EditorStyles.miniButtonLeft))
                            {
                                Settings.LoadFromFile();
                                _cameraList = ReorderableHelper.CreateCameraList(settings.cameraList, () => RefreshPreview());
                                _imageConfigList = ReorderableHelper.CreateConfigList(settings.imageConfigList, MenuItemHandler);
                                RefreshPreview();
                                GUIUtility.ExitGUI();
                            }

                            if (GUILayout.Button(GUIContents.save, EditorStyles.miniButtonRight))
                            {
                                Settings.SaveToFile();
                            }
                        }
                    }
                }

                using (var faded = new EditorGUILayout.FadeGroupScope(_showPreview.faded))
                {
                    if (faded.visible)
                    {
                        if (preview)
                        {
                            float aspect = (float)preview.width / preview.height;
                            var rect = GUILayoutUtility.GetAspectRect(aspect);
                            rect = new RectOffset(5, 5, 5, 0).Remove(rect);
                            EditorGUI.DrawPreviewTexture(rect, preview);
                            if (settings.imageConfigList.Count > 0)
                            {
                                var left = new RectOffset(0, (int)rect.width - ((int)rect.width / 8), 0, 0).Remove(rect);
                                var right = new RectOffset((int)rect.width - ((int)rect.width / 8), 0, 0, 0).Remove(rect);
                                var style = "RL FooterButton";
                                EditorGUI.DropShadowLabel(left, "◀", style);
                                if (GUI.Button(left, "◀", style))
                                {
                                    previewIdx -= 1;
                                    previewIdx = Mathf.Clamp(previewIdx, 0, settings.imageConfigList.Count - 1);
                                    RefreshPreview();
                                }

                                EditorGUI.DropShadowLabel(right, "▶", style);
                                if (GUI.Button(right, "▶", style))
                                {
                                    previewIdx += 1;
                                    previewIdx = Mathf.Clamp(previewIdx, 0, settings.imageConfigList.Count - 1);
                                    RefreshPreview();
                                }

                                EditorGUI.DropShadowLabel(rect, string.Format("{0}", settings.imageConfigList[previewIdx].name), style);
                            }
                        }
                    }
                }

                EditorHelper.IconLabel(typeof(Camera), "Camera");

                _cameraList.DoLayoutList();

                var enabledCamCount = settings.cameraList.Count(x => x.enabled);
                var enabledConfigCount = settings.imageConfigList.Count(x => x.enabled);
                _canProcess = (enabledCamCount > 0 && enabledConfigCount > 0);
                if (!_canProcess)
                {
                    EditorGUILayout.HelpBox(GUIContents.configError, MessageType.Error);
                }

                EditorHelper.IconLabel(typeof(Texture), "Image");

                _imageConfigList.DoLayoutList();

                EditorHelper.IconLabel("Folder Icon", "Save");
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 55;
                using (new EditorGUILayout.HorizontalScope())
                {
                    settings.saveFolder = EditorGUILayout.TextField("Project /", settings.saveFolder);

                    if (GUILayout.Button("Browse", GUILayout.Width(60)))
                    {
                        var selectedPath = EditorUtility.SaveFolderPanel("Save screenshots to:", settings.saveFolder, string.Empty);
                        var projectFolder = Application.dataPath.Replace("Assets", "");
                        if (selectedPath.Contains(projectFolder))
                        {
                            settings.saveFolder = selectedPath.Replace(projectFolder, "");
                        }
                    }
                }

                settings.prefix = EditorGUILayout.TextField("Prefix", settings.prefix);
                settings.suffix = EditorGUILayout.TextField("Suffix", settings.suffix);
                tempStr = string.Format("Project/{0}/{1}{2}Name-Size{3}.Ext", settings.saveFolder, settings.collectToFolder ? "TimeStamp/" : string.Empty, settings.prefix, settings.suffix);
                EditorGUILayout.HelpBox(tempStr, MessageType.None);

                using (new EditorGUILayout.HorizontalScope())
                {
                    settings.collectToFolder = GUILayout.Toggle(settings.collectToFolder, GUIContents.collect, EditorStyles.miniButton, GUILayout.Height(20));
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        settings.preserveAlpha = GUILayout.Toggle(settings.preserveAlpha, GUIContents.alpha, EditorStyles.miniButton, GUILayout.Height(20));
                        if (check.changed)
                        {
                            RefreshPreview();
                        }
                    }
                }

                if (string.IsNullOrEmpty(settings.saveFolder) || settings.saveFolder.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                {
                    EditorGUILayout.HelpBox(GUIContents.pathError, MessageType.Error);
                    _canProcess = false;
                }

                EditorGUILayout.Space();
                EditorGUIUtility.labelWidth = labelWidth;

                GUI.enabled &= Directory.Exists(settings.saveFolder);
                if (GUILayout.Button("Open Saved Folder", GUILayout.Height(30)))
                {
                    Application.OpenURL("file://" + Path.GetFullPath(settings.saveFolder));
                }

                using (new EditorGUI.DisabledScope(!_canProcess))
                {
                    GUI.backgroundColor = Color.red;
                    if (_buttonStyle == null)
                    {
                        _buttonStyle = new GUIStyle("LargeButton");
                        _buttonStyle.fontSize = 24;
                        _buttonStyle.fontStyle = FontStyle.BoldAndItalic;
                    }


                    if (GUILayout.Button("Shot", _buttonStyle, GUILayout.Height(60)))
                    {
                        EditorCoroutine.Start(Shot());
                    }
                    //var rect = GUILayoutUtility.GetLastRect();
                    //Style.InnerShadowLabel(rect, new GUIContent("Shot"), _buttonStyle);

                    GUI.backgroundColor = Color.white;
                }
                using (var faded = new EditorGUILayout.FadeGroupScope(_showSettings.faded))
                {
                    if (faded.visible)
                    {
                        if (GUILayout.Button("Created by See1Studios.", EditorStyles.centeredGreyMiniLabel))
                        {
                            Application.OpenURL(string.Format("https://assetstore.unity.com/publishers/42929"));
                        }
                    }
                }
            }
        }

        class Style
        {
            public static void DropShadowLabel(Rect position, GUIContent content)
            {
                DoDropShadowLabel(position, content, (GUIStyle) "PreOverlayLabel", 0.6f);
            }
            public static void InnerShadowLabel(Rect position, GUIContent content)
            {
                DoInnerShadowLabel(position, content, (GUIStyle)"PreOverlayLabel", 0.6f);
            }

            public static void DropShadowLabel(Rect position, GUIContent content, GUIStyle style)
            {
                DoDropShadowLabel(position, content, style, 0.6f);
            }
            public static void InnerShadowLabel(Rect position, GUIContent content, GUIStyle style)
            {
                DoInnerShadowLabel(position, content, style, 0.6f);
            }

            private static void DoInnerShadowLabel(Rect position, GUIContent content, GUIStyle style, float shadowOpa)
            {
                if (Event.current.type != EventType.Repaint)
                    return;
                DrawLabelInnerShadow(position, content, style, shadowOpa);
                style.Draw(position, content, false, false, false, false);
            }

            private static void DoDropShadowLabel(Rect position, GUIContent content, GUIStyle style, float shadowOpa)
            {
                if (Event.current.type != EventType.Repaint)
                    return;
                DrawLabelShadow(position, content, style, shadowOpa);
                style.Draw(position, content, false, false, false, false);
            }

            private static void DrawLabelShadow(Rect position, GUIContent content, GUIStyle style, float shadowOpa)
            {
                Color color = GUI.color;
                Color contentColor = GUI.contentColor;
                Color backgroundColor = GUI.backgroundColor;
                GUI.contentColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                style.Draw(position, content, false, false, false, false);
                ++position.y;
                GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                GUI.contentColor = contentColor;
                Draw4(position, content, 1f, GUI.color.a * shadowOpa, style);
                Draw4(position, content, 2f, (float)((double)GUI.color.a * (double)shadowOpa * 0.419999986886978), style);
                GUI.color = color;
                GUI.backgroundColor = backgroundColor;
            }

            private static void DrawLabelInnerShadow(Rect position, GUIContent content, GUIStyle style, float shadowOpa)
            {
                Color color = GUI.color;
                Color contentColor = GUI.contentColor;
                Color backgroundColor = GUI.backgroundColor;
                GUI.contentColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                style.Draw(position, content, false, false, false, false);
                --position.y;
                GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                GUI.contentColor = contentColor;
                Draw4(position, content, -1f, GUI.color.a * shadowOpa, style);
                Draw4(position, content, -2f, (float) ((double) GUI.color.a * (double) shadowOpa * 0.419999986886978), style);
                GUI.color = color;
                GUI.backgroundColor = backgroundColor;
            }

            private static void Draw4(Rect position, GUIContent content, float offset, float alpha, GUIStyle style)
            {
                GUI.color = new Color(0.0f, 0.0f, 0.0f, alpha);
                position.y -= offset;
                style.Draw(position, content, false, false, false, false);
                position.y += offset * 2f;
                style.Draw(position, content, false, false, false, false);
                position.y -= offset;
                position.x -= offset;
                style.Draw(position, content, false, false, false, false);
                position.x += offset * 2f;
                style.Draw(position, content, false, false, false, false);
            }
        }


        private static IEnumerator Shot()
        {
            _isProcessing = true;
            var currentIndex = GameViewUtil.GetCurrentSizeIndex();

            // Slow down and unpause editor if required
            var paused = EditorApplication.isPaused;
            var timeScale = Time.timeScale;
            var timeStamp = settings.collectToFolder ? DateTime.Now.ToString("yyyyMMddHHmmss") : "";      
            var dir = settings.saveFolder + "/" + timeStamp;
            try
            {
                EditorApplication.isPaused = false;
                Time.timeScale = 0.001f;

                var list = settings.imageConfigList.Where(x => x.enabled).ToList();
                for (var i = 0; i < list.Count; i++)
                {
                    var data = list[i];

                    var info = (i + 1) + " / " + list.Count + " - " + data.name;
                    EditorUtility.DisplayProgressBar("Taking Screenshots", info, (float) (i + 1)/ list.Count);

                    var sizeType = GameViewUtil.GameViewSizeType.FixedResolution;
                    var sizeGroupType = GameViewUtil.GetCurrentGroupType();
                    var sizeName = "temp_" + data.width + "x" + data.height;

                    if (!GameViewUtil.IsSizeExist(sizeGroupType, sizeName))
                    {
                        GameViewUtil.AddCustomSize(sizeType, sizeGroupType, data.width, data.height, sizeName);
                    }

                    var index = GameViewUtil.FindSizeIndex(sizeGroupType, sizeName);
                    GameViewUtil.SetSizeByIndex(index);

                    // add some delay while applying changes
                    var lastFrameTime = EditorApplication.timeSinceStartup;
                    while (EditorApplication.timeSinceStartup - lastFrameTime < 0.1f) yield return null;

                    ScreenshotUtil.TakeShot(settings.cameraList, data, dir, settings.prefix, settings.suffix, settings.preserveAlpha);

                    // just clean it up
                    GameViewUtil.RemoveCustomSize(sizeGroupType, index);
                }                
            }
            finally
            {
                // Restore pause state and time scale
                EditorApplication.isPaused = paused;
                Time.timeScale = timeScale;

                GameViewUtil.SetSizeByIndex(currentIndex);
                EditorUtility.ClearProgressBar();

                _isProcessing = false;
            }                        
        }
        
        private void MenuItemHandler(object target)
        {
            settings.imageConfigList.Add(target as ImageConfig);
            RefreshPreview();
        }

        bool IsDocked()
        {
            BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);
            if (isDockedMethod != null)
            {
                return (bool)isDockedMethod.Invoke(this, null);
            }
            return false;
        }

        [MenuItem("Tools/See1Studios/See1Shot/Show Window")]
        protected static void ShowWindow()
        {
            var window = (See1Shot)GetWindow(typeof(See1Shot));
            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent("See1Shot", EditorGUIUtility.IconContent("d_SceneViewFx").image, "See1Shot");
            window.position = new Rect(0,0, 100, 100);
            window.minSize = new Vector2(250,250);
            window.Show();
        }

        [MenuItem("Tools/See1Studios/See1Shot/Take Screenshots &#s")]
        private static void TakeScreenshotOnHotkey()
        {
            if(!_isProcessing)
                EditorCoroutine.Start(Shot());
        }
    }
}