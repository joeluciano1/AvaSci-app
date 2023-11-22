# CSV

The AvaSci app allows you to exprot the collected skeleton data and measurement values in CSV format.

## CSV structure

The structure of the CSV file is as follows:

- **Timestamp** - seconds since started.
- **Measurements** - a list measurement names.
- **Joints** - a list of human body joints.
   - **Confidence** - the confidence of the joint (0 - 1).
   - **2D Position** - the position of the joint in the 2D screen space (X, Y).
   - **3D Position** - the position of the joint in the 3D world space (X, Y, Z).

## CSV classes

To generate a CSV file from the data collected by the AvaSci app, LightBuzz provides two classes: `CSV` and `CSVManager`.

### `CSV.cs`

The `CSV` class creates a CSV-encoded string of a LightBuzz video session.

Use `CSV.Export()` to create the contents of the CSV file. You need to provide two parameters:

- An absolute path to a valid LightBuzz video folder (with `.color` and `body` files).
- A list of `MeasurementType` for the measurements you want to export.

### `CSVManager.cs`

The `CSVManager` class provides utility methods to create the CSV contents, save them to a local file, and share them in iOS.

- Use `CSVManager.Create()` to create the contents of the CSV file.
- Use `CSVManager.Save()` to save the contents to a local file.
- Use `CSVManager.Export()` to raise the Share Popover window and share the contents in iOS (e.g., saving the file in the Files app, or sending it as an email attachment).
- Use `CSVManager.CreateSaveExport()` to create the contents, save them to a local file, and raise the Share Popover window.
