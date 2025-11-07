using System.Collections.Generic;
using NXOpen;
using NXOpen.Annotations;
using NXOpen.UF;
using System.Collections.Generic;
using System.Linq;
using System;


public static class PmiHighlighter
{
    private static UFSession ufSession = UFSession.GetUFSession();

    // Toggles highlighting of PMI faces:
    // First removes all previous highlights from PMI faces,
    // then highlights only the faces of PMIs that are marked as active (true) in the pmiState.
    public static void ToggleHighlight(Dictionary<Pmi, bool> pmiState, Dictionary<Pmi, List<Face>> pmiFaceMap)
    {
        // remove old highlights
        foreach (var kvp in pmiFaceMap)
        {
            if (kvp.Value == null) continue;
            foreach (var face in kvp.Value)
            {
                if (face != null && face.Tag > 0)
                {
                    try
                    {
                        ufSession.Disp.SetHighlight(face.Tag, 0);
                    }
                    catch (Exception ex)
                    {
        
                        UI.GetUI().NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.Message);
                    }
                }
            }
        }

        foreach (var selectedPmi in pmiState)
        {
            if (!selectedPmi.Value) continue;

            foreach (var kvp in pmiFaceMap)
            {
                if (selectedPmi.Key == kvp.Key)
                {
                    var faces = kvp.Value;
                    foreach (var face in faces)
                    {
                        ufSession.Disp.SetHighlight(face.Tag, 1);
                    }
                }
            }
        }
    }
    // Clears all highlighting from faces associated with all PMIs
    public static void ClearPmiHighlight(Dictionary<Pmi, List<Face>> pmiFaceMap)
    {
        if (ufSession == null)
            ufSession = UFSession.GetUFSession();

        foreach (var kvp in pmiFaceMap)
        {
            if (kvp.Value == null) continue;

            foreach (var face in kvp.Value)
            {
                if (face != null && face.Tag > 0)
                {
                    ufSession.Disp.SetHighlight(face.Tag, 0);
                }
            }
        }
    }
}