using System;
using System.Collections.Generic;
using System.Linq;
using NXOpen;
using NXOpen.Annotations;
using NXOpen.BlockStyler;

public static class PmiListBuilder
{
    // Collects all PMIs from the currently loaded NX part,
    // maps each PMI to a unique string key, associates each PMI with the list of faces it references, 
    // and initializes the boolean state map for each PMI to false (because they are not clicked yet)
    public static void createPmiLists(Dictionary<string, Pmi> pmiMap, Dictionary<Pmi, List<Face>> pmiFaceMap,
        Dictionary<Pmi, bool> pmiState)
    {
        UI theUI = UI.GetUI();
        NXOpen.Session theSession = NXOpen.Session.GetSession();
        NXOpen.Part workPart = theSession.Parts.Work;
        if (workPart == null)
        {
            UI.GetUI().NXMessageBox.Show("Error", NXMessageBox.DialogType.Error, "No part loaded.");
        }

        PmiManager pmiManager = workPart.PmiManager;
        PmiCollection pmis = pmiManager.Pmis;

        List<string> pmiNames = new List<string>();

        foreach (NXOpen.Annotations.Pmi pmi in pmis)
        {
            AssociatedObject assObject = pmi.GetAssociatedObject();
            NXObject[] objekt = assObject.GetObjects();
            // generate a unique key for each PMI
            string key = pmi.Index.ToString();

            pmiMap.Add(key, pmi);

            List<Face> faces = new List<Face>();
            foreach (NXObject nxobj in objekt)
            {
                if (nxobj is Face objface)
                {
                    faces.Add(objface);
                }
            }

            if (!pmiFaceMap.ContainsKey(pmi))
            {
                pmiFaceMap[pmi] = faces;
            }
        }

        foreach (var key in pmiMap.Values)
        {
            pmiState[key] = false;
        }
    }

    // Populates a ListBox with all PMIs, showing their selection state (checked or unchecked), name, type, and key.
    // Updates the ListBox with formatted PMI entries
    public static void PopulatePmiList(ListBox listBox, Dictionary<string, Pmi> pmiMap, Dictionary<Pmi, bool> pmiState)
    {
        try
        {
            List<string> pmiNames = new List<string>();

            // create the names of the PMIs and add them to the list
            foreach (var kvp in pmiMap)
            {
                string key = kvp.Key;
                Pmi pmi = kvp.Value;

                string prefix = pmiState[pmi] ? "[x] " : "[ ] ";

                string pmiName = pmi.Name;
                string pmiType = pmi.Type.ToString();

                string displayText = prefix + pmiName + " - " + pmiType + " - " + "[" + key + "]";

                pmiNames.Add(displayText);
            }

            // set all the PMIs in the list  
            listBox.SetListItems(pmiNames.ToArray());
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("PopulatePmiList"))
            {
                return;
            }
        }
    }

    // Compares the faces associated with each PMI against the faces used in a selected CAM operation.
    // It collects PMIs connected to the selected CAM operation (based on shared faces) and stores them in 'connectedPmi'.
    // If no matching PMIs are found at all for the selected CAM operation, it shows a message
    public static void ComparePmiAndCamFaces(NXOpen.CAM.Operation selectedCam, Dictionary<Pmi, bool> pmiState, Dictionary<Pmi, List<Face>> pmiFaceMap, Dictionary<NXOpen.CAM.Operation, List<Face>> camOperationFaceMap, List<Pmi> connectedPmi)
    {
        connectedPmi.Clear();
        List<Pmi> uniquePmis = new List<Pmi>();

        // variable to check if the selected operation is in the list
        bool selectedOperationInList = false;

        foreach (var pmiEntry in pmiState)
        {
            var pmi = pmiFaceMap.Keys.FirstOrDefault(k => k == pmiEntry.Key);
            if (pmi == null || !pmiFaceMap.ContainsKey(pmi)) continue;

            var selectedFaces = pmiFaceMap[pmi];
            if (selectedFaces == null || selectedFaces.Count == 0) continue;

            foreach (var camEntry in camOperationFaceMap)
            {
                NXOpen.CAM.Operation camOperation = camEntry.Key;

                var camFaces = camEntry.Value;
                if (selectedCam != null && camOperation.Tag != selectedCam.Tag) continue; // only compare with selected operation
                if (selectedFaces.Any(face => camFaces.Contains(face)))
                {
                    selectedOperationInList = true;
                    uniquePmis.Add(pmi);
                }
            }

        }
        connectedPmi.AddRange(uniquePmis);
        if (!selectedOperationInList)
        {
            UI.GetUI().NXMessageBox.Show("Error", NXMessageBox.DialogType.Information, "No PMI found for the selected CAM operation.");
        }
    }

    // Resets the selection state of all PMIs by setting their state to false in the pmiState dictionary
    // (when using clear-button)
    public static void ClearPmiState(Dictionary<Pmi, bool> pmiState)
    {
        foreach (var key in pmiState.Keys.ToList())
        {
            try
            {
                pmiState[key] = false;
            }
            catch (Exception e)
            {
            }
        }
    }
}
