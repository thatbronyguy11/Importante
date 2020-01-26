using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace Malee.Editor
{
	public class ReorderableList
	{
		private static int selectionHash = "ReorderableListSelection".GetHashCode();
		private static int dragAndDropHash = "ReorderableListDragAndDrop".GetHashCode();

		public enum ElementDisplayType
		{
			Auto,
			Expandable,
			SingleLine
		}

		public delegate void DrawDelegate(Rect rect);
		public delegate void DrawElementDelegate(Rect rect, SerializedProperty element, GUIContent label, bool selected, bool focused);
		public delegate void ActionDelegate(ReorderableList list);
		public delegate bool ActionBoolDelegate(ReorderableList list);
		public delegate void AddDropdownDelegate(Rect buttonRect, ReorderableList list);
		public delegate Object DragDropReferenceDelegate(Object[] references);
		public delegate void DragDropAppendDelegate(Object reference);
		public delegate float GetElementHeightDelegate(SerializedProperty element);
		public delegate string GetElementNameDelegate(SerializedProperty element);

		public DrawDelegate drawHeaderCallback;
		public DrawDelegate drawFooterCallback;
		public DrawElementDelegate drawElementCallback;
		public DrawElementDelegate drawElementBackgroundCallback;
		public GetElementHeightDelegate getElementHeightCallback;
		public GetElementNameDelegate getElementNameCallback;
		public DragDropReferenceDelegate onValidateDragAndDropCallback;
		public DragDropAppendDelegate onAppendDragDropCallback;
		public ActionDelegate onReorderCallback;
		public ActionDelegate onSelectCallback;
		public ActionDelegate onAddCallback;
		public AddDropdownDelegate onAddDropdownCallback;
		public ActionDelegate onRemoveCallback;
		public ActionDelegate onMouseUpCallback;
		public ActionBoolDelegate onCanRemoveCallback;
		public ActionDelegate onChangedCallback;

		public bool canAdd;
		public bool canRemove;
		public bool draggable;
		public bool expandable;
		public bool multipleSelection;
		public GUIContent label;
		public float headerHeight;
		public float footerHeight;
		public float slideEasing;
		public bool showDefaultBackground;
		public ElementDisplayType elementDisplayType;
		public string elementNameProperty;
		public Texture elementIcon;

		internal readonly int id;

		private SerializedProperty list;
		private int controlID = -1;
		private Rect[] elementRects;
		private GUIContent elementLabel;
		private ListSelection selection;
		private SlideGroup slideGroup;
		private int pressIndex;

		private bool dragging;
		private bool dragMoved;
		private float dragPosition;
		private List<DragElement> dragList;
		private ListSelection beforeDragSelection;

		private int dragDropControlID = -1;
		private MethodInfo dragDropValidation;
		private object[] dragDropValidationParams;
		private MethodInfo appendDragDrop;
		private object[] appendDragDropParams;

		public ReorderableList(SerializedProperty list)
		: this(list, true, true, true) {
		}

		public ReorderableList(SerializedProperty list, bool canAdd, bool canRemove, bool draggable)
		: this(list, canAdd, canRemove, draggable, ElementDisplayType.Auto, null, null) {
		}

		public ReorderableList(SerializedProperty list, bool canAdd, bool canRemove, bool draggable, ElementDisplayType elementDisplayType, string elementNameProperty, Texture elementIcon) {

			//Throw an error when there is no list
			if (list == null) throw new MissingListExeption();

			//Throw an error when the list is invalid
			else if (!list.isArray) throw new InvalidListException();

			list.isExpanded = true; //Whether or not the list is expanded by default

			this.list = list;
			this.label = new GUIContent(list.displayName);

			this.canAdd = canAdd; //Whether or not the user can add elements to the list
			this.canRemove = canRemove; //Whether or not the user can remove elements from the list
			this.draggable = draggable; //Whether or not the user can drag the elemtns

			this.elementDisplayType = elementDisplayType;
			this.elementNameProperty = elementNameProperty;
			this.elementIcon = elementIcon;

			headerHeight = 18f; //Adds space above the first element block
			footerHeight = 32f; //Adds space under the last element block
			slideEasing = 0.15f; //How smooth the elements get dragged around
			expandable = true; //Whether or not the whole list can be expanded
			showDefaultBackground = true; //Whether or not to show a greyish background behind the list elements
			multipleSelection = true; //Whether or not the user can select multiple list elements at the same time

			id = GetHashCode();// EditorGUIUtil.GetPermanentControlID();
			elementLabel = new GUIContent();
			selection = new ListSelection();
			slideGroup = new SlideGroup();
			elementRects = new Rect[0];

			dragDropValidation = System.Type.GetType("UnityEditor.EditorGUI, UnityEditor").GetMethod("ValidateObjectFieldAssignment", BindingFlags.NonPublic | BindingFlags.Static);
			dragDropValidationParams = new object[3];

			appendDragDrop = list.GetType().GetMethod("AppendFoldoutPPtrValue", BindingFlags.NonPublic | BindingFlags.Instance);
			appendDragDropParams = new object[1];
		}

		//
		// -- PROPERTIES --
		//

		public SerializedProperty List
		{
			get { return list; }
			internal set { list = value; }
		}

		//Boolean to check if there is a list
		public bool HasList
		{
			get { return list != null && list.isArray; }
		}

		//The length of the array
		public int Length
		{
			get { return HasList ? list.arraySize : 0; }
		}

		public int[] Selected {

			get { return selection.ToArray(); }
			set { selection = new ListSelection(value); }
		}

		public int Index {

			get { return selection.First; }
			set { selection.Select(value); }
		}

		//
		// -- PUBLIC --
		//

		public float GetListHeight() {

			if (HasList) {

				return list.isExpanded ? headerHeight + GetElementsHeight() + footerHeight : headerHeight;
			}
			else {

				return EditorGUIUtility.singleLineHeight;
			}
		}

		public void DoLayoutList()
		{
			Rect position = EditorGUILayout.GetControlRect(false, GetListHeight(), EditorStyles.largeLabel);
			DoList(EditorGUI.IndentedRect(position), label);
		}

		public void DoList(Rect rect, GUIContent label)
		{
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0; //The indentation level of the elements

			Rect headerRect = rect;
			headerRect.height = headerHeight;

			controlID = GUIUtility.GetControlID(selectionHash, FocusType.Keyboard, rect);
			dragDropControlID = GUIUtility.GetControlID(dragAndDropHash, FocusType.Passive, rect);

			if (!HasList) DrawEmpty(headerRect, label.text + " is not an Array!", GUIStyle.none, EditorStyles.helpBox);

			else
			{
				DrawHeader(headerRect, label);

				if (list.isExpanded)
				{
					Rect elementBackgroundRect = rect;
					elementBackgroundRect.yMin = headerRect.yMax;
					elementBackgroundRect.yMax = rect.yMax - footerHeight;

					Event evt = Event.current;

					if (selection.Length > 1)
					{
						if (evt.type == EventType.ContextClick && CanSelect(evt.mousePosition)) HandleMultipleContextClick(evt);
					}

					if (list.arraySize > 0)
					{
						UpdateElementRects(elementBackgroundRect, evt);
						HandleDraggingAndSelection(elementBackgroundRect, evt);
						DrawElements(elementBackgroundRect, evt);
					}
					else DrawEmpty(elementBackgroundRect, "No rotations added", Style.boxBackground, Style.verticalLabel);

					//The footer rect
					Rect footerRect = rect;
					footerRect.yMin = elementBackgroundRect.yMax;
					footerRect.xMin = rect.xMax - 87;

					DrawFooter(footerRect);

                    //if (GUILayout.Button(Styles.HelpIcon, Styles.help))
                    //    EditorWindow.GetWindow(typeof(SupportWindow));
                }
			}

			EditorGUI.indentLevel = indent;
		}

		public SerializedProperty AddItem<T>(T item) where T : Object {

			SerializedProperty property = AddItem();

			if (property != null) {

				property.objectReferenceValue = item;
			}

			return property;
		}

		public SerializedProperty AddItem() {

			if (HasList) {

				list.arraySize++;
				selection.Select(list.arraySize - 1);

				DispatchChange();

				return list.GetArrayElementAtIndex(selection.Last);
			}
			else {

				throw new InvalidListException();
			}
		}

		public void Remove(int[] selection) {

			System.Array.Sort(selection);

			int i = selection.Length;

			while (--i > -1) {

				RemoveItem(selection[i]);
			}
		}

		public void RemoveItem(int index) {

			if (HasList && index >= 0 && index < list.arraySize) {

				SerializedProperty property = list.GetArrayElementAtIndex(index);

				if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue) {

					property.objectReferenceValue = null;
				}

				list.DeleteArrayElementAtIndex(index);
				selection.Remove(index);

				if (list.arraySize > 0) {

					selection.Select(Mathf.Max(0, index - 1));
				}

				DispatchChange();
			}
		}

		public SerializedProperty GetItem(int index)
		{

			if (HasList && index >= 0 && index < list.arraySize) {

				return list.GetArrayElementAtIndex(index);
			}
			else {

				return null;
			}
		}

		public int IndexOf(SerializedProperty element) {

			if (element != null) {

				int i = list.arraySize;

				while (--i > -1) {

					if (SerializedProperty.EqualContents(element, list.GetArrayElementAtIndex(i))) {

						return i;
					}
				}
			}

			return -1;
		}

		public void GrabKeyboardFocus() {

			GUIUtility.keyboardControl = id;
		}

		public bool HasKeyboardControl() {

			return GUIUtility.keyboardControl == id;
		}

		public void ReleaseKeyboardFocus() {

			if (GUIUtility.keyboardControl == id) {

				GUIUtility.keyboardControl = 0;
			}
		}

		//
		// -- PRIVATE --
		//

		private float GetElementsHeight() {

			int i, len = list.arraySize;

			if (len == 0) {

				return 25;
			}

			float minHeight = 10;
			float totalHeight = 15;

			for (i = 0; i < len; i++) {

				totalHeight += GetElementHeight(list.GetArrayElementAtIndex(i));
			}

			return Mathf.Max(minHeight, totalHeight);
		}

		private float GetElementHeight(SerializedProperty element) {

			if (getElementHeightCallback != null) {

				return getElementHeightCallback(element) + 6;
			}
			else {

				return EditorGUI.GetPropertyHeight(element, GUIContent.none, true) + 6;
			}
		}

		private Rect GetElementDrawRect(int index, Rect desiredRect) {

			if (slideEasing <= 0) {

				return desiredRect;
			}
			else {

				//lerp the drag easing toward slide easing, this creates a stronger easing at the start then slower at the end
				//when dealing with large lists, we can

				return dragging ? slideGroup.GetRect(dragList[index].startIndex, desiredRect, slideEasing) : slideGroup.SetRect(index, desiredRect);
			}
		}

		private Rect GetElementHeaderRect(SerializedProperty element, Rect elementRect) {

			Rect rect = elementRect;
			rect.height = EditorGUIUtility.singleLineHeight + 2;

			return rect;
		}

		private Rect GetElementRenderRect(SerializedProperty element, Rect elementRect) {

			float offset = draggable ? 20 : 5;

			Rect rect = elementRect;
			rect.xMin += IsElementExpandable(element) ? offset + 10 : offset;
			rect.xMax -= 5;
			rect.yMin += 1;
			rect.yMax -= 1;

			return rect;
		}

		private void DrawHeader(Rect rect, GUIContent label) {

			if (showDefaultBackground && Event.current.type == EventType.Repaint) {

				Style.headerBackground.Draw(rect, false, false, false, false);
			}

			HandleDragAndDrop(rect, Event.current);

			Rect titleRect = rect;
			titleRect.xMin += 6f;
			titleRect.xMax -= 55f;
			titleRect.height -= 2f;
			titleRect.y++;

			EditorGUI.BeginProperty(titleRect, label, list);

			if (drawHeaderCallback != null) {

				drawHeaderCallback(titleRect);
			}
			else if (expandable) {

				titleRect.xMin += 10;

				EditorGUI.BeginChangeCheck();

				bool isExpanded = EditorGUI.Foldout(titleRect, list.isExpanded, label, true);

				if (EditorGUI.EndChangeCheck()) {

					list.isExpanded = isExpanded;
				}
			}
			else {

				GUI.Label(titleRect, label, EditorStyles.label);
			}

			EditorGUI.EndProperty();

			if (elementDisplayType != ElementDisplayType.SingleLine) {

				Rect bRect1 = rect;
				bRect1.xMin = rect.xMax - 25;
				bRect1.xMax = rect.xMax - 5;

				if (GUI.Button(bRect1, Style.expandButton, Style.preButton)) {

					ExpandElements(true);
				}

				Rect bRect2 = rect;
				bRect2.xMin = bRect1.xMin - 20;
				bRect2.xMax = bRect1.xMin;

				if (GUI.Button(bRect2, Style.collapseButton, Style.preButton)) {

					ExpandElements(false);
				}
			}
		}

		private void ExpandElements(bool expand) {

			if (!list.isExpanded && expand) {

				list.isExpanded = true;
			}

			for (int i = 0; i < list.arraySize; i++) {

				list.GetArrayElementAtIndex(i).isExpanded = expand;
			}
		}

		private void DrawEmpty(Rect rect, string label, GUIStyle backgroundStyle, GUIStyle labelStyle) {

			if (showDefaultBackground && Event.current.type == EventType.Repaint) {

				backgroundStyle.Draw(rect, false, false, false, false);
			}

			EditorGUI.LabelField(rect, label, labelStyle);
		}

		private void UpdateElementRects(Rect rect, Event evt) {

			//resize array if elements changed

			int i, len = dragging ? dragList.Count : list.arraySize;

			if (len != elementRects.Length) {

				System.Array.Resize(ref elementRects, len);
			}

			if (evt.type == EventType.Repaint) {

				//start rect

				Rect elementRect = rect;
				elementRect.yMin = elementRect.yMax = rect.yMin + 5;

				for (i = 0; i < len; i++) {

					//if dragging, use the element in the temporary drag list as this list will re-order as the user drags an item

					SerializedProperty element = dragging ? dragList[i].element : list.GetArrayElementAtIndex(i);

					//update the elementRects value for this object. Grab the last elementRect for startPosition

					elementRect.y = elementRect.yMax;
					elementRect.height = GetElementHeight(element);
					elementRects[i] = elementRect;
				}
			}
		}

		private void DrawElements(Rect rect, Event evt) {

			//draw list background

			if (showDefaultBackground && evt.type == EventType.Repaint) {

				Style.boxBackground.Draw(rect, false, false, false, false);
			}

			//only draw dragging elements when repainting

			if (dragging && evt.type != EventType.Repaint) {

				return;
			}

			//draw elements

			int i = dragging ? dragList.Count : list.arraySize;

			while (--i > -1) {

				//if dragging, use the element in the temporary drag list as this list will re-order as the user drags an item

				SerializedProperty element = dragging ? dragList[i].element : list.GetArrayElementAtIndex(i);

				//don't draw the any dragging elements here

				bool isSelected = selection.Contains(i);

				if (dragging && isSelected) {

					continue;
				}

				bool selected = !dragging ? isSelected : false;

				DrawElement(element, GetElementDrawRect(i, elementRects[i]), selected, selected && GUIUtility.keyboardControl == controlID);
			}

			//draw dragging element last, above other items

			if (dragging) {

				//because elementRects are static rects representing indexed positions, make a copy here and move it based on the drag position

				i = selection.Length;

				while (--i > -1) {

					int index = selection[i];

					Rect dragRect = elementRects[index];
					dragRect.y = dragPosition - dragList[index].dragOffset;

					
					
					DrawElement(dragList[index].element, dragRect, true, true);
					
					/*
						EditorGUI.PropertyField(renderRect,element.FindPropertyRelative("RotationType"),label,true);
				EditorGUI.PropertyField(renderRect,element.FindPropertyRelative("InitialAngle"),label,true);
				EditorGUI.PropertyField(renderRect,element.FindPropertyRelative("FinalAngle"),label,true);
				EditorGUI.PropertyField(renderRect,element.FindPropertyRelative("TestBool"),label,true);
				EditorGUI.PropertyField(renderRect,element.FindPropertyRelative("CurveType"),label,true);

				if (element.FindPropertyRelative("TestBool").boolValue == true)
				{
					EditorGUI.PropertyField(renderRect,element.FindPropertyRelative("RotationCurve"),label,true);
				}
					*/
					//TODO: I have to change some code here, so instead of looping throught the list, it specifcially draws my elements how I want to
					
					
				}
			}
		}

		private void DrawElement(SerializedProperty element, Rect rect, bool selected, bool focused) {

			Event evt = Event.current;

			if (drawElementBackgroundCallback != null) {

				drawElementBackgroundCallback(rect, element, null, selected, focused);
			}
			else if (evt.type == EventType.Repaint) {

				Style.elementBackground.Draw(rect, false, selected, selected, focused);
			}

			if (evt.type == EventType.Repaint && draggable) {

				Style.draggingHandle.Draw(new Rect(rect.x + 5, rect.y + 6, 10, rect.height - (rect.height - 6)), false, false, false, false);
			}

			GUIContent label = GetElementLabel(element);

			Rect renderRect = GetElementRenderRect(element, rect);

			if (drawElementCallback != null) {

				drawElementCallback(renderRect, element, label, selected, focused);
			}
			else {

				
		EditorGUI.PropertyField(renderRect, element, label, true); 
			
			

				


			}

			//handle context click

			int controlId = GUIUtility.GetControlID(label, FocusType.Passive, rect);

			switch (evt.GetTypeForControl(controlId)) {

				case EventType.ContextClick:

				if (rect.Contains(evt.mousePosition)) {

					HandleContextClick(evt, element);
				}

				break;
			}
		}

		private GUIContent GetPropertyLabel(SerializedProperty property) {

			elementLabel.text = property.displayName;
			elementLabel.tooltip = property.tooltip;
			elementLabel.image = null;

			return elementLabel;
		}

		private GUIContent GetElementLabel(SerializedProperty element) {

			string name;

			if (getElementNameCallback != null) {

				name = getElementNameCallback(element);
			}
			else {

				name = GetElementName(element, elementNameProperty);
			}

			elementLabel.text = !string.IsNullOrEmpty(name) ? name : element.displayName;
			elementLabel.tooltip = element.tooltip;
			elementLabel.image = elementIcon;

			return elementLabel;
		}

		private string GetElementName(SerializedProperty element, string nameProperty) {

			if (string.IsNullOrEmpty(nameProperty)) {

				return null;
			}
			else if (element.propertyType == SerializedPropertyType.ObjectReference && nameProperty == "name") {

				return element.objectReferenceValue ? element.objectReferenceValue.name : null;
			}

			SerializedProperty prop = element.FindPropertyRelative(nameProperty);

			if (prop != null) {

				switch (prop.propertyType) {

					case SerializedPropertyType.ObjectReference:

					return prop.objectReferenceValue ? prop.objectReferenceValue.name : null;

					case SerializedPropertyType.Enum:

					return prop.enumDisplayNames[prop.enumValueIndex];

					case SerializedPropertyType.Integer:
					case SerializedPropertyType.Character:

					return prop.intValue.ToString();

					case SerializedPropertyType.LayerMask:

					return GetLayerMaskName(prop.intValue);

					case SerializedPropertyType.String:

					return prop.stringValue;

					case SerializedPropertyType.Float:

					return prop.floatValue.ToString();
				}

				return prop.displayName;
			}

			return null;
		}

		private static string GetLayerMaskName(int mask) {

			if (mask == 0) {

				return "Nothing";
			}
			else if (mask < 0) {

				return "Everything";
			}

			string name = string.Empty;
			int n = 0;

			for (int i = 0; i < 32; i++) {

				if (((1 << i) & mask) != 0) {

					if (n == 4) {

						return "Mixed ...";
					}

					name += (n > 0 ? ", " : string.Empty) + LayerMask.LayerToName(i);
					n++;
				}
			}

			return name;
		}

		private void DrawFooter(Rect rect) {

			if (drawFooterCallback != null) {

				drawFooterCallback(rect);
				return;
			}

			if (Event.current.type == EventType.Repaint) {

				Style.footerBackground.Draw(rect, false, false, false, false);
			}


			Rect AddRect = new Rect(rect.xMin + 10f, rect.y - 1f, 25f, 13f);
			Rect RemoveSelectedRect = new Rect(rect.xMax - 50f, rect.y - 1f, 25f, 13f);
			Rect RemoveAllRect = new Rect(rect.xMax - 23f, rect.y - 1f, 25f, 13f);

			EditorGUI.BeginDisabledGroup(!canAdd);

			if (GUI.Button(AddRect, onAddDropdownCallback != null ? Style.iconToolbarAddMore : Style.iconToolbarAdd, Style.preButton))
			{

				if (onAddDropdownCallback != null) {

					onAddDropdownCallback(AddRect, this);
				}
				else if (onAddCallback != null) {

					onAddCallback(this);
				}
				else {

					AddItem();
				}
			}

			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!CanSelect(selection) || !canRemove || (onCanRemoveCallback != null && !onCanRemoveCallback(this)));

			if (GUI.Button(RemoveSelectedRect, Style.iconToolbarRemoveSelected, Style.preButton))
			{

				if (onRemoveCallback != null) {

					onRemoveCallback(this);
				}
				else {

					Remove(selection.ToArray());
				}
			}

			EditorGUI.EndDisabledGroup();


			EditorGUI.BeginDisabledGroup(!CanSelect(selection) || !canRemove || (onCanRemoveCallback != null && !onCanRemoveCallback(this)));

			if (GUI.Button(RemoveAllRect, Style.iconToolbarRemoveAll, Style.preButton))
			{

				if (onRemoveCallback != null) {

					onRemoveCallback(this);
				}
				else {

					list.ClearArray();
				}
			}

			EditorGUI.EndDisabledGroup();
        }

		private void DispatchChange() {

			if (onChangedCallback != null) {

				onChangedCallback(this);
			}
		}

		private void HandleContextClick(Event evt, SerializedProperty element) {

			selection.Select(IndexOf(element));

			GenericMenu menu = new GenericMenu();

			if (element.isInstantiatedPrefab) {

				menu.AddItem(new GUIContent("Revert Value to Prefab"), false, selection.RevertValues, list);
				menu.AddSeparator(string.Empty);
			}

			menu.AddItem(new GUIContent("Duplicate Array Element"), false, selection.Duplicate, list);
			menu.AddItem(new GUIContent("Delete Array Element"), false, selection.Delete, list);
			menu.ShowAsContext();

			evt.Use();
		}

		private void HandleMultipleContextClick(Event evt) {

			GenericMenu menu = new GenericMenu();

			if (selection.CanRevert(list)) {

				menu.AddItem(new GUIContent("Revert Values to Prefab"), false, selection.RevertValues, list);
				menu.AddSeparator(string.Empty);
			}

			menu.AddItem(new GUIContent("Duplicate Array Elements"), false, selection.Duplicate, list);
			menu.AddItem(new GUIContent("Delete Array Elements"), false, selection.Delete, list);
			menu.ShowAsContext();

			evt.Use();
		}

		private void HandleDragAndDrop(Rect rect, Event evt) {

			switch (evt.GetTypeForControl(dragDropControlID)) {

				case EventType.DragUpdated:
				case EventType.DragPerform:

				if (GUI.enabled && rect.Contains(evt.mousePosition)) {

					Object[] objectReferences = DragAndDrop.objectReferences;
					Object[] references = new Object[1];

					bool acceptDrag = false;

					foreach (Object object1 in objectReferences) {

						references[0] = object1;
						Object object2 = ValidateObjectDragAndDrop(references);

						if (object2 != null) {

							DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

							if (evt.type == EventType.DragPerform) {

								if (onAppendDragDropCallback != null) {

									onAppendDragDropCallback(object2);
								}
								else if (list.propertyType == SerializedPropertyType.ObjectReference) {

									AppendDragAndDropValue(object2);
								}

								acceptDrag = true;
								DragAndDrop.activeControlID = 0;
							}
							else {

								DragAndDrop.activeControlID = dragDropControlID;
							}
						}
					}

					if (acceptDrag) {

						GUI.changed = true;
						DragAndDrop.AcceptDrag();
					}
				}

				break;

				case EventType.DragExited:

				if (GUI.enabled) {

					HandleUtility.Repaint();
				}

				break;
			}
		}

		private Object ValidateObjectDragAndDrop(Object[] references) {

			if (onValidateDragAndDropCallback != null) {

				return onValidateDragAndDropCallback(references);
			}

			dragDropValidationParams[0] = references;
			dragDropValidationParams[1] = null;
			dragDropValidationParams[2] = list;

			return dragDropValidation.Invoke(null, dragDropValidationParams) as Object;
		}

		private void AppendDragAndDropValue(Object obj) {

			appendDragDropParams[0] = obj;
			appendDragDrop.Invoke(list, appendDragDropParams);

			DispatchChange();
		}

		private void HandleDraggingAndSelection(Rect rect, Event evt) {

			int len = elementRects.Length;

			if (len <= 0) {

				return;
			}

			rect.yMin = elementRects[0].yMin;
			rect.yMax = elementRects[len - 1].yMax;

			switch (evt.GetTypeForControl(controlID)) {

				case EventType.MouseDown:

				if (rect.Contains(evt.mousePosition) && evt.button == 0) {

					int index = GetSelectionIndex(evt.mousePosition);

					if (CanSelect(index)) {

						//append selections based on action, this may be a additive (ctrl) or range (shift) selection

						if (multipleSelection) {

							selection.AppendWithAction(pressIndex = index, evt);
						}
						else {

							selection.Select(pressIndex = index);
						}

						if (onSelectCallback != null) {

							onSelectCallback(this);
						}

						if (draggable) {

							dragMoved = false;
							dragPosition = evt.mousePosition.y;
							dragList = GetDragList(dragPosition);

							beforeDragSelection = selection.Clone();

							GUIUtility.hotControl = controlID;
						}
					}
					else {

						selection.Clear();
					}

					GUIUtility.keyboardControl = controlID;

					SerializedProperty element = list.GetArrayElementAtIndex(index);

					if (IsElementExpandable(element)) {

						Rect elementHeaderRect = GetElementHeaderRect(element, elementRects[index]);
						Rect elementRenderRect = GetElementRenderRect(element, elementRects[index]);

						Rect elementExpandRect = elementHeaderRect;
						elementExpandRect.xMin = elementRenderRect.xMin - 10;
						elementExpandRect.xMax = elementRenderRect.xMin;

						if (elementHeaderRect.Contains(evt.mousePosition) && !elementExpandRect.Contains(evt.mousePosition)) {

							evt.Use();
						}
					}

					HandleUtility.Repaint();
				}

				break;

				case EventType.MouseUp:

				if (!draggable) {

					//select the single object if no selection modifier is being performed

					selection.SelectWhenNoAction(pressIndex, evt);

					if (onMouseUpCallback != null && IsPositionWithinElement(evt.mousePosition, selection.Last)) {

						onMouseUpCallback(this);
					}
				}
				else if (GUIUtility.hotControl == controlID) {

					evt.Use();
					dragging = false;

					if (dragMoved) {

						//move elements in list

						int dir = dragList[selection[0]].startIndex < selection[0] ? -1 : 1;
						int start = dir == 1 ? 0 : selection.Length - 1;
						int end = dir == 1 ? selection.Length : -1;

						for (int i = start; i != end; i += dir) {

							int selectionIndex = selection[i];

							list.MoveArrayElement(dragList[selectionIndex].startIndex, selectionIndex);
						}

						//restore state

						for (int i = 0; i < dragList.Count; i++) {

							dragList[i].RestoreState(list.GetArrayElementAtIndex(i));
						}

						dragList.Clear();

						//apply changes

						list.serializedObject.ApplyModifiedProperties();
						list.serializedObject.Update();

						if (onReorderCallback != null) {

							onReorderCallback(this);
						}

						DispatchChange();
					}
					else {

						dragList.Clear();

						//if we didn't drag, then select the original pressed object

						selection.SelectWhenNoAction(pressIndex, evt);

						if (onMouseUpCallback != null) {

							onMouseUpCallback(this);
						}
					}

					GUIUtility.hotControl = 0;
				}

				HandleUtility.Repaint();

				break;

				case EventType.MouseDrag:

				if (draggable && GUIUtility.hotControl == controlID) {

					GUIUtility.keyboardControl = controlID;

					UpdateDragPosition(evt.mousePosition, rect, dragList);

					evt.Use();
					dragging = true;
				}

				break;

				case EventType.KeyDown:

				if (GUIUtility.keyboardControl == controlID) {

					if (evt.keyCode == KeyCode.DownArrow) {

						selection.Select(Mathf.Min(selection.Last + 1, list.arraySize - 1));
						evt.Use();
					}
					else if (evt.keyCode == KeyCode.UpArrow) {

						selection.Select(Mathf.Max(selection.Last - 1, 0));
						evt.Use();
					}
					else if (evt.keyCode == KeyCode.Escape && GUIUtility.hotControl == controlID) {

						GUIUtility.hotControl = 0;
						dragging = false;
						selection = beforeDragSelection;
						evt.Use();
					}
				}

				break;
			}
		}

		private List<DragElement> GetDragList(float dragPosition) {

			dragList = dragList ?? new List<DragElement>();
			dragList.Clear();

			for (int i = 0; i < list.arraySize; i++) {

				SerializedProperty element = list.GetArrayElementAtIndex(i);
				Rect elementRect = elementRects[i];

				DragElement dragElement = new DragElement() {
					element = element,
					dragOffset = dragPosition - elementRect.y,
					height = elementRect.height,
					startIndex = i
				};

				dragElement.RecordState();

				dragList.Add(dragElement);
			}

			return dragList;
		}

		private void UpdateDragPosition(Vector2 position, Rect bounds, List<DragElement> dragList) {

			//find new drag position

			float oldPosition = dragPosition;
			float minOffset = dragList[selection.Min].dragOffset;
			float maxOffset = dragList[selection.Max].height - dragList[selection.Max].dragOffset;

			dragPosition = Mathf.Clamp(position.y, bounds.yMin + minOffset, bounds.yMax - maxOffset);

			//check if changed

			if (dragList != null && dragPosition != oldPosition) {

				//get the interation direction with start and end points

				int i, len = dragList.Count;
				int dir = (int)Mathf.Sign(dragPosition - oldPosition);

				int start = dir == 1 ? 0 : len - 1;
				int end = dir == 1 ? len : -1;

				//get the selection iteration direction, this is the inverse of the dragList iteration direction

				int s, sLen = selection.Length;

				int selectionStart = dir == 1 ? sLen - 1 : 0;
				int selectionEnd = dir == 1 ? -1 : sLen;

				//sort before moving

				selection.Sort();

				for (s = selectionStart; s != selectionEnd; s -= dir) {

					int selectionIndex = selection[s];

					//find the min and max positions of this selected item, we need to know where this item moved from and move to
					//to compare with dragList elements

					minOffset = dragList[selectionIndex].dragOffset;
					maxOffset = dragList[selectionIndex].height - dragList[selectionIndex].dragOffset;

					float minPos = Mathf.Min(dragPosition, oldPosition) - minOffset;
					float maxPos = Mathf.Max(dragPosition, oldPosition) + maxOffset;

					//loop over drag list

					for (i = start; i != end; i += dir) {

						//don't consider already selected items

						if (!selection.Contains(i)) {

							//swap the selected item if it's within the dragList item

							Rect elementRect = elementRects[i];

							float middle = elementRect.y + elementRect.height / 2;

							if (minPos < middle && maxPos > middle) {

								//move items in dragList

								DragElement element = dragList[i];
								dragList.RemoveAt(i);
								dragList.Insert(selection[s], element);

								//move the item in selection

								selection[s] = i;

								//update whether drag was changed

								dragMoved = dragList[i].startIndex != i;
							}
						}
					}
				}

				//sort selection when we're done moving

				selection.Sort();
			}
		}

		private int GetSelectionIndex(Vector2 position) {

			int i, len = elementRects.Length;

			for (i = 0; i < len; i++) {

				Rect rect = elementRects[i];

				if (rect.Contains(position) || (i == 0 && position.y <= rect.yMin) || (i == len - 1 && position.y >= rect.yMax)) {

					return i;
				}
			}

			return -1;
		}

		private bool CanSelect(ListSelection selection) {

			int i, len = selection.Length;

			if (len == 0) {

				return false;
			}

			for (i = 0; i < len; i++) {

				if (!CanSelect(selection[i])) {

					return false;
				}
			}

			return true;
		}

		private bool CanSelect(int index) {

			return index >= 0 && index < list.arraySize;
		}

		private bool CanSelect(Vector2 position) {

			int i, len = selection.Length;

			if (len == 0) {

				return false;
			}

			for (i = 0; i < len; i++) {

				if (IsPositionWithinElement(position, selection[i])) {

					return true;
				}
			}

			return false;
		}

		private bool IsPositionWithinElement(Vector2 position, int index) {

			if (CanSelect(index)) {

				Rect elementRect = elementRects[index];

				return elementRect.Contains(position);
			}
			else {

				return false;
			}
		}

		private bool IsElementExpandable(SerializedProperty element) {

			switch (elementDisplayType) {

				case ElementDisplayType.Auto:

				return element.hasChildren && element.propertyType != SerializedPropertyType.ObjectReference;

				case ElementDisplayType.Expandable:

				return true;

				case ElementDisplayType.SingleLine:

				return false;
			}

			return false;
		}

		//
		// -- LIST STYLE --
		//

		static class Style {



			public static GUIContent iconToolbarAdd;
		public static GUIContent iconToolbarAddMore;
			public static GUIContent iconToolbarRemoveSelected;
			public static GUIContent iconToolbarRemoveAll;
			public static GUIStyle draggingHandle;
			public static GUIStyle headerBackground;
			public static GUIStyle footerBackground;
			public static GUIStyle boxBackground;
			public static GUIStyle preButton;
			public static GUIStyle elementBackground;
			public static GUIStyle verticalLabel;
			public static GUIContent expandButton;
			public static GUIContent collapseButton;

			static Style() {
			


              /*  if (EditorPrefs.GetBool("ColorModeKey"))
                {
                    iconToolbarAdd = IconContent("add");
                    iconToolbarRemoveSelected = IconContent("removeselected");
                    iconToolbarRemoveAll = IconContent("removeall");
                }

                else if(EditorPrefs.GetBool("ColorModeKey") == false)
                {*/
                    iconToolbarAdd = EditorGUIUtility.IconContent("ol plus", "Add rotation block.");
                    iconToolbarRemoveSelected = EditorGUIUtility.IconContent("ol minus act", "Remove selected rotation block.");
                    iconToolbarRemoveAll = EditorGUIUtility.IconContent("ol minus", "Remove all rotation blocks.");
               // }
				iconToolbarAddMore = EditorGUIUtility.IconContent("ol plus", "Add rotation block.");


                //iconToolbarAdd = EditorGUIUtility.IconContent("d_winbtn_mac_max", "Add to list");

                //iconToolbarRemoveSelected = EditorGUIUtility.IconContent("d_winbtn_mac_min", "Remove selection from list");

                //iconToolbarAdd = EditorGUIUtility.IconContent("Toolbar Plus", "Add to list");
                //iconToolbarAddMore = EditorGUIUtility.IconContent("Toolbar Plus More", "Choose to add to list");
                //iconToolbarRemoveSelected = EditorGUIUtility.IconContent("Toolbar Minus", "Remove selection from list");

                iconToolbarAdd.tooltip = "Add rotation block.";
                iconToolbarRemoveSelected.tooltip = "Remove selected rotation block.";
                iconToolbarRemoveAll.tooltip = "Remove all rotation blocks.";

                draggingHandle = new GUIStyle("RL DragHandle");
				headerBackground = new GUIStyle("RL Header");
				footerBackground = new GUIStyle("RL Footer");
                elementBackground = new GUIStyle("RL Element")
                {
                    border = new RectOffset(2, 3, 2, 3)
                };
                verticalLabel = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    contentOffset = new Vector2(10, -3)
                };
                boxBackground = new GUIStyle("RL Background")
                {
                    border = new RectOffset(6, 3, 3, 6)
                };
                preButton = new GUIStyle("RL FooterButton")
                {
                    fixedWidth = 12,
                    fixedHeight = 12
                   
                   
                };
                expandButton = EditorGUIUtility.IconContent("winbtn_win_max");
				collapseButton = EditorGUIUtility.IconContent("winbtn_win_min");
			}

            static GUIContent IconContent(string icon)
            {
                Texture2D cached = EditorGUIUtility.Load("Assets/Ameye/Alex's Door System/Icons/" + icon + ".png") as Texture2D;
                return new GUIContent(cached);
            }
        }

		//
		// -- DRAG ELEMENT --
		//

		class DragElement {

			public SerializedProperty element;
			public int startIndex;
			public float dragOffset;
			public float height;

			private bool isExpanded;
			private Dictionary<int, bool> states;

			public void RecordState() {

				states = new Dictionary<int, bool>();

				isExpanded = element.isExpanded;

				//iterate over the properties within this element and record expanded state by iteration index

				Iterate(element, (SerializedProperty property, int index) => { states[index] = property.isExpanded; });
			}

			public void RestoreState(SerializedProperty element) {

				//restore the supplied element to our recorded state.
				//** This does assume that properties don't change between dragging, as states are recorded by an iteration index
				//so long as both iterations are the same (before and after) the properties will be restored :), if not, we have a problem

				element.isExpanded = isExpanded;

				Iterate(element, (SerializedProperty property, int index) => { property.isExpanded = states[index]; });
			}

			private void Iterate(SerializedProperty element, System.Action<SerializedProperty, int> action) {

				SerializedProperty copy = element.Copy();
				SerializedProperty end = copy.GetEndProperty();

				int index = 0;

				while (copy.NextVisible(true) && !SerializedProperty.EqualContents(copy, end)) {

					if (copy.hasVisibleChildren) {

						action(copy, index);
						index++;
					}
				}
			}
		}

		//
		// -- SLIDE GROUP --
		//

		class SlideGroup {

			private Dictionary<int, Rect> animIDs;

			public SlideGroup() {

				animIDs = new Dictionary<int, Rect>();
			}

			public Rect GetRect(int id, Rect r, float easing) {

				if (Event.current.type != EventType.Repaint) {

					return r;
				}

				if (!animIDs.ContainsKey(id)) {

					animIDs.Add(id, r);
					return r;
				}
				else {

					Rect rect = animIDs[id];

					if (rect.y != r.y) {

						float delta = r.y - rect.y;
						float absDelta = Mathf.Abs(delta);

						//if the distance between current rect and target is too large, then move the element towards the target rect so it reaches the destination faster

						if (absDelta > (rect.height * 2)) {

							r.y = delta > 0 ? r.y - rect.height : r.y + rect.height;
						}
						else if (absDelta > 0.5) {

							r.y = Mathf.Lerp(rect.y, r.y, easing);
						}

						animIDs[id] = r;
						HandleUtility.Repaint();
					}

					return r;
				}
			}

			public Rect SetRect(int id, Rect rect) {

				if (animIDs.ContainsKey(id)) {

					animIDs[id] = rect;
				}
				else {

					animIDs.Add(id, rect);
				}

				return rect;
			}
		}

		//
		// -- SELECTION --
		//

		class ListSelection {

			private List<int> indexes;

			internal int? firstSelected;

			public ListSelection() {

				indexes = new List<int>();
			}

			public ListSelection(int[] indexes) {

				this.indexes = new List<int>(indexes);
			}

			public int First {

				get { return indexes.Count > 0 ? indexes[0] : -1; }
			}

			public int Last {

				get { return indexes.Count > 0 ? indexes[indexes.Count - 1] : -1; }
			}

			public int Length {

				get { return indexes.Count; }
			}

			public int Min {

				get { return indexes.Min(); }
			}

			public int Max {

				get { return indexes.Max(); }
			}

			public int this[int index] {

				get { return indexes[index]; }
				set {

					int oldIndex = indexes[index];

					indexes[index] = value;

					if (oldIndex == firstSelected) {

						firstSelected = value;
					}
				}
			}

			public bool Contains(int index) {

				return indexes.Contains(index);
			}

			public void Clear() {

				indexes.Clear();
				firstSelected = null;
			}

			public void SelectWhenNoAction(int index, Event evt) {

				if (!EditorGUI.actionKey && !evt.shift) {

					Select(index);
				}
			}

			public void Select(int index) {

				indexes.Clear();
				indexes.Add(index);

				firstSelected = index;
			}

			public void Remove(int index) {

				if (indexes.Contains(index)) {

					indexes.Remove(index);
				}
			}

			public void AppendWithAction(int index, Event evt) {

				if (EditorGUI.actionKey) {

					if (Contains(index)) {

						Remove(index);
					}
					else {

						Append(index);
						firstSelected = index;
					}
				}
				else if (evt.shift && indexes.Count > 0 && firstSelected.HasValue) {

					indexes.Clear();

					AppendRange(firstSelected.Value, index);
				}
				else if (!Contains(index)) {

					Select(index);
				}
			}

			public void Sort() {

				if (indexes.Count > 0) {

					indexes.Sort();
				}
			}

			public int[] ToArray() {

				return indexes.ToArray();
			}

			public ListSelection Clone() {

                ListSelection clone = new ListSelection(ToArray())
                {
                    firstSelected = firstSelected
                };
                return clone;
			}

			internal bool CanRevert(SerializedProperty list) {

				if (list.serializedObject.targetObjects.Length == 1) {

					for (int i = 0; i < Length; i++) {

						if (list.GetArrayElementAtIndex(this[i]).isInstantiatedPrefab) {

							return true;
						}
					}
				}

				return false;
			}

			internal void RevertValues(object userData) {

				SerializedProperty list = userData as SerializedProperty;

				for (int i = 0; i < Length; i++) {

					SerializedProperty property = list.GetArrayElementAtIndex(this[i]);

					if (property.isInstantiatedPrefab) {

						property.prefabOverride = false;
					}
				}

				list.serializedObject.ApplyModifiedProperties();
				list.serializedObject.Update();

				HandleUtility.Repaint();
			}

			internal void Duplicate(object userData) {

				SerializedProperty list = userData as SerializedProperty;

				int offset = 0;

				for (int i = 0; i < Length; i++) {

					this[i] += offset;

					list.GetArrayElementAtIndex(this[i]).DuplicateCommand();
					list.serializedObject.ApplyModifiedProperties();
					list.serializedObject.Update();

					offset++;
				}

				HandleUtility.Repaint();
			}

			internal void Offset(int offset) {

				for (int i = 0; i < Length; i++) {

					this[i] += offset;
				}
			}

			internal void Delete(object userData) {

				SerializedProperty list = userData as SerializedProperty;

				Sort();

				int i = Length;

				while (--i > -1) {

					list.GetArrayElementAtIndex(this[i]).DeleteCommand();
				}

				Clear();

				list.serializedObject.ApplyModifiedProperties();
				list.serializedObject.Update();

				HandleUtility.Repaint();
			}

			private void Append(int index) {

				if (index >= 0 && !indexes.Contains(index)) {

					indexes.Add(index);
				}
			}

			private void AppendRange(int from, int to) {

				int dir = (int)Mathf.Sign(to - from);

				if (dir != 0) {

					for (int i = from; i != to; i += dir) {

						Append(i);
					}
				}

				Append(to);
			}
		}

		//
		// -- EXCEPTIONS --
		//

		class InvalidListException : System.InvalidOperationException {

			public InvalidListException() : base("ReorderableList serializedProperty must be an array") {
			}
		}

		class MissingListExeption : System.ArgumentNullException {

			public MissingListExeption() : base("ReorderableList serializedProperty is null") {
			}
		}
	}
}
