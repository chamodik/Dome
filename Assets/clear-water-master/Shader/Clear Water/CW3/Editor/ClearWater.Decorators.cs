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

		sealed class DefaultStyles
		{
			public static GUIStyle scmStyle;
			public static GUIStyle sectionHeader;
			public static GUIStyle sectionHeaderBox;
            static DefaultStyles()
            {
				scmStyle = new GUIStyle("DropDownButton");
				sectionHeader = new GUIStyle(EditorStyles.miniBoldLabel);
				sectionHeader.padding.left = 24;
				sectionHeader.padding.right = -24;
				sectionHeaderBox = new GUIStyle( GUI.skin.box );
				sectionHeaderBox.alignment = TextAnchor.MiddleLeft;
				sectionHeaderBox.padding.left = 5;
				sectionHeaderBox.padding.right = -5;
				sectionHeaderBox.padding.top = 0;
				sectionHeaderBox.padding.bottom = 0;
			}
		}

		sealed class HeaderExDecorator : MaterialPropertyDrawer
    	{
	        private readonly string header;

	        public HeaderExDecorator(string header)
	        {
	            this.header = header;
	        }

	        // so that we can accept Header(1) and display that as text
	        public HeaderExDecorator(float headerAsNumber)
	        {
	            this.header = headerAsNumber.ToString();
	        }

	        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
	        {
	            return 24f;
	        }

	        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
	        {/*
	            position.y += 8;
	            position = EditorGUI.IndentedRect(position);
	            GUI.Label(position, header, EditorStyles.boldLabel);
*/
            Rect r = position;
				r.x -= 2.0f;
				r.y += 2.0f;
				r.height = 18.0f;
				r.width -= 0.0f;
			GUI.Box(r, EditorGUIUtility.IconContent("d_FilterByType"), DefaultStyles.sectionHeaderBox);
			position.y += 2;
			GUI.Label(position, header, DefaultStyles.sectionHeader);
	        }
    	}

	    sealed class SetKeywordDrawer : MaterialPropertyDrawer
	    {
	        static bool s_drawing;

	        readonly string _keyword;

	        public SetKeywordDrawer() : this(default) { }

	        public SetKeywordDrawer(string keyword)
	        {
	            _keyword = keyword;
	        }

	        public override void Apply(MaterialProperty prop)
	        {
	            if (!string.IsNullOrEmpty(_keyword))
	            {
	                foreach (Material mat in prop.targets)
	                {
	                    if (mat.GetTexture(prop.name) != null)
	                        mat.EnableKeyword(_keyword);
	                    else
	                        mat.DisableKeyword(_keyword);
	                }
	            }
	        }

	        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
	            => 0;

	        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
	        {
	            if (s_drawing)
	            {
	                editor.DefaultShaderProperty(position, prop, label.text);
	            }
	            else if (prop.type == MaterialProperty.PropType.Texture)
	            {
	                var oldLabelWidth = EditorGUIUtility.labelWidth;
	                EditorGUIUtility.labelWidth = 0f;
	                s_drawing = true;
	                try
	                {
	                    EditorGUI.BeginChangeCheck();
	                    {
	                        editor.TextureProperty(prop, label.text);
	                    }
	                    if (EditorGUI.EndChangeCheck())
	                    {
	                        if (!string.IsNullOrEmpty(_keyword))
	                        {
	                            var useTexture = prop.textureValue != null;
	                            foreach (Material mat in prop.targets)
	                            {
	                                if (useTexture)
	                                    mat.EnableKeyword(_keyword);
	                                else
	                                    mat.DisableKeyword(_keyword);
	                            }
	                        }
	                    }
	                }
	                finally
	                {
	                    s_drawing = false;
	                    EditorGUIUtility.labelWidth = oldLabelWidth;
	                }
	            }
	        }
	    }

	// Used for toggling GrabPass
	internal class SetShaderPassToggleDrawer : MaterialPropertyDrawer
	{
		readonly string _passName;
    	readonly string _keyword;

		public SetShaderPassToggleDrawer() : this("Always", default) { }

		public SetShaderPassToggleDrawer(string passName, string keyword)
		{
			_passName = passName;
        	_keyword = keyword;
		}

		static bool IsPropertyTypeSuitable(MaterialProperty prop)
		{
			return prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range;
			// Not present in 2019.4
            // || prop.type == MaterialProperty.PropType.Int;
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			if (!IsPropertyTypeSuitable(prop))
			{
				return EditorGUIUtility.singleLineHeight * 2.5f;
			}
			return base.GetPropertyHeight(prop, label, editor);
		}

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{

			EditorGUI.BeginChangeCheck();

			bool value = (Math.Abs(prop.floatValue) > 0.001f);
			EditorGUI.showMixedValue = prop.hasMixedValue;
			value = EditorGUI.Toggle(position, label, value);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = value ? 1.0f : 0.0f;
				SetShaderPassEnabled(prop, value);
			}
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!IsPropertyTypeSuitable(prop))
				return;

			if (prop.hasMixedValue)
				return;

			SetShaderPassEnabled(prop, (Math.Abs(prop.floatValue) > 0.001f));
		}

		protected virtual void SetShaderPassEnabled(MaterialProperty prop, bool on)
		{
			foreach (Material mat in prop.targets)
			{
				mat.SetShaderPassEnabled(_passName, on);
				if (on)
				{
					mat.EnableKeyword(_keyword);
				}
				else
				{
					mat.DisableKeyword(_keyword);
				}

			}
		}
	}



    	// From momoma's GeneLit, used with permission
    	// https://github.com/momoma-null/GeneLit
	    sealed class SingleLineDrawer : MaterialPropertyDrawer
	    {
	        static bool s_drawing;

	        readonly string _extraPropName;
	        readonly string _keyword;

	        public SingleLineDrawer() : this(default, default) { }

	        public SingleLineDrawer(string extraPropName) : this(extraPropName, default) { }

	        public SingleLineDrawer(string extraPropName, string keyword)
	        {
	            _extraPropName = extraPropName;
	            _keyword = keyword;
	        }

	        public override void Apply(MaterialProperty prop)
	        {
	            if (!string.IsNullOrEmpty(_keyword))
	            {
	                foreach (Material mat in prop.targets)
	                {
	                    if (mat.GetTexture(prop.name) != null)
	                        mat.EnableKeyword(_keyword);
	                    else
	                        mat.DisableKeyword(_keyword);
	                }
	            }
	        }

	        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
	            => 0;

	        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
	        {
	            if (s_drawing)
	            {
	                editor.DefaultShaderProperty(position, prop, label.text);
	            }
	            else if (prop.type == MaterialProperty.PropType.Texture)
	            {
	                var oldLabelWidth = EditorGUIUtility.labelWidth;
	                EditorGUIUtility.labelWidth = 0f;
	                s_drawing = true;
	                try
	                {
	                    EditorGUI.BeginChangeCheck();
	                    if (string.IsNullOrEmpty(_extraPropName))
	                    {
	                        editor.TexturePropertySingleLine(label, prop);
	                    }
	                    else
	                    {
	                        var extraProp = MaterialEditor.GetMaterialProperty(prop.targets, _extraPropName);
	                        if (extraProp.type == MaterialProperty.PropType.Color && (extraProp.flags & MaterialProperty.PropFlags.HDR) > 0)
	                            editor.TexturePropertyWithHDRColor(label, prop, extraProp, false);
	                        else
	                            editor.TexturePropertySingleLine(label, prop, extraProp);
	                    }
	                    if (EditorGUI.EndChangeCheck())
	                    {
	                        if (!string.IsNullOrEmpty(_keyword))
	                        {
	                            var useTexture = prop.textureValue != null;
	                            foreach (Material mat in prop.targets)
	                            {
	                                if (useTexture)
	                                    mat.EnableKeyword(_keyword);
	                                else
	                                    mat.DisableKeyword(_keyword);
	                            }
	                        }
	                    }
	                }
	                finally
	                {
	                    s_drawing = false;
	                    EditorGUIUtility.labelWidth = oldLabelWidth;
	                }
	            }
	        }
	    }

    	sealed class ScaleOffsetDecorator : MaterialPropertyDrawer
    	{
    	    bool _initialized = false;

    	    public ScaleOffsetDecorator() { }

    	    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    	    {
    	        if (!_initialized)
    	        {
    	            prop.ReplacePostDecorator(this);
    	            _initialized = true;
    	        }
    	        return 2f * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    	    }

    	    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    	    {
    	        position.xMin += 15f;
    	        position.y = position.yMax - (2f * EditorGUIUtility.singleLineHeight + 2.5f * EditorGUIUtility.standardVerticalSpacing);
    	        editor.TextureScaleOffsetProperty(position, prop);
    	    }
    	}

        sealed class CW3IfDefDecorator : MaterialPropertyDrawer
    	{
    		readonly string[] _keywords;

    		// 1 Argument: Chains to master
    		public CW3IfDefDecorator(string k1) : this(k1, null, null, null) { }

    		// 2 Arguments: Chains to master
    		public CW3IfDefDecorator(string k1, string k2) : this(k1, k2, null, null) { }

    		// 3 Arguments: Chains to master
    		public CW3IfDefDecorator(string k1, string k2, string k3) : this(k1, k2, k3, null) { }

    		// Master Constructor (4 Arguments)
    		public CW3IfDefDecorator(string k1, string k2, string k3, string k4)
    		{
    			// Collect valid keywords into the array
    			var list = new List<string>();
    			if (!string.IsNullOrEmpty(k1)) list.Add(k1);
    			if (!string.IsNullOrEmpty(k2)) list.Add(k2);
    			if (!string.IsNullOrEmpty(k3)) list.Add(k3);
    			if (!string.IsNullOrEmpty(k4)) list.Add(k4);
    			_keywords = list.ToArray();
    		}

    		private bool IsConditionMet(Material mat)
    		{
    			// OR Logic: If ANY of the keywords are enabled, return true
    			foreach (var keyword in _keywords)
    			{
    				if (mat.IsKeywordEnabled(keyword))
    					return true;
    			}
    			return false;
    		}

    		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    		{
    			var materials = Array.ConvertAll(prop.targets, o => o as Material);

    			// Check first material
    			var conditionMet = IsConditionMet(materials[0]);

    			// If not met, skip.
    			if (!conditionMet)
    			{
    				prop.SkipRemainingDrawers(this);
    				return 0;
    			}

    			// If mixed (some objects have it, some don't), we usually hide to avoid conflicts
    			for (var i = 1; i < materials.Length; ++i)
    			{
    				if (IsConditionMet(materials[i]) != conditionMet)
    				{
    					prop.SkipRemainingDrawers(this);
    					break;
    				}
    			}
    			return 0;
    		}

    		public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    		{
    			var materials = Array.ConvertAll(prop.targets, o => o as Material);
    			var conditionMet = IsConditionMet(materials[0]);

    			if (!conditionMet)
    			{
    				prop.SkipRemainingDrawers(this);
    			}
    			else
    			{
    				for (var i = 1; i < materials.Length; ++i)
    				{
    					if (IsConditionMet(materials[i]) != conditionMet)
    					{
    						prop.SkipRemainingDrawers(this);
    						break;
    					}
    				}
    			}
    		}
    	}

    	sealed class IfNDefDecorator : MaterialPropertyDrawer
    	{
    		readonly string[] _keywords;

    		// 1 Argument: Chains to master
    		public IfNDefDecorator(string k1) : this(k1, null, null, null) { }

    		// 2 Arguments: Chains to master
    		public IfNDefDecorator(string k1, string k2) : this(k1, k2, null, null) { }

    		// 3 Arguments: Chains to master
    		public IfNDefDecorator(string k1, string k2, string k3) : this(k1, k2, k3, null) { }

    		// Master Constructor (4 Arguments)
    		public IfNDefDecorator(string k1, string k2, string k3, string k4)
    		{
    			var list = new List<string>();
    			if (!string.IsNullOrEmpty(k1)) list.Add(k1);
    			if (!string.IsNullOrEmpty(k2)) list.Add(k2);
    			if (!string.IsNullOrEmpty(k3)) list.Add(k3);
    			if (!string.IsNullOrEmpty(k4)) list.Add(k4);
    			_keywords = list.ToArray();
    		}

    		private bool IsConditionMet(Material mat)
    		{
    			foreach (var keyword in _keywords)
    			{
    				if (mat.IsKeywordEnabled(keyword))
    					return true;
    			}
    			return false;
    		}

    		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    		{
    			var materials = Array.ConvertAll(prop.targets, o => o as Material);
    			var conditionMet = IsConditionMet(materials[0]);

    			// IfNDef Logic: If the condition IS met (keyword defined), we SKIP (Hide)
    			if (conditionMet)
    			{
    				prop.SkipRemainingDrawers(this);
    				return 0;
    			}

    			for (var i = 1; i < materials.Length; ++i)
    			{
    				if (IsConditionMet(materials[i]) != conditionMet)
    				{
    					prop.SkipRemainingDrawers(this);
    					break;
    				}
    			}
    			return 0;
    		}

    		public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    		{
    			var materials = Array.ConvertAll(prop.targets, o => o as Material);
    			var conditionMet = IsConditionMet(materials[0]);

    			if (conditionMet)
    			{
    				prop.SkipRemainingDrawers(this);
    			}
    			else
    			{
    				for (var i = 1; i < materials.Length; ++i)
    				{
    					if (IsConditionMet(materials[i]) != conditionMet)
    					{
    						prop.SkipRemainingDrawers(this);
    						break;
    					}
    				}
    			}
    		}
    	}

		sealed class IndentDecorator : MaterialPropertyDrawer
		{
			bool _initialized = false;
			readonly int _indentLevel;

			public IndentDecorator() : this(1) { }

			public IndentDecorator(float indentLevel)
			{
				_indentLevel = (int)indentLevel;
			}

			public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
			{
				if (!_initialized)
				{
					prop.ReplacePostDecorator(new UnindentPostDecorator(_indentLevel));
					_initialized = true;
				}
				return 0f;
			}

			public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
			{
				EditorGUI.indentLevel += _indentLevel;
				// Safely absorb the layout gap injected by MaterialEditor/WrapDrawer
				GUILayout.Space(-EditorGUIUtility.standardVerticalSpacing);
			}
		}

		sealed class UnindentPostDecorator : MaterialPropertyDrawer
		{
			readonly int _indentLevel;

			public UnindentPostDecorator(int indentLevel)
			{
				_indentLevel = indentLevel;
			}

			public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
			{
				return 0f;
			}

			public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
			{
				EditorGUI.indentLevel -= _indentLevel;
				// Safely absorb the layout gap injected by MaterialEditor/WrapDrawer
				GUILayout.Space(-EditorGUIUtility.standardVerticalSpacing);
			}
		}

		sealed class ReciprocalDrawer : MaterialPropertyDrawer
		{
			public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
			{
				return MaterialEditor.GetDefaultPropertyHeight(prop);
			}

			public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
			{
				if (prop.type != MaterialProperty.PropType.Float && prop.type != MaterialProperty.PropType.Range)
				{
					editor.DefaultShaderProperty(position, prop, label.text);
					return;
				}

				EditorGUI.BeginChangeCheck();
				EditorGUI.showMixedValue = prop.hasMixedValue;

				float val = prop.floatValue;
				float displayVal = (Math.Abs(val) > 1e-6f) ? 1.0f / val : 0.0f;

				// Binding the label directly natively makes the text handle draggable
				displayVal = EditorGUI.FloatField(position, label, displayVal);

				EditorGUI.showMixedValue = false;
				if (EditorGUI.EndChangeCheck())
				{
					prop.floatValue = (Math.Abs(displayVal) > 1e-6f) ? 1.0f / displayVal : 0.0f;
				}
			}
		}

		sealed class MultiFloatDrawer : MaterialPropertyDrawer
		{
			readonly string[] _extraProps;

			public MultiFloatDrawer(string prop1) { _extraProps = new[] { prop1 }; }
			public MultiFloatDrawer(string prop1, string prop2) { _extraProps = new[] { prop1, prop2 }; }
			public MultiFloatDrawer(string prop1, string prop2, string prop3) { _extraProps = new[] { prop1, prop2, prop3 }; }

			public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
			{
				return EditorGUIUtility.singleLineHeight;
			}

			public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
			{
				int totalCount = 1 + _extraProps.Length;
				float spacing = 4f;
				float remainingWidth = position.width - EditorGUIUtility.labelWidth;
				float fieldWidth = (remainingWidth - (spacing * (totalCount - 1))) / totalCount;

				// Frame the first rect over the Label and the First Control. 
				// This natively handles the indent and keeps the main text fully draggable!
				Rect firstRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth + fieldWidth, position.height);
				
				EditorGUI.BeginChangeCheck();
				EditorGUI.showMixedValue = prop.hasMixedValue;
				float val0 = EditorGUI.FloatField(firstRect, label, prop.floatValue);
				EditorGUI.showMixedValue = false;
				if (EditorGUI.EndChangeCheck())
				{
					prop.floatValue = val0;
				}

				float currentX = firstRect.xMax + spacing;
				float oldLabelWidth = EditorGUIUtility.labelWidth;
				int oldIndent = EditorGUI.indentLevel;
				
				// Suspend indent shift for sub-fields so they don't double-indent
				EditorGUI.indentLevel = 0; 
				EditorGUIUtility.labelWidth = 14f; 

				for (int i = 0; i < _extraProps.Length; i++)
				{
					var extraProp = MaterialEditor.GetMaterialProperty(prop.targets, _extraProps[i]);
					if (extraProp != null)
					{
						Rect rect = new Rect(currentX, position.y, fieldWidth, position.height);
						
						EditorGUI.BeginChangeCheck();
						EditorGUI.showMixedValue = extraProp.hasMixedValue;
						
						string subLabelStr = string.IsNullOrEmpty(extraProp.displayName) ? "F" : extraProp.displayName.Substring(0, 1);
						GUIContent subLabel = new GUIContent(subLabelStr, extraProp.displayName);

						float val = EditorGUI.FloatField(rect, subLabel, extraProp.floatValue);
						
						EditorGUI.showMixedValue = false;
						if (EditorGUI.EndChangeCheck())
						{
							extraProp.floatValue = val;
						}
					}
					currentX += fieldWidth + spacing;
				}

				// Restore layout styles
				EditorGUIUtility.labelWidth = oldLabelWidth;
				EditorGUI.indentLevel = oldIndent;
			}
		}

		sealed class ColorFloatDrawer : MaterialPropertyDrawer
		{
			readonly string _floatPropName;

			public ColorFloatDrawer(string floatPropName)
			{
				_floatPropName = floatPropName;
			}

			public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
			{
				return EditorGUIUtility.singleLineHeight;
			}

			public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
			{
				var floatProp = MaterialEditor.GetMaterialProperty(prop.targets, _floatPropName);

				// PrefixLabel properly handles the indentLevel for the text, 
				// and gives us the exact absolute space left for the controls.
				Rect controlRect = EditorGUI.PrefixLabel(position, label);

				float spacing = 4f;
				float halfWidth = (controlRect.width - spacing) / 2f;
				
				Rect colorRect = new Rect(controlRect.x, controlRect.y, halfWidth, controlRect.height);
				Rect floatRect = new Rect(colorRect.xMax + spacing, controlRect.y, halfWidth, controlRect.height);

				// Cache and reset indentLevel. 
				// Otherwise, EditorGUI.ColorField sees the indent and shifts itself right twice.
				int oldIndent = EditorGUI.indentLevel;
				float oldLabelWidth = EditorGUIUtility.labelWidth;
				
				EditorGUI.indentLevel = 0;

				if (prop.type == MaterialProperty.PropType.Color)
				{
					EditorGUI.BeginChangeCheck();
					EditorGUI.showMixedValue = prop.hasMixedValue;
					bool isHdr = (prop.flags & MaterialProperty.PropFlags.HDR) != 0;
					
					Color c = EditorGUI.ColorField(colorRect, GUIContent.none, prop.colorValue, true, true, isHdr);
					
					EditorGUI.showMixedValue = false;
					if (EditorGUI.EndChangeCheck())
					{
						prop.colorValue = c;
					}
				}

				if (floatProp != null && (floatProp.type == MaterialProperty.PropType.Float || floatProp.type == MaterialProperty.PropType.Range))
				{
					// Apply local layout for the mini-label so it becomes draggable
					EditorGUIUtility.labelWidth = 14f;

					EditorGUI.BeginChangeCheck();
					EditorGUI.showMixedValue = floatProp.hasMixedValue;
					
					string floatLabelStr = string.IsNullOrEmpty(floatProp.displayName) ? "F" : floatProp.displayName.Substring(0, 1);
					GUIContent floatLabel = new GUIContent(floatLabelStr, floatProp.displayName);

					float f = EditorGUI.FloatField(floatRect, floatLabel, floatProp.floatValue);
					
					EditorGUI.showMixedValue = false;
					if (EditorGUI.EndChangeCheck())
					{
						floatProp.floatValue = f;
					}
				}

				// Restore layout styles
				EditorGUIUtility.labelWidth = oldLabelWidth;
				EditorGUI.indentLevel = oldIndent;
			}
		}

		public class CW3BlendModeSelectorDrawer : MaterialPropertyDrawer
		{
			private readonly string _srcBlend;
			private readonly string _dstBlend;
			private readonly string _customRenderQueue;
			private readonly string _zWrite;
			private readonly string _alphaToMask;

			public CW3BlendModeSelectorDrawer(string srcBlend, string dstBlend, string customRenderQueue, string zWrite = null, string alphaToMask = null)
			{
				_srcBlend = srcBlend;
				_dstBlend = dstBlend;
				_customRenderQueue = customRenderQueue;
				_zWrite = zWrite;
				_alphaToMask = alphaToMask;
			}

			private struct BlendModeData
			{
				public string name;
				public string keyword;
				public BlendMode srcBlend;
				public BlendMode dstBlend;
				public RenderQueue renderQueue;
				public string renderType;

				public BlendModeData(string name, string keyword, BlendMode srcBlend, BlendMode dstBlend, RenderQueue renderQueue, string renderType)
				{
					this.name = name;
					this.keyword = keyword;
					this.srcBlend = srcBlend;
					this.dstBlend = dstBlend;
					this.renderQueue = renderQueue;
					this.renderType = renderType;
				}
			}

			// Modified for Clear Water.
			private static readonly BlendModeData[] blendModes = new BlendModeData[]
			{
				new BlendModeData("Transparent", "", BlendMode.One, BlendMode.OneMinusSrcAlpha, RenderQueue.Transparent, "Transparent"),
				new BlendModeData("Opaque", "", BlendMode.One, BlendMode.Zero, RenderQueue.Geometry, "Opaque"),
			};

			public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
			{
				EditorGUI.BeginChangeCheck();
				int blendMode = (int)prop.floatValue;
				string[] blendModeNames = System.Array.ConvertAll(blendModes, mode => mode.name);
				blendMode = EditorGUI.Popup(position, "Blend Mode", blendMode, blendModeNames);
				if (EditorGUI.EndChangeCheck())
				{
					editor.RegisterPropertyChangeUndo("Blend Mode");
					prop.floatValue = blendMode;
					foreach (var target in prop.targets)
					{
						SetMaterialBlendModeData((Material)target, blendMode);
					}
				}
			}

			private void SetMaterialBlendModeData(Material targetMat, int blendMode)
			{
				// Disable all keywords
				foreach (var mode in blendModes)
				{
					targetMat.DisableKeyword(mode.keyword);
				}

				// Enable the selected blend mode keyword and set the blend mode properties
				var data = blendModes[blendMode];
				targetMat.EnableKeyword(data.keyword);
				targetMat.SetInt(_srcBlend, (int)data.srcBlend);
				targetMat.SetInt(_dstBlend, (int)data.dstBlend);
				targetMat.SetOverrideTag("RenderType", data.renderType);

				// Set ZWrite based on blend mode and render queue
				if (!string.IsNullOrEmpty(_zWrite))
				{
					bool zWrite = (targetMat.renderQueue < 3000);
					targetMat.SetInt(_zWrite, zWrite ? 1 : 0);
				}

				// If the user has overridden the render queue, don't change it
				if (targetMat.HasProperty(_customRenderQueue))
				{
					if (targetMat.renderQueue == -1)
					{
						targetMat.SetInt(_customRenderQueue, -1);
					}
					int renderQueue = targetMat.GetInt(_customRenderQueue);
					targetMat.renderQueue = renderQueue > 1000 ? renderQueue : (int)data.renderQueue;
				}
				else
				{
					targetMat.renderQueue = (int)data.renderQueue;
				}
			}
		}
    }
}
