# Siemens-NX-GWIEIT

## What Is the PMI–CAM Dialog?

The PMI–CAM dialog is a custom-built tool developed for Siemens NX that allows users to easily explore how PMI annotations are linked to CAM operations within a part file. It is specifically designed to assist engineers and NX programmers during the manufacturing setup process by clearly showing which product features are referenced by each CAM operation. This tool has been tested and validated using Siemens NX 2412. By integrating design and manufacturing data in one interface, the dialog reduces the need to switch between windows and improves clarity. It also helps ensure that critical tolerances, finishes, and dimensions are not overlooked during machining, supporting more accurate and efficient workflows.

### Why Use It?

- To verify that all important PMIs are addressed by machining operations  
- To trace which operations are connected to which part features  
- To highlight and inspect geometry associated with both PMIs and CAM steps  
- To get a clear overview of PMI–CAM relationships in a tree structure  
- To reduce time spent switching between windows and improve workflow efficiency  
- To minimize the risk of human error by providing a direct visual connection between design annotations and machining steps  
- To support model-based manufacturing workflows with better traceability and context  

### How to Use It

1. Load a part that contains both PMI and CAM data.
2. Open the dialog (for example, via journal or NX Open execution).
3. Use the PMI-list to select PMIs and view:
   - Highlighted geometry in the model
   - Related CAM operations in the list and tree
4. Or select a CAM operation from the CAM-Operations-list to see which PMIs are associated with it.
5. Use the tree view on top to explore how PMIs and CAM operations are linked.
6. Use the **"Clear"** button to reset selections and remove all highlights.

## Installation and Usage Instructions

1. **Download the Project**  
   - Go to the GitHub repository.  
   - Click on the **"Code"** button and select **"Download ZIP"**.  
   - Extract the downloaded ZIP file to a location of your choice.

2. **Run in Siemens NX**  
   - Open Siemens NX.  
   - Navigate to **File > Execute > NX Open...**  
     *(Alternatively, press **Ctrl + U**.)*
   - In the file selection dialog, locate and open the following file from the extracted folder:  
     `Siemens-NX_GWIEIT-main/projectDLLs/CamAndPMINX.dll`  

## Optional: Button Customization in Siemens NX
![Screenshot 2025-05-26 083807](https://github.com/user-attachments/assets/e9a2b483-2b2c-49a2-852e-5c1dcb01ccab)

While you can execute the code directly via **File > Execute > NX Open...** or by pressing **Ctrl + U**, creating a custom button in Siemens NX can make access faster and provide a cleaner integration into the UI.

#### Steps to Add a Custom Button:

1. Open Siemens NX and navigate to the tab where you want the button to appear (e.g., **Tools**).
2. Right-click in the upper toolbar area and select **Customize...**
   Alternatively, you can search for "Customize" using the search bar.
3. In the Customize window:

   * Click on **New Item** in the left panel.
   * In the right panel, drag **New User Command** into the desired location on the toolbar.
4. In the left panel, go to **My Items > My User Commands**.
5. In the right panel, right-click your new user command and select **Edit Action**.
   
![Screenshot 2025-05-26 082732](https://github.com/user-attachments/assets/18c3d3f3-73d3-4418-a9f1-db159e8a4c61)

   * In the field "Enter an Action or Use Browse", provide the path to the `CamAndPMINX.dll` file located in the `projectDLLs` folder.
   * Click **OK** to save.
7. While the Customize window is still open:

   * Right-click the new button in the toolbar.
   * Select **Rename** and change the name to something like **PMI-CAM-Operations**.
   * Click **Change Icon** to assign a custom icon, if desired.

The button is now ready to use and will persist even after restarting Siemens NX.
