using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

    #endregion

    #region [ Serialized Property ]

    protected SerializedProperty mapGridSize;
    protected SerializedProperty mapCells;

    #endregion


    void OnEnable()
    {
        mapGridSize = serializedObject.FindProperty("m_MapGridSize");
        mapCells = serializedObject.FindProperty("m_MapData");

        newMapGridSize = mapGridSize.vector2IntValue;
        cellSize = new Vector2(CellWidth, CellHeight);
    }

    public override void OnInspectorGUI()
    {
        // Always do this at the beginning of InspectorGUI.
        serializedObject.Update(); 

        EditorGUI.BeginChangeCheck();
        
        MapCellsEditor();

        EditorGUI.EndChangeCheck();

        // Apply changes to all serializedProperties - always do this at the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties(); 
    }

    // MapCellLayoutView
    private void MapCellsEditor()
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

        DisplayMapGrid();
    }

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

    private void DisplayMapGrid()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        {
            Debug.Log(mapGridSize.vector2IntValue);
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
}

