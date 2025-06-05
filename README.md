Revit API .net reference code

App.cs - Revit Plug-in external application start up class implementing IExternalApplication interface
RevitTools.addin - Manifest file for Revit to read loccation of start up class in dll assembly

Directories:
1. ConvoidOpenings - application to update, control, export convoid openings parameters
   - works with IUpdater interface, settings are available as Revit Dock Panel
   - includes RevitLink data local buffering
   - includes data sending via HTTP Client to MS Flows
   - includes finding family instance solid coordinates
  
2. Generate Sheets - application to create sheets with room views
   - creates elevation marker and NS EW views for selected rooms
   - creates floor plan, ceiling plan nad 3D view for selected rooms
   - creates sheet view of required size and places all created views on it
   - creates title block and places it on sheet view
   - saves information about created views and sheet to each room as IExtensibleStorage
