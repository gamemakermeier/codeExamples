using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility
{
    private DialogueGraphView _targetGraphView;
    private string _lastLoadedPath = "";
    private DialogueContainer _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();
    private static GraphSaveUtility Instance;
    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        if (Instance == null)
        {
            Instance = new GraphSaveUtility()
            {
                _targetGraphView = targetGraphView,
            };
        }
        else
        {
            Instance._targetGraphView = targetGraphView;
        }
        return Instance;

    }

    public void SaveGraph(string currentFileName = "")
    {
        string savePath = "";
        if (_lastLoadedPath != "")
        {
            savePath = _lastLoadedPath.Replace(_lastLoadedPath.Split("/")[^1].Replace(".asset",""),currentFileName);
        }
        else
        {
            Debug.Log("It is not, opening panel");
            savePath = Ampere.EditorTools.GetLocalPath(EditorUtility.SaveFilePanel("Save Dialog", "", "New Dialogue Tree", "asset"));
        }
        if (string.IsNullOrEmpty(savePath))
        {
            UnityEditor.EditorUtility.DisplayDialog("Invalid file name!", "The name of a save file can not be empty", "OK");
            return;
        }

        GraphSaveUtility graphSaveUtility = new();
        if (!Edges.Any())
        {
            return;
        }

        DialogueContainer dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        Edge[] connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (int i = 0; i < connectedPorts.Length; ++i)
        {
            DialogueNode outputNode = connectedPorts[i].output.node as DialogueNode;
            DialogueNode inputNode = connectedPorts[i].input.node as DialogueNode;

            dialogueContainer.linkData.Add(new()
            {
                baseNodeGUID = outputNode.GUID,
                portname = connectedPorts[i].output.portName,
                targetNodeGUID = inputNode.GUID
            });
        }

        foreach (DialogueNode node in Nodes.Where(node => !node.entryPoint))
        {
            dialogueContainer.nodeData.Add(new()
            {
                GUID = node.GUID,
                dialogueText = node.dialogueText,
                position = node.GetPosition().position
            });
        }
        AssetDatabase.CreateAsset(dialogueContainer, savePath);
        AssetDatabase.SaveAssets();
    }

    public void LoadGraph(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            UnityEditor.EditorUtility.DisplayDialog("Invalid file name!", "The name of a save file can not be empty", "OK");
            return;
        }
        _containerCache = AssetDatabase.LoadAssetAtPath(filePath, typeof(DialogueContainer)) as DialogueContainer;
        if (_containerCache == null)
        {
            UnityEditor.EditorUtility.DisplayDialog("File not found", $"Could not find a file named {filePath}", "OK");
            return;
        }
        Instance._lastLoadedPath = filePath;
        ClearGraph();
        CreateNodes();
        ConnectNodes();
        Debug.Log($"Last loaded path set to {_lastLoadedPath}");
    }

    private void ConnectNodes()
    {
        for (int i = 0; i < Nodes.Count; ++i)
        {
            List<NodeLinkData> linkdata = _containerCache.linkData.Where(x => x.baseNodeGUID == Nodes[i].GUID).ToList();
            for (int j = 0; j < linkdata.Count; ++j)
            {
                string targetNodeGUID = linkdata[j].targetNodeGUID;
                DialogueNode targetNode = Nodes.First(x => x.GUID == targetNodeGUID);
                LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(_containerCache.nodeData.First(x => x.GUID == targetNodeGUID).position, _targetGraphView._defaultNodeSize));
            }
        }
    }

    private void LinkNodes(Port outPut, Port input)
    {
        Edge tempEdge = new()
        {
            output = outPut,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);

        _targetGraphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (DialogueNodeData dialogueNodeData in _containerCache.nodeData)
        {
            DialogueNode tempNode = _targetGraphView.CreateDialogueNode(dialogueNodeData.dialogueText);
            tempNode.GUID = dialogueNodeData.GUID;
            _targetGraphView.AddElement(tempNode);

            List<NodeLinkData> nodeLinks = _containerCache.linkData.Where(x => x.baseNodeGUID == dialogueNodeData.GUID).ToList();
            nodeLinks.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portname));
        }
    }

    private void ClearGraph()
    {
        Nodes.Find(x => x.entryPoint).GUID = _containerCache.linkData[0].baseNodeGUID;

        foreach (DialogueNode node in Nodes)
        {
            if (node.entryPoint)
            {
                continue;
            }
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
            _targetGraphView.RemoveElement(node);
        }
    }
}
