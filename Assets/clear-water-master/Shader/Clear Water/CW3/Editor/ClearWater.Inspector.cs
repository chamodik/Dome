using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using System.Linq;

namespace SilentClearWater.Unity
{
	public partial class ClearWaterInspector : ShaderGUI
	{
		public class SCWBoot : AssetPostprocessor {
			private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
			string[] movedFromAssetPaths) {
			var isUpdated = importedAssets.Any(path => path.StartsWith("Assets/")) &&
							importedAssets.Any(path => path.Contains("SCW_InspectorData"));

			if (isUpdated) {
				InitializeOnLoad();
			}
			}

			[InitializeOnLoadMethod]
			private static void InitializeOnLoad() {
			ClearWaterInspector.LoadInspectorData();
			}
		}

		static private TextAsset inspectorData;
		public static Dictionary<string, GUIContent> styles = new Dictionary<string, GUIContent>();

		protected MaterialProperty renderQueueOverride;

		protected GUIContent Content(string i)
		{
			GUIContent style;
			if (styles.TryGetValue(i, out style))
			{
				return style;
			} 
			return new GUIContent(i);
		}

		protected Material target;
		protected MaterialEditor editor;
		Dictionary<string, MaterialProperty> props = new Dictionary<string, MaterialProperty>();

		protected MaterialProperty Property(string i)
		{
			MaterialProperty prop;
			if (props.TryGetValue(i, out prop))
			{
				return prop;
			} 
			return new MaterialProperty();
		}

		public static void LoadInspectorData()
		{
			string[] guids = AssetDatabase.FindAssets("t:TextAsset SCW_InspectorData." + Application.systemLanguage);
			if (guids.Length == 0)
			{
				guids = AssetDatabase.FindAssets("t:TextAsset SCW_InspectorData.English");
				if (guids.Length == 0) return;
			}
			inspectorData = (TextAsset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(TextAsset));

			if (inspectorData == null) return;

			styles.Clear(); // Clear existing entries for a clean reload

			// Process the file line-by-line, splitting on both Windows and Unix line endings
			string[] lines = inspectorData.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string line in lines)
			{
				string trimmedLine = line.Trim();
				
				// Skip comments and empty lines
				if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#") || trimmedLine.StartsWith(";"))
				{
					continue;
				}

				// Locate the assignment symbol
				int eqIndex = trimmedLine.IndexOf('=');
				if (eqIndex == -1)
				{
					continue;
				}

				string rawKey = trimmedLine.Substring(0, eqIndex).Trim();
				string value = trimmedLine.Substring(eqIndex + 1).Trim();

				if (rawKey.EndsWith(".tooltip", StringComparison.OrdinalIgnoreCase))
				{
					string baseKey = rawKey.Substring(0, rawKey.Length - 8).Trim(); // Truncate ".tooltip"
					if (styles.TryGetValue(baseKey, out GUIContent content))
					{
						content.tooltip = value;
					}
					else
					{
						styles[baseKey] = new GUIContent(string.Empty, value);
					}
				}
				else
				{
					if (styles.TryGetValue(rawKey, out GUIContent content))
					{
						content.text = value;
					}
					else
					{
						styles[rawKey] = new GUIContent(value);
					}
				}
			}
		}

		protected void FindProperties(MaterialProperty[] matProps, Material material)
		{ 	
			foreach (MaterialProperty prop in matProps)
			{
				props[prop.name] = FindProperty(prop.name, matProps, false);
			}
		}
        
        protected bool initialised;

        protected void Initialise(Material material)
        {
            if (!initialised)
            {
                MaterialChanged(material, true);
                initialised = true;
            }
        }

        protected virtual void MaterialChanged(Material material, bool overrideRenderQueue)
        {
        	// Do nothing
        }	

// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/Inspector/MaterialEditor.cs#L1468
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] matProps)
		{ 
			this.target = materialEditor.target as Material;
			this.editor = materialEditor;
			Material material = this.target;
		
            FindProperties(matProps,material);

			int propertyIndex = 0;
			foreach (MaterialProperty prop in matProps)
			{
				if (!ShaderUtil.IsShaderPropertyHidden(material.shader, propertyIndex)) 
				{
	                if (!styles.ContainsKey(prop.name) ) 
	                {
	                    editor.ShaderProperty(prop, prop.displayName);
	                } else {
						// Ensure label text isn't empty if only a tooltip was defined in the .properties file
						if (string.IsNullOrEmpty(styles[prop.name].text))
						{
							styles[prop.name].text = prop.displayName;
						}
					    ShaderProperty(prop.name);
	                }
				}
				propertyIndex++;
			}
			FooterOptions();
        }

		protected void FooterOptions()
		{
			EditorGUILayout.Space();

			if (WithChangeCheck(() => 
			{
				//editor.ShaderProperty(renderQueueOverride, Content(BaseStyles.renderQueueOverrideName));
				editor.RenderQueueField();
			})) {
				MaterialChanged(target, true);
			}
			editor.EnableInstancingField();
		}
    }
}