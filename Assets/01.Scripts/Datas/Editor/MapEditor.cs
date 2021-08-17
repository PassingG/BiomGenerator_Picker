using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(MapData))]
public class MapEditor : Editor
{
    #region [ Property ]

    private const int marginY = 5;

    protected int CellWidth => 20;
    protected int CellHeight => 20;

    bool gridSizeChanged;
    bool wrongSize;

    protected Vector2Int newMapGridSize;
    private Vector2 scrollPos;
    private Vector2 cellSize;

    private ReorderableList reorderableList;

    #endregion

    #region [ Serialized Property ]

    protected SerializedProperty mapGridSize;
    protected SerializedProperty mapCells;
    protected SerializedProperty biomDatas;

    #endregion

    void OnEnable()
    {
        PropertyInit();
        ReorderableListInit();
    }

    public override void OnInspectorGUI()
    {
        // Always do this at the beginning of InspectorGUI.
        serializedObject.Update(); 

        EditorGUI.BeginChangeCheck();

        ShowMapCellsEditor();
        ShowReorderableList();

        EditorGUI.EndChangeCheck();

        // Apply changes to all serializedProperties - always do this at the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties(); 
    }

    #region [ Initialize ]
    private void PropertyInit()
    {
        mapGridSize = serializedObject.FindProperty("m_MapGridSize");
        mapCells = serializedObject.FindProperty("m_MapData");
        biomDatas = serializedObject.FindProperty("m_Bioms");

        newMapGridSize = mapGridSize.vector2IntValue;
        cellSize = new Vector2(CellWidth, CellHeight);
    }

    private void ReorderableListInit()
    {
        reorderableList = new ReorderableList(serializedObject, biomDatas, true, true, true, true);

        // Set Header
        reorderableList.drawHeaderCallback = (rect) =>
                     EditorGUI.LabelField(rect, biomDatas.displayName);

        // Element list size
        reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = biomDatas.GetArrayElementAtIndex(index);
            rect.height -= 4;
            rect.y += 2;
            EditorGUI.PropertyField(rect, element);
        };

        reorderableList.onRemoveCallback = (ReorderableList l) =>
        {
            if (EditorUtility.DisplayDialog("Warning!",
                "Are you sure you want to delete the BiomData?", "Yes", "No"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            }
        };
    }
#endregion

    #region [ GUI Editors ]
    // MapCellLayoutView
    private void ShowMapCellsEditor()
    {
        using (var h = new EditorGUILayout.HorizontalScope())
        {
            newMapGridSize = EditorGUILayout.Vector2IntField(
                new GUIContent("Map Size", "NOTE: X is the number of ROWS and Y the number of COLUMNS.")
                ,newMapGridSize);

            // Is GridSize changed?
            gridSizeChanged = newMapGridSize != mapGridSize.vector2IntValue;

            // Is GridSize have wrong size?
            wrongSize = (newMapGridSize.x <= 0 || newMapGridSize.y <= 0);

            GUI.enabled = gridSizeChanged && wrongSize.Equals(false);

            if (GUILayout.Button("Apply", EditorStyles.miniButton))
            {
                var operationAllowed = false;

                if (newMapGridSize.x < mapGridSize.vector2IntValue.x ||
                    newMapGridSize.y < mapGridSize.vector2IntValue.y) // Smaller grid
                {
                    operationAllowed = EditorUtility.DisplayDialog("Are you sure?",
                        "You're about to reduce the width or height of the grid. This may erase some cells. Do you really want this?",
                        "Yes", "No");
                }
                else // Bigger grid
                {
                    operationAllowed = true;
                }

                if (operationAllowed)
                {
                    InitNewGridMap(newMapGridSize);
                }
            }

            GUI.enabled = true;

            if (GUILayout.Button("Clear", EditorStyles.miniButton))
            {
                InitNewGridMap(newMapGridSize, true);
            }
        }

        if (wrongSize)
        {
            EditorGUILayout.HelpBox("Wrong size.", MessageType.Error);
        }

        EditorGUILayout.Space();

        ShowMapGrid();
    }


    private void ShowMapGrid()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        {
            for (var y = mapGridSize.vector2IntValue.y - 1; y >= 0; y--)
            {
                var row = GetRowAtMap(y);

                using (var h = new EditorGUILayout.HorizontalScope())
                {
                    for (var x = 0; x < mapGridSize.vector2IntValue.x; x++)
                    {
                        EditorGUILayout.PropertyField(row.GetArrayElementAtIndex(x), GUIContent.none,
                            GUILayout.Width(cellSize.x), GUILayout.Height(cellSize.y));
                    }
                }

                GUILayout.Space(marginY);
            }
        }
        EditorGUILayout.EndScrollView();
    }
    
    protected void ShowReorderableList()
    {
        EditorGUILayout.Space(10);
        reorderableList.DoLayoutList();
    }
#endregion

    #region [ Other Function ]
    private void InitNewGridMap(Vector2Int newSize, bool clear = false)
    {
        mapCells.ClearArray();

        for (var y = 0; y < newSize.y; y++)
        {
            mapCells.InsertArrayElementAtIndex(y);
            var row = GetRowAtMap(y);

            for (var x = 0; x < newSize.x; x++)
            {
                row.InsertArrayElementAtIndex(x);

                SetValueMap(row.GetArrayElementAtIndex(x), x, y, clear);
            }
        }

        mapGridSize.vector2IntValue = newMapGridSize;
    }

    protected SerializedProperty GetRowAtMap(int idx)
    {
        return mapCells.GetArrayElementAtIndex(idx).FindPropertyRelative("row");
    }

    protected void SetValueMap(SerializedProperty cell, int x, int y, bool clear = false)
    {
        int[,] previousCells = (target as MapData).GetMapCells();

        cell.intValue = default(int);

        if (x < mapGridSize.vector2IntValue.x && y < mapGridSize.vector2IntValue.y)
        {
            cell.intValue = clear ? 0 : previousCells[x, y];
        }
    }
#endregion
}

