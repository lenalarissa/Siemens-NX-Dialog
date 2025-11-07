using System;
using System.Collections.Generic;
using System.IO;
using NXOpen;
using NXOpen.CAM;
using NXOpen.BlockStyler;
using NXOpen.UF;
using NXOpen.CAE;
using Operation = NXOpen.CAM.Operation;
using NXOpen.Annotations;
using System.Linq;
using static NXOpen.Annotations.Pmi;
using static NXOpen.CAE.AeroStructures.MatrixManip;

public static class CamListBuilder

{
    // stores all CAM operations in 'camMap', and initializes their selection state in 'camState' to false.
    public static void createCamOperationLists(CAMFeature[] camFeatures, Dictionary<string, NXOpen.CAM.Operation> camMap, Dictionary<NXOpen.CAM.Operation, bool> camState)
    {
        NXOpen.CAM.Operation[] camOperations;
        List<string> operationNames = new List<string>();

        foreach (var camFeature in camFeatures)
        {
            if (camFeature is NXOpen.CAM.CAMFeature)
            {
                camOperations = camFeature.GetOperations();
                foreach (var operation in camOperations)
                {
                    camMap[operation.Tag.ToString()] = operation;
                    operationNames.Add(operation.Name);
                }
            }
        }
        foreach (var key in camMap.Values)
        {
            camState[key] = false;
        }
    }

    // Populates a ListBox with all CAM operations, showing their selection state (checked or unchecked) and operation name.
    public static void PopulateCamOperationList(ListBox listBox, Dictionary<string, NXOpen.CAM.Operation> camMap, Dictionary<NXOpen.CAM.Operation, bool> camState)
    {
        try
        {
            List<string> camNames = new List<string>();

            // create the names of the CAM operations and add them to the list
            foreach (var kvp in camMap)
            {
                string key = kvp.Key;
                NXOpen.CAM.Operation camOperation = kvp.Value;
                string camName = kvp.Value.Name;

                string prefix = camState[camOperation] ? "[x] " : "[ ] ";

                string displayText = prefix + camName + " - " + "[" + key + "]";

                camNames.Add(displayText);
            }

            listBox.SetListItems(camNames.ToArray());
        }
        catch (Exception ex)
        {
            {
                UI.GetUI().NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
        }
    }

    // Fills a tree by listing PMIs and their connected CAM operations underneath,
    // showing the hierarchy of PMI to related CAM operations.
    public static void PopulateConnectedCamList(Tree tree, Dictionary<string, NXOpen.CAM.Operation> camMap, List<NXOpen.CAM.Operation> connectedCam, Dictionary<Pmi, List<NXOpen.CAM.Operation>> pmiCamOperationMap)
    {

        ClearTree(tree);
        try
        {

            foreach (var kvp in pmiCamOperationMap)
            {
                string label = kvp.Key.Name + " " + kvp.Key.Type;
                var parent = tree.CreateNode(label);
                tree.InsertNode(parent, null, null, Tree.NodeInsertOption.Last);

                foreach (var cam in kvp.Value)
                {
                    var child = tree.CreateNode(cam.Name);
                    tree.InsertNode(child, parent, null, Tree.NodeInsertOption.Last);
                }
            }
        }
        catch (Exception ex)
        {
        }
        finally
        {
            tree.Redraw(true); // tree is redrawn after all nodes are deleted and added
        }
    }

    // Creates a mapping of CAM operations to their associated faces,
    // by matching operations from 'camMap' with features from 'camFeatures'.
    public static void PopulateCamWithFaces(CAMFeature[] camFeatures, Dictionary<string, NXOpen.CAM.Operation> camMap, Dictionary<NXOpen.CAM.Operation, List<Face>> camOperationFaceMap)
    {
        foreach (var camOperation in camMap)
        {
            List<Face> camFaces = new List<Face>();
            Operation currentOperation = camOperation.Value;

            foreach (CAMFeature feature in camFeatures)
            {
                NXOpen.CAM.Operation[] featureOps = feature.GetOperations();
                foreach (NXOpen.CAM.Operation op in featureOps)
                {
                    if (op.Tag == currentOperation.Tag)
                    {
                        camFaces.AddRange(feature.GetFaces());
                        break;
                    }
                }
            }
            camOperationFaceMap[currentOperation] = camFaces;
        }
    }

    // Compares the selected PMIs (pmiState) with CAM operations based on shared faces,
    // collects connected CAM operations, updates the pmiCamOperationMap and camState accordingly.
    public static void ComparePmiAndCamFaces(
        Dictionary<Pmi, bool> pmiState,
        Dictionary<Pmi, List<Face>> pmiFaceMap,
        Dictionary<NXOpen.CAM.Operation, List<Face>> camOperationFaceMap, List<Operation> connectedCam,
        Dictionary<NXOpen.CAM.Operation, bool> camState,
        Dictionary<Pmi, List<NXOpen.CAM.Operation>> pmiCamOperationMap)
    {
        connectedCam.Clear();
        // clear pmicamoperationmap
        foreach (var key in pmiCamOperationMap.Keys.ToList())
        {
            pmiCamOperationMap[key].Clear();
        }
        List<Operation> uniqueCamOps = new List<Operation>();

        foreach (var pmiEntry in pmiState)
        {
            if (!pmiEntry.Value) continue;

            var pmi = pmiFaceMap.Keys.FirstOrDefault(k => k == pmiEntry.Key);
            if (pmi == null || !pmiFaceMap.ContainsKey(pmi)) continue;

            var selectedFaces = pmiFaceMap[pmi];
            if (selectedFaces == null || selectedFaces.Count == 0) continue;

            foreach (var camEntry in camOperationFaceMap)
            {
                var camOperation = camEntry.Key;
                var camFaces = camEntry.Value;

                if (selectedFaces.Any(face => camFaces.Contains(face)))
                {
                    uniqueCamOps.Add(camOperation);
                    // add the cam operation to the pmicamoperationmap and check if PMI is already in the list
                    if (pmiCamOperationMap.ContainsKey(pmi))
                    {
                        if (!pmiCamOperationMap[pmi].Contains(camOperation))
                        {
                            pmiCamOperationMap[pmi].Add(camOperation);
                        }
                    }
                    else
                    {
                        pmiCamOperationMap[pmi] = new List<NXOpen.CAM.Operation> { camOperation };
                    }
                }
            }
        }

        connectedCam.AddRange(uniqueCamOps);

        ClearCamState(camState);
        // update camstate
        foreach (var oper in connectedCam)
        {
            camState[oper] = true;
        }
    }

    // Clears the list of CAM operations displayed in the ListBox,
    // resetting both the item list and selected items.
    public static void ClearCamOperationList(ListBox listBox)
    {
        // clear tree 
        listBox.SetSelectedItemStrings(new string[] { });
        listBox.SetListItems(new string[] { });
    }

    // Resets the selection state of all CAM operations to false in the camState dictionary
    public static void ClearCamState(Dictionary<NXOpen.CAM.Operation, bool> camState)
    {
        foreach (var key in camState.Keys.ToList())
        {
            camState[key] = false;
        }
    }

    // Clears all nodes from the and redraws it,
    public static Tree ClearTree(Tree tree)
    {
        try
        {
            while (tree.RootNode != null)
            {
                try
                {
                    tree.DeleteNode(tree.RootNode);
                }
                catch
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            UI.GetUI().NXMessageBox.Show("Warning", NXMessageBox.DialogType.Error, ex.Message);
        }
        finally
        {
            tree.Redraw(true);
        }

        return tree;
    }

}