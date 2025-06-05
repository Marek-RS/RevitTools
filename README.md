Revit API .net reference code

App.cs - Revit Plug-in external application start up class implementing IExternalApplication interface
RevitTools.addin - Manifest file for Revit to read loccation of start up class in dll assembly

Directories:
1. ConvoidOpenings - includes functions to update, control, export convoid openings parameters
   - works with IUpdater interface, settings are available as Revit Dock Panel
   - includes RevitLink data local buffering
   - includes data sending via HTTP Client to MS Flows
