using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{

    private DialogueGraphView _graphView;
    private string _dialogueTreefilename = "New DialogueTree";

    [MenuItem("BehindUnyieldingEyes/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        EditorWindow window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        ConstructGraphGraphView();
        GenerateToolBar();
        CreateMiniMap();
    }

    private void CreateMiniMap()
    {
        MiniMap miniMap = new()
        {
            anchored = true
        };
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }

    private void ConstructGraphGraphView()
    {
        _graphView = new DialogueGraphView();
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolBar()
    {
        Toolbar toolbar = new();

        TextField fileNameTextField = new("File Name:");
        fileNameTextField.SetValueWithoutNotify(_dialogueTreefilename);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _dialogueTreefilename = evt.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(() => GraphSaveUtility.GetInstance(_graphView).SaveGraph(_dialogueTreefilename)) { text = "Save data" });
        toolbar.Add(new Button(() => GraphSaveUtility.GetInstance(_graphView).LoadGraph(Ampere.EditorTools.GetLocalPath(EditorUtility.OpenFilePanel("File to load", "", "asset")))) { text = "Load data" });


        Button createNodeButton = new(() =>
        {
            _graphView.AddNewNodeToGraphView("Dialogue Node");
        })
        {
            text = "Create new Node"
        };

        toolbar.Add(createNodeButton);

        _graphView.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                GraphSaveUtility.GetInstance(_graphView).SaveGraph(_dialogueTreefilename);
            }
        });
        rootVisualElement.Add(toolbar);
    }
}