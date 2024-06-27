using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DialogueGraphView : GraphView
{
    public readonly Vector2 _defaultNodeSize = new Vector2(150, 200);
    public DialogueGraphView()
    {
        StyleSheet styleSheet = Resources.Load<StyleSheet>("DialogueGraph");
        styleSheets.Add(styleSheet);
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        GridBackground gridBackground = new();
        Insert(0, gridBackground);
        gridBackground.StretchToParentSize();

        AddElement(GenerateEntryPointNode());
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new();

        ports.ForEach(port =>
        {
            if (startPort != port && startPort.node != port.node)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }

    public Port GeneratePort(DialogueNode targetNode, Direction portDirection, Port.Capacity portCapacity = Port.Capacity.Single)
    {
        return targetNode.InstantiatePort(Orientation.Horizontal, portDirection, portCapacity, typeof(float));
    }

    private DialogueNode GenerateEntryPointNode()
    {
        DialogueNode node = new()
        {
            title = "Start",
            GUID = Guid.NewGuid().ToString(),
            dialogueText = "EntryPoint",
            entryPoint = true
        };

        Port rightHandPort = GeneratePort(node, Direction.Output);
        rightHandPort.portName = "Start";
        node.outputContainer.Add(rightHandPort);

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 150));
        return node;
    }

    public void AddNewNodeToGraphView(string name)
    {
        AddElement(CreateDialogueNode(name));
    }

    public DialogueNode CreateDialogueNode(string name)
    {
        DialogueNode newNode = new()
        {
            title = name,
            dialogueText = name,
            GUID = Guid.NewGuid().ToString()
        };

        var inputPort = GeneratePort(newNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        newNode.inputContainer.Add(inputPort);

        Button addChoiceButton = new(() =>
        {
            AddChoicePort(newNode);
        })
        {
            text = "New Choice"
        };
        newNode.titleContainer.Add(addChoiceButton);

        TextField textField = new()
        {
            label = ""
        };
        textField.RegisterValueChangedCallback(evt =>
        {
            newNode.dialogueText = evt.newValue;
            newNode.title = evt.newValue;
            newNode.RefreshExpandedState();
            newNode.RefreshPorts();
        });
        textField.SetValueWithoutNotify(newNode.title);
        newNode.mainContainer.Add(textField);

        newNode.RefreshExpandedState();
        newNode.RefreshPorts();
        newNode.SetPosition(new Rect(Vector2.zero, _defaultNodeSize));

        return newNode;
    }

    public void AddChoicePort(DialogueNode targetNode, string overridenPortname = "")
    {
        Port newPort = GeneratePort(targetNode, Direction.Output);

        Label oldLabel = newPort.contentContainer.Q<Label>("type");
        newPort.contentContainer.Remove(oldLabel);

        float currentOutputCount = targetNode.outputContainer.Query("connector").ToList().Count;
        string portChoiceName = string.IsNullOrEmpty(overridenPortname) ? $"Choice {currentOutputCount}" : overridenPortname;

        TextField textfield = new()
        {
            name = string.Empty,
            label = portChoiceName
        };
        TextInputBaseField<string> inputField = textfield.Q<TextInputBaseField<string>>();
        textfield.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.KeypadEnter)
            {
                VisualElement parent = textfield.parent;
                while (parent != null && !parent.focusable)
                {
                    parent = parent.parent;
                }
                parent.Focus();
            }
        });
        inputField.RegisterCallback<FocusOutEvent>(evt =>
        {
            if (textfield.value == "")
            {
                return;
            }
            newPort.portName = textfield.value;
            textfield.label = textfield.value;
            textfield.value = "";
        });
        newPort.contentContainer.Add(new Label(" "));
        newPort.contentContainer.Add(textfield);

        Button deleteButton = new(() =>
        {
            DeleteConnection(targetNode, newPort);
            targetNode.outputContainer.Remove(newPort);
            targetNode.RefreshPorts();
            targetNode.RefreshExpandedState();
        })
        {
            text = "X"
        };
        newPort.contentContainer.Add(deleteButton);


        newPort.portName = portChoiceName;
        targetNode.outputContainer.Add(newPort);
        targetNode.RefreshPorts();
        targetNode.RefreshExpandedState();
    }

    private void DeleteConnection(DialogueNode targetNode, Port newPort)
    {
        var targetEdges = edges.ToList().Where(x => x.output.portName == newPort.portName && x.output.node == newPort.node);

        if (!targetEdges.Any())
        {
            return;
        }
        Edge edge = targetEdges.First();
        edge.input.Disconnect(edge);
        RemoveElement(targetEdges.First());

    }
}
